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
    // KoreTestSkiaSharp.RunTests(testLog)
    public static void RunTests(KoreTestLog testLog)
    {
        TestBasicImage(testLog);
        TestCircumcircle(testLog);
        TestPlane(testLog);
        TestImage(testLog);
    }

    // Draw a testcard image
    private static void TestBasicImage(KoreTestLog testLog)
    {
        // Create a new image with a testcard pattern
        var imagePlotter = new KoreSkiaSharpPlotter(1000, 1000);

        // Draw a boundary
        KoreXYRect boundsRect = new KoreXYRect(0, 0, 1000, 1000);
        KoreXYRect boundsRectInset = boundsRect.Inset(5);

        SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            Color = SKColors.Black,
            IsAntialias = false // Match the line's anti-aliasing setting
        };
        imagePlotter.DrawRect(boundsRectInset, fillPaint);

        // Test Lines - Various line widths and colors
        int xStart = 10;
        int yStart = 10;
        int yEnd = 100;

        KoreXYVector startPnt = new KoreXYVector(xStart, yStart);
        KoreXYVector endPnt = new KoreXYVector(xStart, yEnd);

        imagePlotter.DrawSettings.LineWidth = 1;
        imagePlotter.DrawSettings.Color = SKColors.Black;
        imagePlotter.DrawSettings.IsAntialias = false; // Disable anti-aliasing for crisp 1px lines
        imagePlotter.DrawLine(startPnt, endPnt);

        // Draw text in a specific box
        imagePlotter.DrawSettings.Color = SKColors.Red;
        KoreXYVector markPoint = new KoreXYVector(100, 50);
        imagePlotter.DrawPointAsCross(markPoint, 5);
        KoreXYVector markPoint2 = new KoreXYVector(markPoint.X, markPoint.Y + 30); // Move down for the next text
        imagePlotter.DrawPointAsCross(markPoint2, 5);
        KoreXYVector markPoint3 = new KoreXYVector(markPoint2.X, markPoint2.Y + 30); // Move down for the next text
        imagePlotter.DrawPointAsCross(markPoint3, 5);

        imagePlotter.DrawSettings.Color = SKColors.Black;
        string testText = "Test Text";
        imagePlotter.DrawText(testText, markPoint, 20);
        imagePlotter.DrawTextCentered("Another Line of Text", markPoint2, 20);

        imagePlotter.DrawTextAtPosition("Yet Another Line of Text", markPoint3, KoreXYRectPosition.Center, 20);

        // Save the image to a file
        string filePath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "testcard.png");
        KoreFileOps.CreateDirectoryForFile(filePath);

        imagePlotter.Save(filePath);
        testLog.AddComment("Test card image saved to " + filePath);
    }
}


