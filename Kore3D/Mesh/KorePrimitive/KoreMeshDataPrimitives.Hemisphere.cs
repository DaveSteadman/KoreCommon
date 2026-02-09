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
    public static KoreMeshData BasicHemisphere(float radius, KoreColorRGB color, int numLatSegments)
    {
        int latSegments = numLatSegments;
        int lonSegments = numLatSegments * 2;

        var mesh = new KoreMeshData();

        // List of lists to store vertex IDs for each latitude ring
        var latitudeRings = new List<List<int>>();

        // Create the top point (singular point at the hemisphere top)
        var topVertex = new KoreXYZVector(0, radius, 0);
        int topVertexId = mesh.AddCompleteVertex(topVertex, null, color);

        // Add the top point as the first "ring" (single point)
        latitudeRings.Add(new List<int> { topVertexId });

        // Generate latitude rings from top to bottom (excluding the very top which is the singular point)
        for (int lat = 1; lat <= latSegments; lat++)
        {
            double a1 = (Math.PI / 2.0) * lat / latSegments; // 0 to PI/2
            double sin1 = Math.Sin(a1);
            double cos1 = Math.Cos(a1);

            var currentRing = new List<int>();

            for (int lon = 0; lon < lonSegments; lon++)
            {
                double a2 = 2 * Math.PI * lon / lonSegments;
                double sin2 = Math.Sin(a2);
                double cos2 = Math.Cos(a2);

                double x = radius * sin1 * cos2;
                double y = radius * cos1;
                double z = radius * sin1 * sin2;

                var vertex = new KoreXYZVector(x, y, z);
                int vertexId = mesh.AddCompleteVertex(vertex, null, color);
                currentRing.Add(vertexId);
            }

            latitudeRings.Add(currentRing);
        }

        // Generate triangles between latitude rings
        for (int lat = 0; lat < latitudeRings.Count - 1; lat++)
        {
            var upperRing = latitudeRings[lat];
            var lowerRing = latitudeRings[lat + 1];

            if (lat == 0)
            {
                // Special case: connect top point to first ring (triangular fans - CW)
                for (int lon = 0; lon < lowerRing.Count; lon++)
                {
                    int nextLon = (lon + 1) % lowerRing.Count;
                    mesh.AddTriangle(topVertexId, lowerRing[nextLon], lowerRing[lon]);
                }
            }
            else
            {
                // Normal case: connect two rings with quads (2 triangles each - CW)
                for (int lon = 0; lon < upperRing.Count; lon++)
                {
                    int nextLon = (lon + 1) % upperRing.Count;

                    // Two triangles forming a quad (CW winding)
                    mesh.AddTriangle(upperRing[lon], upperRing[nextLon], lowerRing[nextLon]);
                    mesh.AddTriangle(upperRing[lon], lowerRing[nextLon], lowerRing[lon]);
                }
            }
        }

        // Generate wireframe lines
        // Horizontal lines (latitude circles)
        for (int lat = 1; lat < latitudeRings.Count; lat++) // Skip top point
        {
            var ring = latitudeRings[lat];
            for (int lon = 0; lon < ring.Count; lon++)
            {
                int nextLon = (lon + 1) % ring.Count;
                mesh.AddLine(ring[lon], ring[nextLon], color, color);
            }
        }

        // Vertical lines (longitude lines)
        for (int lon = 0; lon < lonSegments; lon++)
        {
            // Connect from top point down through all rings
            mesh.AddLine(topVertexId, latitudeRings[1][lon], color, color);

            for (int lat = 1; lat < latitudeRings.Count - 1; lat++)
            {
                mesh.AddLine(latitudeRings[lat][lon], latitudeRings[lat + 1][lon], color, color);
            }
        }

        // calculate the normals from the triangles.
        KoreMeshDataEditOps.SetNormalsFromTriangles(mesh);

        return mesh;
    }

}
