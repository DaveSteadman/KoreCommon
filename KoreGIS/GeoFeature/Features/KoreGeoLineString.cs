// <fileheader>

#nullable enable

using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// A geographic line string (sequence of connected points)
public class KoreGeoLineString : KoreGeoFeature
{
    public List<KoreLLPoint> Points { get; set; } = new List<KoreLLPoint>();
    public double LineWidth { get; set; } = 1.0;
    public KoreColorRGB Color { get; set; } = KoreColorRGB.Black;
    public bool IsGreatCircle { get; set; } = false; // Future: curved vs straight
    public KoreLLBox? BoundingBox { get; private set; }

    public void CalcBoundingBox()
    {
        BoundingBox = Points.Count > 0 ? KoreLLBox.FromList(Points) : null;
    }
}

