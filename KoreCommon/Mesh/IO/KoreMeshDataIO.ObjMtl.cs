// <fileheader>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

#nullable enable

namespace KoreCommon;

// OBJ and MTL file format export/import for KoreMeshData
// OBJ format is a simple text-based 3D geometry format widely supported by 3D applications
// MTL format defines materials referenced by OBJ files

public static partial class KoreMeshDataIO
{
    // --------------------------------------------------------------------------------------------
    // MARK: OBJ Export
    // --------------------------------------------------------------------------------------------

    // Export KoreMeshData to OBJ format string
    // mesh: Mesh data to export
    // objectName: Name for the object in the OBJ file
    // mtlFileName: Name of the MTL file (without extension)
    // returns: OBJ file content as string

    // Usage: KoreMeshDataIO.ToObj(meshData, "MyMesh", "MyMeshMaterial");

    public static string ToObj(KoreMeshData mesh, string objectName = "KoreMesh", string? mtlFileName = null)
    {
        var sb = new StringBuilder();

        // OBJ Header
        sb.AppendLine("# OBJ file exported from KoreMeshData");
        sb.AppendLine($"# Object: {objectName}");
        sb.AppendLine();

        // Reference MTL file if materials exist and MTL filename provided
        if (mesh.Materials.Count > 0 && !string.IsNullOrEmpty(mtlFileName))
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

        // Export normals if they exist (vn x y z)
        var hasNormals = mesh.Normals.Count > 0;
        if (hasNormals)
        {
            foreach (var kvp in mesh.Vertices.OrderBy(x => x.Key))
            {
                int vertexId = kvp.Key;
                if (mesh.Normals.TryGetValue(vertexId, out KoreXYZVector normal))
                {
                    sb.AppendLine($"vn {normal.X.ToString("F6", CultureInfo.InvariantCulture)} {normal.Y.ToString("F6", CultureInfo.InvariantCulture)} {normal.Z.ToString("F6", CultureInfo.InvariantCulture)}");
                }
                else
                {
                    // Default normal if missing
                    sb.AppendLine("vn 0.0 1.0 0.0");
                }
            }
            sb.AppendLine();
        }

        // Export texture coordinates if they exist (vt u v)
        var hasUVs = mesh.UVs.Count > 0;
        if (hasUVs)
        {
            foreach (var kvp in mesh.Vertices.OrderBy(x => x.Key))
            {
                int vertexId = kvp.Key;
                if (mesh.UVs.TryGetValue(vertexId, out KoreXYVector uv))
                {
                    sb.AppendLine($"vt {uv.X.ToString("F6", CultureInfo.InvariantCulture)} {uv.Y.ToString("F6", CultureInfo.InvariantCulture)}");
                }
                else
                {
                    // Default UV if missing
                    sb.AppendLine("vt 0.0 0.0");
                }
            }
            sb.AppendLine();
        }

        // Export triangles grouped by material
        if (mesh.Triangles.Count > 0)
        {
            ExportTrianglesByMaterial(mesh, sb, vertexIdToObjIndex, hasNormals, hasUVs);
        }

        // Export lines as line elements (l v1 v2)
        if (mesh.Lines.Count > 0)
        {
            sb.AppendLine("# Lines");
            foreach (var kvp in mesh.Lines.OrderBy(x => x.Key))
            {
                KoreMeshLine line = kvp.Value;
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

    private static void ExportTrianglesByMaterial(KoreMeshData mesh, StringBuilder sb, Dictionary<int, int> vertexIdToObjIndex, bool hasNormals, bool hasUVs)
    {
        // Group triangles by material
        var trianglesByMaterial = new Dictionary<string, List<KoreMeshTriangle>>();

        // First, try to use named triangle groups which have material assignments
        foreach (var groupKvp in mesh.NamedTriangleGroups)
        {
            string groupName = groupKvp.Key;
            KoreMeshTriangleGroup group = groupKvp.Value;

            // Get material name from the group
            string materialName = "default";
            if (!string.IsNullOrEmpty(group.MaterialName))
            {
                materialName = group.MaterialName;
            }

            if (!trianglesByMaterial.ContainsKey(materialName))
                trianglesByMaterial[materialName] = new List<KoreMeshTriangle>();

            // Add triangles from this group
            foreach (int triangleId in group.TriangleIds)
            {
                if (mesh.Triangles.TryGetValue(triangleId, out KoreMeshTriangle triangle))
                {
                    trianglesByMaterial[materialName].Add(triangle);
                }
            }
        }

        // Add any remaining triangles not in named groups
        var usedTriangleIds = new HashSet<int>();
        foreach (var group in mesh.NamedTriangleGroups.Values)
        {
            foreach (int triangleId in group.TriangleIds)
                usedTriangleIds.Add(triangleId);
        }

        var ungroupedTriangles = mesh.Triangles.Where(kvp => !usedTriangleIds.Contains(kvp.Key)).Select(kvp => kvp.Value).ToList();
        if (ungroupedTriangles.Count > 0)
        {
            trianglesByMaterial["default"] = ungroupedTriangles;
        }

        // Export triangles grouped by material
        foreach (var materialGroup in trianglesByMaterial)
        {
            string materialName = materialGroup.Key;
            List<KoreMeshTriangle> triangles = materialGroup.Value;

            if (triangles.Count == 0) continue;

            sb.AppendLine($"# Material: {materialName}");
            if (materialName != "default")
            {
                sb.AppendLine($"usemtl {materialName}");
            }

            foreach (var triangle in triangles)
            {
                if (vertexIdToObjIndex.TryGetValue(triangle.A, out int objIndexA) &&
                    vertexIdToObjIndex.TryGetValue(triangle.B, out int objIndexB) &&
                    vertexIdToObjIndex.TryGetValue(triangle.C, out int objIndexC))
                {
                    // Format: f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3
                    if (hasNormals && hasUVs)
                    {
                        sb.AppendLine($"f {objIndexA}/{objIndexA}/{objIndexA} {objIndexB}/{objIndexB}/{objIndexB} {objIndexC}/{objIndexC}/{objIndexC}");
                    }
                    else if (hasNormals)
                    {
                        sb.AppendLine($"f {objIndexA}//{objIndexA} {objIndexB}//{objIndexB} {objIndexC}//{objIndexC}");
                    }
                    else if (hasUVs)
                    {
                        sb.AppendLine($"f {objIndexA}/{objIndexA} {objIndexB}/{objIndexB} {objIndexC}/{objIndexC}");
                    }
                    else
                    {
                        sb.AppendLine($"f {objIndexA} {objIndexB} {objIndexC}");
                    }
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
    public static string ToMtl(KoreMeshData mesh)
    {
        var sb = new StringBuilder();

        // MTL Header
        sb.AppendLine("# MTL file exported from KoreMeshData");
        sb.AppendLine();

        foreach (KoreMeshMaterial material in mesh.Materials)
        {
            // KoreMeshMaterial material = kvp.Value;

            sb.AppendLine($"newmtl {material.Name}");

            // Ambient color (usually same as diffuse)
            float r = material.BaseColor.R / 255.0f;
            float g = material.BaseColor.G / 255.0f;
            float b = material.BaseColor.B / 255.0f;

            sb.AppendLine($"Ka {r.ToString("F6", CultureInfo.InvariantCulture)} {g.ToString("F6", CultureInfo.InvariantCulture)} {b.ToString("F6", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Kd {r.ToString("F6", CultureInfo.InvariantCulture)} {g.ToString("F6", CultureInfo.InvariantCulture)} {b.ToString("F6", CultureInfo.InvariantCulture)}");

            // Specular color (white for metals, darker for non-metals)
            float specular = material.Metallic;
            sb.AppendLine($"Ks {specular.ToString("F6", CultureInfo.InvariantCulture)} {specular.ToString("F6", CultureInfo.InvariantCulture)} {specular.ToString("F6", CultureInfo.InvariantCulture)}");

            // Specular exponent (inversely related to roughness)
            float shininess = 1.0f + (1.0f - material.Roughness) * 199.0f; // Linear interpolation from 1 to 200
            sb.AppendLine($"Ns {shininess.ToString("F2", CultureInfo.InvariantCulture)}");

            // Transparency (from alpha channel)
            float alpha = material.BaseColor.A / 255.0f;
            if (alpha < 1.0f)
            {
                sb.AppendLine($"d {alpha.ToString("F6", CultureInfo.InvariantCulture)}");
                sb.AppendLine($"Tr {(1.0f - alpha).ToString("F6", CultureInfo.InvariantCulture)}");
            }

            // Texture map (if filename is specified)
            if (!string.IsNullOrEmpty(material.Filename))
            {
                sb.AppendLine($"map_Kd {material.Filename}");
            }

            // Illumination model (2 = color on and ambient on and highlight on)
            sb.AppendLine("illum 2");

            sb.AppendLine();
        }

        return sb.ToString();
    }



    // --------------------------------------------------------------------------------------------
    // MARK: Convenience Methods
    // --------------------------------------------------------------------------------------------

    // Export mesh to both OBJ and MTL format strings
    // mesh: The mesh data to export
    // objectName: Name for the object
    // mtlFileName: Name of the MTL file (without extension)
    // returns: Tuple containing (objContent, mtlContent)
    public static (string objContent, string mtlContent) ToObjMtl(KoreMeshData mesh, string objectName = "KoreMesh", string mtlFileName = "materials")
    {
        string objContent = ToObj(mesh, objectName, mtlFileName);
        string mtlContent = ToMtl(mesh);
        return (objContent, mtlContent);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: OBJ Import
    // --------------------------------------------------------------------------------------------

    // Import KoreMeshData from OBJ format string
    // objContent: OBJ file content
    // mtlContent: Optional MTL file content for materials
    // returns: Imported mesh data
    public static KoreMeshData FromObj(string objContent, string? mtlContent = null)
    {
        var mesh = new KoreMeshData();

        // Add the materials into the mesh - there is no dependency there
        // Parse materials first if provided
        var materials = new Dictionary<string, KoreMeshMaterial>();
        if (!string.IsNullOrEmpty(mtlContent))
        {
            materials = ParseMtl(mtlContent);

            // Add materials to mesh using AddMaterial
            foreach (var kvp in materials)
            {
                mesh.AddMaterial(kvp.Value);
            }
        }

        // Parse OBJ content
        ParseObj(objContent, mesh, materials);

        return mesh;
    }

    private static void ParseObj(string objContent, KoreMeshData mesh, Dictionary<string, KoreMeshMaterial> materials)
    {
        var lines = objContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var vertices = new List<KoreXYZVector>();
        var normals = new List<KoreXYZVector>();
        var uvs = new List<KoreXYVector>();

        string currentMaterial = "default";
        var materialTriangles = new Dictionary<string, List<int>>();

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            string[] parts = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0].ToLower())
            {
                case "v": // Vertex position
                    if (parts.Length >= 4)
                    {
                        if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                            float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                        {
                            vertices.Add(new KoreXYZVector(x, y, z));
                        }
                    }
                    break;

                case "vn": // Vertex normal
                    if (parts.Length >= 4)
                    {
                        if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float nx) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float ny) &&
                            float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float nz))
                        {
                            normals.Add(new KoreXYZVector(nx, ny, nz));
                        }
                    }
                    break;

                case "vt": // Texture coordinate
                    if (parts.Length >= 3)
                    {
                        if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float u) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                        {
                            uvs.Add(new KoreXYVector(u, v));
                        }
                    }
                    break;

                case "usemtl": // Use material
                    if (parts.Length >= 2)
                    {
                        currentMaterial = parts[1];
                        if (!materialTriangles.ContainsKey(currentMaterial))
                            materialTriangles[currentMaterial] = new List<int>();
                    }
                    break;

                case "f": // Face (triangle)
                    if (parts.Length >= 4) // At least 3 vertices for a triangle
                    {
                        var faceVertices = new List<(int vertex, int uv, int normal)>();

                        // Parse face vertices (can be v, v/vt, v/vt/vn, or v//vn format)
                        for (int i = 1; i < parts.Length; i++)
                        {
                            var indices = ParseFaceVertex(parts[i]);
                            if (indices.HasValue)
                                faceVertices.Add(indices.Value);
                        }

                        // Triangulate if more than 3 vertices (fan triangulation)
                        for (int i = 1; i < faceVertices.Count - 1; i++)
                        {
                            var v1 = faceVertices[0];
                            var v2 = faceVertices[i];
                            var v3 = faceVertices[i + 1];

                            // Add vertices to mesh
                            int vertexId1 = AddObjVertex(mesh, vertices, normals, uvs, v1);
                            int vertexId2 = AddObjVertex(mesh, vertices, normals, uvs, v2);
                            int vertexId3 = AddObjVertex(mesh, vertices, normals, uvs, v3);

                            // Add triangle
                            int triangleId = mesh.AddTriangle(vertexId1, vertexId2, vertexId3);

                            // Track material assignment
                            if (!materialTriangles.ContainsKey(currentMaterial))
                                materialTriangles[currentMaterial] = new List<int>();
                            materialTriangles[currentMaterial].Add(triangleId);
                        }
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
                                    var vertex1 = vertices[v1 - 1];
                                    var vertex2 = vertices[v2 - 1];

                                    int vertexId1 = mesh.AddVertex(vertex1);
                                    int vertexId2 = mesh.AddVertex(vertex2);

                                    mesh.AddLine(vertexId1, vertexId2);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        // Create named triangle groups for materials
        foreach (var kvp in materialTriangles)
        {
            string materialName = kvp.Key;
            List<int> triangleIds = kvp.Value;

            if (triangleIds.Count > 0)
            {
                // Ensure the material exists in the mesh (add it if found in materials dictionary)
                if (materials.ContainsKey(materialName))
                {
                    mesh.AddMaterial(materials[materialName]);
                }

                // Create triangle group using the NamedTriangleGroups collection
                string groupName = materialName == "default" ? "DefaultMaterial" : materialName;
                var triangleGroup = new KoreMeshTriangleGroup(materialName, triangleIds);
                mesh.NamedTriangleGroups[groupName] = triangleGroup;
            }
        }
    }

    private static (int vertex, int uv, int normal)? ParseFaceVertex(string faceVertex)
    {
        string[] indices = faceVertex.Split('/');

        int vertex = 0, uv = 0, normal = 0;

        // Vertex index (required)
        if (indices.Length >= 1 && int.TryParse(indices[0], out vertex))
        {
            // UV index (optional)
            if (indices.Length >= 2 && !string.IsNullOrEmpty(indices[1]))
                int.TryParse(indices[1], out uv);

            // Normal index (optional)
            if (indices.Length >= 3 && !string.IsNullOrEmpty(indices[2]))
                int.TryParse(indices[2], out normal);

            return (vertex, uv, normal);
        }

        return null;
    }

    private static int AddObjVertex(KoreMeshData mesh, List<KoreXYZVector> vertices, List<KoreXYZVector> normals, List<KoreXYVector> uvs, (int vertex, int uv, int normal) indices)
    {
        // Convert from 1-based OBJ indices to 0-based array indices
        int vertexIndex = indices.vertex - 1;
        int uvIndex = indices.uv - 1;
        int normalIndex = indices.normal - 1;

        if (vertexIndex < 0 || vertexIndex >= vertices.Count)
            throw new ArgumentOutOfRangeException($"Invalid vertex index: {indices.vertex}");

        KoreXYZVector position = vertices[vertexIndex];

        KoreXYZVector? normal = null;
        if (normalIndex >= 0 && normalIndex < normals.Count)
            normal = normals[normalIndex];

        KoreXYVector? uv = null;
        if (uvIndex >= 0 && uvIndex < uvs.Count)
            uv = uvs[uvIndex];

        return mesh.AddCompleteVertex(position, normal, null, uv);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: MTL Import
    // --------------------------------------------------------------------------------------------

    private static Dictionary<string, KoreMeshMaterial> ParseMtl(string mtlContent)
    {
        var materials = new Dictionary<string, KoreMeshMaterial>();
        var lines = mtlContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        string? currentMaterialName = null;
        var currentMaterial = new Dictionary<string, string>();

        Console.WriteLine($"ParseMtl: Processing {lines.Length} lines");

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            string[] parts = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            Console.WriteLine($"ParseMtl: Processing line: '{trimmedLine}'");

            switch (parts[0].ToLower())
            {
                case "newmtl": // New material
                    // Save previous material if it exists
                    if (currentMaterialName != null)
                    {
                        Console.WriteLine($"ParseMtl: Saving material '{currentMaterialName}' with {currentMaterial.Count} properties");
                        materials[currentMaterialName] = CreateMaterialFromProperties(currentMaterialName, currentMaterial);
                    }

                    // Start new material
                    if (parts.Length >= 2)
                    {
                        currentMaterialName = parts[1];
                        currentMaterial.Clear();
                        Console.WriteLine($"ParseMtl: Starting new material '{currentMaterialName}'");
                    }
                    break;

                case "kd": // Diffuse color
                    if (parts.Length >= 4)
                    {
                        currentMaterial["kd"] = $"{parts[1]} {parts[2]} {parts[3]}";
                        Console.WriteLine($"ParseMtl: Set Kd for '{currentMaterialName}': {currentMaterial["kd"]}");
                    }
                    break;

                case "ks": // Specular color
                    if (parts.Length >= 4)
                    {
                        currentMaterial["ks"] = $"{parts[1]} {parts[2]} {parts[3]}";
                        Console.WriteLine($"ParseMtl: Set Ks for '{currentMaterialName}': {currentMaterial["ks"]}");
                    }
                    break;

                case "ns": // Specular exponent (shininess)
                    if (parts.Length >= 2)
                    {
                        currentMaterial["ns"] = parts[1];
                        Console.WriteLine($"ParseMtl: Set Ns for '{currentMaterialName}': {currentMaterial["ns"]}");
                    }
                    break;

                case "d": // Transparency
                    if (parts.Length >= 2)
                    {
                        currentMaterial["d"] = parts[1];
                        Console.WriteLine($"ParseMtl: Set d for '{currentMaterialName}': {currentMaterial["d"]}");
                    }
                    break;

                case "tr": // Transparency (alternative)
                    if (parts.Length >= 2)
                    {
                        currentMaterial["tr"] = parts[1];
                        Console.WriteLine($"ParseMtl: Set Tr for '{currentMaterialName}': {currentMaterial["tr"]}");
                    }
                    break;

                case "map_kd": // Diffuse texture map
                    if (parts.Length >= 2)
                    {
                        currentMaterial["map_kd"] = parts[1];
                        Console.WriteLine($"ParseMtl: Set map_Kd for '{currentMaterialName}': {currentMaterial["map_kd"]}");
                    }
                    break;

                case "map_ka": // Ambient texture map (treat as diffuse)
                    if (parts.Length >= 2)
                    {
                        currentMaterial["map_ka"] = parts[1];
                        Console.WriteLine($"ParseMtl: Set map_Ka for '{currentMaterialName}': {currentMaterial["map_ka"]}");
                    }
                    break;
            }
        }

        // Don't forget the last material
        if (currentMaterialName != null)
        {
            Console.WriteLine($"ParseMtl: Saving final material '{currentMaterialName}' with {currentMaterial.Count} properties");
            materials[currentMaterialName] = CreateMaterialFromProperties(currentMaterialName, currentMaterial);
        }

        Console.WriteLine($"ParseMtl: Parsed {materials.Count} materials total");
        foreach (var kvp in materials)
        {
            Console.WriteLine($"ParseMtl: Material '{kvp.Key}': {kvp.Value.Name}");
        }

        return materials;
    }

    private static KoreMeshMaterial CreateMaterialFromProperties(string name, Dictionary<string, string> properties)
    {
        // Default values
        KoreColorRGB baseColor = KoreColorRGB.White;
        float metallic = 0.0f;
        float roughness = 0.5f;
        string? textureFilename = null;

        // Check for texture maps first (priority over color)
        if (properties.TryGetValue("map_kd", out string? mapKdValue))
        {
            textureFilename = mapKdValue;
        }
        else if (properties.TryGetValue("map_ka", out string? mapKaValue))
        {
            textureFilename = mapKaValue;
        }

        // Parse diffuse color (fallback if no texture)
        if (properties.TryGetValue("kd", out string? kdValue))
        {
            var parts = kdValue.Split(' ');
            if (parts.Length >= 3 &&
                float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float r) &&
                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float g) &&
                float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float b))
            {
                baseColor = new KoreColorRGB(
                    (byte)(r * 255),
                    (byte)(g * 255),
                    (byte)(b * 255)
                );
            }
        }

        // Parse transparency
        if (properties.TryGetValue("d", out string? dValue))
        {
            if (float.TryParse(dValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float alpha))
            {
                baseColor = new KoreColorRGB(baseColor.R, baseColor.G, baseColor.B, (byte)(alpha * 255));
            }
        }
        else if (properties.TryGetValue("tr", out string? trValue))
        {
            if (float.TryParse(trValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float transparency))
            {
                float alpha = 1.0f - transparency;
                baseColor = new KoreColorRGB(baseColor.R, baseColor.G, baseColor.B, (byte)(alpha * 255));
            }
        }

        // Estimate metallic from specular color
        if (properties.TryGetValue("ks", out string? ksValue))
        {
            var parts = ksValue.Split(' ');
            if (parts.Length >= 3 &&
                float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float sr) &&
                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float sg) &&
                float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float sb))
            {
                // Higher specular values suggest more metallic behavior
                metallic = (sr + sg + sb) / 3.0f;
            }
        }

        // Convert shininess to roughness
        if (properties.TryGetValue("ns", out string? nsValue))
        {
            if (float.TryParse(nsValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float shininess))
            {
                // Convert shininess (1-200) to roughness (1-0)
                roughness = 1.0f - ((shininess - 1.0f) / 199.0f);
                roughness = Math.Max(0.0f, Math.Min(1.0f, roughness)); // Clamp to [0,1]
            }
        }

        return new KoreMeshMaterial(name, baseColor, metallic, roughness, textureFilename);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Convenience Methods
    // --------------------------------------------------------------------------------------------

    // Import mesh from both OBJ and MTL format strings
    // objContent: OBJ file content
    // mtlContent: MTL file content
    // returns: Imported mesh data
    public static KoreMeshData FromObjMtl(string objContent, string mtlContent)
    {
        return FromObj(objContent, mtlContent);
    }
}
