using System.Collections.Generic;
using System.Text;


// CLI Usage: ele prep <inEleFilename> <inTileCode> <inOutDir> <action>
// CLI Usage: ele prep c:/Util/KorebeLibrary_MapPrep/Europe/W005N50_UkCentral/Ele_BF_BF_50m.asc BF_BF C:/Util/_temp yes

using KoreCommon;

namespace KoreGIS;

public class KoreCommandEleReport : KoreCommand
{
    public KoreCommandEleReport()
    {
        Signature.Add("ele");
        Signature.Add("report");
    }

    public override string HelpString => $"{SignatureString}";

    public override string Execute(List<string> parameters)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(KoreGISHub.EleManager.Report());

        // -------------------------------------------------

        return sb.ToString();
    }
}