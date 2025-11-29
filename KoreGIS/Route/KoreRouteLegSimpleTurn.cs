// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;

// Simple turn leg following a circular arc defined by a start point,
// a turn centre point and an angular change.
public class KoreRouteLegSimpleTurn : IKoreRouteLeg
{
    // The pre-determined centre point of the turn arc. The distance from the start point to this point defines the turn radius.
    public KoreLLAPoint TurnPoint { get; set; }

    // Delta angle. Positive for right turns, negative for left turns, from the perspective of the platform.
    // Right turns are clockwise, left turns are counter-clockwise.
    public double DeltaAngleRads { get; set; }
    public double DeltaAngleDegs
    {
        get => DeltaAngleRads * KoreConsts.RadsToDegsMultiplier;
        set => DeltaAngleRads = value * KoreConsts.DegsToRadsMultiplier;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Constructors
    // --------------------------------------------------------------------------------------------

    public KoreRouteLegSimpleTurn(KoreLLAPoint startPoint, KoreLLAPoint turnPoint, double deltaAngleRads, double speedMps)
    {
        StartPoint = startPoint;
        TurnPoint = turnPoint;
        DeltaAngleRads = deltaAngleRads;
        SpeedMps = speedMps;
        Setup();
    }

    private void Setup()
    {
        double radius = TurnRadiusM();
        double startBearing = TurnPoint.BearingToRads(StartPoint);
        double endBearing = startBearing + DeltaAngleRads;

        EndPoint = TurnPoint.PlusRangeBearing(new KoreRangeBearing(radius, endBearing));

        double startHeading = startBearing + (DeltaAngleRads > 0 ? Math.PI / 2 : -Math.PI / 2);
        double endHeading = endBearing + (DeltaAngleRads > 0 ? Math.PI / 2 : -Math.PI / 2);

        StartCourse = new KoreCourse() { SpeedMps = SpeedMps, HeadingRads = startHeading, ClimbRateMps = 0 };
        EndCourse = new KoreCourse() { SpeedMps = SpeedMps, HeadingRads = endHeading, ClimbRateMps = 0 };

        StartAttitude = KoreAttitude.Zero;
        EndAttitude = KoreAttitude.Zero;
        StartAttitudeDelta = KoreAttitudeDelta.Zero;
        EndAttitudeDelta = KoreAttitudeDelta.Zero;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Setup Calls
    // --------------------------------------------------------------------------------------------

    // Functions to take a few defining parameters and create the rest of the leg object

    public void SetupRoute(KoreLLAPoint startPoint, double turnRadiusM, double deltaAngleRads, double speedMps)
    {
        StartPoint = startPoint;
        TurnPoint = FindTurnPoint(startPoint, new KoreCourse() { SpeedMps = speedMps, HeadingRads = StartPoint.BearingToRads(EndPoint) }, turnRadiusM, deltaAngleRads);
        DeltaAngleRads = deltaAngleRads;
        SpeedMps = speedMps;
        Setup();
    }


    // Radius from centre to start
    public double TurnRadiusM() => TurnPoint.CurvedDistanceToM(StartPoint);

    // Turn fraction of a whole circle (2PiRads)
    //public double TurnFraction() => Math.Abs(DeltaAngleRads) / (2 * Math.PI);
    public double TurnFraction() => Math.Abs(DeltaAngleRads) * TurnRadiusM();

    // Turn length, calculated as 2PiR * TurnFraction
    public double TurnLengthM() => (2 * Math.PI * TurnRadiusM()) * TurnFraction();

    public double TurnDurationSecs() => (SpeedMps < KoreConsts.ArbitrarySmallDouble) ? 0 : TurnLengthM() / SpeedMps;


    public override double GetDurationS() => TurnDurationSecs();

    // ---------------------------------------------------------------------
    // MARK: Distances
    // ---------------------------------------------------------------------


    public override double GetCalculatedDistanceM() => TurnLengthM();


    // ---------------------------------------------------------------------
    // MARK: Time
    // ---------------------------------------------------------------------

    //public override double GetDurationS() => GetCalculatedDistanceM() / SpeedMps;

    // ---------------------------------------------------------------------
    // MARK: Position and derivatives
    // ---------------------------------------------------------------------

    private KoreLLAPoint PositionAtTurnFraction(double fraction)
    {
        fraction =KoreNumericRange<double>.ZeroToOne.Apply(fraction);
        double startBearing = TurnPoint.BearingToRads(StartPoint);
        double angle = startBearing + DeltaAngleRads * fraction;
        return TurnPoint.PlusRangeBearing(new KoreRangeBearing(TurnRadiusM(), angle));
    }

    public override KoreLLAPoint PositionAtLegTime(double legtimeS)
    {
        double frac = (GetDurationS() > 0) ? legtimeS / GetDurationS() : 0;
        return PositionAtTurnFraction(frac);
    }

    public override KoreCourse CourseAtLegTime(double legtimeS)
    {
        double frac = (GetDurationS() > 0) ? legtimeS / GetDurationS() : 0;
        frac =KoreNumericRange<double>.ZeroToOne.Apply(frac);
        double startBearing = TurnPoint.BearingToRads(StartPoint);
        double angle = startBearing + DeltaAngleRads * frac;
        double heading = angle + (DeltaAngleRads > 0 ? Math.PI / 2 : -Math.PI / 2);
        return new KoreCourse() { SpeedMps = SpeedMps, HeadingRads =KoreNumericRange<double>.ZeroToTwoPiRadians.Apply(heading), ClimbRateMps = 0 };
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Static Helper Methods
    // --------------------------------------------------------------------------------------------

    // Given a previous leg values (position and course), a desired turn radius, and a delta angle (sign for left/right),
    // compute the center point of the turn arc (the turn point).
    // public static KoreLLAPoint FindTurnPoint(KoreLLAPoint prevLegStartPoint, KoreLLAPoint prevLegEndPoint, double turnRadiusM, double deltaAngleRads)

    public static KoreLLAPoint FindTurnPoint(KoreLLAPoint startPoint, KoreCourse startCourse, double turnRadiusM, double deltaAngleRads)
    {
        double legDirectionRads = startCourse.HeadingRads;

        // Determine the perpendicular direction for the turn center - +ve is pilot turning right X radians, -ve if left.
        double perpendicularAngleRads = (deltaAngleRads > 0) ? (Math.PI / 2) : -(Math.PI / 2);

        // Combined angle from the start point to the turn point
        double startPointBearingToTurnPoint =KoreNumericRange<double>.ZeroToTwoPiRadians.Apply(legDirectionRads + perpendicularAngleRads);

        // The turn center is offset from the previous leg end point by the turn radius in the perpendicular direction
        KoreRangeBearing rbToTurnPoint = new KoreRangeBearing(turnRadiusM, startPointBearingToTurnPoint);
        return startPoint.PlusRangeBearing(rbToTurnPoint);
    }

}



