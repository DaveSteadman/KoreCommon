// <fileheader>

using System;
using System.IO;
using System.Text.Json;

using KoreCommon;
using KoreCommon.UnitTest;
using KoreGIS;
namespace KoreGIS.UnitTest;

// Unit tests for KoreGeoFeatureLibrary - Point/GeoJSON functionality
public static partial class KoreTestGeoFeatureLibrary
{
    public static void TestSaveSinglePointToGeoJSON(KoreTestLog testLog)
    {
        const string testName = "GeoFeatureLibrary SaveToGeoJSON";

        try
        {
            var library = new KoreGeoFeatureLibrary
            {
                Name = "Unit Test Library"
            };

            var point = new KoreGeoPoint
            {
                Name = "FarnboroughAirport",
                Label = "Farnborough Airport",
                Position = new KoreLLPoint { LatDegs = 51.2758, LonDegs = -0.7763 }, // Farnborough Airport
                Color = new KoreColorRGB(255, 0, 0),
                Size = 6
            };
            point.Properties["category"] = "airport";
            point.Properties["iata"] = "FAB";

            library.AddFeature(point);

            string artefactsDir = KoreTestCenter.TestPath;
            Directory.CreateDirectory(artefactsDir);

            string geoJsonPath = KoreFileOps.JoinPaths(artefactsDir, "geo_feature_point.json");
            var LibIO = new KoreGeoJsonNativeIO(library);
            LibIO.SaveToJsonFile(geoJsonPath);

            if (!File.Exists(geoJsonPath))
            {
                testLog.AddResult(testName, false, $"File not created: {geoJsonPath}");
                return;
            }

            string json = File.ReadAllText(geoJsonPath);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            bool isFeatureCollection = root.TryGetProperty("type", out var typeElement)
                && string.Equals(typeElement.GetString(), "FeatureCollection", StringComparison.OrdinalIgnoreCase);

            if (!isFeatureCollection)
            {
                testLog.AddResult(testName, false, "GeoJSON root type is not FeatureCollection");
                return;
            }

            if (!root.TryGetProperty("features", out var featuresElement) || featuresElement.ValueKind != JsonValueKind.Array)
            {
                testLog.AddResult(testName, false, "GeoJSON features array missing");
                return;
            }

            if (featuresElement.GetArrayLength() != 1)
            {
                testLog.AddResult(testName, false, "GeoJSON did not contain exactly one feature");
                return;
            }

            JsonElement feature = featuresElement[0];
            if (!feature.TryGetProperty("geometry", out var geometryElement))
            {
                testLog.AddResult(testName, false, "GeoJSON feature missing geometry");
                return;
            }

            if (!string.Equals(geometryElement.GetProperty("type").GetString(), "Point", StringComparison.OrdinalIgnoreCase))
            {
                testLog.AddResult(testName, false, "GeoJSON geometry type was not Point");
                return;
            }

            JsonElement coordinates = geometryElement.GetProperty("coordinates");
            double lon = coordinates[0].GetDouble();
            double lat = coordinates[1].GetDouble();

            const double coordTolerance = 1e-6;

            bool coordsMatch =
                KoreValueUtils.EqualsWithinTolerance(lat, point.Position.LatDegs, coordTolerance) &&
                KoreValueUtils.EqualsWithinTolerance(lon, point.Position.LonDegs, coordTolerance);

            if (!coordsMatch)
            {
                testLog.AddResult(testName, false, $"Coordinate mismatch. Expected ({point.Position.LatDegs}, {point.Position.LonDegs}) but got ({lat}, {lon})");
                return;
            }

            testLog.AddComment($"GeoJSON single point saved to {geoJsonPath}");
            testLog.AddResult(testName, true);
        }
        catch (Exception ex)
        {
            testLog.AddResult(testName, false, $"Exception: {ex.Message}");
        }
    }
}
