// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;

using KoreCommon;

namespace KoreGIS;

// GeoJSON MultiLineString feature import/export for KoreGeoFeatureLibrary
public partial class KoreGeoJsonNativeIO
{
    // ----------------------------------------------------------------------------------------
    // MARK: MultiLineString Feature Import/Export
    // ----------------------------------------------------------------------------------------

    private void ImportMultiLineStringFeature(JsonElement featureElement, JsonElement geometryElement)
    {

        if (!geometryElement.TryGetProperty("coordinates", out var coordinatesElement) || coordinatesElement.ValueKind != JsonValueKind.Array)
            return;

        var multiLine = new KoreGeoMultiLineString();

        foreach (var lineElement in coordinatesElement.EnumerateArray())
        {
            if (lineElement.ValueKind != JsonValueKind.Array)
                continue;

            var line = new List<KoreLLPoint>();

            foreach (var coordElement in lineElement.EnumerateArray())
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

                line.Add(new KoreLLPoint
                {
                    LonDegs = lon,
                    LatDegs = lat
                });
            }

            if (line.Count >= 2)
            {
                multiLine.LineStrings.Add(line);
            }
        }

        if (multiLine.LineStrings.Count == 0)
            return;

        // Load optional id field (RFC 7946 Section 3.2)
        PopulateFeatureId(multiLine, featureElement);

        if (featureElement.TryGetProperty("properties", out var propertiesElement) && propertiesElement.ValueKind == JsonValueKind.Object)
        {
            PopulateFeatureProperties(multiLine, propertiesElement);
        }

        var rawName = multiLine.Properties.TryGetValue("name", out var storedNameObj) ? storedNameObj?.ToString() : null;
        multiLine.Name = GeoLibrary.GenerateUniqueFeatureName(string.IsNullOrWhiteSpace(rawName) ? "MultiLineString" : rawName!);
        multiLine.Properties["name"] = multiLine.Name;

        GeoLibrary.AddFeature(multiLine);
    }

    // ----------------------------------------------------------------------------------------

    private static Dictionary<string, object?> BuildMultiLineStringProperties(KoreGeoMultiLineString multiLine)
    {
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = multiLine.Name
        };

        if (multiLine.LineWidth != 1.0)
        {
            properties["lineWidth"] = multiLine.LineWidth;
        }

        if (multiLine.IsGreatCircle)
        {
            properties["isGreatCircle"] = multiLine.IsGreatCircle;
        }

        foreach (var kvp in multiLine.Properties)
        {
            if (string.Equals(kvp.Key, "name", StringComparison.OrdinalIgnoreCase) || string.Equals(kvp.Key, "lineWidth", StringComparison.OrdinalIgnoreCase) || string.Equals(kvp.Key, "isGreatCircle", StringComparison.OrdinalIgnoreCase))
                continue;

            if (TryConvertPropertyValue(kvp.Value, out var convertedValue))
            {
                properties[kvp.Key] = convertedValue;
            }
        }

        return properties;
    }
}

