// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;


public struct KoreCourse
{
    public double SpeedMps; // Metres per second
    public double HeadingRads;
    public double ClimbRateMps;

    // --------------------------------------------------------------------------------------------

    public double SpeedKph
    {
        get { return SpeedMps * KoreWorldConsts.MPStoKPHMultiplier; }
        set { SpeedMps = value * KoreWorldConsts.KPHtoMPSMultiplier; }
    }

    public double HeadingDegs
    {
        get { return HeadingRads * KoreConsts.RadsToDegsMultiplier; }
        set { HeadingRads = (value % 360.0) * KoreConsts.DegsToRadsMultiplier; }
    }

    // --------------------------------------------------------------------------------------------

    public KoreCourse(double inSpeedKph, double inHeadingDegs, double inClimbRateMtrSec)
    {
        SpeedMps     = inSpeedKph * KoreWorldConsts.KPHtoMPSMultiplier;
        HeadingRads  = KoreValueUtils.NormalizeAngle2Pi(inHeadingDegs * KoreConsts.DegsToRadsMultiplier);
        ClimbRateMps = inClimbRateMtrSec;
    }

    public static KoreCourse Zero
    {
        get { return new KoreCourse { SpeedMps = 0.0, HeadingRads = 0.0, ClimbRateMps = 0.0 }; }
    }

    // --------------------------------------------------------------------------------------------

    public bool IsStationary()
    {
        return (SpeedMps < 0.001);
    }

    // --------------------------------------------------------------------------------------------

    public KoreRangeBearing OffsetForTime(double elapsedSeconds)
    {
        return new KoreRangeBearing(SpeedMps * elapsedSeconds, HeadingRads);
    }

    // Derive the course delta from two values and a duration
    public KoreCourseDelta DeltaForTime(double elapsedSeconds, KoreCourse newCourse)
    {
        double speedChange = (newCourse.SpeedMps - SpeedMps) / elapsedSeconds;
        double headingChange = ((newCourse.HeadingRads - HeadingRads) / elapsedSeconds) * KoreConsts.RadsToDegsMultiplier;
        return new KoreCourseDelta { SpeedChangeMpMps = speedChange, HeadingChangeClockwiseDegsSec = headingChange };
    }

    // Derive a new course from applying a duration for a period of time

    public KoreCourse PlusDeltaForTime(KoreCourseDelta delta, double elapsedSeconds)
    {
        double newSpeedMps = SpeedMps + delta.SpeedChangeMpMps * elapsedSeconds;
        double newHeadingDegs = HeadingDegs + (delta.HeadingChangeClockwiseDegsSec * -1.0 * elapsedSeconds);
        return new KoreCourse(newSpeedMps * KoreWorldConsts.MPStoKPHMultiplier, newHeadingDegs, ClimbRateMps);
    }

    // --------------------------------------------------------------------------------------------

    public KoreAzElRange ToPolarOffset(double elapsedSeconds)
    {
        double rangeM = SpeedMps * elapsedSeconds;
        return new KoreAzElRange(rangeM, HeadingRads, 0);
    }

    // --------------------------------------------------------------------------------------------

    // Lerp - for use cases where we move/smooth the course over time

    public static KoreCourse Lerp(KoreCourse start, KoreCourse end, double t)
    {
        double speedMps     = KoreValueUtils.Interpolate(start.SpeedMps,     end.SpeedMps,     t);
        double headingRads  = KoreValueUtils.Interpolate(start.HeadingRads,  end.HeadingRads,  t);
        double climbRateMps = KoreValueUtils.Interpolate(start.ClimbRateMps, end.ClimbRateMps, t);

        return new KoreCourse() { SpeedMps = speedMps, HeadingRads = headingRads, ClimbRateMps = climbRateMps };
    }

    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        string speedStr = (IsStationary()) ? "Stationary" : $"Speed:{SpeedKph:F2}Kph";

        return $"{speedStr}, Heading:{HeadingDegs:F2}Degs, ClimbRate:{ClimbRateMps:F2}Mps";
    }
}




