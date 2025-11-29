// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;


public struct KoreRangeBearing
{
    public double RangeM      { get; set; }
    public double BearingRads { get; set; }

    // --------------------------------------------------------------------------------------------

    public double RangeKm // Alt above EarthRadius
    {
        get { return (RangeM * KoreWorldConsts.MetresToKmMultiplier); }
        set { RangeM = (value * KoreWorldConsts.KmToMetresMultiplier); }
    }

    public double BearingDegs
    {
        get { return BearingRads * KoreConsts.RadsToDegsMultiplier; }
        set { BearingRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    // --------------------------------------------------------------------------------------------

    public KoreRangeBearing(double inRangeM, double inBearingRads)
    {
        this.RangeM      = inRangeM;
        this.BearingRads = inBearingRads;
    }

    public double BearingDifferenceDegs(KoreRangeBearing compareRB)
    {
        return BearingDegs - compareRB.BearingDegs;
    }

}

