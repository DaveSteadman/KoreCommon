// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace KoreCommon;

public static partial class KoreMiniMeshOps
{
    // --------------------------------------------------------------------------------------------
    // MARK: VariateGroup
    // --------------------------------------------------------------------------------------------

    // VariateGroup: An experiment in creating additional materials/groups on a mesh as minor variations
    // on a stated group, then assigning the group triangles across new groups. The aspiration is to create
    // some presentation variation on the model, making plain materials more interesting.

    public static void VariateGroup(KoreMiniMesh mesh, string groupName, float variationAmount, int numberOfVariations)
    {
        // Basic validation
        if (variationAmount <= 0) throw new ArgumentException("Variation amount must be positive and non-zero.");
        if (numberOfVariations < 2) throw new ArgumentException("Number of variations must be at least 2.");
        if (!mesh.HasGroup(groupName)) throw new ArgumentException("Group not found in mesh.");

        // Find the group
        KoreMiniMeshGroup group = mesh.GetGroup(groupName);

        // Find the material
        string materialName = group.MaterialName;
        KoreMiniMeshMaterial baseMat = KoreMiniMeshOps.GetGroupMaterial(mesh, groupName);

        List<KoreMiniMeshGroup> newGroups = new();

        // Create the variations
        for (int i = 0; i < numberOfVariations; i++)
        {
            KoreColorRGB noiseCol = KoreColorOps.ColorWithRGBNoise(baseMat.BaseColor, variationAmount);

            float newRough = KoreNumericUtils.ValuePlusNoise(baseMat.Roughness, variationAmount);
            float newMetal = KoreNumericUtils.ValuePlusNoise(baseMat.Metallic, variationAmount);

            // Use constructor to ensure proper clamping of metallic and roughness values
            var newMaterial = new KoreMiniMeshMaterial(
                $"{baseMat.Name}_var{i + 1}",
                noiseCol,
                newMetal,
                newRough
            );
            mesh.AddMaterial(newMaterial);
            KoreCentralLog.AddEntry($"Created variation material: {newMaterial.Name}");

            // Create a new group for this variation
            string newGroupName = $"{groupName}_var{i + 1}";
            mesh.AddGroup(newGroupName, new KoreMiniMeshGroup(newMaterial.Name, new List<int>()));

            // add the new group to a list we need to randomly assign triangles to
            newGroups.Add(mesh.GetGroup(newGroupName));
        }

        // Reassign triangles from the base group to the new groups in round-robin fashion
        // First extract the triangles in the base group
        List<int> baseTriangleIds = new List<int>(group.TriIdList);

        // Clear the existing triangles from the base group
        group.TriIdList.Clear();

        // Add the original group into the destination list
        newGroups.Add(group);

        // Now we step through the triangle list, assigning each one to an element in the newGroups list at random. (Random is important).
        Random rand = new Random();
        foreach (int triId in baseTriangleIds)
        {
            // Pick a random group from the newGroups list
            int randomGroupIndex = rand.Next(newGroups.Count);
            KoreMiniMeshGroup randomGroup = newGroups[randomGroupIndex];

            // Assign the triangle to the random group
            randomGroup.TriIdList.Add(triId);
        }
    }

}
