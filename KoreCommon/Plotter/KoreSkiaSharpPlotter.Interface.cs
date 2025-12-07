// <fileheader>

using SkiaSharp;
using System;
using System.Collections.Generic;

namespace KoreCommon.SkiaSharp;

// KoreSkiaSharpPlotter.Interface: Conversion functions between Kore types and SK types
// - Provides seamless integration between KoreCommon types and SkiaSharp plotting
// - Extension methods for easy conversion and plotting with Kore data structures

public partial class KoreSkiaSharpPlotter
{
    // --------------------------------------------------------------------------------------------
    // MARK: Point
    // --------------------------------------------------------------------------------------------

    public void DrawPoint(KoreXYVector v, int crossSize = 3)        => DrawPointAsCross(KoreSkiaSharpConv.ToSKPoint(v), crossSize);
    public void DrawPointAsCross(KoreXYVector v, int crossSize = 3) => DrawPointAsCross(KoreSkiaSharpConv.ToSKPoint(v), crossSize);
    public void DrawPointAsCircle(KoreXYVector v, int radius = 3)   => DrawPointAsCircle(KoreSkiaSharpConv.ToSKPoint(v), radius);

    // --------------------------------------------------------------------------------------------
    // MARK: Circle
    // --------------------------------------------------------------------------------------------

    public void DrawCircle(KoreXYVector center, float radius)
    {
        SKPoint skCenter = KoreSkiaSharpConv.ToSKPoint(center);
        canvas.DrawCircle(skCenter, radius, DrawSettings.Paint);
    }

    public void DrawCircle(KoreXYCircle circle)
    {
        SKPoint skCenter = KoreSkiaSharpConv.ToSKPoint(circle.Center);
        canvas.DrawCircle(skCenter, (float)circle.Radius, DrawSettings.Paint);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Arc
    // --------------------------------------------------------------------------------------------

    public void DrawArc(KoreXYArc arc)
    {
        SKRect skRect = new SKRect(
            (float)(arc.Center.X - arc.Radius),
            (float)(arc.Center.Y - arc.Radius),
            (float)(arc.Center.X + arc.Radius),
            (float)(arc.Center.Y + arc.Radius)
        );

        float startAngleDegs = (float)KoreValueUtils.RadsToDegs(arc.StartAngleRads);
        float sweepAngleDegs = (float)KoreValueUtils.RadsToDegs(arc.DeltaAngleRads);

        canvas.DrawArc(skRect, startAngleDegs, sweepAngleDegs, false, DrawSettings.Paint);
    }

    public void DrawArcBox(KoreXYAnnularSector arcbox)
    {
        KoreXYArc innerArc = arcbox.InnerArc;
        KoreXYArc outerArc = arcbox.OuterArc;

        DrawArc(innerArc);  // Draw inner arc
        DrawArc(outerArc);  // Draw outer arc

        DrawLine(arcbox.StartInnerOuterLine); // Draw connecting start lines
        DrawLine(arcbox.EndInnerOuterLine);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Line
    // --------------------------------------------------------------------------------------------

    public void DrawLine(KoreXYVector v1, KoreXYVector v2) => DrawLine(KoreSkiaSharpConv.ToSKPoint(v1), KoreSkiaSharpConv.ToSKPoint(v2));

    public void DrawLine(KoreXYLine line)
    {
        SKPoint p1 = KoreSkiaSharpConv.ToSKPoint(line.P1);
        SKPoint p2 = KoreSkiaSharpConv.ToSKPoint(line.P2);
        DrawLine(p1, p2);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Triangle
    // --------------------------------------------------------------------------------------------

    public void DrawTriangle(KoreXYVector p1, KoreXYVector p2, KoreXYVector p3) => DrawTriangle(KoreSkiaSharpConv.ToSKPoint(p1), KoreSkiaSharpConv.ToSKPoint(p2), KoreSkiaSharpConv.ToSKPoint(p3));

    // --------------------------------------------------------------------------------------------
    // MARK: Rect
    // --------------------------------------------------------------------------------------------

    public void DrawRect(KoreXYRect rect, SKPaint fillPaint)
    {
        SKRect skRect = KoreSkiaSharpConv.ToSKRect(rect);
        DrawRect(skRect, fillPaint);
    }
    public void DrawRect(KoreXYRect rect)
    {
        SKRect skRect = KoreSkiaSharpConv.ToSKRect(rect);
        DrawRect(skRect, DrawSettings.Paint);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Color
    // --------------------------------------------------------------------------------------------

    public void SetColor(KoreColorRGB color)
    {
        DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(color);
    }

    public void SetColor(KoreColorRGB color, float alpha)
    {
        DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(color, alpha);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Text
    // --------------------------------------------------------------------------------------------

    public void DrawText(string text, KoreXYVector pos, int fontSize = 12)           => DrawText(text, KoreSkiaSharpConv.ToSKPoint(pos), fontSize);
    public void DrawTextCentered(string text, KoreXYVector pos, float fontSize = 12) => DrawTextCentered(text, KoreSkiaSharpConv.ToSKPoint(pos), fontSize);

    // --------------------------------------------------------------------------------------------
    // MARK: Mesh Data Drawing
    // --------------------------------------------------------------------------------------------

    public void DrawMeshWireframe(KoreMeshData meshData, KoreColorRGB? lineColor = null)
    {
        if (lineColor.HasValue)
        {
            DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(lineColor.Value);
        }

        // Draw all lines in the mesh
        foreach (var line in meshData.Lines.Values)
        {
            if (meshData.Vertices.TryGetValue(line.A, out var vertexA) &&
                meshData.Vertices.TryGetValue(line.B, out var vertexB))
            {
                DrawLine(KoreSkiaSharpConv.ToSKPoint(vertexA), KoreSkiaSharpConv.ToSKPoint(vertexB));
            }
        }
    }

    public void DrawMeshPoints(KoreMeshData meshData, KoreColorRGB? pointColor = null, int pointSize = 3)
    {
        if (pointColor.HasValue)
        {
            DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(pointColor.Value);
        }

        // Draw all vertices as points
        foreach (var vertex in meshData.Vertices.Values)
        {
            DrawPointAsCross(KoreSkiaSharpConv.ToSKPoint(vertex), pointSize);
        }
    }

    public void DrawMeshTriangles(KoreMeshData meshData, KoreColorRGB? fillColor = null)
    {
        if (fillColor.HasValue)
        {
            DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(fillColor.Value);
            DrawSettings.Paint.Style = SKPaintStyle.Fill;
        }

        // Draw all triangles as filled shapes
        foreach (var triangle in meshData.Triangles.Values)
        {
            if (meshData.Vertices.TryGetValue(triangle.A, out var vertexA) &&
                meshData.Vertices.TryGetValue(triangle.B, out var vertexB) &&
                meshData.Vertices.TryGetValue(triangle.C, out var vertexC))
            {
                DrawTriangle(KoreSkiaSharpConv.ToSKPoint(vertexA), KoreSkiaSharpConv.ToSKPoint(vertexB), KoreSkiaSharpConv.ToSKPoint(vertexC));
            }
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Path Drawing (for Bezier curves)
    // --------------------------------------------------------------------------------------------

    public void DrawPath(List<KoreXYVector> pathPoints, KoreColorRGB? lineColor = null)
    {
        if (pathPoints.Count < 2) return;

        if (lineColor.HasValue)
        {
            DrawSettings.Color = KoreSkiaSharpConv.ToSKColor(lineColor.Value);
        }

        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            DrawLine(KoreSkiaSharpConv.ToSKPoint(pathPoints[i]), KoreSkiaSharpConv.ToSKPoint(pathPoints[i + 1]));
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Gradient
    // --------------------------------------------------------------------------------------------

    public void DrawCircularGradientInPolygon(KoreXYPolygon polygon, KoreXYVector center, float radius, SKColor innerColor, SKColor outerColor)
    {
        SKPoint[] skPoints = new SKPoint[polygon.Vertices.Count];
        for (int i = 0; i < polygon.Vertices.Count; i++)
        {
            skPoints[i] = KoreSkiaSharpConv.ToSKPoint(polygon.Vertices[i]);
        }

        SKPoint skCenter = KoreSkiaSharpConv.ToSKPoint(center);
        DrawCircularGradientInPolygon(skPoints, skCenter, radius, innerColor, outerColor);
    }

    public void DrawColorRangeGradientInPolygon(
        KoreXYPolygon polygon, KoreXYVector center,
        float radius, KoreColorRange colorRange)
    {
        SKPoint[] skPoints = new SKPoint[polygon.Vertices.Count];
        for (int i = 0; i < polygon.Vertices.Count; i++)
        {
            skPoints[i] = KoreSkiaSharpConv.ToSKPoint(polygon.Vertices[i]);
        }

        SKPoint skCenter = KoreSkiaSharpConv.ToSKPoint(center);

        using var path = new SKPath();

        if (skPoints.Length > 0)
        {
            path.MoveTo(skPoints[0]);
            for (int i = 1; i < skPoints.Length; i++)
            {
                path.LineTo(skPoints[i]);
            }
            path.Close();
        }

        canvas.Save();
        canvas.ClipPath(path);

        // Sample the color range to create gradient stops
        // Use a reasonable number of samples for smooth gradients
        int sampleCount = 32;
        SKColor[] colors = new SKColor[sampleCount];
        float[] positions = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float fraction = i / (float)(sampleCount - 1);
            positions[i] = fraction;
            KoreColorRGB koreColor = colorRange.GetColor(fraction);
            colors[i] = KoreSkiaSharpConv.ToSKColor(koreColor);
        }

        using var shader = SKShader.CreateRadialGradient(
            skCenter,
            radius,
            colors,
            positions,
            SKShaderTileMode.Clamp
        );

        using var paint = new SKPaint
        {
            Shader = shader,
            IsAntialias = true
        };

        canvas.DrawCircle(skCenter, radius, paint);
        canvas.Restore();
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Export
    // --------------------------------------------------------------------------------------------

    // Gets the PNG byte array from the plotter's current canvas
    public byte[] GetPngBytes()
    {
        using var image = GetBitmap().Encode(SKEncodedImageFormat.Png, 100);
        return image.ToArray();
    }

    // --------------------------------------------------------------------------------------------


}