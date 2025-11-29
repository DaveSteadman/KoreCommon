// <fileheader>


using System.Text;
using System.IO;
using System.Collections.Generic;

namespace KoreCommon;
using KoreCommon.UnitTest;

#nullable enable

public class KoreCommandCommonUnitTest : KoreCommand
{
    public KoreCommandCommonUnitTest()
    {
        Signature.Add("unittest");
        Signature.Add("common");
    }

    //public override string HelpString => $"{SignatureString} <old_path> <new_path> [test]";

    public override string Execute(List<string> parameters)
    {
        // Print paths for test mode or regular operation
        StringBuilder sb = new StringBuilder();

        // Actually perform the rename
        try
        {
            KoreTestLog testLog = new KoreTestLog();
            KoreTestCenter.RunTests(testLog);

            string fullReport = testLog.FullReport();
            string failReport = testLog.FailReport();

            sb.AppendLine("------------------------------------------------------------------------");
            if (testLog.OverallPass())
                sb.AppendLine(fullReport);
            else
                sb.AppendLine(failReport);
            sb.AppendLine("------------------------------------------------------------------------");
            sb.AppendLine(testLog.OneLineReport());
            sb.AppendLine("------------------------------------------------------------------------");
        }
        catch (System.Exception ex)
        {
            sb.AppendLine($"KoreCommandFileRename: ERROR - Rename failed: {ex.Message}");
        }

        return sb.ToString();
    }
}