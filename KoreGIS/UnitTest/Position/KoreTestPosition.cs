// <fileheader>

using System;

using KoreCommon;
using KoreCommon.UnitTest;
using KoreGIS;
namespace KoreGIS.UnitTest;



public static partial class KoreTestPosition
{
    public static void RunTests(KoreTestLog testLog)
    {
        // 2D
        TestKoreXYVector(testLog);
        TestKoreXYLine(testLog);


        // 3D
        TestKoreXYZ(testLog);
        TestKoreXYZLine(testLog);
        //TestKoreXYZPlane(testLog);
    }

    private static void TestKoreXYZ(KoreTestLog testLog)
    {
        // Example: Test creation of KoreXYZVector points and basic operations
        var pointA = new KoreXYZVector(1, 2, 3);
        var pointB = new KoreXYZVector(4, 5, 6);

        testLog.AddResult("KoreXYZVector Creation", pointA.X == 1 && pointA.Y == 2 && pointA.Z == 3);
        testLog.AddResult("KoreXYZVector Distance", Math.Abs(pointA.DistanceTo(pointB) - 5.196) < 0.001); // Example threshold for floating point comparison

        // Add more tests for KoreXYZVector
    }

    private static void TestKoreXYZLine(KoreTestLog testLog)
    {
        // Example: Test KoreXYZLine creation and properties
        var line = new KoreXYZLine(new KoreXYZVector(0, 0, 0), new KoreXYZVector(1, 1, 1));

        testLog.AddResult("KoreXYZLine Length", Math.Abs(line.Length - Math.Sqrt(3)) < 0.001);
        //testLog.Add("KoreXYZLine MidPoint", line.MidPoint().Equals(new KoreXYZVector(0.5, 0.5, 0.5)));

        // Add more tests for KoreXYZLine
    }

    // private static void TestKoreXYZPlane(KoreTestLog testLog)
    // {
    //     // Example: Test KoreXYZPlane creation and normal calculation
    //     KoreXYZPlane plane = new KoreXYZPlane(new KoreXYZVector(0, 0, 0), new KoreXYZVector(1, 0, 0), new KoreXYZVector(0, 1, 0));

    //     bool testColinear = KoreXYZPlaneOps.PointsCollinear(new KoreXYZVector(0, 0, 0), new KoreXYZVector(1, 0, 0), new KoreXYZVector(0, 1, 0));
    //     testLog.Add("KoreXYZPlane Collinear", (testColinear == false));

    //     // testLog.Add("KoreXYZPlane Normal", plane.Normal().Equals(new KoreXYZVector(0, 0, 1)));

    //     // // Example: Test distance from a point to the plane
    //     // var point = new KoreXYZVector(0, 0, 1);
    //     // testLog.Add("KoreXYZPlane DistanceFromPlane", Math.Abs(KoreXYZPlane.DistanceFromPlane(point, plane) - 1) < 0.001);

    //     // Add more tests for KoreXYZPlane
    // }

    // Additional helper methods or test cases as needed
}
