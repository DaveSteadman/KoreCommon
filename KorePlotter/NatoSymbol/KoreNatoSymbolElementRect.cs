using SkiaSharp;

#nullable enable

namespace KorePlotter.NatoSymbol;

// NatoSymbolElement - Manages the drawing of an element, with the draw info in the parent abstract
// class, and the geometry in concrete child classes
public class KoreNatoSymbolElementRect : KoreNatoSymbolElement
{
    public float Width  { get; set; }
    public float Height { get; set; }

    public SKPoint[] Points { get; set;  } = null!;

    // ----------------------------------------------------------------------------------------

    // Width & Height are total dimensions

    public void CalcPoints(SKPoint centrePos, float width, float height)
    {
        Width = width;
        Height = height;

        Points = new SKPoint[4];
        Points[0] = new SKPoint(centrePos.X - Width / 2, centrePos.Y - Height / 2);
        Points[1] = new SKPoint(centrePos.X + Width / 2, centrePos.Y - Height / 2);
        Points[2] = new SKPoint(centrePos.X + Width / 2, centrePos.Y + Height / 2);
        Points[3] = new SKPoint(centrePos.X - Width / 2, centrePos.Y + Height / 2);
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