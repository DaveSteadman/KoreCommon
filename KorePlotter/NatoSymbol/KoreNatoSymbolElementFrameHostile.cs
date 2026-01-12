using SkiaSharp;

#nullable enable

namespace KorePlotter.NatoSymbol;

// NatoSymbolElement - Manages the drawing of an element, with the draw info in the parent abstract
// class, and the geometry in concrete child classes
public class KoreNatoSymbolElementFrameHostile : KoreNatoSymbolElement
{
    public SKPoint[] Points { get; set;  } = null!;

    // ----------------------------------------------------------------------------------------

    // Width & Height are total dimensions

    public void CalcPoints(KoreNatoSymbolLayout layout, NatoSymbolAffiliation affiliation)
    {
        SKPoint centrePos = layout.Center;
        float lDistance = layout.LDistance;



    }

    // Get the rectangle path for advanced operations
    public static SKPath DefineRectPath(SKPoint[] rectPoints)
    {
        SKPath path = new SKPath();
        path.MoveTo(rectPoints[0]);
        for (int i = 1; i < rectPoints.Length; i++)
        {
            path.LineTo(rectPoints[i]);
        }
        path.Close();
        return path;
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

        SKPath rectPath = DefineRectPath(Points);
        canvas.DrawPath(rectPath, defaultPaint);

    }

}