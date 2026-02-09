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
    public static KoreMeshData Cylinder(KoreXYZVector p1, KoreXYZVector p2, double p1radius, double p2radius, int sides, bool endsClosed)
    {
        var mesh = new KoreMeshData();

        // Direction and length
        KoreXYZVector axis = p2 - p1;
        double height = axis.Magnitude;
        axis = axis.Normalize();

        // return early if invalid
        if (height < 1e-6f || sides < 3) return mesh;

        // Find a vector not parallel to axis for basis
        KoreXYZVector up      = Math.Abs(KoreXYZVector.DotProduct(axis, KoreXYZVector.Up)) < 0.99f ? KoreXYZVector.Up : KoreXYZVector.Right;
        KoreXYZVector side    = KoreXYZVector.CrossProduct(axis, up).Normalize();
        KoreXYZVector forward = KoreXYZVector.CrossProduct(axis, side).Normalize();

        // Generate circle points for both ends
        var p1Circle = new List<KoreXYZVector>();
        var p2Circle = new List<KoreXYZVector>();
        var leftUVs  = new List<KoreXYVector>();
        var rightUVs = new List<KoreXYVector>();

        double angleStep = Math.Tau / sides;

        for (int i = 0; i < sides; i++)
        {
            double angle = i * angleStep;
            KoreXYZVector offset = (Math.Cos(angle) * side + Math.Sin(angle) * forward);
            p1Circle.Add(p1 + offset * p1radius);
            p2Circle.Add(p2 + offset * p2radius);

            // Generate UVs for ribbon (u = angle progress, v = height progress)
            double u = (double)i / sides;
            leftUVs.Add(new KoreXYVector(u, 0.0));   // p1 end
            rightUVs.Add(new KoreXYVector(u, 1.0));  // p2 end
        }

        // Create the cylindrical surface using Ribbon
        KoreMeshData ribbonMesh = Ribbon(p1Circle, leftUVs, p2Circle, rightUVs, true);
        ribbonMesh.AddAllTrianglesToGroup("cylinder");
        mesh = KoreMeshData.BasicAppendMesh(mesh, ribbonMesh);

        // Add end caps using Fan
        if (endsClosed)
        {
            // p1 cap (bottom)
            KoreMeshData p1CapMesh = Fan(p1, p1Circle, true);
            p1CapMesh.AddAllTrianglesToGroup("p1endcap");
            mesh = KoreMeshData.BasicAppendMesh(mesh, p1CapMesh);

            // p2 cap (top) - reverse the order for proper winding
            var p2CircleReversed = new List<KoreXYZVector>(p2Circle);
            p2CircleReversed.Reverse();
            KoreMeshData p2CapMesh = Fan(p2, p2CircleReversed, true);
            p2CapMesh.AddAllTrianglesToGroup("p2endcap");
            mesh = KoreMeshData.BasicAppendMesh(mesh, p2CapMesh);
        }

        // Calculate normals from triangles for proper lighting. The endcaps and
        // main cylinder all use different vertices. So this call does not average out edge normals.
        KoreMeshDataEditOps.SetNormalsFromTriangles(mesh);

        mesh.AddMaterial(new KoreMeshMaterial("DefaultMaterial", new KoreColorRGB(0.5f, 0.5f, 0.5f)));
        mesh.SetGroupMaterialName("p1endcap", "DefaultMaterial");
        mesh.SetGroupMaterialName("p2endcap", "DefaultMaterial");
        mesh.SetGroupMaterialName("cylinder", "DefaultMaterial");

        return mesh;
    }
}
