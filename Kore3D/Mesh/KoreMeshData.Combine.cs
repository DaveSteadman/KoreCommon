// <fileheader>

using System;
using System.Collections.Generic;

#nullable enable

namespace KoreCommon;

public partial class KoreMeshData
{
    // --------------------------------------------------------------------------------------------
    // MARK: Offset IDs
    // --------------------------------------------------------------------------------------------

    // Renumber all the IDs in the mesh down to minimum values

    public KoreMeshData RenumberIDs()
    {
        KoreMeshData newMesh = new KoreMeshData();

        // We can end up with a complex mesh with holes in the IDs, so we need to renumber them in a controlled manner, with a
        // dictionary to map the old IDs to the new IDs.
        Dictionary<int, int> vertexIdMap   = new Dictionary<int, int>();
        Dictionary<int, int> lineIdMap     = new Dictionary<int, int>();
        Dictionary<int, int> triangleIdMap = new Dictionary<int, int>();

        int newVertexId = 0;
        foreach (var kvp in Vertices)
            vertexIdMap[kvp.Key] = newVertexId++;

        int newLineId = 0;
        foreach (var kvp in Lines)
            lineIdMap[kvp.Key] = newLineId++;

        int newTriangleId = 0;
        foreach (var kvp in Triangles)
            triangleIdMap[kvp.Key] = newTriangleId++;

        // Now copy across the data indexed by each of the new vertices, lines, and triangles ID maps.

        // - - - - Vertices based lists - - - -

        foreach (var kvp in Vertices)
            newMesh.Vertices[vertexIdMap[kvp.Key]] = kvp.Value;

        foreach (var kvp in Normals)
            newMesh.Normals[vertexIdMap[kvp.Key]] = kvp.Value;

        foreach (var kvp in UVs)
            newMesh.UVs[vertexIdMap[kvp.Key]] = kvp.Value;

        foreach (var kvp in VertexColors)
            newMesh.VertexColors[vertexIdMap[kvp.Key]] = kvp.Value;

        // - - - - Copy lines - - - -

        foreach (var kvp in Lines)
        {
            var originalLine = kvp.Value;
            var newLine = new KoreMeshLine(vertexIdMap[originalLine.A], vertexIdMap[originalLine.B]);
            newMesh.Lines[lineIdMap[kvp.Key]] = newLine;
        }

        foreach (var kvp in LineColors)
            newMesh.LineColors[lineIdMap[kvp.Key]] = kvp.Value;

        // - - - - Copy triangles - - - -

        foreach (var kvp in Triangles)
        {
            var originalTriangle = kvp.Value;
            var newTriangle = new KoreMeshTriangle(vertexIdMap[originalTriangle.A], vertexIdMap[originalTriangle.B], vertexIdMap[originalTriangle.C]);
            newMesh.Triangles[triangleIdMap[kvp.Key]] = newTriangle;
        }

        // - - - - Copy materials - - - -

        // Materials don't have IDs, so copy them directly
        foreach (var material in Materials)
            newMesh.Materials.Add(material);

        // - - - - Copy named triangle groups with remapped triangle IDs - - - -

        foreach (var kvp in NamedTriangleGroups)
        {
            var originalGroup = kvp.Value;
            var remappedTriangleIds = new List<int>();

            // Remap each triangle ID in the group
            foreach (var originalTriangleId in originalGroup.TriangleIds)
            {
                if (triangleIdMap.ContainsKey(originalTriangleId))
                    remappedTriangleIds.Add(triangleIdMap[originalTriangleId]);
            }

            // Create new group with remapped triangle IDs
            var newGroup = new KoreMeshTriangleGroup(originalGroup.MaterialName, remappedTriangleIds);
            newMesh.NamedTriangleGroups[kvp.Key] = newGroup;
        }

        // Update the new mesh Next-ID values based on the new counts
        newMesh.ResetMaxIDs();

        return newMesh;
    }

    // --------------------------------------------------------------------------------------------

    // Offset the IDs of all the vertices, lines, triangles, and normals by a given offset, so we can
    // then merge two meshes together.

    public KoreMeshData OffsetIDs(KoreMeshData offsetFrom)
    {
        return OffsetIDs(
            offsetFrom.NextVertexId,
            offsetFrom.NextLineId,
            offsetFrom.NextTriangleId
        );
    }

    // --------------------------------------------------------------------------------------------

    // Offset the IDs of all the vertices, lines, triangles, and normals by a given offset, so we can
    // then merge two meshes together.

    public KoreMeshData OffsetIDs(int verticesOffset, int linesOffset, int trianglesOffset)
    {
        KoreMeshData newMesh = new KoreMeshData();

        // We can end up with a complex mesh with holes in the IDs, so we need to renumber them in a controlled manner, with a
        // dictionary to map the old IDs to the new IDs.
        Dictionary<int, int> vertexIdMap   = new Dictionary<int, int>();
        Dictionary<int, int> lineIdMap     = new Dictionary<int, int>();
        Dictionary<int, int> triangleIdMap = new Dictionary<int, int>();

        int newVertexId = verticesOffset;
        foreach (var kvp in Vertices)
            vertexIdMap[kvp.Key] = newVertexId++;

        int newLineId = linesOffset;
        foreach (var kvp in Lines)
            lineIdMap[kvp.Key] = newLineId++;

        int newTriangleId = trianglesOffset;
        foreach (var kvp in Triangles)
            triangleIdMap[kvp.Key] = newTriangleId++;


        // Now copy across the data indexed by each of the new vertices, lines, and triangles ID maps.

        // - - - - Vertices based lists - - - -

        foreach (var kvp in Vertices)
            newMesh.Vertices[vertexIdMap[kvp.Key]] = kvp.Value;

        foreach (var kvp in Normals)
            newMesh.Normals[vertexIdMap[kvp.Key]] = kvp.Value;

        foreach(var kvp in UVs)
            newMesh.UVs[vertexIdMap[kvp.Key]] = kvp.Value;

        foreach (var kvp in VertexColors)
            newMesh.VertexColors[vertexIdMap[kvp.Key]] = kvp.Value;

        // - - - - Copy lines - - - -

        foreach (var kvp in Lines)
        {
            var originalLine = kvp.Value;
            var newLine = new KoreMeshLine(vertexIdMap[originalLine.A], vertexIdMap[originalLine.B]);
            newMesh.Lines[lineIdMap[kvp.Key]] = newLine;
        }

        foreach (var kvp in LineColors)
            newMesh.LineColors[lineIdMap[kvp.Key]] = kvp.Value;

        // - - - - Copy triangles - - - -

        foreach (var kvp in Triangles)
        {
            var originalTriangle = kvp.Value;
            var newTriangle = new KoreMeshTriangle(vertexIdMap[originalTriangle.A], vertexIdMap[originalTriangle.B], vertexIdMap[originalTriangle.C]);
            newMesh.Triangles[triangleIdMap[kvp.Key]] = newTriangle;
        }

        // - - - - Copy materials - - - -

        // Materials don't have IDs, so copy them directly
        foreach (var material in Materials)
            newMesh.Materials.Add(material);

        // - - - - Copy named triangle groups with remapped triangle IDs - - - -

        foreach (var kvp in NamedTriangleGroups)
        {
            var originalGroup = kvp.Value;
            var remappedTriangleIds = new List<int>();

            // Remap each triangle ID in the group
            foreach (var originalTriangleId in originalGroup.TriangleIds)
            {
                if (triangleIdMap.ContainsKey(originalTriangleId))
                    remappedTriangleIds.Add(triangleIdMap[originalTriangleId]);
            }

            // Create new group with remapped triangle IDs
            var newGroup = new KoreMeshTriangleGroup(originalGroup.MaterialName, remappedTriangleIds);
            newMesh.NamedTriangleGroups[kvp.Key] = newGroup;
        }

        // Update the new mesh Next-ID values based on the new counts
        newMesh.ResetMaxIDs();

        return newMesh;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Basic Combine
    // --------------------------------------------------------------------------------------------

    // A Basic append takes the

    // Usage:  newMesh = KoreMeshData.BasicAppendMesh(mesh1, mesh2);
    public static KoreMeshData BasicAppendMesh(KoreMeshData mesh1, KoreMeshData mesh2)
    {
        // Renumber both meshes down to contiguous lists
        KoreMeshData newMesh         = mesh1.RenumberIDs();
        KoreMeshData mesh2Renumbered = mesh2.RenumberIDs();

        mesh2Renumbered.ResetMaxIDs();

        // Renumber 2 off of 1
        KoreMeshData mesh2Offset = mesh2Renumbered.OffsetIDs(newMesh);

        // Copy in the vertices, normals, UVs, and colors from mesh2, into mesh 1
        foreach (var kvp in mesh2Offset.Vertices)
            newMesh.Vertices[kvp.Key] = kvp.Value;

        foreach (var kvp in mesh2Offset.Normals)
            newMesh.Normals[kvp.Key] = kvp.Value;

        foreach (var kvp in mesh2Offset.UVs)
            newMesh.UVs[kvp.Key] = kvp.Value;

        foreach (var kvp in mesh2Offset.VertexColors)
            newMesh.VertexColors[kvp.Key] = kvp.Value;

        // Copy in the lines from mesh2, into mesh 1
        foreach (var kvp in mesh2Offset.Lines)
            newMesh.Lines[kvp.Key] = kvp.Value;

        foreach (var kvp in mesh2Offset.LineColors)
            newMesh.LineColors[kvp.Key] = kvp.Value;

        // Copy in the triangles from mesh2, into mesh 1
        foreach (var kvp in mesh2Offset.Triangles)
            newMesh.Triangles[kvp.Key] = kvp.Value;

        // Copy in the materials from mesh2, into mesh 1
        foreach (var material in mesh2Offset.Materials)
            newMesh.Materials.Add(material);

        // Copy in the named triangle groups from mesh2, into mesh 1
        foreach (var kvp in mesh2Offset.NamedTriangleGroups)
            newMesh.NamedTriangleGroups[kvp.Key] = kvp.Value;

        // Update the new mesh Next-ID values based on the new counts
        newMesh.ResetMaxIDs();

        return newMesh;
    }
}
