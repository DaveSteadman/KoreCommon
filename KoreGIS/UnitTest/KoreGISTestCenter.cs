// <fileheader>

using System;
using System.IO;

using KoreCommon;
using KoreCommon.UnitTest;
using KoreGIS;
namespace KoreGIS.UnitTest;


// Usage: KoreTestLog testLog = KoreGISTestCenter.RunCoreTests();

public static class KoreGISTestCenter
{
    public static void RunTests(KoreTestLog testLog)
    {
        try
        {
            bool success = KoreTestCenter.EnsureTestDirectory(testLog);
            if (!success)
            {
                testLog.AddResult("Test Centre Run", false, "Failed to create test directory.");
                return;
            }

            // Test geographic and position classes
            KoreTestPosition.RunTests(testLog);
            KoreTestPositionLLA.RunTests(testLog);
            KoreTestRoute.RunTests(testLog);

            // Shapefile tests (run early since they don't depend on other tests)
            KoreTestShapefile.RunTests(testLog);

            // SkiaSharp Plotter tests
            KoreTestWorldPlotter.RunTests(testLog);
        }
        catch (Exception)
        {
            testLog.AddResult("Test Centre Run", false, "Exception");
        }
    }

    // --------------------------------------------------------------------------------------------

    // Usage: KoreTestCenter.RunAdHocTests()
    public static KoreTestLog RunAdHocTests(KoreTestLog testLog)
    {

        try
        {
            // KoreTestXYZVector.TestArbitraryPerpendicular(testLog);
        }
        catch (Exception)
        {
            testLog.AddResult("Test Centre Run", false, "Exception");
        }

        return testLog;
    }
}

