// <fileheader>

using System;
using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;


// The struct KoreLLAPoint stores a Lat Long Altitude (LLA) position and calculates various new positions, angles and offsets related to it.
// Data is stored in SI units and additional accessor properties allow setting and getting in different units.
// An associates KoreXYZVector class defines a position in an ENU, XYZ format. An associates KoreConsts class hold constants regarding conversion and earth radius.

// Design Decisions:
// - The code uses a struct rather than an immutable class, as the constructor options with flexible units
//   are simply too useful. We rely on the struct's pass by value to avoid issues with mutability.

public struct KoreLLAPoint
{
    // SI Units and a pure radius value so the trig functions are simple.
    // Accomodate units and MSL during accessor functions

    public double LatRads { get; set; }
    public double LonRads { get; set; }
    public double RadiusM { get; set; } // Alt above EarthCentre

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
    public double AltMslKm // Alt above MSL
    {
        get { return (RadiusM - KoreWorldConsts.EarthRadiusM) * KoreWorldConsts.MetresToKmMultiplier; }
        set { RadiusM = (value + KoreWorldConsts.EarthRadiusKm) * KoreWorldConsts.KmToMetresMultiplier; }
    }
    public double AltMslM // Alt above M
    {
        get { return (RadiusM - KoreWorldConsts.EarthRadiusM); }
        set { RadiusM = value + KoreWorldConsts.EarthRadiusM; }
    }
    public double RadiusKm // Alt above EarthRadius
    {
        get { return (RadiusM * KoreWorldConsts.MetresToKmMultiplier); }
        set { RadiusM = (value * KoreWorldConsts.KmToMetresMultiplier); }
    }

    public override string ToString()
    {
        return string.Format($"(LatDegs:{LatDegs:F2}, LonDegs:{LonDegs:F2}, AltMslM:{AltMslM:F2})");
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Constructors - different options and units
    // --------------------------------------------------------------------------------------------

    // Note that fields can be set:
    //   KoreLLAPoint pos = new KoreLLAPoint() { latDegs = X, LonDegs = Y, AltMslM = Z };

    public KoreLLAPoint(double laRads, double loRads, double radiusM)
    {
        this.LatRads = laRads;
        this.LonRads = loRads;
        this.RadiusM = radiusM;
    }

    public KoreLLAPoint(double laRads, double loRads)
    {
        this.LatRads = laRads;
        this.LonRads = loRads;
        this.RadiusM = KoreWorldConsts.EarthRadiusM;
    }

    public KoreLLAPoint(KoreLLPoint llPoint)
    {
        this.LatRads = llPoint.LatRads;
        this.LonRads = llPoint.LonRads;
        this.RadiusM = KoreWorldConsts.EarthRadiusM;
    }

    public KoreLLAPoint(KoreLLPoint llPoint, double radiusM)
    {
        this.LatRads = llPoint.LatRads;
        this.LonRads = llPoint.LonRads;
        this.RadiusM = radiusM;
    }

    public static KoreLLAPoint Zero
    {
        get { return new KoreLLAPoint { LatRads = 0.0, LonRads = 0.0, RadiusM = 0.0 }; }
    }

    // Function to return an LLA position on the ground, either using the default MSL value, or an optional elevation above MSL in metres.

    public KoreLLAPoint Grounded(double? ElevAboveMslM = null)
    {
        // Default to MSL radius
        double radius = KoreWorldConsts.EarthRadiusM;

        // If the ElevAboveMslM value is provided, use it added to earth radius
        if (ElevAboveMslM.HasValue)
            radius = KoreWorldConsts.EarthRadiusM + ElevAboveMslM.Value;

        // Return new "grounded" position
        return new KoreLLAPoint(LatRads, LonRads, radius);
    }

    public KoreLLAPoint WithMinAltMslM(double minAltMslM)
    {
        double retElev = Math.Max(KoreWorldConsts.EarthRadiusM + minAltMslM, AltMslM);
        return new KoreLLAPoint(LatRads, LonRads, retElev);
    }

    // Currently consider the wrapping of latitude and longitude values to be done by the caller, in case consistency over the dateline is desired.
    public KoreLLAPoint WrapValues()
    {
        double newLonRads = KoreNumericRange<double>.MinusPiToPiRadians.Apply(LonRads);

        return new KoreLLAPoint(LatRads, newLonRads, RadiusM);
    }


    // --------------------------------------------------------------------------------------------
    // MARK: Conversion
    // --------------------------------------------------------------------------------------------

    // Convert To/From XYZ coordinates
    // - Where X = right to longitude 90� (East)
    // -       Y = up to North Pole (lat 90�) and Y-ve to South Pole (lat -90�)
    // -       Z = forward to zero lat/long and Z-ve to longitude 180 (date line)

    public KoreXYZVector ToXYZ()
    {
        KoreXYZVector retXYZ = new KoreXYZVector(
            RadiusM * Math.Cos(LatRads) * Math.Sin(LonRads),   // X = r.cos(lat).sin(lon) - to match +X=lon90
            RadiusM * Math.Sin(LatRads),                       // Y = r.sin(lat)
            RadiusM * Math.Cos(LatRads) * Math.Cos(LonRads));  // Z = r.cos(lat).cos(lon) - to match +Z=lon0
        return retXYZ;
    }

    // Usage: KoreLLAPoint pos = KoreLLAPoint.FromXYZ(xyz);
    public static KoreLLAPoint FromXYZ(KoreXYZVector inputXYZ)
    {
        double radius = inputXYZ.Magnitude;

        // Protect against div0 radius
        if (radius < KoreConsts.ArbitrarySmallDouble)
            return KoreLLAPoint.Zero;

        double latRads = Math.Asin(inputXYZ.Y / radius);
        double lonRads = Math.Atan2(inputXYZ.X, inputXYZ.Z);  // Fixed to match comment: +Z=lon0, +X=lon90
        return new KoreLLAPoint(latRads, lonRads, radius);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Distance
    // --------------------------------------------------------------------------------------------

    // Nullable default parameter allows it to assume the altitude for the calculation
    // is the average of the two points, unless a value (such a zero "do calc at ground
    // level") is supplied.
    // The curved distance can be at MSL if the Alt parameter is zero, will otherwise
    // use an average to the two input altitudes.

    public double CurvedDistanceToM(KoreLLAPoint destPos, double? AtAltMslM = null)
    {
        // If a specific altitude is provided, use it for the radius calculation.
        // Otherwise, use the average of the radii from the Earth's center to each point.
        double radius = KoreWorldConsts.EarthRadiusM;
        if (AtAltMslM.HasValue)
            radius = AtAltMslM.Value;
        else
            radius = (this.RadiusM + destPos.RadiusM) / 2;

        // Calculate the difference in latitude and longitude between the two points.
        // The AngleDiffRads function ensures the angle difference wraps correctly
        // around the Kore, handling the transition across the 180-degree meridian.
        double dLat = KoreValueUtils.AngleDiffRads(destPos.LatRads, this.LatRads);
        double dLon = KoreValueUtils.AngleDiffRads(destPos.LonRads, this.LonRads);

        // Compute the components of the haversine formula:
        // 'a' calculates the square of half the chord length between the points.
        // 'c' is the angular distance in radians, derived from 'a'.
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(this.LatRads) * Math.Cos(destPos.LatRads) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // The final distance is the product of the radius and the angular distance.
        // This gives the curved (arc) distance along the surface of the sphere.
        double distance = radius * c;

        return distance;
    }

    public double StraightLineDistanceToM(KoreLLAPoint destPos)
    {
        KoreXYZVector startXYZ = this.ToXYZ();
        KoreXYZVector destXYZ = destPos.ToXYZ();
        return startXYZ.DistanceTo(destXYZ);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Elevation
    // --------------------------------------------------------------------------------------------

    // The elevation to a distant point. Useful in aiming a camera.

    public double ElevationToRads(KoreLLAPoint destPos)
    {
        // Calculate horizontal distance
        //double horizontalDistance = this.CurvedDistanceToM(destPos);
        double horizontalDistance = this.StraightLineDistanceToM(destPos);

        // Calculate altitude difference (raw)
        double altitudeDifference = destPos.AltMslM - this.AltMslM;

        // Adjust altitude difference to account for Earth's curvature
        double curvatureDrop = (horizontalDistance * horizontalDistance) / (2 * KoreWorldConsts.EarthRadiusM);
        double adjustedAltitudeDifference = altitudeDifference - curvatureDrop;

        // Calculate the elevation angle in radians
        double elevationRads = Math.Atan2(adjustedAltitudeDifference, horizontalDistance);

        // Return the elevation angle
        return elevationRads * -1;
    }


    // /// <summary>
    // /// Updates the altitude of this KoreLLAPoint point to result in a specific elevation angle when viewed from another point.
    // /// The elevation angle is specified in radians, where 0 radians represents no elevation change (horizontal line of sight),
    // /// negative values represent a downward angle, and positive values represent an upward angle relative to the local plane.
    // /// </summary>
    // /// <param name="relativetoPos">The KoreLLAPoint point from which the elevation is measured.</param>
    // /// <param name="elevRads">The desired elevation angle in radians. Negative for downward, positive for upward.</param>
    // /// <returns>The updated KoreLLAPoint object with adjusted altitude.</returns>
    // public KoreLLAPoint WithElevationRads(KoreLLAPoint relativetoPos, double elevRads)
    // {
    //     // Step 1: Calculate the horizontal distance
    //     double horizontalDistance = this.CurvedDistanceToM(relativetoPos, null);

    //     // Step 2: Calculate the altitude difference
    //     double altitudeDifference = Math.Tan(elevRads) * horizontalDistance;

    //     // Step 3: Update the altitude of this point
    //     // Assuming AltMslM is the altitude above mean sea level
    //     this.AltMslM = relativetoPos.AltMslM + altitudeDifference;

    //     return this; // Return the modified object
    // }

    // --------------------------------------------------------------------------------------------
    // MARK: Azimuth
    // --------------------------------------------------------------------------------------------

    public double BearingToRads(KoreLLAPoint destPos)
    {
        // Convert latitudes and longitudes from degrees to radians
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

    // --------------------------------------------------------------------------------------------
    // MARK: Range Bearing
    // --------------------------------------------------------------------------------------------

    // Use haversine formula to calculate distance

    public KoreRangeBearing RangeBearingTo(KoreLLAPoint destPos)
    {
        double lat1 = this.LatRads;
        double lon1 = this.LonRads;
        double lat2 = destPos.LatRads;
        double lon2 = destPos.LonRads;

        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);

        // Use the mean radius of the two points so that the returned range is
        // consistent with PlusRangeBearing() which operates at the point's
        // altitude.  Using the plain earth radius here caused small errors when
        // the caller expected altitude to be taken into account.
        double calcRadius = (this.RadiusM + destPos.RadiusM) / 2.0;

        double distanceM = 2 * calcRadius * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Calculate bearing
        double y = Math.Sin(dLon) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
        double bearingRads = Math.Atan2(y, x);

        return new KoreRangeBearing { RangeM = distanceM, BearingRads = bearingRads };
    }

    public KoreLLAPoint PlusRangeBearing(KoreRangeBearing inputRB)
    {
        // Setup working variables
        double LatRads          = this.LatRads;
        double LonRads          = this.LonRads;
        double CalcRadius       = this.RadiusM;
        double InputRangeM      = inputRB.RangeM;
        double InputBearingRads = inputRB.BearingRads;

        // Shortcut calculations, avoid repetition.
        double SinLatRads           = Math.Sin(LatRads);
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

        return new KoreLLAPoint(NewLatRads, NewLonRads, this.RadiusM);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Range Bearing Alt
    // --------------------------------------------------------------------------------------------

    public KoreRangeBearingAlt RangeBearingAltTo(KoreLLAPoint destPos)
    {
        // Calculate the range and bearing to the destination point
        KoreRangeBearing rb = this.RangeBearingTo(destPos);

        // Calculate the altitude difference
        double altDeltaM = destPos.AltMslM - this.AltMslM;

        // Return a new KoreRangeBearingAlt with the calculated values
        return new KoreRangeBearingAlt(rb.RangeM, rb.BearingRads, altDeltaM);
    }

    public KoreLLAPoint PlusRangeBearingAlt(KoreRangeBearingAlt inputRBAlt)
    {
        // Create a KoreRangeBearing from the input KoreRangeBearingAlt
        KoreRangeBearing rb = new KoreRangeBearing
        {
            RangeM = inputRBAlt.RangeM,
            BearingRads = inputRBAlt.BearingRads
        };

        // Use the existing PlusRangeBearing method to calculate the new position
        KoreLLAPoint newPos = this.PlusRangeBearing(rb);

        // Adjust the altitude based on the altitude difference in the input KoreRangeBearingAlt
        newPos.RadiusM += inputRBAlt.AltDeltaM;

        return newPos;
    }

    // --------------------------------------------------------------------------------------------

    public KoreLLAPoint PlusDeltaForTime(KoreCourse course, double timeSecs)
    {
        // Turn the course into a range bearing distance (for the time duration) that we can apply)
        double distM = course.SpeedMps * timeSecs;
        KoreRangeBearing rb = new KoreRangeBearing() { RangeM = distM, BearingRads = course.HeadingRads };
        KoreLLAPoint retPnt = PlusRangeBearing(rb);

        // Now accomodate the climb rate
        double climbM = course.ClimbRateMps * timeSecs;
        retPnt.RadiusM += climbM;

        return retPnt;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Polar Offset
    // --------------------------------------------------------------------------------------------

    public KoreAzElRange StraightLinePolarOffsetTo(KoreLLAPoint destPos)
    {
        double azRads = this.BearingToRads(destPos);
        double elRads = this.ElevationToRads(destPos);
        double distM  = this.StraightLineDistanceToM(destPos);
        //double distM  = this.CurvedDistanceToM(destPos);

        return new KoreAzElRange() { AzRads = azRads, ElRads = elRads, RangeM = distM };
    }

    public KoreLLAPoint PlusPolarOffset(KoreAzElRange offset)
    {
        // We start by calculating the displacement along the ground (range*Cos(elevation)).
        double rangeOnGroundM = offset.RangeM * Math.Cos(offset.ElRads);

        // Calculate the change in latitude and longitude from the displacement and azimuth.
        double deltaLatRads = rangeOnGroundM * Math.Cos(offset.AzRads) / this.RadiusM;
        double deltaLonRads = rangeOnGroundM * Math.Sin(offset.AzRads) / (this.RadiusM * Math.Cos(this.LatRads));

        // The new latitude and longitude are then the old values plus the changes.
        double newLatRads = this.LatRads + deltaLatRads;
        double newLonRads = this.LonRads + deltaLonRads;

        // The change in altitude (range*Sin(elevation)) is added to the original altitude to find the new altitude.
        double newRadiusM = this.RadiusM + offset.RangeM * ( Math.Sin(offset.ElRads) * -1);

        return new KoreLLAPoint(newLatRads, newLonRads, newRadiusM);
    }

    public KoreLLAPoint PlusPolarOffset(double azRads, double elRads, double rangeM)
    {
        KoreAzElRange offset = new KoreAzElRange() { AzRads = azRads, ElRads = elRads, RangeM = rangeM };
        return this.PlusPolarOffset(offset);
    }

    public KoreLLAPoint PlusPolarOffsetDegs(double azDegs, double elDegs, double rangeM)
    {
        double azRads = azDegs * KoreConsts.DegsToRadsMultiplier;
        double elRads = elDegs * KoreConsts.DegsToRadsMultiplier;

        KoreAzElRange offset = new KoreAzElRange() { AzRads = azRads, ElRads = elRads, RangeM = rangeM };
        return this.PlusPolarOffset(offset);
    }


}

