
using System;
using System.Collections.Generic;

using SkiaSharp;
using KoreCommon.SkiaSharp;

namespace KoreCommon.UnitTest;

#nullable enable

public static class KoreTestPlotter
{

    public static void RunTests(KoreTestLog testLog)
    {
        RunTest_BLPlot(testLog);
        RunTest_AnglePlot(testLog);
        RunTest_CircularGradient(testLog);
    }

    public static void RunTest_BLPlot(KoreTestLog testLog)
    {
        try
        {
            KoreSkiaSharpPlotter plotter = new(1000, 1000); // 1000x1000 pixels

            // Define out basic objects
            KoreXYCircle circle1 = new(250, 300, 200); // X, y, radius (scaled 5x)
            KoreXYCircle circle2 = new(500, 500, 400);
            KoreXYLine line1 = new(60, 110, 440, 595);
            KoreXYLine line2 = new(650, 685, 895, 65);
            KoreXYLine line3 = new(700, 750, 850, 900);
            KoreXYLine line4 = new(800, 800, 900, 900);

            // Set the line width
            plotter.DrawSettings.Paint.StrokeWidth = 3;
            plotter.DrawSettings.PointCrossSize = 5;
            plotter.DrawSettings.LineSpacing = 3f;
            plotter.DrawSettings.TextSize = 14f;

            // Draw our basic objects
            plotter.DrawSettings.Color = SKColors.LightGray;
            plotter.DrawCircle(circle1);
            plotter.DrawLine(line1);
            plotter.DrawLine(line3);
            plotter.DrawLine(line4);

            plotter.DrawSettings.Color = SKColors.LightBlue;
            plotter.DrawCircle(circle2);
            plotter.DrawLine(line2);

            KoreXYRect outline = new(10, 10, 990, 990);
            plotter.DrawRect(outline);

            // --- RED: Tangent points ---

            plotter.DrawSettings.Color = SKColors.Red;
            plotter.DrawPoint(line1.P2);

            List<KoreXYVector> circle1Tangents1 = KoreXYCircleOps.TangentPoints(circle1, line1.P2);
            foreach (KoreXYVector p in circle1Tangents1)
            {
                plotter.DrawSettings.Color = SKColors.Gray;
                plotter.DrawLine(line1.P2, p);
                plotter.DrawSettings.Color = SKColors.Red;
                plotter.DrawPoint(p);
            }

            plotter.DrawSettings.Color = SKColors.Red;
            plotter.DrawPoint(line1.P1);

            List<KoreXYVector> circle1Tangents2 = KoreXYCircleOps.TangentPoints(circle1, line1.P1);
            foreach (KoreXYVector p in circle1Tangents2)
            {
                plotter.DrawSettings.Color = SKColors.Gray;
                plotter.DrawLine(line1.P1, p);
                plotter.DrawSettings.Color = SKColors.Red;
                plotter.DrawPoint(p);
            }

            // --- GREEN: Intersection points ---
            plotter.DrawSettings.Color = SKColors.Green;

            List<KoreXYVector> circleInt1 = KoreXYCircleOps.IntersectionPoints(circle1, line1);
            foreach (KoreXYVector p in circleInt1)
                plotter.DrawPoint(p);

            List<KoreXYVector> circleInt2 = KoreXYCircleOps.IntersectionPoints(circle2, line2);
            foreach (KoreXYVector p in circleInt2)
                plotter.DrawPoint(p);

            List<KoreXYVector> circleInt3 = KoreXYCircleOps.IntersectionPoints(circle2, line1);
            foreach (KoreXYVector p in circleInt3)
                plotter.DrawPoint(p);

            List<KoreXYVector> circleInt4 = KoreXYCircleOps.IntersectionPoints(circle2, line3);
            foreach (KoreXYVector p in circleInt4)
                plotter.DrawPoint(p);

            List<KoreXYVector> circleInt5 = KoreXYCircleOps.IntersectionPoints(circle2, line4);
            foreach (KoreXYVector p in circleInt5)
                plotter.DrawPoint(p);

            // --- MAGENTA: Circle intersection points ---

            plotter.DrawSettings.Color = SKColors.Magenta;
            List<KoreXYVector> circleCircleInt1 = KoreXYCircleOps.IntersectionPoints(circle1, circle2);
            foreach (KoreXYVector p in circleCircleInt1)
                plotter.DrawPoint(p);

            KoreXYVector textPoint = new(600, 50);
            plotter.DrawSettings.Color = SKColors.Orange;
            plotter.DrawPoint(textPoint);

            plotter.DrawSettings.Paint.StrokeWidth = 1;
            plotter.DrawSettings.Paint.Style = SKPaintStyle.Fill;
            plotter.DrawSettings.Color = SKColors.Black;
            plotter.DrawTextAtPosition($"{KoreCentralTime.TimestampLocal}\nPlotter Test 1 // BL Origin", textPoint, KoreXYRectPosition.BottomLeft);
            plotter.DrawSettings.ResetToDefaults();

            plotter.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "Plotter_Test.png"));
        }
        catch (Exception e)
        {
            testLog.AddComment($"KoreTestPlotter Exception: {false}, {e.Message}");
        }
    }

    public static void RunTest_AnglePlot(KoreTestLog testLog)
    {
        try
        {
            KoreSkiaSharpPlotter plotter = new(800, 800); // 800x800 pixels

            // Define out basic objects (scaled 40x)
            KoreXYCircle circleMain = new(400, 400, 360); // X, y, radius

            // Set the line width
            plotter.DrawSettings.Paint.StrokeWidth = 3;
            plotter.DrawSettings.PointCrossSize = 5;
            plotter.DrawSettings.LineSpacing = 3f;
            plotter.DrawSettings.TextSize = 14f;

            // Draw basic outline, just confirm we have the right draw area
            plotter.DrawSettings.Color = SKColors.LightGray;
            KoreXYRect outline = new(4, 4, 796, 796);
            plotter.DrawRect(outline);

            // Draw our main objects
            plotter.DrawSettings.Color = SKColors.LightGray;
            plotter.DrawCircle(circleMain);

            // Create arc from the main circle
            KoreXYArc arcMain = new(circleMain.Center, circleMain.Radius - 40, 0, KoreValueUtils.DegsToRads(80));
            plotter.DrawArc(arcMain);

            // Draw tghe Arc points (greeen start, red end)
            plotter.DrawSettings.Color = SKColors.Green;
            plotter.DrawPoint(arcMain.StartPoint);
            plotter.DrawSettings.Color = SKColors.Red;
            plotter.DrawPoint(arcMain.EndPoint);

            // Draw an arc box
            KoreXYAnnularSector arcBox = new(circleMain.Center, arcMain.Radius - 200, arcMain.Radius - 40, arcMain.StartAngleRads, KoreValueUtils.DegsToRads(80));
            plotter.DrawArcBox(arcBox);

            // Draw an intersecting line
            KoreXYLine line1 = new(120, 600, 760, 520);
            plotter.DrawSettings.Color = SKColors.LightGray;
            plotter.DrawLine(line1);

            List<KoreXYVector> arcInts = KoreXYAnnularSectorOps.IntersectionPoints(arcBox, line1);
            plotter.DrawSettings.Color = SKColors.Magenta;
            foreach (KoreXYVector p in arcInts)
                plotter.DrawPoint(p);

            // Test the 3 point bezier curve
            {
                KoreXYVector pA = new(40, 40);
                KoreXYVector pB = new(200, 400);
                KoreXYVector pC = new(600, 200);
                KoreXYLine lineAB = new(pA, pB);
                KoreXYLine lineBC = new(pB, pC);
                KoreXYPolyLine? bezier = KoreXYPolyLineOps.Create3PointBezier(pA, pB, pC, 10);

                if (bezier != null)
                {
                    // Draw the points, lines and bezier
                    plotter.DrawSettings.Color = SKColors.LightBlue;
                    plotter.DrawLine(lineAB);
                    plotter.DrawLine(lineBC);

                    plotter.DrawSettings.Color = SKColors.LightGreen;
                    plotter.DrawPath(bezier.Points);

                    plotter.DrawSettings.Color = SKColors.Magenta;
                    plotter.DrawPoint(pA);
                    plotter.DrawPoint(pB);
                    plotter.DrawPoint(pC);
                }
            }

            // Test the 4 point bezier curve
            {
                KoreXYVector pA = new(40, 760);
                KoreXYVector pB = new(120, 560);
                KoreXYVector pC = new(200, 600);
                KoreXYVector pD = new(280, 720);
                KoreXYLine lineAB = new(pA, pB);
                KoreXYLine lineBC = new(pB, pC);
                KoreXYLine lineCD = new(pC, pD);
                KoreXYPolyLine? bezier = KoreXYPolyLineOps.Create4PointBezier(pA, pB, pC, pD, 6);

                if (bezier != null)
                {
                    // Draw the points, lines and bezier
                    plotter.DrawSettings.Color = SKColors.LightBlue;
                    plotter.DrawLine(lineAB);
                    plotter.DrawLine(lineBC);
                    plotter.DrawLine(lineCD);

                    plotter.DrawSettings.Color = SKColors.LightGreen;
                    plotter.DrawPath(bezier.Points);

                    plotter.DrawSettings.Color = SKColors.Magenta;
                    plotter.DrawPointAsCircle(pA);
                    plotter.DrawPointAsCircle(pB);
                    plotter.DrawPointAsCircle(pC);
                    plotter.DrawPointAsCircle(pD);
                }
            }

            // draw an outline box, check the bounds
            KoreXYRect outline2 = new(4, 4, 796, 796);
            plotter.DrawSettings.Color = SKColors.LightBlue;
            plotter.DrawSettings.Paint.StrokeWidth = 2;
            plotter.DrawRect(outline2);

            // Final: Save plot:
            plotter.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "Plotter_Test2.png"));
        }
        catch (Exception e)
        {
            testLog.AddResult("KoreTestPlotter Exception", false, e.Message);
        }
    }

    public static void RunTest_CircularGradient(KoreTestLog testLog)
    {
        try
        {
            KoreSkiaSharpPlotter plotter = new(800, 500);

            // Fill background with white
            // plotter.DrawSettings.Color = SKColors.White;
            // plotter.DrawSettings.Paint.Style = SKPaintStyle.Fill;
            // plotter.FillRect(new KoreXYRect(0, 0, 800, 500));
            // plotter.DrawSettings.ResetToDefaults();

            // Create a red to blue gradient centered at centre-left (x=200, y=250)
            KoreXYVector gradientCenter = new(200, 250);
            float gradientRadius = 300;

            plotter.DrawCircularGradient(gradientCenter, gradientRadius, SKColors.Red, SKColors.Blue);

            // Draw a small marker at the gradient center for reference
            plotter.DrawSettings.Color = SKColors.Black;
            plotter.DrawSettings.Paint.StrokeWidth = 2;
            plotter.DrawPoint(gradientCenter);

            // Create a 5-point star polygon on the right side
            KoreXYVector starCenter = new(600, 250);
            float outerRadius = 120;
            float innerRadius = 50;
            List<KoreXYVector> starPoints = new();

            for (int i = 0; i < 10; i++)
            {
                double angle = (i * Math.PI / 5.0) - (Math.PI / 2.0); // Start from top
                float radius = (i % 2 == 0) ? outerRadius : innerRadius;
                float x = (float)starCenter.X + (float)(radius * Math.Cos(angle));
                float y = (float)starCenter.Y + (float)(radius * Math.Sin(angle));
                starPoints.Add(new KoreXYVector(x, y));
            }

            KoreXYPolygon starPolygon = new(starPoints);

            // Draw gradient within the star
            plotter.DrawCircularGradientInPolygon(starPolygon, starCenter, 150, SKColors.Yellow, SKColors.Purple);

            // Draw star outline for visibility
            plotter.DrawSettings.Color = SKColors.Black;
            plotter.DrawSettings.Paint.StrokeWidth = 2;
            plotter.DrawSettings.Paint.Style = SKPaintStyle.Stroke;
            plotter.DrawSettings.Paint.StrokeCap = SKStrokeCap.Round;
            plotter.DrawSettings.Paint.StrokeJoin = SKStrokeJoin.Round;
            for (int i = 0; i < starPoints.Count; i++)
            {
                plotter.DrawLine(starPoints[i], starPoints[(i + 1) % starPoints.Count]);
            }

            // Create another 5-point star with multi-color gradient (bottom center)
            KoreXYVector star2Center = new(400, 380);
            float outerRadius2 = 90;
            float innerRadius2 = 45;
            float drawRadius = (outerRadius2 * 2) + 10;
            List<KoreXYVector> star2Points = new();

            for (int i = 0; i < 10; i++)
            {
                double angle = (i * Math.PI / 5.0) - (Math.PI / 2.0); // Start from top
                float radius = (i % 2 == 0) ? outerRadius2 : innerRadius2;
                float x = (float)star2Center.X + (float)(radius * Math.Cos(angle));
                float y = (float)star2Center.Y + (float)(radius * Math.Sin(angle));
                star2Points.Add(new KoreXYVector(x, y));
            }

            KoreXYPolygon star2Polygon = new(star2Points);
            KoreColorRange colorRange = KoreColorRange.BlueGreenYellowOrangeRed();

            // Draw multi-color gradient within the star
            plotter.DrawColorRangeGradientInPolygon(star2Polygon, star2Polygon.Vertices[0], drawRadius, colorRange);

            // Draw star2 outline for visibility
            plotter.DrawSettings.Color = SKColors.Black;
            plotter.DrawSettings.Paint.StrokeWidth = 4;
            plotter.DrawSettings.Paint.Style = SKPaintStyle.Stroke;
            plotter.DrawSettings.Paint.StrokeCap = SKStrokeCap.Round;
            plotter.DrawSettings.Paint.StrokeJoin = SKStrokeJoin.Round;
            for (int i = 0; i < star2Points.Count; i++)
            {
                plotter.DrawLine(star2Points[i], star2Points[(i + 1) % star2Points.Count]);
            }

            // Save the test image
            plotter.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "Plotter_CircularGradient.png"));

            testLog.AddResult("Circular Gradient Test", true, "Created 800x500 gradient image");
        }
        catch (Exception e)
        {
            testLog.AddResult("Circular Gradient Test", false, e.Message);
        }
    }
}

