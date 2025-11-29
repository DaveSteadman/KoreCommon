// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;


// GC = Great Circle
// This class represents a great circle path between XYZ locations with a 0,0,0 reference point.

public class KoreGCPath
{
    public KoreXYZVector StartPos { get; private set; }
    public KoreXYZVector EndPos { get; private set; }

    private double RefAngleRads { get; set; } // Internal pre-calculated reference angle

    // Initialize the path with start and end positions
    public KoreGCPath(KoreLLAPoint startLLA, KoreLLAPoint endLLA)
    {
        this.StartPos = startLLA.ToXYZ();
        this.EndPos   = endLLA.ToXYZ();
        this.RefAngleRads = KoreXYZVectorOps.AngleBetweenRads(this.StartPos, this.EndPos);
    }

    public KoreGCPath(KoreLLAPoint startLLA, double heading)
    {
        // Extrapolate point along heading to create second LLA point.
        // Assuming an arbitrary distance to create a meaningful second point.
        double arbitraryDistance = 10000; // 10 km for instance
        KoreLLAPoint endLLA = startLLA.PlusRangeBearing(new KoreRangeBearing(arbitraryDistance, heading));

        // Call other constructor to create object
        this.StartPos = startLLA.ToXYZ();
        this.EndPos = endLLA.ToXYZ();
        this.RefAngleRads = KoreXYZVectorOps.AngleBetweenRads(this.StartPos, this.EndPos);
    }

    // Calculate position along the path given a fraction
    public KoreXYZVector PositionAtFractionOfRoute(double fraction)
    {
        // Handle edge cases
        if (RefAngleRads < KoreConsts.ArbitrarySmallDouble)
            return StartPos;

        // Spherical Linear Interpolation (SLERP)
        return KoreXYZVectorOps.Slerp(StartPos, EndPos, fraction);
    }

    public KoreXYZVector PositionAtFractionOfFullCircle(double fraction)
    {
        // Normalize the fraction to ensure it's between 0 and 1
        fraction = fraction % 1;

        // Scale the fraction to the full circle angle
        double angleForFullCircle = fraction * 2 * Math.PI;

        // Scale the angle relative to the reference angle
        double scaledAngle = angleForFullCircle / RefAngleRads;

        // Use the Slerp function for the scaled angle
        return KoreXYZVectorOps.Slerp(StartPos, EndPos, scaledAngle);
    }

    public KoreXYZVector PositionAtDistance(double distAlongRouteM)
    {
        // Find the fraction of the path covered by the given distance
        double totalPathDistance = this.PathDistance();
        double fraction = distAlongRouteM / totalPathDistance;

        // Calculate the position at the given fraction
        return this.PositionAtFractionOfRoute(fraction);
    }

    // Calculate the total distance of the path
    public double PathDistance()
    {
        // Assuming radius is the same for both points or taking the average if different
        double radius = (StartPos.Magnitude + EndPos.Magnitude) / 2;
        return radius * RefAngleRads;
    }
}

