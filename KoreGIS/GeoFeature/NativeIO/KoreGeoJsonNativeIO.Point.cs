// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;

using KoreCommon;

namespace KoreGIS;

// GeoJSON Point feature import/export for KoreGeoFeatureLibrary
public partial class KoreGeoJsonNativeIO
{
    // ----------------------------------------------------------------------------------------
    // MARK: Point Feature Import/Export
    // ----------------------------------------------------------------------------------------

    private void ImportPointFeature(JsonElement featureElement, JsonElement geometryElement)
    {
        if (!geometryElement.TryGetProperty("coordinates", out var coordinatesElement) || coordinatesElement.ValueKind != JsonValueKind.Array)
            return;

        var coordinateEnumerator = coordinatesElement.EnumerateArray();
        if (!coordinateEnumerator.MoveNext())
            return;
        var lon = coordinateEnumerator.Current.GetDouble();

        if (!coordinateEnumerator.MoveNext())
            return;
        var lat = coordinateEnumerator.Current.GetDouble();

        var point = new KoreGeoPoint
        {
            Position = new KoreLLPoint
            {
                LonDegs = lon,
                LatDegs = lat
            }
        };

        // Load optional id field (RFC 7946 Section 3.2)
        PopulateFeatureId(point, featureElement);

        // Load properties (name, label, etc.)
        if (featureElement.TryGetProperty("properties", out var propertiesElement) && propertiesElement.ValueKind == JsonValueKind.Object)
        {
            PopulateFeatureProperties(point, propertiesElement);

            if (propertiesElement.TryGetProperty("label", out var labelElement) && labelElement.ValueKind == JsonValueKind.String)
            {
                point.Label = labelElement.GetString();
            }
        }

        var rawName = point.Properties.TryGetValue("name", out var storedNameObj) ? storedNameObj?.ToString() : null;
        if (string.IsNullOrWhiteSpace(rawName))
        {
            rawName = point.Label;
        }

        point.Name = GeoLibrary.GenerateUniqueFeatureName(string.IsNullOrWhiteSpace(rawName) ? "Point" : rawName!);

        // Ensure the feature dictionary reflects the final name
        point.Properties["name"] = point.Name;

        GeoLibrary.AddFeature(point);
    }

    // ----------------------------------------------------------------------------------------

    private static Dictionary<string, object?> BuildPointProperties(KoreGeoPoint point)
    {
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = point.Name
        };

        if (!string.IsNullOrWhiteSpace(point.Label))
        {
            properties["label"] = point.Label;
        }

        foreach (var kvp in point.Properties)
        {
            if (string.Equals(kvp.Key, "name", StringComparison.OrdinalIgnoreCase) || string.Equals(kvp.Key, "label", StringComparison.OrdinalIgnoreCase))
                continue;

            if (TryConvertPropertyValue(kvp.Value, out var convertedValue))
            {
                properties[kvp.Key] = convertedValue;
            }
        }

        return properties;
    }
}

