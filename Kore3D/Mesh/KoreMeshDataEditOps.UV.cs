// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

// Static validity and cleanup operations for KoreMeshData
// Contains methods for mesh validation, cleanup, and population of missing data
public static partial class KoreMeshDataEditOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: UVs
    // --------------------------------------------------------------------------------------------

    // Remove UVs that don't have supporting vertex IDs
    public static void RemoveBrokenUVs(KoreMeshData mesh)
    {
        var invalidUVIds = mesh.UVs.Keys.Where(id => !mesh.Vertices.ContainsKey(id)).ToList();
        foreach (int uvId in invalidUVIds)
        {
            mesh.UVs.Remove(uvId);
        }
    }

    // Create missing UVs for vertices
    public static void CreateMissingUVs(KoreMeshData mesh, KoreXYVector? defaultUV = null)
    {
        KoreXYVector uv = defaultUV ?? new KoreXYVector(0, 0);

        foreach (int vertexId in mesh.Vertices.Keys)
        {
            if (!mesh.UVs.ContainsKey(vertexId))
            {
                mesh.UVs[vertexId] = uv;
            }
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Flip
    // --------------------------------------------------------------------------------------------

    public static void FlipAllUVsVertical(KoreMeshData mesh)
    {
        // Loop through all the UVs and flip their V coordinate
        foreach (var kvp in mesh.UVs)
        {
            int uvId = kvp.Key;
            KoreXYVector uv = kvp.Value;

            // Flip the V coordinate (Y axis in KoreXYVector)
            mesh.UVs[uvId] = new KoreXYVector(uv.X, 1.0 - uv.Y);
        }
    }

    public static void FlipAllUVsHorizontal(KoreMeshData mesh)
    {
        // Loop through all the UVs and flip their U coordinate
        foreach (var kvp in mesh.UVs)
        {
            int uvId = kvp.Key;
            KoreXYVector uv = kvp.Value;

            // Flip the U coordinate (X axis in KoreXYVector)
            mesh.UVs[uvId] = new KoreXYVector(1.0 - uv.X, uv.Y);
        }
    }


}
