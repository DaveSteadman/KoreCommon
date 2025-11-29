// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;

public struct KoreAttitude
{
    public double RollClockwiseRads { get; set; } // +/- rads clockwise from straight ahead
    public double PitchUpRads { get; set; } // +/- rads up from straight ahead
    public double YawClockwiseRads { get; set; } // +/- rads clockwise from straight ahead

    // --------------------------------------------------------------------------------------------

    public double RollClockwiseDegs // +/- degs clockwise from straight ahead
    {
        get { return RollClockwiseRads * KoreConsts.RadsToDegsMultiplier; }
        set { RollClockwiseRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    public double PitchUpDegs       // +/- degs up from straight ahead
    {
        get { return PitchUpRads * KoreConsts.RadsToDegsMultiplier; }
        set { PitchUpRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    public double YawClockwiseDegs  // +/- degs clockwise from straight ahead
    {
        get { return YawClockwiseRads * KoreConsts.RadsToDegsMultiplier; }
        set { YawClockwiseRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    // --------------------------------------------------------------------------------------------

    public KoreAttitude(double r, double p, double y)
    {
        this.RollClockwiseRads = r;
        this.PitchUpRads = p;
        this.YawClockwiseRads = y;
    }

    public static KoreAttitude Zero
    {
        get { return new KoreAttitude { RollClockwiseRads = 0.0, PitchUpRads = 0.0, YawClockwiseRads = 0.0 }; }
    }

    public bool IsZero()
    {
        return (
            KoreValueUtils.IsZero(RollClockwiseRads) &&
            KoreValueUtils.IsZero(PitchUpRads) &&
            KoreValueUtils.IsZero(YawClockwiseRads)
        );
    }

    public override string ToString()
    {
        return $"RollClockwiseDegs:{RollClockwiseDegs:F2}, PitchUpDegs:{PitchUpDegs:F2}, YawClockwiseDegs:{YawClockwiseDegs:F2}";
    }

    // --------------------------------------------------------------------------------------------

    public KoreAttitude PlusDeltaForTime(KoreAttitudeDelta delta, double elapsedSeconds)
    {
        KoreAttitude retAtt = new KoreAttitude
        {
            RollClockwiseRads = RollClockwiseRads + delta.RollRateClockwiseRadsPerSec * elapsedSeconds,
            PitchUpRads = PitchUpRads + delta.PitchRateUpRadsPerSec * elapsedSeconds,
            YawClockwiseRads = YawClockwiseRads + delta.YawRateClockwiseRadsPerSec * elapsedSeconds,
        };

        return retAtt;
    }

    // --------------------------------------------------------------------------------------------

    public KoreAzElRange ToPolarOffset(double distance)
    {
        double roll = RollClockwiseRads;
        double pitch = PitchUpRads;
        double yaw = YawClockwiseRads;

        // Trigonometric calculations to account for the roll
        double azimuth = Math.Atan2(Math.Sin(yaw) * Math.Cos(roll) + Math.Cos(yaw) * Math.Sin(roll) * Math.Sin(pitch),
                                    Math.Cos(yaw) * Math.Cos(roll) - Math.Sin(yaw) * Math.Sin(roll) * Math.Sin(pitch));

        double elevation = Math.Asin(Math.Cos(yaw) * Math.Sin(pitch) + Math.Sin(yaw) * Math.Sin(roll) * Math.Cos(pitch));

        return new KoreAzElRange(azimuth, elevation, distance);
    }

}

