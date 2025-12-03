// <fileheader>

using System;
using System.Linq;

using KoreCommon;
using KoreCommon.SkiaSharp;
using KoreCommon.UnitTest;
using KoreGIS;

namespace KoreGIS.UnitTest;

public static class KoreTestWorldPlotter
{
    public static void RunTests(KoreTestLog testLog)
    {
        // Putting the library first, as the plotter could have dependencies on it
        KoreTestGeoFeatureLibrary.RunTests(testLog);

        TestBasicWorldMap(testLog);
        TestCoordinateMapping(testLog);
        TestEuropeMap(testLog);
        TestUKMap(testLog);
        TestUKCountryOutline(testLog);
        TestAllCountriesMap(testLog);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: World
    // --------------------------------------------------------------------------------------------

    private static void TestBasicWorldMap(KoreTestLog testLog)
    {
        // Create a world plotter (2:1 aspect ratio - equirectangular projection)
        var worldPlotter = new KoreWorldPlotter(3600, 1800);

        // Draw the grid overlay
        KoreWorldPlotterOps.DrawGrid(worldPlotter, 15, 15, new KoreColorRGB(200, 200, 200), 1.0);

        // Draw border
        KoreWorldPlotterOps.DrawBorder(worldPlotter);

        // Draw all country outlines if available
        string countriesPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "CountryOutline_All.geojson");
        if (System.IO.File.Exists(countriesPath))
        {
            try
            {
                var countriesLibrary = new KoreGeoFeatureLibrary();
                var LibIO = new KoreGeoJsonNativeIO(countriesLibrary);
                LibIO.LoadFromJsonFile(countriesPath);

                int polygonCount = 0;

                // Define base color for all countries
                KoreColorRGB baseColor = new KoreColorRGB(210, 210, 210, 150);

                foreach (var geoPolygon in countriesLibrary.GetAllPolygons())
                {
                    geoPolygon.StrokeColor = new KoreColorRGB(120, 120, 120);  // Medium gray outline
                    geoPolygon.StrokeWidth = 2;

                    // Apply random color variation to single polygons too
                    KoreColorRGB noiseColor = KoreColorOps.ColorWithRGBNoise(baseColor, 0.2f);
                    geoPolygon.FillColor = noiseColor;

                    worldPlotter.DrawGeoPolygon(geoPolygon);
                    polygonCount++;
                }

                foreach (var multiPolygon in countriesLibrary.GetAllMultiPolygons())
                {
                    multiPolygon.StrokeColor = new KoreColorRGB(120, 120, 160);  // Medium gray outline
                    multiPolygon.StrokeWidth = 2;

                    // Apply random color variation to multi-polygons
                    KoreColorRGB noiseColor = KoreColorOps.ColorWithRGBNoise(baseColor, 0.1f);
                    multiPolygon.FillColor = noiseColor;

                    worldPlotter.DrawGeoMultiPolygon(multiPolygon);
                    polygonCount += multiPolygon.Polygons.Count;
                }

                testLog.AddComment($"Loaded and drew {polygonCount} country polygons");
            }
            catch (Exception ex)
            {
                testLog.AddComment($"Failed to load countries: {ex.Message}");
            }
        }

        // Draw a great circle route (e.g., London to Sydney)
        var londonPos = KorePositionLibrary.GetLLPos("London");
        var sydneyPos = KorePositionLibrary.GetLLPos("Sydney");

        // Generate 100 points along the great circle
        var greatCirclePoints = KoreLLPointOps.GreatCirclePointList(londonPos, sydneyPos, 100);

        // Draw the great circle in segments, filtering out longitude wraps
        worldPlotter.Plotter.DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(new KoreColorRGB(255, 50, 50)); // Bright red
        worldPlotter.Plotter.DrawSettings.Paint.StrokeWidth = 3.0f;


        for (int i = 0; i < greatCirclePoints.Count - 1; i++)
        {
            var p1 = greatCirclePoints[i];
            var p2 = greatCirclePoints[i + 1];

            // Filter out segments that wrap around (longitude jump > 180 degrees)
            double lonDiff = Math.Abs(p2.LonDegs - p1.LonDegs);
            if (lonDiff < 180)
            {
                var pixel1 = worldPlotter.LatLonToPixel(p1);
                var pixel2 = worldPlotter.LatLonToPixel(p2);
                worldPlotter.Plotter.DrawLine(pixel1, pixel2);
            }
        }

        testLog.AddComment($"Drew great circle route from London to Sydney with {greatCirclePoints.Count} points");

        // Draw ALL positions from KorePositionLibrary
        var pointColor = new KoreColorRGB(255, 100, 100); // Light red
        foreach (var position in KorePositionLibrary.Positions)
        {
            worldPlotter.DrawPoint(position.Value, pointColor, 3);
        }

        // Save
        worldPlotter.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "world_map_world.png"));
        testLog.AddComment($"World map with all {KorePositionLibrary.Positions.Count} positions saved to {KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "world_map_world.png")}");
        testLog.AddResult("World Map (All Positions)", true);
    }

    // --------------------------------------------------------------------------------------------

    private static void TestCoordinateMapping(KoreTestLog testLog)
    {
        var worldPlotter = new KoreWorldPlotter(3600, 1800);

        // Test round-trip conversion: LatLon -> Pixel -> LatLon
        var originalLL = new KoreLLPoint { LatDegs = 45.0, LonDegs = -120.0 }; // Somewhere in Oregon
        var pixel = worldPlotter.LatLonToPixel(originalLL);
        var roundTripLL = worldPlotter.PixelToLatLon(pixel);

        // Check if we got back approximately the same values (within tolerance)
        double latError = Math.Abs(originalLL.LatDegs - roundTripLL.LatDegs);
        double lonError = Math.Abs(originalLL.LonDegs - roundTripLL.LonDegs);

        bool passed = (latError < 0.01 && lonError < 0.01);

        string message = $"Original: {originalLL.LatDegs:F4}, {originalLL.LonDegs:F4} | " +
                        $"Pixel: {pixel.X:F1}, {pixel.Y:F1} | " +
                        $"RoundTrip: {roundTripLL.LatDegs:F4}, {roundTripLL.LonDegs:F4} | " +
                        $"Error: {latError:F6}, {lonError:F6}";

        testLog.AddResult("Coordinate Mapping Round-Trip", passed, message);

        // Test corner cases
        var northPole = new KoreLLPoint { LatDegs = 90, LonDegs = 0 };
        var southPole = new KoreLLPoint { LatDegs = -90, LonDegs = 0 };
        var dateLine = new KoreLLPoint { LatDegs = 0, LonDegs = 180 };
        var primeMeridian = new KoreLLPoint { LatDegs = 0, LonDegs = 0 };

        var npPixel = worldPlotter.LatLonToPixel(northPole);
        var spPixel = worldPlotter.LatLonToPixel(southPole);
        var dlPixel = worldPlotter.LatLonToPixel(dateLine);
        var pmPixel = worldPlotter.LatLonToPixel(primeMeridian);

        testLog.AddComment($"North Pole -> Pixel: ({npPixel.X:F0}, {npPixel.Y:F0})");
        testLog.AddComment($"South Pole -> Pixel: ({spPixel.X:F0}, {spPixel.Y:F0})");
        testLog.AddComment($"Date Line -> Pixel: ({dlPixel.X:F0}, {dlPixel.Y:F0})");
        testLog.AddComment($"Prime Meridian -> Pixel: ({pmPixel.X:F0}, {pmPixel.Y:F0})");
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Europe
    // --------------------------------------------------------------------------------------------

    private static void TestEuropeMap(KoreTestLog testLog)
    {
        // Create a regional map focused on Europe, extending south to include Gibraltar
        var europeBounds = new KoreLLBox()
        {
            MaxLatDegs = 71.0,  // Northern extent (Scandinavia)
            MinLatDegs = 28.0,  // Southern extent (includes Gibraltar at 36.14°N)
            MinLonDegs = -11.5, // Western extent (Atlantic/Ireland)
            MaxLonDegs = 40.0   // Eastern extent (Eastern Europe/Turkey)
        };

        // Get the width, the ratio and determine the height
        double pixelWidth   = 1600;
        double pixelsPerDeg = pixelWidth / europeBounds.DeltaLonDegs;
        double pixelHeight  = pixelWidth * (europeBounds.DeltaLatDegs / europeBounds.DeltaLonDegs);

        // create the map size accordingly
        var europeMap = new KoreWorldPlotter((int)Math.Round(pixelWidth), (int)Math.Round(pixelHeight), europeBounds); // Taller aspect ratio with square-degree scaling

        // Draw grid with 5-degree spacing
        KoreWorldPlotterOps.DrawGrid(europeMap, 5, 5, new KoreColorRGB(180, 180, 180), 1.0);

        // Draw border
        KoreWorldPlotterOps.DrawBorder(europeMap, new KoreColorRGB(100, 100, 100), 2.0);

        // Plot all positions from KorePositionLibrary that fall within the Europe bounding box
        var pointColor = new KoreColorRGB(200, 50, 50); // Dark red
        int positionCount = 0;

        foreach (var position in KorePositionLibrary.Positions)
        {
            if (europeBounds.Contains(position.Value))
            {
                europeMap.DrawPoint(position.Value, pointColor, 6);
                positionCount++;
            }
        }

        // Save
        europeMap.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "world_map_europe.png"));
        testLog.AddComment($"Regional Europe map with {positionCount} positions saved to {KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "world_map_europe.png")}");
        testLog.AddResult("Regional Map (Europe)", true);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: UK
    // --------------------------------------------------------------------------------------------

    private static void TestUKMap(KoreTestLog testLog)
    {
        // Create a UK and Ireland regional map
        // UK mainland: roughly 49.9°N to 59°N, -8°W to 2°E
        // Ireland: roughly 51.4°N to 55.4°N, -10.5°W to -5.5°W
        var ukBounds = new KoreLLBox()
        {
            MinLatDegs = 49.5,  // Southern extent (Isles of Scilly, Channel Islands)
            MaxLatDegs = 58.0,  // Northern extent (Shetland Islands)
            MinLonDegs = -5.0, // Western extent (Western Ireland)
            MaxLonDegs = 2.5    // Eastern extent (East Anglia)
        };

        // Define the image size
        double ukPixelWidth   = 2400;
        double ukPixelsPerDeg = ukPixelWidth / ukBounds.DeltaLonDegs;
        double ukPixelHeight  = ukPixelsPerDeg * ukBounds.DeltaLatDegs;

        // Create the image
        var ukMap = new KoreWorldPlotter((int)Math.Round(ukPixelWidth), (int)Math.Round(ukPixelHeight), ukBounds); // Maintain square-degree scaling

        // Draw the grid with 1-degree spacing, and labels
        KoreWorldPlotterOps.DrawGridWithLabels(ukMap, 1, 1, new KoreColorRGB(150, 150, 150), new KoreColorRGB(80, 80, 80), 1.0, 12);

        // Draw border
        KoreWorldPlotterOps.DrawBorder(ukMap, new KoreColorRGB(80, 80, 80), 20.0);

        // Plot all UK and Ireland positions from KorePositionLibrary
        var pointColor = new KoreColorRGB(50, 100, 200); // Blue
        int positionCount = 0;

        foreach (var position in KorePositionLibrary.Positions)
        {
            if (ukBounds.Contains(position.Value))
            {
                ukMap.DrawPoint(position.Value, pointColor, 8);
                positionCount++;
            }
        }

        // Load GeoJSON library and draw features prominently
        var geoLibrary = new KoreGeoFeatureLibrary();

        // Load point features (airport)
        string pointJsonPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "geo_feature_point.json");
        if (System.IO.File.Exists(pointJsonPath))
        {
            try
            {
                var LibIO = new KoreGeoJsonNativeIO(geoLibrary);
                LibIO.LoadFromJsonFile(pointJsonPath);

                // Draw all points from the library prominently
                foreach (var geoPoint in geoLibrary.GetAllPoints())
                {
                    if (ukBounds.Contains(geoPoint.Position))
                    {
                        // Draw a larger, more prominent point for the airport
                        ukMap.DrawGeoPoint(geoPoint);
                        positionCount++;
                        testLog.AddComment($"Loaded and drew point feature: {geoPoint.Name} from GeoJSON");
                    }
                }
            }
            catch (Exception ex)
            {
                testLog.AddComment($"Failed to load point GeoJSON: {ex.Message}");
            }
        }

        // Load line features (routes)
        string lineJsonPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "geo_feature_line.json");
        if (System.IO.File.Exists(lineJsonPath))
        {
            try
            {
                var lineLibrary = new KoreGeoFeatureLibrary();
                var LibIO = new KoreGeoJsonNativeIO(lineLibrary);
                LibIO.LoadFromJsonFile(lineJsonPath);

                // Draw all lines from the library
                foreach (var geoLine in lineLibrary.GetAllLineStrings())
                {
                    // Check if any point of the line is in bounds
                    bool lineInBounds = geoLine.Points.Any(p => ukBounds.Contains(p));
                    if (lineInBounds)
                    {
                        ukMap.DrawGeoLineString(geoLine);
                        testLog.AddComment($"Loaded and drew line feature: {geoLine.Name} from GeoJSON");
                    }
                }
            }
            catch (Exception ex)
            {
                testLog.AddComment($"Failed to load line GeoJSON: {ex.Message}");
            }
        }

        // Load UK country outline (MultiPolygon with all islands)
        string ukOutlinePath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "CountryOutline_UK.geojson");
        if (System.IO.File.Exists(ukOutlinePath))
        {
            try
            {
                var ukOutlineLibrary = new KoreGeoFeatureLibrary();
                var LibIO = new KoreGeoJsonNativeIO(ukOutlineLibrary);
                LibIO.LoadFromJsonFile(ukOutlinePath);

                int outlinePolygonCount = 0;
                // Draw all country outline polygons (UK mainland, islands, etc.)
                foreach (var geoPolygon in ukOutlineLibrary.GetAllPolygons())
                {
                    // Set outline colors for country border
                    geoPolygon.StrokeColor = new KoreColorRGB(100, 100, 180);  // Medium blue
                    geoPolygon.StrokeWidth = 1.5;
                    geoPolygon.FillColor = new KoreColorRGB(235, 245, 235, 60); // Very light green, semi-transparent

                    ukMap.DrawGeoPolygon(geoPolygon);
                    outlinePolygonCount++;
                }

                foreach (var multiPolygon in ukOutlineLibrary.GetAllMultiPolygons())
                {
                    multiPolygon.StrokeColor = new KoreColorRGB(100, 100, 180);  // Medium blue
                    multiPolygon.StrokeWidth = 1.5;
                    multiPolygon.FillColor = new KoreColorRGB(235, 245, 235, 60); // Very light green, semi-transparent

                    ukMap.DrawGeoMultiPolygon(multiPolygon);
                    outlinePolygonCount += multiPolygon.Polygons.Count;
                }

                testLog.AddComment($"Loaded and drew {outlinePolygonCount} UK country outline polygons from GeoJSON");
            }
            catch (Exception ex)
            {
                testLog.AddComment($"Failed to load UK country outline GeoJSON: {ex.Message}");
            }
        }

        // Load polygon features (areas)
        string polygonJsonPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "geo_feature_polygon.json");
        if (System.IO.File.Exists(polygonJsonPath))
        {
            try
            {
                var polygonLibrary = new KoreGeoFeatureLibrary();
                var LibIO = new KoreGeoJsonNativeIO(polygonLibrary);
                LibIO.LoadFromJsonFile(polygonJsonPath);

                // Draw all polygons from the library
                foreach (var geoPolygon in polygonLibrary.GetAllPolygons())
                {
                    // Check if any point of the outer ring is in bounds
                    bool polygonInBounds = geoPolygon.OuterRing.Any(p => ukBounds.Contains(p));
                    if (polygonInBounds)
                    {
                        ukMap.DrawGeoPolygon(geoPolygon);
                        testLog.AddComment($"Loaded and drew polygon feature: {geoPolygon.Name} from GeoJSON");
                    }
                }

                foreach (var multiPolygon in polygonLibrary.GetAllMultiPolygons())
                {
                    bool polygonInBounds = multiPolygon.Polygons.Any(poly => poly.OuterRing.Any(p => ukBounds.Contains(p)));
                    if (polygonInBounds)
                    {
                        ukMap.DrawGeoMultiPolygon(multiPolygon);
                        testLog.AddComment($"Loaded and drew multi-polygon feature: {multiPolygon.Name} from GeoJSON");
                    }
                }
            }
            catch (Exception ex)
            {
                testLog.AddComment($"Failed to load polygon GeoJSON: {ex.Message}");
            }
        }

        // Load all airports and draw UK airports
        string airportsPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "airport-codes.geojson");
        if (System.IO.File.Exists(airportsPath))
        {
            try
            {
                var airportsLibrary = new KoreGeoFeatureLibrary();
                var LibIO = new KoreGeoJsonNativeIO(airportsLibrary);
                LibIO.LoadFromJsonFile(airportsPath);

                // Get all airports that have an ICAO code
                var airportsWithIcao = airportsLibrary.GetFeaturesWithPropertyName("icao");

                int airportCount = 0;
                foreach (var feature in airportsWithIcao)
                {
                    // Only process point features
                    if (feature is KoreGeoPoint airportPoint)
                    {
                        // Check if airport is in UK bounds
                        if (ukBounds.Contains(airportPoint.Position))
                        {
                            // Draw airport as a small circle
                            var airportColor = new KoreColorRGB(200, 50, 50); // Dark red
                            ukMap.DrawPoint(airportPoint.Position, airportColor, 2);

                            // Draw ICAO code as label
                            string icaoCode = airportPoint.Properties["icao"].ToString() ?? "";
                            if (!string.IsNullOrEmpty(icaoCode))
                            {
                                ukMap.DrawTextAtPosition(icaoCode, airportPoint.Position, KoreXYRectPosition.TopRight, 8);
                            }

                            airportCount++;
                        }
                    }
                }

                testLog.AddComment($"Loaded and drew {airportCount} airports with ICAO codes in UK region");
            }
            catch (Exception ex)
            {
                testLog.AddComment($"Failed to load airports GeoJSON: {ex.Message}");
            }
        }

        // Add labels for major cities
        if (ukBounds.Contains(KorePositionLibrary.GetLLPos("London")))
            ukMap.DrawTextAtPosition("London", KorePositionLibrary.GetLLPos("London"), KoreXYRectPosition.BottomRight, 14);

        if (ukBounds.Contains(KorePositionLibrary.GetLLPos("Edinburgh")))
            ukMap.DrawTextAtPosition("Edinburgh", KorePositionLibrary.GetLLPos("Edinburgh"), KoreXYRectPosition.BottomLeft, 14);

        if (ukBounds.Contains(KorePositionLibrary.GetLLPos("Dublin")))
            ukMap.DrawTextAtPosition("Dublin", KorePositionLibrary.GetLLPos("Dublin"), KoreXYRectPosition.TopRight, 14);

        if (ukBounds.Contains(KorePositionLibrary.GetLLPos("Cardiff")))
            ukMap.DrawTextAtPosition("Cardiff", KorePositionLibrary.GetLLPos("Cardiff"), KoreXYRectPosition.TopLeft, 14);

        if (ukBounds.Contains(KorePositionLibrary.GetLLPos("Belfast")))
            ukMap.DrawTextAtPosition("Belfast", KorePositionLibrary.GetLLPos("Belfast"), KoreXYRectPosition.BottomCenter, 14);

        // Save
        ukMap.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "world_map_uk.png"));
        testLog.AddComment($"UK and Ireland map with {positionCount} positions saved to {KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "world_map_uk.png")}");
        testLog.AddResult("Regional Map (UK & Ireland)", true);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: UK Country Outline from Real GeoJSON Data
    // --------------------------------------------------------------------------------------------

    private static void TestUKCountryOutline(KoreTestLog testLog)
    {
        // Create a UK-focused map with appropriate bounds
        var ukBounds = new KoreLLBox
        {
            MinLonDegs = -11.0,  // West of Ireland
            MaxLonDegs = 2.5,    // East coast
            MinLatDegs = 49.5,   // South coast
            MaxLatDegs = 61.0    // North of Shetland
        };

        var ukMap = new KoreWorldPlotter(1800, 1800, ukBounds);

        // Draw the grid overlay (every 2 degrees)
        KoreWorldPlotterOps.DrawGrid(ukMap, 2, 2, new KoreColorRGB(220, 220, 220), 1.0);

        // Draw border
        KoreWorldPlotterOps.DrawBorder(ukMap);

        // Load UK country outline from MultiPolygon GeoJSON
        string ukOutlinePath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "CountryOutline_UK.geojson");
        if (System.IO.File.Exists(ukOutlinePath))
        {
            try
            {
                var ukLibrary = new KoreGeoFeatureLibrary();
                var LibIO = new KoreGeoJsonNativeIO(ukLibrary);
                LibIO.LoadFromJsonFile(ukOutlinePath);

                int polygonCount = 0;
                // Draw all polygons (UK mainland, islands, etc.)
                foreach (var geoPolygon in ukLibrary.GetAllPolygons())
                {
                    // Set outline colors for country border
                    geoPolygon.StrokeColor = new KoreColorRGB(50, 50, 150);  // Dark blue
                    geoPolygon.StrokeWidth = 2.0;
                    geoPolygon.FillColor = new KoreColorRGB(220, 240, 220, 80); // Light green, semi-transparent

                    ukMap.DrawGeoPolygon(geoPolygon);
                    polygonCount++;
                }

                foreach (var multiPolygon in ukLibrary.GetAllMultiPolygons())
                {
                    multiPolygon.StrokeColor = new KoreColorRGB(50, 50, 150);  // Dark blue
                    multiPolygon.StrokeWidth = 2.0;
                    multiPolygon.FillColor = new KoreColorRGB(220, 240, 220, 80); // Light green, semi-transparent

                    ukMap.DrawGeoMultiPolygon(multiPolygon);
                    polygonCount += multiPolygon.Polygons.Count;
                }

                testLog.AddComment($"Loaded and drew {polygonCount} polygons from UK GeoJSON");
            }
            catch (Exception ex)
            {
                testLog.AddComment($"Failed to load UK outline GeoJSON: {ex.Message}");
                testLog.AddResult("UK Country Outline", false);
                return;
            }
        }
        else
        {
            testLog.AddComment($"UK outline file not found at: {ukOutlinePath}");
            testLog.AddResult("UK Country Outline", false);
            return;
        }

        // Save
        ukMap.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "uk_country_outline.png"));
        testLog.AddComment($"UK country outline map saved to {KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "uk_country_outline.png")}");
        testLog.AddResult("UK Country Outline", true);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: All Countries
    // --------------------------------------------------------------------------------------------

    private static void TestAllCountriesMap(KoreTestLog testLog)
    {
        const string testName = "All Countries World Map";

        // Create world map
        var worldMap = new KoreWorldPlotter(3600, 1800);

        // Draw grid and border
        KoreWorldPlotterOps.DrawGrid(worldMap, 15, 15, new KoreColorRGB(220, 220, 220), 0.5);
        KoreWorldPlotterOps.DrawBorder(worldMap);

        // Check if all countries file exists
        string countriesPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "CountryOutline_All.geojson");
        if (System.IO.File.Exists(countriesPath))
        {
            try
            {
                // ONE LINE: Load the file into a library
                var countriesLibrary = new KoreGeoFeatureLibrary();
                var LibIO = new KoreGeoJsonNativeIO(countriesLibrary);
                LibIO.LoadFromJsonFile(countriesPath);

                // A FEW LINES: Iterate across all the countries and draw them on the world map
                int polygonCount = 0;
                foreach (var geoPolygon in countriesLibrary.GetAllPolygons())
                {
                    // Set colors for country outlines
                    geoPolygon.StrokeColor = new KoreColorRGB(80, 80, 80);  // Dark gray outline
                    geoPolygon.StrokeWidth = 0.5;
                    geoPolygon.FillColor = new KoreColorRGB(200, 220, 200, 120); // Light green fill, semi-transparent

                    worldMap.DrawGeoPolygon(geoPolygon);
                    polygonCount++;
                }

                foreach (var multiPolygon in countriesLibrary.GetAllMultiPolygons())
                {
                    multiPolygon.StrokeColor = new KoreColorRGB(80, 80, 80);  // Dark gray outline
                    multiPolygon.StrokeWidth = 0.5;
                    multiPolygon.FillColor = new KoreColorRGB(200, 220, 200, 120); // Light green fill, semi-transparent

                    worldMap.DrawGeoMultiPolygon(multiPolygon);
                    polygonCount += multiPolygon.Polygons.Count;
                }

                testLog.AddComment($"Loaded and drew {polygonCount} country polygons from CountryOutline_All.geojson");

                // Save
                worldMap.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "world_map_all_countries.png"));
                testLog.AddComment($"World map with all countries saved to {KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "world_map_all_countries.png")}");
                testLog.AddResult(testName, true);
            }
            catch (Exception ex)
            {
                testLog.AddComment($"Failed to load countries GeoJSON: {ex.Message}");
                testLog.AddResult(testName, false);
            }
        }
        else
        {
            testLog.AddComment($"Countries file not found at: {countriesPath} - skipping test");
            testLog.AddResult(testName, true); // Pass but skip
        }
    }
}
