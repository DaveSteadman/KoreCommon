// <fileheader>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace KoreCommon;

// Static class to create KoreMeshData primitives
// This class is used to create various 3D shapes and meshes in Godot

public static partial class KoreMiniMeshPrimitives
{
    // Usage: KoreMiniMesh cubeMesh = KoreMiniMeshPrimitives.BasicCube(1.0f, new KoreColorRGB(255, 0, 0), new KoreColorRGB(255, 255, 255));
    public static KoreMiniMesh BasicCube(float size, KoreMiniMeshMaterial material, KoreColorRGB lineCol)
    {
        var mesh = new KoreMiniMesh();

        // Add the material and color
        mesh.AddMaterial(material);
        int lineColorId = mesh.AddColor(lineCol);
        string matName = material.Name;

        // Define the vertices of the cube in Godot coordinate system (X+ right, Y+ up, Z- away/forward)
        // FRONT FACE (z = +size, closer to viewer):
        int v0 = mesh.AddVertex(new KoreXYZVector(-size, +size, +size)); // Top-Left-Front
        int v1 = mesh.AddVertex(new KoreXYZVector(+size, +size, +size)); // Top-Right-Front  
        int v2 = mesh.AddVertex(new KoreXYZVector(+size, -size, +size)); // Bottom-Right-Front
        int v3 = mesh.AddVertex(new KoreXYZVector(-size, -size, +size)); // Bottom-Left-Front

        // BACK FACE (z = -size, further from viewer):
        int v4 = mesh.AddVertex(new KoreXYZVector(-size, +size, -size)); // Top-Left-Back
        int v5 = mesh.AddVertex(new KoreXYZVector(+size, +size, -size)); // Top-Right-Back
        int v6 = mesh.AddVertex(new KoreXYZVector(+size, -size, -size)); // Bottom-Right-Back  
        int v7 = mesh.AddVertex(new KoreXYZVector(-size, -size, -size)); // Bottom-Left-Back

        // Lines (wireframe)
        mesh.AddLine(new KoreMiniMeshLine(v0, v1, lineColorId)); // Front top
        mesh.AddLine(new KoreMiniMeshLine(v1, v2, lineColorId)); // Front right
        mesh.AddLine(new KoreMiniMeshLine(v2, v3, lineColorId)); // Front bottom
        mesh.AddLine(new KoreMiniMeshLine(v3, v0, lineColorId)); // Front left
        
        mesh.AddLine(new KoreMiniMeshLine(v4, v5, lineColorId)); // Back top
        mesh.AddLine(new KoreMiniMeshLine(v5, v6, lineColorId)); // Back right
        mesh.AddLine(new KoreMiniMeshLine(v6, v7, lineColorId)); // Back bottom
        mesh.AddLine(new KoreMiniMeshLine(v7, v4, lineColorId)); // Back left
        
        mesh.AddLine(new KoreMiniMeshLine(v0, v4, lineColorId)); // Top-left edge
        mesh.AddLine(new KoreMiniMeshLine(v1, v5, lineColorId)); // Top-right edge
        mesh.AddLine(new KoreMiniMeshLine(v2, v6, lineColorId)); // Bottom-right edge
        mesh.AddLine(new KoreMiniMeshLine(v3, v7, lineColorId)); // Bottom-left edge

        List<int> allTris = new();
        
        // FACES - All faces wound CLOCKWISE when viewed from OUTSIDE the cube
        // Front face (z = +size) - viewed from outside (positive Z direction)
        // v0(TL) -> v1(TR) -> v2(BR) -> v3(BL), clockwise from outside
        allTris.AddRange(KoreMiniMeshOps.AddFace(mesh, v0, v1, v2, v3));

        // Back face (z = -size) - viewed from outside (negative Z direction, so we flip order)  
        // v5(TR) -> v4(TL) -> v7(BL) -> v6(BR), clockwise from outside
        allTris.AddRange(KoreMiniMeshOps.AddFace(mesh, v5, v4, v7, v6));

        // Top face (y = +size) - viewed from above (positive Y direction)
        // v4(TL) -> v5(TR) -> v1(TR) -> v0(TL), clockwise from above
        allTris.AddRange(KoreMiniMeshOps.AddFace(mesh, v4, v5, v1, v0));

        // Bottom face (y = -size) - viewed from below (negative Y direction, so we flip order)
        // v3(BL) -> v2(BR) -> v6(BR) -> v7(BL), clockwise from below  
        allTris.AddRange(KoreMiniMeshOps.AddFace(mesh, v3, v2, v6, v7));

        // Right face (x = +size) - viewed from right side (positive X direction)
        // v1(TR) -> v5(TR) -> v6(BR) -> v2(BR), clockwise from right
        allTris.AddRange(KoreMiniMeshOps.AddFace(mesh, v1, v5, v6, v2));

        // Left face (x = -size) - viewed from left side (negative X direction, so we flip order)
        // v4(TL) -> v0(TL) -> v3(BL) -> v7(BL), clockwise from left  
        allTris.AddRange(KoreMiniMeshOps.AddFace(mesh, v4, v0, v3, v7));

        // Groups
        mesh.AddGroup("All", new KoreMiniMeshGroup(matName, allTris));

        return mesh;
    }
}
