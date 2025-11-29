using System;
using System.Collections.Generic;
using System.Text;

// KoreCommandElePrep


// CLI Usage: ele prep <inEleFilename> <inTileCode> <inOutDir> <action>
// CLI Usage: ele prep c:/Util/KorebeLibrary_MapPrep/Europe/W005N50_UkCentral/Ele_BF_BF_50m.asc BF_BF C:/Util/_temp yes
using KoreCommon;

namespace KoreGIS;

#nullable enable

public class KoreCommandElePatchLoad : KoreCommand
{
    public KoreCommandElePatchLoad()
    {
        Signature.Add("ele");
        Signature.Add("patch");
        Signature.Add("load");
    }

    public override string HelpString => $"{SignatureString} <Patch Filename>";

    public override string Execute(List<string> parameters)
    {
        StringBuilder sb = new StringBuilder();

        try
        {
            if (parameters.Count != 1)
                return "KoreCommandElePatchLoad -> parameter count mismatch";

            string inPatchFilepath = parameters[0];
            sb.AppendLine($"Elevation Prep:");
            sb.AppendLine($"- inEleFilename: {inPatchFilepath}");

            bool validOperation = true;

            // -------------------------------------------------

            // Convert and validate the inputs
            if (!System.IO.File.Exists(inPatchFilepath))
            {
                sb.AppendLine($"File not found: {inPatchFilepath}");
                validOperation = false;
            }

            // -------------------------------------------------

            if (validOperation)
            {
                sb.AppendLine($"Valid operation: Progressing...");

                KoreGISHub.EleManager.LoadPatchFile(inPatchFilepath);
            }

            // -------------------------------------------------

            sb.AppendLine($"Elevation System Report:");
            sb.AppendLine(KoreGISHub.EleManager.Report());

            return sb.ToString();
        }
        catch (Exception ex)
        {
            sb.AppendLine($"KoreCommandElePatchLoad.Execute -> Exception: {ex.Message}");
            return sb.ToString();
        }

    }
}