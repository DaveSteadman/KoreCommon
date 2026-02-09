// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;


// KoreMeshDataEditOps: A static class to hold functions to edit a mesh

public static partial class KoreMeshDataEditOps
{
    // Remove lines that don't have supporting vertex IDs

    public static void RemoveBrokenLines(KoreMeshData mesh)
    {
        var invalidLineIds = mesh.Lines.Where(kvp =>
            !mesh.Vertices.ContainsKey(kvp.Value.A) ||
            !mesh.Vertices.ContainsKey(kvp.Value.B))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (int lineId in invalidLineIds)
        {
            mesh.Lines.Remove(lineId);
            mesh.LineColors.Remove(lineId);
        }
    }

    // --------------------------------------------------------------------------------------------

    // Remove duplicate lines

    public static void RemoveDuplicateLines(KoreMeshData mesh)
    {
        var linesToRemove = new List<int>();
        var linesArray = mesh.Lines.ToArray();

        for (int i = 0; i < linesArray.Length; i++)
        {
            for (int j = i + 1; j < linesArray.Length; j++)
            {
                var line1 = linesArray[i].Value;
                var line2 = linesArray[j].Value;

                // Check if lines are the same (either direction)
                if ((line1.A == line2.A && line1.B == line2.B) ||
                    (line1.A == line2.B && line1.B == line2.A))
                {
                    linesToRemove.Add(linesArray[j].Key);
                }
            }
        }

        foreach (int lineId in linesToRemove)
        {
            mesh.Lines.Remove(lineId);
            mesh.LineColors.Remove(lineId);
        }
    }

    // --------------------------------------------------------------------------------------------

    // Order Line Vertices Ascending, so A < B is guaranteed

    public static void OrderLineVertexAscending(KoreMeshData mesh)
    {
        foreach (var kvp in mesh.Lines)
        {
            int lineId = kvp.Key;
            KoreMeshLine line = kvp.Value;

            if (line.A > line.B)
            {
                mesh.Lines[lineId] = new KoreMeshLine(line.B, line.A);
            }
        }
    }

}
