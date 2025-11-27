// <fileheader>

using System;
using System.IO;
using KoreCommon;
namespace KoreCommon.UnitTest;


// Usage: KoreTestLog testLog = KoreTestCenter.RunCoreTests();

public static class KoreTestCenter
{
    // Central path for all unit test output files
    public static string TestPath => KoreFileOps.JoinPaths(Directory.GetCurrentDirectory(), "UnitTestArtefacts");

    public static KoreTestLog RunCoreTests()
    {
        KoreTestLog testLog = new KoreTestLog();

        try
        {
            if (!EnsureTestDirectory(testLog))
                return testLog;

            // Test Core maths and data structures
            KoreTestMath.RunTests(testLog);
            KoreTestXYZVector.RunTests(testLog);
            KoreTestLine.RunTests(testLog);
            KoreTestTriangle.RunTests(testLog);
            KoreTestList1D.RunTests(testLog);
            KoreTestList2D.RunTests(testLog);
            KoreTestStringDictionary.RunTests(testLog);

            // Test geographic and position classes
            KoreTestPosition.RunTests(testLog);
            KoreTestPositionLLA.RunTests(testLog);
            KoreTestRoute.RunTests(testLog);

            // Shapefile tests (run early since they don't depend on other tests)
            KoreTestShapefile.RunTests(testLog);

            // Graphics: Mesh and color tests
            KoreTestColor.RunTests(testLog);
            KoreTestMesh.RunTests(testLog);
            KoreTestMeshUvOps.RunTests(testLog);
            KoreTestMiniMesh.RunTests(testLog);

            // Database tests
            KoreTestDatabase.RunTests(testLog);

            // SkiaSharp Plotter tests
            KoreTestPlotter.RunTests(testLog);
            KoreTestSkiaSharp.RunTests(testLog);
            KoreTestWorldPlotter.RunTests(testLog);
            KoreTestNatoSymbolPlotter.RunTests(testLog);
        }
        catch (Exception)
        {
            testLog.AddResult("Test Centre Run", false, "Exception");
        }

        return testLog;
    }

    // --------------------------------------------------------------------------------------------

    // Usage: KoreTestCenter.RunAdHocTests()
    public static KoreTestLog RunAdHocTests(KoreTestLog testLog)
    {

        try
        {
            KoreTestXYZVector.TestArbitraryPerpendicular(testLog);
        }
        catch (Exception)
        {
            testLog.AddResult("Test Centre Run", false, "Exception");
        }

        return testLog;
    }

    // --------------------------------------------------------------------------------------------

    private static bool EnsureTestDirectory(KoreTestLog testLog)
    {
        bool retval = false;

        testLog.AddComment("Attempting to create directory at: " + TestPath);

        KoreFileOps.CreateDirectory(TestPath);

        // Verify the directory was actually created
        if (Directory.Exists(TestPath))
        {
            testLog.AddComment("? Test directory successfully created at: " + TestPath);
            retval = true;
        }
        else
        {
            testLog.AddResult("Test Directory Creation", false, $"? Test directory was NOT created at: {TestPath}");
        }

        return retval;
    }

}

