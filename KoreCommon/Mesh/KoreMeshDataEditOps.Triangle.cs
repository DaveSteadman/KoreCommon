// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

// Static validity and cleanup operations for KoreMeshData
// Contains methods for mesh validation, cleanup, and population of missing data
public static partial class KoreMeshDataEditOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Triangles
    // --------------------------------------------------------------------------------------------

    // Remove triangles that don't have supporting vertex IDs

    public static void RemoveBrokenTriangles(KoreMeshData mesh)
    {
        var invalidTriangleIds = mesh.Triangles.Where(kvp =>
            !mesh.Vertices.ContainsKey(kvp.Value.A) ||
            !mesh.Vertices.ContainsKey(kvp.Value.B) ||
            !mesh.Vertices.ContainsKey(kvp.Value.C))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (int triangleId in invalidTriangleIds)
        {
            mesh.Triangles.Remove(triangleId);
        }
    }

    // Remove duplicate triangles

    public static void RemoveDuplicateTriangles(KoreMeshData mesh)
    {
        var trianglesToRemove = new List<int>();
        var trianglesArray = mesh.Triangles.ToArray();

        for (int i = 0; i < trianglesArray.Length; i++)
        {
            for (int j = i + 1; j < trianglesArray.Length; j++)
            {
                var tri1 = trianglesArray[i].Value;
                var tri2 = trianglesArray[j].Value;

                // Check if triangles have the same vertices (any permutation)
                var vertices1 = new HashSet<int> { tri1.A, tri1.B, tri1.C };
                var vertices2 = new HashSet<int> { tri2.A, tri2.B, tri2.C };

                if (vertices1.SetEquals(vertices2))
                {
                    trianglesToRemove.Add(trianglesArray[j].Key);
                }
            }
        }

        foreach (int triangleId in trianglesToRemove)
        {
            mesh.Triangles.Remove(triangleId);
        }
    }


    // --------------------------------------------------------------------------------------------
    // MARK: Winding
    // --------------------------------------------------------------------------------------------

    public static void FlipTriangleWinding(KoreMeshData mesh, int triId)
    {
        if (!mesh.Triangles.ContainsKey(triId))
            return;

        KoreMeshTriangle triangle = mesh.Triangles[triId];

        // Swap the vertices to flip the winding
        int temp = triangle.B;
        triangle.B = triangle.C;
        triangle.C = temp;

        // Update the triangle in the dictionary
        mesh.Triangles[triId] = triangle;
    }

    public static void FlipAllTriangleWindings(KoreMeshData mesh)
    {
        // Loop through all triangles and flip their winding
        foreach (var kvp in mesh.Triangles.ToList())
        {
            int triangleId = kvp.Key;
            FlipTriangleWinding(mesh, triangleId);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: TRIANGLES
    // --------------------------------------------------------------------------------------------

    // Isolates a triangle by creating duplicate vertices if they are shared with other triangles.
    // This ensures the triangle has its own unique vertices that can be modified independently.
    public static void IsolateTriangle(KoreMeshData mesh, int triId)
    {
        if (!mesh.Triangles.ContainsKey(triId))
            return;

        KoreMeshTriangle triangle = mesh.Triangles[triId];

        // Find which vertices are shared with other triangles
        HashSet<int> sharedVertices = FindSharedVertices(mesh, triId);

        // Create new vertices for any shared ones
        int newA = sharedVertices.Contains(triangle.A) ? DuplicateVertex(mesh, triangle.A) : triangle.A;
        int newB = sharedVertices.Contains(triangle.B) ? DuplicateVertex(mesh, triangle.B) : triangle.B;
        int newC = sharedVertices.Contains(triangle.C) ? DuplicateVertex(mesh, triangle.C) : triangle.C;

        // Update the triangle with new vertex IDs
        mesh.Triangles[triId] = new KoreMeshTriangle(newA, newB, newC);
    }

    // Finds vertices of a triangle that are shared with other triangles
    private static HashSet<int> FindSharedVertices(KoreMeshData mesh, int targetTriangleId)
    {
        if (!mesh.Triangles.ContainsKey(targetTriangleId))
            return new HashSet<int>();

        KoreMeshTriangle targetTriangle = mesh.Triangles[targetTriangleId];
        HashSet<int> targetVertices = new HashSet<int> { targetTriangle.A, targetTriangle.B, targetTriangle.C };
        HashSet<int> sharedVertices = new HashSet<int>();

        // Check all other triangles for shared vertices
        foreach (var kvp in mesh.Triangles)
        {
            if (kvp.Key == targetTriangleId)
                continue;

            KoreMeshTriangle otherTriangle = kvp.Value;

            if (targetVertices.Contains(otherTriangle.A)) sharedVertices.Add(otherTriangle.A);
            if (targetVertices.Contains(otherTriangle.B)) sharedVertices.Add(otherTriangle.B);
            if (targetVertices.Contains(otherTriangle.C)) sharedVertices.Add(otherTriangle.C);
        }

        return sharedVertices;
    }

    // Usage: KoreMeshDataEditOps.IsolateAllTriangles(mesh);
    public static void IsolateAllTriangles(KoreMeshData mesh)
    {
        foreach (var kvp in mesh.Triangles)
        {
            IsolateTriangle(mesh, kvp.Key);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Delete
    // --------------------------------------------------------------------------------------------

    // Deletes the triangle definition and any use in groups.

    // Usage: KoreMeshDataEditOps.DeleteTriangle(mesh, triangleId);
    public static void DeleteTriangle(KoreMeshData mesh, int triId)
    {
        if (!mesh.Triangles.ContainsKey(triId))
            return;

        KoreMeshTriangle triangle = mesh.Triangles[triId];

        // Remove the triangle
        mesh.Triangles.Remove(triId);

        // loop through each group and remove the id from its list
        foreach (var group in mesh.NamedTriangleGroups.Values)
        {
            group.TriangleIds.Remove(triId);
        }
    }


}
