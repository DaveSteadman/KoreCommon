// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;


public struct KoreCourseDelta
{
    public double SpeedChangeMpMps; // Metres per metres per second
    public double HeadingChangeClockwiseDegsSec;

    // --------------------------------------------------------------------------------------------

    public static KoreCourseDelta Zero
    {
        get { return new KoreCourseDelta { SpeedChangeMpMps = 0.0, HeadingChangeClockwiseDegsSec = 0.0 }; }
    }

    public readonly bool IsZero()
    {
        bool speedZero         = KoreValueUtils.IsZero(SpeedChangeMpMps);
        bool headingChangeZero = KoreValueUtils.IsZero(HeadingChangeClockwiseDegsSec);

        return speedZero && headingChangeZero;
    }
    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        if (IsZero()) return "Zero Delta";

        return $"SpeedChangeMpMps: {SpeedChangeMpMps:F3}, HeadingChangeClockwiseDegsSec: {HeadingChangeClockwiseDegsSec:F3}";
    }
}

