// <fileheader>

using System;
using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;


// KoreLLAPointOps: A static class to hold operations on KoreLLAPoint objects that are not part of its
// core responsibilites. This class is static, all operations are stateless and return a new object.

// Design Decisions:
// - The LLA Point code uses a struct rather than an immutable class, as the constructor options with flexible units
//   are simply too useful. We rely on the struct's pass by value to avoid issues with mutability.

public static class KoreGCPathOps
{
    // Usage: KoreGCPathOps.PointsOnGCPath(startLLA, endLLA, 1000)
    public static List<KoreLLAPoint> PointsOnGCPath(KoreLLAPoint startLLA, KoreLLAPoint endLLA, double sepDistM)
    {
        // Create the returning list and add the start point
        List<KoreLLAPoint> retList = new List<KoreLLAPoint>();
        retList.Add(startLLA);

        // Create the GC path and find its distance, to determine the intermediary points along the path
        KoreGCPath    gcPath   = new KoreGCPath(startLLA, endLLA);
        double       pathLenM = gcPath.PathDistance();
        //const double minDistM = 1000; // 1 km minimum distance

        // Calculate the number of points to add
        int numPoints = (int)(pathLenM / sepDistM);
        double stepFraction = 1.0 / numPoints;

        for (int i = 1; i < numPoints; i++)
        {
            double distAlongPath = i * sepDistM;
            KoreLLAPoint newPoint = KoreLLAPoint.FromXYZ(gcPath.PositionAtFractionOfRoute(stepFraction));
            retList.Add(newPoint);
        }
        retList.Add(endLLA);

        return retList;
    }

    public static List<KoreLLAPoint> PointsOnGCPath2(KoreLLAPoint startLLA, KoreLLAPoint endLLA, int numPoints)
    {
        // Create the returning list and add the start point
        List<KoreLLAPoint> retList = new List<KoreLLAPoint>();
        retList.Add(startLLA);

        // Create the GC path and find its distance, to determine the intermediary points along the path
        KoreGCPath    gcPath   = new KoreGCPath(startLLA, endLLA);

        // Calculate the number of points to add
        double stepFraction = 1.0 / numPoints;

        for (int i = 1; i < numPoints; i++)
        {
            double currFraction = stepFraction * i;
            KoreLLAPoint newPoint = KoreLLAPoint.FromXYZ(gcPath.PositionAtFractionOfRoute(stepFraction));
            retList.Add(newPoint);
        }
        retList.Add(endLLA);

        return retList;
    }

}

