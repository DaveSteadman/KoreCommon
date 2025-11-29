using System.Collections.Generic;
using System.Text;


// CLI Usage: ele prep <inEleFilename> <inTileCode> <inOutDir> <action>
// CLI Usage: ele prep c:/Util/KorebeLibrary_MapPrep/Europe/W005N50_UkCentral/Ele_BF_BF_50m.asc BF_BF C:/Util/_temp yes
using KoreCommon;

namespace KoreGIS;


public class KoreCommandEleForPos : KoreCommand
{
    public KoreCommandEleForPos()
    {
        Signature.Add("ele");
        Signature.Add("forpos");
    }

    public override string HelpString => $"{SignatureString} <float lat degs> <float lon degs>";

    public override string Execute(List<string> parameters)
    {
        StringBuilder sb = new StringBuilder();

        try
        {
            if (parameters.Count < 2)
            {
                return "KoreCommandEleForPos.Execute -> insufficient parameters";
            }

            int count = 0;
            foreach (string param in parameters)
            {
                sb.AppendLine($"param[{count}]: {param}");
                count++;
            }
            // return sb.ToString();

            double inLatDegs = float.Parse(parameters[0]);
            float inLonDegs = float.Parse(parameters[1]);

            bool validOperation = true;


            if (validOperation)
            {
                sb.AppendLine($"Valid operation: Progressing...");

                KoreLLPoint pos = new KoreLLPoint() { LatDegs = inLatDegs, LonDegs = inLonDegs };
                float newEle = KoreGISHub.EleManager.ElevationAtPos(pos);

                //string eleWithReport = KoreSimFactory.Instance.EleManager.ElevationAtPosWithReport(pos);


                sb.AppendLine($"Elevation at position: {pos} = {newEle:F2}");
                //sb.AppendLine(eleWithReport);

            }

            // -------------------------------------------------

            return sb.ToString();

        }
        catch (System.Exception ex)
        {
            sb.AppendLine($"KoreCommandEleForPos.Execute -> Exception: {ex.Message}");
            return sb.ToString();
        }
    }
}