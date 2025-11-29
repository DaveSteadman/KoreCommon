// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;

using KoreCommon;

namespace KoreGIS;

// GeoJSON Polygon feature import/export for KoreGeoFeatureLibrary
public partial class KoreGeoJsonNativeIO
{
    // ----------------------------------------------------------------------------------------
    // MARK: Polygon Feature Import/Export
    // ----------------------------------------------------------------------------------------

    private void ImportPolygonFeature(JsonElement featureElement, JsonElement geometryElement)
    {
        if (!geometryElement.TryGetProperty("coordinates", out var coordinatesElement) || coordinatesElement.ValueKind != JsonValueKind.Array)
            return;

        var polygon = new KoreGeoPolygon();

        var ringIndex = 0;
        // Parse rings: first is outer ring, rest are holes
        // NOTE: RFC 7946 specifies outer ring should be CCW, holes should be CW
        // but we accept any winding direction during import
        foreach (var ringElement in coordinatesElement.EnumerateArray())
        {
            if (ringElement.ValueKind != JsonValueKind.Array)
                continue;

            var ring = new List<KoreLLPoint>();

            // Parse coordinate array [[lon, lat], [lon, lat], ...]
            foreach (var coordElement in ringElement.EnumerateArray())
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

                ring.Add(new KoreLLPoint
                {
                    LonDegs = lon,
                    LatDegs = lat
                });
            }

            // Need at least 3 points for a valid ring (though GeoJSON spec says 4 with closure)
            if (ring.Count < 3)
                continue;

            if (ringIndex == 0)
            {
                // First ring is the outer boundary
                polygon.OuterRing = ring;
            }
            else
            {
                // Subsequent rings are holes
                polygon.InnerRings.Add(ring);
            }

            ringIndex++;
        }

        // Must have at least an outer ring
        if (polygon.OuterRing.Count == 0)
            return;

        // Load optional id field (RFC 7946 Section 3.2)
        PopulateFeatureId(polygon, featureElement);

        // Load properties (name, fillColor, strokeColor, etc.)
        if (featureElement.TryGetProperty("properties", out var propertiesElement) && propertiesElement.ValueKind == JsonValueKind.Object)
        {
            PopulateFeatureProperties(polygon, propertiesElement);

            // Parse fill color if present
            if (propertiesElement.TryGetProperty("fillColor", out var fillColorElement) && fillColorElement.ValueKind == JsonValueKind.String)
            {
                var fillColorStr = fillColorElement.GetString();
                if (!string.IsNullOrWhiteSpace(fillColorStr))
                {
                    polygon.FillColor = KoreColorIO.HexStringToRGB(fillColorStr);
                }
            }

            // Parse stroke color if present
            if (propertiesElement.TryGetProperty("strokeColor", out var strokeColorElement) && strokeColorElement.ValueKind == JsonValueKind.String)
            {
                var strokeColorStr = strokeColorElement.GetString();
                if (!string.IsNullOrWhiteSpace(strokeColorStr))
                {
                    polygon.StrokeColor = KoreColorIO.HexStringToRGB(strokeColorStr);
                }
            }

            // Parse stroke width if present
            if (propertiesElement.TryGetProperty("strokeWidth", out var strokeWidthElement))
            {
                if (strokeWidthElement.ValueKind == JsonValueKind.Number)
                {
                    polygon.StrokeWidth = strokeWidthElement.GetDouble();
                }
            }
        }

        var rawName = polygon.Properties.TryGetValue("name", out var storedNameObj) ? storedNameObj?.ToString() : null;
        polygon.Name = GeoLibrary.GenerateUniqueFeatureName(string.IsNullOrWhiteSpace(rawName) ? "Polygon" : rawName!);

        // Ensure the feature dictionary reflects the final name
        polygon.Properties["name"] = polygon.Name;

        GeoLibrary.AddFeature(polygon);
    }

    // ----------------------------------------------------------------------------------------

    private static Dictionary<string, object?> BuildPolygonProperties(KoreGeoPolygon polygon)
    {
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = polygon.Name
        };

        // Add polygon-specific properties if they differ from defaults
        if (polygon.StrokeWidth != 1.0)
        {
            properties["strokeWidth"] = polygon.StrokeWidth;
        }

        // Serialize fill color if present
        if (polygon.FillColor.HasValue)
        {
            properties["fillColor"] = KoreColorIO.RBGtoHexString(polygon.FillColor.Value);
        }

        // Serialize stroke color if present
        if (polygon.StrokeColor.HasValue)
        {
            properties["strokeColor"] = KoreColorIO.RBGtoHexString(polygon.StrokeColor.Value);
        }

        // Include other custom properties
        foreach (var kvp in polygon.Properties)
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

