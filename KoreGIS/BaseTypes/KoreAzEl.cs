// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;


public struct KoreAzEl
{
    public double AzRads { get; set; }
    public double ElRads { get; set; }

    // --------------------------------------------------------------------------------------------

    public double AzDegs
    {
        get { return AzRads * KoreConsts.RadsToDegsMultiplier; }
        set { AzRads = value * KoreConsts.DegsToRadsMultiplier; }
    }
    public double ElDegs
    {
        get { return ElRads * KoreConsts.RadsToDegsMultiplier; }
        set { ElRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    // --------------------------------------------------------------------------------------------

    // Constructor accepts radians
    public KoreAzEl(double azRads, double elRads)
    {
        AzRads = azRads;
        ElRads = elRads;
    }

    public static KoreAzEl Zero
    {
        get { return new KoreAzEl { AzRads = 0.0,  ElRads = 0.0 }; }
    }

    // --------------------------------------------------------------------------------------------

    // Determine the angle between two azimuth/elevation directions.
    public static double AngleBetween(KoreAzEl v1, KoreAzEl v2)
    {
        double cosTheta = Math.Cos(v1.ElRads) * Math.Cos(v2.ElRads) * Math.Cos(v1.AzRads - v2.AzRads) +
                          Math.Sin(v1.ElRads) * Math.Sin(v2.ElRads);
        return Math.Acos(cosTheta);
    }

    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        return $"Az:{AzDegs:F2}Degs  El:{ElDegs:F2}Degs";
    }

}

