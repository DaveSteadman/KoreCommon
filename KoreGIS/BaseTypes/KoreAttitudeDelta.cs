// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;

public struct KoreAttitudeDelta
{
    public double RollRateClockwiseRadsPerSec { get; set; } // +/- rads clockwise from straight ahead
    public double PitchRateUpRadsPerSec { get; set; } // +/- rads up from straight ahead
    public double YawRateClockwiseRadsPerSec { get; set; } // +/- rads clockwise from straight ahead

    public double RollRateClockwiseDegsPerSec // +/- degs clockwise from straight ahead
    {
        get { return RollRateClockwiseRadsPerSec * KoreConsts.RadsToDegsMultiplier; }
        set { RollRateClockwiseRadsPerSec = value * KoreConsts.DegsToRadsMultiplier; }
    }

    public double PitchRateUpDegsPerSec       // +/- degs up from straight ahead
    {
        get { return PitchRateUpRadsPerSec * KoreConsts.RadsToDegsMultiplier; }
        set { PitchRateUpRadsPerSec = value * KoreConsts.DegsToRadsMultiplier; }
    }

    public double YawRateClockwiseDegsPerSec  // +/- degs clockwise from straight ahead
    {
        get { return YawRateClockwiseRadsPerSec * KoreConsts.RadsToDegsMultiplier; }
        set { YawRateClockwiseRadsPerSec = value * KoreConsts.DegsToRadsMultiplier; }
    }

    public KoreAttitudeDelta(double r, double p, double y)
    {
        this.RollRateClockwiseRadsPerSec = r;
        this.PitchRateUpRadsPerSec = p;
        this.YawRateClockwiseRadsPerSec = y;
    }

    // --------------------------------------------------------------------------------------------

    public static KoreAttitudeDelta Zero
    {
        get { return new KoreAttitudeDelta { RollRateClockwiseRadsPerSec = 0.0, PitchRateUpRadsPerSec = 0.0, YawRateClockwiseRadsPerSec = 0.0 }; }
    }

    public readonly bool IsZero()
    {
        bool rollZero = KoreValueUtils.IsZero(RollRateClockwiseRadsPerSec);
        bool pitchZero = KoreValueUtils.IsZero(PitchRateUpRadsPerSec);
        bool yawZero = KoreValueUtils.IsZero(YawRateClockwiseRadsPerSec);

        return rollZero && pitchZero && yawZero;
    }

    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        return string.Format(
            $"Roll(Clockwise):{RollRateClockwiseDegsPerSec:F2} deg/s, Pitch(Up):{PitchRateUpDegsPerSec:F2}deg/s, Yaw(Clockwise):{YawRateClockwiseDegsPerSec:F2}deg/s");
    }
}


