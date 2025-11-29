using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using KoreCommon;

namespace KoreGIS;

#nullable enable

// Overarching class to manage elevation activities, including background tasks around the loading and creating.

// Terminology:
// - A "Patch" is an arbitrary lat/lon rectangle, with an arbitrary resolution in either direction.
// - A "Tile" is a defined "BF_BF" tile code defining a specific lat/long area and through the code, a specific resolution.
// We load ASCII Arc Grid patches as the raw input data, which we can then save out to our own patches that have embedded
// lat long boxes, and save them out as tiles for full use in the application.

public class KoreElevationManager
{
    // Consume general Arc ASCII grid files and output elevations for a lat/lon.
    private KoreElevationPatchSystem ElePatchSystem = new();

    // Hold map tiles for use in display. Load/save tiles for caching work.
    private KoreElevationTileSystem EleTiles = new();

    // Semaphore to limit the number of concurrent tasks.
    private static readonly SemaphoreSlim semaphore = new(10); // Adjust the number as needed.

    // --------------------------------------------------------------------------------------------
    // MARK: Prep
    // --------------------------------------------------------------------------------------------

    public void LoadArcASCIIGridFile(string filename, KoreLLBox llBox)
    {
        KoreElevationPatch? newTile = ElePatchSystem.LoadPatchFromArcASCIIFile(filename, llBox);

        if (newTile == null)
            KoreCentralLog.AddEntry($"Failed to load Arc ASCII Grid: {filename} // {llBox}");
        else
            KoreCentralLog.AddEntry($"Loaded Arc ASCII Grid: {filename} // {llBox}");
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Patches
    // --------------------------------------------------------------------------------------------

    public void CreatePatchFile(string filePath, KoreLLBox llBox, int latNumPoints, int lonNumPoints)
    {
        // Basic validation of the inputs
        if (latNumPoints < 5 || lonNumPoints < 5)
        {
            KoreCentralLog.AddEntry($"CreatePatchFile: Insufficient points: {latNumPoints} x {lonNumPoints}");
            return;
        }

        KoreElevationPatch newPatch = ElePatchSystem.CreateNewPatch(llBox, latNumPoints, lonNumPoints);

        // Write the patch to string and then to file
        KoreElevationPatchIO.WriteToTextFileIncrementally(newPatch, filePath);

    }

    public void LoadPatchFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            KoreCentralLog.AddEntry($"Patch file not found: {filePath}");
            return;
        }

        // Read all the text content into a string
        string content = File.ReadAllText(filePath);

        if (string.IsNullOrEmpty(content))
        {
            KoreCentralLog.AddEntry($"Patch file content not loaded: {filePath}");
            return;
        }

        KoreElevationPatch? newPatch = KoreElevationPatchIO.ReadFromTextFile(filePath);
        if (newPatch != null)
        {
            ElePatchSystem.AddPatch(newPatch!);
        }
        else
        {
            KoreCentralLog.AddEntry($"Failed to load patch file: {filePath}");
        }

    }

    public void AddPatch(KoreElevationPatch patch)
    {
        // Add the patch to the system, which will sort it by resolution.
        ElePatchSystem.AddPatch(patch);
    }

    public float GetPatchElevationAtPos(KoreLLPoint pos)
    {
        // Get the elevation at a position, using the patch system.
        return ElePatchSystem.ElevationAtPos(pos);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Tiles
    // --------------------------------------------------------------------------------------------

    // Non-blocking call to start the tile preparation activities, that will require nested looping through
    // a large number of LL pairs.
    public void PrepTile(KoreMapTileCode tileCode, bool saveFile)
    {
        Task.Run(() =>
        {
            //await semaphore.WaitAsync(); // Wait for an available slot
            try
            {
                KoreElevationTile newTile = KoreElevationTileSystem.CreateTile(ElePatchSystem, tileCode);

                // If the tile contains no valuable data, don't save it. (will need to deal with below-sea-level data at some point in the future).
                float maxVal = newTile.ElevationData.MaxVal();
                // if (newTile.ElevationData.MaxVal() < 0)
                // {
                //     float minVal = newTile.ElevationData.MinVal();

                //     KoreCentralLog.AddEntry($"Tile contains no positive data: {tileCode} // {tileCode.LLBox.ToString()} // Range {minVal:F2} to {maxVal:F2}");
                //     return;
                // }

                if (saveFile)
                {
                    KoreMapTileFilepaths filepaths = new(tileCode);
                    try
                    {
                        KoreFileOps.CreateDirectoryForFile(filepaths.EleArrFilepath);
                        KoreElevationTileIO.WriteToTextFile(newTile, filepaths.EleArrFilepath);
                    }
                    catch (Exception ex)
                    {
                        KoreCentralLog.AddEntry($"Failed to write: {filepaths.EleArrFilepath} // {ex.Message}");
                    }
                }

                EleTiles.AddTile(newTile);
            }
            catch (Exception ex)
            {
                KoreCentralLog.AddEntry($"{ex.Message}");
            }

        });
    }

    // --------------------------------------------------------------------------------------------

    public void LoadTile(KoreMapTileCode tileCode)
    {
        Task.Run(async () =>
        {
            await semaphore.WaitAsync(); // Wait for an available slot
            try
            {
                // Avoid the workload if tile already exists
                if (HasTile(tileCode))
                    return;

                // Get the filepaths
                KoreMapTileFilepaths filepaths = new KoreMapTileFilepaths(tileCode);

                LoadTileFile(filepaths.EleArrFilepath);
            }
            finally
            {
                semaphore.Release(); // Release the slot
            }
        });
    }

    private void LoadTileFile(string filepath)
    {
        try
        {
            KoreElevationTile? newTile = KoreElevationTileIO.ReadFromTextFile(filepath);

            if (newTile != null)
            {
                EleTiles.AddTile(newTile);
            }
            else
            {
                KoreCentralLog.AddEntry($"Failed to load file: {filepath}");
            }
        }
        catch (Exception ex)
        {
            KoreCentralLog.AddEntry($"EXCEPTION: Failed to load file: {filepath} // {ex.Message}");
        }
    }

    public bool HasTile(KoreMapTileCode tileCode) => EleTiles.HasTile(tileCode.TileCode);
    public KoreElevationTile GetTile(KoreMapTileCode tileCode) => EleTiles.GetTile(tileCode.TileCode); // Returns a default tile when not found, use HasTile first
    public void DeleteTile(KoreMapTileCode tileCode) => EleTiles.DeleteTile(tileCode.TileCode);
    public void DeleteAllTiles() => EleTiles.DeleteAllTiles();

    // --------------------------------------------------------------------------------------------
    // MARK: High-Level task
    // --------------------------------------------------------------------------------------------

    // Function called by a tile that wants the elevation tile for it, and doesn't care about the complexity required to acheive that.
    // This function exists to perform whater tasks are required to supply that data, through the HasTile() and GetTile() calls.

    public void RequestTile(KoreMapTileCode tileCode)
    {
        Task.Run(async () =>
        {
            await semaphore.WaitAsync(); // Wait for an available slot
            try
            {
                // Avoid the workload if tile already exists
                if (HasTile(tileCode))
                    return;

                // Get the filepaths
                KoreMapTileFilepaths filepaths = new KoreMapTileFilepaths(tileCode);

                // If a saved tile exists, load that.
                if (filepaths.EleArrFileExists)
                {
                    LoadTileFile(filepaths.EleArrFilepath);
                    return;
                }

                // No pre-existing tile, so get one from the Prep area
                PrepTile(tileCode, true);
            }
            finally
            {
                semaphore.Release(); // Release the slot
            }
        });
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Report
    // --------------------------------------------------------------------------------------------

    public string Report()
    {
        string prepReport = ElePatchSystem.Report();
        string tileReport = EleTiles.Report();

        return $"Elevation Patch Report\n{prepReport}\n{tileReport}";
    }

    public float ElevationAtPos(KoreLLPoint pos)
    {
        return ElePatchSystem.ElevationAtPos(pos);
        //return 0f; // KoreLLPoint.Zero;
    }


}

