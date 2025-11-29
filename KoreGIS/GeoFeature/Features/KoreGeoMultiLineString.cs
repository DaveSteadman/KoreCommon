// <fileheader>

#nullable enable

using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// A collection of geographic line strings
public class KoreGeoMultiLineString : KoreGeoFeature
{
    public List<List<KoreLLPoint>> LineStrings { get; set; } = new List<List<KoreLLPoint>>();
    public double LineWidth { get; set; } = 1.0;
    public KoreColorRGB Color { get; set; } = KoreColorRGB.Black;
    public bool IsGreatCircle { get; set; } = false;
    public KoreLLBox? BoundingBox { get; private set; }

    public void CalcBoundingBox()
    {
        var allPoints = new List<KoreLLPoint>();
        foreach (var line in LineStrings)
        {
            allPoints.AddRange(line);
        }
        BoundingBox = allPoints.Count > 0 ? KoreLLBox.FromList(allPoints) : null;
    }
}

