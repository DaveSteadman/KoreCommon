// <fileheader>

#nullable enable

using KoreCommon;

namespace KoreGIS;

// A geographic circle defined by center point and radius
public class KoreGeoCircle : KoreGeoFeature
{
    public KoreLLPoint Center { get; set; }
    public double RadiusMeters { get; set; }
    public KoreColorRGB? FillColor { get; set; }
    public KoreColorRGB? StrokeColor { get; set; }
    public double StrokeWidth { get; set; } = 1.0;
}

