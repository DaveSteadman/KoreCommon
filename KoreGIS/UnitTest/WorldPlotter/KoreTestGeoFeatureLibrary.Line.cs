// <fileheader>

using System;
using System.IO;
using System.Text.Json;

using KoreCommon;
using KoreCommon.UnitTest;
using KoreGIS;
namespace KoreGIS.UnitTest;

// Unit tests for KoreGeoFeatureLibrary - LineString/GeoJSON functionality
public static partial class KoreTestGeoFeatureLibrary
{
    public static void TestSaveLineStringToGeoJSON(KoreTestLog testLog)
    {
        const string testName = "GeoFeatureLibrary LineString Save/Load";

        try
        {
            var library = new KoreGeoFeatureLibrary
            {
                Name = "Line Test Library"
            };

            // Create a simple route: London -> Farnborough -> Southampton
            var line = new KoreGeoLineString
            {
                Name = "TestRoute",
                LineWidth = 4.0, // Wider stroke for better visibility
                Color = new KoreColorRGB(0, 128, 255)
            };

            line.Points.Add(new KoreLLPoint { LatDegs = 51.5074, LonDegs = -0.1278 }); // London
            line.Points.Add(new KoreLLPoint { LatDegs = 51.2758, LonDegs = -0.7763 }); // Farnborough
            line.Points.Add(new KoreLLPoint { LatDegs = 50.9097, LonDegs = -1.4044 }); // Southampton

            line.Properties["category"] = "route";
            line.Properties["description"] = "Test flight path";

            library.AddFeature(line);

            string artefactsDir = KoreTestCenter.TestPath;
            Directory.CreateDirectory(artefactsDir);

            string geoJsonPath = KoreFileOps.JoinPaths(artefactsDir, "geo_feature_line.json");
            var LibIO = new KoreGeoJsonNativeIO(library);
            LibIO.SaveToJsonFile(geoJsonPath);


            if (!File.Exists(geoJsonPath))
            {
                testLog.AddResult(testName, false, $"File not created: {geoJsonPath}");
                return;
            }

            // Verify the GeoJSON structure
            string json = File.ReadAllText(geoJsonPath);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

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

            if (!string.Equals(geometryElement.GetProperty("type").GetString(), "LineString", StringComparison.OrdinalIgnoreCase))
            {
                testLog.AddResult(testName, false, "GeoJSON geometry type was not LineString");
                return;
            }

            JsonElement coordinates = geometryElement.GetProperty("coordinates");
            if (coordinates.GetArrayLength() != 3)
            {
                testLog.AddResult(testName, false, $"Expected 3 coordinate pairs but got {coordinates.GetArrayLength()}");
                return;
            }

            // Now test loading it back
            var loadedLibrary = new KoreGeoFeatureLibrary();
            var LibIO2 = new KoreGeoJsonNativeIO(loadedLibrary);
            LibIO2.LoadFromJsonFile(geoJsonPath);

            var loadedLine = loadedLibrary.GetLineString("TestRoute");
            if (loadedLine == null)
            {
                testLog.AddResult(testName, false, "Failed to load line feature from GeoJSON");
                return;
            }

            if (loadedLine.Points.Count != 3)
            {
                testLog.AddResult(testName, false, $"Expected 3 points but got {loadedLine.Points.Count}");
                return;
            }

            // Verify first point (London)
            const double coordTolerance = 1e-6;
            bool firstPointMatch =
                KoreValueUtils.EqualsWithinTolerance(loadedLine.Points[0].LatDegs, 51.5074, coordTolerance) &&
                KoreValueUtils.EqualsWithinTolerance(loadedLine.Points[0].LonDegs, -0.1278, coordTolerance);

            if (!firstPointMatch)
            {
                testLog.AddResult(testName, false, "First point coordinate mismatch after round-trip");
                return;
            }

            testLog.AddComment($"GeoJSON LineString saved to {geoJsonPath} and loaded successfully");
            testLog.AddResult(testName, true);
        }
        catch (Exception ex)
        {
            testLog.AddResult(testName, false, $"Exception: {ex.Message}");
        }
    }
}
