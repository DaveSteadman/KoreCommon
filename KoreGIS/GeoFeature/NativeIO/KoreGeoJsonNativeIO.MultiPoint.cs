// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;

using KoreCommon;

namespace KoreGIS;

// GeoJSON MultiPoint feature import/export for KoreGeoFeatureLibrary
public partial class KoreGeoJsonNativeIO
{
    // ----------------------------------------------------------------------------------------
    // MARK: MultiPoint Feature Import/Export
    // ----------------------------------------------------------------------------------------

    private void ImportMultiPointFeature(JsonElement featureElement, JsonElement geometryElement)
    {
        if (!geometryElement.TryGetProperty("coordinates", out var coordinatesElement) || coordinatesElement.ValueKind != JsonValueKind.Array)
            return;

        var multiPoint = new KoreGeoMultiPoint();

        foreach (var coordElement in coordinatesElement.EnumerateArray())
        {
            if (coordElement.ValueKind != JsonValueKind.Array)
                continue;

            var coordEnumerator = coordElement.EnumerateArray();
            if (!coordEnumerator.MoveNext())
                continue;
            var lon = coordEnumerator.Current.GetDouble();

            if (!coordEnumerator.MoveNext())
                continue;
            var lat = coordEnumerator.Current.GetDouble();

            multiPoint.Points.Add(new KoreLLPoint
            {
                LonDegs = lon,
                LatDegs = lat
            });
        }

        if (multiPoint.Points.Count == 0)
            return;

        // Load optional id field (RFC 7946 Section 3.2)
        PopulateFeatureId(multiPoint, featureElement);

        if (featureElement.TryGetProperty("properties", out var propertiesElement) && propertiesElement.ValueKind == JsonValueKind.Object)
        {
            PopulateFeatureProperties(multiPoint, propertiesElement);
        }

        var rawName = multiPoint.Properties.TryGetValue("name", out var storedNameObj) ? storedNameObj?.ToString() : null;
        multiPoint.Name = GeoLibrary.GenerateUniqueFeatureName(string.IsNullOrWhiteSpace(rawName) ? "MultiPoint" : rawName!);
        multiPoint.Properties["name"] = multiPoint.Name;

        GeoLibrary.AddFeature(multiPoint);
    }

    // ----------------------------------------------------------------------------------------

    private static Dictionary<string, object?> BuildMultiPointProperties(KoreGeoMultiPoint multiPoint)
    {
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = multiPoint.Name
        };

        foreach (var kvp in multiPoint.Properties)
        {
            if (string.Equals(kvp.Key, "name", StringComparison.OrdinalIgnoreCase))
                continue;

            if (TryConvertPropertyValue(kvp.Value, out var convertedValue))
            {
                properties[kvp.Key] = convertedValue;
            }
        }

        return properties;
    }
}

