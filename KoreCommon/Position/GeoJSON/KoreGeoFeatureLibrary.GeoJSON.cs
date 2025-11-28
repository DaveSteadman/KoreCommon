// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

using KoreCommon;
namespace KoreGIS;

// Library for managing and querying geographic features
// Provides storage, retrieval, and spatial filtering of geo features

// GeoJSON Definition: See https://www.rfc-editor.org/rfc/rfc7946

public partial class KoreGeoFeatureLibrary
{
    // --------------------------------------------------------------------------------------------
    // MARK: GeoJSON I/O
    // --------------------------------------------------------------------------------------------

    // Load features from a GeoJSON file
    public void LoadFromGeoJSON(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("GeoJSON file path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("GeoJSON file not found", filePath);

        var geoJsonText = File.ReadAllText(filePath);
        ImportGeoJSON(geoJsonText);
    }

    // Save features to a GeoJSON file
    public void SaveToGeoJSON(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("GeoJSON file path cannot be null or empty", nameof(filePath));

        var geoJsonText = ExportToGeoJSON();
        File.WriteAllText(filePath, geoJsonText);
    }

    // Import from a GeoJSON string
    public void ImportGeoJSON(string geoJsonString)
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

            // Import bbox if present (RFC 7946 Section 5)
            if (root.TryGetProperty("bbox", out var bboxElement) && bboxElement.ValueKind == JsonValueKind.Array)
            {
                var bbox = ParseBoundingBox(bboxElement);
                if (bbox.HasValue)
                {
                    // Store in KoreGeoFeatureCollection properties if needed
                    // For now, we can calculate it on demand via CalculateBoundingBox()
                }
            }

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

    // Export to a GeoJSON string
    public string ExportToGeoJSON()
    {
        var allFeatures = new List<object>();

        // Export all points
        foreach (var point in GetAllPoints())
        {
            var properties = BuildPointProperties(point);
            var geometry = new
            {
                type = "Point",
                coordinates = new[] { point.Position.LonDegs, point.Position.LatDegs }
            };

            allFeatures.Add(BuildFeatureObject(point, properties, geometry));
        }

        // Export all multi points
        foreach (var multiPoint in GetAllMultiPoints())
        {
            var properties = BuildMultiPointProperties(multiPoint);
            var geometry = new
            {
                type = "MultiPoint",
                coordinates = multiPoint.Points.ConvertAll(p => new[] { p.LonDegs, p.LatDegs })
            };

            allFeatures.Add(BuildFeatureObject(multiPoint, properties, geometry));
        }

        // Export all line strings
        foreach (var lineString in GetAllLineStrings())
        {
            var properties = BuildLineStringProperties(lineString);
            var geometry = new
            {
                type = "LineString",
                coordinates = lineString.Points.ConvertAll(p => new[] { p.LonDegs, p.LatDegs })
            };

            allFeatures.Add(BuildFeatureObject(lineString, properties, geometry));
        }

        // Export all multi line strings
        foreach (var multiLine in GetAllMultiLineStrings())
        {
            var properties = BuildMultiLineStringProperties(multiLine);
            var geometry = new
            {
                type = "MultiLineString",
                coordinates = multiLine.LineStrings.ConvertAll(line => line.ConvertAll(p => new[] { p.LonDegs, p.LatDegs }))
            };

            allFeatures.Add(BuildFeatureObject(multiLine, properties, geometry));
        }

        // Export all polygons
        foreach (var polygon in GetAllPolygons())
        {
            var properties = BuildPolygonProperties(polygon);

            // Build rings array: [outerRing, hole1, hole2, ...]
            // NOTE: RFC 7946 specifies outer ring should be CCW, holes should be CW
            // We export as-is without enforcing winding direction
            var rings = new List<List<double[]>>();

            // Outer ring
            rings.Add(polygon.OuterRing.ConvertAll(p => new[] { p.LonDegs, p.LatDegs }));

            // Inner rings (holes)
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

        // Export all multi polygons
        foreach (var multiPolygon in GetAllMultiPolygons())
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

        // Calculate bounding box for the entire collection (RFC 7946 Section 5)
        var bbox = LibraryBoundingBox();
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
    // MARK: Generic Helpers (Current support: Points, Lines)
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
                // Other geometry types will be supported in future iterations
                break;
        }
    }


    private void PopulateFeatureProperties(KoreGeoFeature feature, JsonElement propertiesElement)
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

    // Populate the optional id field from a GeoJSON Feature (RFC 7946 Section 3.2)
    // The id can be a string or number
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

    private bool TryConvertPropertyValue(object? value, out object? converted)
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
            case int i:
            case long l:
            case short sh:
            case byte by:
            case uint ui:
            case ulong ul:
            case ushort us:
                converted = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                return true;
            case float f:
            case double d:
            case decimal m:
                converted = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return true;
            default:
                converted = value.ToString();
                return !string.IsNullOrEmpty(converted as string);
        }
    }

    private string GenerateUniqueName(string baseName)
    {
        var candidate = baseName.Trim();
        if (string.IsNullOrEmpty(candidate))
        {
            candidate = "Feature";
        }

        if (GetFeature(candidate) is null)
        {
            return candidate;
        }

        var suffix = 2;
        var uniqueCandidate = candidate;
        while (GetFeature(uniqueCandidate) is not null)
        {
            uniqueCandidate = $"{candidate}_{suffix}";
            suffix++;
        }

        return uniqueCandidate;
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

    // Parse a GeoJSON bbox array into a KoreLLBox (RFC 7946 Section 5)
    // Format: [minLon, minLat, maxLon, maxLat] for 2D
    private static KoreLLBox? ParseBoundingBox(JsonElement bboxElement)
    {
        if (bboxElement.ValueKind != JsonValueKind.Array)
            return null;

        var coords = new List<double>();
        foreach (var coord in bboxElement.EnumerateArray())
        {
            if (coord.ValueKind == JsonValueKind.Number)
            {
                coords.Add(coord.GetDouble());
            }
        }

        // GeoJSON bbox format: [minLon, minLat, maxLon, maxLat] (and optionally minAlt, maxAlt)
        if (coords.Count >= 4)
        {
            return new KoreLLBox
            {
                MinLonDegs = coords[0],
                MinLatDegs = coords[1],
                MaxLonDegs = coords[2],
                MaxLatDegs = coords[3]
            };
        }

        return null;
    }

    // Calculate bounding box from a list of points using KoreLLBox.FromList
    private static KoreLLBox? CalculateBoundingBoxFromPoints(List<KoreLLPoint> points)
    {
        if (points == null || points.Count == 0)
            return null;

        return KoreLLBox.FromList(points);
    }

    // Collect all points from a feature for bbox calculation
    private static List<KoreLLPoint> CollectAllPointsFromFeature(KoreGeoFeature feature)
    {
        var points = new List<KoreLLPoint>();

        switch (feature)
        {
            case KoreGeoPoint point:
                points.Add(point.Position);
                break;
            case KoreGeoMultiPoint multiPoint:
                points.AddRange(multiPoint.Points);
                break;
            case KoreGeoLineString lineString:
                points.AddRange(lineString.Points);
                break;
            case KoreGeoMultiLineString multiLine:
                foreach (var line in multiLine.LineStrings)
                {
                    points.AddRange(line);
                }
                break;
            case KoreGeoPolygon polygon:
                points.AddRange(polygon.OuterRing);
                foreach (var innerRing in polygon.InnerRings)
                {
                    points.AddRange(innerRing);
                }
                break;
            case KoreGeoMultiPolygon multiPolygon:
                foreach (var poly in multiPolygon.Polygons)
                {
                    points.AddRange(poly.OuterRing);
                    foreach (var innerRing in poly.InnerRings)
                    {
                        points.AddRange(innerRing);
                    }
                }
                break;
            case KoreGeoCircle circle:
                // For a circle, just use the center point
                // A more accurate bbox would need to calculate the circle's extent
                points.Add(circle.Center);
                break;
        }

        return points;
    }

    // Build a GeoJSON Feature object with optional id (RFC 7946 Section 3.2)
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
        else
        {
            return new
            {
                type = "Feature",
                properties,
                geometry
            };
        }
    }
}

