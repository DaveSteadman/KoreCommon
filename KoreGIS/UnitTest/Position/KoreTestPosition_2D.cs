// <fileheader>

using System;

using KoreCommon;
using KoreCommon.UnitTest;
using KoreGIS;
namespace KoreGIS.UnitTest;


public static partial class KoreTestPosition
{
    private static void TestKoreXYVector(KoreTestLog testLog)
    {
        // Example: Test creation of KoreXYVector points and basic operations
        var pointA = new KoreXYVector(1, 2);
        var pointB = new KoreXYVector(4, 5);

        testLog.AddResult("KoreXYVector Creation", pointA.X == 1 && pointA.Y == 2);

        double calcDistance = Math.Sqrt((4 - 1) * (4 - 1) + (5 - 2) * (5 - 2));
        testLog.AddResult("KoreXYVector Distance", KoreValueUtils.EqualsWithinTolerance(pointA.DistanceTo(pointB), calcDistance, 0.001));


        // Add more tests for KoreXYZVector
    }

    private static void TestKoreXYLine(KoreTestLog testLog)
    {

    }



}
