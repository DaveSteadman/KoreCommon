// <fileheader>

using SkiaSharp;
using System;
using System.IO;
using System.Collections.Generic;

namespace KoreCommon.SkiaSharp;

// KoreSkiaSharpPlotter: 2D plotting functionality using SkiaSharp
// - Based on FssPlotter from TechDemo-SimpleMeshDecimation
// - Uses SK types exclusively for performance and compatibility
// - See KoreSkiaSharpPlotter.Interface.cs for Kore type conversions

// Contains the paint settings and provides some level of abstraction in case we ever use a different plotter to SkiaSharp
public class KoreSkiaSharpPlotterDrawSettings
{
    public SKPaint Paint { get; } = new SKPaint(); // This is a cache of the last paint object created, so we can reuse it if the settings haven't changed
    public SKFont Font { get; } = new SKFont();

    public float LineWidth
    {
        get { return Paint.StrokeWidth; }
        set { Paint.StrokeWidth = value; }
    }

    public SKColor Color
    {
        get { return Paint.Color; }
        set { Paint.Color = value; }
    }

    public bool IsAntialias
    {
        get { return Paint.IsAntialias; }
        set { Paint.IsAntialias = value; }
    }

    public float TextSize
    {
        get { return Font.Size; }
        set { Font.Size = value; }
    }

    public float LineSpacing { get; set; } = 0;
    public float PointCrossSize { get; set; } = 3;

    public KoreSkiaSharpPlotterDrawSettings()
    {
        ResetToDefaults();
    }

    // Usage: KoreSkiaSharpPlotterDrawSettings settings = new KoreSkiaSharpPlotterDrawSettings();
    //         settings.ResetToDefaults();
    public void ResetToDefaults()
    {
        try
        {
            Paint.StrokeWidth = 1;
            Paint.Color = SKColors.Black;
            Paint.Style = SKPaintStyle.Stroke;
            Paint.IsAntialias = true;

            LineSpacing = 0;
            PointCrossSize = 3;
        }
        catch (Exception ex)
        {
            KoreCentralLog.AddEntry($"Error setting default paint settings: {ex.Message}");
        }
    }

    // Create a new object configured to fill shapes
    public static KoreSkiaSharpPlotterDrawSettings FillSettings()
    {
        KoreSkiaSharpPlotterDrawSettings settings = new();
        settings.Paint.Style = SKPaintStyle.Fill;
        settings.Paint.StrokeWidth = 0;

        return settings;
    }

    // Read the current SKPaint settings, so they can be applied back later

    public KoreSkiaSharpPlotterDrawSettings AbsorbCurrentSettings()
    {
        KoreSkiaSharpPlotterDrawSettings newSettings = new KoreSkiaSharpPlotterDrawSettings();

        newSettings.Paint.StrokeWidth = this.Paint.StrokeWidth;
        newSettings.Paint.Color = this.Paint.Color;
        newSettings.Paint.Style = this.Paint.Style;
        newSettings.Paint.IsAntialias = this.Paint.IsAntialias;

        newSettings.LineSpacing = this.LineSpacing;
        newSettings.PointCrossSize = this.PointCrossSize;

        return newSettings;
    }
}
