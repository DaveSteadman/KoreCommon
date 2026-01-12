// <fileheader>

using SkiaSharp;
using System;
using System.IO;
using System.Collections.Generic;

namespace KoreCommon.SkiaSharp;

// KoreSkiaSharpPlotter: 2D plotting functionality using SkiaSharp
// - Uses SK types exclusively for performance and compatibility
// - See KoreSkiaSharpPlotter.Interface.cs for Kore type conversions

public partial class KoreSkiaSharpPlotter
{
    // --------------------------------------------------------------------------------------------
    // MARK: Gradient
    // --------------------------------------------------------------------------------------------

    public void DrawCircularGradient(KoreXYVector center, float radius, SKColor innerColor, SKColor outerColor)
    {
        using var shader = SKShader.CreateRadialGradient(
            KoreSkiaSharpConv.ToSKPoint(center),
            radius,
            new SKColor[] { innerColor, outerColor },
            new float[] { 0, 1 },
            SKShaderTileMode.Clamp
        );

        using var paint = new SKPaint
        {
            Shader = shader,
            IsAntialias = true
        };

        canvas.DrawCircle(KoreSkiaSharpConv.ToSKPoint(center), radius, paint);
    }

    // --------------------------------------------------------------------------------------------

    public void DrawCircularGradientInPolygon(
        SKPoint[] polygonPoints, SKPoint center,
        float radius, SKColor innerColor, SKColor outerColor)
    {
        using var path = new SKPath();

        if (polygonPoints.Length > 0)
        {
            path.MoveTo(polygonPoints[0]);
            for (int i = 1; i < polygonPoints.Length; i++)
            {
                path.LineTo(polygonPoints[i]);
            }
            path.Close();
        }

        canvas.Save();
        canvas.ClipPath(path);

        using var shader = SKShader.CreateRadialGradient(
            center,
            radius,
            new SKColor[] { innerColor, outerColor },
            new float[] { 0, 1 },
            SKShaderTileMode.Clamp
        );

        using var paint = new SKPaint
        {
            Shader = shader,
            IsAntialias = true
        };

        canvas.DrawCircle(center, radius, paint);
        canvas.Restore();
    }

}
