// <fileheader>

using System;
using System.IO;

using KoreCommon;
using KoreCommon.UnitTest;
using KoreGIS;
namespace KoreGIS.UnitTest;
// Unit tests for KoreGeoFeatureLibrary - Route/RouteLeg functionality
public static partial class KoreTestGeoFeatureLibrary
{
    public static void TestMultiSegmentRouteAcrossUK(KoreTestLog testLog)
    {
        const string testName = "Multi-Segment Route Across UK";

        try
        {
            // Define UK bounds for the map
            var ukBounds = new KoreLLBox()
            {
                MaxLatDegs = 59.0,  // Northern Scotland
                MinLatDegs = 49.0,  // Southern England
                MinLonDegs = -8.0,  // Western Ireland
                MaxLonDegs = 2.5    // Eastern England
            };

            // Define the image size
            double ukPixelWidth = 2400;
            double ukPixelsPerDeg = ukPixelWidth / ukBounds.DeltaLonDegs;
            double ukPixelHeight = ukPixelsPerDeg * ukBounds.DeltaLatDegs;

            // Create the map
            var ukMap = new KoreWorldPlotter((int)Math.Round(ukPixelWidth), (int)Math.Round(ukPixelHeight), ukBounds);

            // Draw grid and border
            KoreWorldPlotterOps.DrawGridWithLabels(ukMap, 1, 1, new KoreColorRGB(150, 150, 150), new KoreColorRGB(80, 80, 80), 1.0, 12);
            KoreWorldPlotterOps.DrawBorder(ukMap, new KoreColorRGB(80, 80, 80), 20.0);

            // Load UK country outline
            string ukOutlinePath = Path.Combine(Directory.GetCurrentDirectory(), "UnitTestArtefacts", "CountryOutline_UK.geojson");
            if (File.Exists(ukOutlinePath))
            {
                var ukOutlineLibrary = new KoreGeoFeatureLibrary();
                var LibIO = new KoreGeoJsonNativeIO(ukOutlineLibrary);
                LibIO.LoadFromJsonFile(ukOutlinePath);

                foreach (var geoPolygon in ukOutlineLibrary.GetAllPolygons())
                {
                    geoPolygon.StrokeColor = new KoreColorRGB(100, 100, 180);
                    geoPolygon.StrokeWidth = 1.5;
                    geoPolygon.FillColor = new KoreColorRGB(235, 245, 235, 60);
                    ukMap.DrawGeoPolygon(geoPolygon);
                }

                foreach (var multiPolygon in ukOutlineLibrary.GetAllMultiPolygons())
                {
                    multiPolygon.StrokeColor = new KoreColorRGB(100, 100, 180);
                    multiPolygon.StrokeWidth = 1.5;
                    multiPolygon.FillColor = new KoreColorRGB(235, 245, 235, 60);
                    ukMap.DrawGeoMultiPolygon(multiPolygon);
                }
            }

            // Create waypoints for the route
            var london = new KoreLLPoint { LatDegs = 51.5074, LonDegs = -0.1278 };      // London
            var birmingham = new KoreLLPoint { LatDegs = 52.4862, LonDegs = -1.8904 };  // Birmingham
            var manchester = new KoreLLPoint { LatDegs = 53.4808, LonDegs = -2.2426 };  // Manchester
            var glasgow = new KoreLLPoint { LatDegs = 55.8642, LonDegs = -4.2518 };     // Glasgow
            var edinburgh = new KoreLLPoint { LatDegs = 55.9533, LonDegs = -3.1883 };   // Edinburgh

            // Create a multi-segment route demonstrating different leg types
            var route = new KoreGeoRoute
            {
                Name = "UK Tour Route",
                LineWidth = 3.0,
                Color = new KoreColorRGB(255, 100, 0) // Orange
            };

            // Leg 1: London to Birmingham - Straight line
            var leg1 = new KoreGeoRouteLegStraight
            {
                StartPoint = london,
                EndPoint = birmingham,
                Color = new KoreColorRGB(255, 0, 0), // Red
                LineWidth = 3.0
            };
            route.AddLeg(leg1);

            // Leg 2: Birmingham to Manchester - Great Circle
            var leg2 = new KoreGeoRouteLegGreatCircle
            {
                StartPoint = birmingham,
                EndPoint = manchester,
                Color = new KoreColorRGB(0, 255, 0), // Green
                LineWidth = 3.0
            };
            route.AddLeg(leg2);

            // Leg 3: Manchester to Glasgow - Bezier curve (smooth arc)
            var leg3 = new KoreGeoRouteLegBezier
            {
                StartPoint = manchester,
                EndPoint = glasgow,
                Color = new KoreColorRGB(0, 100, 255), // Blue
                LineWidth = 3.0
            };
            // Create a control point to the west for a smooth arc
            var controlPoint1 = new KoreLLPoint { LatDegs = 54.4, LonDegs = -3.5 };
            leg3.SetControlPoints(new System.Collections.Generic.List<KoreLLPoint> { manchester, controlPoint1, glasgow });
            route.AddLeg(leg3);

            // Leg 4: Glasgow to Edinburgh - Bezier curve with 4 control points
            var leg4 = new KoreGeoRouteLegBezier
            {
                StartPoint = glasgow,
                EndPoint = edinburgh,
                Color = new KoreColorRGB(255, 0, 255), // Magenta
                LineWidth = 3.0
            };
            // Create control points for a more complex curve
            var controlPoint2 = new KoreLLPoint { LatDegs = 55.95, LonDegs = -4.0 };
            var controlPoint3 = new KoreLLPoint { LatDegs = 55.9, LonDegs = -3.5 };
            leg4.SetControlPoints(new System.Collections.Generic.List<KoreLLPoint> { glasgow, controlPoint2, controlPoint3, edinburgh });
            route.AddLeg(leg4);

            // Calculate bounding box for the route
            route.CalcBoundingBox();

            // Draw each leg individually with its color
            int pointsPerLeg = 50; // More points for smooth curves

            foreach (var leg in route.Legs)
            {
                var legPoints = leg.GeneratePoints(pointsPerLeg);
                var legLineString = new KoreGeoLineString
                {
                    Points = legPoints,
                    Color = leg.Color,
                    LineWidth = leg.LineWidth
                };
                ukMap.DrawGeoLineString(legLineString);
            }

            // Draw waypoint markers
            var waypointColor = new KoreColorRGB(255, 255, 0); // Yellow
            var waypointSize = 8.0;

            ukMap.DrawPoint(london, waypointColor, waypointSize);
            ukMap.DrawPoint(birmingham, waypointColor, waypointSize);
            ukMap.DrawPoint(manchester, waypointColor, waypointSize);
            ukMap.DrawPoint(glasgow, waypointColor, waypointSize);
            ukMap.DrawPoint(edinburgh, waypointColor, waypointSize);

            // Draw control points for visualization
            var controlPointColor = new KoreColorRGB(255, 128, 0); // Orange
            var controlPointSize = 4.0;
            ukMap.DrawPoint(controlPoint1, controlPointColor, controlPointSize);
            ukMap.DrawPoint(controlPoint2, controlPointColor, controlPointSize);
            ukMap.DrawPoint(controlPoint3, controlPointColor, controlPointSize);

            // Save the map
            string outputPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "uk_multi_segment_route.png");
            ukMap.Save(outputPath);

            testLog.AddComment($"Route has {route.Legs.Count} legs and {route.WaypointCount} waypoints");
            testLog.AddComment($"Leg 1 (London->Birmingham): Straight - RED");
            testLog.AddComment($"Leg 2 (Birmingham->Manchester): Great Circle - GREEN");
            testLog.AddComment($"Leg 3 (Manchester->Glasgow): Bezier (3 points) - BLUE");
            testLog.AddComment($"Leg 4 (Glasgow->Edinburgh): Bezier (4 points) - MAGENTA");

            if (route.BoundingBox.HasValue)
            {
                var bbox = route.BoundingBox.Value;
                testLog.AddComment($"Route bbox: [{bbox.MinLatDegs:F2}, {bbox.MinLonDegs:F2}] to [{bbox.MaxLatDegs:F2}, {bbox.MaxLonDegs:F2}]");
            }

            testLog.AddResult(testName, true, $"Map saved to {outputPath}");
        }
        catch (Exception ex)
        {
            testLog.AddResult(testName, false, $"Exception: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Route JSON IO
    // --------------------------------------------------------------------------------------------

    public static void TestRouteGeoJSONKoreIO(KoreTestLog testLog)
    {
        const string testName = "Route GeoJSONKore Import/Export";

        try
        {
            // Create a test route with various leg types
            var route = new KoreGeoRoute
            {
                Name = "Test Route for JSON IO",
                Id = "test-route-001",
                LineWidth = 2.5,
                Color = new KoreColorRGB(128, 64, 200)
            };
            route.Properties["description"] = "A test route demonstrating all leg types";
            route.Properties["created"] = "2025-10-28";

            // Define waypoints
            var start = new KoreLLPoint { LatDegs = 51.5074, LonDegs = -0.1278 };     // London
            var point2 = new KoreLLPoint { LatDegs = 52.4862, LonDegs = -1.8904 };    // Birmingham
            var point3 = new KoreLLPoint { LatDegs = 53.4808, LonDegs = -2.2426 };    // Manchester
            var point4 = new KoreLLPoint { LatDegs = 55.8642, LonDegs = -4.2518 };    // Glasgow
            var end = new KoreLLPoint { LatDegs = 55.9533, LonDegs = -3.1883 };       // Edinburgh

            // Leg 1: Straight line
            var leg1 = new KoreGeoRouteLegStraight
            {
                Name = "Leg 1",
                Id = "leg-001",
                StartPoint = start,
                EndPoint = point2,
                Color = new KoreColorRGB(255, 0, 0),
                LineWidth = 2.5
            };
            leg1.Properties["type"] = "departure";

            // Leg 2: Great Circle
            var leg2 = new KoreGeoRouteLegGreatCircle
            {
                Name = "Leg 2",
                Id = "leg-002",
                StartPoint = point2,
                EndPoint = point3,
                Color = new KoreColorRGB(0, 255, 0),
                LineWidth = 2.5
            };

            // Leg 3: Bezier curve
            var leg3 = new KoreGeoRouteLegBezier
            {
                Name = "Leg 3",
                Id = "leg-003",
                StartPoint = point3,
                EndPoint = point4,
                Color = new KoreColorRGB(0, 100, 255),
                LineWidth = 2.5
            };
            var controlPoint = new KoreLLPoint { LatDegs = 54.5, LonDegs = -3.5 };
            leg3.SetControlPoints(new System.Collections.Generic.List<KoreLLPoint> { point3, controlPoint, point4 });

            route.AddLeg(leg1);
            route.AddLeg(leg2);
            route.AddLeg(leg3);

            route.CalcBoundingBox();

            // Export to GeoJSONKore
            string exportedJson = route.ToGeoJSONKore(30);

            // Save to file
            string outputPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "test_route.geojsonkore");
            route.SaveToGeoJSONKore(outputPath, 30);

            testLog.AddComment($"Exported route with {route.Legs.Count} legs to GeoJSONKore");
            testLog.AddComment($"JSON length: {exportedJson.Length} characters");

            // Import from GeoJSONKore
            var importedRoute = KoreGeoRouteGeoJSONKore.FromGeoJSONKore(exportedJson);

            testLog.AddComment($"Imported route with {importedRoute.Legs.Count} legs");
            testLog.AddComment($"Route name: {importedRoute.Name}");
            testLog.AddComment($"Route ID: {importedRoute.Id}");

            // Verify basic properties
            bool nameMatch = importedRoute.Name == route.Name;
            bool idMatch = importedRoute.Id == route.Id;
            bool legCountMatch = importedRoute.Legs.Count == route.Legs.Count;
            bool colorMatch = importedRoute.Color.R == route.Color.R &&
                             importedRoute.Color.G == route.Color.G &&
                             importedRoute.Color.B == route.Color.B;

            testLog.AddComment($"Name match: {nameMatch}");
            testLog.AddComment($"ID match: {idMatch}");
            testLog.AddComment($"Leg count match: {legCountMatch} ({importedRoute.Legs.Count} vs {route.Legs.Count})");
            testLog.AddComment($"Color match: {colorMatch}");

            // Verify leg types
            bool leg1TypeMatch = importedRoute.Legs[0] is KoreGeoRouteLegStraight;
            bool leg2TypeMatch = importedRoute.Legs[1] is KoreGeoRouteLegGreatCircle;
            bool leg3TypeMatch = importedRoute.Legs[2] is KoreGeoRouteLegBezier;

            testLog.AddComment($"Leg 1 type (Straight): {leg1TypeMatch}");
            testLog.AddComment($"Leg 2 type (GreatCircle): {leg2TypeMatch}");
            testLog.AddComment($"Leg 3 type (Bezier): {leg3TypeMatch}");

            // Check Bezier control points
            if (importedRoute.Legs[2] is KoreGeoRouteLegBezier importedBezier)
            {
                testLog.AddComment($"Bezier control points: {importedBezier.ControlPoints.Count}");
                bool controlPointMatch = importedBezier.ControlPoints.Count == 3;
                testLog.AddComment($"Control point count match: {controlPointMatch}");
            }

            // Load from file
            var loadedRoute = KoreGeoRouteGeoJSONKore.LoadFromGeoJSONKore(outputPath);
            testLog.AddComment($"Loaded route from file: {loadedRoute.Name} with {loadedRoute.Legs.Count} legs");

            bool allMatch = nameMatch && idMatch && legCountMatch && colorMatch &&
                           leg1TypeMatch && leg2TypeMatch && leg3TypeMatch;

            testLog.AddResult(testName, allMatch,
                allMatch ? $"Successfully exported and imported route. File saved to {outputPath}"
                         : "Some properties did not match after import");
        }
        catch (Exception ex)
        {
            testLog.AddResult(testName, false, $"Exception: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Flex Route
    // --------------------------------------------------------------------------------------------

    public static void TestFlexibleJoinRouteAcrossUK(KoreTestLog testLog)
    {
        const string testName = "Flexible Join Route Across UK";

        try
        {
            // Define UK bounds for the map
            var ukBounds = new KoreLLBox()
            {
                MaxLatDegs = 59.0,  // Northern Scotland
                MinLatDegs = 49.0,  // Southern England
                MinLonDegs = -8.0,  // Western Ireland
                MaxLonDegs = 2.5    // Eastern England
            };

            // Define the image size
            double ukPixelWidth = 2400;
            double ukPixelsPerDeg = ukPixelWidth / ukBounds.DeltaLonDegs;
            double ukPixelHeight = ukPixelsPerDeg * ukBounds.DeltaLatDegs;

            // Create the map
            var ukMap = new KoreWorldPlotter((int)Math.Round(ukPixelWidth), (int)Math.Round(ukPixelHeight), ukBounds);

            // Draw grid and border
            KoreWorldPlotterOps.DrawGridWithLabels(ukMap, 1, 1, new KoreColorRGB(150, 150, 150), new KoreColorRGB(80, 80, 80), 1.0, 12);
            KoreWorldPlotterOps.DrawBorder(ukMap, new KoreColorRGB(80, 80, 80), 20.0);

            // Load UK country outline
            string ukOutlinePath = Path.Combine(Directory.GetCurrentDirectory(), "UnitTestArtefacts", "CountryOutline_UK.geojson");
            if (File.Exists(ukOutlinePath))
            {
                var ukOutlineLibrary = new KoreGeoFeatureLibrary();
                var LibIO = new KoreGeoJsonNativeIO(ukOutlineLibrary);
                LibIO.LoadFromJsonFile(ukOutlinePath);

                foreach (var geoPolygon in ukOutlineLibrary.GetAllPolygons())
                {
                    geoPolygon.StrokeColor = new KoreColorRGB(100, 100, 180);
                    geoPolygon.StrokeWidth = 1.5;
                    geoPolygon.FillColor = new KoreColorRGB(235, 245, 235, 60);
                    ukMap.DrawGeoPolygon(geoPolygon);
                }

                foreach (var multiPolygon in ukOutlineLibrary.GetAllMultiPolygons())
                {
                    multiPolygon.StrokeColor = new KoreColorRGB(100, 100, 180);
                    multiPolygon.StrokeWidth = 1.5;
                    multiPolygon.FillColor = new KoreColorRGB(235, 245, 235, 60);
                    ukMap.DrawGeoMultiPolygon(multiPolygon);
                }
            }

            // Create waypoints - a sharp zigzag pattern to demonstrate smooth joins
            var london = new KoreLLPoint { LatDegs = 51.5074, LonDegs = -0.1278 };      // London
            var birmingham = new KoreLLPoint { LatDegs = 52.4862, LonDegs = -1.8904 };  // Birmingham
            var liverpool = new KoreLLPoint { LatDegs = 53.4084, LonDegs = -2.9916 };   // Liverpool
            var leeds = new KoreLLPoint { LatDegs = 53.8008, LonDegs = -1.5491 };       // Leeds
            var newcastle = new KoreLLPoint { LatDegs = 54.9783, LonDegs = -1.6178 };   // Newcastle
            var edinburgh = new KoreLLPoint { LatDegs = 55.9533, LonDegs = -3.1883 };   // Edinburgh

            // Create route with straight legs connected by flexible joins
            var route = new KoreGeoRoute
            {
                Name = "UK Zigzag Route with Flexible Joins",
                LineWidth = 3.0,
                Color = new KoreColorRGB(255, 100, 0)
            };

            // Main leg 1: London to Birmingham (straight)
            var leg1 = new KoreGeoRouteLegStraight
            {
                StartPoint = london,
                EndPoint = birmingham,
                Color = new KoreColorRGB(255, 0, 0), // Red
                LineWidth = 3.0
            };

            // Main leg 2: Liverpool to Leeds (straight)
            var leg2 = new KoreGeoRouteLegStraight
            {
                StartPoint = liverpool,
                EndPoint = leeds,
                Color = new KoreColorRGB(0, 255, 0), // Green
                LineWidth = 3.0
            };

            // Main leg 3: Newcastle to Edinburgh (straight)
            var leg3 = new KoreGeoRouteLegStraight
            {
                StartPoint = newcastle,
                EndPoint = edinburgh,
                Color = new KoreColorRGB(0, 100, 255), // Blue
                LineWidth = 3.0
            };

            // Flexible join 1: Birmingham to Liverpool (connects leg1 to leg2)
            var join1 = new KoreGeoRouteLegFlexibleJoin
            {
                StartPoint = birmingham,
                EndPoint = liverpool,
                PreviousLeg = leg1,
                NextLeg = leg2,
                Color = new KoreColorRGB(255, 165, 0), // Orange
                LineWidth = 3.0,
                StartControlPointDistanceM = 60000.0, // 60km - moderate curve at entry
                EndControlPointDistanceM = 80000.0    // 80km - wider sweep at exit
            };

            // Flexible join 2: Leeds to Newcastle (connects leg2 to leg3)
            var join2 = new KoreGeoRouteLegFlexibleJoin
            {
                StartPoint = leeds,
                EndPoint = newcastle,
                PreviousLeg = leg2,
                NextLeg = leg3,
                Color = new KoreColorRGB(255, 0, 255), // Magenta
                LineWidth = 3.0,
                StartControlPointDistanceM = 40000.0,  // 40km - tighter at entry
                EndControlPointDistanceM = 70000.0     // 70km - wider sweep at exit
            };

            // Add legs in sequence
            route.AddLeg(leg1);
            route.AddLeg(join1);
            route.AddLeg(leg2);
            route.AddLeg(join2);
            route.AddLeg(leg3);

            // Calculate bounding box for the route
            route.CalcBoundingBox();

            // Draw each leg individually with its color
            int pointsPerLeg = 50;

            foreach (var leg in route.Legs)
            {
                var legPoints = leg.GeneratePoints(pointsPerLeg);
                var legLineString = new KoreGeoLineString
                {
                    Points = legPoints,
                    Color = leg.Color,
                    LineWidth = leg.LineWidth
                };
                ukMap.DrawGeoLineString(legLineString);
            }

            // Draw waypoint markers
            var waypointColor = new KoreColorRGB(255, 255, 0); // Yellow
            var waypointSize = 8.0;

            ukMap.DrawPoint(london, waypointColor, waypointSize);
            ukMap.DrawPoint(birmingham, waypointColor, waypointSize);
            ukMap.DrawPoint(liverpool, waypointColor, waypointSize);
            ukMap.DrawPoint(leeds, waypointColor, waypointSize);
            ukMap.DrawPoint(newcastle, waypointColor, waypointSize);
            ukMap.DrawPoint(edinburgh, waypointColor, waypointSize);

            // Draw calculated control points for the flexible joins
            var controlPointColor = new KoreColorRGB(255, 128, 0); // Orange
            var controlPointSize = 4.0;

            var join1ControlPoints = join1.GetControlPoints();
            var join2ControlPoints = join2.GetControlPoints();

            // Draw control points (skip first and last as they're the start/end points)
            if (join1ControlPoints.Count >= 4)
            {
                ukMap.DrawPoint(join1ControlPoints[1], controlPointColor, controlPointSize);
                ukMap.DrawPoint(join1ControlPoints[2], controlPointColor, controlPointSize);
            }

            if (join2ControlPoints.Count >= 4)
            {
                ukMap.DrawPoint(join2ControlPoints[1], controlPointColor, controlPointSize);
                ukMap.DrawPoint(join2ControlPoints[2], controlPointColor, controlPointSize);
            }

            // Save the map
            string outputPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "uk_flexible_join_route.png");
            ukMap.Save(outputPath);

            testLog.AddComment($"Route has {route.Legs.Count} legs ({route.WaypointCount} waypoints)");
            testLog.AddComment($"Leg 1 (London->Birmingham): Straight - RED");
            testLog.AddComment($"Join 1 (Birmingham->Liverpool): Flexible Join - ORANGE");
            testLog.AddComment($"Leg 2 (Liverpool->Leeds): Straight - GREEN");
            testLog.AddComment($"Join 2 (Leeds->Newcastle): Flexible Join - MAGENTA");
            testLog.AddComment($"Leg 3 (Newcastle->Edinburgh): Straight - BLUE");
            testLog.AddComment($"Flexible joins automatically match entry/exit directions for smooth transitions");

            if (route.BoundingBox.HasValue)
            {
                var bbox = route.BoundingBox.Value;
                testLog.AddComment($"Route bbox: [{bbox.MinLatDegs:F2}, {bbox.MinLonDegs:F2}] to [{bbox.MaxLatDegs:F2}, {bbox.MaxLonDegs:F2}]");
            }

            testLog.AddResult(testName, true, $"Map saved to {outputPath}");
        }
        catch (Exception ex)
        {
            testLog.AddResult(testName, false, $"Exception: {ex.Message}\n{ex.StackTrace}");
        }
    }
}

