// <fileheader>

using SkiaSharp;
using System;
using System.IO;
using System.Collections.Generic;

namespace KoreCommon.SkiaSharp;

// KoreSkiaSharpPlotter: 2D plotting functionality using SkiaSharp
// - Uses SK types exclusively for performance and compatibility
// - See KoreSkiaSharpPlotter.Interface.cs for Kore type conversions

public partial class KoreSkiaSharpPlotter
{
    // --------------------------------------------------------------------------------------------
    // MARK: Text
    // --------------------------------------------------------------------------------------------

    public void DrawText(string text, SKPoint position, float fontSize = 12)
    {
        using var font = new SKFont(SKTypeface.Default, fontSize);
        using var textPaint = new SKPaint
        {
            Color = DrawSettings.Paint.Color,
            IsAntialias = true
        };

        canvas.DrawText(text, position.X, position.Y, font, textPaint);
    }

    public void DrawText(string text, SKPoint position, SKFont font, SKPaint textPaint)
    {
        canvas.DrawText(text, position.X, position.Y, font, textPaint);
    }

    public void DrawTextCentered(string text, SKPoint centerPosition, float fontSize = 12)
    {
        using var font = new SKFont(SKTypeface.Default, fontSize);
        using var textPaint = new SKPaint
        {
            Color = DrawSettings.Paint.Color,
            IsAntialias = true
        };

        // Measure text to center it both horizontally and vertically
        var textWidth = font.MeasureText(text);

        // Get font metrics to properly center vertically
        var fontMetrics = font.Metrics;
        var textHeight = fontMetrics.Descent - fontMetrics.Ascent;

        // Calculate centered position
        var centeredX = centerPosition.X - (textWidth / 2);
        var centeredY = centerPosition.Y - (textHeight / 2) - fontMetrics.Ascent;

        canvas.DrawText(text, centeredX, centeredY, font, textPaint);
    }

    // --------------------------------------------------------------------------------------------

    public SKRect RectForTextAtPosition(string text, KoreXYVector pos, KoreXYRectPosition anchorPos, int fontSize = 12)
    {
        using var font = new SKFont(SKTypeface.Default, fontSize);

        var fontMetrics = font.Metrics;

        // Measure text dimensions
        float textWidth = font.MeasureText(text);
        float textHeight = fontMetrics.Descent - fontMetrics.Ascent;

        // Calculate anchor point positions relative to the text bounds
        // Note: SkiaSharp draws text from the baseline, so we need to account for that
        float leftX = (float)pos.X;
        float centerX = (float)pos.X - textWidth / 2f;
        float rightX = (float)pos.X - textWidth;

        float topY = (float)pos.Y - fontMetrics.Ascent;  // Top of text
        float centerY = (float)pos.Y - (textHeight / 2f) - fontMetrics.Ascent;  // Center of text
        float bottomY = (float)pos.Y - fontMetrics.Descent;  // Bottom of text

        float drawX, drawY;

        switch (anchorPos)
        {
            case KoreXYRectPosition.TopLeft:
                drawX = leftX;
                drawY = topY;
                break;
            case KoreXYRectPosition.TopCenter:
                drawX = centerX;
                drawY = topY;
                break;
            case KoreXYRectPosition.TopRight:
                drawX = rightX;
                drawY = topY;
                break;
            case KoreXYRectPosition.LeftCenter:
                drawX = leftX;
                drawY = centerY;
                break;
            case KoreXYRectPosition.Center:
                drawX = centerX;
                drawY = centerY;
                break;
            case KoreXYRectPosition.RightCenter:
                drawX = rightX;
                drawY = centerY;
                break;
            case KoreXYRectPosition.BottomLeft:
                drawX = leftX;
                drawY = bottomY;
                break;
            case KoreXYRectPosition.BottomCenter:
                drawX = centerX;
                drawY = bottomY;
                break;
            case KoreXYRectPosition.BottomRight:
                drawX = rightX;
                drawY = bottomY;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(anchorPos), anchorPos, null);
        }

        // Calculate bounds from the draw position (baseline)
        float boundsLeft = drawX;
        float boundsTop = drawY + fontMetrics.Ascent;
        float boundsRight = drawX + textWidth;
        float boundsBottom = drawY + fontMetrics.Descent;

        return new SKRect(boundsLeft, boundsTop, boundsRight, boundsBottom);
    }

    public void DrawTextAtPosition(string text, KoreXYVector pos, KoreXYRectPosition anchorPos, int fontSize = 12)
    {
        using var font = new SKFont(SKTypeface.Default, fontSize);
        using var textPaint = new SKPaint
        {
            Color = DrawSettings.Paint.Color,
            IsAntialias = true
        };

        var fontMetrics = font.Metrics;

        // Measure text dimensions
        float textWidth = font.MeasureText(text);
        float textHeight = fontMetrics.Descent - fontMetrics.Ascent;

        // Calculate anchor point positions relative to the text bounds
        // Note: SkiaSharp draws text from the baseline, so we need to account for that
        float leftX = (float)pos.X;
        float centerX = (float)pos.X - textWidth / 2f;
        float rightX = (float)pos.X - textWidth;

        float topY = (float)pos.Y - fontMetrics.Ascent;  // Top of text
        float centerY = (float)pos.Y - (textHeight / 2f) - fontMetrics.Ascent;  // Center of text
        float bottomY = (float)pos.Y - fontMetrics.Descent;  // Bottom of text

        SKPoint drawPosition = anchorPos switch
        {
            KoreXYRectPosition.TopLeft      => new SKPoint(leftX, topY),
            KoreXYRectPosition.TopCenter    => new SKPoint(centerX, topY),
            KoreXYRectPosition.TopRight     => new SKPoint(rightX, topY),
            KoreXYRectPosition.LeftCenter   => new SKPoint(leftX, centerY),
            KoreXYRectPosition.Center       => new SKPoint(centerX, centerY),
            KoreXYRectPosition.RightCenter  => new SKPoint(rightX, centerY),
            KoreXYRectPosition.BottomLeft   => new SKPoint(leftX, bottomY),
            KoreXYRectPosition.BottomCenter => new SKPoint(centerX, bottomY),
            KoreXYRectPosition.BottomRight  => new SKPoint(rightX, bottomY),
            _ => throw new ArgumentOutOfRangeException(nameof(anchorPos), anchorPos, null)
        };

        canvas.DrawText(text, drawPosition.X, drawPosition.Y, font, textPaint);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Font
    // --------------------------------------------------------------------------------------------

    // Load a font from a file path
    public SKTypeface LoadFontFromFile(string fontFilePath)
    {
        if (!File.Exists(fontFilePath))
            throw new FileNotFoundException("Font file not found.", fontFilePath);

        return SKTypeface.FromFile(fontFilePath);
    }

    // Create a font with specified size from a typeface
    // Note: - SKFont is lightweight; dispose after use
    //       - Font size unit is in pixels
    public SKFont CreateFont(SKTypeface typeface, float fontSizePixels)
    {
        return new SKFont(typeface, fontSizePixels);
    }

    // Delete a font (requires explicit disposal in SkiaSharp)
    public void DeleteFont(SKFont font)
    {
        font.Dispose();
    }
}
