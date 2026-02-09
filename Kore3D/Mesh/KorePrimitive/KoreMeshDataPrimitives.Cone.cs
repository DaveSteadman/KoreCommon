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
    public static KoreMeshData Cone(
        KoreXYZVector     apexPoint,
        KoreXYZVector     baseCenterPoint,
        double           baseRadius,
        int              numRadialPoints,
        KoreColorRGB      color)      // Number of dots for dotted lines.
    {
        KoreMeshData mesh = new KoreMeshData();

        // Compute axis vector and its length
        KoreXYZVector axis = baseCenterPoint - apexPoint;
        double length = axis.Magnitude;
        if (length == 0) length = 1e-8; // Avoid division by zero
        KoreXYZVector axisDir = axis.Normalize();

        // Find two perpendicular vectors to axisDir for the base circle
        // perp1: Any unit vector perpendicular to axisDir, found using ArbitraryPerpendicular().
        KoreXYZVector perp1 = axisDir.ArbitraryPerpendicular().Normalize();

        // perp2: The cross product of axisDir and perp1, normalized.
        // perp2 is guaranteed to be perpendicular to both axisDir and perp1, and thus also lies in the base plane.
        // Together, perp1 and perp2 form an orthonormal basis for the plane perpendicular to the cone's axis.
        // By using perp1 * cos(angle) + perp2 * sin(angle), we can sweep out the entire circle in the base plane.
        KoreXYZVector perp2 = KoreXYZVector.CrossProduct(axisDir, perp1).Normalize();

        // Add apex and base center points
        int idxApex = mesh.AddCompleteVertex(apexPoint, null, color);
        int idxBaseCenter = mesh.AddCompleteVertex(baseCenterPoint, null, color);

        // Generate base points in the plane perpendicular to axisDir
        List<int> basePointIndices = new List<int>();
        for (int i = 0; i < numRadialPoints; i++)
        {
            double angle = 2 * Math.PI * i / numRadialPoints;
            KoreXYZVector offset = (perp1 * (baseRadius * Math.Cos(angle))) + (perp2 * (baseRadius * Math.Sin(angle)));
            KoreXYZVector basePoint = baseCenterPoint + offset;
            int idx = mesh.AddCompleteVertex(basePoint, null, color);
            basePointIndices.Add(idx);
        }

        // Create the cone's base by connecting each base point to the base center
        // and linking consecutive points.
        for (int i = 0; i < basePointIndices.Count; i++)
        {
            int currentIdx = basePointIndices[i];
            int nextIdx    = basePointIndices[(i + 1) % basePointIndices.Count];

            // Draw solid lines.
            mesh.AddLine(mesh.Vertices[idxBaseCenter], mesh.Vertices[currentIdx], color, color);
            mesh.AddLine(mesh.Vertices[currentIdx], mesh.Vertices[nextIdx], color, color);
        }

        // Create the lateral sides: connect the apex with every base point.
        foreach (int idx in basePointIndices)
        {
            mesh.AddLine(mesh.Vertices[idxApex], mesh.Vertices[idx], color, color);
        }

        return mesh;
    }
}
