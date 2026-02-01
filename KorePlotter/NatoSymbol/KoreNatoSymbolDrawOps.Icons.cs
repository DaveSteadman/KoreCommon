using System.Collections;
using SkiaSharp;

namespace KorePlotter.NatoSymbol;

// KoreNatoSymbolDrawOps: Static methods to draw specific shapes for NATO symbols.
// - Functions passed the canvas and the necessary parameters to draw the shape.
// - Static class for utility functions, holds no state.

public static partial class KoreNatoSymbolDrawOps
{
    public static void DrawRotary(KoreNatoSymbolCanvas canvas, DrawMode drawMode = DrawMode.FillAndStroke)
    {
        // Draw a rotary symbol - a circle with an octagon inside

        // Draw octagon
        DrawOctagon(canvas, drawMode);

        // Draw circle around octagon
        using SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = KoreNatoSymbolColorPalette.Colors["OffWhiteGrey"],
            IsAntialias = true
        };

        using SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = KoreNatoSymbolColorPalette.Colors["MidGrey"],
            StrokeWidth = StrokeWidthForCanvas(canvas),
            IsAntialias = true
        };

        // draw a cirlce around the octagon
        SKPoint center = canvas.LPoint(0f, 0f);
        float radius = canvas.LDistance * 0.5f;
        if (drawMode == DrawMode.Fill || drawMode == DrawMode.FillAndStroke)
            canvas.Canvas.DrawCircle(center, radius, fillPaint);
        if (drawMode == DrawMode.Stroke || drawMode == DrawMode.FillAndStroke)
            canvas.Canvas.DrawCircle(center, radius, paint);
    }

}


