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
    // Remove normals that don't have supporting vertex IDs

    public static void RemoveBrokenNormals(KoreMeshData mesh)
    {
        var invalidNormalIds = mesh.Normals.Keys.Where(id => !mesh.Vertices.ContainsKey(id)).ToList();
        foreach (int normalId in invalidNormalIds)
        {
            mesh.Normals.Remove(normalId);
        }
    }

    // --------------------------------------------------------------------------------------------

    // Create missing normals for vertices

    public static void CreateMissingNormals(KoreMeshData mesh, KoreXYZVector? defaultNormal = null)
    {
        KoreXYZVector normal = defaultNormal ?? new KoreXYZVector(0, 1, 0); // Default to up vector

        foreach (int vertexId in mesh.Vertices.Keys)
        {
            if (!mesh.Normals.ContainsKey(vertexId))
            {
                mesh.Normals[vertexId] = normal;
            }
        }
    }

    // --------------------------------------------------------------------------------------------

    // Calculate normal for a triangle

    public static KoreXYZVector NormalForTriangle(KoreMeshData mesh, int triangleId)
    {
        // Check if triangle exists
        if (!mesh.Triangles.ContainsKey(triangleId))
            return new KoreXYZVector(0, 1, 0);

        var triangle = mesh.Triangles[triangleId];

        // Ensure all vertices exist
        if (!mesh.Vertices.ContainsKey(triangle.A) ||
            !mesh.Vertices.ContainsKey(triangle.B) ||
            !mesh.Vertices.ContainsKey(triangle.C))
            return new KoreXYZVector(0, 1, 0);

        var vertexA = mesh.Vertices[triangle.A];
        var vertexB = mesh.Vertices[triangle.B];
        var vertexC = mesh.Vertices[triangle.C];

        // Calculate cross product for normal
        // For CW winding: use (A→C) × (A→B) to get outward-facing normal
        var edge1 = vertexC - vertexA;  // A → C
        var edge2 = vertexB - vertexA;  // A → B

        // Cross product: edge1 × edge2 = (A→C) × (A→B)
        var normal = KoreXYZVector.CrossProduct(edge1, edge2);

        // Normalize the normal vector
        var length = Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z);
        if (length > 0.0001) // Avoid division by zero
            normal = new KoreXYZVector(normal.X / length, normal.Y / length, normal.Z / length);
        else
            normal = new KoreXYZVector(0, 1, 0); // Default up vector

        return normal;
    }

    // --------------------------------------------------------------------------------------------

    // Set normal from first triangle that uses this vertex

    public static void SetNormalFromFirstTriangle(KoreMeshData mesh, int vertexId)
    {
        foreach (var triangleKvp in mesh.Triangles)
        {
            int triangleId = triangleKvp.Key;
            var triangle = triangleKvp.Value;
            if (triangle.A == vertexId || triangle.B == vertexId || triangle.C == vertexId)
            {
                var normal = NormalForTriangle(mesh, triangleId);
                mesh.Normals[vertexId] = normal;
                return;
            }
        }

        // If no triangle found, set default normal
        mesh.Normals[vertexId] = new KoreXYZVector(0, 1, 0);
    }

    // --------------------------------------------------------------------------------------------

    // Set normals from triangles for all vertices

    public static void SetNormalsFromTriangles(KoreMeshData mesh)
    {
        // Clear existing normals
        mesh.Normals.Clear();

        // Calculate normals for each vertex based on triangles
        foreach (int vertexId in mesh.Vertices.Keys)
        {
            SetNormalFromFirstTriangle(mesh, vertexId);
        }
    }

    // --------------------------------------------------------------------------------------------

    public static void FlipAllNormals(KoreMeshData mesh)
    {
        // Loop through all the normals and flip their direction
        foreach (var kvp in mesh.Normals)
        {
            int vertexId = kvp.Key;
            KoreXYZVector normal = kvp.Value;

            // Flip the normal by inverting its direction
            mesh.Normals[vertexId] = normal.Invert();
        }
    }

}
