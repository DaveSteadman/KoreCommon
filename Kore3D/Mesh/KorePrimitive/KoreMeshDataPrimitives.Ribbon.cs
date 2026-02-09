// <fileheader>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace KoreCommon;

// Static class to create KoreMeshData primitives
// This class is used to create various 3D shapes and meshes in Godot

public static partial class KoreMeshDataPrimitives
{
    // Ribbon: Creates a ribbon mesh of points, wih the left and right sides from the perspect of looking down the ribbon fro the starting edge
    // The normals are generated automatically from the triangles.
    public static KoreMeshData Ribbon(
        List<KoreXYZVector> leftPoints, List<KoreXYVector> leftUVs,
        List<KoreXYZVector> rightPoints, List<KoreXYVector> rightUVs,
        bool isClosed = false)
    {
        var mesh = new KoreMeshData();

        if (leftPoints.Count != rightPoints.Count || leftUVs.Count != rightUVs.Count)
            throw new ArgumentException("Left and right points/UVs must have the same count.");

        if (leftPoints.Count < 2)
            throw new ArgumentException("Ribbon needs at least 2 points.");

        // Create lists to store vertex IDs
        var leftVertexIds = new List<int>();
        var rightVertexIds = new List<int>();

        // Add all vertices and collect their IDs
        for (int i = 0; i < leftPoints.Count; i++)
        {
            int leftId  = mesh.AddCompleteVertex(leftPoints[i], null, null, leftUVs[i]);
            int rightId = mesh.AddCompleteVertex(rightPoints[i], null, null, rightUVs[i]);

            leftVertexIds.Add(leftId);
            rightVertexIds.Add(rightId);
        }

        // Add cross lines connecting left and right sides
        for (int i = 0; i < leftVertexIds.Count; i++)
        {
            mesh.AddLine(leftVertexIds[i], rightVertexIds[i]);
        }

        // Add longitudinal lines along left and right edges
        for (int i = 0; i < leftVertexIds.Count - 1; i++)
        {
            mesh.AddLine(leftVertexIds[i], leftVertexIds[i + 1]);
            mesh.AddLine(rightVertexIds[i], rightVertexIds[i + 1]);
        }

        // If closed, connect the last points back to the first
        if (isClosed)
        {
            int lastIndex = leftVertexIds.Count - 1;
            mesh.AddLine(leftVertexIds[lastIndex], leftVertexIds[0]);
            mesh.AddLine(rightVertexIds[lastIndex], rightVertexIds[0]);
        }

        // Create triangles for each segment
        for (int i = 0; i < leftVertexIds.Count - 1; i++)
        {
            int leftCurrent = leftVertexIds[i];
            int leftNext = leftVertexIds[i + 1];
            int rightCurrent = rightVertexIds[i];
            int rightNext = rightVertexIds[i + 1];

            // Create two triangles for the current segment - CW winding
            mesh.AddTriangle(leftCurrent, rightCurrent, rightNext);
            mesh.AddTriangle(leftCurrent, rightNext, leftNext);
        }

        // If closed, add the final triangles connecting end to start
        if (isClosed)
        {
            int lastIndex = leftVertexIds.Count - 1;
            int leftLast = leftVertexIds[lastIndex];
            int leftFirst = leftVertexIds[0];
            int rightLast = rightVertexIds[lastIndex];
            int rightFirst = rightVertexIds[0];

            // Create two triangles for the closing segment
            mesh.AddTriangle(leftLast, rightLast, rightFirst);
            mesh.AddTriangle(leftLast, rightFirst, leftFirst);
        }

        // We've added the triangles okay, so we can now loop through them and auto-calculate normals
        KoreMeshDataEditOps.SetNormalsFromTriangles(mesh);

        return mesh;
    }

}
