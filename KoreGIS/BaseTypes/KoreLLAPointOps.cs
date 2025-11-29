// <fileheader>

using System;
using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;


// KoreLLAPointOps: A static class to hold operations on KoreLLAPoint objects that are not part of its
// core responsibilites. This class is static, all operations are stateless and return a new object.

// Design Decisions:
// - The LLA Point code uses a struct rather than an immutable class, as the constructor options with flexible units
//   are simply too useful. We rely on the struct's pass by value to avoid issues with mutability.

public static class KoreLLAPointOps
{
    public static KoreLLAPoint RhumbLineInterpolation(KoreLLAPoint fromPoint, KoreLLAPoint toPoint, double fraction)
    {
        // This is a simple linear interpolation of the lat and lon, and a weighted average of the altitudes.
        // The fraction is the fraction of the distance from the fromPoint to the toPoint.

        double latDegs = fromPoint.LatDegs + (toPoint.LatDegs - fromPoint.LatDegs) * fraction;
        double lonDegs = fromPoint.LonDegs + (toPoint.LonDegs - fromPoint.LonDegs) * fraction;
        double alt     = fromPoint.AltMslM + (toPoint.AltMslM - fromPoint.AltMslM) * fraction;

        return new KoreLLAPoint() { LatDegs = latDegs, LonDegs = lonDegs, AltMslM = alt };
    }

    // --------------------------------------------------------------------------------------------

    public static List<KoreLLAPoint> DividedRhumbLine(KoreLLAPoint fromPoint, KoreLLAPoint toPoint, int returnedListCount)
    {
        // Ensure the list count is at least 2, as the list needs to include both endpoints.
        if (returnedListCount < 2)
        {
            throw new ArgumentException("returnedListCount must be 2 or greater.");
        }

        List<KoreLLAPoint> result = new List<KoreLLAPoint>();

        // Add the first point (fromPoint)
        result.Add(fromPoint);

        // Calculate the step fraction for each interpolation point
        double stepFraction = 1.0 / (returnedListCount - 1);

        // Interpolate points along the rhumb line
        for (int i = 1; i < returnedListCount - 1; i++)
        {
            double fraction = i * stepFraction;
            KoreLLAPoint interpolatedPoint = RhumbLineInterpolation(fromPoint, toPoint, fraction);
            result.Add(interpolatedPoint);
        }

        // Add the last point (toPoint)
        result.Add(toPoint);

        return result;
    }

    // --------------------------------------------------------------------------------------------

    // Usage: List<KoreLLAPoint> pointList = KoreLLAPointOps.DividedGCLine(fromPoint, toPoint, 10)

    public static List<KoreLLAPoint> DividedGCLine(KoreLLAPoint fromPoint, KoreLLAPoint toPoint, int returnedListCount)
    {
        // Ensure the list count is at least 2, as the list needs to include both endpoints.
        if (returnedListCount < 2)
        {
            throw new ArgumentException("returnedListCount must be 2 or greater.");
        }

        List<KoreLLAPoint> result = new List<KoreLLAPoint>();

        // Add the first point (fromPoint)
        result.Add(fromPoint);

        // Calculate the step fraction for each interpolation point
        double stepFraction = 1.0 / (returnedListCount - 1);

        // Interpolate points along the rhumb line
        for (int i = 1; i < returnedListCount - 1; i++)
        {
            double fraction = i * stepFraction;
            KoreLLAPoint interpolatedPoint = GreatCircleInterpolation(fromPoint, toPoint, fraction);
            result.Add(interpolatedPoint);
        }

        // Add the last point (toPoint)
        result.Add(toPoint);

        return result;
    }

    // --------------------------------------------------------------------------------------------

    public static KoreLLAPoint StraightLineInterpolation(KoreLLAPoint fromPoint, KoreLLAPoint toPoint, double fraction)
    {
        KoreXYZVector fromXYZ   = fromPoint.ToXYZ();
        KoreXYZVector toXYZ     = toPoint.ToXYZ();
        KoreXYZVector interpXYZ = KoreXYZVectorOps.Lerp(fromXYZ, toXYZ, fraction);

        return KoreLLAPoint.FromXYZ(interpXYZ);
    }

    // --------------------------------------------------------------------------------------------

    // Usage: KoreLLAPoint result = KoreLLAPointOps.GreatCircleInterpolation(pointA, pointB, 0.5);
    public static KoreLLAPoint GreatCircleInterpolation(KoreLLAPoint fromPoint, KoreLLAPoint toPoint, double fraction)
    {
        // Convert latitude and longitude from degrees to radians
        double lat1 = fromPoint.LatRads;
        double lon1 = fromPoint.LonRads;
        double lat2 = toPoint.LatRads;
        double lon2 = toPoint.LonRads;

        // Compute spherical interpolation
        double delta = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((lat2 - lat1) / 2), 2) +
                                              Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin((lon2 - lon1) / 2), 2)));
        double A = Math.Sin((1 - fraction) * delta) / Math.Sin(delta);
        double B = Math.Sin(fraction * delta) / Math.Sin(delta);

        double x = A * Math.Cos(lat1) * Math.Cos(lon1) + B * Math.Cos(lat2) * Math.Cos(lon2);
        double y = A * Math.Cos(lat1) * Math.Sin(lon1) + B * Math.Cos(lat2) * Math.Sin(lon2);
        double z = A * Math.Sin(lat1) + B * Math.Sin(lat2);

        double newLatRads = Math.Atan2(z, Math.Sqrt(x * x + y * y));
        double newLonRads = Math.Atan2(y, x);

        // Convert the interpolated point back to degrees
        double newLatDegs = newLatRads * (180.0 / Math.PI);
        double newLonDegs = newLonRads * (180.0 / Math.PI);

        // Interpolate altitude
        double newAlt = fromPoint.AltMslM + (toPoint.AltMslM - fromPoint.AltMslM) * fraction;

        return new KoreLLAPoint() { LatDegs = newLatDegs, LonDegs = newLonDegs, AltMslM = newAlt };
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Pos + Aim
    // --------------------------------------------------------------------------------------------

    public enum MoveDirection { Forward, Backward, Left, Right, Up, Down }

    // Move the LLA point in the specified direction, with UP being the normal to the surface at the current LLA for a spherical earth.

    public static KoreLLAPoint MoveWithAim(KoreLLAPoint currentPos, KoreAzEl aim, MoveDirection direction, double distanceM)
    {
        // --- Local frame at currentPos (spherical Earth, matches ToXYZ basis) ---
        double lat = currentPos.LatRads;
        double lon = currentPos.LonRads;

        // Up  (surface normal, unit)
        KoreXYZVector up = new KoreXYZVector(Math.Cos(lat) * Math.Cos(lon),
                                                Math.Sin(lat),
                                                Math.Cos(lat) * Math.Sin(lon)).Normalize();

        // East (unit)
        KoreXYZVector east = new KoreXYZVector(-Math.Sin(lon),
                                                0.0,
                                                Math.Cos(lon)); // already unit

        // North (unit)
        KoreXYZVector north = new KoreXYZVector(-Math.Sin(lat) * Math.Cos(lon),
                                                Math.Cos(lat),
                                                -Math.Sin(lat) * Math.Sin(lon)).Normalize();

        // Yaw-only forward on tangent plane
        double yaw = aim.AzRads;
        KoreXYZVector fFlat = (north * Math.Cos(yaw)) + (east * Math.Sin(yaw));
        fFlat = fFlat.Normalize();

        // Full forward with pitch (elevation relative to horizon)
        double pitch = aim.ElRads;
        KoreXYZVector fwd = (fFlat * Math.Cos(pitch)) + (up * Math.Sin(pitch));
        fwd = fwd.Normalize();

        // Right (strafe) = fFlat � Up
        KoreXYZVector right = KoreXYZVector.CrossProduct(fFlat, up).Normalize();

        // Choose movement direction
        KoreXYZVector dir = direction switch
        {
            MoveDirection.Forward => fwd,
            MoveDirection.Backward => fwd.Invert(),
            MoveDirection.Left => right.Invert(),
            MoveDirection.Right => right,
            MoveDirection.Up => up,
            MoveDirection.Down => up.Invert(),
            _ => KoreXYZVector.Zero
        };

        // Step in XYZ
        KoreXYZVector currPt = currentPos.ToXYZ();
        KoreXYZVector curr = new KoreXYZVector(currPt);
        KoreXYZVector step = dir * distanceM;
        KoreXYZVector moved = curr + step;

        // Keep radius for strafes (and F/B when pitch � 0) so you stay on the same altitude
        bool keepRadius =
            direction == MoveDirection.Left ||
            direction == MoveDirection.Right ||
            ((direction == MoveDirection.Forward || direction == MoveDirection.Backward) && Math.Abs(pitch) < 1e-6);

        if (keepRadius)
            moved.Magnitude = currentPos.RadiusM;   // project back to original radius

        // Convert back to LLA
        KoreXYZVector newPt = new KoreXYZVector(moved.X, moved.Y, moved.Z);
        return KoreLLAPoint.FromXYZ(newPt);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Projected Point
    // --------------------------------------------------------------------------------------------

    // Given an LLA point, and an aim az/el, what point on the surface (0 MSL, spherical earth) is being aimed at?

    public static KoreLLAPoint ProjectedSurfacePoint(KoreLLAPoint fromPoint, KoreAzEl aim)
    {
        // --- Local frame at fromPoint (spherical Earth, matches ToXYZ basis) ---
        double lat = fromPoint.LatRads;
        double lon = fromPoint.LonRads;

        // Up  (surface normal, unit)
        KoreXYZVector up = new KoreXYZVector(Math.Cos(lat) * Math.Cos(lon),
                                                Math.Sin(lat),
                                                Math.Cos(lat) * Math.Sin(lon)).Normalize();

        // East (unit)
        KoreXYZVector east = new KoreXYZVector(-Math.Sin(lon),
                                                0.0,
                                                Math.Cos(lon)); // already unit

        // North (unit)
        KoreXYZVector north = new KoreXYZVector(-Math.Sin(lat) * Math.Cos(lon),
                                                Math.Cos(lat),
                                                -Math.Sin(lat) * Math.Sin(lon)).Normalize();

        // Calculate aim direction vector
        double yaw = aim.AzRads;
        double pitch = aim.ElRads;

        // Yaw-only forward on tangent plane
        KoreXYZVector fFlat = (north * Math.Cos(yaw)) + (east * Math.Sin(yaw));
        fFlat = fFlat.Normalize();

        // Full forward with pitch (elevation relative to horizon)
        KoreXYZVector aimDir = (fFlat * Math.Cos(pitch)) + (up * Math.Sin(pitch));
        aimDir = aimDir.Normalize();

        // Ray-sphere intersection: find where aim ray intersects Earth surface (radius = KoreWorldConsts.EarthRadiusM)
        KoreXYZVector rayStart = fromPoint.ToXYZ();
        KoreXYZVector rayDir = aimDir;
        double sphereRadius = KoreWorldConsts.EarthRadiusM;

        // Solve: |rayStart + t * rayDir|² = sphereRadius²
        double a = rayDir.Magnitude * rayDir.Magnitude;
        double b = 2.0 * KoreXYZVector.DotProduct(rayStart, rayDir);
        double c = (rayStart.Magnitude * rayStart.Magnitude) - (sphereRadius * sphereRadius);

        double discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            // No intersection (aiming away from Earth) - project forward by MSL altitude distance
            double projectionDistance = Math.Min(fromPoint.AltMslM, 100000.0); // Cap at 100km
            KoreXYZVector projectedPoint = rayStart + (rayDir * projectionDistance);
            return KoreLLAPoint.FromXYZ(projectedPoint);
        }

        // Take the closer intersection point (smaller positive t)
        double t1 = (-b - Math.Sqrt(discriminant)) / (2 * a);
        double t2 = (-b + Math.Sqrt(discriminant)) / (2 * a);

        // Choose the intersection point that's in front of the ray (positive t)
        double t = (t1 > 0) ? t1 : t2;

        if (t <= 0)
        {
            // No forward intersection - project forward by MSL altitude distance
            double projectionDistance = Math.Min(fromPoint.AltMslM, 100000.0); // Cap at 100km
            KoreXYZVector projectedPoint = rayStart + (rayDir * projectionDistance);
            return KoreLLAPoint.FromXYZ(projectedPoint);
        }

        // Calculate intersection point
        KoreXYZVector intersectionPoint = rayStart + (rayDir * t);

        // Convert back to LLA
        return KoreLLAPoint.FromXYZ(intersectionPoint);
    }

}

