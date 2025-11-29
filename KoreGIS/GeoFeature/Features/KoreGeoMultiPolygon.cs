// <fileheader>

#nullable enable

using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// A collection of geographic polygons
public class KoreGeoMultiPolygon : KoreGeoFeature
{
    public List<KoreGeoPolygon> Polygons { get; set; } = new List<KoreGeoPolygon>();
    public KoreColorRGB? FillColor { get; set; }
    public KoreColorRGB? StrokeColor { get; set; }
    public double StrokeWidth { get; set; } = 1.0;
    public KoreLLBox? BoundingBox { get; private set; }

    public void CalcBoundingBox()
    {
        var allPoints = new List<KoreLLPoint>();
        foreach (var polygon in Polygons)
        {
            allPoints.AddRange(polygon.OuterRing);
            foreach (var innerRing in polygon.InnerRings)
            {
                allPoints.AddRange(innerRing);
            }
        }
        BoundingBox = allPoints.Count > 0 ? KoreLLBox.FromList(allPoints) : null;
    }

    // --------------------------------------------------------------------------------------------

    public bool Contains(KoreLLPoint point)
    {
        foreach (var polygon in Polygons)
        {
            if (polygon.Contains(point))
            {
                return true; // Inside one of the polygons
            }
        }
        return false; // Outside all polygons
    }
}

