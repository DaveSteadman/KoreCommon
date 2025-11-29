// <fileheader>

using System;
using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;


// KoreLLALocation: A class that encapsulates the idea of a location with latitude, longitude, and altitude, but without
// a fixed radius or altitude type.

public struct KoreLLALocation
{
    // SI Units and a pure radius value so the trig functions are simple.
    // Accomodate units and MSL during accessor functions

    public double LatRads { get; set; }
    public double LonRads { get; set; }
    public double AltM { get; set; } // Alt - dependent on AltType Value

    public enum AltType
    {
        MSL, // Mean Sea Level - default
        AGL  // Above Ground Level
    }
    public AltType AltTypeValue { get; set; } = AltType.MSL;

    // --------------------------------------------------------------------------------------------
    // Additional simple accessors - adding units
    // --------------------------------------------------------------------------------------------

    public double LatDegs
    {
        get { return LatRads * KoreConsts.RadsToDegsMultiplier; }
        set { LatRads = value * KoreConsts.DegsToRadsMultiplier; }
    }
    public double LonDegs
    {
        get { return LonRads * KoreConsts.RadsToDegsMultiplier; }
        set { LonRads = value * KoreConsts.DegsToRadsMultiplier; }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Altitude Accessors
    // --------------------------------------------------------------------------------------------

    // An altitude is made up of up to three components
    // - Distance from Earth center to MSL - calculated from latitude
    // - Distance from MSL to AGL - Sourced from a terrain elevation function
    // - Distance from AGL to the point - The AltM value

    private double EllipsoidMslAltM()
    {
        double surfaceRadius = KoreWorldOps.EllipsoidRadiusForLatitude(LatDegs);

        if (AltTypeValue == AltType.MSL)
            return surfaceRadius + AltM;
        else
            throw new InvalidOperationException("Cannot calculate MSL altitude when AltType is not MSL.");
    }

    private double EllipsoidRadiusM => KoreWorldOps.EllipsoidRadiusForLatitude(LatDegs);

    // public double AltMslKm // Alt above MSL
    // {
    //     get { return (RadiusM - KoreWorldConsts.EarthRadiusM) * KoreWorldConsts.MetresToKmMultiplier; }
    //     set { RadiusM = (value + KoreWorldConsts.EarthRadiusKm) * KoreWorldConsts.KmToMetresMultiplier; }
    // }
    // public double AltMslM // Alt above M
    // {
    //     get { return (RadiusM - KoreWorldConsts.EarthRadiusM); }
    //     set { RadiusM = value + KoreWorldConsts.EarthRadiusM; }
    // }
    // public double RadiusKm // Alt above EarthRadius
    // {
    //     get { return (RadiusM * KoreWorldConsts.MetresToKmMultiplier); }
    //     set { RadiusM = (value * KoreWorldConsts.KmToMetresMultiplier); }
    // }


    // --------------------------------------------------------------------------------------------
    // MARK: Constructors - different options and units
    // --------------------------------------------------------------------------------------------

    // Note that fields can be set:
    //   KoreLLAPoint pos = new KoreLLAPoint() { latDegs = X, LonDegs = Y, AltMslM = Z };

    public KoreLLALocation(double laRads, double loRads, double altM)
    {
        this.LatRads = laRads;
        this.LonRads = loRads;
        this.AltM = altM;
    }

    public KoreLLALocation(double laRads, double loRads)
    {
        this.LatRads = laRads;
        this.LonRads = loRads;
        this.AltM = 0;
    }

    public static KoreLLALocation Zero
    {
        get { return new KoreLLALocation { LatRads = 0.0, LonRads = 0.0, AltM = 0.0 }; }
    }

    // Function to return an LLA position on the ground, either using the default MSL value, or an optional elevation above MSL in metres.

    public KoreLLAPoint ToLLA()
    {
        double newRadiusM = 0;

        switch (AltTypeValue)
        {
            case AltType.MSL:
                newRadiusM = KoreWorldConsts.EarthRadiusM + AltM;
                break;
            case AltType.AGL:
                newRadiusM = KoreWorldConsts.EarthRadiusM + AltM;
                break;
            default:
                newRadiusM = KoreWorldConsts.EarthRadiusM + AltM;
                break;
        }

        return new KoreLLAPoint(LatRads, LonRads) { RadiusM = newRadiusM };
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Utility
    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        return string.Format($"({LatDegs:F3}, {LonDegs:F3}, {AltM:F2})");
    }

}

