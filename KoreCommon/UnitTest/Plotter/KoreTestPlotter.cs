
using System;
using System.Collections.Generic;

using SkiaSharp;
using KoreCommon.SkiaSharp;

namespace KoreCommon.UnitTest;


public static class KoreTestPlotter
{

    public static void RunTests(KoreTestLog testLog)
    {
        RunTest_BLPlot(testLog);
        RunTest_AnglePlot(testLog);
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
}

