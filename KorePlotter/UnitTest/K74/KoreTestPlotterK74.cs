
using System;
using System.Collections.Generic;

using SkiaSharp;
using KoreCommon.SkiaSharp;
using KoreCommon.PlotterK74;

namespace KoreCommon.UnitTest;


public static class KoreTestPlotterK74
{
    public static void RunAllTests(KoreTestLog testLog)
    {
        KoreTestPlotterK74.BasicGridTest(testLog);
    }

    // --------------------------------------------------------------------------------------------

    public static void BasicGridTest(KoreTestLog testLog)
    {
        bool testPassed = true;
        string testName = "KorePlotterK74.BasicGridTest";

        // Define the output file path
        string outputFilePath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "KorePlotterK74_BasicGridTest.png");

        try
        {
            int widthCells = 10;
            int heightCells = 10;
            int cellSizePixels = 20;

            KorePlotterK74 plotter = new KorePlotterK74(widthCells, heightCells, cellSizePixels);

            // Fill each cell with a different color
            for (int x = 0; x < widthCells; x++)
            {
                for (int y = 0; y < heightCells; y++)
                {
                    SKColor color = SKColor.FromHsv((x * 36) % 360, 100, (y + 1) * 10);
                    plotter.FillCell(x, y, color);
                }
            }

            plotter.DrawCellRect(1, 1, widthCells - 2, heightCells - 2, SKColors.White);

            // Clip to a rectangle area
            plotter.ClipRectForCells(2, 2, 5, 5);

            // draw an arc in the clipped area
            plotter.DrawArcAtCell(
                3, 3, KoreXYRectPosition.Center,
                2, 3,
                30, 30,
                SKColors.Red);
            plotter.DrawArcAtCell(
                3, 3, KoreXYRectPosition.Center,
                3, 4,
                0, 30,
                SKColors.Black);

            // Draw a set of arcs to cover teh full 360 degrees, in increments
            int numIncrements = 12;
            float angleIncrement = 360f / numIncrements;
            float colourIncrement = 1f / numIncrements;

            plotter.ClearAllClips();

            for (int i = 0; i < numIncrements; i++)
            {
                float colourVal = i * colourIncrement;

                KoreColorRGB c = new KoreColorRGB(1f - colourVal, 0f, colourVal);
                SKColor color = KoreSkiaSharpColorOps.ColorFromKoreColor(c);

                plotter.DrawSettings.IsAntialias = true;

                float startAngle = i * angleIncrement;
                plotter.DrawArcAtCell(
                    3, 3, KoreXYRectPosition.Center,
                    1, 2,
                    startAngle, angleIncrement,
                    color);

            }


            // Optionally save the bitmap to a file for visual verification
            using (var image = SKImage.FromBitmap(plotter.KorePlotter.GetBitmap()))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = System.IO.File.OpenWrite(outputFilePath))
            {
                data.SaveTo(stream);
            }
        }
        catch (Exception ex)
        {
            testPassed = false;
            string message = $"Exception during {testName}: {ex.Message}";
            testLog.AddResult(testName, false, message);
        }

        if (testPassed)
        {
            string message = $"Test completed successfully: {outputFilePath}";
            testLog.AddResult(testName, true, message);
        }
    }
}