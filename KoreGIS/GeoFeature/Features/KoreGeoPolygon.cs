// <fileheader>

#nullable enable

using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// A geographic polygon with optional holes
public class KoreGeoPolygon : KoreGeoFeature
{
    // NOTE: GeoJSON RFC 7946 specifies winding direction conventions:
    // - OuterRing should be counter-clockwise (CCW)
    // - InnerRings (holes) should be clockwise (CW)
    // However, the Contains() method works regardless of actual winding direction
    public List<KoreLLPoint> OuterRing { get; set; } = new List<KoreLLPoint>();
    public List<List<KoreLLPoint>> InnerRings { get; set; } = new List<List<KoreLLPoint>>(); // Holes
    public KoreColorRGB? FillColor { get; set; }
    public KoreColorRGB? StrokeColor { get; set; }
    public double StrokeWidth { get; set; } = 1.0;
    public KoreLLBox? BoundingBox { get; private set; }

    public void CalcBoundingBox()
    {
        var allPoints = new List<KoreLLPoint>();
        allPoints.AddRange(OuterRing);
        foreach (var innerRing in InnerRings)
        {
            allPoints.AddRange(innerRing);
        }
        BoundingBox = allPoints.Count > 0 ? KoreLLBox.FromList(allPoints) : null;
    }

    // --------------------------------------------------------------------------------------------

    public bool Contains(KoreLLPoint point)
    {
        if (!KoreLLPointOps.IsPointInRing(OuterRing, point))
        {
            return false; // Outside outer ring
        }

        foreach (var innerRing in InnerRings)
        {
            if (KoreLLPointOps.IsPointInRing(innerRing, point))
            {
                return false; // Inside a hole
            }
        }

        return true; // Inside polygon
    }
}

