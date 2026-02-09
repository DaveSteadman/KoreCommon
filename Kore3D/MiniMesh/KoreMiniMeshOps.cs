// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

public static partial class KoreMiniMeshOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Vertices
    // --------------------------------------------------------------------------------------------


    // --------------------------------------------------------------------------------------------
    // MARK: Normals
    // --------------------------------------------------------------------------------------------

    // Usage: KoreXYZVector triNorm = KoreMiniMeshOps.CalculateFaceNormal(mesh, tri);

    public static KoreXYZVector CalculateFaceNormal(KoreMiniMesh mesh, KoreMiniMeshTri tri)
    {
        // Get the vertex positions
        var vA = mesh.GetVertex(tri.A);
        var vB = mesh.GetVertex(tri.B);
        var vC = mesh.GetVertex(tri.C);

        // Calc the edges
        var abEdge = vA.XYZTo(vB);
        var acEdge = vA.XYZTo(vC);

        // Cross product and magnitude
        KoreXYZVector normal = KoreXYZVectorOps.CrossProduct(acEdge, abEdge);
        double length = normal.Magnitude;

        // Normalize the normal vector
        if (length > 0)
            return normal.Normalize();

        return KoreXYZVector.Zero;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Colors
    // --------------------------------------------------------------------------------------------

    public static bool HasColor(KoreMiniMesh mesh, int colorId) { return mesh.Colors.ContainsKey(colorId); }
    public static bool HasColor(KoreMiniMesh mesh, KoreColorRGB color) { return mesh.Colors.ContainsValue(color); }

    public static int GetColorId(KoreMiniMesh mesh, KoreColorRGB color)
    {
        foreach (var kvp in mesh.Colors)
        {
            if (kvp.Value.Equals(color))
                return kvp.Key;
        }
        return -1;
    }
    
    public static int GetOrCreateColorId(KoreMiniMesh mesh, KoreColorRGB color)
    {
        int id = GetColorId(mesh, color);
        if (id >= 0)
            return id;

        return mesh.AddColor(color);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Lines
    // --------------------------------------------------------------------------------------------





    // --------------------------------------------------------------------------------------------
    // MARK: Triangles
    // --------------------------------------------------------------------------------------------

    // Define four points of a face, in CW order, to be stored as two new triangles.
    // return a list of the new triangle IDs

    // A -- B
    // |    |
    // D -- C

    public static List<int> AddFace(KoreMiniMesh mesh, int a, int b, int c, int d)
    {
        var triangleIds = new List<int>();

        // Split the quad into two triangles using a fan from vertex a
        // Triangle 1: a -> b -> c
        triangleIds.Add(mesh.AddTriangle(new KoreMiniMeshTri(a, b, c)));

        // Triangle 2: a -> c -> d
        triangleIds.Add(mesh.AddTriangle(new KoreMiniMeshTri(a, c, d)));

        return triangleIds;
    }


}
