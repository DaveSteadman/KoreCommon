// <fileheader>

#nullable enable

using KoreCommon;

namespace KoreGIS;

// A geographic point feature
public class KoreGeoPoint : KoreGeoFeature
{
    public KoreLLPoint Position { get; set; }
    public string? Label { get; set; }
    public double Size { get; set; } = 5.0;
    public KoreColorRGB Color { get; set; } = KoreColorRGB.Black;
    public KoreXYRectPosition LabelPosition { get; set; } = KoreXYRectPosition.TopRight;
    public int LabelFontSize { get; set; } = 12;
}

