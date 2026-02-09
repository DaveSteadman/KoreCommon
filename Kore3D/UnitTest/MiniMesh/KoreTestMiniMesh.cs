// <fileheader>

using System;
using System.IO;
using KoreCommon;
using KoreCommon.SkiaSharp;

namespace KoreCommon.UnitTest;

public static partial class KoreTestMiniMesh
{
    public static void RunTests(KoreTestLog testLog)
    {
        testLog.AddComment("=== Testing KoreTestMiniMesh ===");

        TestSimpleJSON(testLog);
        TestSphere(testLog);
        TestCylinder(testLog);
        TestPyramid(testLog);
    }


    private static void TestSimpleJSON(KoreTestLog testLog)
    {
        // Create a cube with the primitives class
        KoreMiniMesh cubeMesh = KoreMiniMeshPrimitives.BasicCube(1.0f, KoreMiniMeshMaterialPalette.Find("MattOrange"), new KoreColorRGB(255, 255, 255));
        string json = KoreMiniMeshIO.ToJson(cubeMesh);
        testLog.AddComment($"Cube JSON: {json}");

        var loadedCube = KoreMiniMeshIO.FromJson(json);
        testLog.AddComment($"Loaded Cube: {loadedCube}");


        // Save to Obj/MTL
        var (objContent, mtlContent) = KoreMiniMeshIO.ToObjMtl(cubeMesh, "MyMesh", "MyMaterials");
        testLog.AddComment($"OBJ Content: {objContent}");
        testLog.AddComment($"MTL Content: {mtlContent}");

        // Save OBJMtl content to file
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "MyMesh.obj"), objContent);
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "MyMaterials.mtl"), mtlContent);

    }

    private static void TestSphere(KoreTestLog testLog)
    {
        testLog.AddComment("=== Testing Sphere Primitives ===");

        // Create both sphere types for comparison
        KoreXYZVector center = new KoreXYZVector(0, 0, 0);
        double radius = 1.0;
        int latSegments = 8; // Low res for testing
        var material = KoreMiniMeshMaterialPalette.Find("BlueShiny");
        var lineColor = new KoreColorRGB(255, 255, 0); // Yellow wireframe

        // Test BasicSphere
        testLog.AddComment("--- Basic Sphere ---");
        KoreMiniMesh basicSphere = KoreMiniMeshPrimitives.BasicSphere(center, radius, latSegments, material, lineColor);

        testLog.AddComment($"Basic Sphere Vertices: {basicSphere.Vertices.Count}");
        testLog.AddComment($"Basic Sphere Groups: {basicSphere.Groups.Count}");
        testLog.AddComment($"Basic Sphere Materials: {basicSphere.Materials.Count}");
        testLog.AddComment($"Basic Sphere Lines: {basicSphere.Lines.Count}");

        // Test JSON serialization for both
        string basicJson = KoreMiniMeshIO.ToJson(basicSphere);

        var loadedBasic = KoreMiniMeshIO.FromJson(basicJson);

        testLog.AddResult("Basic Sphere JSON roundtrip vertices", loadedBasic.Vertices.Count == basicSphere.Vertices.Count);

        // Save both to OBJ/MTL for visual comparison
        var (basicObjContent, basicMtlContent) = KoreMiniMeshIO.ToObjMtl(basicSphere, "BasicSphere", "BasicSphereMaterials");
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "BasicSphere.obj"), basicObjContent);
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "BasicSphereMaterials.mtl"), basicMtlContent);

        testLog.AddComment($"Sphere comparison completed - check {KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "BasicSphere.obj")} vs OptimizedSphere.obj");

    }

    private static void TestCylinder(KoreTestLog testLog)
    {
        testLog.AddComment("=== Testing Cylinder Primitive ===");

        // Create a cylinder
        KoreXYZVector p1 = new KoreXYZVector(0, -1, 0);
        KoreXYZVector p2 = new KoreXYZVector(0, 1, 0);
        double radius = 0.5;
        int sides = 8; // Low res for testing
        var material = KoreMiniMeshMaterialPalette.Find("MattGreen");
        var lineColor = new KoreColorRGB(255, 255, 0); // Yellow wireframe

        KoreMiniMesh cylinderMesh = KoreMiniMeshPrimitives.CreateCylinder(
            p1, p2, radius, radius, sides, true, material, lineColor);

        testLog.AddComment($"Cylinder Vertices: {cylinderMesh.Vertices.Count}");
        testLog.AddComment($"Cylinder Groups: {cylinderMesh.Groups.Count}");
        testLog.AddComment($"Cylinder Materials: {cylinderMesh.Materials.Count}");
        testLog.AddComment($"Cylinder Lines: {cylinderMesh.Lines.Count}");
        testLog.AddComment($"Cylinder Triangles: {cylinderMesh.Triangles.Count}");

        // Test JSON serialization
        string json = KoreMiniMeshIO.ToJson(cylinderMesh);
        testLog.AddComment($"Cylinder JSON length: {json.Length}");

        var loadedCylinder = KoreMiniMeshIO.FromJson(json);
        testLog.AddResult("Cylinder JSON roundtrip vertices", loadedCylinder.Vertices.Count == cylinderMesh.Vertices.Count);
        testLog.AddResult("Cylinder JSON roundtrip groups", loadedCylinder.Groups.Count == cylinderMesh.Groups.Count);

        // Save to OBJ/MTL for visual verification
        var (objContent, mtlContent) = KoreMiniMeshIO.ToObjMtl(cylinderMesh, "TestCylinder", "CylinderMaterials");
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "TestCylinder.obj"), objContent);
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "CylinderMaterials.mtl"), mtlContent);

        testLog.AddComment($"Cylinder test completed - check {KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "TestCylinder.obj")}");

    }

    private static void TestPyramid(KoreTestLog testLog)
    {
        testLog.AddComment("Testing KoreMiniMeshPrimitives.CreatePyramid");

        // Create test pyramid with tilted axis
        KoreXYZVector pApex = new KoreXYZVector(1, 2, 0.5);      // Tilted apex
        KoreXYZVector pBaseCenter = new KoreXYZVector(0, 0, 0);  // Base center at origin
        KoreXYZVector baseReference = new KoreXYZVector(1, 0, 1);  // Reference direction for base orientation
        double width = 0.8;
        double height = 1.2;

        var pyramidMesh = KoreMiniMeshPrimitives.CreatePyramid(
            pApex, pBaseCenter, baseReference, width, height, true,
            KoreMiniMeshMaterialPalette.Find("MattGreen"),
            new KoreColorRGB(0, 0, 255));

        // Verify basic structure
        testLog.AddComment($"Pyramid created with {pyramidMesh.Vertices.Count} vertices and {pyramidMesh.Triangles.Count} triangles");

        // Test JSON serialization
        string json = KoreMiniMeshIO.ToJson(pyramidMesh);
        testLog.AddResult("JSON serialization", !string.IsNullOrEmpty(json));

        // Test OBJ/MTL export
        var (objContent, mtlContent) = KoreMiniMeshIO.ToObjMtl(pyramidMesh, "TestPyramid", "PyramidMaterials");
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "TestPyramid.obj"), objContent);
        File.WriteAllText(KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "PyramidMaterials.mtl"), mtlContent);

        testLog.AddComment($"Pyramid test completed - check {KoreFileOps.JoinPaths(KoreTestCenter.TestPath, "TestPyramid.obj")}");
    }

}
