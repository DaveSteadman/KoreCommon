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
    // MARK: Vertex Colors
    // --------------------------------------------------------------------------------------------

    // Create missing vertex colors
    public static void CreateMissingVertexColors(KoreMeshData mesh, KoreColorRGB? defaultColor = null)
    {
        KoreColorRGB color = defaultColor ?? KoreColorRGB.White;

        foreach (int vertexId in mesh.Vertices.Keys)
        {
            if (!mesh.VertexColors.ContainsKey(vertexId))
            {
                mesh.VertexColors[vertexId] = color;
            }
        }
    }


}
