// <fileheader>

using System;
using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;


// KoreLLBox: Class to represent a 3D box, curving to a spherical surface. Deals with points

// in the box based on simple angle and distance comparisons.
// Can also be used to represent a box out from an entity, such as a radar wedge.

public struct KoreLLBox
{
    public double MinLatRads { get; set; }
    public double MinLonRads { get; set; }
    public double MaxLatRads { get; set; }
    public double MaxLonRads { get; set; }

    public double MinLatDegs
    {
        get { return MinLatRads * KoreConsts.RadsToDegsMultiplier; }
        set { MinLatRads = value * KoreConsts.DegsToRadsMultiplier; }
    }
    public double MinLonDegs
    {
        get { return MinLonRads * KoreConsts.RadsToDegsMultiplier; }
        set { MinLonRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    public double MaxLatDegs
    {
        get { return MaxLatRads * KoreConsts.RadsToDegsMultiplier; }
        set { MaxLatRads = value * KoreConsts.DegsToRadsMultiplier; }
    }
    public double MaxLonDegs
    {
        get { return MaxLonRads * KoreConsts.RadsToDegsMultiplier; }
        set { MaxLonRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    // --------------------------------------------------------------------------------------------

    // Latitude range in degrees, min to max
    public KoreNumericRange<double> LatRangeDegs => new KoreNumericRange<double>(MinLatDegs, MaxLatDegs);

    // Longitude range in degrees, min to max
    public KoreNumericRange<double> LonRangeDegs => new KoreNumericRange<double>(MinLonDegs, MaxLonDegs);

    // --------------------------------------------------------------------------------------------

    public double MidLatDegs => (MinLatDegs + MaxLatDegs) / 2.0;
    public double MidLonDegs => (MinLonDegs + MaxLonDegs) / 2.0;

    // --------------------------------------------------------------------------------------------

    public double DeltaLatRads
    {
        get { return MaxLatRads - MinLatRads; }
        set { MaxLatRads = MinLatRads + value; }
    }
    public double DeltaLonRads
    {
        get { return MaxLonRads - MinLonRads; }
        set { MaxLonRads = MinLonRads + value; }
    }
    public double DeltaLatDegs
    {
        get { return MaxLatDegs - MinLatDegs; }
        set { MaxLatDegs = MinLatDegs + value; }
    }
    public double DeltaLonDegs
    {
        get { return MaxLonDegs - MinLonDegs; }
        set { MaxLonDegs = MinLonDegs + value; }
    }

    public double WidthDegs => DeltaLonDegs;
    public double HeightDegs => DeltaLatDegs;

    // --------------------------------------------------------------------------------------------

    public double HalfDeltaLonRads => DeltaLonRads / 2.0;
    public double HalfDeltaLatRads => DeltaLatRads / 2.0;
    public double HalfDeltaLonDegs => DeltaLonDegs / 2.0;
    public double HalfDeltaLatDegs => DeltaLatDegs / 2.0;

    public double LargestHalfDeltaDegs => Math.Max(HalfDeltaLatDegs, HalfDeltaLonDegs);

    // --------------------------------------------------------------------------------------------

    public KoreLLPoint PosTopLeft
    {
        get { return new KoreLLPoint() { LatRads = MaxLatRads, LonRads = MinLonRads }; }
        set { MaxLatRads = value.LatRads; MinLonRads = value.LonRads; }
    }

    public KoreLLPoint PosTopRight
    {
        get { return new KoreLLPoint() { LatRads = MaxLatRads, LonRads = MaxLonRads }; }
        set { MaxLatRads = value.LatRads; MaxLonRads = value.LonRads; }
    }

    public KoreLLPoint PosBottomLeft
    {
        get { return new KoreLLPoint() { LatRads = MinLatRads, LonRads = MinLonRads }; }
        set { MinLatRads = value.LatRads; MinLonRads = value.LonRads; }
    }

    public KoreLLPoint PosBottomRight
    {
        get { return new KoreLLPoint() { LatRads = MinLatRads, LonRads = MaxLonRads }; }
        set { MinLatRads = value.LatRads; MaxLonRads = value.LonRads; }
    }

    // --------------------------------------------------------------------------------------------

    public KoreLLPoint CenterPoint => new KoreLLPoint() { LatDegs = (MinLatDegs + MaxLatDegs) / 2.0, LonDegs = (MinLonDegs + MaxLonDegs) / 2.0 };

    // --------------------------------------------------------------------------------------------

    public KoreLLBox(double tlLatRads, double tlLonRads, double latHeightRads, double lonWidthRads)
    {
        MinLatRads = tlLatRads;
        MinLonRads = tlLonRads;
        MaxLatRads = tlLatRads + latHeightRads;
        MaxLonRads = tlLonRads + lonWidthRads;
    }

    public static KoreLLBox GlobalBox => new KoreLLBox(-Math.PI / 2.0, -Math.PI, Math.PI, 2.0 * Math.PI);

    public static KoreLLBox Zero => new KoreLLBox(0.0, 0.0, 0.0, 0.0);

    // --------------------------------------------------------------------------------------------

    // Create a new KoreLLBox that encompasses all the given points
    // Usage: KoreLLBox box = KoreLLBox.FromList(pointList);
    public static KoreLLBox FromList(List<KoreLLPoint> inPoints)
    {
        double minLat = double.MaxValue;
        double maxLat = double.MinValue;
        double minLon = double.MaxValue;
        double maxLon = double.MinValue;

        foreach (var p in inPoints)
        {
            minLat = Math.Min(minLat, p.LatRads);
            maxLat = Math.Max(maxLat, p.LatRads);
            minLon = Math.Min(minLon, p.LonRads);
            maxLon = Math.Max(maxLon, p.LonRads);
        }

        return new KoreLLBox()
        {
            MinLatRads = minLat,
            MaxLatRads = maxLat,
            MinLonRads = minLon,
            MaxLonRads = maxLon
        };
    }

    // --------------------------------------------------------------------------------------------

    public bool Contains(KoreLLPoint inPos)
    {
        if (inPos.LatRads < MinLatRads) return false;
        if (inPos.LatRads > MaxLatRads) return false;
        if (inPos.LonRads < MinLonRads) return false;
        if (inPos.LonRads > MaxLonRads) return false;

        return true;
    }

    public (float, float) GetLatLonFraction(KoreLLPoint inPos)
    {
        float latFrac = (float)((inPos.LatRads - MinLatRads) / DeltaLatRads);
        float lonFrac = (float)((inPos.LonRads - MinLonRads) / DeltaLonRads);

        return (latFrac, lonFrac);
    }

    // --------------------------------------------------------------------------------------------

    public KoreLLBox ShiftBox(double adjustLatDegs, double adjustLonDegs, double adjustAltM)
    {
        return new KoreLLBox()
        {
            MinLatDegs = this.MinLatDegs + adjustLatDegs,
            MaxLatDegs = this.MaxLatDegs + adjustLatDegs,
            MinLonDegs = this.MinLonDegs + adjustLonDegs,
            MaxLonDegs = this.MaxLonDegs + adjustLonDegs,
        };
    }

    // --------------------------------------------------------------------------------------------

    public List<KoreLLPoint> CornersList()
    {
        return new List<KoreLLPoint>
        {
            PosTopLeft,
            PosTopRight,
            PosBottomRight,
            PosBottomLeft
        };
    }

    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        return $"[{MinLatDegs:F3}, {MinLonDegs:F3}, {MaxLatDegs:F3}, {MaxLonDegs:F3}]";
    }

    public string ToStringVerbose()
    {
        return $"[MinLat:{MinLatDegs:F3}Degs, MinLon:{MinLonDegs:F3}Degs, MaxLat:{MaxLatDegs:F3}Degs, MaxLon:{MaxLonDegs:F3}Degs]";
    }

}


