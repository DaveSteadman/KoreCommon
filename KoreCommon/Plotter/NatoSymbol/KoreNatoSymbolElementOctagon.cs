using System;

using SkiaSharp;

#nullable enable

namespace KoreCommon.Plotter.NatoSymbolGen;

// NatoSymbolElement - Manages the drawing of an element, with the draw info in the parent abstract
// class, and the geometry in concrete child classes
public class KoreNatoSymbolElementOctagon : KoreNatoSymbolElement
{
    public SKPoint[] Points { get; set; } = null!;
    public SKPath Path { get; set; } = null!;

    // ----------------------------------------------------------------------------------------

    // Octagon shape points
    // [0] Right point
    // [1] Top-Right point
    // [2] Top point
    // [3] Top-Left point
    // [4] Left point
    // [5] Bottom-Left point
    // [6] Bottom point
    // [7] Bottom-Right point

    public void DefinePoints(KoreNatoSymbolLayout layout)
    {
        // Extract layout values
        SKPoint centrePos = layout.Center;
        float lDistance = layout.LDistance;

        // radius - octagon is 1L across
        float radius = lDistance / 2f;

        Points = new SKPoint[8];
        for (int i = 0; i < 8; i++)
        {
            double angleDeg =  i * 45.0;
            double angleRad = Math.PI / 180.0 * angleDeg;
            float x = centrePos.X + radius * (float)Math.Cos(angleRad);
            float y = centrePos.Y + radius * (float)Math.Sin(angleRad);
            Points[i] = new SKPoint(x, y);
        }

        Path = new SKPath();
        Path.MoveTo(Points[0]);
        for (int i = 1; i < Points.Length; i++)
        {
            Path.LineTo(Points[i]);
        }
        Path.Close();
    }

    // ----------------------------------------------------------------------------------------

    // Draw all elements to the canvas
    public override void Render(SKCanvas canvas)
    {
        using var defaultPaint = new SKPaint
        {
            IsAntialias = true,
            Color = StrokeColor,
            StrokeWidth = StrokeWidth,
            Style = SKPaintStyle.Stroke
        };

        SKPath octagonPath = Path;
        canvas.DrawPath(octagonPath, defaultPaint);

    }

}