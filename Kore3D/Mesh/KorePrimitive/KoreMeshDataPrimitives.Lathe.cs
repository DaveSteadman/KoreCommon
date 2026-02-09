// <fileheader>

using System;
using System.Collections.Generic;

namespace KoreCommon;

public struct LathePoint
{
    public double Fraction { get; set; }  // 0.0 to 1.0 along P1→P2 axis
    public double Radius { get; set; }    // Distance from axis

    public LathePoint(double fraction, double radius)
    {
        Fraction = fraction;
        Radius = radius;
    }
}

public static partial class KoreMeshDataPrimitives
{
    public static KoreMeshData Lathe(KoreXYZVector p1, KoreXYZVector p2, List<LathePoint> profile, int numSegments, bool capEnds)
    {
        var mesh = new KoreMeshData();

        if (profile.Count < 2 || numSegments < 3) return mesh;

        // Calculate axis and basis vectors (similar to Cylinder)
        KoreXYZVector axis = p2 - p1;
        double axisLength = axis.Magnitude;
        if (axisLength < 1e-6) return mesh;

        axis = axis.Normalize();

        // Create perpendicular basis vectors
        KoreXYZVector up = Math.Abs(KoreXYZVector.DotProduct(axis, KoreXYZVector.Up)) < 0.99
            ? KoreXYZVector.Up : KoreXYZVector.Right;
        KoreXYZVector side = KoreXYZVector.CrossProduct(axis, up).Normalize();
        KoreXYZVector forward = KoreXYZVector.CrossProduct(axis, side).Normalize();

        // Generate all vertices in a 2D grid: [profileIndex][segmentIndex]
        var vertexGrid = new int[profile.Count, numSegments];

        for (int i = 0; i < profile.Count; i++)
        {
            var lathePoint = profile[i];
            KoreXYZVector axisPosition = p1 + axis * (lathePoint.Fraction * axisLength);

            for (int j = 0; j < numSegments; j++)
            {
                double angle = (2.0 * Math.PI * j) / numSegments;
                KoreXYZVector radialOffset = (Math.Cos(angle) * side + Math.Sin(angle) * forward) * lathePoint.Radius;
                KoreXYZVector worldPos = axisPosition + radialOffset;

                // UV coordinates
                double u = (double)j / numSegments;
                double v = lathePoint.Fraction;

                vertexGrid[i, j] = mesh.AddCompleteVertex(worldPos, null, null, new KoreXYVector(u, v));
            }
        }

        // Create ribbons for each band between adjacent profile points
        // This gives us accurate normals without cross-contamination between bands
        for (int i = 0; i < profile.Count - 1; i++)
        {
            // Create the left and right circles for this band
            var leftCircle = new List<KoreXYZVector>();
            var rightCircle = new List<KoreXYZVector>();
            var leftUVs = new List<KoreXYVector>();
            var rightUVs = new List<KoreXYVector>();

            var currentProfile = profile[i];
            var nextProfile = profile[i + 1];

            for (int j = 0; j < numSegments; j++)
            {
                // Current profile points (left side of ribbon)
                leftCircle.Add(mesh.Vertices[vertexGrid[i, j]]);
                leftUVs.Add(new KoreXYVector((double)j / numSegments, currentProfile.Fraction));

                // Next profile points (right side of ribbon)
                rightCircle.Add(mesh.Vertices[vertexGrid[i + 1, j]]);
                rightUVs.Add(new KoreXYVector((double)j / numSegments, nextProfile.Fraction));
            }

            // Create ribbon for this band
            KoreMeshData bandMesh = Ribbon(leftCircle, leftUVs, rightCircle, rightUVs, true);

            // Add wireframe lines for this band
            var wireframeColor = new KoreColorRGB(255, 255, 255);
            foreach (var lineKvp in bandMesh.Lines)
            {
                var line = lineKvp.Value;
                var startPos = bandMesh.Vertices[line.A];
                var endPos = bandMesh.Vertices[line.B];
                mesh.AddLine(startPos, endPos, wireframeColor);
            }

            // Append the band mesh to the main mesh
            mesh = KoreMeshData.BasicAppendMesh(mesh, bandMesh);
        }

        // Add end caps if first or last radius > 0
        if (capEnds && profile.Count > 0)
        {
            // Bottom cap (if first radius > 0)
            if (profile[0].Radius > 1e-6)
            {
                var bottomCircle = new List<KoreXYZVector>();
                for (int j = 0; j < numSegments; j++)
                {
                    bottomCircle.Add(mesh.Vertices[vertexGrid[0, j]]);
                }
                // Keep original order for bottom cap to face outward
                KoreMeshData bottomCap = Fan(p1 + axis * (profile[0].Fraction * axisLength), bottomCircle, true);
                mesh = KoreMeshData.BasicAppendMesh(mesh, bottomCap);
            }

            // Top cap (if last radius > 0)
            int lastIndex = profile.Count - 1;
            if (profile[lastIndex].Radius > 1e-6)
            {
                var topCircle = new List<KoreXYZVector>();
                for (int j = 0; j < numSegments; j++)
                {
                    topCircle.Add(mesh.Vertices[vertexGrid[lastIndex, j]]);
                }
                topCircle.Reverse(); // Reverse for top cap to face outward
                KoreMeshData topCap = Fan(p1 + axis * (profile[lastIndex].Fraction * axisLength), topCircle, true);
                mesh = KoreMeshData.BasicAppendMesh(mesh, topCap);
            }
        }

        // Note: Normals are already calculated by each individual ribbon
        // No need for SetNormalsFromTriangles() as each band has its own accurate normals

        return mesh;
    }
}
