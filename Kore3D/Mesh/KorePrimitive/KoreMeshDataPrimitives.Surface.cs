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
    // COORDINATE SYSTEM:
    // - vertices[0,        0] represents the TOP-LEFT corner of the surface
    // - vertices[0,        width-1] represents the TOP-RIGHT corner
    // - vertices[height-1, 0] represents the BOTTOM-LEFT corner
    // - vertices[height-1, width-1] represents the BOTTOM-RIGHT corner
    //
    // The first array index (i) corresponds to X-axis (left-to-right)
    // The second array index (j) corresponds to Z-axis (top-to-bottom when viewed from above)
    //
    // TRIANGLE WINDING:
    // - Triangles are wound clockwise when viewed from positive Y (looking down at XZ plane)
    // - Surface normals point upward in positive Y direction for flat surfaces
    //
    // USAGE:
    // var vertices = new KoreXYZVector[height, width];
    // Populate vertices where [0,0] is top-left corner
    // var surfaceMesh = KoreMeshDataPrimitives.Surface(vertices, KoreUVBox.Full);

    // vertices: 2D array of vertices where [0,0] is top-left corner
    // uvBox: UV mapping coordinates for the surface
    // returns: KoreMeshData representing the surface with proper CW triangle winding

    public static KoreMeshData Surface(KoreXYZVector[,] vertices, KoreUVBox uvBox)
    {
        var mesh = new KoreMeshData();

        // Basic setup, dimensions and UVs
        int height = vertices.GetLength(0);
        int width  = vertices.GetLength(1);
        KoreXYVector[,] uvGrid = uvBox.GetUVGrid(height, width);

        KoreCentralLog.AddEntry($"Creating surface mesh with dimensions {width}x{height} and UV box {uvBox}");
        KoreCentralLog.AddEntry($"UV00 = {uvGrid[0, 0]:F3}");

        // Loop through the grid, adding points and UVs. Create a corresponding output grid of the point IDs
        int[,] pointIds = new int[height, width];

        for (int iX = 0; iX < width; iX++)
        {
            for (int iY = 0; iY < height; iY++)
            {
                // Add vertex with position and UV
                pointIds[iY, iX] = mesh.AddCompleteVertex(
                    vertices[iY, iX],
                    null, // normal
                    null, // color
                    uvGrid[iX, iY]);
            }
        }

        // Loop through the point IDs to create triangles and lines
        for (int iY = 0; iY < height - 1; iY++)
        {
            for (int iX = 0; iX < width - 1; iX++)
            {
                // Create two triangles for each quad (CW winding)
                int p00 = pointIds[iY, iX];           // Top-left
                int p01 = pointIds[iY + 1, iX];       // Bottom-left
                int p10 = pointIds[iY, iX + 1];       // Top-right
                int p11 = pointIds[iY + 1, iX + 1];   // Bottom-right

                mesh.AddTriangle(p00, p11, p01);
                mesh.AddTriangle(p00, p10, p11);

                // Always add top and left edges
                mesh.AddLine(p00, p10);  // Left edge
                mesh.AddLine(p01, p00);  // Top edge

                // Add right edge only for last column
                if (iX == width - 2) mesh.AddLine(p10, p11);  // Right edge
                // Add bottom edge only for last row
                if (iY == height - 2) mesh.AddLine(p11, p01);  // Bottom edge
            }
        }

        // Create the normals based on the triangles
        KoreMeshDataEditOps.SetNormalsFromTriangles(mesh);

        return mesh;
    }

}
