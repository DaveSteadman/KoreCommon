// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

public static partial class KoreMiniMeshOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Ranges
    // --------------------------------------------------------------------------------------------

    /* Example:

        // Get the ranges, the co-ordinate bounds of the mesh
        (KoreNumericRange<double> xRange, KoreNumericRange<double> yRange, KoreNumericRange<double> zRange) = GetRanges(mesh);
        
        // translate the y-axis, so the min is zero
        yRange.Max -= yRange.Min;
        yRange.Min = 0;

        // apply the new ranges to the mesh
        SetRanges(mesh, (xRange, yRange, zRange), (newXRange, newYRange, newZRange));

    */

    // Functions to get and set KoreNumericRange objects for each X, Y, Z axis on a mesh. 
    // Allows us to translate and scale meshes.

    public static (KoreNumericRange<double>, KoreNumericRange<double>, KoreNumericRange<double>) GetRanges(KoreMiniMesh mesh)
    {
        if (mesh.Vertices.Count == 0)
            return (new KoreNumericRange<double>(0, 0), new KoreNumericRange<double>(0, 0), new KoreNumericRange<double>(0, 0));

        // Define min/max values
        var firstVertex = mesh.Vertices.Values.First();
        double minX = firstVertex.X, maxX = firstVertex.X;
        double minY = firstVertex.Y, maxY = firstVertex.Y;
        double minZ = firstVertex.Z, maxZ = firstVertex.Z;

        // Loop through each of the points to expand the ranges.
        foreach (var vertex in mesh.Vertices.Values)
        {
            if (vertex.X < minX) minX = vertex.X;
            if (vertex.X > maxX) maxX = vertex.X;
            if (vertex.Y < minY) minY = vertex.Y;
            if (vertex.Y > maxY) maxY = vertex.Y;
            if (vertex.Z < minZ) minZ = vertex.Z;
            if (vertex.Z > maxZ) maxZ = vertex.Z;
        }

        return (new KoreNumericRange<double>(minX, maxX),
                new KoreNumericRange<double>(minY, maxY),
                new KoreNumericRange<double>(minZ, maxZ));
    }

    // --------------------------------------------------------------------------------------------

    public static void SetRanges(
        KoreMiniMesh mesh,
        (KoreNumericRange<double> xRange, KoreNumericRange<double> yRange, KoreNumericRange<double> zRange) oldranges,
        (KoreNumericRange<double> xRange, KoreNumericRange<double> yRange, KoreNumericRange<double> zRange) newranges)
    {
        // Loop through each point and assign the new ranges
        foreach (var kvp in mesh.Vertices.ToList()) // ToList to avoid modifying collection during enumeration
        {
            int id = kvp.Key;
            var v = kvp.Value;

            // Apply the new and old ranges, performing essentially a y=mx+c between the two.
            double newX = KoreNumericUtils.ScaleToRange(v.X, oldranges.xRange.Min, oldranges.xRange.Max, newranges.xRange.Min, newranges.xRange.Max);
            double newY = KoreNumericUtils.ScaleToRange(v.Y, oldranges.yRange.Min, oldranges.yRange.Max, newranges.yRange.Min, newranges.yRange.Max);
            double newZ = KoreNumericUtils.ScaleToRange(v.Z, oldranges.zRange.Min, oldranges.zRange.Max, newranges.zRange.Min, newranges.zRange.Max);

            mesh.Vertices[id] = new KoreXYZVector(newX, newY, newZ);
        }
    }

    // --------------------------------------------------------------------------------------------

    // Setup a model in a standard position, centered on the XZ origin 

    // Usage: KoreMiniMesh newRootedMesh = KoreMiniMeshOps.RootMesh(mesh);
    public static void RootMesh(KoreMiniMesh mesh)
    {
        // Get the ranges, the co-ordinate bounds of the mesh
        (KoreNumericRange<double> xRange, KoreNumericRange<double> yRange, KoreNumericRange<double> zRange) = GetRanges(mesh);

        // Translate the y-axis, so the min is zero
        yRange.Offset(-yRange.Min);

        // Center the x and z axes on zero
        xRange = xRange.CenterOnValue(0);
        zRange = zRange.CenterOnValue(0);

        // Apply the new ranges to the mesh
        SetRanges(mesh, (xRange, yRange, zRange), (xRange, yRange, zRange));
    }

}



