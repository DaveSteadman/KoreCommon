// <fileheader>

using System;
using System.IO;
using KoreCommon;
using KoreCommon.SkiaSharp;

namespace KoreCommon.UnitTest;

public static partial class KoreTestMeshUvOps
{
    public static void RunTests(KoreTestLog testLog)
    {
        testLog.AddComment("=== Testing KoreMeshDataUvOps ===");

        TestDiceCubeUVLayout(testLog);
        TestSimpleQuadUVs(testLog);
        TestOilBarrelUVLayout(testLog);
    }

    // Creates a cube with UV layout like an unfolded dice
    // Layout:
    //     [2]
    // [4] [1] [3] [6]
    //     [5]
    // Where numbers represent the dice faces
    private static void TestDiceCubeUVLayout(KoreTestLog testLog)
    {
        var mesh = CreateDiceCube();

        // Save debug version with vertex numbers
        string debugPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "dice_cube_uv_debug.png");
        KoreFileOps.CreateDirectoryForFile(debugPath);
        KoreMeshDataUvOps.SaveUVLayout(mesh, debugPath, 2048, true, true);

        // Save clean version without vertex numbers
        string cleanPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "dice_cube_uv_clean.png");
        KoreMeshDataUvOps.SaveUVLayout(mesh, cleanPath, 1024, false, true);

        testLog.AddResult("Dice cube UV layout", true,
            $"Created dice cube with {mesh.Vertices.Count} vertices, {mesh.Triangles.Count} triangles");
        testLog.AddComment($"Debug image: {debugPath}");
        testLog.AddComment($"Clean image: {cleanPath}");

        // Assign the UV layout image as texture to all materials for visual verification
        //AssignUVLayoutAsTexture(mesh, "dice_cube_uv_clean.png");

        {
            KoreMeshMaterial m = mesh.GetMaterial("DiceCubeFaces");
            m.Filename = "dice_cube_uv_clean2.png";
            mesh.AddMaterial(m);
        }

        var (objContent, mtlContent) = KoreMeshDataIO.ToObjMtl(mesh, "TestUVCube", "TestUVCubeMats");
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "TestUVCube.obj"), objContent);
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "TestUVCubeMats.mtl"), mtlContent);
        testLog.AddComment("OBJ/MTL files created for dice cube UV layout with UV texture assignment");
    }

    private static void TestSimpleQuadUVs(KoreTestLog testLog)
    {
        // Create a simple quad for basic UV testing
        var mesh = new KoreMeshData();

        // Add vertices forming a square with UVs covering the full 0-1 space
        int v1 = mesh.AddCompleteVertex(new KoreXYZVector(-1, -1, 0), null, null, new KoreXYVector(0, 0)); // Bottom-left
        int v2 = mesh.AddCompleteVertex(new KoreXYZVector( 1, -1, 0), null, null, new KoreXYVector(1, 0)); // Bottom-right
        int v3 = mesh.AddCompleteVertex(new KoreXYZVector( 1,  1, 0), null, null, new KoreXYVector(1, 1)); // Top-right
        int v4 = mesh.AddCompleteVertex(new KoreXYZVector(-1,  1, 0), null, null, new KoreXYVector(0, 1)); // Top-left

        // Add two triangles to form the quad
        mesh.AddTriangle(v1, v2, v3); // Bottom-right triangle
        mesh.AddTriangle(v1, v3, v4); // Top-left triangle

        string filePath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "simple_quad_uv.png");
        KoreFileOps.CreateDirectoryForFile(filePath);
        KoreMeshDataUvOps.SaveUVLayout(mesh, filePath, 512, true, true);

        testLog.AddResult("Simple quad UV layout", true, $"Created simple quad with full UV coverage");
    }

    // Assigns the UV layout image as texture to all materials in the mesh.
    // This allows visual verification of UV mapping when the OBJ is opened in Blender.
    // private static void AssignUVLayoutAsTexture(KoreMeshData mesh, string uvImageFilename)
    // {
    //     // Update existing materials or create default material if none exist
    //     if (mesh.Materials.Count == 0)
    //     {
    //         // Create a default material with the UV texture
    //         var defaultMaterial = KoreMeshMaterial.FromTexture("DefaultUVMaterial", uvImageFilename, KoreColorRGB.White);
    //         mesh.AddMaterial(defaultMaterial);
    //     }
    //     else
    //     {
    //         // Update all existing materials to use the UV layout as texture
    //         var updatedMaterials = new List<KoreMeshMaterial>();

    //         foreach (var material in mesh.Materials)
    //         {
    //             var updatedMaterial = new KoreMeshMaterial(
    //                 material.Name,
    //                 material.BaseColor,
    //                 material.Metallic,
    //                 material.Roughness,
    //                 uvImageFilename  // Assign UV image as texture
    //             );
    //             updatedMaterials.Add(updatedMaterial);
    //         }

    //         // Replace materials in mesh
    //         mesh.Materials.Clear();
    //         foreach (var material in updatedMaterials)
    //         {
    //             mesh.AddMaterial(material);
    //         }
    //     }
    // }

    private static KoreMeshData CreateDiceCube()
    {
        var mesh = new KoreMeshData();

        // Define the UV layout for a dice unfolded:
        //     [2]
        // [4] [1] [3] [6]
        //     [5]

        // Each face is 0.25 x 0.25 in UV space, so half face size is 0.125
        double faceSize = 0.25;
        double halfFaceSize = faceSize / 2.0; // 0.125

        // UV positions for each face center - positioned at multiples of halfFaceSize
        // Centers range from 1*halfFaceSize to 7*halfFaceSize (0.125 to 0.875)
        var faceUVs = new[]
        {
            new { centerU = 3 * halfFaceSize, centerV = 4 * halfFaceSize }, // Face 1 (front)  - 0.25, 0.50
            new { centerU = 3 * halfFaceSize, centerV = 6 * halfFaceSize }, // Face 2 (top)   - 0.25, 0.75
            new { centerU = 5 * halfFaceSize, centerV = 4 * halfFaceSize }, // Face 3 (right) - 0.50, 0.50
            new { centerU = 1 * halfFaceSize, centerV = 4 * halfFaceSize }, // Face 4 (left)  - 0.125, 0.50
            new { centerU = 3 * halfFaceSize, centerV = 2 * halfFaceSize }, // Face 5 (bottom)- 0.25, 0.25
            new { centerU = 7 * halfFaceSize, centerV = 4 * halfFaceSize }  // Face 6 (back)  - 0.75, 0.50
        };

        // Create vertices for each face
        var faceVertices = new int[6, 4]; // 6 faces, 4 vertices each

        for (int face = 0; face < 6; face++)
        {
            var uvCenter = faceUVs[face];

            // Calculate UV coordinates for the four corners of this face
            double u1 = uvCenter.centerU - faceSize / 2;
            double u2 = uvCenter.centerU + faceSize / 2;
            double v1 = uvCenter.centerV - faceSize / 2;
            double v2 = uvCenter.centerV + faceSize / 2;

            // Create 3D positions for the cube face
            var positions = GetCubeFacePositions(face);

            // Add vertices: bottom-left, bottom-right, top-right, top-left
            faceVertices[face, 0] = mesh.AddCompleteVertex(positions[0], null, null, new KoreXYVector(u1, v1));
            faceVertices[face, 1] = mesh.AddCompleteVertex(positions[1], null, null, new KoreXYVector(u2, v1));
            faceVertices[face, 2] = mesh.AddCompleteVertex(positions[2], null, null, new KoreXYVector(u2, v2));
            faceVertices[face, 3] = mesh.AddCompleteVertex(positions[3], null, null, new KoreXYVector(u1, v2));

            // Add two triangles for this face
            int vert0 = faceVertices[face, 0];
            int vert1 = faceVertices[face, 1];
            int vert2 = faceVertices[face, 2];
            int vert3 = faceVertices[face, 3];

            mesh.AddTriangle(vert0, vert1, vert2); // Bottom-right triangle
            mesh.AddTriangle(vert0, vert2, vert3); // Top-left triangle
        }

        // create a named group and add all triangles to it.
        KoreMeshMaterial m = new KoreMeshMaterial("DiceCubeFaces", new KoreColorRGB(1, 0, 0));
        mesh.AddGroupWithMaterial("DiceCubeFaces", m);
        mesh.AddAllTrianglesToGroup("DiceCubeFaces");

        return mesh;
    }

    private static KoreXYZVector[] GetCubeFacePositions(int face)
    {
        // Return the 3D positions for the four corners of each cube face
        // Each face returns: bottom-left, bottom-right, top-right, top-left

        return face switch
        {
            0 => [ // Front face (Z+)
                new KoreXYZVector(-1, -1,  1),
                new KoreXYZVector( 1, -1,  1),
                new KoreXYZVector( 1,  1,  1),
                new KoreXYZVector(-1,  1,  1)
            ],
            1 => [ // Top face (Y+)
                new KoreXYZVector(-1,  1, -1),
                new KoreXYZVector( 1,  1, -1),
                new KoreXYZVector( 1,  1,  1),
                new KoreXYZVector(-1,  1,  1)
            ],
            2 => [ // Right face (X+)
                new KoreXYZVector( 1, -1,  1),
                new KoreXYZVector( 1, -1, -1),
                new KoreXYZVector( 1,  1, -1),
                new KoreXYZVector( 1,  1,  1)
            ],
            3 => [ // Left face (X-)
                new KoreXYZVector(-1, -1, -1),
                new KoreXYZVector(-1, -1,  1),
                new KoreXYZVector(-1,  1,  1),
                new KoreXYZVector(-1,  1, -1)
            ],
            4 => [ // Bottom face (Y-)
                new KoreXYZVector(-1, -1,  1),
                new KoreXYZVector( 1, -1,  1),
                new KoreXYZVector( 1, -1, -1),
                new KoreXYZVector(-1, -1, -1)
            ],
            5 => [ // Back face (Z-)
                new KoreXYZVector( 1, -1, -1),
                new KoreXYZVector(-1, -1, -1),
                new KoreXYZVector(-1,  1, -1),
                new KoreXYZVector( 1,  1, -1)
            ],
            _ => throw new ArgumentException($"Invalid face index: {face}")
        };
    }
}
