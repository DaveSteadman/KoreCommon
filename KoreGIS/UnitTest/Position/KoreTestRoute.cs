// <fileheader>

using System;
using System.Collections.Generic;

using KoreCommon;
using KoreCommon.UnitTest;
using KoreGIS;
namespace KoreGIS.UnitTest;


public static class KoreTestRoute
{
    public static void RunTests(KoreTestLog testLog)
    {
        try
        {
            TestRouteLegLine(testLog);
            //TestRouteSimpleTurn(testLog);
        }
        catch (Exception ex)
        {
            testLog.AddResult("KoreTestRoute RunTests", false, ex.Message);
        }
    }

    // --------------------------------------------------------------------------------------------------
    // MARK: Route Elements
    // --------------------------------------------------------------------------------------------------


    // Create a route element and query various attributes.

    private static void TestRouteLegLine(KoreTestLog testLog)
    {
        // Setup some standard values
        KoreLLAPoint p1 = new KoreLLAPoint() { LatDegs = 45, LonDegs = 10, AltMslM = 1000 };
        KoreLLAPoint p2 = new KoreLLAPoint() { LatDegs = 48, LonDegs = 11, AltMslM = 1000 };
        double legSpeed = 100; // m/s

        // Determine some values for the leg to check against
        double checkDistance = p1.CurvedDistanceToM(p2); // distance in meters - curved across average altitude.
        double checkDuration = checkDistance / legSpeed; // duration in seconds
        double checkBearingRads = KoreNumericRange<double>.ZeroToTwoPiRadians.Apply(p1.BearingToRads(p2));
        double checkBearingDegs = checkBearingRads * KoreConsts.RadsToDegsMultiplier;
        KoreRangeBearing rbLeg = p1.RangeBearingTo(p2);

        // - - - - - -

        // Create a simple route leg with a straight line
        var leg1 = new KoreRouteLegLine(p1, p2, legSpeed);

        // Validate the leg properties
        testLog.AddResult("Route Leg Length", KoreValueUtils.EqualsWithinTolerance(
            leg1.GetCalculatedDistanceM(), checkDistance, 0.001),
            $"Expected: {checkDistance:F1} m, Actual: {leg1.GetCalculatedDistanceM():F1} m");

        testLog.AddResult("Route Leg Duration", KoreValueUtils.EqualsWithinTolerance(
            leg1.GetDurationS(), checkDuration, 0.001),
            $"Expected: {checkDuration:F1} s, Actual: {leg1.GetDurationS():F1} s");

        testLog.AddResult("Route Leg Bearing",
            KoreValueUtils.EqualsWithinTolerance(leg1.StartCourse.HeadingRads, checkBearingRads, 0.001),
            $"Expected: {checkBearingRads:F3} rad {checkBearingDegs:F3} deg, Actual: {leg1.StartCourse.HeadingRads:F3} rad {leg1.StartCourse.HeadingRads * KoreConsts.RadsToDegsMultiplier:F3} deg");

        // - - - - - -

        // Create a second leg using other constructor params
        var leg2 = new KoreRouteLegLine(p1, rbLeg, legSpeed);

        // Validate the second leg properties
        testLog.AddResult("Route Leg2 Length", KoreValueUtils.EqualsWithinTolerance(
            leg2.GetCalculatedDistanceM(), checkDistance, 0.001),
            $"Expected: {checkDistance:F1} m, Actual: {leg2.GetCalculatedDistanceM():F1} m");

        testLog.AddResult("Route Leg2 Duration", KoreValueUtils.EqualsWithinTolerance(
            leg2.GetDurationS(), checkDuration, 0.001),
            $"Expected: {checkDuration:F1} s, Actual: {leg2.GetDurationS():F1} s");

        testLog.AddResult("Route Leg2 Bearing", KoreValueUtils.EqualsWithinTolerance(
            leg2.StartCourse.HeadingRads, checkBearingRads, 0.001),
            $"Expected: {checkBearingRads:F3} rad {checkBearingDegs:F3} deg, Actual: {leg2.StartCourse.HeadingRads:F3} rad {leg2.StartCourse.HeadingRads * KoreConsts.RadsToDegsMultiplier:F3} deg");
    }

    // --------------------------------------------------------------------------------------------------

    private static void TestRouteSimpleTurn(KoreTestLog testLog)
    {
        // Create a simple turn leg with a 90 degree right turn
        KoreLLAPoint startPoint = new KoreLLAPoint() { LatDegs = 0, LonDegs = 0, AltMslM = 0 };

        KoreCourse startCourse = new KoreCourse()
        {
            SpeedMps = 100, // m/s
            HeadingRads = 90 * KoreConsts.DegsToRadsMultiplier
        };
        double turnRadiusM = 5000; // radius of the turn arc
        double turnAngleRads = 90 * KoreConsts.DegsToRadsMultiplier; // +ve (right) 90 degrees

        KoreLLAPoint turnCentre = KoreRouteLegSimpleTurn.FindTurnPoint(startPoint, startCourse, turnRadiusM, turnAngleRads);

        var turnLeg = new KoreRouteLegSimpleTurn(startPoint, turnCentre, turnRadiusM, turnAngleRads);

        // Now we can test various properties of the turn leg
        double testRouteLen = (2 * Math.PI * 5000) / 4; // quarter circle length at 5000m
        string testComment = $"Turn Leg: Start={startPoint}, Centre={turnCentre}, Radius={turnRadiusM:F1}, Angle={turnAngleRads:F3}, calcLen={turnLeg.GetCalculatedDistanceM():F1} checklen={testRouteLen:F1}";
        testLog.AddResult("Route Length", KoreValueUtils.EqualsWithinTolerance(turnLeg.GetCalculatedDistanceM(), testRouteLen, 0.001), testComment);

    }


    // private static void TestSimpleRoute(KoreTestLog testLog)
    // {
    //     // Build two legs: a straight line followed by a 90 degree right turn
    //     KoreLLAPoint start = new KoreLLAPoint() { LatDegs = 0, LonDegs = 0, AltMslM = 0 };
    //     KoreLLAPoint mid = start.PlusRangeBearing(new KoreRangeBearing(1000, 90 * KoreConsts.DegsToRadsMultiplier));

    //     double speed = 100; // m/s
    //     var leg1 = new KoreRouteLegLine(start, mid, speed);

    //     // Turn centre 500 m to the right of mid point
    //     double centreBearing =KoreNumericRange<double>.ZeroToTwoPiRadians.Apply(leg1.EndCourse.HeadingRads + Math.PI / 2);
    //     KoreLLAPoint centre = mid.PlusRangeBearing(new KoreRangeBearing(500, centreBearing));

    //     var turnLeg = new KoreRouteLegSimpleTurn(mid, centre, Math.PI / 2, speed);

    //     var route = new KoreRoute(new List<IKoreRouteLeg>() { leg1, turnLeg });

    //     testLog.AddResult("Route NumLegs", route.NumLegs() == 2);

    //     double expectedDuration = leg1.GetDurationS() + turnLeg.GetDurationS();
    //     bool durOk = KoreValueUtils.EqualsWithinTolerance(route.GetDurationSeconds(), expectedDuration, 0.001);
    //     testLog.AddResult("Route Duration", durOk);

    //     KoreLLAPoint midOfLine = route.CurrentPosition(leg1.GetDurationS() / 2);
    //     KoreLLAPoint expectedMid = KoreLLAPointOps.RhumbLineInterpolation(start, mid, 0.5);
    //     bool midLat = KoreValueUtils.EqualsWithinTolerance(midOfLine.LatDegs, expectedMid.LatDegs, 0.0001);
    //     bool midLon = KoreValueUtils.EqualsWithinTolerance(midOfLine.LonDegs, expectedMid.LonDegs, 0.0001);
    //     testLog.AddResult("Route Line MidPos", midLat && midLon);

    //     KoreLLAPoint endPos = route.CurrentPosition(expectedDuration);
    //     bool endLat = KoreValueUtils.EqualsWithinTolerance(endPos.LatDegs, turnLeg.EndPoint.LatDegs, 0.0001);
    //     bool endLon = KoreValueUtils.EqualsWithinTolerance(endPos.LonDegs, turnLeg.EndPoint.LonDegs, 0.0001);
    //     testLog.AddResult("Route EndPos", endLat && endLon);

    //     KoreLLAPoint midTurnPos = route.CurrentPosition(leg1.GetDurationS() + turnLeg.GetDurationS() / 2);
    //     KoreLLAPoint expectedMidTurn = turnLeg.PositionAtLegFraction(0.5);
    //     bool mtLat = KoreValueUtils.EqualsWithinTolerance(midTurnPos.LatDegs, expectedMidTurn.LatDegs, 0.0001);
    //     bool mtLon = KoreValueUtils.EqualsWithinTolerance(midTurnPos.LonDegs, expectedMidTurn.LonDegs, 0.0001);
    //     testLog.AddResult("Route Turn MidPos", mtLat && mtLon);
    // }
}
