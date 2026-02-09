// <fileheader>

using System;
using System.Collections.Generic;

#nullable enable

namespace KoreCommon;

// KoreMeshDataEditOps: A static class to hold functions to edit a mesh

public static partial class KoreMeshDataEditOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Bounding Box
    // --------------------------------------------------------------------------------------------

    // Loop through the vertices, recording the max/min X, Y, Z values. Then return a KoreXYZBox

    public static KoreXYZBox GetBoundingBox(KoreMeshData meshData)
    {
        if (meshData.Vertices.Count == 0)
            return KoreXYZBox.Zero;

        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;
        double minZ = double.MaxValue, maxZ = double.MinValue;

        foreach (var kvp in meshData.Vertices)
        {
            KoreXYZVector vertex = kvp.Value;
            if (vertex.X < minX) minX = vertex.X;
            if (vertex.X > maxX) maxX = vertex.X;
            if (vertex.Y < minY) minY = vertex.Y;
            if (vertex.Y > maxY) maxY = vertex.Y;
            if (vertex.Z < minZ) minZ = vertex.Z;
            if (vertex.Z > maxZ) maxZ = vertex.Z;
        }

        KoreXYZVector center = new KoreXYZVector((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
        double width = maxX - minX;
        double height = maxY - minY;
        double length = maxZ - minZ;

        return new KoreXYZBox(center, width, height, length);
    }
}
