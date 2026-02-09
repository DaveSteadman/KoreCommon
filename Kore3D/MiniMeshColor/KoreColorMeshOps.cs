// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

public static partial class KoreColorMeshOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Vertices
    // --------------------------------------------------------------------------------------------

    // Usage: KoreColorRGB color = KoreColorMeshOps.FirstColorForVertex(mesh, vertexId);

    public static KoreColorRGB FirstColorForVertex(KoreColorMesh mesh, int vertexId)
    {
        foreach (var tri in mesh.Triangles.Values)
        {
            if (tri.A == vertexId || tri.B == vertexId || tri.C == vertexId)
                return tri.Color;
        }

        return KoreColorRGB.White; // Default color if no triangles found
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Normals
    // --------------------------------------------------------------------------------------------

    // Usage: KoreXYZVector triNorm = KoreColorMeshOps.CalculateFaceNormal(mesh, tri);

    public static KoreXYZVector CalculateFaceNormal(KoreColorMesh mesh, KoreColorMeshTri tri)
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

    // Usage: KoreColorMeshOps.SetAllColors(mesh, KoreColorRGB.Red);

    public static void SetAllColors(KoreColorMesh mesh, KoreColorRGB color)
    {
        foreach (var kvp in mesh.Triangles.ToList()) // ToList() to avoid modification during iteration
        {
            int triId = kvp.Key;
            var tri = kvp.Value;

            // Create new struct with updated color
            var updatedTri = tri with { Color = color }; // C# record syntax
                                                         // OR if it's a regular struct:
                                                         // var updatedTri = new KoreColorMeshTri(tri.A, tri.B, tri.C, color);

            mesh.Triangles[triId] = updatedTri;
        }
    }

    public static void SetAllColorsWithNoise(KoreColorMesh mesh, KoreColorRGB color, float noiseAmount)
    {
        foreach (var kvp in mesh.Triangles.ToList()) // ToList() to avoid modification during iteration
        {
            int triId = kvp.Key;
            var tri = kvp.Value;

            // Add some noise to the base color
            KoreColorRGB noiseCol = KoreColorOps.ColorWithRGBNoise(color, noiseAmount);

            // Create new struct with updated color
            var updatedTri = tri with { Color = noiseCol };

            mesh.Triangles[triId] = updatedTri;
        }
    }
    // --------------------------------------------------------------------------------------------
    // MARK: Triangles
    // --------------------------------------------------------------------------------------------

    // Define four points of a face, in CW order, to be stored as two new triangles.
    // return a list of the new triangle IDs

    // A -- B
    // |    |
    // D -- C

    public static List<int> AddFace(KoreColorMesh mesh, int a, int b, int c, int d, KoreColorRGB color)
    {
        var triangleIds = new List<int>();

        // Split the quad into two triangles using a fan from vertex a
        // Triangle 1: a -> b -> c
        triangleIds.Add(mesh.AddTriangle(new KoreColorMeshTri(a, b, c, color)));

        // Triangle 2: a -> c -> d
        KoreColorRGB col2 = KoreColorOps.Lerp(color, KoreColorRGB.Black, 0.15f); // Slightly different color for second triangle
        triangleIds.Add(mesh.AddTriangle(new KoreColorMeshTri(a, c, d, col2)));

        return triangleIds;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Surfaces
    // --------------------------------------------------------------------------------------------

    // Usage:
    // - KoreColorMesh mesh = new KoreColorMesh();
    // - KoreXYZVector[,] surfaceArray = new KoreXYZVector[rows, cols];
    // - KoreColorMeshOps.AddSurface(mesh, surfaceArray, color);

    public static void AddSurface(KoreColorMesh mesh, KoreXYZVector[,] surfaceArray, KoreColorRGB color)
    {
        // Determine sizes
        int rows = surfaceArray.GetLength(0);
        int cols = surfaceArray.GetLength(1);

        // Add vertices to the mesh and store their IDs
        int[,] vertexIds = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                vertexIds[i, j] = mesh.AddVertex(surfaceArray[i, j]);
            }
        }

        // Create faces (quads) between the vertices
        for (int i = 0; i < rows - 1; i++)
        {
            for (int j = 0; j < cols - 1; j++)
            {
                int a = vertexIds[i, j];
                int b = vertexIds[i, j + 1];
                int c = vertexIds[i + 1, j + 1];
                int d = vertexIds[i + 1, j];

                // Add the face to the mesh
                AddFace(mesh, a, b, c, d, color);
            }
        }
    }

    public static void AddSurface(KoreColorMesh mesh, KoreXYZVector[,] surfaceArray, KoreColorRGB[,] colorArray)
    {
        // Determine sizes
        int rows = surfaceArray.GetLength(0);
        int cols = surfaceArray.GetLength(1);

        // Add vertices to the mesh and store their IDs
        int[,] vertexIds = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                vertexIds[i, j] = mesh.AddVertex(surfaceArray[i, j]);
            }
        }

        // Create faces (quads) between the vertices
        for (int i = 0; i < rows - 1; i++)
        {
            for (int j = 0; j < cols - 1; j++)
            {
                int a = vertexIds[i, j];
                int b = vertexIds[i, j + 1];
                int c = vertexIds[i + 1, j + 1];
                int d = vertexIds[i + 1, j];

                // Add the face to the mesh
                AddFace(mesh, a, b, c, d, colorArray[i, j]);
            }
        }
    }


}
