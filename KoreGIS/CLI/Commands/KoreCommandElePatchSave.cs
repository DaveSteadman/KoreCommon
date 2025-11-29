using System;
using System.Collections.Generic;
using System.Text;

// KoreCommandElePrep

using KoreCommon;

namespace KoreGIS;

// CLI Usage: ele prep <inEleFilename> <inTileCode> <inOutDir> <action>
// CLI Usage: ele prep c:/Util/KorebeLibrary_MapPrep/Europe/W005N50_UkCentral/Ele_BF_BF_50m.asc BF_BF C:/Util/_temp yes

#nullable enable

public class KoreCommandElePatchSave : KoreCommand
{
    public KoreCommandElePatchSave()
    {
        Signature.Add("ele");
        Signature.Add("patch");
        Signature.Add("save");
    }

    public override string HelpString => $"{SignatureString} <Output Patch Filepath>   <longitude num points> <min lon degs> <max lon degs>   <latitude num points> <min lat degs> <max lat degs>";

    public override string Execute(List<string> parameters)
    {
        StringBuilder sb = new StringBuilder();

        if (parameters.Count != 7)
            return "KoreCommandEleLoadArc.Execute -> parameter count mismatch";

        string inPatchFilepath = parameters[0];
        int    inNumLonPoints  =    int.Parse(parameters[1]);
        double inMinLonDegs    = double.Parse(parameters[2]);
        double inMaxLonDegs    = double.Parse(parameters[3]);
        int    inNumLatPoints  =    int.Parse(parameters[4]);
        double inMinLatDegs    = double.Parse(parameters[5]);
        double inMaxLatDegs    = double.Parse(parameters[6]);

        KoreLLBox llBox = new KoreLLBox() {
            MinLatDegs = inMinLatDegs,
            MinLonDegs = inMinLonDegs,
            MaxLatDegs = inMaxLatDegs,
            MaxLonDegs = inMaxLonDegs };

        sb.AppendLine($"Elevation Prep:");
        sb.AppendLine($"- inPatchFilepath: {inPatchFilepath}");
        sb.AppendLine($"- LLBox: {llBox}");


        bool validOperation = true;

        // -------------------------------------------------

        // Convert and validate the inputs
        // if (!System.IO.File.Exists(inPatchFilepath))
        // {
        //     sb.AppendLine($"File not found: {inEleFilename}");
        //     validOperation = false;
        // }

        // -------------------------------------------------

        if (validOperation)
        {
            sb.AppendLine($"Valid operation: Progressing...");

            try
            {
                KoreGISHub.EleManager.CreatePatchFile(inPatchFilepath, llBox, inNumLatPoints, inNumLonPoints);
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Exception: {ex.Message}");
            }

        }

        // -------------------------------------------------

        sb.AppendLine($"Elevation System Report:");
        //sb.AppendLine(KoreSimFactory.Instance.EleSystem.Report());

        return sb.ToString();

    }
}