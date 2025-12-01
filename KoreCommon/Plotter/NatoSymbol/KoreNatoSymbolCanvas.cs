using System;
using System.IO;

using SkiaSharp;

namespace KoreCommon.Plotter.NatoSymbolGen;

// NatoSymbolCanvas - Manages the drawing surface and composition of symbol elements
public class KoreNatoSymbolCanvas
{
    // Arrangement of points and constructs to control draw areas
    public KoreNatoSymbolLayout Layout { get; }

    // SkiaSharp drawing surface and canvas
    public SKSurface Surface { get; }
    public SKCanvas Canvas { get; }

    // List of draw elements added to the canvas
    //private readonly List<NatoSymbolElement> _elements = new();

    // ----------------------------------------------------------------------------------------

    public KoreNatoSymbolCanvas(float canvasSize = 1000f)
    {
        Layout = new KoreNatoSymbolLayout(canvasSize);

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

    // // Add an element to be drawn on the canvas
    // public void AddElement(NatoSymbolElement element) => _elements.Add(element);
    // public void AddElements(params NatoSymbolElement[] elements) => _elements.AddRange(elements);

    // // Clear all elements from the canvas
    // public void ClearElements()
    // {
    //     _elements.Clear();
    //     Canvas.Clear(SKColors.Transparent);
    // }

    // ----------------------------------------------------------------------------------------

    // Draw all elements to the canvas
    // public void Render()
    // {
    //     using var defaultPaint = new SKPaint
    //     {
    //         IsAntialias = true,
    //         Color = SKColors.Black,
    //         StrokeWidth = Layout.ElementStrokeWidth,
    //         Style = SKPaintStyle.Stroke
    //     };

    //     // Draw elements in the order they were added
    //     foreach (var element in _elements)
    //     {
    //         element.Draw(Canvas, Layout, defaultPaint);
    //     }
    // }

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

    public void Dispose()
    {
        Surface?.Dispose();
    }
}