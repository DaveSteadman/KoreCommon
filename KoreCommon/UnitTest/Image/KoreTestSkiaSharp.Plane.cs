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
    // Test plane projection - draw circles on 3D planes and project back to 2D
    // KoreTestSkiaSharp.TestPlane
    private static void TestPlane(KoreTestLog testLog)
    {
        // Create a new image for plane testing - expanded to use full area
        var imagePlotter = new KoreSkiaSharpPlotter(1200, 800);

        // Clear background to white
        imagePlotter.Clear(SKColors.White);

        // Layout: 3x2 grid to use full image space
        // Row 1: 0°, 30°, 45°
        // Row 2: 60°, 90°, Complex

        // Test Case 1: Flat plane (XY plane at Z=0) - 0°
        var flatPlane = KoreXYZPlane.MakePlane(
            new KoreXYZVector(0, 0, 0),     // Origin
            new KoreXYZVector(0, 0, 1),     // Normal (Z-up)
            new KoreXYZVector(0, 1, 0)      // Y-axis (world Y)
        );

        DrawPlaneWithCircle(imagePlotter, flatPlane, "0° Plane (XY)",
            new KoreXYVector(200, 200), new KoreXYVector(120, 120), 150);

        // Test Case 2: 30 degree plane (tilted around X-axis)
        var angle30 = Math.PI / 6; // 30 degrees
        var plane30 = KoreXYZPlane.MakePlane(
            new KoreXYZVector(0, 0, 0),                                    // Origin
            new KoreXYZVector(0, Math.Sin(angle30), Math.Cos(angle30)),   // Normal tilted 30° around X
            new KoreXYZVector(0, Math.Cos(angle30), -Math.Sin(angle30))   // Y-axis rotated accordingly
        );

        DrawPlaneWithCircle(imagePlotter, plane30, "30° Plane (tilted around X-axis)",
            new KoreXYVector(600, 200), new KoreXYVector(520, 120), 150);

        // Test Case 3: 45 degree plane (tilted around X-axis)
        var angle45 = Math.PI / 4; // 45 degrees
        var plane45 = KoreXYZPlane.MakePlane(
            new KoreXYZVector(0, 0, 0),                                    // Origin
            new KoreXYZVector(0, Math.Sin(angle45), Math.Cos(angle45)),   // Normal tilted 45° around X
            new KoreXYZVector(0, Math.Cos(angle45), -Math.Sin(angle45))   // Y-axis rotated accordingly
        );

        DrawPlaneWithCircle(imagePlotter, plane45, "45° Plane (tilted around X-axis)",
            new KoreXYVector(1000, 200), new KoreXYVector(920, 120), 150);

        // Test Case 4: 60 degree plane (tilted around Y-axis)
        var angle60 = Math.PI / 3; // 60 degrees
        var plane60 = KoreXYZPlane.MakePlane(
            new KoreXYZVector(0, 0, 0),                                    // Origin
            new KoreXYZVector(Math.Sin(angle60), 0, Math.Cos(angle60)),   // Normal tilted 60° around Y
            new KoreXYZVector(0, 1, 0)                                     // Y-axis stays as world Y
        );

        DrawPlaneWithCircle(imagePlotter, plane60, "60° Plane (tilted around Y-axis)",
            new KoreXYVector(200, 600), new KoreXYVector(120, 520), 150);

        // Test Case 5: 90 degree plane (YZ plane)
        var plane90 = KoreXYZPlane.MakePlane(
            new KoreXYZVector(0, 0, 0),     // Origin
            new KoreXYZVector(1, 0, 0),     // Normal (X-axis)
            new KoreXYZVector(0, 1, 0)      // Y-axis (world Y)
        );

        DrawPlaneWithCircle(imagePlotter, plane90, "90° Plane (YZ)",
            new KoreXYVector(600, 600), new KoreXYVector(520, 520), 150);

        // Test Case 6: Complex plane (tilted around both X and Y axes)
        var planeComplex = KoreXYZPlane.MakePlane(
            new KoreXYZVector(0, 0, 0),                        // Origin
            new KoreXYZVector(0.5, 0.5, Math.Sqrt(0.5)),       // Complex normal
            new KoreXYZVector(0, 0.5, 0.5)                     // Y-axis (world Y)
        );

        DrawPlaneWithCircle(imagePlotter, planeComplex, "Complex Plane",
            new KoreXYVector(1000, 600), new KoreXYVector(920, 520), 150);

        // Save the test image
        string filePath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "plane_test.png");
        imagePlotter.Save(filePath);

        testLog.AddResult("Plane test", true, "Test completed - check " + filePath);
    }

    // Helper method to draw a circle on a 3D plane projected to 2D
    private static void DrawPlaneWithCircle(KoreSkiaSharpPlotter plotter, KoreXYZPlane plane, string label,
        KoreXYVector screenCenter, KoreXYVector labelPos, double radius)
    {
        const int segments = 32; // Number of points around the circle
        var circlePoints2D = new List<KoreXYVector>();
        var circlePoints3D = new List<KoreXYZVector>();

        // Create circle points on the plane (in plane's 2D coordinate system)
        for (int i = 0; i < segments; i++)
        {
            double angle = i * 2 * Math.PI / segments;
            var planePoint2D = new KoreXYVector(
                radius * Math.Cos(angle),
                radius * Math.Sin(angle)
            );

            // Project from plane 2D to world 3D using the pure math class
            var worldPoint3D = plane.Project2DTo3D(planePoint2D);
            circlePoints3D.Add(worldPoint3D);

            // Project 3D world coordinates to 2D screen coordinates (visualization layer)
            var screenPoint = ProjectWorldToScreen(worldPoint3D, screenCenter);
            circlePoints2D.Add(screenPoint);
        }

        // Draw the circle outline
        plotter.DrawSettings.Color = SKColors.Blue;
        plotter.DrawSettings.LineWidth = 2;
        plotter.DrawSettings.IsAntialias = true;

        for (int i = 0; i < segments; i++)
        {
            int nextIndex = (i + 1) % segments;
            plotter.DrawLine(circlePoints2D[i], circlePoints2D[nextIndex]);
        }

        // Draw plane axes for reference
        DrawPlaneAxes(plotter, plane, screenCenter, radius);

        // Mark some key points
        plotter.DrawSettings.Color = SKColors.Red;
        plotter.DrawPoint(screenCenter, 4); // Center

        // Mark a few circle points
        plotter.DrawSettings.Color = SKColors.Green;
        for (int i = 0; i < segments; i += 8) // Every 8th point
        {
            plotter.DrawPoint(circlePoints2D[i], 3);
        }

        // Add label
        plotter.DrawSettings.Color = SKColors.Black;
        plotter.DrawText(label, labelPos, 16);

        // Add normal vector info
        var normal = plane.VecNormal;
        string normalText = $"Normal: ({normal.X:F2}, {normal.Y:F2}, {normal.Z:F2})";
        plotter.DrawText(normalText, labelPos.Offset(0, 20), 12);
    }

    // Helper method to draw plane axes for reference
    private static void DrawPlaneAxes(KoreSkiaSharpPlotter plotter, KoreXYZPlane plane, KoreXYVector screenCenter, double axisLength)
    {
        // Get 3D points for main axes using pure math class
        var xAxis3D = plane.Project2DTo3D(new KoreXYVector(axisLength, 0));
        var yAxis3D = plane.Project2DTo3D(new KoreXYVector(0, axisLength));

        // Project to screen coordinates (visualization layer)
        var xAxisScreen = ProjectWorldToScreen(xAxis3D, screenCenter);
        var yAxisScreen = ProjectWorldToScreen(yAxis3D, screenCenter);

        // Draw X axis (red) - 0 degrees
        plotter.DrawSettings.Color = SKColors.Red;
        plotter.DrawSettings.LineWidth = 2;
        plotter.DrawLine(screenCenter, xAxisScreen);

        // Draw Y axis (green) - 90 degrees
        plotter.DrawSettings.Color = SKColors.Green;
        plotter.DrawSettings.LineWidth = 2;
        plotter.DrawLine(screenCenter, yAxisScreen);

        // Draw intermediate angle lines: 30° and 60°
        // 30 degree line (orange)
        var angle30 = Math.PI / 6; // 30 degrees
        var axis30_3D = plane.Project2DTo3D(new KoreXYVector(
            axisLength * Math.Cos(angle30),
            axisLength * Math.Sin(angle30)
        ));
        var axis30Screen = ProjectWorldToScreen(axis30_3D, screenCenter);

        plotter.DrawSettings.Color = SKColors.Orange;
        plotter.DrawSettings.LineWidth = 1;
        plotter.DrawLine(screenCenter, axis30Screen);

        // 60 degree line (purple)
        var angle60 = Math.PI / 3; // 60 degrees
        var axis60_3D = plane.Project2DTo3D(new KoreXYVector(
            axisLength * Math.Cos(angle60),
            axisLength * Math.Sin(angle60)
        ));
        var axis60Screen = ProjectWorldToScreen(axis60_3D, screenCenter);

        plotter.DrawSettings.Color = SKColors.Purple;
        plotter.DrawSettings.LineWidth = 1;
        plotter.DrawLine(screenCenter, axis60Screen);

        // Add axis labels
        plotter.DrawSettings.Color = SKColors.Red;
        plotter.DrawText("X (0°)", xAxisScreen.Offset(5, 0), 12);

        plotter.DrawSettings.Color = SKColors.Orange;
        plotter.DrawText("30°", axis30Screen.Offset(5, 0), 10);

        plotter.DrawSettings.Color = SKColors.Purple;
        plotter.DrawText("60°", axis60Screen.Offset(5, 0), 10);

        plotter.DrawSettings.Color = SKColors.Green;
        plotter.DrawText("Y (90°)", yAxisScreen.Offset(5, 0), 12);
    }

    // Screen projection logic - handles conversion from 3D world coordinates to 2D screen coordinates
    // This is where we define our viewing convention and handle screen coordinate system differences
    private static KoreXYVector ProjectWorldToScreen(KoreXYZVector worldPoint, KoreXYVector screenCenter)
    {
        // Simple orthographic projection with user-friendly coordinate convention:
        // - 3D +X maps to screen +X (right-positive) - FIXED: was going left, now goes right
        // - 3D +Y maps to screen -Y (up-positive, compensating for screen Y-down convention)
        // - 3D +Z is ignored (orthographic projection)

        // Add slight perspective effect by scaling based on Z depth
        double scale = 1.0 / (1.0 + worldPoint.Z * 0.001);

        return new KoreXYVector(
            screenCenter.X - worldPoint.X * scale,   // FIXED: negate X to make +X go right instead of left
            screenCenter.Y - worldPoint.Y * scale    // +Y goes up (flip Y for screen coordinates)
        );
    }
}
