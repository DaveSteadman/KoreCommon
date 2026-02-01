using System;
using System.IO;

using SkiaSharp;

namespace KorePlotter.NatoSymbol;

// NatoSymbolCanvas - Manages the drawing surface and composition of symbol elements
public class KoreNatoSymbolCanvas
{
    // Arrangement of points and constructs to control draw areas
    // SkiaSharp drawing surface and canvas
    public SKSurface Surface { get; }
    public SKCanvas Canvas { get; }

    public float CanvasWidth { get; }
    public float CanvasHeight { get; }
    public SKPoint Center { get; }

    // L Distance
    public float LDistance => CanvasWidth * 0.5f;

    // ----------------------------------------------------------------------------------------

    public KoreNatoSymbolCanvas(float canvasSize = 1000f)
    {
        CanvasWidth  = canvasSize;
        CanvasHeight = canvasSize;
        Center       = new SKPoint(CanvasWidth / 2f, CanvasHeight / 2f);

        var imageInfo = new SKImageInfo(
            (int)canvasSize,
            (int)canvasSize,
            SKColorType.Rgba8888,
            SKAlphaType.Premul
        );

        Surface = SKSurface.Create(imageInfo);
        Canvas = Surface.Canvas;

        // Start with transparent background
        Canvas.Clear(SKColors.Transparent);
    }

    // ----------------------------------------------------------------------------------------

    public void Clear()
    {
        Canvas.Clear(SKColors.Transparent);
    }

    // ----------------------------------------------------------------------------------------

    public SKPoint LPoint(float lx, float ly)
    {
        float xDist = lx * LDistance;
        float yDist = ly * LDistance;
        float xPos = Center.X + xDist;
        float yPos = Center.Y + yDist;
        return new SKPoint(xPos, yPos);
    }

    // ----------------------------------------------------------------------------------------

    // Export the canvas as PNG bytes
    public byte[] ToPngBytes()
    {
        // Render(); // Ensure all elements are drawn

        using var image = Surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    // Save the canvas directly to a PNG file
    public void SaveToPng(string filePath)
    {
        var pngBytes = ToPngBytes();
        File.WriteAllBytes(filePath, pngBytes);
    }

    // ----------------------------------------------------------------------------------------

    // Get the bitmap representation of the canvas
    // Usage: SKBitmap bitmap = canvas.ToBitmap();
    public SKBitmap ToBitmap()
    {
        using var image = Surface.Snapshot();
        return SKBitmap.FromImage(image);
    }

    public void Dispose()
    {
        Surface?.Dispose();
    }
}