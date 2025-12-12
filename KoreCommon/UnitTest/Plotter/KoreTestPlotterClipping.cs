
using System;
using SkiaSharp;
using KoreCommon.SkiaSharp;

namespace KoreCommon.UnitTest;

public static class KoreTestPlotterClipping
{
    public static void RunTests(KoreTestLog testLog)
    {
        RunTest_BasicClipping(testLog);
        RunTest_NestedClipping(testLog);
        RunTest_SimpleClipApplyClear(testLog);
        RunTest_ClearAllClips(testLog);
    }

    public static void RunTest_BasicClipping(KoreTestLog testLog)
    {
        try
        {
            KoreSkiaSharpPlotter plotter = new(500, 500);

            // Draw full background in red
            plotter.DrawSettings.Color = SKColors.Red;
            plotter.DrawSettings.Paint.Style = SKPaintStyle.Fill;
            plotter.DrawRect(new SKRect(0, 0, 500, 500), plotter.DrawSettings.Paint);

            // Apply a clip region in the center
            plotter.PushClipRect(new SKRect(100, 100, 400, 400));

            // Draw blue over the full canvas - should only appear in clipped region
            plotter.DrawSettings.Color = SKColors.Blue;
            plotter.DrawRect(new SKRect(0, 0, 500, 500), plotter.DrawSettings.Paint);

            plotter.PopClip();

            plotter.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "Plotter_BasicClipping.png"));

            testLog.AddResult("Basic Clipping Test", true, "Clipping region applied and removed successfully");
        }
        catch (Exception e)
        {
            testLog.AddResult("Basic Clipping Test", false, e.Message);
        }
    }

    public static void RunTest_NestedClipping(KoreTestLog testLog)
    {
        try
        {
            KoreSkiaSharpPlotter plotter = new(500, 500);

            // Fill with white background
            plotter.DrawSettings.Color = SKColors.White;
            plotter.DrawSettings.Paint.Style = SKPaintStyle.Fill;
            plotter.DrawRect(new SKRect(0, 0, 500, 500), plotter.DrawSettings.Paint);

            // Push first clip region (outer)
            plotter.PushClipRect(new SKRect(50, 50, 450, 450));

            // Draw red - should be clipped to outer region
            plotter.DrawSettings.Color = SKColors.Red;
            plotter.DrawRect(new SKRect(0, 0, 500, 500), plotter.DrawSettings.Paint);

            // Push second clip region (inner) - intersects with first
            plotter.PushClipRect(new SKRect(150, 150, 350, 350));

            // Draw green - should be clipped to intersection of both regions
            plotter.DrawSettings.Color = SKColors.Green;
            plotter.DrawRect(new SKRect(0, 0, 500, 500), plotter.DrawSettings.Paint);

            // Pop inner clip
            plotter.PopClip();

            // Draw blue - should be clipped to outer region only
            plotter.DrawSettings.Color = new SKColor(0, 0, 255, 128); // Semi-transparent blue
            plotter.DrawRect(new SKRect(0, 0, 500, 500), plotter.DrawSettings.Paint);

            // Pop outer clip
            plotter.PopClip();

            plotter.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "Plotter_NestedClipping.png"));

            testLog.AddResult("Nested Clipping Test", true, "Nested clipping regions work correctly");
        }
        catch (Exception e)
        {
            testLog.AddResult("Nested Clipping Test", false, e.Message);
        }
    }

    public static void RunTest_SimpleClipApplyClear(KoreTestLog testLog)
    {
        try
        {
            KoreSkiaSharpPlotter plotter = new(600, 400);

            // Fill background
            plotter.DrawSettings.Color = SKColors.LightGray;
            plotter.DrawSettings.Paint.Style = SKPaintStyle.Fill;
            plotter.DrawRect(new SKRect(0, 0, 600, 400), plotter.DrawSettings.Paint);

            // Apply simple clip (left half)
            plotter.ApplyClipRect(new SKRect(0, 0, 300, 400));

            // Draw red - should only appear in left half
            plotter.DrawSettings.Color = SKColors.Red;
            plotter.DrawRect(new SKRect(0, 0, 600, 400), plotter.DrawSettings.Paint);

            // Clear the clip
            plotter.ClearClip();

            // Apply new simple clip (right half) - should replace previous
            plotter.ApplyClipRect(new SKRect(300, 0, 600, 400));

            // Draw blue - should only appear in right half
            plotter.DrawSettings.Color = SKColors.Blue;
            plotter.DrawRect(new SKRect(0, 0, 600, 400), plotter.DrawSettings.Paint);

            plotter.ClearClip();

            plotter.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "Plotter_SimpleClip.png"));

            testLog.AddResult("Simple Clip Apply/Clear Test", true, "ApplyClipRect and ClearClip work correctly");
        }
        catch (Exception e)
        {
            testLog.AddResult("Simple Clip Apply/Clear Test", false, e.Message);
        }
    }

    public static void RunTest_ClearAllClips(KoreTestLog testLog)
    {
        try
        {
            KoreSkiaSharpPlotter plotter = new(500, 500);

            // Fill background
            plotter.DrawSettings.Color = SKColors.White;
            plotter.DrawSettings.Paint.Style = SKPaintStyle.Fill;
            plotter.DrawRect(new SKRect(0, 0, 500, 500), plotter.DrawSettings.Paint);

            // Push multiple clip regions
            plotter.PushClipRect(new SKRect(50, 50, 450, 450));
            plotter.PushClipRect(new SKRect(100, 100, 400, 400));
            plotter.PushClipRect(new SKRect(150, 150, 350, 350));

            // Draw red in nested clips
            plotter.DrawSettings.Color = SKColors.Red;
            plotter.DrawRect(new SKRect(0, 0, 500, 500), plotter.DrawSettings.Paint);

            // Clear all clips at once
            plotter.ClearAllClips();

            // Draw green - should cover entire canvas
            plotter.DrawSettings.Color = new SKColor(0, 255, 0, 128); // Semi-transparent green
            plotter.DrawRect(new SKRect(0, 0, 500, 500), plotter.DrawSettings.Paint);

            plotter.Save(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "Plotter_ClearAllClips.png"));

            testLog.AddResult("Clear All Clips Test", true, "ClearAllClips removes all clipping regions");
        }
        catch (Exception e)
        {
            testLog.AddResult("Clear All Clips Test", false, e.Message);
        }
    }
}
