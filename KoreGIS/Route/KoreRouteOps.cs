// <fileheader>

using System;
using System.Collections.Generic;


using KoreCommon;

namespace KoreGIS;



// KoreRouteOps: A class of static methods that perform operations on routes, such as merging, splitting, and transforming them.

public static class KoreRouteOps
{
    // --------------------------------------------------------------------------------------------
    // MARK Append
    // --------------------------------------------------------------------------------------------

    public static bool AppendLegLine(KoreRoute route, KoreLLAPoint newEndPoint)
    {
        //if (route == null || newEndPoint == null)
        //    return false;

        // Get the last leg of the route
        if (route.NumLegs() == 0)
            return false;

        IKoreRouteLeg lastLeg = route.LastLeg();

        // If the last leg is a line leg, extend it
        if (lastLeg is KoreRouteLegLine lineLeg)
        {
            lineLeg.EndPoint = newEndPoint;
            return true;
        }

        // Otherwise, create a new line leg and append it
        double speedMpsNew = 10; // Assuming a default speed for the new leg
        KoreRouteLegLine newLineLeg = new KoreRouteLegLine(lastLeg.EndPoint, newEndPoint, speedMpsNew);
        route.AppendLeg(newLineLeg);
        return true;
    }

    // --------------------------------------------------------------------------------------------

    public static KoreRoute StraightLineRouteFromPoints(List<KoreLLAPoint> points, double speedMps)
    {
        if (points == null || points.Count < 2)
            throw new ArgumentException("At least two points are required to create a route.");

        KoreRoute route = new KoreRoute();

        for (int i = 0; i < points.Count - 1; i++)
        {
            KoreLLAPoint startPoint = points[i];
            KoreLLAPoint endPoint = points[i + 1];
            KoreRouteLegLine leg = new KoreRouteLegLine(startPoint, endPoint, speedMps);
            route.AppendLeg(leg);
        }

        return route;
    }

    // --------------------------------------------------------------------------------------------

}

