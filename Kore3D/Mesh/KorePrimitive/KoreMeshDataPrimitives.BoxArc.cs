// <fileheader>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace KoreCommon;

// Static class to create KoreMeshData primitives
// This class is used to create various 3D shapes and meshes in Godot

public static partial class KoreMeshDataPrimitives
{

    // Box Arc: A box section describing an arc.
    // - inner and outer radius.
    // - vertical angle of the section - half applied above and below the horizontal plane.
    // - horizontal start of the arc and an angle delta (wrapping issues).
    // - The box section is all four curved sides of the arc, with end caps.
    // - The centre point is 0,0,0

    public static KoreMeshData BoxArc(
        float innerRadius, float outerRadius,
        float vertAngleDegs,
        float hozStartAngleDegs, float hozAngleDeltaDegs)
    {
        var mesh = new KoreMeshData();

        // We create four lists of points, starting at the start angle, and progressing around to the start+delta angle.
        // This way we can later construct lines and triangles from known points.

        int numsteps = (int)(hozAngleDeltaDegs / 2); // steps per degree

        KoreNumeric1DArray<double> pointAngleList = KoreNumeric1DArrayOps<double>.ListForRange(hozStartAngleDegs, hozStartAngleDegs + hozAngleDeltaDegs, numsteps);

        List<KoreXYZVector> pointInnerLowerList = new List<KoreXYZVector>();
        List<KoreXYZVector> pointInnerUpperList = new List<KoreXYZVector>();
        List<KoreXYZVector> pointOuterLowerList = new List<KoreXYZVector>();
        List<KoreXYZVector> pointOuterUpperList = new List<KoreXYZVector>();

        List<KoreXYVector> defaultUVList = new List<KoreXYVector>();
        for (int i = 0; i < numsteps; i++)
            defaultUVList.Add(KoreXYVector.Zero);

        foreach (double pointAngle in pointAngleList)
        {
            KoreXYZPolarOffset polarInnerLowerVect = new KoreXYZPolarOffset() { AzDegs = pointAngle, ElDegs = -vertAngleDegs / 2, Range = innerRadius };
            KoreXYZPolarOffset polarOuterLowerVect = new KoreXYZPolarOffset() { AzDegs = pointAngle, ElDegs = -vertAngleDegs / 2, Range = outerRadius };
            KoreXYZPolarOffset polarInnerUpperVect = new KoreXYZPolarOffset() { AzDegs = pointAngle, ElDegs = vertAngleDegs / 2, Range = innerRadius };
            KoreXYZPolarOffset polarOuterUpperVect = new KoreXYZPolarOffset() { AzDegs = pointAngle, ElDegs = vertAngleDegs / 2, Range = outerRadius };

            pointInnerLowerList.Add(polarInnerLowerVect.ToXYZ());
            pointInnerUpperList.Add(polarInnerUpperVect.ToXYZ());
            pointOuterLowerList.Add(polarOuterLowerVect.ToXYZ());
            pointOuterUpperList.Add(polarOuterUpperVect.ToXYZ());
        }

        KoreMeshData innerRibbon = KoreMeshDataPrimitives.Ribbon(pointInnerUpperList, defaultUVList, pointInnerLowerList, defaultUVList);
        KoreMeshData outerRibbon = KoreMeshDataPrimitives.Ribbon(pointOuterLowerList, defaultUVList, pointOuterUpperList, defaultUVList);
        KoreMeshData upperRibbon = KoreMeshDataPrimitives.Ribbon(pointOuterUpperList, defaultUVList, pointInnerUpperList, defaultUVList);
        KoreMeshData lowerRibbon = KoreMeshDataPrimitives.Ribbon(pointInnerLowerList, defaultUVList, pointOuterLowerList, defaultUVList);

        KoreMeshData combiningMesh = KoreMeshData.BasicAppendMesh(innerRibbon, outerRibbon);
        combiningMesh = KoreMeshData.BasicAppendMesh(combiningMesh, upperRibbon);
        combiningMesh = KoreMeshData.BasicAppendMesh(combiningMesh, lowerRibbon);



        // Now loop through the index of the lists, starting at 1, as we already have the first point in prevloopPointIds.
        // Join the loops of points with lines and triangles.
        // for (int i = 0; i < pointInnerLowerList.Count - 1; i++)
        // {
        //     // Add the next loop of points
        //     currloopPointIds[0] = mesh.AddVertex(new KoreXYZVector(pointInnerLowerList[i]));
        //     currloopPointIds[1] = mesh.AddVertex(new KoreXYZVector(pointOuterLowerList[i]));
        //     currloopPointIds[2] = mesh.AddVertex(new KoreXYZVector(pointOuterUpperList[i]));
        //     currloopPointIds[3] = mesh.AddVertex(new KoreXYZVector(pointInnerUpperList[i]));

        //     // Add lines for the loop of points.
        //     mesh.AddLine(prevloopPointIds[0], currloopPointIds[0]);
        //     mesh.AddLine(prevloopPointIds[1], currloopPointIds[1]);
        //     mesh.AddLine(prevloopPointIds[2], currloopPointIds[2]);
        //     mesh.AddLine(prevloopPointIds[3], currloopPointIds[3]);

        //     // Add lines to join the loops to the previous points.
        //     mesh.AddLine(prevloopPointIds[0], currloopPointIds[1]);
        //     mesh.AddLine(prevloopPointIds[1], currloopPointIds[2]);
        //     mesh.AddLine(prevloopPointIds[2], currloopPointIds[3]);
        //     mesh.AddLine(prevloopPointIds[3], currloopPointIds[0]);

        //     // Add the triangles between loops
        //     // - Outer triangles
        //     mesh.AddTriangle(prevloopPointIds[1], currloopPointIds[1], currloopPointIds[2]);
        //     mesh.AddTriangle(prevloopPointIds[1], currloopPointIds[2], prevloopPointIds[2]);

        //     // - Inner triangles
        //     mesh.AddTriangle(prevloopPointIds[0], currloopPointIds[0], currloopPointIds[3]);
        //     mesh.AddTriangle(prevloopPointIds[0], currloopPointIds[3], prevloopPointIds[3]);

        //     // - Top triangles
        //     mesh.AddTriangle(prevloopPointIds[2], currloopPointIds[2], currloopPointIds[3]);
        //     mesh.AddTriangle(prevloopPointIds[2], currloopPointIds[3], prevloopPointIds[3]);

        //     // - Bottom triangles
        //     mesh.AddTriangle(prevloopPointIds[0], currloopPointIds[0], currloopPointIds[1]);
        //     mesh.AddTriangle(prevloopPointIds[0], currloopPointIds[1], prevloopPointIds[1]);

        //     // move the curr loop to the previous loop for the next iteration
        //     for (int j = 0; j < 4; j++)
        //         prevloopPointIds[j] = currloopPointIds[j];

        // }
        // Update the mesh, to have each point's normal calculated off of the triangles it is part of.
        //mesh.UpdateNormals();

        combiningMesh.AddFace(
            pointOuterUpperList[0],
            pointInnerUpperList[0],
            pointInnerLowerList[0],
            pointOuterLowerList[0]);

        int endId = pointInnerLowerList.Count - 1;
        combiningMesh.AddFace(
            pointInnerUpperList[endId],
            pointOuterUpperList[endId],
            pointOuterLowerList[endId],
            pointInnerLowerList[endId]);


        // // Add the end-caps to the box.
        // int[] startCapPntIds = new int[4];
        // startCapPntIds[0] = mesh.AddVertex(pointInnerLowerList[0]);
        // startCapPntIds[1] = mesh.AddVertex(pointOuterLowerList[0]);
        // startCapPntIds[2] = mesh.AddVertex(pointOuterUpperList[0]);
        // startCapPntIds[3] = mesh.AddVertex(pointInnerUpperList[0]);
        // mesh.AddTriangle(startCapPntIds[0], startCapPntIds[1], startCapPntIds[2]);
        // mesh.AddTriangle(startCapPntIds[0], startCapPntIds[2], startCapPntIds[3]);

        // int[] endCapPntIds = new int[4];
        // endCapPntIds[0] = mesh.AddVertex(pointInnerLowerList[pointInnerLowerList.Count - 1]);
        // endCapPntIds[1] = mesh.AddVertex(pointOuterLowerList[pointOuterLowerList.Count - 1]);
        // endCapPntIds[2] = mesh.AddVertex(pointOuterUpperList[pointOuterUpperList.Count - 1]);
        // endCapPntIds[3] = mesh.AddVertex(pointInnerUpperList[pointInnerUpperList.Count - 1]);
        // mesh.AddTriangle(endCapPntIds[0], endCapPntIds[1], endCapPntIds[2]);
        // mesh.AddTriangle(endCapPntIds[0], endCapPntIds[2], endCapPntIds[3]);

        KoreMeshDataEditOps.SetNormalsFromTriangles(combiningMesh);

        return combiningMesh;


        // mesh.MakeValid();
        // return mesh;
    }

    // --------------------------------------------------------------------------------------------

    // Usage: var cubeMesh = KoreMeshDataPrimitives.BasicCube(1.0f, new KoreColorRGB(255, 0, 0));
    // public static KoreMeshData BasicCube(float size, KoreColorRGB color)
    // {
    //     var mesh = new KoreMeshData();

    //     // Define the vertices of the cube
    //     int v0 = mesh.AddVertex(new KoreXYZVector(-size, -size, -size), null, color);
    //     int v1 = mesh.AddVertex(new KoreXYZVector(size, -size, -size), null, color);
    //     int v2 = mesh.AddVertex(new KoreXYZVector(size, size, -size), null, color);
    //     int v3 = mesh.AddVertex(new KoreXYZVector(-size, size, -size), null, color);
    //     int v4 = mesh.AddVertex(new KoreXYZVector(-size, -size, size), null, color);
    //     int v5 = mesh.AddVertex(new KoreXYZVector(size, -size, size), null, color);
    //     int v6 = mesh.AddVertex(new KoreXYZVector(size, size, size), null, color);
    //     int v7 = mesh.AddVertex(new KoreXYZVector(-size, size, size), null, color);

    //     // Lines
    //     mesh.AddLine(v0, v1, color, color);
    //     mesh.AddLine(v1, v5, color, color);
    //     mesh.AddLine(v5, v4, color, color);
    //     mesh.AddLine(v4, v0, color, color);
    //     mesh.AddLine(v2, v3, color, color);
    //     mesh.AddLine(v3, v7, color, color);
    //     mesh.AddLine(v7, v6, color, color);
    //     mesh.AddLine(v6, v2, color, color);
    //     mesh.AddLine(v0, v3, color, color);
    //     mesh.AddLine(v1, v2, color, color);
    //     mesh.AddLine(v4, v7, color, color);
    //     mesh.AddLine(v5, v6, color, color);

    //     // Triangles
    //     mesh.AddTriangle(v0, v1, v2); mesh.AddTriangle(v0, v2, v3);
    //     mesh.AddTriangle(v0, v3, v4); mesh.AddTriangle(v3, v7, v4);
    //     mesh.AddTriangle(v4, v7, v6); mesh.AddTriangle(v4, v6, v5);
    //     mesh.AddTriangle(v5, v6, v2); mesh.AddTriangle(v5, v2, v1);
    //     mesh.AddTriangle(v2, v7, v3); mesh.AddTriangle(v2, v6, v7); // top
    //     mesh.AddTriangle(v0, v5, v1); mesh.AddTriangle(v0, v4, v5); // bottom

    //     mesh.MakeValid();
    //     return mesh;
    // }

    // // Usage: var cubeMesh = KoreMeshDataPrimitives.BasicCubeSharpEdges(1.0f, new KoreColorRGB(255, 0, 0));
    // public static KoreMeshData BasicCubeSharpEdges(float size, KoreColorRGB color)
    // {
    //     var mesh = new KoreMeshData();

    //     // Create 24 vertices (4 per face) with proper face normals for sharp edges
    //     // Each face gets its own 4 vertices with the correct normal

    //     // Front face (normal: 0, 0, -1)
    //     var frontNormal = new KoreXYZVector(0, 0, -1);
    //     int f0 = mesh.AddVertex(new KoreXYZVector(-size, -size, -size), frontNormal, color);
    //     int f1 = mesh.AddVertex(new KoreXYZVector(size, -size, -size), frontNormal, color);
    //     int f2 = mesh.AddVertex(new KoreXYZVector(size, size, -size), frontNormal, color);
    //     int f3 = mesh.AddVertex(new KoreXYZVector(-size, size, -size), frontNormal, color);

    //     // Back face (normal: 0, 0, 1)
    //     var backNormal = new KoreXYZVector(0, 0, 1);
    //     int b0 = mesh.AddVertex(new KoreXYZVector(size, -size, size), backNormal, color);
    //     int b1 = mesh.AddVertex(new KoreXYZVector(-size, -size, size), backNormal, color);
    //     int b2 = mesh.AddVertex(new KoreXYZVector(-size, size, size), backNormal, color);
    //     int b3 = mesh.AddVertex(new KoreXYZVector(size, size, size), backNormal, color);

    //     // Left face (normal: -1, 0, 0)
    //     var leftNormal = new KoreXYZVector(-1, 0, 0);
    //     int l0 = mesh.AddVertex(new KoreXYZVector(-size, -size, size), leftNormal, color);
    //     int l1 = mesh.AddVertex(new KoreXYZVector(-size, -size, -size), leftNormal, color);
    //     int l2 = mesh.AddVertex(new KoreXYZVector(-size, size, -size), leftNormal, color);
    //     int l3 = mesh.AddVertex(new KoreXYZVector(-size, size, size), leftNormal, color);

    //     // Right face (normal: 1, 0, 0)
    //     var rightNormal = new KoreXYZVector(1, 0, 0);
    //     int r0 = mesh.AddVertex(new KoreXYZVector(size, -size, -size), rightNormal, color);
    //     int r1 = mesh.AddVertex(new KoreXYZVector(size, -size, size), rightNormal, color);
    //     int r2 = mesh.AddVertex(new KoreXYZVector(size, size, size), rightNormal, color);
    //     int r3 = mesh.AddVertex(new KoreXYZVector(size, size, -size), rightNormal, color);

    //     // Top face (normal: 0, 1, 0)
    //     var topNormal = new KoreXYZVector(0, 1, 0);
    //     int t0 = mesh.AddVertex(new KoreXYZVector(-size, size, -size), topNormal, color);
    //     int t1 = mesh.AddVertex(new KoreXYZVector(size, size, -size), topNormal, color);
    //     int t2 = mesh.AddVertex(new KoreXYZVector(size, size, size), topNormal, color);
    //     int t3 = mesh.AddVertex(new KoreXYZVector(-size, size, size), topNormal, color);

    //     // Bottom face (normal: 0, -1, 0)
    //     var bottomNormal = new KoreXYZVector(0, -1, 0);
    //     int bot0 = mesh.AddVertex(new KoreXYZVector(-size, -size, size), bottomNormal, color);
    //     int bot1 = mesh.AddVertex(new KoreXYZVector(size, -size, size), bottomNormal, color);
    //     int bot2 = mesh.AddVertex(new KoreXYZVector(size, -size, -size), bottomNormal, color);
    //     int bot3 = mesh.AddVertex(new KoreXYZVector(-size, -size, -size), bottomNormal, color);

    //     // Add triangles for each face (2 triangles per face)
    //     // Front face
    //     mesh.AddTriangle(f0, f1, f2, color);
    //     mesh.AddTriangle(f0, f2, f3, color);

    //     // Back face
    //     mesh.AddTriangle(b0, b1, b2, color);
    //     mesh.AddTriangle(b0, b2, b3, color);

    //     // Left face
    //     mesh.AddTriangle(l0, l1, l2, color);
    //     mesh.AddTriangle(l0, l2, l3, color);

    //     // Right face
    //     mesh.AddTriangle(r0, r1, r2, color);
    //     mesh.AddTriangle(r0, r2, r3, color);

    //     // Top face
    //     mesh.AddTriangle(t0, t1, t2, color);
    //     mesh.AddTriangle(t0, t2, t3, color);

    //     // Bottom face
    //     mesh.AddTriangle(bot0, bot1, bot2, color);
    //     mesh.AddTriangle(bot0, bot2, bot3, color);

    //     // Add edge lines for wireframe (using separate vertices to avoid interfering with face normals)
    //     int v0 = mesh.AddVertex(new KoreXYZVector(-size, -size, -size), null, color);
    //     int v1 = mesh.AddVertex(new KoreXYZVector(size, -size, -size), null, color);
    //     int v2 = mesh.AddVertex(new KoreXYZVector(size, size, -size), null, color);
    //     int v3 = mesh.AddVertex(new KoreXYZVector(-size, size, -size), null, color);
    //     int v4 = mesh.AddVertex(new KoreXYZVector(-size, -size, size), null, color);
    //     int v5 = mesh.AddVertex(new KoreXYZVector(size, -size, size), null, color);
    //     int v6 = mesh.AddVertex(new KoreXYZVector(size, size, size), null, color);
    //     int v7 = mesh.AddVertex(new KoreXYZVector(-size, size, size), null, color);

    //     // Lines
    //     mesh.AddLine(v0, v1, color, color);
    //     mesh.AddLine(v1, v5, color, color);
    //     mesh.AddLine(v5, v4, color, color);
    //     mesh.AddLine(v4, v0, color, color);
    //     mesh.AddLine(v2, v3, color, color);
    //     mesh.AddLine(v3, v7, color, color);
    //     mesh.AddLine(v7, v6, color, color);
    //     mesh.AddLine(v6, v2, color, color);
    //     mesh.AddLine(v0, v3, color, color);
    //     mesh.AddLine(v1, v2, color, color);
    //     mesh.AddLine(v4, v7, color, color);
    //     mesh.AddLine(v5, v6, color, color);

    //     // Don't call MakeValid() as it would overwrite our carefully set normals
    //     return mesh;
    // }

    // // Usage: var cubeMesh = KoreMeshDataPrimitives.BasicCubeSharpEdges2(1.0f, new KoreColorRGB(255, 0, 0));
    // public static KoreMeshData BasicCubeSharpEdges2(float size, KoreColorRGB color)
    // {
    //     var mesh = new KoreMeshData();

    //     // Define the 8 corner vertices manually outside of the mesh
    //     var v0 = new KoreXYZVector(-size, -size, -size); // front bottom left
    //     var v1 = new KoreXYZVector(size, -size, -size);  // front bottom right
    //     var v2 = new KoreXYZVector(size, size, -size);   // front top right
    //     var v3 = new KoreXYZVector(-size, size, -size);  // front top left
    //     var v4 = new KoreXYZVector(-size, -size, size);  // back bottom left
    //     var v5 = new KoreXYZVector(size, -size, size);   // back bottom right
    //     var v6 = new KoreXYZVector(size, size, size);    // back top right
    //     var v7 = new KoreXYZVector(-size, size, size);   // back top left

    //     // Create all 12 triangles (2 per face) using AddIsolatedTriangle
    //     // Using the EXACT same triangle winding as the working BasicCube function
    //     // Each triangle automatically calculates and assigns the correct face normal

    //     float noiseFactor = 0.1f; // Adjust noise factor as needed

    //     // Copy the triangulation from BasicCube (which works perfectly)
    //     mesh.AddIsolatedTriangle(v0, v1, v2, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color); mesh.AddIsolatedTriangle(v0, v2, v3, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color);
    //     mesh.AddIsolatedTriangle(v0, v3, v4, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color); mesh.AddIsolatedTriangle(v3, v7, v4, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color);
    //     mesh.AddIsolatedTriangle(v4, v7, v6, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color); mesh.AddIsolatedTriangle(v4, v6, v5, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color);
    //     mesh.AddIsolatedTriangle(v5, v6, v2, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color); mesh.AddIsolatedTriangle(v5, v2, v1, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color);
    //     mesh.AddIsolatedTriangle(v2, v7, v3, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color); mesh.AddIsolatedTriangle(v2, v6, v7, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color); // top
    //     mesh.AddIsolatedTriangle(v0, v5, v1, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color); mesh.AddIsolatedTriangle(v0, v4, v5, KoreColorOps.ColorWithRGBNoise(color, noiseFactor), color); // bottom

    //     // Add wireframe lines using the same 8 corner vertices
    //     int lv0 = mesh.AddVertex(v0, null, color);
    //     int lv1 = mesh.AddVertex(v1, null, color);
    //     int lv2 = mesh.AddVertex(v2, null, color);
    //     int lv3 = mesh.AddVertex(v3, null, color);
    //     int lv4 = mesh.AddVertex(v4, null, color);
    //     int lv5 = mesh.AddVertex(v5, null, color);
    //     int lv6 = mesh.AddVertex(v6, null, color);
    //     int lv7 = mesh.AddVertex(v7, null, color);

    //     // 12 edges of the cube
    //     mesh.AddLine(lv0, lv1, color, color); // front bottom
    //     mesh.AddLine(lv1, lv2, color, color); // front right
    //     mesh.AddLine(lv2, lv3, color, color); // front top
    //     mesh.AddLine(lv3, lv0, color, color); // front left
    //     mesh.AddLine(lv4, lv5, color, color); // back bottom
    //     mesh.AddLine(lv5, lv6, color, color); // back right
    //     mesh.AddLine(lv6, lv7, color, color); // back top
    //     mesh.AddLine(lv7, lv4, color, color); // back left
    //     mesh.AddLine(lv0, lv4, color, color); // left bottom
    //     mesh.AddLine(lv1, lv5, color, color); // right bottom
    //     mesh.AddLine(lv2, lv6, color, color); // right top
    //     mesh.AddLine(lv3, lv7, color, color); // left top

    //     return mesh;
    // }

    // // ---------------------------------------------------------------------------------------------

    // public static KoreMeshData SizedBox(
    //     double sizeUp, double sizeDown,
    //     double sizeLeft, double sizeRight,
    //     double sizeFront, double sizeBack,
    //     KoreColorRGB color)
    // {
    //     // Create a new KoreMeshData object
    //     var mesh = new KoreMeshData();

    //     // Define 8 unique vertices for the rectangular box
    //     // Front face vertices:
    //     int v0 = mesh.AddVertex(new KoreXYZVector(-sizeLeft, -sizeDown, -sizeFront), null, color); // Lower left front
    //     int v1 = mesh.AddVertex(new KoreXYZVector(sizeRight, -sizeDown, -sizeFront), null, color); // Lower right front
    //     int v2 = mesh.AddVertex(new KoreXYZVector(sizeRight, sizeUp, -sizeFront), null, color); // Upper right front
    //     int v3 = mesh.AddVertex(new KoreXYZVector(-sizeLeft, sizeUp, -sizeFront), null, color); // Upper left front

    //     // Back face vertices:
    //     int v4 = mesh.AddVertex(new KoreXYZVector(-sizeLeft, -sizeDown, sizeBack), null, color); // Lower left back
    //     int v5 = mesh.AddVertex(new KoreXYZVector(sizeRight, -sizeDown, sizeBack), null, color); // Lower right back
    //     int v6 = mesh.AddVertex(new KoreXYZVector(sizeRight, sizeUp, sizeBack), null, color); // Upper right back
    //     int v7 = mesh.AddVertex(new KoreXYZVector(-sizeLeft, sizeUp, sizeBack), null, color); // Upper left back

    //     // Define edges (lines)
    //     // Lines
    //     mesh.AddLine(v0, v1, color, color);
    //     mesh.AddLine(v1, v5, color, color);
    //     mesh.AddLine(v5, v4, color, color);
    //     mesh.AddLine(v4, v0, color, color);
    //     mesh.AddLine(v2, v3, color, color);
    //     mesh.AddLine(v3, v7, color, color);
    //     mesh.AddLine(v7, v6, color, color);
    //     mesh.AddLine(v6, v2, color, color);
    //     mesh.AddLine(v0, v3, color, color);
    //     mesh.AddLine(v1, v2, color, color);
    //     mesh.AddLine(v4, v7, color, color);
    //     mesh.AddLine(v5, v6, color, color);

    //     // Triangles
    //     mesh.AddTriangle(v0, v1, v2); mesh.AddTriangle(v0, v2, v3);
    //     mesh.AddTriangle(v4, v5, v6); mesh.AddTriangle(v4, v6, v7);
    //     mesh.AddTriangle(v0, v1, v5); mesh.AddTriangle(v0, v5, v4);
    //     mesh.AddTriangle(v1, v2, v6); mesh.AddTriangle(v1, v6, v5);
    //     mesh.AddTriangle(v2, v3, v7); mesh.AddTriangle(v2, v7, v6);
    //     mesh.AddTriangle(v3, v0, v4); mesh.AddTriangle(v3, v4, v7);

    //     return mesh;
    // }

    // // ---------------------------------------------------------------------------------------------

    // public static KoreMeshData SizedBox(
    //     KoreXYZBox box,
    //     KoreColorRGB? linecolor = null)
    // {
    //     return SizedBox(
    //         box.OffsetUp, box.OffsetDown,
    //         box.OffsetLeft, box.OffsetRight,
    //         box.OffsetForwards, box.OffsetBackwards,
    //         linecolor ?? KoreColorRGB.White);
    // }

    // // Usage: var cubeMesh = KoreMeshDataPrimitives.BasicCubeIsolatedTriangles(1.0f, new KoreColorRGB(255, 0, 0));
    // public static KoreMeshData BasicCubeIsolatedTriangles(float size, KoreColorRGB color)
    // {
    //     var mesh = new KoreMeshData();

    //     // Define the 8 corner vertices for reference
    //     var v0 = new KoreXYZVector(-size, -size, -size); // front bottom left
    //     var v1 = new KoreXYZVector(size, -size, -size);  // front bottom right
    //     var v2 = new KoreXYZVector(size, size, -size);   // front top right
    //     var v3 = new KoreXYZVector(-size, size, -size);  // front top left
    //     var v4 = new KoreXYZVector(-size, -size, size);  // back bottom left
    //     var v5 = new KoreXYZVector(size, -size, size);   // back bottom right
    //     var v6 = new KoreXYZVector(size, size, size);    // back top right
    //     var v7 = new KoreXYZVector(-size, size, size);   // back top left

    //     // Create all 12 triangles (2 per face) using AddIsolatedTriangle for sharp edges
    //     // Each triangle will automatically get the correct face normal

    //     // Front face (2 triangles)
    //     mesh.AddIsolatedTriangle(v0, v1, v2, color, color);
    //     mesh.AddIsolatedTriangle(v0, v2, v3, color, color);

    //     // Back face (2 triangles)
    //     mesh.AddIsolatedTriangle(v5, v4, v7, color, color);
    //     mesh.AddIsolatedTriangle(v5, v7, v6, color, color);

    //     // Left face (2 triangles)
    //     mesh.AddIsolatedTriangle(v4, v0, v3, color, color);
    //     mesh.AddIsolatedTriangle(v4, v3, v7, color, color);

    //     // Right face (2 triangles)
    //     mesh.AddIsolatedTriangle(v1, v5, v6, color, color);
    //     mesh.AddIsolatedTriangle(v1, v6, v2, color, color);

    //     // Top face (2 triangles)
    //     mesh.AddIsolatedTriangle(v3, v2, v6, color, color);
    //     mesh.AddIsolatedTriangle(v3, v6, v7, color, color);

    //     // Bottom face (2 triangles)
    //     mesh.AddIsolatedTriangle(v4, v5, v1, color, color);
    //     mesh.AddIsolatedTriangle(v4, v1, v0, color, color);

    //     // Add wireframe lines using shared vertices to avoid duplicating line geometry
    //     int lv0 = mesh.AddVertex(v0, null, color);
    //     int lv1 = mesh.AddVertex(v1, null, color);
    //     int lv2 = mesh.AddVertex(v2, null, color);
    //     int lv3 = mesh.AddVertex(v3, null, color);
    //     int lv4 = mesh.AddVertex(v4, null, color);
    //     int lv5 = mesh.AddVertex(v5, null, color);
    //     int lv6 = mesh.AddVertex(v6, null, color);
    //     int lv7 = mesh.AddVertex(v7, null, color);

    //     mesh.AddLine(lv0, lv1, color, color);
    //     mesh.AddLine(lv1, lv5, color, color);
    //     mesh.AddLine(lv5, lv4, color, color);
    //     mesh.AddLine(lv4, lv0, color, color);
    //     mesh.AddLine(lv2, lv3, color, color);
    //     mesh.AddLine(lv3, lv7, color, color);
    //     mesh.AddLine(lv7, lv6, color, color);
    //     mesh.AddLine(lv6, lv2, color, color);
    //     mesh.AddLine(lv0, lv3, color, color);
    //     mesh.AddLine(lv1, lv2, color, color);
    //     mesh.AddLine(lv4, lv7, color, color);
    //     mesh.AddLine(lv5, lv6, color, color);

    //     return mesh;
    // }

}
