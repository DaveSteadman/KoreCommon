// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

public static partial class KoreMiniMeshOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Circle Points
    // --------------------------------------------------------------------------------------------

    // Function to create a circle of points, returning the list of Ids
    // KoreMiniMeshOps.AddCirclePoints(mesh, center, normal, radius, numSides, referenceDirection);
    public static List<int> AddCirclePoints(
        KoreMiniMesh mesh,
        KoreXYZVector center,
        KoreXYZVector normal,
        double radius,
        int numSides,
        KoreXYZVector? referenceDirection = null)
    {
        if (numSides < 3) throw new ArgumentException("Circle must have at least 3 sides");

        List<int> pointIds = new List<int>();

        // Create a plane for the circle using KoreXYZPlane
        KoreXYZPlane plane;

        if (referenceDirection.HasValue)
        {
            // Use provided reference direction as the plane's Y-axis
            plane = KoreXYZPlane.MakePlane(center, normal, referenceDirection.Value);
        }
        else
        {
            // Use automatic reference direction selection
            KoreXYZVector autoReference = FindPerpendicularVector(normal.Normalize());
            plane = KoreXYZPlane.MakePlane(center, normal, autoReference);
        }

        // Generate circle points using the plane's 2D->3D projection
        double angleStep = Math.Tau / numSides;

        for (int i = 0; i < numSides; i++)
        {
            double angle = i * angleStep;

            // Create 2D point in the plane's coordinate system
            var point2D = new KoreXYVector(
                radius * Math.Cos(angle),
                radius * Math.Sin(angle)
            );

            // Project to 3D using the plane
            KoreXYZVector point3D = plane.Project2DTo3D(point2D);

            int vertexId = mesh.AddVertex(point3D);
            pointIds.Add(vertexId);
        }

        return pointIds;
    }

    // --------------------------------------------------------------------------------------------

    // Find a vector perpendicular to the given vector using a consistent strategy
    private static KoreXYZVector FindPerpendicularVector(KoreXYZVector vector)
    {
        // Strategy: Try standard basis vectors and pick the one that's most perpendicular
        KoreXYZVector[] candidates = { KoreXYZVector.Right, KoreXYZVector.Up, KoreXYZVector.Forward };

        double minDot = double.MaxValue;
        KoreXYZVector bestCandidate = KoreXYZVector.Right;

        foreach (var candidate in candidates)
        {
            double dot = Math.Abs(KoreXYZVector.DotProduct(vector, candidate));
            if (dot < minDot)
            {
                minDot = dot;
                bestCandidate = candidate;
            }
        }

        // Return the most perpendicular candidate (will be made orthogonal by MakePlane)
        return bestCandidate;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Shape Parts
    // --------------------------------------------------------------------------------------------

    // Create a ribbon surface connecting two circles of vertices
    public static List<int> AddRibbon(
        KoreMiniMesh mesh,
        List<int> circle1,
        List<int> circle2)
    {
        if (circle1.Count != circle2.Count || circle1.Count < 3)
            throw new ArgumentException("Circles must have the same count and at least 3 vertices");

        List<int> triangleIds = new List<int>();
        int sides = circle1.Count;

        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;

            // Create a quad between corresponding points on the two circles
            int v1 = circle1[i];      // current circle1
            int v2 = circle2[i];      // current circle2
            int v3 = circle2[next];   // next circle2
            int v4 = circle1[next];   // next circle1

            // Add the quad as two triangles
            triangleIds.AddRange(KoreMiniMeshOps.AddFace(mesh, v1, v2, v3, v4));
        }

        return triangleIds;
    }

    // --------------------------------------------------------------------------------------------

    // Create a fan surface from a center point to a circle of vertices
    public static List<int> AddFan(
        KoreMiniMesh mesh,
        int centerVertex,
        List<int> circleVertices,
        bool windingOutward = true)
    {
        if (circleVertices.Count < 3)
            throw new ArgumentException("Circle must have at least 3 vertices");

        List<int> triangleIds = new List<int>();

        for (int i = 0; i < circleVertices.Count; i++)
        {
            int next = (i + 1) % circleVertices.Count;

            int v1 = centerVertex;
            int v2 = circleVertices[i];
            int v3 = circleVertices[next];

            // Wind triangles based on direction parameter
            if (windingOutward)
            {
                triangleIds.Add(mesh.AddTriangle(new KoreMiniMeshTri(v1, v2, v3)));
            }
            else
            {
                triangleIds.Add(mesh.AddTriangle(new KoreMiniMeshTri(v1, v3, v2)));
            }
        }

        return triangleIds;
    }

    // --------------------------------------------------------------------------------------------

    // Add circle lines connecting vertices in a circle
    public static void AddCircleLines(
        KoreMiniMesh mesh,
        List<int> circleVertices,
        int lineColorId)
    {
        for (int i = 0; i < circleVertices.Count; i++)
        {
            int next = (i + 1) % circleVertices.Count;
            mesh.AddLine(new KoreMiniMeshLine(circleVertices[i], circleVertices[next], lineColorId));
        }
    }

    // --------------------------------------------------------------------------------------------
    //
    // --------------------------------------------------------------------------------------------

}


