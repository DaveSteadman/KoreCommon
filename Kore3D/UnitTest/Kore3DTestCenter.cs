// <fileheader>

using System;
using System.IO;
using KoreCommon;
namespace KoreCommon.UnitTest;


// Usage: KoreTestLog testLog = KoreTestCenter.RunCoreTests();

public static class Kore3DTestCenter
{
    // Central path for all unit test output files
    public static string TestPath => KoreFileOps.JoinPaths(Directory.GetCurrentDirectory(), "UnitTestArtefacts");

    public static void RunTests(KoreTestLog testLog)
    {
        try
        {
            if (!EnsureTestDirectory(testLog))
            {
                testLog.AddResult("Test Centre Run", false, "Failed to create test directory.");
                return;
            }



            // Graphics: Mesh and color tests
            KoreTestColor.RunTests(testLog);
            KoreTestMesh.RunTests(testLog);
            KoreTestMeshUvOps.RunTests(testLog);
            KoreTestMiniMesh.RunTests(testLog);

        }
        catch (Exception)
        {
            testLog.AddResult("Test Centre Run", false, "Exception");
        }
    }

    // --------------------------------------------------------------------------------------------

    // Usage: KoreTestCenter.RunAdHocTests()
    public static void RunAdHocTests(KoreTestLog testLog)
    {
        try
        {
            KoreTestXYZVector.TestArbitraryPerpendicular(testLog);
        }
        catch (Exception)
        {
            testLog.AddResult("Test Centre Run", false, "Exception");
        }
    }

    // --------------------------------------------------------------------------------------------

    // Called from the higher level namespace Unit Test Centers to ensure the test directory exists
    // Usage: bool success = KoreTestCenter.EnsureTestDirectory(testLog);
    public static bool EnsureTestDirectory(KoreTestLog testLog)
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

