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
        int imageWidth = 350;
        int imageHeight = 200;
        var imagePlotter = new KoreSkiaSharpPlotter(imageWidth, imageHeight);

        // Draw a boundary
        KoreXYRect boundsRect = new KoreXYRect(0, 0, imageWidth, imageHeight);
        KoreXYRect boundsRectInset = boundsRect.Inset(5);

        SKPaint fillPaint = new SKPaint
        {
            Style       = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            Color       = SKColors.Black,
            IsAntialias = false
        };
        imagePlotter.DrawRect(boundsRectInset, fillPaint);

        // Test all nine text positions with labels, boxes, and anchor points
        imagePlotter.DrawSettings.TextSize = 16f;
        imagePlotter.DrawSettings.Paint.StrokeWidth = 1;

        int marginEdge = 20;
        KoreNumeric1DArray<int> posX = new (new int[] { marginEdge, imageWidth / 2, imageWidth - marginEdge });
        KoreNumeric1DArray<int> posY = new (new int[] { marginEdge, imageHeight / 2, imageHeight - marginEdge });

        // Define positions spread across the image
        (KoreXYVector pos, KoreXYRectPosition anchor, string label)[] testPositions = new[]
        {
            (new KoreXYVector(posX[0], posY[0]),  KoreXYRectPosition.TopLeft,      "TopLeft"),
            (new KoreXYVector(posX[1], posY[0]),  KoreXYRectPosition.TopCenter,    "TopCenter"),
            (new KoreXYVector(posX[2], posY[0]),  KoreXYRectPosition.TopRight,     "TopRight"),
            (new KoreXYVector(posX[0], posY[1]),  KoreXYRectPosition.LeftCenter,   "LeftCenter"),
            (new KoreXYVector(posX[1], posY[1]),  KoreXYRectPosition.Center,       "Center"),
            (new KoreXYVector(posX[2], posY[1]),  KoreXYRectPosition.RightCenter,  "RightCenter"),
            (new KoreXYVector(posX[0], posY[2]),  KoreXYRectPosition.BottomLeft,   "BottomLeft"),
            (new KoreXYVector(posX[1], posY[2]),  KoreXYRectPosition.BottomCenter, "BottomCenter"),
            (new KoreXYVector(posX[2], posY[2]),  KoreXYRectPosition.BottomRight,  "BottomRight")
        };

        foreach (var (pos, anchor, label) in testPositions)
        {
            // Draw the anchor point in red
            imagePlotter.DrawSettings.Color = SKColors.Red;
            imagePlotter.DrawPointAsCross(pos, 5);

            // Draw the text at the position with the specified anchor
            imagePlotter.DrawSettings.Color = SKColors.Black;
            imagePlotter.DrawTextAtPosition(label, pos, anchor, 16);

            // Get the text bounds and draw a box around it in blue
            SKRect textBounds = imagePlotter.RectForTextAtPosition(label, pos, anchor, 16);
            imagePlotter.DrawSettings.Color = SKColors.Blue;
            imagePlotter.DrawSettings.Paint.Style = SKPaintStyle.Stroke;
            KoreXYRect textRect = new KoreXYRect(textBounds.Left, textBounds.Top, textBounds.Right, textBounds.Bottom);
            imagePlotter.DrawRect(textRect);
            imagePlotter.DrawSettings.Paint.Style = SKPaintStyle.Fill;
        }

        // Save the image to a file
        string filePath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "Plotter_textpos.png");
        KoreFileOps.CreateDirectoryForFile(filePath);

        imagePlotter.Save(filePath);
        testLog.AddComment("Test card image saved to " + filePath);
    }
}


