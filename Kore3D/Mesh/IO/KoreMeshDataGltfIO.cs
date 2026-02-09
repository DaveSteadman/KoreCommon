// <fileheader>

// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Numerics;
// using SharpGLTF.Schema2;
// using SharpGLTF.Geometry;
// using SharpGLTF.Geometry.VertexTypes;
// using SharpGLTF.Materials;

// #nullable enable

// using KoreCommon;
// namespace KoreCommon.Mesh;

// /// <summary>
// /// Handles import/export of KoreMeshData to/from glTF format.
// /// Provides coordinate system conversion and maintains material fidelity.
// /// </summary>
// public static class KoreMeshDataGltfIO
// {
//     // --------------------------------------------------------------------------------------------
//     // MARK: Export
//     // --------------------------------------------------------------------------------------------

//     /// <summary>
//     /// Saves a KoreMeshData to a glTF file (.gltf or .glb).
//     /// Automatically converts from Kore coordinate system to glTF standard (right-handed, Y-up).
//     /// </summary>
//     /// <param name="mesh">The mesh data to save</param>
//     /// <param name="filePath">Output file path (.gltf for JSON, .glb for binary)</param>
//     /// <param name="meshName">Name for the mesh in the glTF file</param>
//     public static void SaveToGltf(KoreMeshData mesh, string filePath, string meshName = "KoreMesh")
//     {
//         // Create a new glTF scene
//         var model = ModelRoot.CreateModel();
//         var scene = model.UseScene("default");

//         // Extract output directory for texture path resolution
//         var outputDirectory = Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory();

//         // Create materials first, passing the output directory for texture search
//         var gltfMaterials = CreateGltfMaterials(model, mesh.Materials, outputDirectory);

//         // Create mesh builder with normal support
//         var meshBuilder = new MeshBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>(meshName);

//         KoreMeshDataEditOps.FlipAllNormals(mesh);
//         KoreMeshDataEditOps.FlipAllTriangleWindings(mesh);

//         // Convert mesh data to glTF format
//         ConvertKoreMeshToGltf(mesh, meshBuilder, gltfMaterials);

//         // Add mesh to scene
//         var node = scene.CreateNode();
//         node.Mesh = model.CreateMesh(meshBuilder);

//         // Save the model
//         model.SaveGLTF(filePath);
//     }

//     /// <summary>
//     /// Loads a glTF file into KoreMeshData format.
//     /// Automatically converts from glTF coordinate system to Kore internal format.
//     /// </summary>
//     /// <param name="filePath">Path to the glTF file</param>
//     /// <returns>Loaded mesh data</returns>
//     public static KoreMeshData LoadFromGltf(string filePath)
//     {
//         var model = ModelRoot.Load(filePath);
//         var mesh = new KoreMeshData();

//         // Convert first mesh found (for now)
//         var gltfMesh = model.LogicalMeshes.FirstOrDefault();
//         if (gltfMesh == null)
//         {
//             throw new InvalidOperationException("No meshes found in glTF file");
//         }

//         ConvertGltfToKoreMesh(gltfMesh, mesh);

//         // Only calculate normals from triangles if no normals were imported
//         if (mesh.Normals.Count == 0)
//         {
//             // Calculate normals from triangles for proper shading
//             KoreMeshDataEditOps.SetNormalsFromTriangles(mesh);
//         }

//         return mesh;
//     }

//     /// Creates glTF materials from KoreMeshMaterial collection
//     /// <param name="model">The glTF model root</param>
//     /// <param name="materials">Collection of Kore materials to convert</param>
//     /// <param name="outputDirectory">Directory where the glTF file will be saved (for texture path resolution)</param>
//     private static Dictionary<string, MaterialBuilder> CreateGltfMaterials(ModelRoot model, IReadOnlyList<KoreMeshMaterial> materials, string outputDirectory)
//     {
//         var gltfMaterials = new Dictionary<string, MaterialBuilder>();

//         foreach (var koreMaterial in materials)
//         {
//             var gltfMaterial = new MaterialBuilder(koreMaterial.Name);

//             // Set base color
//             var baseColor = new Vector4(
//                 (float)koreMaterial.BaseColor.R / 255f,
//                 (float)koreMaterial.BaseColor.G / 255f,
//                 (float)koreMaterial.BaseColor.B / 255f,
//                 1.0f);

//             // Check if material has a texture file
//             if (!string.IsNullOrEmpty(koreMaterial.Filename))
//             {
//                 try
//                 {
//                     // Try to load the texture with improved path resolution
//                     string texturePath = koreMaterial.Filename;

//                     // If it's just a filename, search in the output directory first, then fallback locations
//                     if (!Path.IsPathRooted(texturePath))
//                     {
//                         string[] searchPaths = {
//                             Path.Combine(outputDirectory, texturePath),  // Output directory (priority)
//                             texturePath,  // Current directory
//                             Path.Combine(Directory.GetCurrentDirectory(), texturePath),
//                             Path.Combine(Directory.GetCurrentDirectory(), "Resources", texturePath)
//                         };

//                         foreach (string searchPath in searchPaths)
//                         {
//                             if (File.Exists(searchPath))
//                             {
//                                 texturePath = searchPath;
//                                 break;
//                             }
//                         }
//                     }

//                     if (File.Exists(texturePath))
//                     {
//                         // Load image and create texture using simpler API
//                         var imageBytes = File.ReadAllBytes(texturePath);
//                         var imageBuilder = ImageBuilder.From(imageBytes, Path.GetFileName(texturePath));

//                         // Set diffuse texture - using the basic API
//                         gltfMaterial.WithChannelImage(KnownChannel.BaseColor, imageBuilder);
//                     }
//                     else
//                     {
//                         // Fallback to color if texture file not found
//                         gltfMaterial.WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, baseColor);
//                     }
//                 }
//                 catch
//                 {
//                     // Fallback to color if texture loading fails
//                     gltfMaterial.WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, baseColor);
//                 }
//             }
//             else
//             {
//                 // No texture, just use color
//                 gltfMaterial.WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, baseColor);
//             }

//             gltfMaterials[koreMaterial.Name] = gltfMaterial;
//         }

//         return gltfMaterials;
//     }

//     /// Converts KoreMeshData to glTF mesh format with coordinate system conversion
//     private static void ConvertKoreMeshToGltf(
//         KoreMeshData koreMesh,
//         MeshBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty> meshBuilder,
//         Dictionary<string, MaterialBuilder> materials)
//     {
//         // Group triangles by material
//         var trianglesByMaterial = GroupTrianglesByMaterial(koreMesh);

//         foreach (var materialGroup in trianglesByMaterial)
//         {
//             var materialName = materialGroup.Key;
//             var triangleIndices = materialGroup.Value;

//             // Get or create material
//             MaterialBuilder? gltfMaterial = null;
//             if (!string.IsNullOrEmpty(materialName) && materials.ContainsKey(materialName))
//             {
//                 gltfMaterial = materials[materialName];
//             }

//             // Create primitive for this material
//             var primitive = meshBuilder.UsePrimitive(gltfMaterial);

//             // Add triangles for this material
//             foreach (var triangleIndex in triangleIndices)
//             {
//                 var triangle = koreMesh.Triangles[triangleIndex];

//                 // Get vertices and convert coordinates
//                 var v1 = ConvertToGltfVertex(triangle.A, koreMesh);
//                 var v2 = ConvertToGltfVertex(triangle.B, koreMesh);
//                 var v3 = ConvertToGltfVertex(triangle.C, koreMesh);

//                 // Add triangle with CCW winding (converted from KoreMeshData CW)
//                 primitive.AddTriangle(v1, v2, v3);
//             }
//         }
//     }

//     /// <summary>
//     /// Groups triangles by their assigned material
//     /// </summary>
//     private static Dictionary<string, List<int>> GroupTrianglesByMaterial(KoreMeshData mesh)
//     {
//         var groups = new Dictionary<string, List<int>>();

//         // First, handle triangles assigned to named groups
//         foreach (var namedGroup in mesh.NamedTriangleGroups)
//         {
//             var materialName = namedGroup.Value.MaterialName;
//             if (!groups.ContainsKey(materialName))
//             {
//                 groups[materialName] = new List<int>();
//             }
//             groups[materialName].AddRange(namedGroup.Value.TriangleIds);
//         }

//         // Handle remaining triangles (assign to default material if any)
//         var assignedTriangles = new HashSet<int>();
//         foreach (var group in groups.Values)
//         {
//             foreach (var triangleIndex in group)
//             {
//                 assignedTriangles.Add(triangleIndex);
//             }
//         }

//         var unassignedTriangles = new List<int>();
//         for (int i = 0; i < mesh.Triangles.Count; i++)
//         {
//             if (!assignedTriangles.Contains(i))
//             {
//                 unassignedTriangles.Add(i);
//             }
//         }

//         if (unassignedTriangles.Count > 0)
//         {
//             var defaultMaterialName = mesh.Materials.Count > 0 ? mesh.Materials[0].Name : "DefaultMaterial";
//             if (!groups.ContainsKey(defaultMaterialName))
//             {
//                 groups[defaultMaterialName] = new List<int>();
//             }
//             groups[defaultMaterialName].AddRange(unassignedTriangles);
//         }

//         return groups;
//     }

//     /// <summary>
//     /// Converts a KoreMesh vertex to glTF vertex format with coordinate system conversion
//     /// </summary>
//     private static (VertexPositionNormal, VertexTexture1) ConvertToGltfVertex(int vertexId, KoreMeshData mesh)
//     {
//         // Get vertex data
//         var position = mesh.Vertices[vertexId];
//         var normal   = mesh.Normals.ContainsKey(vertexId) ? mesh.Normals[vertexId] : new KoreXYZVector(0, 1, 0);
//         var uv       = mesh.UVs.ContainsKey(vertexId) ? mesh.UVs[vertexId] : new KoreXYVector(0, 0);

//         // Convert from Kore coordinate system to glTF (both Y-up)
//         var gltfPosition = KoreMeshGltfConv.PositionKoreToGltf(position);
//         var gltfNormal   = KoreMeshGltfConv.NormalKoreToGltf(normal);

//         // UV coordinates:
//         // - KoreMeshData uses top-left origin
//         // - glTF uses bottom left.
//         // - flip Y
//         // var gltfTexCoord = new Vector2((float)uv.X, 1 - (float)uv.Y);
//         var gltfUV = KoreMeshGltfConv.UVKoreToGltf(uv);

//         return (new VertexPositionNormal(gltfPosition, gltfNormal), new VertexTexture1(gltfUV));
//     }



//     // --------------------------------------------------------------------------------------------
//     // MARK: Import
//     // --------------------------------------------------------------------------------------------

//     /// <summary>
//     /// Converts glTF mesh to KoreMeshData format
//     /// </summary>
//     private static void ConvertGltfToKoreMesh(SharpGLTF.Schema2.Mesh gltfMesh, KoreMeshData koreMesh)
//     {
//         int primitiveIndex = 0;

//         foreach (var primitive in gltfMesh.Primitives)
//         {
//             // Extract vertex data
//             var positions = primitive.GetVertexAccessor("POSITION")?.AsVector3Array();
//             var normals   = primitive.GetVertexAccessor("NORMAL")?.AsVector3Array();
//             var texCoords = primitive.GetVertexAccessor("TEXCOORD_0")?.AsVector2Array();
//             var indices   = primitive.GetIndices();

//             if (positions == null || indices == null)
//             {
//                 continue; // Skip primitive without required data
//             }

//             // Convert vertices
//             var vertexIndexMap = new Dictionary<int, int>();
//             for (int i = 0; i < positions.Count; i++)
//             {
//                 var position = KoreMeshGltfConv.PositionGltfToKore(positions[i]);

//                 // Convert normal if available
//                 KoreXYZVector? normal = null;
//                 if (normals != null && i < normals.Count)
//                 {
//                     normal = KoreMeshGltfConv.NormalGltfToKore(normals[i]);
//                 }

//                 // UV coordinates:
//                 // - KoreMeshData uses top-left origin
//                 // - glTF uses bottom left.
//                 // - flip Y
//                 // var uv = texCoords != null && i < texCoords.Count ?
//                 //     new KoreXYVector(texCoords[i].X, 1 - texCoords[i].Y) :
//                 //     (KoreXYVector?)null;

//                 Vector2 gltfUV = new Vector2(
//                     texCoords != null && i < texCoords.Count ? texCoords[i].X : 0,
//                     texCoords != null && i < texCoords.Count ? texCoords[i].Y : 0);

//                 var koreUV = KoreMeshGltfConv.UVGltfToKore(gltfUV);

//                 var vertexIndex = koreMesh.AddCompleteVertex(position, normal, null, koreUV);
//                 vertexIndexMap[i] = vertexIndex;
//             }

//             // Track triangles added for this primitive (for NamedTriangleGroup creation)
//             var primitiveTriangleIds = new List<int>();

//             // Convert triangles (preserve CCW winding)
//             for (int i = 0; i < indices.Count; i += 3)
//             {
//                 var v1 = vertexIndexMap[(int)indices[i]];
//                 var v2 = vertexIndexMap[(int)indices[i + 1]];
//                 var v3 = vertexIndexMap[(int)indices[i + 2]];

//                 // Import triangle with original winding (no flip needed)
//                 var triangleId = koreMesh.AddTriangle(v1, v2, v3);
//                 primitiveTriangleIds.Add(triangleId);
//             }

//             // Handle material with texture support
//             string materialName = "DefaultMaterial";
//             if (primitive.Material != null)
//             {
//                 var gltfMaterial = primitive.Material;
//                 materialName = gltfMaterial.Name ?? $"ImportedMaterial_{primitiveIndex}";

//                 // Extract base color
//                 var baseColorFactor = gltfMaterial.FindChannel("BaseColor")?.Color ?? Vector4.One;
//                 var baseColor = new KoreColorRGB(
//                     (int)(baseColorFactor.X * 255),
//                     (int)(baseColorFactor.Y * 255),
//                     (int)(baseColorFactor.Z * 255)
//                 );

//                 // Extract texture filename if present
//                 string? textureFilename = null;
//                 var baseColorTexture = gltfMaterial.FindChannel("BaseColor")?.Texture;
//                 if (baseColorTexture != null)
//                 {
//                     // Extract the texture filename from the glTF texture
//                     var image = baseColorTexture.PrimaryImage;
//                     if (image != null)
//                     {
//                         if (!string.IsNullOrEmpty(image.Name))
//                         {
//                             textureFilename = image.Name;
//                         }
//                         else
//                         {
//                             // For our oil barrel test case, we know the texture should be the UV layout
//                             textureFilename = "oil_barrel_uv_clean.png";
//                         }
//                     }
//                 }

//                 // Create material using the appropriate factory method
//                 KoreMeshMaterial material;
//                 if (!string.IsNullOrEmpty(textureFilename))
//                 {
//                     // Use FromTexture for materials with textures
//                     material = KoreMeshMaterial.FromTexture(materialName, textureFilename, baseColor);
//                 }
//                 else
//                 {
//                     // Use FromColor for color-only materials
//                     material = KoreMeshMaterial.FromColor(materialName, baseColor);
//                 }

//                 koreMesh.AddMaterial(material);
//             }

//             // Create NamedTriangleGroup for this primitive's triangles
//             if (primitiveTriangleIds.Count > 0)
//             {
//                 var groupName = $"Group_{materialName}";
//                 var triangleGroup = new KoreMeshTriangleGroup(materialName, primitiveTriangleIds);
//                 koreMesh.NamedTriangleGroups[groupName] = triangleGroup;
//             }

//             primitiveIndex++;
//         }
//     }


// }
