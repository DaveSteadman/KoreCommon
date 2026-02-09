// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

public partial class KoreColorMesh
{
    // --------------------------------------------------------------------------------------------
    // MARK: Vertices
    // --------------------------------------------------------------------------------------------

    public int AddVertex(KoreXYZVector vertex)
    {
        Vertices[NextVertexId] = vertex;
        return NextVertexId++; // post-increment, we return the value used, then increase it
    }

    // Check if a vertex exists with the given ID
    public bool HasVertex(int vertexId) { return Vertices.ContainsKey(vertexId); }
    public KoreXYZVector GetVertex(int vertexId) { return Vertices[vertexId]; }
    public void RemoveVertexA(int vertexId) { Vertices.Remove(vertexId); }

    // --------------------------------------------------------------------------------------------
    // MARK: Triangles
    // --------------------------------------------------------------------------------------------

    public int AddTriangle(KoreColorMeshTri triangle)
    {
        Triangles[NextTriangleId] = triangle;
        return NextTriangleId++;
    }

    public bool HasTriangle(int triangleId) { return Triangles.ContainsKey(triangleId); }
    public KoreColorMeshTri GetTriangle(int triangleId) { return Triangles[triangleId]; }
    public void RemoveTriangle(int triangleId) { Triangles.Remove(triangleId); }
}
