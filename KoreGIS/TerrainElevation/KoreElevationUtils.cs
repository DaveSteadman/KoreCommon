
using System;

using KoreCommon;

namespace KoreGIS;

public static class KoreElevationUtils
{
    // KoreElevationUtils.InvalidEle
    public static float InvalidEle      = -9999f;
    public static float InvalidEleCheck = -9990f; // For checking < or > comparisons

    // --------------------------------------------------------------------------------------------

    // Function to return the number of points for longitude given a latitude.
    // Usage: int lonRes = KoreElevationUtils.LonResForLat(20, 60.0);
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

        return adjustedResolution;
    }
}