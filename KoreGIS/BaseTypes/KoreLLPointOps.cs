// <fileheader>

using System;
using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;


// KoreLLAPointOps: A static class to hold operations on KoreLLAPoint objects that are not part of its
// core responsibilites. This class is static, all operations are stateless and return a new object.

public static class KoreLLPointOps
{
    // Calculates the angle between two points on a sphere
    public static double AngleBetween(KoreLLPoint fromPoint, KoreLLPoint toPoint)
    {
        double lat1 = fromPoint.LatRads;
        double lon1 = fromPoint.LonRads;
        double lat2 = toPoint.LatRads;
        double lon2 = toPoint.LonRads;

        double deltaLat = lat2 - lat1;
        double deltaLon = KoreValueUtils.AngleDiffRads(lon1, lon2);

        double a = Math.Pow(Math.Sin(deltaLat / 2), 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLon / 2), 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return c;
    }

    // --------------------------------------------------------------------------------------------

    public static KoreLLPoint RhumbLineInterpolation(KoreLLPoint fromPoint, KoreLLPoint toPoint, double fraction)
    {
        // This is a simple linear interpolation of the lat and lon, and a weighted average of the altitudes.
        // The fraction is the fraction of the distance from the fromPoint to the toPoint.

        double latDegs = fromPoint.LatDegs + ((toPoint.LatDegs - fromPoint.LatDegs) * fraction);
        double lonDegs = fromPoint.LonDegs + ((toPoint.LonDegs - fromPoint.LonDegs) * fraction);

        return new KoreLLPoint() { LatDegs = latDegs, LonDegs = lonDegs };
    }

    // --------------------------------------------------------------------------------------------

    public static KoreLLPoint GreatCircleInterpolation(KoreLLPoint fromPoint, KoreLLPoint toPoint, double fraction)
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

        return new KoreLLPoint() { LatDegs = newLatDegs, LonDegs = newLonDegs };
    }

    // --------------------------------------------------------------------------------------------

    // Generate a list of points that form a full circle path
    public static List<KoreLLPoint> GreatCirclePointList(KoreLLPoint fromPoint, KoreLLPoint toPoint, int numPoints)
    {
        var points = new List<KoreLLPoint>();
        double angleBetween = AngleBetween(fromPoint, toPoint);

        // Divide 360 degrees by the angle between the two points, to get the maximum fraction we need to create the full great circle.
        double maxFraction = (2 * Math.PI) / angleBetween;

        // Calculate the fraction step based on the total fraction needed and the number of points
        double fractionStep = maxFraction / numPoints;

        for (int i = 0; i < numPoints; i++)
        {
            points.Add(GreatCircleInterpolation(fromPoint, toPoint, fractionStep * i));
        }

        return points;
    }

    // --------------------------------------------------------------------------------------------

    // A quick/dirty "is facing camera" type check.
    // KoreLLPointOps.InSameHemisphere
    public static bool InSameHemisphere(KoreLLPoint refPoint, KoreLLPoint comparePoint)
    {
        if (KoreValueUtils.Diff(refPoint.LatDegs, comparePoint.LatDegs) > 90)
            return false;
        if (KoreValueUtils.Diff(refPoint.LonDegs, comparePoint.LonDegs) > 90)
            return false;

        return true;
    }

    // --------------------------------------------------------------------------------------------

    // Determines if a point is inside a polygon ring using the ray-casting algorithm
    // NOTE: Winding direction (clockwise vs counter-clockwise) does not matter for this algorithm
    // The ray-casting method counts edge intersections regardless of polygon orientation
    // ring: The polygon ring (list of points defining the boundary)
    // point: The point to test
    // returns: True if the point is inside the ring, false otherwise
    public static bool IsPointInRing(List<KoreLLPoint> ring, KoreLLPoint point)
    {
        int n = ring.Count;
        bool inside = false;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if (((ring[i].LatDegs > point.LatDegs) != (ring[j].LatDegs > point.LatDegs)) &&
                (point.LonDegs < (ring[j].LonDegs - ring[i].LonDegs) * (point.LatDegs - ring[i].LatDegs) / (ring[j].LatDegs - ring[i].LatDegs) + ring[i].LonDegs))
            {
                inside = !inside;
            }
        }
        return inside;
    }

}




