// <fileheader>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using KoreCommon;
using KoreCommon.UnitTest;
using KoreGIS;
namespace KoreGIS.UnitTest;

// Unit tests for KoreGeoFeatureLibrary - Polygon/GeoJSON functionality
public static partial class KoreTestGeoFeatureLibrary
{
    public static void TestSavePolygonToGeoJSON(KoreTestLog testLog)
    {
        string testName = "GeoFeatureLibrary SavePolygonToGeoJSON";
        try
        {
            var library = new KoreGeoFeatureLibrary
            {
                Name = "Polygon Test Library"
            };

            // Create a polygon representing Greater London boundary (irregular shape)
            // This tests complex polygon rendering with multiple vertices and semi-transparent fill
            var londonPolygon = new KoreGeoPolygon
            {
                Name = "GreaterLondon",
                FillColor = new KoreColorRGB(255, 100, 150, 120), // Semi-transparent pink/magenta
                StrokeColor = new KoreColorRGB(180, 0, 100), // Dark magenta stroke
                StrokeWidth = 3.0
            };

            // Outer ring - irregular polygon roughly following Greater London boundary
            // Starting from southwest, going clockwise with more realistic outline
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.28, LonDegs = -0.50 }); // Southwest (Heathrow area)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.32, LonDegs = -0.62 }); // West bulge (Uxbridge)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.42, LonDegs = -0.68 }); // Northwest bulge (Harrow)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.52, LonDegs = -0.55 }); // North (Barnet)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.62, LonDegs = -0.35 }); // North bulge (Enfield)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.65, LonDegs = -0.10 }); // Northeast (Waltham Forest)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.62, LonDegs = 0.10 }); // East bulge (Havering)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.52, LonDegs = 0.25 }); // East (Upminster)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.42, LonDegs = 0.32 }); // Southeast bulge (Bexley)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.35, LonDegs = 0.28 }); // South (Bromley)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.28, LonDegs = 0.18 }); // Southeast (Croydon)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.25, LonDegs = -0.05 }); // South center (Sutton)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.26, LonDegs = -0.25 }); // Southwest (Kingston)
            londonPolygon.OuterRing.Add(new KoreLLPoint { LatDegs = 51.28, LonDegs = -0.50 }); // Close the ring

            // Add an irregular hole representing central exclusion zone (e.g., a park or restricted area)
            var centralParkHole = new List<KoreLLPoint>
            {
                new KoreLLPoint { LatDegs = 51.48, LonDegs = -0.18 }, // Hyde Park area - irregular pentagon
                new KoreLLPoint { LatDegs = 51.46, LonDegs = -0.10 },
                new KoreLLPoint { LatDegs = 51.50, LonDegs = -0.05 },
                new KoreLLPoint { LatDegs = 51.54, LonDegs = -0.08 },
                new KoreLLPoint { LatDegs = 51.53, LonDegs = -0.16 },
                new KoreLLPoint { LatDegs = 51.48, LonDegs = -0.18 } // Close the hole
            };
            londonPolygon.InnerRings.Add(centralParkHole);

            // Add a second smaller hole to test multiple holes support
            var secondHole = new List<KoreLLPoint>
            {
                new KoreLLPoint { LatDegs = 51.35, LonDegs = -0.15 }, // South London exclusion
                new KoreLLPoint { LatDegs = 51.34, LonDegs = -0.08 },
                new KoreLLPoint { LatDegs = 51.37, LonDegs = -0.06 },
                new KoreLLPoint { LatDegs = 51.38, LonDegs = -0.12 },
                new KoreLLPoint { LatDegs = 51.35, LonDegs = -0.15 } // Close the hole
            };
            londonPolygon.InnerRings.Add(secondHole);

            londonPolygon.Properties["area_type"] = "administrative";
            londonPolygon.Properties["population"] = 9000000;

            library.AddFeature(londonPolygon);

            string artefactsDir = KoreTestCenter.TestPath;
            Directory.CreateDirectory(artefactsDir);

            string geoJsonPath = KoreFileOps.JoinPaths(artefactsDir, "geo_feature_polygon.json");
            var LibIO = new KoreGeoJsonNativeIO(library);
            LibIO.SaveToJsonFile(geoJsonPath);

            if (!File.Exists(geoJsonPath))
            {
                testLog.AddResult(testName, false, "GeoJSON file was not created");
                return;
            }

            // Now test loading it back
            var loadedLibrary = new KoreGeoFeatureLibrary();
            var LibIO2 = new KoreGeoJsonNativeIO(loadedLibrary);
            LibIO2.LoadFromJsonFile(geoJsonPath);

            var loadedPolygon = loadedLibrary.GetPolygon("GreaterLondon");
            if (loadedPolygon == null)
            {
                testLog.AddResult(testName, false, "Failed to load polygon feature from GeoJSON");
                return;
            }

            if (loadedPolygon.OuterRing.Count != 14)
            {
                testLog.AddResult(testName, false, $"Expected 14 outer ring points but got {loadedPolygon.OuterRing.Count}");
                return;
            }

            if (loadedPolygon.InnerRings.Count != 2)
            {
                testLog.AddResult(testName, false, $"Expected 2 inner rings but got {loadedPolygon.InnerRings.Count}");
                return;
            }

            if (loadedPolygon.InnerRings[0].Count != 6)
            {
                testLog.AddResult(testName, false, $"Expected 6 points in first inner ring but got {loadedPolygon.InnerRings[0].Count}");
                return;
            }

            if (loadedPolygon.InnerRings[1].Count != 5)
            {
                testLog.AddResult(testName, false, $"Expected 5 points in second inner ring but got {loadedPolygon.InnerRings[1].Count}");
                return;
            }

            // Verify first outer ring point (Southwest - Heathrow area)
            const double coordTolerance = 1e-6;
            bool firstPointMatch =
                KoreValueUtils.EqualsWithinTolerance(loadedPolygon.OuterRing[0].LatDegs, 51.28, coordTolerance) &&
                KoreValueUtils.EqualsWithinTolerance(loadedPolygon.OuterRing[0].LonDegs, -0.50, coordTolerance);

            if (!firstPointMatch)
            {
                testLog.AddResult(testName, false, "First outer ring point coordinate mismatch after round-trip");
                return;
            }

            // Verify first inner ring point (Hyde Park area)
            bool firstHolePointMatch =
                KoreValueUtils.EqualsWithinTolerance(loadedPolygon.InnerRings[0][0].LatDegs, 51.48, coordTolerance) &&
                KoreValueUtils.EqualsWithinTolerance(loadedPolygon.InnerRings[0][0].LonDegs, -0.18, coordTolerance);

            if (!firstHolePointMatch)
            {
                testLog.AddResult(testName, false, "First inner ring point coordinate mismatch after round-trip");
                return;
            }

            // Verify second inner ring point (South London)
            bool secondHolePointMatch =
                KoreValueUtils.EqualsWithinTolerance(loadedPolygon.InnerRings[1][0].LatDegs, 51.35, coordTolerance) &&
                KoreValueUtils.EqualsWithinTolerance(loadedPolygon.InnerRings[1][0].LonDegs, -0.15, coordTolerance);

            if (!secondHolePointMatch)
            {
                testLog.AddResult(testName, false, "Second inner ring point coordinate mismatch after round-trip");
                return;
            }

            // Verify colors survived round-trip
            if (loadedPolygon.FillColor == null)
            {
                testLog.AddResult(testName, false, "Fill color was not preserved");
                return;
            }

            bool fillColorMatch =
                loadedPolygon.FillColor.Value.R == 255 &&
                loadedPolygon.FillColor.Value.G == 100 &&
                loadedPolygon.FillColor.Value.B == 150 &&
                loadedPolygon.FillColor.Value.A == 120;

            if (!fillColorMatch)
            {
                testLog.AddResult(testName, false, $"Fill color mismatch: expected RGBA(255,100,150,120), got RGBA({loadedPolygon.FillColor.Value.R},{loadedPolygon.FillColor.Value.G},{loadedPolygon.FillColor.Value.B},{loadedPolygon.FillColor.Value.A})");
                return;
            }

            if (loadedPolygon.StrokeColor == null)
            {
                testLog.AddResult(testName, false, "Stroke color was not preserved");
                return;
            }

            // Verify properties survived round-trip
            if (!loadedPolygon.Properties.ContainsKey("area_type"))
            {
                testLog.AddResult(testName, false, "Property 'area_type' was not preserved");
                return;
            }

            testLog.AddComment($"GeoJSON Polygon with {loadedPolygon.OuterRing.Count} outer points and {loadedPolygon.InnerRings.Count} holes saved to {geoJsonPath} and loaded successfully");
            testLog.AddResult(testName, true);
        }
        catch (Exception ex)
        {
            testLog.AddResult(testName, false, $"Exception: {ex.Message}");
        }
    }
}
