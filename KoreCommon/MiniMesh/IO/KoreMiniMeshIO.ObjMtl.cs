// <fileheader>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

#nullable enable

namespace KoreCommon;

// OBJ and MTL file format export/import for KoreMiniMesh
// KoreMiniMesh is a simplified mesh format focusing on colored materials without UVs and normals
// OBJ format is a simple text-based 3D geometry format widely supported by 3D applications
// MTL format defines materials referenced by OBJ files

public static partial class KoreMiniMeshIO
{
    // --------------------------------------------------------------------------------------------
    // MARK: OBJ Export
    // --------------------------------------------------------------------------------------------

    // Export KoreMiniMesh to OBJ format string
    // mesh: Mesh data to export
    // objectName: Name for the object in the OBJ file
    // mtlFileName: Name of the MTL file (without extension)
    // returns: OBJ file content as string
    public static string ToObj(KoreMiniMesh mesh, string objectName = "KoreMiniMesh", string? mtlFileName = null)
    {
        var sb = new StringBuilder();

        // OBJ Header
        sb.AppendLine("# OBJ file exported from KoreMiniMesh");
        sb.AppendLine($"# Object: {objectName}");
        sb.AppendLine();

        // Reference MTL file if colors exist and MTL filename provided
        if (mesh.Colors.Count > 0 && !string.IsNullOrEmpty(mtlFileName))
        {
            sb.AppendLine($"mtllib {mtlFileName}.mtl");
            sb.AppendLine();
        }

        sb.AppendLine($"o {objectName}");
        sb.AppendLine();

        // Export vertices (v x y z)
        // Create a mapping from internal vertex IDs to OBJ vertex indices (1-based)
        var vertexIdToObjIndex = new Dictionary<int, int>();
        int objVertexIndex = 1;

        foreach (var kvp in mesh.Vertices.OrderBy(x => x.Key))
        {
            int vertexId = kvp.Key;
            KoreXYZVector pos = kvp.Value;

            vertexIdToObjIndex[vertexId] = objVertexIndex++;
            sb.AppendLine($"v {pos.X.ToString("F6", CultureInfo.InvariantCulture)} {pos.Y.ToString("F6", CultureInfo.InvariantCulture)} {pos.Z.ToString("F6", CultureInfo.InvariantCulture)}");
        }
        sb.AppendLine();

        // Export triangles grouped by color/material
        if (mesh.Triangles.Count > 0)
        {
            ExportTrianglesByColor(mesh, sb, vertexIdToObjIndex);
        }

        // Export lines as line elements (l v1 v2)
        if (mesh.Lines.Count > 0)
        {
            sb.AppendLine("# Lines");
            foreach (var kvp in mesh.Lines.OrderBy(x => x.Key))
            {
                KoreMiniMeshLine line = kvp.Value;
                if (vertexIdToObjIndex.TryGetValue(line.A, out int objIndexA) &&
                    vertexIdToObjIndex.TryGetValue(line.B, out int objIndexB))
                {
                    sb.AppendLine($"l {objIndexA} {objIndexB}");
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static void ExportTrianglesByColor(KoreMiniMesh mesh, StringBuilder sb, Dictionary<int, int> vertexIdToObjIndex)
    {
        // Group triangles by their associated groups (which have material names)
        var trianglesByGroup = new Dictionary<string, (string materialName, List<KoreMiniMeshTri> triangles)>();

        // Process named groups
        foreach (var groupKvp in mesh.Groups)
        {
            string groupName = groupKvp.Key;
            KoreMiniMeshGroup group = groupKvp.Value;

            trianglesByGroup[groupName] = (group.MaterialName, new List<KoreMiniMeshTri>());

            // Add triangles from this group
            foreach (int triangleId in group.TriIdList)
            {
                if (mesh.Triangles.TryGetValue(triangleId, out KoreMiniMeshTri triangle))
                {
                    trianglesByGroup[groupName].triangles.Add(triangle);
                }
            }
        }

        // Add any remaining triangles not in named groups to a default group
        var usedTriangleIds = new HashSet<int>();
        foreach (var group in mesh.Groups.Values)
        {
            foreach (int triangleId in group.TriIdList)
                usedTriangleIds.Add(triangleId);
        }

        var ungroupedTriangles = mesh.Triangles.Where(kvp => !usedTriangleIds.Contains(kvp.Key)).Select(kvp => kvp.Value).ToList();
        if (ungroupedTriangles.Count > 0)
        {
            trianglesByGroup["default"] = ("", ungroupedTriangles); // Empty string indicates no specific material
        }

        // Export triangles grouped by material
        foreach (var groupData in trianglesByGroup)
        {
            string groupName = groupData.Key;
            string materialName = groupData.Value.materialName;
            List<KoreMiniMeshTri> triangles = groupData.Value.triangles;

            if (triangles.Count == 0) continue;

            sb.AppendLine($"# Group: {groupName}");

            // Use material if available
            if (!string.IsNullOrEmpty(materialName))
            {
                sb.AppendLine($"usemtl {materialName}");
            }
            else if (groupName != "default")
            {
                sb.AppendLine($"usemtl {groupName}");
            }

            foreach (var triangle in triangles)
            {
                if (vertexIdToObjIndex.TryGetValue(triangle.A, out int objIndexA) &&
                    vertexIdToObjIndex.TryGetValue(triangle.B, out int objIndexB) &&
                    vertexIdToObjIndex.TryGetValue(triangle.C, out int objIndexC))
                {
                    sb.AppendLine($"f {objIndexA} {objIndexB} {objIndexC}");
                }
            }
            sb.AppendLine();
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: MTL Export
    // --------------------------------------------------------------------------------------------

    // Export materials to MTL format string
    // mesh: Mesh data containing materials
    // returns: MTL file content as string
    public static string ToMtl(KoreMiniMesh mesh)
    {
        var sb = new StringBuilder();

        // MTL Header
        sb.AppendLine("# MTL file exported from KoreMiniMesh");
        sb.AppendLine();

        // Collect all unique materials used in groups
        var usedMaterials = new HashSet<string>();
        foreach (var group in mesh.Groups.Values)
        {
            if (!string.IsNullOrEmpty(group.MaterialName))
                usedMaterials.Add(group.MaterialName);
        }

        // Export materials
        foreach (string materialName in usedMaterials.OrderBy(x => x))
        {
            var material = mesh.Materials.FirstOrDefault(m => m.Name == materialName);
            if (material.Name != null) // Check if material was found
            {
                sb.AppendLine($"newmtl {materialName}");

                // Convert color to 0-1 range
                float r = material.BaseColor.R / 255.0f;
                float g = material.BaseColor.G / 255.0f;
                float b = material.BaseColor.B / 255.0f;

                // Standard MTL properties
                sb.AppendLine($"Ka 1.000000 1.000000 1.000000");
                sb.AppendLine($"Kd {r.ToString("F6", CultureInfo.InvariantCulture)} {g.ToString("F6", CultureInfo.InvariantCulture)} {b.ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"Ks 0.500000 0.500000 0.500000");
                sb.AppendLine($"Ns 96.0");

                // PBR Extensions - these preserve your exact metallic/roughness values for Blender
                sb.AppendLine($"Pr {material.Roughness.ToString("F6", CultureInfo.InvariantCulture)}"); // Roughness
                sb.AppendLine($"Pm {material.Metallic.ToString("F6", CultureInfo.InvariantCulture)}");   // Metallic
                sb.AppendLine($"Pc {r.ToString("F6", CultureInfo.InvariantCulture)} {g.ToString("F6", CultureInfo.InvariantCulture)} {b.ToString("F6", CultureInfo.InvariantCulture)}"); // Base Color

                // Transparency (from alpha channel)
                float alpha = material.BaseColor.A / 255.0f;
                if (alpha < 1.0f)
                {
                    sb.AppendLine($"d {alpha.ToString("F6", CultureInfo.InvariantCulture)}");
                    sb.AppendLine($"Tr {(1.0f - alpha).ToString("F6", CultureInfo.InvariantCulture)}");
                }

                // Illumination model (2 = color on and ambient on, with specular)
                sb.AppendLine("illum 2");

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Convenience Methods
    // --------------------------------------------------------------------------------------------

    // Export mesh to both OBJ and MTL format strings
    // mesh: Mesh data to export
    // objectName: Name for the object
    // mtlFileName: Name of the MTL file (without extension)
    // returns: Tuple containing (objContent, mtlContent)
    public static (string objContent, string mtlContent) ToObjMtl(KoreMiniMesh mesh, string objectName = "KoreMiniMesh", string mtlFileName = "materials")
    {
        string objContent = ToObj(mesh, objectName, mtlFileName);
        string mtlContent = ToMtl(mesh);
        return (objContent, mtlContent);
    }

    // Import KoreMiniMesh from OBJ and MTL files (convenience method)
    // objContent: OBJ file content
    // mtlContent: MTL file content
    // returns: Imported KoreMiniMesh
    public static KoreMiniMesh FromObjMtl(string objContent, string mtlContent)
    {
        return FromObj(objContent, mtlContent);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: OBJ Import
    // --------------------------------------------------------------------------------------------

    // Import KoreMiniMesh from OBJ format string
    // objContent: OBJ file content
    // mtlContent: Optional MTL file content
    // returns: Imported KoreMiniMesh
    public static KoreMiniMesh FromObj(string objContent, string? mtlContent = null)
    {
        var mesh = new KoreMiniMesh();

        // Parse MTL content first to get material information
        if (!string.IsNullOrEmpty(mtlContent))
        {
            ParseMtlForMaterials(mesh, mtlContent);
        }

        // Parse OBJ content
        ParseObjContent(mesh, objContent);

        return mesh;
    }

    private static void ParseMtlForMaterials(KoreMiniMesh mesh, string mtlContent)
    {
        var lines = mtlContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        string? currentMaterialName = null;
        KoreColorRGB currentColor = KoreColorRGB.White;
        float currentMetallic = 0.0f;
        float currentRoughness = 1.0f;

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            string[] parts = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0].ToLower())
            {
                case "newmtl": // New material
                    // Save previous material if it exists
                    if (!string.IsNullOrEmpty(currentMaterialName))
                    {
                        var material = new KoreMiniMeshMaterial(currentMaterialName, currentColor, currentMetallic, currentRoughness);
                        mesh.Materials.Add(material);
                    }

                    // Start new material
                    if (parts.Length >= 2)
                    {
                        currentMaterialName = parts[1];
                        currentColor = KoreColorRGB.White;
                        currentMetallic = 0.0f;
                        currentRoughness = 1.0f;
                    }
                    break;

                case "kd": // Diffuse color
                    if (parts.Length >= 4 &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float r) &&
                        float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float g) &&
                        float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float b))
                    {
                        currentColor = new KoreColorRGB(
                            (byte)(Math.Clamp(r, 0f, 1f) * 255),
                            (byte)(Math.Clamp(g, 0f, 1f) * 255),
                            (byte)(Math.Clamp(b, 0f, 1f) * 255),
                            255
                        );
                    }
                    break;

                case "ks": // Specular color (we'll use this to infer metallic)
                    if (parts.Length >= 4 &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float sr) &&
                        float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float sg) &&
                        float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float sb))
                    {
                        // Convert specular to metallic (simplified approach)
                        currentMetallic = (sr + sg + sb) / 3.0f;
                    }
                    break;

                case "ns": // Shininess (legacy - only use if no Pr extension found)
                    if (parts.Length >= 2 &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float shininess))
                    {
                        // Convert shininess to roughness (inverse relationship) - backup if no Pr found
                        currentRoughness = 1.0f - Math.Clamp(shininess / 100.0f, 0f, 1f);
                    }
                    break;

                case "pr": // PBR Roughness extension - preserves exact values
                    if (parts.Length >= 2 &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float roughness))
                    {
                        currentRoughness = Math.Clamp(roughness, 0f, 1f);
                    }
                    break;

                case "pm": // PBR Metallic extension - preserves exact values
                    if (parts.Length >= 2 &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float metallic))
                    {
                        currentMetallic = Math.Clamp(metallic, 0f, 1f);
                    }
                    break;

                case "pc": // PBR Base Color extension - preserves exact color
                    if (parts.Length >= 4 &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float pr) &&
                        float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float pg) &&
                        float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float pb))
                    {
                        currentColor = new KoreColorRGB(
                            (byte)(Math.Clamp(pr, 0f, 1f) * 255),
                            (byte)(Math.Clamp(pg, 0f, 1f) * 255),
                            (byte)(Math.Clamp(pb, 0f, 1f) * 255),
                            currentColor.A // Preserve alpha
                        );
                    }
                    break;

                case "d": // Transparency
                    if (parts.Length >= 2 &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float alpha))
                    {
                        currentColor = new KoreColorRGB(
                            currentColor.R,
                            currentColor.G,
                            currentColor.B,
                            (byte)(Math.Clamp(alpha, 0f, 1f) * 255)
                        );
                    }
                    break;

                case "ka": // Ambient color (ignore - we use Kd for base color)
                    // Example: Ka 1.000000 1.000000 1.000000
                    break;

                case "ke": // Emissive color (ignore - KoreMiniMesh doesn't support emission)
                    // Example: Ke 0.000000 0.000000 0.000000
                    break;

                case "ni": // Optical density/index of refraction (ignore)
                    // Example: Ni 1.500000
                    break;

                case "illum": // Illumination model (ignore)
                    // Example: illum 1, illum 2
                    break;
            }
        }

        // Save the last material
        if (!string.IsNullOrEmpty(currentMaterialName))
        {
            var material = new KoreMiniMeshMaterial(currentMaterialName, currentColor, currentMetallic, currentRoughness);
            mesh.Materials.Add(material);
        }
    }

    private static void ParseObjContent(KoreMiniMesh mesh, string objContent)
    {
        var lines = objContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var vertices = new List<KoreXYZVector>();

        var currentMaterialTriangles = new Dictionary<string, List<int>>();
        string currentMaterial = "default";

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            string[] parts = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0].ToLower())
            {
                case "mtllib": // Material library file (ignore for now, since we accept MTL content directly)
                    // Example: mtllib MiniMesh_Cube2.mtl
                    // We ignore this since MTL content is passed separately
                    break;

                case "o": // Object name
                    // Example: o MiniMesh_Cube
                    // We could store this for metadata, but currently ignore it
                    break;

                case "s": // Smoothing group
                    // Example: s 0, s 1, s off
                    // We ignore smoothing groups since KoreMiniMesh doesn't use them
                    break;

                case "v": // Vertex
                    if (parts.Length >= 4 &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                        float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                    {
                        vertices.Add(new KoreXYZVector(x, y, z));
                    }
                    break;

                case "usemtl": // Use material
                    if (parts.Length >= 2)
                    {
                        currentMaterial = parts[1];
                    }
                    break;

                case "g": // Group name (alternative to object name)
                    // Example: g cube_group
                    // We ignore groups since we use materials for organization
                    break;

                case "vt": // Texture coordinate (ignore - KoreMiniMesh doesn't use UVs)
                    // Example: vt 0.5 0.5
                    break;

                case "vn": // Vertex normal (ignore - KoreMiniMesh calculates normals on demand)
                    // Example: vn 0.0 1.0 0.0
                    break;

                case "f": // Face (triangle only)
                    if (parts.Length >= 4) // Exactly 3 vertices for a triangle (plus the 'f' command)
                    {
                        var faceVertices = new List<int>();

                        // Parse face vertices - supports multiple formats:
                        // v1 v2 v3 (vertex only)
                        // v1/vt1 v2/vt2 v3/vt3 (vertex/texture)
                        // v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3 (vertex/texture/normal)
                        // v1//vn1 v2//vn2 v3//vn3 (vertex//normal)
                        for (int i = 1; i < parts.Length && i <= 3; i++) // Only take first 3 vertices
                        {
                            // Split by '/' and take only the first part (vertex index)
                            string vertexRef = parts[i].Split('/')[0];
                            if (int.TryParse(vertexRef, out int vertexIndex))
                            {
                                // Handle negative indices (relative to end of vertex list)
                                if (vertexIndex < 0)
                                {
                                    vertexIndex = vertices.Count + vertexIndex + 1;
                                }

                                if (vertexIndex > 0 && vertexIndex <= vertices.Count)
                                {
                                    faceVertices.Add(vertexIndex);
                                }
                                else
                                {
                                    // Invalid vertex index - skip this face
                                    goto nextLine;
                                }
                            }
                            else
                            {
                                // Invalid vertex format - skip this face
                                goto nextLine;
                            }
                        }

                        // Only process if we have exactly 3 vertices
                        if (faceVertices.Count == 3)
                        {
                            try
                            {
                                // Add vertices to mesh (convert from 1-based to mesh vertex IDs)
                                int vertexId1 = AddObjVertexToMiniMesh(mesh, vertices, faceVertices[0]);
                                int vertexId2 = AddObjVertexToMiniMesh(mesh, vertices, faceVertices[1]);
                                int vertexId3 = AddObjVertexToMiniMesh(mesh, vertices, faceVertices[2]);

                                // Add triangle
                                int triangleId = mesh.NextTriangleId++;
                                mesh.Triangles[triangleId] = new KoreMiniMeshTri(vertexId1, vertexId2, vertexId3);

                                // Track material assignment
                                if (!currentMaterialTriangles.ContainsKey(currentMaterial))
                                    currentMaterialTriangles[currentMaterial] = new List<int>();
                                currentMaterialTriangles[currentMaterial].Add(triangleId);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                // Invalid vertex reference - skip this triangle
                            }
                        }

                        nextLine:; // Label for breaking out of vertex parsing loop
                    }
                    break;

                case "l": // Line
                    if (parts.Length >= 3)
                    {
                        for (int i = 1; i < parts.Length - 1; i++)
                        {
                            if (int.TryParse(parts[i], out int v1) && int.TryParse(parts[i + 1], out int v2))
                            {
                                // Convert from 1-based to 0-based, then to vertex IDs
                                if (v1 > 0 && v1 <= vertices.Count && v2 > 0 && v2 <= vertices.Count)
                                {
                                    int vertexId1 = AddObjVertexToMiniMesh(mesh, vertices, v1);
                                    int vertexId2 = AddObjVertexToMiniMesh(mesh, vertices, v2);

                                    int lineId = mesh.NextLineId++;
                                    // Use a default color for lines (could be enhanced later)
                                    int defaultColorId = GetOrCreateDefaultColor(mesh);
                                    mesh.Lines[lineId] = new KoreMiniMeshLine(vertexId1, vertexId2, defaultColorId);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        // Create groups for materials
        foreach (var kvp in currentMaterialTriangles)
        {
            string materialName = kvp.Key;
            List<int> triangleIds = kvp.Value;

            if (triangleIds.Count > 0)
            {
                string groupName = materialName == "default" ? "DefaultGroup" : materialName;
                mesh.Groups[groupName] = new KoreMiniMeshGroup(materialName, triangleIds);
            }
        }
    }

    private static int AddObjVertexToMiniMesh(KoreMiniMesh mesh, List<KoreXYZVector> vertices, int objVertexIndex)
    {
        // Convert from 1-based OBJ index to 0-based array index
        int vertexIndex = objVertexIndex - 1;

        if (vertexIndex < 0 || vertexIndex >= vertices.Count)
            throw new ArgumentOutOfRangeException($"Invalid vertex index: {objVertexIndex}");

        KoreXYZVector position = vertices[vertexIndex];

        // Check if this vertex already exists in the mesh
        foreach (var kvp in mesh.Vertices)
        {
            if (kvp.Value.Equals(position))
                return kvp.Key;
        }

        // Add new vertex
        int vertexId = mesh.NextVertexId++;
        mesh.Vertices[vertexId] = position;
        return vertexId;
    }

    private static int GetOrCreateDefaultColor(KoreMiniMesh mesh)
    {
        // Look for existing white color
        foreach (var kvp in mesh.Colors)
        {
            if (kvp.Value.Equals(KoreColorRGB.White))
                return kvp.Key;
        }

        // Create new default white color
        int colorId = mesh.NextColorId++;
        mesh.Colors[colorId] = KoreColorRGB.White;
        return colorId;
    }
}

