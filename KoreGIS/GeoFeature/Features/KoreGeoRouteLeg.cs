// <fileheader>

#nullable enable

using System;
using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// --------------------------------------------------------------------------------------------
// MARK: Base Route Leg
// --------------------------------------------------------------------------------------------

// Base class for all route leg types
// A route leg represents a segment of a path that can be drawn or traversed
public abstract class KoreGeoRouteLeg : KoreGeoFeature
{
    public KoreLLPoint  StartPoint  { get; set; }
    public KoreLLPoint  EndPoint    { get; set; }
    public double       LineWidth   { get; set; } = 1.0;
    public KoreColorRGB Color       { get; set; } = KoreColorRGB.Black;
    public KoreLLBox?   BoundingBox { get; protected set; }

    // Generate points along the route leg for rendering
    // <param name="numPoints">Number of points to generate</param>
    // <returns>List of points along the route</returns>
    public abstract List<KoreLLPoint> GeneratePoints(int numPoints);

    // Get a point at a specific fraction (0.0 to 1.0) along the route leg
    public abstract KoreLLPoint PointAtFraction(double fraction);

    // Calculate the bounding box for this route leg
    public abstract void CalcBoundingBox();
}

// --------------------------------------------------------------------------------------------
// MARK: Straight Line Leg
// --------------------------------------------------------------------------------------------

// A straight line segment between two points (rhumb line)
public class KoreGeoRouteLegStraight : KoreGeoRouteLeg
{
    public override List<KoreLLPoint> GeneratePoints(int numPoints)
    {
        if (numPoints < 2)
            numPoints = 2;

        var points = new List<KoreLLPoint>();

        for (int i = 0; i < numPoints; i++)
        {
            double fraction = i / (double)(numPoints - 1);
            points.Add(PointAtFraction(fraction));
        }

        return points;
    }

    // --------------------------------------------------------------------------------------------

    public override KoreLLPoint PointAtFraction(double fraction)
    {
        fraction = Math.Clamp(fraction, 0.0, 1.0);

        // Linear interpolation in radians
        double latRads = StartPoint.LatRads + fraction * (EndPoint.LatRads - StartPoint.LatRads);
        double lonRads = StartPoint.LonRads + fraction * (EndPoint.LonRads - StartPoint.LonRads);

        return new KoreLLPoint { LatRads = latRads, LonRads = lonRads };
    }

    // --------------------------------------------------------------------------------------------

    public override void CalcBoundingBox()
    {
        var points = new List<KoreLLPoint> { StartPoint, EndPoint };
        BoundingBox = KoreLLBox.FromList(points);
    }
}

// --------------------------------------------------------------------------------------------
// MARK: Great Circle Leg
// --------------------------------------------------------------------------------------------

// A great circle route between two points (shortest path on a sphere)
// Used for aviation routes and long-distance paths
public class KoreGeoRouteLegGreatCircle : KoreGeoRouteLeg
{
    public override List<KoreLLPoint> GeneratePoints(int numPoints)
    {
        if (numPoints < 2)
            numPoints = 2;

        var points = new List<KoreLLPoint>();

        for (int i = 0; i < numPoints; i++)
        {
            double fraction = i / (double)(numPoints - 1);
            points.Add(PointAtFraction(fraction));
        }

        return points;
    }

    // --------------------------------------------------------------------------------------------

    public override KoreLLPoint PointAtFraction(double fraction)
    {
        fraction = Math.Clamp(fraction, 0.0, 1.0);

        // Use spherical linear interpolation (slerp)
        // Convert to XYZ, slerp, convert back to lat/lon
        var startXYZ = StartPoint.ToXYZ(1.0); // Unit sphere
        var endXYZ = EndPoint.ToXYZ(1.0);

        // Calculate angle between points
        double dot = startXYZ.X * endXYZ.X + startXYZ.Y * endXYZ.Y + startXYZ.Z * endXYZ.Z;
        dot = Math.Clamp(dot, -1.0, 1.0);
        double angle = Math.Acos(dot);

        // Avoid division by zero for very close points
        if (Math.Abs(angle) < 1e-10)
            return StartPoint;

        // Slerp formula
        double sinAngle = Math.Sin(angle);
        double a = Math.Sin((1.0 - fraction) * angle) / sinAngle;
        double b = Math.Sin(fraction * angle) / sinAngle;

        var interpolatedXYZ = new KoreXYZVector(
            a * startXYZ.X + b * endXYZ.X,
            a * startXYZ.Y + b * endXYZ.Y,
            a * startXYZ.Z + b * endXYZ.Z
        );

        return KoreLLPoint.FromXYZ(interpolatedXYZ);
    }

    // --------------------------------------------------------------------------------------------

    public override void CalcBoundingBox()
    {
        // For great circles, need to sample points to get accurate bounds
        // especially important when the route crosses poles or date line
        var samplePoints = GeneratePoints(20);
        BoundingBox = KoreLLBox.FromList(samplePoints);
    }
}

// --------------------------------------------------------------------------------------------
// MARK: Bezier Curve Leg
// --------------------------------------------------------------------------------------------

// A bezier curve route with control points
// Useful for smooth curved paths and artistic routes
public class KoreGeoRouteLegBezier : KoreGeoRouteLeg
{
    // Control points for the bezier curve (must be 3, 4, or 5 points total)
    // First point is StartPoint, last point is EndPoint
    public List<KoreLLPoint> ControlPoints { get; set; } = new List<KoreLLPoint>();

    public override List<KoreLLPoint> GeneratePoints(int numPoints)
    {
        if (numPoints < 2)
            numPoints = 2;

        if (ControlPoints.Count < 3 || ControlPoints.Count > 5)
            throw new InvalidOperationException($"Bezier curves require 3-5 control points, got {ControlPoints.Count}");

        var points = new List<KoreLLPoint>();

        for (int i = 0; i < numPoints; i++)
        {
            double fraction = i / (double)(numPoints - 1);
            points.Add(PointAtFraction(fraction));
        }

        return points;
    }

    // --------------------------------------------------------------------------------------------

    public override KoreLLPoint PointAtFraction(double fraction)
    {
        fraction = Math.Clamp(fraction, 0.0, 1.0);

        if (ControlPoints.Count < 3 || ControlPoints.Count > 5)
            throw new InvalidOperationException($"Bezier curves require 3-5 control points, got {ControlPoints.Count}");

        // Create separate arrays for lat and lon coordinates
        var latArray = new KoreNumeric1DArray<double>(ControlPoints.ConvertAll(p => p.LatRads).ToArray());
        var lonArray = new KoreNumeric1DArray<double>(ControlPoints.ConvertAll(p => p.LonRads).ToArray());

        // Calculate bezier point for each dimension
        double latRads = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(fraction, latArray);
        double lonRads = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(fraction, lonArray);

        return new KoreLLPoint { LatRads = latRads, LonRads = lonRads };
    }

    // --------------------------------------------------------------------------------------------

    public override void CalcBoundingBox()
    {
        // Sample the curve to get accurate bounds
        var samplePoints = GeneratePoints(30);
        BoundingBox = KoreLLBox.FromList(samplePoints);
    }

    // Helper to set control points including start and end
    public void SetControlPoints(List<KoreLLPoint> points)
    {
        if (points.Count < 3 || points.Count > 5)
            throw new ArgumentException($"Bezier curves require 3-5 control points, got {points.Count}");

        ControlPoints = new List<KoreLLPoint>(points);
        StartPoint = points[0];
        EndPoint = points[points.Count - 1];
    }
}

// --------------------------------------------------------------------------------------------
// MARK: Flexible Join Leg
// --------------------------------------------------------------------------------------------

// A smooth bezier curve that automatically joins two legs
// Calculates control points based on the exit direction from the previous leg
// and entry direction to the next leg, creating a smooth transition
public class KoreGeoRouteLegFlexibleJoin : KoreGeoRouteLeg
{
    // The previous leg whose exit direction will be matched
    public KoreGeoRouteLeg? PreviousLeg { get; set; }

    // The next leg whose entry direction will be matched
    public KoreGeoRouteLeg? NextLeg { get; set; }

    // Distance in meters for first control point
    // Controls how far the first control point is from the start
    // Larger values create wider curves at the entry
    public double StartControlPointDistanceM { get; set; } = 50000.0; // Default 50km

    // Distance in meters for second control point
    // Controls how far the second control point is from the end
    // Larger values create wider curves at the exit
    public double EndControlPointDistanceM { get; set; } = 50000.0; // Default 50km

    private List<KoreLLPoint> _calculatedControlPoints = new List<KoreLLPoint>();

    // Calculate the control points based on the previous and next leg directions
    public void CalculateControlPoints()
    {
        if (PreviousLeg == null)
            throw new InvalidOperationException("PreviousLeg must be set before calculating control points");

        if (NextLeg == null)
            throw new InvalidOperationException("NextLeg must be set before calculating control points");

        // Get exit direction from previous leg (direction at end)
        var prevExitPoint = PreviousLeg.PointAtFraction(0.99); // Very close to end
        var prevEndPoint = PreviousLeg.EndPoint;

        // Calculate exit bearing (direction leaving the previous leg)
        double exitBearingRads = prevExitPoint.BearingToRads(prevEndPoint);

        // Get entry direction to next leg (direction at start)
        var nextStartPoint = NextLeg.StartPoint;
        var nextEntryPoint = NextLeg.PointAtFraction(0.01); // Very close to start

        // Calculate entry bearing (direction entering the next leg)
        double entryBearingRads = nextStartPoint.BearingToRads(nextEntryPoint);

        // First control point: offset from start in exit direction using range/bearing
        var controlPoint1 = StartPoint.PlusRangeBearing(new KoreRangeBearing
        {
            RangeM = StartControlPointDistanceM,
            BearingRads = exitBearingRads
        });

        // Second control point: offset backwards from end (opposite of entry direction)
        // We go backwards from the end point, so we add PI to reverse the bearing
        var controlPoint2 = EndPoint.PlusRangeBearing(new KoreRangeBearing
        {
            RangeM = EndControlPointDistanceM,
            BearingRads = entryBearingRads + Math.PI // Reverse direction
        });

        // Create 4-point bezier curve: start, control1, control2, end
        _calculatedControlPoints = new List<KoreLLPoint>
        {
            StartPoint,
            controlPoint1,
            controlPoint2,
            EndPoint
        };
    }

    public override List<KoreLLPoint> GeneratePoints(int numPoints)
    {
        if (numPoints < 2)
            numPoints = 2;

        if (_calculatedControlPoints.Count != 4)
            CalculateControlPoints();

        var points = new List<KoreLLPoint>();

        for (int i = 0; i < numPoints; i++)
        {
            double fraction = i / (double)(numPoints - 1);
            points.Add(PointAtFraction(fraction));
        }

        return points;
    }

    public override KoreLLPoint PointAtFraction(double fraction)
    {
        fraction = Math.Clamp(fraction, 0.0, 1.0);

        if (_calculatedControlPoints.Count != 4)
            CalculateControlPoints();

        // Create separate arrays for lat and lon coordinates
        var latArray = new KoreNumeric1DArray<double>(_calculatedControlPoints.ConvertAll(p => p.LatRads).ToArray());
        var lonArray = new KoreNumeric1DArray<double>(_calculatedControlPoints.ConvertAll(p => p.LonRads).ToArray());

        // Calculate bezier point for each dimension
        double latRads = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(fraction, latArray);
        double lonRads = KoreNumeric1DArrayOps<double>.CalculateBezierPoint(fraction, lonArray);

        return new KoreLLPoint { LatRads = latRads, LonRads = lonRads };
    }

    public override void CalcBoundingBox()
    {
        // Sample the curve to get accurate bounds
        var samplePoints = GeneratePoints(30);
        BoundingBox = KoreLLBox.FromList(samplePoints);
    }

    // Get the calculated control points for debugging/visualization
    public List<KoreLLPoint> GetControlPoints()
    {
        if (_calculatedControlPoints.Count != 4)
            CalculateControlPoints();

        return new List<KoreLLPoint>(_calculatedControlPoints);
    }
}

