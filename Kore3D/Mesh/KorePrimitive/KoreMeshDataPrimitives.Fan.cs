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
    // Fan: Creates a new (isolated) fan mesh of triangles from a list of points, with the origin point at the (user defined) center.
    // - Closed flag indicates if the last point connects back to the first point, useful for capping cylinders etc.
    public static KoreMeshData Fan(
        KoreXYZVector origin, List<KoreXYZVector> fanPoints, bool isClosed)
    {
        var mesh = new KoreMeshData();

        if (fanPoints.Count < 2)
            throw new ArgumentException("Fan points must have at least 2 points.");

        // Add the origin
        int originId = mesh.AddCompleteVertex(origin, null, null, null);

        // Add the list of points
        List<int> pointIds = new List<int>();
        foreach (var point in fanPoints)
            pointIds.Add(mesh.AddCompleteVertex(point, null, null, null));

        // loop through the points list adding the lines
        foreach(int pointId in pointIds)
        {
            // Add a line from the origin to the point
            mesh.AddLine(originId, pointId);
        }

        // Loop through the points list (-1), and use the current position and the next position to create two triangles at a time.
        for (int i = 0; i < pointIds.Count - 1; i++)
        {
            // Get the current and next positions
            int currentId = pointIds[i];
            int nextId = pointIds[i + 1];

            // Add the triangles
            mesh.AddTriangle(originId, currentId, nextId);
        }

        // If closed, add the last triangle to close the fan
        if (isClosed)
        {
            int lastId = pointIds[pointIds.Count - 1];
            mesh.AddTriangle(originId, lastId, pointIds[0]);
        }

        // We've added the triangles okay, so we can now loop through them and auto-calculate normals
        KoreMeshDataEditOps.SetNormalsFromTriangles(mesh);

        return mesh;
    }

}
