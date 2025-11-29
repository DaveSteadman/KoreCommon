// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;


public struct KoreAzElBox
{
    public double MinAzRads { get; set; }
    public double MaxAzRads { get; set; }
    public double MinElRads { get; set; }
    public double MaxElRads { get; set; }

    // --------------------------------------------------------------------------------------------

    public double MinAzDegs
    {
        get { return MinAzRads * KoreConsts.RadsToDegsMultiplier; }
        set { MinAzRads = value * KoreConsts.DegsToRadsMultiplier; }
    }
    public double MaxAzDegs
    {
        get { return MaxAzRads * KoreConsts.RadsToDegsMultiplier; }
        set { MaxAzRads = value * KoreConsts.DegsToRadsMultiplier; }
    }
    public double MinElDegs
    {
        get { return MinElRads * KoreConsts.RadsToDegsMultiplier; }
        set { MinElRads = value * KoreConsts.DegsToRadsMultiplier; }
    }
    public double MaxElDegs
    {
        get { return MaxElRads * KoreConsts.RadsToDegsMultiplier; }
        set { MaxElRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    // --------------------------------------------------------------------------------------------

    public double ArcAzRads { get { return (MaxAzRads - MinAzRads); } }
    public double ArcAzDegs { get { return (MaxAzRads - MinAzRads) * KoreConsts.RadsToDegsMultiplier; } }
    public double ArcElRads { get { return (MaxElRads - MinElRads); } }
    public double ArcElDegs { get { return (MaxElRads - MinElRads) * KoreConsts.RadsToDegsMultiplier; } }

    // --------------------------------------------------------------------------------------------

    public double HalfArcAzRads { get { return ArcAzRads / 2.0; } }
    public double HalfArcAzDegs { get { return ArcAzDegs / 2.0; } }
    public double HalfArcElRads { get { return ArcElRads / 2.0; } }
    public double HalfArcElDegs { get { return ArcElDegs / 2.0; } }

    // --------------------------------------------------------------------------------------------

    public double MidAzRads { get { return MinAzRads + HalfArcAzRads; } }
    public double MidAzDegs { get { return MinAzDegs + HalfArcAzDegs; } }
    public double MidElRads { get { return MinElRads + HalfArcElRads; } }
    public double MidElDegs { get { return MinElDegs + HalfArcElDegs; } }

    // --------------------------------------------------------------------------------------------

    public KoreAzEl Center { get { return new KoreAzEl(MidAzRads, MidElRads); } }

    // --------------------------------------------------------------------------------------------

    public static KoreAzElBox Zero
    {
        get { return new KoreAzElBox { MinAzRads = 0.0, MaxAzRads = 0.0, MinElRads = 0.0, MaxElRads = 0.0 }; }
    }

    // --------------------------------------------------------------------------------------------

    public bool IsOffsetInBox(KoreAzElRange offset)
    {
        bool inAz = (offset.AzRads >= MinAzRads && offset.AzRads <= MaxAzRads);
        bool inEl = (offset.ElRads >= MinElRads && offset.ElRads <= MaxElRads);
        return (inAz && inEl);
    }

    // --------------------------------------------------------------------------------------------

    public static KoreAzElBox BoxFromOffset(KoreAzElRange offset)
    {
        return new KoreAzElBox
        {
            MinAzRads = offset.AzRads,
            MaxAzRads = offset.AzRads,
            MinElRads = offset.ElRads,
            MaxElRads = offset.ElRads,
        };
    }

    // Usage: KoreAzElBox.BoxFromDimensions(10, 20);
    public static KoreAzElBox BoxFromDimensions(double azDegs, double elDegs)
    {
        double halfAzDegs = azDegs / 2.0;
        double halfElDegs = elDegs / 2.0;
        return new KoreAzElBox
        {
            MinAzDegs = -halfAzDegs,
            MaxAzDegs = halfAzDegs,
            MinElDegs = -halfElDegs,
            MaxElDegs = halfElDegs,
        };
    }

    // --------------------------------------------------------------------------------------------

    public KoreAzElBox Inflate(double azInflateDegs, double elInflateDegs)
    {
        return new KoreAzElBox
        {
            MinAzDegs = MinAzDegs - azInflateDegs,
            MaxAzDegs = MaxAzDegs + azInflateDegs,
            MinElDegs = MinElDegs - elInflateDegs,
            MaxElDegs = MaxElDegs + elInflateDegs
        };
    }

    public KoreAzElBox ExpandBoxByOffset(KoreAzElRange offset)
    {
        return new KoreAzElBox
        {
            MinAzRads = Math.Min(MinAzRads, offset.AzRads),
            MaxAzRads = Math.Max(MaxAzRads, offset.AzRads),
            MinElRads = Math.Min(MinElRads, offset.ElRads),
            MaxElRads = Math.Max(MaxElRads, offset.ElRads)
        };
    }

    public KoreAzElBox ExpandBoxByBox(KoreAzElBox box)
    {
        return new KoreAzElBox
        {
            MinAzRads = Math.Min(MinAzRads, box.MinAzRads),
            MaxAzRads = Math.Max(MaxAzRads, box.MaxAzRads),
            MinElRads = Math.Min(MinElRads, box.MinElRads),
            MaxElRads = Math.Max(MaxElRads, box.MaxElRads)
        };
    }

    // --------------------------------------------------------------------------------------------

    public KoreAzElBox CentreBoxOnOffset(KoreAzElRange offset)
    {
        return new KoreAzElBox
        {
            MinAzRads = MinAzRads - HalfArcAzRads,
            MaxAzRads = MaxAzRads + HalfArcAzRads,
            MinElRads = MinElRads - HalfArcElRads,
            MaxElRads = MaxElRads + HalfArcElRads
        };
    }

    public KoreAzElBox ShiftBoxAzElRange(double azAdjustDegs, double elAdjustDegs)
    {
        return new KoreAzElBox
        {
            MinAzDegs = MinAzDegs + azAdjustDegs,
            MaxAzDegs = MaxAzDegs + azAdjustDegs,
            MinElDegs = MinElDegs + elAdjustDegs,
            MaxElDegs = MaxElDegs + elAdjustDegs
        };
    }

    // --------------------------------------------------------------------------------------------

    public KoreAzElBox ConsistencyCheck()
    {
        return new KoreAzElBox
        {
            MinAzRads = MinAzRads,
            MaxAzRads = MaxAzRads,
            MinElRads = (MinElRads < MaxElRads) ? MinElRads : MaxElRads,
            MaxElRads = (MinElRads < MaxElRads) ? MaxElRads : MinElRads
        };
    }

    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        return $"MinAz:{MinAzDegs:F2}Degs MaxAz:{MaxAzDegs:F2}Degs / MinEl:{MinElDegs:F2}Degs MaxEl:{MaxElDegs:F2}Degs";
    }

}

