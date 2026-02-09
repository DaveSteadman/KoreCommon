// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

public static partial class KoreMeshDataEditOps
{
    // Remove line colors that don't have supporting line IDs
    public static void RemoveBrokenLineColors(KoreMeshData mesh)
    {
        var invalidLineColorIds = mesh.LineColors.Keys.Where(id => !mesh.Lines.ContainsKey(id)).ToList();
        foreach (int lineColorId in invalidLineColorIds)
        {
            mesh.LineColors.Remove(lineColorId);
        }
    }

    // --------------------------------------------------------------------------------------------

    // Create missing line colors
    public static void CreateMissingLineColors(KoreMeshData mesh, KoreColorRGB? defaultColor = null)
    {
        KoreColorRGB color = defaultColor ?? KoreColorRGB.White;

        foreach (int lineId in mesh.Lines.Keys)
        {
            if (!mesh.LineColors.ContainsKey(lineId))
            {
                mesh.LineColors[lineId] = new KoreMeshLineColour(color, color);
            }
        }
    }

}
