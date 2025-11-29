// <fileheader>

using System;
using System.Collections.Generic;

namespace KoreCommon;

// Static class to create KoreMiniMesh primitives
// This class is used to create various 3D shapes and meshes

public static partial class KoreMiniMeshPrimitives
{
    // Create a cylinder mesh for KoreMiniMesh
    // p1: Bottom center point
    // p2: Top center point
    // p1radius: Radius at bottom (p1)
    // p2radius: Radius at top (p2)
    // sides: Number of sides around the cylinder
    // endsClosed: Whether to create end cap faces
    // material: Material for the cylinder surface
    // lineCol: Color for wireframe lines
    // returns: KoreMiniMesh cylinder

    // Usage: KoreMiniMesh cyl1 = KoreMiniMeshPrimitives.CreateCylinder(p1, p2, p1radius, p2radius, sides, endsClosed, material, lineCol);
    public static KoreMiniMesh CreateCylinder(
        KoreXYZVector p1,
        KoreXYZVector p2,
        double p1radius,
        double p2radius,
        int sides,
        bool endsClosed,
        KoreMiniMeshMaterial material,
        KoreColorRGB lineCol)
    {
        if (sides < 3) throw new ArgumentException("Cylinder must have at least 3 sides");

        var mesh = new KoreMiniMesh();

        // Add material and line color
        mesh.AddMaterial(material);
        int lineColorId = mesh.AddColor(lineCol);
        string matName = material.Name;

        // Calculate cylinder axis and reference direction
        KoreXYZVector axis = (p2 - p1).Normalize();
        double height = (p2 - p1).Magnitude;

        if (height < 1e-6) throw new ArgumentException("Cylinder height must be greater than zero");

        // Debug output to check axis calculation
        Console.WriteLine($"Cylinder axis: {axis}, height: {height}");
        Console.WriteLine($"P1: {p1}, P2: {p2}");

        List<int> allTriangles = new List<int>();

        // Generate circle points for both ends using AddCirclePoints
        List<int> p1Circle = KoreMiniMeshOps.AddCirclePoints(mesh, p1, axis, p1radius, sides);
        List<int> p2Circle = KoreMiniMeshOps.AddCirclePoints(mesh, p2, axis, p2radius, sides);

        // Create the cylindrical surface using ribbon
        allTriangles.AddRange(KoreMiniMeshOps.AddRibbon(mesh, p1Circle, p2Circle));

        // Add end caps if requested
        if (endsClosed)
        {
            // Bottom cap (p1) - wind inward (normal pointing down the axis)
            int p1Center = mesh.AddVertex(p1);
            allTriangles.AddRange(KoreMiniMeshOps.AddFan(mesh, p1Center, p1Circle, true));

            // Top cap (p2) - wind outward (normal pointing up the axis)
            int p2Center = mesh.AddVertex(p2);
            allTriangles.AddRange(KoreMiniMeshOps.AddFan(mesh, p2Center, p2Circle, false));
        }

        // Create wireframe lines
        // Circle lines for both ends
        KoreMiniMeshOps.AddCircleLines(mesh, p1Circle, lineColorId);
        KoreMiniMeshOps.AddCircleLines(mesh, p2Circle, lineColorId);

        // Vertical lines connecting the circles
        for (int i = 0; i < sides; i++)
        {
            mesh.AddLine(new KoreMiniMeshLine(p1Circle[i], p2Circle[i], lineColorId));
        }

        // Add radial lines to center if caps are closed
        if (endsClosed)
        {
            int p1Center = mesh.Vertices.Count - 2; // Second to last vertex added
            int p2Center = mesh.Vertices.Count - 1; // Last vertex added

            // Add a few radial lines (not all, to avoid clutter)
            int radialLines = Math.Min(4, sides);
            for (int i = 0; i < radialLines; i++)
            {
                int idx = i * sides / radialLines;
                mesh.AddLine(new KoreMiniMeshLine(p1Center, p1Circle[idx], lineColorId));
                mesh.AddLine(new KoreMiniMeshLine(p2Center, p2Circle[idx], lineColorId));
            }
        }

        // Create groups
        mesh.AddGroup("All", new KoreMiniMeshGroup(matName, allTriangles));

        return mesh;
    }

    // Create a simple cylinder with default parameters
    // center: Center point of the cylinder
    // axis: Axis direction (normalized)
    // height: Height of the cylinder
    // radius: Radius shared by both ends
    // sides: Number of sides around the cylinder
    // material: Material for the cylinder surface
    // lineCol: Color for wireframe lines
    // returns: KoreMiniMesh cylinder
    public static KoreMiniMesh Cylinder(
        KoreXYZVector center,
        KoreXYZVector axis,
        double height,
        double radius,
        int sides,
        KoreMiniMeshMaterial material,
        KoreColorRGB lineCol)
    {
        axis = axis.Normalize();
        KoreXYZVector p1 = center - axis * (height * 0.5);
        KoreXYZVector p2 = center + axis * (height * 0.5);

        return CreateCylinder(p1, p2, radius, radius, sides, true, material, lineCol);
    }
}