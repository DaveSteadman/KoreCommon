using System.Collections.Generic;
using System.Text;
using System.IO;

// CLI Usage: sat collate AB
using KoreCommon;

namespace KoreGIS;

public class KoreCommandSatDivide : KoreCommand
{
    public KoreCommandSatDivide()
    {
        Signature.Add("sat");
        Signature.Add("divide");
    }

    public override string HelpString => $"{SignatureString} <tilecode> <source image filename>";

    public override string Execute(List<string> parameters)
    {
        StringBuilder sb = new StringBuilder();

        if (parameters.Count != 2)
        {
            return "KoreCommandEleSaveTile.Execute -> insufficient parameters";
        }

        foreach (string param in parameters)
        {
            sb.AppendLine($"param: {param}");
        }

        return "pending";

        // string tileCodeStr   = parameters[0];
        // string inImgFilename = parameters[1];

        // if (tileCodeStr == "world")
        // {
        //     string worldTileFilepath = KoreMapTileFilepaths.WorldTileFilepath();

        //     sb.AppendLine($"Collating child tile images for the world tile: {worldTileFilepath}");
        //     KoreMapFileOperations.CollateChildTileImages_Lvl0();
        // }
        // else
        // {
        //     bool validOperation = true;

        //     KoreMapTileCode tileCode = KoreMapTileCode.TileCodeFromString(tileCodeStr);
        //     if (!tileCode.IsValid())         { validOperation = false; sb.AppendLine("Invalid tile code."); }
        //     if (!File.Exists(inImgFilename)) { validOperation = false; sb.AppendLine("source image not found."); }

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

        //         KoreMapFileOperations.DivideChildTileImages(tileCodeStr, inImgFilename);

        //         sb.AppendLine("Done.");
        //     }

        //     // -------------------------------------------------

        //     // sb.AppendLine($"Elevation System Report:");
        //     // sb.AppendLine(KoreSimFactory.Instance.EleSystem.Report());
        // }

        // return sb.ToString();
    }
}