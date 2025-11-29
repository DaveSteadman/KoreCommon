using System.Collections.Generic;
using System.Text;
using System.IO;

// CLI Usage: sat collate AB

#nullable enable

using KoreCommon;

namespace KoreGIS;



public class KoreCommandSatDivideTo : KoreCommand
{
    public KoreCommandSatDivideTo()
    {
        Signature.Add("sat");
        Signature.Add("divideto");
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

        bool validOperation = true;


        KoreMapTileCode tileCode = KoreMapTileCode.TileCodeFromString(tileCodeStr);
        if (!tileCode.IsValid()) { validOperation = false; sb.AppendLine("Invalid tile code."); }

        KoreMapTileCode parentTileCode = tileCode.ParentCode();
        if (!parentTileCode.IsValid()) { validOperation = false; sb.AppendLine("Invalid parent tile code."); }

        // Load the parent tile image
        KoreMapTileFilepaths parentFilepaths = new KoreMapTileFilepaths(parentTileCode);

        if (!File.Exists(parentFilepaths.WebpFilepath))
        {
            sb.AppendLine($"File not found: {parentFilepaths.WebpFilepath}");
            validOperation = false;
        }

        // if (validOperation)
        // {
        //     Image? parentImage = KoreImageOperations.LoadImage(parentFilepaths.WebpFilepath);

        //     if (parentImage == null)
        //     {
        //         sb.AppendLine($"Failed to load image: {parentFilepaths.WebpFilepath}");
        //         validOperation = false;
        //     }
        //     else
        //     {
        //         sb.AppendLine($"Loaded image: {parentFilepaths.WebpFilepath}");

        //         // Divide up the image into child tiles
        //         //KoreMapFileOperations.DivideChildTileImages(tileCodeStr, parentImage!);

        //         Image[,] childImages = KoreMapFileOperations.DivideIntoChildTileImages(parentTileCode, parentImage);
        //         Kore2DGridPos tilePos = parentTileCode.ChildPositionInParent();

        //         Image childImage = childImages[tilePos.PosX, tilePos.PosY];

        //         // Save the child image
        //         sb.AppendLine($"Saving:");
        //         sb.AppendLine($"- Child image size: {childImage.GetWidth()} x {childImage.GetHeight()}");
        //         sb.AppendLine($"- Child image pos: {tilePos.PosX} {tilePos.PosY}");
        //         sb.AppendLine($"- Child image file: {parentFilepaths.WebpFilepath}");

        //         // Now save out the child image
        //         KoreMapTileFilepaths childFilepaths = new KoreMapTileFilepaths(tileCode);
        //         childImage.SaveWebp(childFilepaths.WebpFilepath);
        //     }
        // }

        // -------------------------------------------------

        // // Convert and validate the inputs
        // if (!System.IO.File.Exists(inEleFilename))
        // {
        //     sb.AppendLine($"File not found: {inEleFilename}");
        //     validOperation = false;
        // }

        // -------------------------------------------------

        if (validOperation)
        {
            sb.AppendLine($"Collating child tile images for tile {tileCodeStr}");

            //KoreMapFileOperations.DivideChildTileImages(tileCodeStr, inImgFilename);

            sb.AppendLine("Done.");
        }

        // -------------------------------------------------

        // sb.AppendLine($"Elevation System Report:");
        // sb.AppendLine(KoreSimFactory.Instance.EleSystem.Report());


        return sb.ToString();
    }
}