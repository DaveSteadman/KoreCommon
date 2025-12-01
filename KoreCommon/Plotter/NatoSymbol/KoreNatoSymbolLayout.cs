using System;

using SkiaSharp;

namespace KoreCommon.Plotter.NatoSymbolGen;

// NATO Symbol Layout - Defines the geometric layout based on NATO APP-6(C) standard
// Centers around the octagon layout shown on page 63 of the NATO document
public partial class KoreNatoSymbolLayout
{
    public float CanvasWidth { get; }
    public float CanvasHeight { get; }
    public SKPoint Center { get; }

    // L Distance
    public float LDistance => CanvasWidth * 0.3f;

    // Main control dimensions
    // public float OctagonRadius { get; }

    // // Core geometric regions based on NATO standard
    // public SKPoint[] OctagonPoints { get; }
    // public SKRect OctagonBounds { get; }

    // // Diamond shape points
    // public SKPoint[] DiamondPoints { get; }
    // public SKRect DiamondBounds { get; }

    // // Frame thickness and spacing
    // public float FrameStrokeWidth { get; }
    // public float ElementStrokeWidth { get; }
    // public float ModifierSpacing { get; }

    public KoreNatoSymbolLayout(float canvasSize = 1000f, float octagonRadius = 200f)
    {
        CanvasWidth = canvasSize;
        CanvasHeight = canvasSize;
        Center = new SKPoint(CanvasWidth / 2f, CanvasHeight / 2f);

        // Calculate octagon radius based on canvas size and scale
        // OctagonRadius = octagonRadius;
        // OctagonPoints = DefineOctagonPoints(Center, OctagonRadius);
        // OctagonBounds = BoundingRectFromPoints(OctagonPoints);

        // // Calculate diamond points and bounds (relies on octagon radius)
        // DiamondPoints = DiamondFromCenter(Center, OctagonBounds.Width / 2f);
        // DiamondBounds = BoundingRectFromPoints(DiamondPoints);

        // Frame dimensions
        // FrameStrokeWidth = canvasSize * 0.008f; // 0.8% of canvas
        // ElementStrokeWidth = canvasSize * 0.006f; // 0.6% of canvas
        // ModifierSpacing = canvasSize * 0.05f;  // 5% spacing
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


}
