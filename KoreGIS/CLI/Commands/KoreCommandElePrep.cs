using System.Collections.Generic;
using System.Text;

// KoreCommandElePrep

// CLI Usage: ele prep <inEleFilename> <inTileCode> <inOutDir> <action>
// CLI Usage: ele prep c:/Util/KorebeLibrary_MapPrep/Europe/W005N50_UkCentral/Ele_BF_BF_50m.asc BF_BF C:/Util/_temp yes
using KoreCommon;

namespace KoreGIS;


#nullable enable

public class KoreCommandElePrep : KoreCommand
{
    public KoreCommandElePrep()
    {
        Signature.Add("ele");
        Signature.Add("prep");
    }

    public override string Execute(List<string> parameters)
    {
        StringBuilder sb = new StringBuilder();

        if (parameters.Count < 4)
        {
            return "KoreCommandElePrep.Execute -> insufficient parameters";
        }

        string inEleFilename = parameters[0];
        string inTileCode = parameters[1];
        string inOutDir = parameters[2];
        string action = parameters[3];

        sb.AppendLine($"Elevation Prep:");
        sb.AppendLine($"- inEleFilename: {inEleFilename}");
        sb.AppendLine($"- inTileCode:    {inTileCode}");
        sb.AppendLine($"- inOutDir:      {inOutDir}");
        sb.AppendLine($"- action:        {action}");

        bool validOperation = true;

        bool actionWrite = KoreStringOps.BoolForString(action);

        // -------------------------------------------------

        // Convert and validate the inputs
        if (!System.IO.File.Exists(inEleFilename))
        {
            sb.AppendLine($"File not found: {inEleFilename}");
            validOperation = false;
        }
        KoreMapTileCode? topTileCode = KoreMapTileCode.TileCodeFromString(inTileCode);
        if (topTileCode == null)
        {
            sb.AppendLine($"Invalid tile code: {inTileCode}");
            validOperation = false;
        }
        if (!System.IO.Directory.Exists(inOutDir))
        {
            sb.AppendLine($"Output directory not found: {inOutDir}");
            validOperation = false;
        }

        // -------------------------------------------------

        if (validOperation)
        {
            sb.AppendLine($"Valid operation: Progressing...");

            // // Load the elevation data
            // KoreFloat2DArray asciiArcArry = KoreElevationPatchIO.LoadFromArcASIIGridFile(inEleFilename);

            // // Get the tile code - figure out the subtiles for this level.
            // List<KoreMapTileCode> childCodes = topTileCode!.ChildCodesList();

            // Kore2DGridPos gridPos = topTileCode!.GridPos;

            // int cellsAcross = gridPos.Width;
            // int cellsDown = gridPos.Height;

            // KoreFloat2DArray[,] newSubSampledGrid = asciiArcArry.GetInterpolatedSubGridCellWithOverlap(
            //     gridPos.Width, gridPos.Height, 2000, 2000);

            // foreach (KoreMapTileCode currChildCode in childCodes)
            // {
            //     KoreFloat2DArray currSubSampledGrid = newSubSampledGrid[currChildCode.GridPos.PosX, currChildCode.GridPos.PosY];

            //     // Determine the new tile code
            //     KoreMapTileCode currChildTileCode = topTileCode.ChildCode(currChildCode.GridPos.PosX, currChildCode.GridPos.PosY);

            //     // Save the sub-sampled grid
            //     string outEleFilename = KoreFileOperations.JoinPaths(inOutDir, $"Ele_{currChildTileCode.ToString()}.asc");

            //     if (actionWrite)
            //     {
            //         sb.AppendLine($"Saved: {outEleFilename} ({currSubSampledGrid.sizeStr()})");
            //         KoreElevationPatchIO.SaveToArcASIIGridFile(currSubSampledGrid, outEleFilename);
            //     }
            //     else
            //     {
            //         sb.AppendLine($"Would save: {outEleFilename} ({currSubSampledGrid.sizeStr()})");
            //     }
            // }
        }

        // -------------------------------------------------

        return sb.ToString();
    }
}