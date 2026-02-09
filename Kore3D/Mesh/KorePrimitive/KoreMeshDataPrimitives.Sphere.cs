// <fileheader>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace KoreCommon;

// Static class to create KoreMeshData primitives
// This class is used to create various 3D shapes and meshes in Godot

public static partial class KoreMeshDataPrimitives
{

    // Usage: KoreMeshData mesh = KoreMeshDataPrimitives.BasicSphere(1.0f, new KoreColorRGB(1, 0, 0), 16);
    public static KoreMeshData BasicSphere(float radius, KoreColorRGB color, int numLatSegments)
    {
        int latSegments = numLatSegments;
        int lonSegments = numLatSegments * 2;

        var mesh = new KoreMeshData();

        var indexMap = new List<int>(); // Flat list of vertex indices

        for (int lat = 0; lat <= latSegments; lat++)
        {
            float a1 = (float)Math.PI * lat / latSegments;
            float sin1 = (float)Math.Sin(a1);
            float cos1 = (float)Math.Cos(a1);

            for (int lon = 0; lon <= lonSegments; lon++)
            {
                float a2 = 2f * (float)Math.PI * lon / lonSegments;
                float sin2 = (float)Math.Sin(a2);
                float cos2 = (float)Math.Cos(a2);

                float x = radius * sin1 * cos2;
                float y = radius * cos1;
                float z = radius * sin1 * sin2;

                KoreXYZVector vertex = new KoreXYZVector(x, y, z);
                KoreXYZVector normal = radius != 0 ? vertex / radius : new KoreXYZVector(0, 1, 0);

                int idx = mesh.AddCompleteVertex(vertex, normal, color);
                indexMap.Add(idx);
            }
        }

        // Triangles
        for (int lat = 0; lat < latSegments; lat++)
        {
            for (int lon = 0; lon < lonSegments; lon++)
            {
                int current = lat * (lonSegments + 1) + lon;
                int next = current + lonSegments + 1;

                mesh.AddTriangle(indexMap[current],     indexMap[next], indexMap[current + 1]);
                mesh.AddTriangle(indexMap[current + 1], indexMap[next], indexMap[next + 1]);
            }
        }

        // Horizontal wireframe lines (latitude circles)
        for (int lat = 0; lat <= latSegments; lat++)
        {
            int rowStart = lat * (lonSegments + 1);
            for (int lon = 0; lon < lonSegments; lon++)
            {
                int a = indexMap[rowStart + lon];
                int b = indexMap[rowStart + lon + 1];
                mesh.AddLine(a, b, color, color);
            }
        }

        // Vertical wireframe lines (longitude lines)
        for (int lon = 0; lon <= lonSegments; lon++)
        {
            for (int lat = 0; lat < latSegments; lat++)
            {
                int a = indexMap[lat       * (lonSegments + 1) + lon];
                int b = indexMap[(lat + 1) * (lonSegments + 1) + lon];
                mesh.AddLine(a, b, color, color);
            }
        }

        return mesh;
    }
    
    // --------------------------------------------------------------------------------------------
    
    // Creates a basic sphere mesh with a specified center, radius, and color
    public static KoreMeshData BasicSphere(KoreXYZVector center, double radius, KoreColorRGB? color = null, int numLatSegments = 16)
    {
        KoreMeshData sphereMesh = BasicSphere((float)radius, color ?? KoreColorPalette.Colors["White"], numLatSegments);

        KoreMeshDataEditOps.OffsetAllVertices(sphereMesh, center);

        return sphereMesh;
    }
    
}
