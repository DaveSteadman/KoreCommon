// <fileheader>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace KoreCommon;

// MagSphere - A sphere where the radius of each point is defined by an input array of numbers.
// The radius is the distance from the center of the sphere to the point on the surface of the sphere.
// The radius is defined in spherical coordinates, with the azimuth and elevation angles defining the position of the point on the sphere.
// The color of a point is the magnitude of the radius, defined by an input color range.

public static partial class KoreMeshDataPrimitives
{
    public static KoreMeshData MagSphere(KoreXYZVector center, KoreFloat2DArray radiusList, KoreColorRange colorRange)
    {
        var mesh = new KoreMeshData();

        int vertSegments = radiusList.Height - 1;
        int horizSegments = radiusList.Width - 1;

        double vertAngInc = 180.0 / vertSegments;
        double horizAngInc = 360.0 / horizSegments;

        double maxRadius = radiusList.MaxVal();
        double minRadius = radiusList.MinVal();

        var vertexIndices = new List<int>();

        // Generate all vertices
        for (int i = 0; i <= vertSegments; i++)
        {
            for (int j = 0; j < horizSegments; j++)
            {
                float radius = radiusList[j, i];

                double azRads = KoreAngle.DegsToRads(90f - (vertAngInc * i));
                double elRads = KoreAngle.DegsToRads(horizAngInc * j);

                double y = radius * Math.Sin(azRads);
                double r = radius * Math.Cos(azRads);
                double x = r * Math.Cos(elRads);
                double z = r * Math.Sin(elRads);

                var position = new KoreXYZVector(x, y, z);
                var worldPosition = position + center;
                var normal = position.Normalize();

                double uvX = (double)j / horizSegments;
                double uvY = (double)i / vertSegments;

                double radiusFraction = (radius - minRadius) / (maxRadius - minRadius);
                var color = colorRange.GetColor((float)radiusFraction);

                int idx = mesh.AddCompleteVertex(worldPosition, normal, color);
                mesh.SetUV(idx, new KoreXYVector(uvX, uvY));

                vertexIndices.Add(idx);
            }
        }

        // Create triangles + wireframe lines
        for (int row = 0; row < vertSegments; row++)
        {
            int rowStart = row * horizSegments;
            int nextRowStart = (row + 1) * horizSegments;

            for (int i = 0; i < horizSegments; i++)
            {
                int i00 = vertexIndices[rowStart + i];
                int i10 = vertexIndices[rowStart + (i + 1) % horizSegments];
                int i01 = vertexIndices[nextRowStart + i];
                int i11 = vertexIndices[nextRowStart + (i + 1) % horizSegments];

                // CW winding
                mesh.AddTriangle(i00, i11, i10);
                mesh.AddTriangle(i00, i01, i11);

                var c1 = mesh.VertexColors[i00];
                var c2 = mesh.VertexColors[i10];
                var c3 = mesh.VertexColors[i01];
                var c4 = mesh.VertexColors[i11];

                mesh.AddLine(i00, i10, c1, c2);
                mesh.AddLine(i00, i01, c1, c3);

                // Optionally:
                // mesh.AddLine(i1, i4, c1, c4);
                // mesh.AddLine(i2, i3, c2, c3);
                // mesh.AddLine(i2, i4, c2, c4);
                // mesh.AddLine(i3, i4, c3, c4);
            }
        }

        return mesh;
    }
}
