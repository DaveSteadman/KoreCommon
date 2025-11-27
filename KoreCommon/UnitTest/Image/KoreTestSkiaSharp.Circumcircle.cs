// <fileheader>

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using KoreCommon;
using KoreCommon.SkiaSharp;
using SkiaSharp;

namespace KoreCommon.UnitTest;

public static partial class KoreTestSkiaSharp
{
    // Test circumcircle calculation and visualization
    // KoreTestSkiaSharp.TestCircumcircle
    private static void TestCircumcircle(KoreTestLog testLog)
    {
        // Create a new image for circumcircle testing
        var imagePlotter = new KoreSkiaSharpPlotter(800, 600);

        // Clear background to white
        imagePlotter.Clear(SKColors.White);

        // Test Case 1: Regular triangle
        var triangle1 = new KoreXYTriangle(
            new KoreXYVector(150, 100),  // A
            new KoreXYVector(350, 150),  // B
            new KoreXYVector(250, 300)   // C
        );

        DrawTriangleWithCircumcircle(imagePlotter, triangle1, "Acute Triangle", new KoreXYVector(200, 50));

        // Test Case 2: Right triangle
        var triangle2 = new KoreXYTriangle(
            new KoreXYVector(450, 100),  // A
            new KoreXYVector(600, 100),  // B (horizontal)
            new KoreXYVector(450, 250)   // C (vertical - makes right angle at A)
        );

        DrawTriangleWithCircumcircle(imagePlotter, triangle2, "Right Triangle", new KoreXYVector(525, 50));

        // Test Case 3: Obtuse triangle
        var triangle3 = new KoreXYTriangle(
            new KoreXYVector(100, 420),  // A
            new KoreXYVector(170, 360),  // B
            new KoreXYVector(150, 570)   // C (moved further down to create obtuse angle at A)
        );

        DrawTriangleWithCircumcircle(imagePlotter, triangle3, "Obtuse Triangle", new KoreXYVector(200, 350));

        // Test Case 4: Nearly degenerate triangle (very thin)
        var triangle4 = new KoreXYTriangle(
            new KoreXYVector(450, 400),  // A
            new KoreXYVector(650, 410),  // B (slightly offset)
            new KoreXYVector(550, 405)   // C (nearly collinear)
        );

        DrawTriangleWithCircumcircle(imagePlotter, triangle4, "Thin Triangle", new KoreXYVector(550, 350));

        // Save the test image
        string filePath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "circumcircle_test.png");
        imagePlotter.Save(filePath);

        testLog.AddResult("Circumcircle test", true, "Test completed - check " + filePath);
    }

    // Helper method to draw a triangle with its circumcircle
    private static void DrawTriangleWithCircumcircle(KoreSkiaSharpPlotter plotter, KoreXYTriangle triangle, string label, KoreXYVector labelPos)
    {
        // Calculate circumcircle
        var circumcircle = triangle.Circumcircle();

        // Draw the circumcircle first (so triangle draws on top)
        plotter.DrawSettings.Color = SKColors.LightGray;
        plotter.DrawSettings.LineWidth = 1;
        plotter.DrawSettings.IsAntialias = true;

        // Draw circumcircle (we'll need to add circle drawing capability)
        if (!double.IsInfinity(circumcircle.Radius))
        {
            // For now, draw circle as multiple line segments (approximation)
            DrawCircleApproximation(plotter, circumcircle.Center, circumcircle.Radius);
        }

        // Draw the triangle
        plotter.DrawSettings.Color = SKColors.Blue;
        plotter.DrawSettings.LineWidth = 2;
        plotter.DrawLine(triangle.A, triangle.B);
        plotter.DrawLine(triangle.B, triangle.C);
        plotter.DrawLine(triangle.C, triangle.A);

        // Mark the vertices
        plotter.DrawSettings.Color = SKColors.Green;
        plotter.DrawPoint(triangle.A, 4);
        plotter.DrawPoint(triangle.B, 4);
        plotter.DrawPoint(triangle.C, 4);

        // Mark the circumcenter
        plotter.DrawSettings.Color = SKColors.Red;
        plotter.DrawPoint(circumcircle.Center, 6);

        // Add label
        plotter.DrawSettings.Color = SKColors.Black;
        plotter.DrawText(label, labelPos, 16);

        // Add radius info
        string radiusText = double.IsInfinity(circumcircle.Radius) ? "R = ∞" : $"R = {circumcircle.Radius:F1}";
        plotter.DrawText(radiusText, labelPos.Offset(0, 20), 12);
    }

    // Helper method to draw a circle using line segments (until we add proper circle drawing)
    private static void DrawCircleApproximation(KoreSkiaSharpPlotter plotter, KoreXYVector center, double radius)
    {
        const int segments = 64; // Number of line segments to approximate the circle
        const double angleStep = 2 * Math.PI / segments;

        KoreXYVector? prevPoint = null;

        for (int i = 0; i <= segments; i++)
        {
            double angle = i * angleStep;
            var point = new KoreXYVector(
                center.X + radius * Math.Cos(angle),
                center.Y + radius * Math.Sin(angle)
            );

            if (prevPoint.HasValue)
            {
                plotter.DrawLine(prevPoint.Value, point);
            }
            prevPoint = point;
        }
    }
}
