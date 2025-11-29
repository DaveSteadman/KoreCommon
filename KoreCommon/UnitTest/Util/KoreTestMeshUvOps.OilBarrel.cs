// <fileheader>

using System;
using System.Collections.Generic;
using System.IO;
using KoreCommon;
using KoreCommon.SkiaSharp;

namespace KoreCommon.UnitTest;

public static partial class KoreTestMeshUvOps
{
    // Creates an oil barrel mesh with proper UV mapping for texture application.
    // UV Layout:
    // - Top circle: Center at (0.25, 0.75) with 0.20 radius
    // - Bottom circle: Center at (0.75, 0.75) with 0.20 radius
    // - Cylinder sides: Rectangle in lower half (0.0-1.0, 0.0-0.5)

    // segments: Number of segments around the cylinder (default 16)
    // radius: Radius of the barrel (default 1.0)
    // height: Height of the barrel (default 2.0)
    // returns: KoreMeshData representing the oil barrel
    public static KoreMeshData CreateOilBarrelWithUV(int segments = 16, double radius = 1.0, double height = 3.0)
    {
        var mesh = new KoreMeshData();

        // UV layout parameters: Square image, 2 circles in top half, 2:1 barrel wrap in the bottom half
        // UV 0,0 is bottom left (Godot/OpenGL native), u is x, v is y
        var topCenterU = 0.25;
        var topCenterV = 0.25;  // Flipped: was 0.25, now 0.75 (top of image in bottom-left coordinate system)
        var bottomCenterU = 0.75;
        var bottomCenterV = 0.25; // Flipped: was 0.25, now 0.75 (top of image in bottom-left coordinate system)
        var uvRadius = 0.24;  // Slightly smaller than 0.25 to avoid edge bleeding

        //var wrapTLU = 0;
        var wrapTLV = 0.5;
        //var wrapBRU = 1;
        var wrapBRV = 1;

        // Create ring vertices for top and bottom caps
        var topRingVertices = new int[segments];
        var bottomRingVertices = new int[segments];

        // So we want a decreasing list (clockwise progression for proper texture orientation in Z- forward system)
        List<double> anglesRads = KoreValueUtils.CreateRangeList(segments, 0, -Math.PI * 2);

        // --- TOP ---

        // Create center vertices for top and bottom caps
        int topCenterVertex = mesh.AddCompleteVertex(
            new KoreXYZVector(0, height / 2, 0),
            null, null,
            new KoreXYVector(topCenterU, topCenterV)
        );

        for (int i = 0; i < segments; i++)
        {
            // So the angle starts at 0 - where Cos increases X and Z starts at 0 and increases.
            // So from the top down, we're at 3, going CCW.
            double x = Math.Cos(anglesRads[i]) * radius;
            double z = Math.Sin(anglesRads[i]) * radius;

            // UV coordinates for circular caps
            double topU = topCenterU + Math.Cos(anglesRads[i]) * uvRadius;
            double topV = topCenterV + Math.Sin(anglesRads[i]) * uvRadius; // + = go up in bottom-left coordinate system

            // Top ring vertex - CCW from above
            topRingVertices[i] = mesh.AddCompleteVertex(
                new KoreXYZVector(x, height / 2, z),
                null, null,
                new KoreXYVector(topU, topV)
            );
        }

        // Create triangles for top cap (fan pattern - CCW winding)
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            mesh.AddTriangle(topCenterVertex, topRingVertices[next], topRingVertices[i]);
        }


        // --- BOTTOM ---

        int bottomCenterVertex = mesh.AddCompleteVertex(
            new KoreXYZVector(0, -height / 2, 0),
            null, null,
            new KoreXYVector(bottomCenterU, bottomCenterV)
        );
        for (int i = 0; i < segments; i++)
        {
            // So the angle starts at 0 - where Cos increases X and Z starts at 0 and increases.
            // So from the top down, we're at 3, going CCW.
            double x = Math.Cos(anglesRads[i]) * radius;
            double z = Math.Sin(anglesRads[i]) * radius;

            // UV coordinates for circular caps

            double bottomU = bottomCenterU + Math.Cos(anglesRads[i]) * uvRadius;
            double bottomV = bottomCenterV - Math.Sin(anglesRads[i]) * uvRadius; // Bottom cap viewed from below, so invert V

            // Bottom ring vertex - CW from below
            bottomRingVertices[i] = mesh.AddCompleteVertex(
                new KoreXYZVector(x, -height / 2, z),
                null, null,
                new KoreXYVector(bottomU, bottomV)
            );
        }

        // Create triangles for bottom cap (fan pattern - CCW winding)
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            mesh.AddTriangle(bottomCenterVertex, bottomRingVertices[i], bottomRingVertices[next]);
        }

        // --- WRAP ---

        // Create cylinder side vertices (duplicate positions but different UVs)
        var sideTopVertices = new int[segments]; // +1 for closing the seam
        var sideBottomVertices = new int[segments]; // +1 for closing the seam

        List<double> textureU = KoreValueUtils.CreateRangeList(segments, 0, 1);

        for (int i = 0; i < segments; i++) // <= to include the closing vertex
        {
            int angleindex = i % segments;
            //double angle = (double)(i % segments) / segments * Math.PI * 2; // Wrap the angle for last vertex
            double x = Math.Cos(anglesRads[angleindex]) * radius;
            double z = Math.Sin(anglesRads[angleindex]) * radius;

            // UV coordinates for cylinder sides - wrap horizontally, span vertically
            double u = textureU[i];

            //double u = (double)i / segments;              // 0..1
            //if (i == segments) u = 0.0;

            // Side vertices (same 3D position as ring vertices but different UV)
            sideTopVertices[i] = mesh.AddCompleteVertex(
                new KoreXYZVector(x, height / 2, z),
                null, null,
                new KoreXYVector(u, wrapTLV) // Top of cylinder rectangle
            );

            sideBottomVertices[i] = mesh.AddCompleteVertex(
                new KoreXYZVector(x, -height / 2, z),
                null, null,
                new KoreXYVector(u, wrapBRV) // Bottom of cylinder rectangle
            );
        }



        // Create triangles for cylinder sides (CCW winding)
        for (int i = 0; i < segments; i++)
        {
            int next = i + 1; // No modulo needed since we have segments+1 vertices

            next = next % segments; // Wrap around to first vertex
            // // Two triangles per side segment (CCW winding)
            // mesh.AddTriangle(sideBottomVertices[i], sideTopVertices[next], sideTopVertices[i]);
            // mesh.AddTriangle(sideBottomVertices[i], sideBottomVertices[next], sideTopVertices[next]);


            // replace your two AddTriangle calls in the WRAP loop with:
            mesh.AddTriangle(sideTopVertices[i], sideTopVertices[next], sideBottomVertices[next]); // upper-left tri
            mesh.AddTriangle(sideTopVertices[i], sideBottomVertices[next], sideBottomVertices[i]);    // lower-right tri

        }

        // Add material with texture
        var barrelMaterial = new KoreMeshMaterial("OilBarrelMaterial", new KoreColorRGB(200, 10, 10));
        barrelMaterial.Filename = "oil_barrel_texture2.png"; // Use existing test image
        mesh.AddMaterial(barrelMaterial);

        // Create named triangle group for all barrel geometry
        var allTriangleIds = new List<int>();
        for (int i = 0; i < mesh.Triangles.Count; i++)
        {
            allTriangleIds.Add(i);
        }

        var triangleGroup = new KoreMeshTriangleGroup("OilBarrelMaterial", allTriangleIds);
        mesh.NamedTriangleGroups["OilBarrel"] = triangleGroup;

        // Set the normals to a simple flat shading from the triangles
        KoreMeshDataEditOps.SetNormalsFromTriangles(mesh);
        // mesh.FlipAllNormals();
        // mesh.FlipAllTriangleWindings();

        return mesh;
    }

    // Test method for creating and visualizing an oil barrel with UV mapping
    public static void TestOilBarrelUVLayout(KoreTestLog testLog)
    {
        var mesh = CreateOilBarrelWithUV(16, 1.0, 3.0);

        // Save UV layout images
        string debugPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "oil_barrel_uv_debug.png");
        string cleanPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "oil_barrel_uv_clean.png");

        KoreFileOps.CreateDirectoryForFile(debugPath);
        KoreMeshDataUvOps.SaveUVLayout(mesh, debugPath, 2048, true, true);
        KoreMeshDataUvOps.SaveUVLayout(mesh, cleanPath, 1024, false, true);

        testLog.AddResult("Oil barrel UV layout", true,
            $"Created oil barrel with {mesh.Vertices.Count} vertices, {mesh.Triangles.Count} triangles");
        testLog.AddComment($"Debug image: {debugPath}");
        testLog.AddComment($"Clean image: {cleanPath}");

        // Assign UV texture to material for visual verification
        var material = mesh.GetMaterial("OilBarrelMaterial");
        // if (material != null)
        // {
        var updatedMaterial = new KoreMeshMaterial(
            material.Name,
            material.BaseColor,
            material.Metallic,
            material.Roughness,
            "oil_barrel_texture2.png"
        );
        mesh.AddMaterial(updatedMaterial);
        // }

        // Export OBJ/MTL files
        var (objContent, mtlContent) = KoreMeshDataIO.ToObjMtl(mesh, "TestOilBarrel", "TestOilBarrelMats");
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "TestOilBarrel.obj"), objContent);
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "TestOilBarrelMats.mtl"), mtlContent);
        testLog.AddComment("OBJ/MTL files created for oil barrel UV layout with UV texture assignment");

        // Export glTF file
        // try
        // {
        //     KoreMeshDataGltfIO.SaveToGltf(mesh, "UnitTestArtefacts/TestOilBarrel.gltf", "OilBarrel");
        //     testLog.AddComment("glTF file created: UnitTestArtefacts/TestOilBarrel.gltf");
        //     testLog.AddResult("glTF export", true, "Successfully exported oil barrel to glTF format");
        // }
        // catch (Exception ex)
        // {
        //     testLog.AddResult("glTF export", false, $"glTF export failed: {ex.Message}");
        // }

        // Export the JSON representation
        string jsonPath = KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "TestOilBarrel.json");
        KoreFileOps.CreateDirectoryForFile(jsonPath);
        File.WriteAllText(jsonPath, KoreMeshDataIO.ToJson(mesh));

        testLog.AddComment($"JSON file created: {jsonPath}");
    }
}
