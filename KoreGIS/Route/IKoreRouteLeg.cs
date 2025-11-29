// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;

// Abstract base class for route legs. Provides common properties and helper
// functions that can be used by all leg types.
public abstract class IKoreRouteLeg
{
    // End points
    public KoreLLAPoint StartPoint { get; set; }
    public KoreLLAPoint EndPoint { get; set; }

    // Course information
    public KoreCourse StartCourse { get; protected set; } = KoreCourse.Zero;
    public KoreCourse EndCourse { get; protected set; } = KoreCourse.Zero;

    // Attitude information
    public KoreAttitude StartAttitude { get; protected set; } = KoreAttitude.Zero;
    public KoreAttitude EndAttitude { get; protected set; } = KoreAttitude.Zero;

    // Attitude delta information
    public KoreAttitudeDelta StartAttitudeDelta { get; protected set; } = KoreAttitudeDelta.Zero;
    public KoreAttitudeDelta EndAttitudeDelta { get; protected set; } = KoreAttitudeDelta.Zero;

    // Speed in m/s
    public double SpeedMps { get; set; } = KoreConsts.ArbitrarySmallDouble;

    // ---------------------------------------------------------------------
    // MARK: Distances
    // ---------------------------------------------------------------------

    // Straight line distance between the start and end points
    public virtual double GetStraightLineDistanceM() => StartPoint.CurvedDistanceToM(EndPoint);

    // Calculated distance along the leg path. Default is straight line distance.
    public virtual double GetCalculatedDistanceM() => GetStraightLineDistanceM();

    // ---------------------------------------------------------------------
    // MARK: Time
    // ---------------------------------------------------------------------

    public virtual double GetDurationS() => GetCalculatedDistanceM() / SpeedMps;

    // ---------------------------------------------------------------------
    // MARK: Position and derivatives
    // ---------------------------------------------------------------------

    public double LegTimeForFraction(double fraction) =>KoreNumericRange<double>.ZeroToOne.Apply(fraction) * GetDurationS();

    public abstract KoreLLAPoint PositionAtLegTime(double legtimeS);

    public KoreLLAPoint PositionAtLegFraction(double fraction) => PositionAtLegTime(LegTimeForFraction(fraction));

    public virtual KoreCourse CourseAtLegTime(double legtimeS) => StartCourse;
    public virtual KoreCourse CourseAtLegFraction(double fraction) => CourseAtLegTime(LegTimeForFraction(fraction));

    public virtual KoreAttitude AttitudeAtLegTime(double legtimeS) => StartAttitude;
    public virtual KoreAttitude AttitudeAtLegFraction(double fraction) => AttitudeAtLegTime(LegTimeForFraction(fraction));

    public virtual KoreAttitudeDelta AttitudeDeltaAtLegTime(double legtimeS) => StartAttitudeDelta;
    public virtual KoreAttitudeDelta AttitudeDeltaAtLegFraction(double fraction) => AttitudeDeltaAtLegTime(LegTimeForFraction(fraction));
}


