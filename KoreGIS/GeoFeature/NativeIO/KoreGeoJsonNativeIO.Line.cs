// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;

using KoreCommon;

namespace KoreGIS;

// GeoJSON LineString feature import/export for KoreGeoFeatureLibrary

public partial class KoreGeoJsonNativeIO
{
    // ----------------------------------------------------------------------------------------
    // MARK: LineString Feature Import/Export
    // ----------------------------------------------------------------------------------------

    private void ImportLineStringFeature(JsonElement featureElement, JsonElement geometryElement)
    {
        if (!geometryElement.TryGetProperty("coordinates", out var coordinatesElement) || coordinatesElement.ValueKind != JsonValueKind.Array)
            return;

        var lineString = new KoreGeoLineString();

        // Parse coordinate array [[lon, lat], [lon, lat], ...]
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

            lineString.Points.Add(new KoreLLPoint
            {
                LonDegs = lon,
                LatDegs = lat
            });
        }

        // Need at least 2 points for a line
        if (lineString.Points.Count < 2)
            return;

        // Load optional id field (RFC 7946 Section 3.2)
        PopulateFeatureId(lineString, featureElement);

        // Load properties (name, lineWidth, color, etc.)
        if (featureElement.TryGetProperty("properties", out var propertiesElement) && propertiesElement.ValueKind == JsonValueKind.Object)
        {
            PopulateFeatureProperties(lineString, propertiesElement);
        }

        var rawName = lineString.Properties.TryGetValue("name", out var storedNameObj) ? storedNameObj?.ToString() : null;
        lineString.Name = GeoLibrary.GenerateUniqueFeatureName(string.IsNullOrWhiteSpace(rawName) ? "LineString" : rawName!);

        // Ensure the feature dictionary reflects the final name
        lineString.Properties["name"] = lineString.Name;

        GeoLibrary.AddFeature(lineString);
    }

    // ----------------------------------------------------------------------------------------

    private static Dictionary<string, object?> BuildLineStringProperties(KoreGeoLineString lineString)
    {
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = lineString.Name
        };

        // Add line-specific properties if they differ from defaults
        if (lineString.LineWidth != 1.0)
        {
            properties["lineWidth"] = lineString.LineWidth;
        }

        if (lineString.IsGreatCircle)
        {
            properties["isGreatCircle"] = lineString.IsGreatCircle;
        }

        // Include other custom properties
        foreach (var kvp in lineString.Properties)
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

