// <fileheader>




using KoreCommon;
using SkiaSharp;

namespace KoreCommon.SkiaSharp;

public static class KoreSkiaSharpColorOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: To SKColor
    // --------------------------------------------------------------------------------------------

    // Usage: KoreSkiaSharpColorOps.ColorFromKoreColor(koreColor);
    public static SKColor ColorFromKoreColor(KoreColorRGB color)
    {
        // Convert KoreColor to SKColor
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    public static SKColor ColorFromKoreColorGreyScale(KoreColorGreyscale color)
    {
        // Convert KoreColorGreyScale to SKColor
        return new SKColor(color.V, color.V, color.V, 255);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: From SKColor
    // --------------------------------------------------------------------------------------------

    public static KoreColorRGB KoreColorFromSkiaColor(SKColor color)
    {
        // Convert SKColor to KoreColor
        return new KoreColorRGB(color.Red, color.Green, color.Blue, color.Alpha);
    }

    public static KoreColorGreyscale KoreColorFromSkiaColorGreyScale(SKColor color)
    {
        KoreColorRGB rgb = KoreColorFromSkiaColor(color);
        return KoreColorConv.RGBtoGreyscale(rgb);
    }
}


