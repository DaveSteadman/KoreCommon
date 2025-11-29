#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

using KoreCommon;

namespace KoreGIS;

// Object-oriented helpers for importing/exporting KoreGeoFeatureLibrary data via JSON based formats
public partial class KoreGeoJsonNativeIO
{
    private readonly KoreGeoFeatureLibrary GeoLibrary;

    // --------------------------------------------------------------------------------------------
    // MARK: Constructors
    // --------------------------------------------------------------------------------------------

    // Takes on the library parameter to work on during its operations
    // var LibIO = new KoreGeoJsonNativeIO(library);
    public KoreGeoJsonNativeIO(KoreGeoFeatureLibrary lib)
    {
        ArgumentNullException.ThrowIfNull(lib);
        GeoLibrary = lib;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: GeoJSON I/O
    // --------------------------------------------------------------------------------------------

    public void LoadFromJsonFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("GeoJSON file path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("GeoJSON file not found", filePath);

        var geoJsonText = File.ReadAllText(filePath);
        ImportJsonString(geoJsonText);
    }

    public void SaveToJsonFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("GeoJSON file path cannot be null or empty", nameof(filePath));

        var geoJsonText = ExportToJsonString();
        File.WriteAllText(filePath, geoJsonText);
    }

    public void ImportJsonString(string geoJsonString)
    {
        if (string.IsNullOrWhiteSpace(geoJsonString))
            throw new ArgumentException("GeoJSON string cannot be null or empty", nameof(geoJsonString));

        using var document = JsonDocument.Parse(geoJsonString);
        var root = document.RootElement;

        var type = GetStringCaseInsensitive(root, "type");
        if (string.Equals(type, "FeatureCollection", StringComparison.OrdinalIgnoreCase))
        {
            if (!root.TryGetProperty("features", out var featuresElement) || featuresElement.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("GeoJSON FeatureCollection is missing a features array");

            foreach (var featureElement in featuresElement.EnumerateArray())
            {
                TryImportFeature(featureElement);
            }
        }
        else if (string.Equals(type, "Feature", StringComparison.OrdinalIgnoreCase))
        {
            TryImportFeature(root);
        }
        else
        {
            throw new NotSupportedException($"GeoJSON type '{type}' is not supported. Only FeatureCollection and Feature documents are supported.");
        }
    }

    public string ExportToJsonString()
    {
        var allFeatures = new List<object>();

        foreach (var point in GeoLibrary.GetAllPoints())
        {
            var properties = BuildPointProperties(point);
            var geometry = new
            {
                type = "Point",
                coordinates = new[] { point.Position.LonDegs, point.Position.LatDegs }
            };

            allFeatures.Add(BuildFeatureObject(point, properties, geometry));
        }

        foreach (var multiPoint in GeoLibrary.GetAllMultiPoints())
        {
            var properties = BuildMultiPointProperties(multiPoint);
            var geometry = new
            {
                type = "MultiPoint",
                coordinates = multiPoint.Points.ConvertAll(p => new[] { p.LonDegs, p.LatDegs })
            };

            allFeatures.Add(BuildFeatureObject(multiPoint, properties, geometry));
        }

        foreach (var lineString in GeoLibrary.GetAllLineStrings())
        {
            var properties = BuildLineStringProperties(lineString);
            var geometry = new
            {
                type = "LineString",
                coordinates = lineString.Points.ConvertAll(p => new[] { p.LonDegs, p.LatDegs })
            };

            allFeatures.Add(BuildFeatureObject(lineString, properties, geometry));
        }

        foreach (var multiLine in GeoLibrary.GetAllMultiLineStrings())
        {
            var properties = BuildMultiLineStringProperties(multiLine);
            var geometry = new
            {
                type = "MultiLineString",
                coordinates = multiLine.LineStrings.ConvertAll(line => line.ConvertAll(p => new[] { p.LonDegs, p.LatDegs }))
            };

            allFeatures.Add(BuildFeatureObject(multiLine, properties, geometry));
        }

        foreach (var polygon in GeoLibrary.GetAllPolygons())
        {
            var properties = BuildPolygonProperties(polygon);

            var rings = new List<List<double[]>>
            {
                polygon.OuterRing.ConvertAll(p => new[] { p.LonDegs, p.LatDegs })
            };

            foreach (var innerRing in polygon.InnerRings)
            {
                rings.Add(innerRing.ConvertAll(p => new[] { p.LonDegs, p.LatDegs }));
            }

            var geometry = new
            {
                type = "Polygon",
                coordinates = rings
            };

            allFeatures.Add(BuildFeatureObject(polygon, properties, geometry));
        }

        foreach (var multiPolygon in GeoLibrary.GetAllMultiPolygons())
        {
            var properties = BuildMultiPolygonProperties(multiPolygon);

            var coordinates = new List<List<List<double[]>>>();
            foreach (var polygon in multiPolygon.Polygons)
            {
                var rings = new List<List<double[]>>
                {
                    polygon.OuterRing.ConvertAll(p => new[] { p.LonDegs, p.LatDegs })
                };

                foreach (var innerRing in polygon.InnerRings)
                {
                    rings.Add(innerRing.ConvertAll(p => new[] { p.LonDegs, p.LatDegs }));
                }

                coordinates.Add(rings);
            }

            var geometry = new
            {
                type = "MultiPolygon",
                coordinates
            };

            allFeatures.Add(BuildFeatureObject(multiPolygon, properties, geometry));
        }

        var bbox = GeoLibrary.LibraryBoundingBox();
        object featureCollection;

        if (bbox.HasValue)
        {
            featureCollection = new
            {
                type = "FeatureCollection",
                bbox = new[] { bbox.Value.MinLonDegs, bbox.Value.MinLatDegs, bbox.Value.MaxLonDegs, bbox.Value.MaxLatDegs },
                features = allFeatures
            };
        }
        else
        {
            featureCollection = new
            {
                type = "FeatureCollection",
                features = allFeatures
            };
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        return JsonSerializer.Serialize(featureCollection, options);
    }

    // ----------------------------------------------------------------------------------------
    // MARK: Generic Helpers
    // ----------------------------------------------------------------------------------------

    private void TryImportFeature(JsonElement featureElement)
    {
        if (featureElement.ValueKind != JsonValueKind.Object)
            return;

        var featureType = GetStringCaseInsensitive(featureElement, "type");
        if (!string.Equals(featureType, "Feature", StringComparison.OrdinalIgnoreCase))
            return;

        if (!featureElement.TryGetProperty("geometry", out var geometryElement) || geometryElement.ValueKind != JsonValueKind.Object)
            return;

        var geometryType = GetStringCaseInsensitive(geometryElement, "type");
        switch (geometryType)
        {
            case "Point":
                ImportPointFeature(featureElement, geometryElement);
                break;
            case "MultiPoint":
                ImportMultiPointFeature(featureElement, geometryElement);
                break;
            case "LineString":
                ImportLineStringFeature(featureElement, geometryElement);
                break;
            case "MultiLineString":
                ImportMultiLineStringFeature(featureElement, geometryElement);
                break;
            case "Polygon":
                ImportPolygonFeature(featureElement, geometryElement);
                break;
            case "MultiPolygon":
                ImportMultiPolygonFeature(featureElement, geometryElement);
                break;
            default:
                break;
        }
    }

    private static void PopulateFeatureProperties(KoreGeoFeature feature, JsonElement propertiesElement)
    {
        foreach (var property in propertiesElement.EnumerateObject())
        {
            object? value = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.TryGetInt64(out var longValue)
                    ? longValue
                    : property.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => property.Value.GetRawText()
            };

            if (value is not null)
            {
                feature.Properties[property.Name] = value;
            }
        }
    }

    private static void PopulateFeatureId(KoreGeoFeature feature, JsonElement featureElement)
    {
        if (featureElement.TryGetProperty("id", out var idElement))
        {
            feature.Id = idElement.ValueKind switch
            {
                JsonValueKind.String => idElement.GetString(),
                JsonValueKind.Number => idElement.GetInt64().ToString(),
                _ => null
            };
        }
    }

    private static bool TryConvertPropertyValue(object? value, out object? converted)
    {
        switch (value)
        {
            case null:
                converted = null;
                return true;
            case string s:
                converted = s;
                return true;
            case bool b:
                converted = b;
                return true;
            case int or long or short or byte or uint or ulong or ushort:
                converted = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                return true;
            case float or double or decimal:
                converted = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return true;
            default:
                converted = value.ToString();
                return !string.IsNullOrEmpty(converted as string);
        }
    }

    private static string? GetStringCaseInsensitive(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        if (element.TryGetProperty(propertyName, out var directProperty))
            return directProperty.ValueKind == JsonValueKind.String ? directProperty.GetString() : null;

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind == JsonValueKind.String)
            {
                return property.Value.GetString();
            }
        }

        return null;
    }

    private static object BuildFeatureObject(KoreGeoFeature feature, Dictionary<string, object?> properties, object geometry)
    {
        if (!string.IsNullOrWhiteSpace(feature.Id))
        {
            return new
            {
                type = "Feature",
                id = feature.Id,
                properties,
                geometry
            };
        }

        return new
        {
            type = "Feature",
            properties,
            geometry
        };
    }
}
// <fileheader>

