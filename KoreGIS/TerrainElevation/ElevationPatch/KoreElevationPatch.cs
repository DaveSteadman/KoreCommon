
using System;
using System.Collections.Generic;
using System.Linq;

using KoreCommon;


namespace KoreGIS;
//nullable enable

// KoreElevationPatch: A tile sized by an arbitrary AzEl box, and at an arbitrary resolution. Exists to
// interpolate elevation values at lat/long values required for exporting tiles.

public class KoreElevationPatch
{
    // create a boilerplate elevation data array until replaced with real data in a constructor or method.
    public KoreNumeric2DArray<float> ElevationData { get; set; } = new KoreNumeric2DArray<float>(10, 10);
    public KoreLLBox LLBox { get; set; } = KoreLLBox.Zero;

    // --------------------------------------------------------------------------------------------
    // MARK: Resolution
    // --------------------------------------------------------------------------------------------

    // The lat long box will have a resolution in degrees. The 2D array will have a number of points.
    // GetRes calculates and returns the lowest number of points per degree in the lat long box.

    public float TileRes()
    {
        float latRes = (float)LLBox.DeltaLatDegs / ElevationData.Height;
        float lonRes = (float)LLBox.DeltaLonDegs / ElevationData.Width;

        return (latRes < lonRes) ? latRes : lonRes;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Set
    // --------------------------------------------------------------------------------------------

    public void SetElevationArray(KoreNumeric2DArray<float> elevationData)
    {
        // Validate the elevation data dimensions match the LLBox dimensions.
        if (elevationData.Width < 5 || elevationData.Height < 5)
        {
            KoreCentralLog.AddEntry($"SetElevationArray: Insufficient points: {elevationData.Width} x {elevationData.Height}");
            return;
        }

        ElevationData = elevationData;
    }

    public void SetLLBox(KoreLLBox llBox)
    {
        // Validate the LLBox dimensions are reasonable.
        if (llBox.DeltaLatDegs <= 0 || llBox.DeltaLonDegs <= 0)
        {
            KoreCentralLog.AddEntry($"SetLLBox: Invalid LLBox dimensions: {llBox}");
            return;
        }

        LLBox = llBox;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Elevation for Position
    // --------------------------------------------------------------------------------------------

    public bool Contains(KoreLLPoint pos)
    {
        return LLBox.Contains(pos);
    }

    public float ElevationAtPos(KoreLLPoint pos)
    {
        // Check if the position is within the bounds of the tile.
        if (!LLBox.Contains(pos))
            return KoreElevationUtils.InvalidEle;

        // Calculate the fractional position within the tile, then use that to access the value.
        double fracLat = (pos.LatDegs - LLBox.MinLatDegs) / LLBox.DeltaLatDegs;
        double fracLon = (pos.LonDegs - LLBox.MinLonDegs) / LLBox.DeltaLonDegs;

        // Validate the fraction
        fracLat = KoreValueUtils.LimitToRange(fracLat, 0, 1);
        fracLon = KoreValueUtils.LimitToRange(fracLon, 0, 1);

        // invert the lat fraction, as the data is stored with the top left as 0,0
        fracLat = 1 - fracLat;

        // Read the values that would be used in any interpolation, check they are valid.
        List<float> interpVals = ElevationData.InterpolationValues((float)fracLon, (float)fracLat);
        bool hasInvalidValues = interpVals.Any(val => val < KoreElevationUtils.InvalidEleCheck);
        if (hasInvalidValues) return KoreElevationUtils.InvalidEle;

        // Perform the interpolation using the fractional values and return the result
        return ElevationData.InterpolatedValue((float)fracLon, (float)fracLat);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Report
    // --------------------------------------------------------------------------------------------

    public string Report()
    {
        return $"LLBox: {LLBox.ToStringVerbose()} / Res: {TileRes()} / GridSize:{ElevationData.Width}x{ElevationData.Height}";
    }

}
