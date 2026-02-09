// <fileheader>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace KoreCommon;

// Static class to create KoreMeshData primitives
// This class is used to create various 3D shapes and meshes in Godot

public static partial class KoreMiniMeshPrimitives
{
    // Create a sphere mesh for KoreMiniMesh
    // Usage: KoreMiniMesh sphereMesh = KoreMiniMeshPrimitives.BasicSphere(center, radius, latSegments, material, lineColor);

    public static KoreMiniMesh BasicSphere(
        KoreXYZVector center,
        double radius,
        int latSegments,
        KoreMiniMeshMaterial material,
        KoreColorRGB lineCol)
    {
        if (latSegments < 3) throw new ArgumentException("Sphere must have at least 3 latitude segments");

        var mesh = new KoreMiniMesh();

        // Add material and line color
        mesh.AddMaterial(material);
        int lineColorId = mesh.AddColor(lineCol);
        string matName = material.Name;

        int lonSegments = latSegments * 2; // longitude segments (horizontal divisions)

        var allTriangles = new List<int>();

        // Create single vertices for the poles
        KoreXYZVector topPole = center + new KoreXYZVector(0, (float)radius, 0);
        KoreXYZVector bottomPole = center + new KoreXYZVector(0, -(float)radius, 0);
        int topPoleId = mesh.AddVertex(topPole);
        int bottomPoleId = mesh.AddVertex(bottomPole);

        // Create vertex grid for intermediate latitudes only (excluding poles)
        var vertexGrid = new List<List<int>>();

        for (int lat = 1; lat < latSegments; lat++) // Skip poles (lat=0 and lat=latSegments)
        {
            var latRow = new List<int>();

            float a1 = (float)Math.PI * lat / latSegments; // latitude angle (0 to π)
            float sin1 = (float)Math.Sin(a1);
            float cos1 = (float)Math.Cos(a1);

            for (int lon = 0; lon <= lonSegments; lon++)
            {
                float a2 = 2f * (float)Math.PI * lon / lonSegments; // longitude angle (0 to 2π)
                float sin2 = (float)Math.Sin(a2);
                float cos2 = (float)Math.Cos(a2);

                // Spherical to Cartesian conversion
                float x = (float)(radius * sin1 * cos2);
                float y = (float)(radius * cos1);
                float z = (float)(radius * sin1 * sin2);

                KoreXYZVector vertex = center + new KoreXYZVector(x, y, z);
                int vertexId = mesh.AddVertex(vertex);
                latRow.Add(vertexId);
            }

            vertexGrid.Add(latRow);
        }

        // Create triangles connecting to top pole
        if (vertexGrid.Count > 0)
        {
            var firstRow = vertexGrid[0];
            for (int lon = 0; lon < lonSegments; lon++)
            {
                int v1 = topPoleId;
                int v2 = firstRow[lon];
                int v3 = firstRow[lon + 1];
                allTriangles.Add(mesh.AddTriangle(new KoreMiniMeshTri(v1, v2, v3)));
            }
        }

        // Create triangles between intermediate latitude rows
        for (int lat = 0; lat < vertexGrid.Count - 1; lat++)
        {
            for (int lon = 0; lon < lonSegments; lon++)
            {
                // Get the four vertices of the current quad to match original sphere winding
                int v1 = vertexGrid[lat][lon];         // current lat, current lon
                int v2 = vertexGrid[lat + 1][lon];     // next lat, current lon
                int v3 = vertexGrid[lat + 1][lon + 1]; // next lat, next lon
                int v4 = vertexGrid[lat][lon + 1];     // current lat, next lon

                // Add the quad as two triangles using AddFace helper
                allTriangles.AddRange(KoreMiniMeshOps.AddFace(mesh, v1, v2, v3, v4));
            }
        }

        // Create triangles connecting to bottom pole
        if (vertexGrid.Count > 0)
        {
            var lastRow = vertexGrid[vertexGrid.Count - 1];
            for (int lon = 0; lon < lonSegments; lon++)
            {
                int v1 = bottomPoleId;
                int v2 = lastRow[lon + 1];
                int v3 = lastRow[lon];
                allTriangles.Add(mesh.AddTriangle(new KoreMiniMeshTri(v1, v2, v3)));
            }
        }

        // Create wireframe lines
        // Horizontal lines (latitude circles) for intermediate latitudes
        foreach (var latRow in vertexGrid)
        {
            for (int lon = 0; lon < lonSegments; lon++)
            {
                int v1 = latRow[lon];
                int v2 = latRow[lon + 1];
                mesh.AddLine(new KoreMiniMeshLine(v1, v2, lineColorId));
            }
        }

        // Vertical lines (longitude lines)
        for (int lon = 0; lon <= lonSegments; lon++)
        {
            // Connect from top pole to first latitude ring
            if (vertexGrid.Count > 0)
            {
                mesh.AddLine(new KoreMiniMeshLine(topPoleId, vertexGrid[0][lon], lineColorId));
            }

            // Connect between latitude rings
            for (int lat = 0; lat < vertexGrid.Count - 1; lat++)
            {
                int v1 = vertexGrid[lat][lon];
                int v2 = vertexGrid[lat + 1][lon];
                mesh.AddLine(new KoreMiniMeshLine(v1, v2, lineColorId));
            }

            // Connect from last latitude ring to bottom pole
            if (vertexGrid.Count > 0)
            {
                mesh.AddLine(new KoreMiniMeshLine(vertexGrid[vertexGrid.Count - 1][lon], bottomPoleId, lineColorId));
            }
        }

        // Create groups
        mesh.AddGroup("All", new KoreMiniMeshGroup(matName, allTriangles));

        return mesh;
    }








}
