using System.Collections;
using SkiaSharp;

namespace KorePlotter.NatoSymbol;

// KoreNatoSymbolDrawOps: Static methods to draw specific shapes for NATO symbols.
// - Functions passed the canvas and the necessary parameters to draw the shape.
// - Static class for utility functions, holds no state.

public static partial class KoreNatoSymbolDrawOps
{
    // Draw the octagon shape - mainly for debug purposes
    // - Octagon is centered on canvas center, with a radius of 0.5L

    public static void DrawOctagon(KoreNatoSymbolCanvas canvas, DrawMode drawMode = DrawMode.FillAndStroke)
    {
        float s = (float)(1.0 / Math.Sqrt(2)); // sin(45°) = cos(45°) = √2/2

        // Define octagon points in L coordinates with a radius of 0.5L
        SKPoint[] octagonPoints = new SKPoint[]
        {
            canvas.LPoint( 0.5f,  0f),  // Right
            canvas.LPoint( s * 0.5f,  s * 0.5f),   // Top-Right
            canvas.LPoint( 0f,  0.5f),  // Top
            canvas.LPoint(-s * 0.5f,  s * 0.5f),   // Top-Left
            canvas.LPoint(-0.5f,  0f),  // Left
            canvas.LPoint(-s * 0.5f, -s * 0.5f),   // Bottom-Left
            canvas.LPoint( 0f, -0.5f),  // Bottom
            canvas.LPoint( s * 0.5f, -s * 0.5f)    // Bottom-Right
        };

        // Create path for octagon
        SKPath path = new SKPath();
        path.MoveTo(octagonPoints[0]);
        for (int i = 1; i < octagonPoints.Length; i++)
        {
            path.LineTo(octagonPoints[i]);
        }
        path.Close();

        // Draw filled octagon
        using SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = KoreNatoSymbolColorPalette.Colors["OffWhiteGrey"],
            IsAntialias = true
        };
        if (drawMode == DrawMode.Fill || drawMode == DrawMode.FillAndStroke)
            canvas.Canvas.DrawPath(path, fillPaint);

        // Draw octagon border
        using SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = KoreNatoSymbolColorPalette.Colors["MidGrey"],
            StrokeWidth = StrokeWidthForCanvas(canvas),
            IsAntialias = true
        };
        if (drawMode == DrawMode.Stroke || drawMode == DrawMode.FillAndStroke)
            canvas.Canvas.DrawPath(path, paint);
    }


    // ----------------------------------------------------------------------------------------

    public static SKRect BoundingRectFromPoints(SKPoint[] points)
    {
        if (points == null || points.Length == 0)
            throw new ArgumentException("Points array is null or empty.");

        float minX = points[0].X;
        float minY = points[0].Y;
        float maxX = points[0].X;
        float maxY = points[0].Y;

        foreach (var point in points)
        {
            if (point.X < minX) minX = point.X;
            if (point.Y < minY) minY = point.Y;
            if (point.X > maxX) maxX = point.X;
            if (point.Y > maxY) maxY = point.Y;
        }

        return new SKRect(minX, minY, maxX, maxY);
    }

    // ----------------------------------------------------------------------------------------
    // MARK: STROKE
    // ----------------------------------------------------------------------------------------

    public static float StrokeWidthForCanvas(KoreNatoSymbolCanvas canvas)
    {
        return canvas.LDistance * 0.035f; // 3% of L distance
    }

    // Draws the largest possible text centered inside a rect.
    public static void TextInRect(
        KoreNatoSymbolCanvas canvas,
        SKRect bounds,
        string text,
        SKColor? color = null,
        float paddingFactor = 0.9f,
        SKTypeface? typeface = null)
    {
        if (string.IsNullOrEmpty(text))
            return;

        paddingFactor = Math.Clamp(paddingFactor, 0.01f, 1.0f);

        float testFontSize = 100f;
        using var testFont = new SKFont(typeface ?? SKTypeface.Default, testFontSize);

        float textWidth = testFont.MeasureText(text);
        var fontMetrics = testFont.Metrics;
        float textHeight = fontMetrics.Descent - fontMetrics.Ascent;

        if (textWidth <= 0f || textHeight <= 0f)
            return;

        float scaleX = (bounds.Width * paddingFactor) / textWidth;
        float scaleY = (bounds.Height * paddingFactor) / textHeight;
        float scale = Math.Min(scaleX, scaleY);
        float finalFontSize = testFontSize * scale;

        using var font = new SKFont(typeface ?? SKTypeface.Default, finalFontSize);
        using var textPaint = new SKPaint
        {
            Color = color ?? SKColors.Black,
            IsAntialias = true
        };

        textWidth = font.MeasureText(text);
        fontMetrics = font.Metrics;
        textHeight = fontMetrics.Descent - fontMetrics.Ascent;

        float centeredX = bounds.MidX - (textWidth / 2f);
        float centeredY = bounds.MidY - (textHeight / 2f) - fontMetrics.Ascent;

        canvas.Canvas.DrawText(text, centeredX, centeredY, font, textPaint);
    }


}

