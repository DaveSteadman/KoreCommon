// <fileheader>

// KoreXYArcSegmentBox: Class representing a 2D "box", based on an inner and outer radius, and start and end angles.
// Will have operations around the creation and manipulation of these boxes.

// Design Decisions:
// - Zero angle is "east" (3 o'clock) and angles increase anti-clockwise.
// - Anything beyond the core responibilites will be in a separate "operations" class.
// - Class will be immutable, as operations will return new instances.

namespace KoreCommon;

public struct KoreXYAnnularSector
{
    public KoreXYVector Center   { get; }
    public double InnerRadius    { get; }
    public double OuterRadius    { get; }
    public double StartAngleRads { get; }
    public double DeltaAngleRads { get; }

    // --------------------------------------------------------------------------------------------
    // Read only / derived attributes (increasing complexity order)
    // --------------------------------------------------------------------------------------------

    public double EndAngleRads   { get { return StartAngleRads + DeltaAngleRads; } }

    public double StartAngleDegs { get { return KoreValueUtils.RadsToDegs(StartAngleRads); } }
    public double DeltaAngleDegs { get { return KoreValueUtils.RadsToDegs(DeltaAngleRads); } }
    public double EndAngleDegs   { get { return KoreValueUtils.RadsToDegs(EndAngleRads); } }

    public KoreXYVector StartInnerPoint { get { return KoreXYVectorOps.OffsetPolar(Center, InnerRadius, StartAngleRads); } }
    public KoreXYVector EndInnerPoint   { get { return KoreXYVectorOps.OffsetPolar(Center, InnerRadius, EndAngleRads); } }
    public KoreXYVector StartOuterPoint { get { return KoreXYVectorOps.OffsetPolar(Center, OuterRadius, StartAngleRads); } }
    public KoreXYVector EndOuterPoint   { get { return KoreXYVectorOps.OffsetPolar(Center, OuterRadius, EndAngleRads); } }

    public KoreXYLine StartInnerOuterLine { get { return new KoreXYLine(StartInnerPoint, StartOuterPoint); } }
    public KoreXYLine EndInnerOuterLine   { get { return new KoreXYLine(EndInnerPoint, EndOuterPoint); } }

    public KoreXYCircle InnerCircle { get { return new KoreXYCircle(Center, InnerRadius); } }
    public KoreXYCircle OuterCircle { get { return new KoreXYCircle(Center, OuterRadius); } }

    public KoreXYArc InnerArc { get { return new KoreXYArc(Center, InnerRadius, StartAngleRads, DeltaAngleRads); } }
    public KoreXYArc OuterArc { get { return new KoreXYArc(Center, OuterRadius, StartAngleRads, DeltaAngleRads); } }

    // --------------------------------------------------------------------------------------------
    // Constructor
    // --------------------------------------------------------------------------------------------

    public KoreXYAnnularSector(KoreXYVector center, double innerRadius, double outerRadius, double startAngleRads, double deltaAngleRads)
    {
        // Swap the inner and outer radii if they are the wrong way around
        if (innerRadius > outerRadius)
            (innerRadius, outerRadius) = (outerRadius, innerRadius);

        Center         = center;
        InnerRadius    = innerRadius;
        OuterRadius    = outerRadius;
        StartAngleRads = startAngleRads;
        DeltaAngleRads = deltaAngleRads;
    }

    public KoreXYAnnularSector(KoreXYAnnularSector arc)
    {
        Center         = arc.Center;
        InnerRadius    = arc.InnerRadius;
        OuterRadius    = arc.OuterRadius;
        StartAngleRads = arc.StartAngleRads;
        DeltaAngleRads = arc.DeltaAngleRads;
    }

    // --------------------------------------------------------------------------------------------
    // Public methods
    // --------------------------------------------------------------------------------------------

    public KoreXYAnnularSector offset(double x, double y)
    {
        return new KoreXYAnnularSector(Center.Offset(x, y), InnerRadius, OuterRadius, StartAngleRads, DeltaAngleRads);
    }

    // Check if a point is within the arc box, first by checking distance against the two radii, then by checking the angle.

    public bool Contains(KoreXYVector checkPos)
    {
        // Containing the point is three checks:
        // 1 - Its inside the outer radius
        // 2 - Its outside the inner radius
        // 3 - Its within the start and end angles
        bool isInsideOuterRadius = OuterCircle.Contains(checkPos);
        if (!isInsideOuterRadius)
            return false;

        bool isOutsideInnerRadius = !InnerCircle.Contains(checkPos);
        if (!isOutsideInnerRadius)
            return false;

        bool isWithinAngles = InnerArc.ContainsAngle(Center.AngleToRads(checkPos));
        if (!isWithinAngles)
            return false;

        return true;
    }

    // --------------------------------------------------------------------------------------------
    // Operator overloads
    // --------------------------------------------------------------------------------------------


}