// <fileheader>

using System;
using System.Collections.Generic;
using SkiaSharp;
using KoreCommon.SkiaSharp;

using KoreCommon;
namespace KoreGIS;

// KoreWorldPlotter: Creates equirectangular (2:1 aspect ratio) world map projections
// Provides coordinate mapping between lat/lon and pixel coordinates for visualization
public class KoreWorldPlotter
{
    public KoreSkiaSharpPlotter Plotter { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    // Geographic bounds
    public KoreLLBox GeoBounds { get; private set; }

    // Alias/Projection bounds in degrees
    public double MinLatDegs => GeoBounds.MinLatDegs;
    public double MaxLatDegs => GeoBounds.MaxLatDegs;
    public double MinLonDegs => GeoBounds.MinLonDegs;
    public double MaxLonDegs => GeoBounds.MaxLonDegs;

    // Pixel bounds
    public KoreXYRect PixelBounds { get; private set; }

    // --------------------------------------------------------------------------------------------
    // MARK: Constructors
    // --------------------------------------------------------------------------------------------

    public KoreWorldPlotter(int width = 3600, int height = 1800)
        : this(width, height, KoreLLBox.GlobalBox)
    {
    }

    public KoreWorldPlotter(int width, int height, KoreLLBox geoBounds)
    {
        Width = width;
        Height = height;
        GeoBounds = geoBounds;
        PixelBounds = new KoreXYRect(0, 0, width, height);
        Plotter = new KoreSkiaSharpPlotter(width, height);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Coordinate Mapping: Lat/Lon <-> Pixel
    // --------------------------------------------------------------------------------------------

    public KoreXYVector LatLonToPixel(KoreLLPoint llPoint)
    {
        return LatLonToPixel(llPoint.LatDegs, llPoint.LonDegs);
    }

    public KoreXYVector LatLonToPixel(double latDegs, double lonDegs)
    {
        // Equirectangular projection: simple linear mapping
        double x = ((lonDegs - MinLonDegs) / (MaxLonDegs - MinLonDegs)) * Width;
        double y = ((MaxLatDegs - latDegs) / (MaxLatDegs - MinLatDegs)) * Height; // Flip Y (image origin is top-left)

        return new KoreXYVector(x, y);
    }

    public KoreLLPoint PixelToLatLon(KoreXYVector pixel)
    {
        return PixelToLatLon(pixel.X, pixel.Y);
    }

    public KoreLLPoint PixelToLatLon(double x, double y)
    {
        // Reverse equirectangular projection
        double lonDegs = MinLonDegs + (x / Width)  * (MaxLonDegs - MinLonDegs);
        double latDegs = MaxLatDegs - (y / Height) * (MaxLatDegs - MinLatDegs); // Flip Y back

        return new KoreLLPoint { LatDegs = latDegs, LonDegs = lonDegs };
    }

    // --------------------------------------------------------------------------------------------
    // Basic Drawing
    // --------------------------------------------------------------------------------------------

    public void DrawPoint(KoreLLPoint llPoint, KoreColorRGB color, double size = 5.0)
    {
        KoreXYVector pixel = LatLonToPixel(llPoint);
        Plotter.DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(color);
        Plotter.DrawPoint(pixel, (int)size);
    }

    public void DrawTextAtPosition(string text, KoreLLPoint llPoint, KoreXYRectPosition anchor, int fontSize = 12)
    {
        KoreXYVector pixel = LatLonToPixel(llPoint);
        Plotter.DrawTextAtPosition(text, pixel, anchor, fontSize);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Point
    // --------------------------------------------------------------------------------------------

    public void DrawGeoPoint(KoreGeoPoint geoPoint)
    {
        DrawPoint(geoPoint.Position, geoPoint.Color, geoPoint.Size);

        if (!string.IsNullOrEmpty(geoPoint.Label))
        {
            DrawTextAtPosition(geoPoint.Label, geoPoint.Position, geoPoint.LabelPosition, geoPoint.LabelFontSize);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: MultiPoint
    // --------------------------------------------------------------------------------------------

    public void DrawGeoMultiPoint(KoreGeoMultiPoint geoMultiPoint)
    {
        foreach (var point in geoMultiPoint.Points)
        {
            DrawPoint(point, geoMultiPoint.Color, geoMultiPoint.Size);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: LineString
    // --------------------------------------------------------------------------------------------

    public void DrawGeoLineString(KoreGeoLineString geoLineString)
    {
        if (geoLineString.Points.Count < 2)
            return;

        Plotter.DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(geoLineString.Color);
        Plotter.DrawSettings.LineWidth = (float)geoLineString.LineWidth;

        // Convert all points to pixels and draw line segments
        for (int i = 0; i < geoLineString.Points.Count - 1; i++)
        {
            var startPixel = LatLonToPixel(geoLineString.Points[i]);
            var endPixel = LatLonToPixel(geoLineString.Points[i + 1]);
            Plotter.DrawLine(startPixel, endPixel);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: MultiLineString
    // --------------------------------------------------------------------------------------------

    public void DrawGeoMultiLineString(KoreGeoMultiLineString geoMultiLine)
    {
        if (geoMultiLine.LineStrings.Count == 0)
            return;

        Plotter.DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(geoMultiLine.Color);
        Plotter.DrawSettings.LineWidth = (float)geoMultiLine.LineWidth;

        foreach (var line in geoMultiLine.LineStrings)
        {
            if (line.Count < 2)
                continue;

            for (int i = 0; i < line.Count - 1; i++)
            {
                var startPixel = LatLonToPixel(line[i]);
                var endPixel = LatLonToPixel(line[i + 1]);
                Plotter.DrawLine(startPixel, endPixel);
            }
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Polygon
    // --------------------------------------------------------------------------------------------

    public void DrawGeoPolygon(KoreGeoPolygon geoPolygon)
    {
        if (geoPolygon.OuterRing.Count < 3)
            return;

        // Convert outer ring to pixel coordinates
        var outerRingPixels = new List<KoreXYVector>();
        foreach (var point in geoPolygon.OuterRing)
        {
            outerRingPixels.Add(LatLonToPixel(point));
        }

        // Create SKPath for the polygon
        using (var path = new SKPath())
        {
            // Add outer ring to path
            path.MoveTo(KoreSkiaSharpConv.ToSKPoint(outerRingPixels[0]));
            for (int i = 1; i < outerRingPixels.Count; i++)
            {
                path.LineTo(KoreSkiaSharpConv.ToSKPoint(outerRingPixels[i]));
            }
            path.Close();

            // Add inner rings (holes) to path with opposite winding
            foreach (var innerRing in geoPolygon.InnerRings)
            {
                if (innerRing.Count < 3)
                    continue;

                var innerRingPixels = new List<KoreXYVector>();
                foreach (var point in innerRing)
                {
                    innerRingPixels.Add(LatLonToPixel(point));
                }

                path.MoveTo(KoreSkiaSharpConv.ToSKPoint(innerRingPixels[0]));
                for (int i = 1; i < innerRingPixels.Count; i++)
                {
                    path.LineTo(KoreSkiaSharpConv.ToSKPoint(innerRingPixels[i]));
                }
                path.Close();
            }

            // Draw filled polygon if fill color is specified
            if (geoPolygon.FillColor.HasValue)
            {
                using (var fillPaint = new SKPaint())
                {
                    fillPaint.Color = KoreSkiaSharpConv.ToSKColor(geoPolygon.FillColor.Value);
                    fillPaint.Style = SKPaintStyle.Fill;
                    fillPaint.IsAntialias = true;

                    Plotter.GetCanvas().DrawPath(path, fillPaint);
                }
            }

            // Draw stroke
            if (geoPolygon.StrokeColor.HasValue || geoPolygon.StrokeWidth > 0)
            {
                var strokeColor = geoPolygon.StrokeColor ?? geoPolygon.FillColor ?? KoreColorRGB.Black;

                using (var strokePaint = new SKPaint())
                {
                    strokePaint.Color = KoreSkiaSharpConv.ToSKColor(strokeColor);
                    strokePaint.Style = SKPaintStyle.Stroke;
                    strokePaint.StrokeWidth = (float)geoPolygon.StrokeWidth;
                    strokePaint.IsAntialias = true;

                    Plotter.GetCanvas().DrawPath(path, strokePaint);
                }
            }
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: MultiPolygon
    // --------------------------------------------------------------------------------------------

    public void DrawGeoMultiPolygon(KoreGeoMultiPolygon geoMultiPolygon)
    {
        foreach (var polygon in geoMultiPolygon.Polygons)
        {
            var originalFill = polygon.FillColor;
            var originalStroke = polygon.StrokeColor;
            var originalStrokeWidth = polygon.StrokeWidth;

            if (!polygon.FillColor.HasValue && geoMultiPolygon.FillColor.HasValue)
            {
                polygon.FillColor = geoMultiPolygon.FillColor;
            }

            if (!polygon.StrokeColor.HasValue && geoMultiPolygon.StrokeColor.HasValue)
            {
                polygon.StrokeColor = geoMultiPolygon.StrokeColor;
            }

            if (Math.Abs(polygon.StrokeWidth - 1.0) < double.Epsilon && Math.Abs(geoMultiPolygon.StrokeWidth - 1.0) > double.Epsilon)
            {
                polygon.StrokeWidth = geoMultiPolygon.StrokeWidth;
            }

            DrawGeoPolygon(polygon);

            polygon.FillColor = originalFill;
            polygon.StrokeColor = originalStroke;
            polygon.StrokeWidth = originalStrokeWidth;
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Circle
    // --------------------------------------------------------------------------------------------

    public void DrawGeoCircle(KoreGeoCircle geoCircle)
    {
        // Convert center to pixels
        var centerPixel = LatLonToPixel(geoCircle.Center);
        var centerSK = KoreSkiaSharpConv.ToSKPoint(centerPixel);

        // Approximate circle radius in pixels (simplified - assumes small circles)
        // For more accurate circles at large scales, would need proper geodetic calculations
        double latDegsPerMeter = 1.0 / 111320.0; // Rough approximation
        double radiusDegs = geoCircle.RadiusMeters * latDegsPerMeter;
        double radiusPixels = (radiusDegs / (MaxLatDegs - MinLatDegs)) * Height;

        // Draw filled circle if fill color specified
        if (geoCircle.FillColor.HasValue)
        {
            var fillColor = KoreSkiaSharpConv.ToSKColor(geoCircle.FillColor.Value);
            using (var fillPaint = new SKPaint
            {
                Color = fillColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                Plotter.GetCanvas().DrawCircle(centerSK, (float)radiusPixels, fillPaint);
            }
        }

        // Draw stroke if stroke color specified
        if (geoCircle.StrokeColor.HasValue)
        {
            Plotter.DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(geoCircle.StrokeColor.Value);
            Plotter.DrawSettings.LineWidth = (float)geoCircle.StrokeWidth;
            Plotter.DrawPointAsCircle(centerSK, (int)radiusPixels);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Collection
    // --------------------------------------------------------------------------------------------

    public void DrawGeoFeatureCollection(KoreGeoFeatureCollection collection)
    {
        foreach (var feature in collection.Features)
        {
            switch (feature)
            {
                case KoreGeoPoint point:
                    DrawGeoPoint(point);
                    break;
                case KoreGeoMultiPoint multiPoint:
                    DrawGeoMultiPoint(multiPoint);
                    break;
                case KoreGeoLineString lineString:
                    DrawGeoLineString(lineString);
                    break;
                case KoreGeoMultiLineString multiLine:
                    DrawGeoMultiLineString(multiLine);
                    break;
                case KoreGeoPolygon polygon:
                    DrawGeoPolygon(polygon);
                    break;
                case KoreGeoMultiPolygon multiPolygon:
                    DrawGeoMultiPolygon(multiPolygon);
                    break;
                case KoreGeoCircle circle:
                    DrawGeoCircle(circle);
                    break;
            }
        }
    }

    // --------------------------------------------------------------------------------------------
    // Save
    // --------------------------------------------------------------------------------------------

    public void Save(string filePath)
    {
        Plotter.Save(filePath);
    }
}
