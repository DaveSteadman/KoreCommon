// <fileheader>

using System;

using KoreCommon;

namespace KoreGIS;


// The struct KoreLLPoint stores Lat Long values from a bottom-left origin, which is useful for map tiles.
// Will convert between this and a KoreLLAPoint where the origin is the "center".

public struct KoreLLPoint
{
    // SI Units and a pure radius value so the trig functions are simple.
    // Accomodate units and MSL during accessor functions

    public double LatRads { get; set; }
    public double LonRads { get; set; }

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
    // Constructors - different options and units
    // --------------------------------------------------------------------------------------------

    // Note that fields can be set:
    //   KoreLLPoint pos = new KoreLLPoint() { latDegs = X, LonDegs = Y };

    public KoreLLPoint(double laRads, double loRads)
    {
        LatRads = laRads;
        LonRads = loRads;
    }

    public KoreLLPoint(KoreLLAPoint llPos)
    {
        this.LatRads = llPos.LatRads;
        this.LonRads = llPos.LonRads;
    }

    public static KoreLLPoint Zero
    {
        get { return new KoreLLPoint { LatRads = 0.0, LonRads = 0.0 }; }
    }

    // --------------------------------------------------------------------------------------------

    // Convert To/From XYZ coordinates
    // - Where X = right to longitude 90� (East)
    // -       Y = up to North Pole (lat 90�) and Y-ve to South Pole (lat -90�)
    // -       Z = forward to zero lat/long and Z-ve to longitude 180 (date line)

    // Usage: KoreXYZVector xyzpos = llpos.ToXYZ(radius);
    public KoreXYZVector ToXYZ(double radius)
    {
        // Protect against div0 radius
        if (radius < KoreConsts.ArbitrarySmallDouble)
            return KoreXYZVector.Zero;

        KoreXYZVector retXYZ = new KoreXYZVector(
            radius * Math.Cos(LatRads) * Math.Sin(LonRads),   // X = r.cos(lat).sin(lon) - to match +X=lon90
            radius * Math.Sin(LatRads),                       // Y = r.sin(lat)
            radius * Math.Cos(LatRads) * Math.Cos(LonRads));  // Z = r.cos(lat).cos(lon) - to match +Z=lon0
        return retXYZ;
    }

    // Usage: KoreLLPoint pos = KoreLLPoint.FromXYZ(xyz);
    public static KoreLLPoint FromXYZ(KoreXYZVector inputXYZ)
    {
        double radius = inputXYZ.Magnitude;

        // Protect against div0 radius
        if (radius < KoreConsts.ArbitrarySmallDouble)
            return KoreLLPoint.Zero;

        double latRads = Math.Asin(inputXYZ.Y / radius);
        double lonRads = Math.Atan2(inputXYZ.X, inputXYZ.Z);
        return new KoreLLPoint(latRads, lonRads);
    }

    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        return string.Format($"({LatDegs:F3}, {LonDegs:F3})");
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Range Bearing
    // --------------------------------------------------------------------------------------------

    // Calculate range and bearing to another point using haversine formula
    // Assumes calculation at Earth's mean radius (MSL)
    public KoreRangeBearing RangeBearingTo(KoreLLPoint destPos)
    {
        double lat1 = this.LatRads;
        double lon1 = this.LonRads;
        double lat2 = destPos.LatRads;
        double lon2 = destPos.LonRads;

        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);

        double calcRadius = KoreWorldConsts.EarthRadiusM;
        double distanceM = 2 * calcRadius * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Calculate bearing
        double y = Math.Sin(dLon) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
        double bearingRads = Math.Atan2(y, x);

        return new KoreRangeBearing { RangeM = distanceM, BearingRads = bearingRads };
    }

    // Calculate a new position by adding range and bearing to this point
    // Uses spherical earth model at mean radius
    public KoreLLPoint PlusRangeBearing(KoreRangeBearing inputRB)
    {
        // Setup working variables
        double LatRads = this.LatRads;
        double LonRads = this.LonRads;
        double CalcRadius = KoreWorldConsts.EarthRadiusM;
        double InputRangeM = inputRB.RangeM;
        double InputBearingRads = inputRB.BearingRads;

        // Shortcut calculations, avoid repetition.
        double SinLatRads = Math.Sin(LatRads);
        double RangeDividedByRadius = InputRangeM / CalcRadius;

        double NewLatRads =
            Math.Asin(
                SinLatRads * Math.Cos(RangeDividedByRadius) +
                Math.Cos(LatRads) * Math.Sin(RangeDividedByRadius) *
                Math.Cos(InputBearingRads)
            );

        double NewLonRads =
            LonRads + Math.Atan2(
                Math.Sin(InputBearingRads) * Math.Sin(RangeDividedByRadius) * Math.Cos(LatRads),
                Math.Cos(RangeDividedByRadius) - SinLatRads * Math.Sin(NewLatRads)
            );

        return new KoreLLPoint(NewLatRads, NewLonRads);
    }

    // Calculate bearing to another point
    public double BearingToRads(KoreLLPoint destPos)
    {
        double lat1 = this.LatRads;
        double lon1 = this.LonRads;
        double lat2 = destPos.LatRads;
        double lon2 = destPos.LonRads;

        // Calculate difference in coordinates
        double dLon = lon2 - lon1;

        // Calculate bearing
        double y = Math.Sin(dLon) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
        double bearingRads = Math.Atan2(y, x);

        // Ensure bearing is between 0 and 2pi
        bearingRads = (bearingRads + 2 * Math.PI) % (2 * Math.PI);

        return bearingRads;
    }

    // Calculate curved distance to another point using haversine formula
    // Assumes calculation at Earth's mean radius (MSL)
    public double CurvedDistanceToM(KoreLLPoint destPos)
    {
        double radius = KoreWorldConsts.EarthRadiusM;

        double dLat = KoreValueUtils.AngleDiffRads(destPos.LatRads, this.LatRads);
        double dLon = KoreValueUtils.AngleDiffRads(destPos.LonRads, this.LonRads);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(this.LatRads) * Math.Cos(destPos.LatRads) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double distance = radius * c;

        return distance;
    }

}


