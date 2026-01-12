// <fileheader>

using System;
using System.Collections.Generic;
using System.Text;

// File to contain a test entry and the overall test log, such that we can run tests and record the
// outputs with enough information to isolate a single test failure.
// Includes a simple output report we can print every time and a detailed report when we need to debug.

using KoreCommon;
namespace KoreCommon.UnitTest;


// ------------------------------------------------------------------------

public enum KoreTestLogEntryType { Test, Comment, Separator}

public enum KoreTestLogResult { Pass, Fail, Untested }

// ------------------------------------------------------------------------

public struct KoreTestLogEntry
{
    public string Name;
    public KoreTestLogEntryType EntryType;
    public KoreTestLogResult Result;
    public string Comment;
}

public class KoreTestLog
{
    private List<KoreTestLogEntry> ResultList = new List<KoreTestLogEntry>();

    // --------------------------------------------------------------------------------------------
    // MARK: Add result
    // --------------------------------------------------------------------------------------------

    public void AddResult(string name, bool result, string comment = "")
    {
        ResultList.Add(
            new KoreTestLogEntry()
            {
                Name = name,
                EntryType = KoreTestLogEntryType.Test,
                Result = result ? KoreTestLogResult.Pass : KoreTestLogResult.Fail,
                Comment = comment
            });
    }

    public void AddComment(string comment)
    {
        ResultList.Add(
            new KoreTestLogEntry()
            {
                Name = "",
                EntryType = KoreTestLogEntryType.Comment,
                Result = KoreTestLogResult.Untested,
                Comment = comment
            });
    }

    public void AddSeparator()
    {
        ResultList.Add(new KoreTestLogEntry()
        {
            Name = "",
            EntryType = KoreTestLogEntryType.Separator,
            Result = KoreTestLogResult.Untested,
            Comment = ""
        });
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Reports
    // --------------------------------------------------------------------------------------------

    public bool OverallPass()
    {
        bool result = true;

        // Find each entry that is a test result, and then ensure its a pass
        foreach (KoreTestLogEntry entry in ResultList)
        {
            if (entry.EntryType == KoreTestLogEntryType.Test)
            {
                if (entry.Result != KoreTestLogResult.Pass)
                {
                    result = false;
                    break; // No need to check further, we have a fail
                }
            }
        }
        return result;
    }

    // --------------------------------------------------------------------------------------------

    public string OneLineReport()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        int passCount = 0;
        int failCount = 0;
        int untestedCount = 0;
        int testCount = 0;
        foreach (var entry in ResultList)
        {
            if (entry.EntryType == KoreTestLogEntryType.Test)
            {
                testCount++;
                if (entry.Result == KoreTestLogResult.Pass) { passCount++; }
                if (entry.Result == KoreTestLogResult.Fail) { failCount++; }
                if (entry.Result == KoreTestLogResult.Untested) { untestedCount++; }
            }
        }

        string passFailStr = "PASS";
        if (failCount > 0)
        {
            passFailStr = "FAIL";
        }
        else
        {
            if (untestedCount > 0)
                passFailStr += " (with Untested: " + untestedCount + ")";
        }

        return $"Overall:{passFailStr} // Time:{timestamp} // NumTests:{testCount} Passes:{passCount} Fails:{failCount} Untested:{untestedCount}";
    }

    // --------------------------------------------------------------------------------------------

    public string FullReport()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var entry in ResultList)
        {
            switch (entry.EntryType)
            {
                case KoreTestLogEntryType.Separator:
                    sb.AppendLine("--------------------------------------------------");
                    break;

                case KoreTestLogEntryType.Comment:
                    string commentSymbol = KoreEmojiSymbolOps.GetSymbol(KoreEmojiType.SpeechBubble);
                    sb.AppendLine($"{commentSymbol} COMMENT: {entry.Comment}");
                    break;

                case KoreTestLogEntryType.Test:
                    string resultStr = ResultToString(entry.Result);
                    string comment = string.IsNullOrEmpty(entry.Comment) ? "" : $" // {entry.Comment}";

                    string resultSymbol = KoreEmojiSymbolOps.GetSymbol(KoreEmojiType.TrafficLightOrange);
                    if (entry.Result == KoreTestLogResult.Pass)
                        resultSymbol = KoreEmojiSymbolOps.GetSymbol(KoreEmojiType.TrafficLightGreen);
                    else if (entry.Result == KoreTestLogResult.Fail)
                        resultSymbol = KoreEmojiSymbolOps.GetSymbol(KoreEmojiType.TrafficLightRed);

                    sb.AppendLine($"{resultSymbol} TEST: {resultStr} // {entry.Name}{comment}");
                    break;
            }
        }

        return sb.ToString();
    }

    // --------------------------------------------------------------------------------------------

    public string FailReport()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var entry in ResultList)
        {
            if (entry.EntryType == KoreTestLogEntryType.Test && entry.Result == KoreTestLogResult.Fail)
            {
                string resultSymbol = KoreEmojiSymbolOps.GetSymbol(KoreEmojiType.TrafficLightRed);
                string resultStr = ResultToString(entry.Result);
                string comment = string.IsNullOrEmpty(entry.Comment) ? "" : $" // {entry.Comment}";

                sb.AppendLine($"{resultSymbol} TEST: {resultStr} // {entry.Name}{comment}");
            }
        }
        return sb.ToString();
    }

    // --------------------------------------------------------------------------------------------

    private string ResultToString(KoreTestLogResult result)
    {
        return result switch
        {
            KoreTestLogResult.Pass => "PASS",
            KoreTestLogResult.Fail => "FAIL",
            KoreTestLogResult.Untested => "UNT",
            _ => "UNKNOWN"
        };
    }
}
