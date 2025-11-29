using System.Collections.Generic;
using System.Text;


// CLI Usage: ele prep <inEleFilename> <inTileCode> <inOutDir> <action>
// CLI Usage: ele prep c:/Util/KorebeLibrary_MapPrep/Europe/W005N50_UkCentral/Ele_BF_BF_50m.asc BF_BF C:/Util/_temp yes
using KoreCommon;

namespace KoreGIS;


public class KoreCommandEleSaveTile : KoreCommand
{
    public KoreCommandEleSaveTile()
    {
        Signature.Add("ele");
        Signature.Add("savetile");
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
        // return sb.ToString();

        string tileCodeStr = parameters[0];

        KoreMapTileCode tileCode = KoreMapTileCode.TileCodeFromString(tileCodeStr);
        KoreMapTileFilepaths filepaths = new(tileCode);




        // double inMinLatDegs  = double.Parse(parameters[1]);
        // double inMinLonDegs  = double.Parse(parameters[2]);
        // double inMaxLatDegs  = double.Parse(parameters[3]);
        // double inMaxLonDegs  = double.Parse(parameters[4]);
        // int inLonRes         = int.Parse(parameters[5]);
        // int inLatRes         = int.Parse(parameters[6]);

        // // sb.AppendLine($"Elevation ve Tile:");
        // // sb.AppendLine($"- inEleFilename: {inEleFilename}");

        // KoreLLBox llBox = new KoreLLBox() {
        //     MinLatDegs = inMinLatDegs,
        //     MinLonDegs = inMinLonDegs,
        //     MaxLatDegs = inMaxLatDegs,
        //     MaxLonDegs = inMaxLonDegs };

        bool validOperation = true;

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
            sb.AppendLine($"Progressing: Writing elevations for tile {tileCode} to {filepaths.EleFilepath}");

            //sb.AppendLine($"Creating new tile: {llBox} {inLatRes} {inLonRes}");

            KoreGISHub.EleManager.PrepTile(tileCode, true);

            //KoreElevationPatchIO.WriteToTextFile(newTile, inEleFilename);
        }

        // -------------------------------------------------

        // sb.AppendLine($"Elevation System Report:");
        // sb.AppendLine(KoreSimFactory.Instance.EleSystem.Report());


        return sb.ToString();
    }
}