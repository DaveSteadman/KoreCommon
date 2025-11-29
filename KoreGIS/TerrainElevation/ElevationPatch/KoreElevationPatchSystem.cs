using System;
using System.Collections.Generic;
using System.Text;

using KoreCommon;

namespace KoreGIS;

#nullable enable

// KoreElevationPatchSystem: A class to contain a number of elevation prep tiles, and supply an elevation value
// for a specific lat/long from the highest resolution (and assumed most accurate) tile it has for that location.

public class KoreElevationPatchSystem
{
    // KoreElevationPatchSystem.InvalidEle

    private List<KoreElevationPatch> PatchList = new();

    // --------------------------------------------------------------------------------------------
    // MARK: Load Patch
    // --------------------------------------------------------------------------------------------

    // Three ways to create a new patch:
    // - Load from a patch file
    // - Load from an ASCII Arc file

    public KoreElevationPatch? LoadPatchFile(string filePath)
    {
        // Read all the text content into a string
        string content = System.IO.File.ReadAllText(filePath);

        KoreElevationPatch? newPatch = KoreElevationPatchIO.ReadFromTextFile(content);
        if (newPatch != null)
        {
            AddPatch(newPatch);
        }
        return newPatch;
    }

    // --------------------------------------------------------------------------------------------

    // An ASCII Arc file is a simple text file with a header followed by a grid of elevation values.
    // The top-left of the data is the top left of the map.

    public KoreElevationPatch? LoadPatchFromArcASCIIFile(string filename, KoreLLBox llBox)
    {
        // Check the file exists
        if (!System.IO.File.Exists(filename))
        {
            KoreCentralLog.AddEntry($"ArcASCIIToTile: File not found: {filename}");
            return null;
        }

        // Read the file
        KoreFloat2DArray data = KoreElevationPatchIO.LoadFromArcASIIGridFile(filename);

        // Orient the data in the 2D array, placing (maxlat, minLon) at top-left.
        //KoreFloat2DArray flippedData = KoreFloat2DArray.FlipXAxis(data);

        // Create the tile
        KoreElevationPatch newTile = new() { ElevationData = data, LLBox = llBox };

        // Add tile to internal list and sort in descending order of resolution
        PatchList.Add(newTile);
        SortPatchList();

        KoreCentralLog.AddEntry($"ArcASCIIToTile: Loaded {filename} {llBox}");

        return newTile;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Sort Tile List
    // --------------------------------------------------------------------------------------------

    // Sort the PatchList from highest resolution to lowest, as returned by the TileRes function.
    // Keeps the list in an order so we use the first patch that contains the position we are looking for.

    public void SortPatchList()
    {
        PatchList.Sort((a, b) => a.TileRes().CompareTo(b.TileRes()));
    }


    // --------------------------------------------------------------------------------------------
    // MARK: Empty Tile List
    // --------------------------------------------------------------------------------------------

    public void AddPatch(KoreElevationPatch newPatch)
    {
        PatchList.Add(newPatch);
        SortPatchList();
    }

    public void EmptyPatchList()
    {
        PatchList.Clear();
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Elevation At Pos
    // --------------------------------------------------------------------------------------------

    public float ElevationAtPos(KoreLLPoint pos)
    {
        // Loop through the PatchList, grabbing points from the highest resolution tile that contains the position.
        // Loop across the points in a tile, populating the requested array.

        if (PatchList.Count == 0) return KoreElevationUtils.InvalidEle;

        foreach (KoreElevationPatch tile in PatchList)
        {
            if (tile.Contains(pos))
                return tile.ElevationAtPos(pos);
        }
        return KoreElevationUtils.InvalidEle;
    }

    // --------------------------------------------------------------------------------------------

    // A debug function to source an elevation value and show its workings, returning a string of the activity
    // rather than a value.

    public string ElevationAtPosWithReport(KoreLLPoint pos)
    {
        StringBuilder sb = new StringBuilder();
        float retEle = KoreElevationUtils.InvalidEle;

        sb.AppendLine($"Elevation at position: {pos}");

        foreach (KoreElevationPatch tile in PatchList)
        {
            sb.AppendLine($"Considering Tile: {tile.Report()}");
            if (tile.Contains(pos))
            {
                sb.AppendLine($"position in Tile");

                retEle = tile.ElevationAtPos(pos);
                sb.AppendLine($"Elevation: {retEle}");
            }
            else
            {
                sb.AppendLine($"position NOT in Tile");
            }
        }
        sb.AppendLine($"Concluding Elevation: {retEle}");
        return sb.ToString();
    }

    // --------------------------------------------------------------------------------------------

    public KoreElevationPatch CreateNewPatch(KoreLLBox llBox, int numLatPoints, int numLonPoints)
    {
        KoreElevationPatch newTile = new() { ElevationData = new KoreFloat2DArray(numLonPoints, numLatPoints), LLBox = llBox };

        // Create the value range for lat and longs that we'll cover in the new patch
        KoreNumericRange<double> latRange = new KoreNumericRange<double>(llBox.MinLatDegs, llBox.MaxLatDegs);
        KoreNumericRange<double> lonRange = new KoreNumericRange<double>(llBox.MinLonDegs, llBox.MaxLonDegs);

        // Turn the range into a list of lat/lon values we can iterate over.
        KoreNumeric1DArray<double> latArray = new KoreNumeric1DArray<double>(latRange, numLatPoints, direction: KoreNumeric1DArray<double>.ListDirection.Reverse);
        KoreNumeric1DArray<double> lonArray = new KoreNumeric1DArray<double>(lonRange, numLonPoints);

        for (int i = 0; i < latArray.Length; i++)
        {
            for (int j = 0; j < lonArray.Length; j++)
            {
                KoreLLPoint newPoint = new KoreLLPoint() { LatDegs = latArray[i], LonDegs = lonArray[j] };
                newTile.ElevationData[j, i] = ElevationAtPos(newPoint);
            }
        }
        return newTile;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Report
    // --------------------------------------------------------------------------------------------

    public string Report()
    {
        string report = $"Elevation System Report: {PatchList.Count} Tile(s)\n";

        foreach (KoreElevationPatch tile in PatchList)
        {
            report += tile.Report() + "\n";
        }
        return report;
    }

}