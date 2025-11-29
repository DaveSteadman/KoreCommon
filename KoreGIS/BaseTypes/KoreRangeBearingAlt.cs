// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;


public struct KoreRangeBearingAlt
{
    public double RangeM { get; set; }
    public double BearingRads { get; set; }
    public double AltDeltaM { get; set; } // Altitude difference in metres

    // --------------------------------------------------------------------------------------------

    public double HorizontalRangeKm // Alt above EarthRadius
    {
        get { return (RangeM * KoreWorldConsts.MetresToKmMultiplier); }
        set { RangeM = (value * KoreWorldConsts.KmToMetresMultiplier); }
    }

    public double SlantRangeKm // Straight-line distance from origin to target
    {
        get { return Math.Sqrt((RangeM * RangeM) + (AltDeltaM * AltDeltaM)) * KoreWorldConsts.MetresToKmMultiplier; }
        set { RangeM = Math.Sqrt((value * value) / (KoreWorldConsts.MetresToKmMultiplier * KoreWorldConsts.MetresToKmMultiplier) - (AltDeltaM * AltDeltaM)); }
    }

    public double BearingDegs
    {
        get { return BearingRads * KoreConsts.RadsToDegsMultiplier; }
        set { BearingRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    // --------------------------------------------------------------------------------------------

    public KoreRangeBearingAlt(double inRangeM, double inBearingRads, double inAltDeltaM = 0.0)
    {
        this.RangeM = inRangeM;
        this.BearingRads = inBearingRads;
        this.AltDeltaM = inAltDeltaM;
    }

    public double BearingDifferenceDegs(KoreRangeBearing compareRB)
    {
        return BearingDegs - compareRB.BearingDegs;
    }

}

