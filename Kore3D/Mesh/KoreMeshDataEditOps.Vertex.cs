// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

// KoreMeshDataEditOps: A static class to hold functions to edit a mesh

public static partial class KoreMeshDataEditOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Duplicate
    // --------------------------------------------------------------------------------------------

    // Creates a duplicate vertex with all associated data (normal, UV, color)

    private static int DuplicateVertex(KoreMeshData mesh, int originalVertexId)
    {
        if (!mesh.Vertices.ContainsKey(originalVertexId))
            return originalVertexId;

        // Get original vertex data
        KoreXYZVector vertex = mesh.Vertices[originalVertexId];

        KoreXYZVector? normal = mesh.Normals.ContainsKey(originalVertexId)      ? mesh.Normals[originalVertexId]      : null;
        KoreXYVector?  uv     = mesh.UVs.ContainsKey(originalVertexId)          ? mesh.UVs[originalVertexId]          : null;
        KoreColorRGB?  color  = mesh.VertexColors.ContainsKey(originalVertexId) ? mesh.VertexColors[originalVertexId] : null;

        // Create new vertex with all associated data
        return mesh.AddCompleteVertex(vertex, normal, color, uv);
    }

    // --------------------------------------------------------------------------------------------

    // Remove duplicate vertices within tolerance

    public static void RemoveDuplicateVertices(KoreMeshData mesh, double tolerance = KoreConsts.ArbitrarySmallDouble)
    {
        var verticesArray = mesh.Vertices.ToArray();
        var toRemove = new List<int>();

        for (int i = 0; i < verticesArray.Length; i++)
        {
            for (int j = i + 1; j < verticesArray.Length; j++)
            {
                int vertex1Id = verticesArray[i].Key;
                int vertex2Id = verticesArray[j].Key;
                var vertex1 = verticesArray[i].Value;
                var vertex2 = verticesArray[j].Value;
                double dist = vertex1.DistanceTo(vertex2);

                // Check if vertices are within tolerance
                if (dist < tolerance)
                {
                    // Mark the second vertex for removal and update references
                    toRemove.Add(vertex2Id);

                    // Update line references
                    foreach (var lineKvp in mesh.Lines)
                    {
                        var line = lineKvp.Value;
                        if (line.A == vertex2Id) line.A = vertex1Id;
                        if (line.B == vertex2Id) line.B = vertex1Id;
                        mesh.Lines[lineKvp.Key] = line;
                    }

                    // Update triangle references
                    foreach (var triangleKvp in mesh.Triangles)
                    {
                        var triangle = triangleKvp.Value;
                        if (triangle.A == vertex2Id) triangle.A = vertex1Id;
                        if (triangle.B == vertex2Id) triangle.B = vertex1Id;
                        if (triangle.C == vertex2Id) triangle.C = vertex1Id;
                        mesh.Triangles[triangleKvp.Key] = triangle;
                    }
                }
            }
        }

        // Remove duplicate vertices
        foreach (int delVertexId in toRemove)
        {
            mesh.Vertices.Remove(delVertexId);
            mesh.Normals.Remove(delVertexId);
            mesh.VertexColors.Remove(delVertexId);
            mesh.UVs.Remove(delVertexId);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Broken
    // --------------------------------------------------------------------------------------------

    // Create a hash set of all the used vertex IDs, then remove any points that are not in that set.

    public static void RemoveBrokenVertices(KoreMeshData mesh)
    {
        // Determine which vertices are referenced by lines or triangles
        HashSet<int> referencedVertices = new HashSet<int>();

        // Add vertices referenced by lines
        foreach (var line in mesh.Lines.Values)
        {
            referencedVertices.Add(line.A);
            referencedVertices.Add(line.B);
        }

        // Add vertices referenced by triangles
        foreach (var triangle in mesh.Triangles.Values)
        {
            referencedVertices.Add(triangle.A);
            referencedVertices.Add(triangle.B);
            referencedVertices.Add(triangle.C);
        }

        // Remove unreferenced vertices (and their associated indexed items)
        var verticesToRemove = mesh.Vertices.Keys.Where(id => !referencedVertices.Contains(id)).ToList();

        foreach (int vertexId in verticesToRemove)
        {
            mesh.Vertices.Remove(vertexId);
            mesh.Normals.Remove(vertexId);
            mesh.VertexColors.Remove(vertexId);
            mesh.UVs.Remove(vertexId);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: OFFSET
    // --------------------------------------------------------------------------------------------

    public static void OffsetVertex(KoreMeshData mesh, int vertexId, KoreXYZVector offset)
    {
        // We want to throw here, because we have a unique ID concept and random new additions break this
        if (!mesh.Vertices.ContainsKey(vertexId))
            throw new ArgumentOutOfRangeException(nameof(vertexId), "Vertex ID is not found.");

        // Offset the vertex by the given offset vector
        mesh.Vertices[vertexId] = mesh.Vertices[vertexId] + offset;
    }

    public static void OffsetAllVertices(KoreMeshData mesh, KoreXYZVector offset)
    {
        foreach (var vertexId in mesh.Vertices.Keys)
        {
            OffsetVertex(mesh, vertexId, offset);
        }
    }

    // --------------------------------------------------------------------------------------------




}
