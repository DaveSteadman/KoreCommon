// <fileheader>

// KoreRouteLegPointToPoint: A class that has a start and end point for straight line route leg, along with speed.

using System;

using KoreCommon;

namespace KoreGIS;

public class KoreRouteLegLine : IKoreRouteLeg
{
    //private readonly double speedMps;

    // --------------------------------------------------------------------------------------------
    // MARK: Constructors
    // --------------------------------------------------------------------------------------------

    // CReate a route leg from two points and a speed.
    public KoreRouteLegLine(KoreLLAPoint startPoint, KoreLLAPoint endPoint, double speedMPS)
    {
        SetupRoute(startPoint, endPoint, speedMPS);
    }

    // Create a route leg from a start point, a range bearing offset and a speed.
    public KoreRouteLegLine(KoreLLAPoint startPoint, KoreRangeBearing rbLeg, double speedMPS)
    {
        SetupRoute(startPoint, rbLeg, speedMPS);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Setup Methods
    // --------------------------------------------------------------------------------------------

    // From a start point, an end point and a speed, set up the remaining route leg properties.

    private void SetupRoute(KoreLLAPoint startPoint, KoreLLAPoint endPoint, double speedMPS)
    {
        // Calculate the course and distance between the two points
        StartPoint = startPoint;
        EndPoint = endPoint;
        SpeedMps = speedMPS;

        KoreRangeBearing rb = startPoint.RangeBearingTo(endPoint);

        KoreCourse course = new KoreCourse()
        {
            SpeedMps = speedMPS,
            HeadingDegs = rb.BearingDegs,
            ClimbRateMps = 0
        };

        // Set the course and distance
        StartCourse = course;
        EndCourse = course;

        // Set the attitude and attitude delta
        StartAttitude = KoreAttitude.Zero;
        EndAttitude = KoreAttitude.Zero;
        StartAttitudeDelta = KoreAttitudeDelta.Zero;
        EndAttitudeDelta = KoreAttitudeDelta.Zero;
    }

    public void SetupRoute(KoreLLAPoint startPoint, KoreRangeBearing rbLeg, double speedMPS)
    {
        StartPoint = startPoint;
        EndPoint = startPoint.PlusRangeBearing(rbLeg);
        SpeedMps = speedMPS;

        // Calculate the course based on the range bearing
        KoreCourse course = new KoreCourse()
        {
            SpeedMps = speedMPS,
            HeadingDegs = rbLeg.BearingDegs,
            ClimbRateMps = 0
        };
        StartCourse = course;
        EndCourse = course;

        StartAttitude = KoreAttitude.Zero;
        EndAttitude = KoreAttitude.Zero;
        StartAttitudeDelta = KoreAttitudeDelta.Zero;
        EndAttitudeDelta = KoreAttitudeDelta.Zero;
    }

    // --------------------------------------------------------------------------------------------
    // Public Methods
    // --------------------------------------------------------------------------------------------

    public override double GetCalculatedDistanceM() => StartPoint.CurvedDistanceToM(EndPoint);

    public override double GetDurationS()
    {
        double dist = GetCalculatedDistanceM();
        return (SpeedMps < KoreConsts.ArbitrarySmallDouble) ? 0 : dist / SpeedMps;
    }

    public override KoreLLAPoint PositionAtLegTime(double legtimeS)
    {
        double dist = SpeedMps * legtimeS;
        double frac = (GetCalculatedDistanceM() > 0) ? dist / GetCalculatedDistanceM() : 0;
        frac =KoreNumericRange<double>.ZeroToOne.Apply(frac);
        return KoreLLAPointOps.RhumbLineInterpolation(StartPoint, EndPoint, frac);
    }
}




