// <fileheader>


using System;

using KoreCommon;

namespace KoreGIS;


public static class KoreWorldOps
{
    private static KoreNumeric1DMappedRange<double> KoreEarthRadiusRange = new KoreNumeric1DMappedRange<double>();

    static KoreWorldOps()
    {
        // Setup a number range matching radius to latitude. We can then interpolate out any absolute latitude value below.
        KoreEarthRadiusRange.Add(0.0,  KoreWorldConsts.EarthRadiusEquatorM);
        KoreEarthRadiusRange.Add(90.0, KoreWorldConsts.EarthRadiusPolarM);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Lat Long Point Resolutions
    // --------------------------------------------------------------------------------------------
    // Function to return the number of points for longitude given a latitude. This is useful to
    // keep the aspect ratio of a tile as square as we can.
    //
    // latitudeResolution: This number is entirely arbitrary, it will be scaled to the latitude to
    //   try and keep a square aspect ratio.
    // Note: The minimum output number is 2, as this could be used to define a row of points in a tile.
    //   the UV values of a texture won't make sense with fewer.
    //
    // Usage: int lonRes = KoreWorldOps.LonResForLat(20, 60.0);
    public static int LonResForLat(int latitudeResolution, double latDegs)
    {
        // Clamp latitude to valid range (-90 to 90 degrees).
        latDegs = Math.Max(-90.0, Math.Min(90.0, latDegs));

        // Convert latitude to radians for easier trigonometric calculations.
        double latRads = Math.Abs(latDegs) * (Math.PI / 180.0);

        // Calculate a scaling factor based on the cosine of the latitude.
        // Near the equator (latitude = 0), factor is 1.0 (full resolution).
        // Near the poles (latitude = +/-90), factor approaches 0 (lower resolution).
        double scale = Math.Cos(latRads);

        // Calculate the adjusted resolution for longitude.
        int adjustedResolution = (int)Math.Max(1, Math.Round(latitudeResolution * scale));

        // Ensure the adjusted resolution is at least 2 to avoid issues with too few points.
        if (adjustedResolution < 2) adjustedResolution = 2; // Ensure at least 2 points for a row.

        return adjustedResolution;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Horizon
    // --------------------------------------------------------------------------------------------

    // Calculate the distance to the horizon from an altitude above mean sea level.
    // Usage: KoreWorldOps.DistanceToHorizonM(1000.0);
    public static double DistanceToHorizonM(double altMslM)
    {
        // Earth's radius in meters (mean radius)
        double earthRadiusM = KoreWorldConsts.EarthRadiusM;

        // Check we have a valid altitude
        if (altMslM < 1) return 0;

        // Calculate the distance to the horizon using the formula:
        // distanceM = sqrt(2 * earthRadiusM * altMslM + altMslM^2)
        double distanceM = Math.Sqrt(2 * earthRadiusM * altMslM + altMslM * altMslM);

        return distanceM;
    }

    // Calculate the angle below horizontal (dip angle) to the horizon.
    // This is useful for aiming cameras or sensors from an altitude.
    public static double HorizonElevationRads(double altMslM)
    {
        // Earth's radius in meters (mean radius)
        double earthRadiusM = KoreWorldConsts.EarthRadiusM;

        // Use the existing method to compute the horizon distance
        double horizonDistanceM = DistanceToHorizonM(altMslM);

        // Check we have a valid horizon distance to work with
        if (horizonDistanceM < 1) return 0;

        // Calculate the dip angle (in radians)
        double horizonAngleRads = Math.Atan(horizonDistanceM / earthRadiusM);

        return horizonAngleRads;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Earth Radius
    // --------------------------------------------------------------------------------------------

    // | Function to return the Earth's radius at a given latitude in meters.
    // Usage: double surfaceRadius = KoreWorldOps.EllipsoidRadiusForLatitude(45.0);
    public static double EllipsoidRadiusForLatitude(double latDegs)
    {
        // Get the absolute value of the latitude in degrees.
        double absLatDegs = Math.Abs(latDegs);

        // Clamp the number to 90->0 degrees. 0: Equator, 90: Poles
        absLatDegs = KoreValueUtils.LimitToRange(absLatDegs, 0.0, 90.0);

        // Lookup and return the Earth radius for the given latitude.
        return KoreEarthRadiusRange.GetOutput(absLatDegs);
    }
}

