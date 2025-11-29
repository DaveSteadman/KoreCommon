// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;


// KoreAzElRange: Class represents a straight line (no earth curvature) between two points.


public struct KoreAzElRange
{
    public double RangeM { get; set; }
    public double AzRads { get; set; }
    public double ElRads { get; set; }

    // Add properties to convert to/from degrees
    public double AzDegs
    {
        get { return AzRads * KoreConsts.RadsToDegsMultiplier; }
        set { AzRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    public double ElDegs
    {
        get { return ElRads * KoreConsts.RadsToDegsMultiplier; }
        set { ElRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    public double RangeKm
    {
        get { return RangeM * KoreWorldConsts.MetresToKmMultiplier; }
        set { RangeM = value * KoreWorldConsts.KmToMetresMultiplier; }
    }

    public double AltOffsetM
    {
        get { return Math.Sin(ElRads) * RangeM; }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Constructors
    // --------------------------------------------------------------------------------------------

    // Constructor accepts radians
    public KoreAzElRange(double rangeM, double azRads, double elRads)
    {
        this.RangeM = rangeM;
        this.AzRads = azRads;
        this.ElRads = elRads;
    }

    public KoreAzElRange(double heightM, double widthM, double depthM, double orientationDeg)
    {
        // Calculate azimuth from orientation and width/depth ratio
        double angleRad = Math.Atan2(widthM, depthM) + orientationDeg * KoreConsts.DegsToRadsMultiplier;

        // Wrap angle between 0 and 2*Pi
        AzRads = (angleRad + 2 * Math.PI) % (2 * Math.PI);

        // Calculate range (distance from origin) using Pythagorean theorem in 3D
        RangeM = Math.Sqrt(Math.Pow(heightM, 2) + Math.Pow(widthM, 2) + Math.Pow(depthM, 2));

        // Calculate elevation angle from height and range
        ElRads = Math.Asin(heightM / RangeM);
    }

    // Static property for a zero offset
    public static KoreAzElRange Zero
    {
        get { return new KoreAzElRange { RangeM = 0.0, AzRads = 0.0, ElRads = 0.0 }; }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Functions
    // --------------------------------------------------------------------------------------------

    public KoreAzElRange Scale(double scaleFactor)
    {
        return new KoreAzElRange(RangeM * scaleFactor, AzRads, ElRads);
    }

    public KoreAzElRange WithRangeM(double newRangeM)
    {
        return new KoreAzElRange(newRangeM, AzRads, ElRads);
    }

    public KoreAzElRange Inverse()
    {
        return new KoreAzElRange()
        {
            RangeM = this.RangeM,
            ElRads  = -this.ElRads,
            AzRads  = this.AzRads + Math.PI // 180 degrees in radians
        };
    }

    public KoreAzElRange SimpleInterpolation(KoreAzElRange toOffset, double fraction)
    {
        fraction = KoreValueUtils.LimitToRange(fraction, 0, 1);
        double invFraction = 1 - fraction;

        KoreAzElRange newOffset = new KoreAzElRange();
        newOffset.AzRads = KoreValueUtils.NormalizeAngle2Pi( (AzRads * invFraction) + (toOffset.AzRads * fraction) );
        newOffset.ElRads = (ElRads * invFraction) + (toOffset.ElRads * fraction);
        newOffset.RangeM = (RangeM * invFraction) + (toOffset.RangeM * fraction);

        return newOffset;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Conversion
    // --------------------------------------------------------------------------------------------

    public KoreXYZVector ToXYZ()
    {
        double x = RangeM * Math.Cos(ElRads) * Math.Sin(AzRads);
        double y = RangeM * Math.Sin(ElRads);
        double z = RangeM * Math.Cos(ElRads) * Math.Cos(AzRads);

        return new KoreXYZVector(x, y, z);
    }

    public static KoreAzElRange FromXYZ(KoreXYZVector xyz)
    {
        KoreAzElRange newOffset = new KoreAzElRange() {
            RangeM = xyz.Length,
            AzRads = Math.Atan2(xyz.X, xyz.Z),
            ElRads = Math.Asin(xyz.Y / xyz.Length)
        };

        return newOffset;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: String
    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        return string.Format($"(AzDegs:{AzDegs:F2}, ElDegs:{ElDegs:F2}, RangeM:{RangeM:F2})");
    }

}

