using SkiaSharp;

namespace KorePlotter.NatoSymbol;

// NatoSymbolElement - Manages the drawing of an element, with the draw info in the parent abstract
// class, and the geometry in concrete child classes
public class KoreNatoSymbolElement
{
    public SKColor FillColor   { get; } = KoreNatoSymbolColorPalette.Black;
    public SKColor StrokeColor { get; } = KoreNatoSymbolColorPalette.White;
    public float   StrokeWidth { get; } = 5f;
    public bool    IsFilled    { get; } = true;

    // ----------------------------------------------------------------------------------------

    // Draw all elements to the canvas
    public virtual void Render(SKCanvas canvas)
    {
        using var defaultPaint = new SKPaint
        {
            IsAntialias = true,
            Color       = FillColor,
            StrokeWidth = StrokeWidth,
            Style       = IsFilled ? SKPaintStyle.Fill : SKPaintStyle.Stroke
        };
    }
}