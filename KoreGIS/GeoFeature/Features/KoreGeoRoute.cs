// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// --------------------------------------------------------------------------------------------
// MARK: Route (Collection of Legs)
// --------------------------------------------------------------------------------------------

// A route composed of multiple route legs
// Each leg can be a different type (straight, great circle, bezier, flexible join)
public class KoreGeoRoute : KoreGeoFeature
{
    public List<KoreGeoRouteLeg> Legs        { get; set; } = new List<KoreGeoRouteLeg>();
    public double                LineWidth   { get; set; } = 1.0;
    public KoreColorRGB          Color       { get; set; } = KoreColorRGB.Black;
    public KoreLLBox?            BoundingBox { get; private set; }

    // Generate all points for the entire route
    public List<KoreLLPoint> GeneratePoints(int pointsPerLeg = 20)
    {
        var allPoints = new List<KoreLLPoint>();

        foreach (var leg in Legs)
        {
            var legPoints = leg.GeneratePoints(pointsPerLeg);

            // Skip first point of subsequent legs to avoid duplication
            if (allPoints.Count > 0)
                legPoints.RemoveAt(0);

            allPoints.AddRange(legPoints);
        }

        return allPoints;
    }

    // --------------------------------------------------------------------------------------------

    public void AddLeg(KoreGeoRouteLeg leg)
    {
        Legs.Add(leg);
    }

    // --------------------------------------------------------------------------------------------

    public void CalcBoundingBox()
    {
        if (Legs.Count == 0)
        {
            BoundingBox = null;
            return;
        }

        var allPoints = new List<KoreLLPoint>();

        foreach (var leg in Legs)
        {
            // Ensure each leg has its bounding box calculated
            if (leg.BoundingBox == null)
                leg.CalcBoundingBox();

            // Sample points from each leg for accurate overall bounds
            allPoints.AddRange(leg.GeneratePoints(10));
        }

        BoundingBox = allPoints.Count > 0 ? KoreLLBox.FromList(allPoints) : null;
    }

    // --------------------------------------------------------------------------------------------

    public int WaypointCount
    {
        get
        {
            if (Legs.Count == 0) return 0;
            return Legs.Count + 1; // n legs = n+1 waypoints
        }
    }
}

