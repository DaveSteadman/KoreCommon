using System.Collections.Generic;
using System.IO;
using System.Text;

using KoreCommon;

namespace KoreGIS;

// CLI Usage: sat collate AB

public class KoreCommandSatCollate : KoreCommand
{
    public KoreCommandSatCollate()
    {
        Signature.Add("sat");
        Signature.Add("collate");
    }

    public override string HelpString => $"{SignatureString} <tilecode>";

    public override string Execute(List<string> parameters)
    {
        StringBuilder sb = new StringBuilder();

        if (parameters.Count != 1)
        {
            return "KoreCommandEleSaveTile.Execute -> insufficient parameters";
        }

        foreach (string param in parameters)
        {
            sb.AppendLine($"param: {param}");
        }

        string tileCodeStr = parameters[0];

       // bool validOperation = true;

        return "pending";



        // if (tileCodeStr == "world")
        // {
        //     string worldTileFilepath = KoreMapTileFilepaths.WorldTileFilepath();

        //     sb.AppendLine($"Collating child tile images for the world tile: {worldTileFilepath}");
        //     KoreMapFileOperations.CollateChildTileImages_Lvl0();
        //     return sb.ToString();
        // }
        // else
        // {


        //     KoreMapTileCode tileCode = KoreMapTileCode.TileCodeFromString(tileCodeStr);
        //     if (!tileCode.IsValid()) { validOperation = false; sb.AppendLine("Invalid tile code."); }

        //     // -------------------------------------------------

        //     // // Convert and validate the inputs
        //     // if (!System.IO.File.Exists(inEleFilename))
        //     // {
        //     //     sb.AppendLine($"File not found: {inEleFilename}");
        //     //     validOperation = false;
        //     // }

        //     // -------------------------------------------------

        //     if (validOperation)
        //     {
        //         sb.AppendLine($"Collating child tile images for tile {tileCodeStr}");

        //         KoreMapFileOperations.CollateChildTileImages(tileCodeStr);

        //         sb.AppendLine("Done.");
        //     }

        //     // -------------------------------------------------

        //     // sb.AppendLine($"Elevation System Report:");
        //     // sb.AppendLine(KoreSimFactory.Instance.EleSystem.Report());

        // }

        // return sb.ToString();
    }
}