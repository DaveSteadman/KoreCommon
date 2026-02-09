// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

public static partial class KoreMiniMeshOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: Group
    // --------------------------------------------------------------------------------------------

    public static KoreMiniMeshGroup GetOrCreateGroup(KoreMiniMesh mesh, string groupName)
    {
        if (!mesh.HasGroup(groupName))
            mesh.AddGroup(groupName, new KoreMiniMeshGroup("", new List<int>()));

        return mesh.GetGroup(groupName);
    }

    public static void SetGroupMaterial(KoreMiniMesh mesh, string groupName, string materialName)
    {
        if (mesh.HasGroup(groupName))
        {
            KoreMiniMeshGroup group = mesh.GetGroup(groupName);
            group = group with { MaterialName = materialName };
            mesh.Groups[groupName] = group; // re-assign the modified group back to the dictionary
        }
    }

    public static void SetGroupMaterial(KoreMiniMesh mesh, string groupName, KoreMiniMeshMaterial material)
    {
        if (mesh.HasGroup(groupName))
        {
            mesh.AddMaterial(material);
            SetGroupMaterial(mesh, groupName, material.Name);
        }
    }

    public static KoreMiniMeshMaterial GetGroupMaterial(KoreMiniMesh mesh, string groupName)
    {
        if (mesh.HasGroup(groupName))
        {
            KoreMiniMeshGroup group = mesh.GetGroup(groupName);
            return mesh.GetMaterial(group.MaterialName);
        }
        return KoreMiniMeshMaterialPalette.DefaultMaterial;
    }

    public static void AddTrianglesToGroup(KoreMiniMesh mesh, string groupName)
    {
        // create the group if it doesn't exist
        if (!mesh.HasGroup(groupName))
            mesh.AddGroup(groupName, new KoreMiniMeshGroup("", new List<int>()));

        // get the group
        KoreMiniMeshGroup group = mesh.GetGroup(groupName);

        // Add the whole triangle list to the group
        foreach (int currTriId in mesh.Triangles.Keys)
            group.TriIdList.Add(currTriId);
    }


    // --------------------------------------------------------------------------------------------
    // MARK: DissolveGroup
    // --------------------------------------------------------------------------------------------

    // Function to dissolve one group into another, moving all triangles from the source group to the destination group
    // and removing the source group from the mesh.
    // Does not impact materials.

    public static void DissolveGroup(KoreMiniMesh mesh, string groupNameSource, string groupNameDestination)
    {
        // Basic validation
        if (string.IsNullOrEmpty(groupNameSource)) throw new ArgumentException("Source group name must be provided.");
        if (string.IsNullOrEmpty(groupNameDestination)) throw new ArgumentException("Destination group name must be provided.");
        if (groupNameSource == groupNameDestination) throw new ArgumentException("Source and destination group names must be different.");
        if (!mesh.HasGroup(groupNameSource)) throw new ArgumentException("Source group not found in mesh.");
        if (!mesh.HasGroup(groupNameDestination)) throw new ArgumentException("Destination group not found in mesh.");

        // Get the groups
        KoreMiniMeshGroup sourceGroup = mesh.GetGroup(groupNameSource);
        KoreMiniMeshGroup destGroup   = mesh.GetGroup(groupNameDestination);

        // Move triangles from source to destination
        foreach (int triId in sourceGroup.TriIdList)
        {
            destGroup.TriIdList.Add(triId);
        }

        // Clear the source group's triangle list
        sourceGroup.TriIdList.Clear();

        // Remove the source group from the mesh
        mesh.Groups.Remove(groupNameSource);
    }

}
