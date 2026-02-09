// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

// KoreMeshDataEditOps: A static class to hold functions to edit a mesh
public static partial class KoreMeshDataEditOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Vertex
    // --------------------------------------------------------------------------------------------

    public static Dictionary<int, KoreXYZVector> VerticesForGroup(KoreMeshData meshData, string groupName)
    {
        var vertexDict = new Dictionary<int, KoreXYZVector>();

        if (!meshData.HasNamedGroup(groupName))
            return vertexDict;

        KoreMeshTriangleGroup? group = meshData.NamedGroup(groupName);
        if (group == null)
            return vertexDict;

        KoreMeshTriangleGroup groupValue = group.Value;
        foreach (int triangleId in groupValue.TriangleIds)
        {
            var triangle = meshData.Triangles[triangleId];
            int[] vertexIds = { triangle.A, triangle.B, triangle.C };

            foreach (var vId in vertexIds)
            {
                if (meshData.Vertices.ContainsKey(vId) && !vertexDict.ContainsKey(vId))
                    vertexDict[vId] = meshData.Vertices[vId];
            }
        }

        return vertexDict;
    }

    // --------------------------------------------------------------------------------------------

    // Helper method to get all vertex IDs for a group (reduces code duplication)
    private static HashSet<int> GetVertexIdsForGroup(KoreMeshData meshData, string groupName)
    {
        HashSet<int> relevantVertexIds = new HashSet<int>();

        if (!meshData.NamedTriangleGroups.ContainsKey(groupName))
            return relevantVertexIds;

        KoreMeshTriangleGroup? sourceGroup = meshData.NamedGroup(groupName);
        foreach (int triangleId in sourceGroup?.TriangleIds ?? Enumerable.Empty<int>())
        {
            if (meshData.Triangles.ContainsKey(triangleId))
            {
                KoreMeshTriangle triangle = meshData.Triangles[triangleId];
                relevantVertexIds.Add(triangle.A);
                relevantVertexIds.Add(triangle.B);
                relevantVertexIds.Add(triangle.C);
            }
        }

        return relevantVertexIds;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Normal
    // --------------------------------------------------------------------------------------------

    public static Dictionary<int, KoreXYZVector> NormalsForGroup(KoreMeshData meshData, string groupName)
    {
        Dictionary<int, KoreXYZVector> groupNormals = new Dictionary<int, KoreXYZVector>();

        // Return an empty dictionary if no group found
        if (!meshData.NamedTriangleGroups.ContainsKey(groupName))
            return groupNormals;

        // Get the relevant vertex IDs for the group
        HashSet<int> relevantVertexIds = GetVertexIdsForGroup(meshData, groupName);

        // Loop through each ID and copy the result into the groupNormals dictionary
        foreach (int currVertexId in relevantVertexIds)
        {
            if (meshData.Normals.ContainsKey(currVertexId))
                groupNormals[currVertexId] = meshData.Normals[currVertexId];
        }

        return groupNormals;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: UV
    // --------------------------------------------------------------------------------------------

    public static Dictionary<int, KoreXYVector> UVsForGroup(KoreMeshData meshData, string groupName)
    {
        Dictionary<int, KoreXYVector> groupUVs = new Dictionary<int, KoreXYVector>();

        // Return an empty dictionary if no group found
        if (!meshData.NamedTriangleGroups.ContainsKey(groupName))
            return groupUVs;

        // Get the relevant vertex IDs for the group
        HashSet<int> relevantVertexIds = GetVertexIdsForGroup(meshData, groupName);

        // Loop through each ID and copy the result into the groupUVs dictionary
        foreach (int currVertexId in relevantVertexIds)
        {
            if (meshData.UVs.ContainsKey(currVertexId))
                groupUVs[currVertexId] = meshData.UVs[currVertexId];
        }

        return groupUVs;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Vertex Color
    // --------------------------------------------------------------------------------------------

    // Get vertex colors for all vertices in a specific group
    public static Dictionary<int, KoreColorRGB> VertexColorsForGroup(KoreMeshData meshData, string groupName)
    {
        Dictionary<int, KoreColorRGB> groupColors = new Dictionary<int, KoreColorRGB>();

        // If no group found, return empty list
        if (!meshData.NamedTriangleGroups.ContainsKey(groupName))
            return groupColors;

        // Get the relevant vertex IDs for the group
        HashSet<int> relevantVertexIds = GetVertexIdsForGroup(meshData, groupName);

        // Loop through each ID and copy the result into the groupColors dictionary
        foreach (int currVertexId in relevantVertexIds)
        {
            if (meshData.VertexColors.ContainsKey(currVertexId))
                groupColors[currVertexId] = meshData.VertexColors[currVertexId];
        }

        return groupColors;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Line
    // --------------------------------------------------------------------------------------------

    public static Dictionary<int, KoreMeshLine> LinesForGroup(KoreMeshData meshData, string groupName)
    {
        Dictionary<int, KoreMeshLine> groupLines = new Dictionary<int, KoreMeshLine>();

        // If no group found, return empty list
        if (!meshData.NamedTriangleGroups.ContainsKey(groupName))
            return groupLines;

        // Get the relevant vertex IDs for the group
        HashSet<int> relevantVertexIds = GetVertexIdsForGroup(meshData, groupName);

        // Find a sub-set of the Lines list where both vertices are in the HashSet
        List<int> relevantLineIds = meshData.Lines
            .Where(line => relevantVertexIds.Contains(line.Value.A) && relevantVertexIds.Contains(line.Value.B))
            .Select(line => line.Key)
            .ToList();

        // Copy the relevantLineIds data into an output dictionary
        foreach (int lineId in relevantLineIds)
        {
            if (meshData.Lines.ContainsKey(lineId))
                groupLines[lineId] = meshData.Lines[lineId];
        }

        return groupLines;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Line Colors
    // --------------------------------------------------------------------------------------------

    public static Dictionary<int, KoreMeshLineColour> LineColorsForGroup(KoreMeshData meshData, string groupName)
    {
        Dictionary<int, KoreMeshLineColour> groupLineColors = new Dictionary<int, KoreMeshLineColour>();

        // If no group found, return empty list
        if (!meshData.NamedTriangleGroups.ContainsKey(groupName))
            return groupLineColors;

        // Get the relevant vertex IDs for the group
        HashSet<int> relevantVertexIds = GetVertexIdsForGroup(meshData, groupName);

        // Find a sub-set of the Lines list where both vertices are in the HashSet
        List<int> relevantLineIds = meshData.Lines
            .Where(line => relevantVertexIds.Contains(line.Value.A) && relevantVertexIds.Contains(line.Value.B))
            .Select(line => line.Key)
            .ToList();

        // Copy the relevantLineColorIds data into an output dictionary
        foreach (int lineId in relevantLineIds)
        {
            if (meshData.LineColors.ContainsKey(lineId))
                groupLineColors[lineId] = meshData.LineColors[lineId];
        }

        return groupLineColors;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Triangle
    // --------------------------------------------------------------------------------------------

    // Get triangles for a specific group (with vertex IDs preserved)
    public static Dictionary<int, KoreMeshTriangle> TrianglesForGroup(KoreMeshData meshData, string groupName)
    {
        Dictionary<int, KoreMeshTriangle> groupTriangles = new Dictionary<int, KoreMeshTriangle>();

        // if no group found, return empty list
        if (!meshData.NamedTriangleGroups.ContainsKey(groupName))
            return groupTriangles;

        // Get the group and copy out the triangle list
        KoreMeshTriangleGroup? sourceGroup = meshData.NamedGroup(groupName);

        foreach (int triangleId in sourceGroup?.TriangleIds ?? Enumerable.Empty<int>())
        {
            if (meshData.Triangles.ContainsKey(triangleId))
                groupTriangles[triangleId] = meshData.Triangles[triangleId];
        }

        return groupTriangles;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Material
    // --------------------------------------------------------------------------------------------

    public static List<KoreMeshMaterial> MaterialsForGroup(KoreMeshData meshData, string groupName)
    {
        List<KoreMeshMaterial> materialList = new List<KoreMeshMaterial>();

        // If no group found, return empty list
        if (!meshData.NamedTriangleGroups.ContainsKey(groupName))
            return materialList;

        // // Get the group and copy out the triangle list
        // KoreMeshTriangleGroup? sourceGroup = meshData.NamedGroup(groupName);

        // if (sourceGroup != null)
        
        materialList.Add(meshData.MaterialForGroup(groupName));

        return materialList;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Group
    // --------------------------------------------------------------------------------------------

    public static Dictionary<string, KoreMeshTriangleGroup> NamedTriangleGroupsForGroup(KoreMeshData meshData, string groupName)
    {
        var groupDict = new Dictionary<string, KoreMeshTriangleGroup>();

        if (!meshData.HasNamedGroup(groupName))
            return groupDict;

        KoreMeshTriangleGroup? group = meshData.NamedGroup(groupName);
        if (group == null)
            return groupDict;

        groupDict[groupName] = group.Value;
        return groupDict;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Mesh
    // --------------------------------------------------------------------------------------------

    // Usage Example: KoreMeshData focussedMesh = KoreMeshDataEditOps.MeshForGroup(meshData, groupName);

    public static KoreMeshData MeshForGroup(KoreMeshData meshData, string groupName)
    {
        KoreMeshData groupMesh = new KoreMeshData();

        groupMesh.Vertices = VerticesForGroup(meshData, groupName);
        groupMesh.Normals = NormalsForGroup(meshData, groupName);
        groupMesh.UVs = UVsForGroup(meshData, groupName);
        groupMesh.VertexColors = VertexColorsForGroup(meshData, groupName);
        groupMesh.Lines = LinesForGroup(meshData, groupName);
        groupMesh.LineColors = LineColorsForGroup(meshData, groupName);
        groupMesh.Triangles = TrianglesForGroup(meshData, groupName);
        groupMesh.Materials = MaterialsForGroup(meshData, groupName);
        groupMesh.NamedTriangleGroups = NamedTriangleGroupsForGroup(meshData, groupName);

        groupMesh.NextVertexId = meshData.NextVertexId;
        groupMesh.NextLineId = meshData.NextLineId;
        groupMesh.NextTriangleId = meshData.NextTriangleId;

        return groupMesh;
    }

    // --------------------------------------------------------------------------------------------

    // Create a separate mesh for each named group in the mesh

    public static Dictionary<string, KoreMeshData> MeshForEachGroup(KoreMeshData meshData)
    {
        Dictionary<string, KoreMeshData> groupMeshes = new Dictionary<string, KoreMeshData>();

        foreach (var groupName in meshData.NamedTriangleGroups.Keys)
        {
            // Get the mesh for the specified group
            KoreMeshData groupMesh = MeshForGroup(meshData, groupName);
            if (groupMesh != null)
                groupMeshes[groupName] = groupMesh;
        }

        return groupMeshes;
    }

}
