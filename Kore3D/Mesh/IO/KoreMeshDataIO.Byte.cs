// <fileheader>

using System;
using System.IO;
using System.Collections.Generic;

namespace KoreCommon;

public static partial class KoreMeshDataIO
{
    // --------------------------------------------------------------------------------------------
    // MARK: ToBytes
    // --------------------------------------------------------------------------------------------

    // Write a structure to bytes. We explicitly cast some of the types, to ensure that the correct
    // types are written to the byte array.

    public static byte[] ToBytes(KoreMeshData mesh)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // Vertices
        bw.Write((int)mesh.Vertices.Count);
        foreach (var kvp in mesh.Vertices)
        {
            int vertexId = kvp.Key;
            KoreXYZVector v = kvp.Value;

            bw.Write((int)vertexId);
            bw.Write((double)v.X);
            bw.Write((double)v.Y);
            bw.Write((double)v.Z);
        }

        // Lines
        bw.Write((int)mesh.Lines.Count);
        foreach (var kvp in mesh.Lines)
        {
            int lineId = kvp.Key;
            KoreMeshLine l = kvp.Value;
            bw.Write((int)lineId);
            bw.Write((int)l.A);
            bw.Write((int)l.B);
        }

        // Triangles
        bw.Write((int)mesh.Triangles.Count);
        foreach (var kvp in mesh.Triangles)
        {
            int triangleId = kvp.Key;
            KoreMeshTriangle t = kvp.Value;
            bw.Write((int)triangleId);
            bw.Write((int)t.A);
            bw.Write((int)t.B);
            bw.Write((int)t.C);
        }

        // Normals
        bw.Write((int)mesh.Normals.Count);
        foreach (var kvp in mesh.Normals)
        {
            int normalId = kvp.Key;
            KoreXYZVector n = kvp.Value;
            bw.Write((int)normalId);
            bw.Write((double)n.X);
            bw.Write((double)n.Y);
            bw.Write((double)n.Z);
        }

        // UVs
        bw.Write((int)mesh.UVs.Count);
        foreach (var kvp in mesh.UVs)
        {
            int uvId = kvp.Key;
            KoreXYVector uv = kvp.Value;
            bw.Write((int)uvId);
            bw.Write((double)uv.X);
            bw.Write((double)uv.Y);
        }

        // Vertex colors
        bw.Write((int)mesh.VertexColors.Count);
        foreach (var kvp in mesh.VertexColors)
        {
            int vertexId = kvp.Key;
            KoreColorRGB color = kvp.Value;

            bw.Write((int)vertexId);
            WriteColor(bw, color);
        }

        // Line colors
        bw.Write((int)mesh.LineColors.Count);
        foreach (var kvp in mesh.LineColors)
        {
            int lineId = kvp.Key;
            KoreMeshLineColour lc = kvp.Value;

            bw.Write((int)lineId);
            WriteColor(bw, lc.StartColor);
            WriteColor(bw, lc.EndColor);
        }

        // Materials
        bw.Write((int)mesh.Materials.Count);
        foreach (KoreMeshMaterial material in mesh.Materials)
        {
            WriteMaterial(bw, material);
        }

        // Triangle Groups
        bw.Write((int)mesh.NamedTriangleGroups.Count);
        foreach (var kvp in mesh.NamedTriangleGroups)
        {
            string groupName = kvp.Key;
            KoreMeshTriangleGroup group = kvp.Value;

            bw.Write(groupName);
            WriteTriangleGroup(bw, group);
        }

        bw.Flush();
        return ms.ToArray();
    }

    // --------------------------------------------------------------------------------------------
    // MARK: FromBytes
    // --------------------------------------------------------------------------------------------

    public static KoreMeshData FromBytes(byte[] data)
    {
        var mesh = new KoreMeshData();
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        // Vertices
        int vCount = br.ReadInt32();
        for (int i = 0; i < vCount; i++)
        {
            int vertexId = br.ReadInt32();
            double x = br.ReadDouble();
            double y = br.ReadDouble();
            double z = br.ReadDouble();
            mesh.Vertices[vertexId] = new KoreXYZVector(x, y, z);
        }

        // Lines
        int lCount = br.ReadInt32();
        for (int i = 0; i < lCount; i++)
        {
            int lineId = br.ReadInt32();
            int a = br.ReadInt32();
            int b = br.ReadInt32();
            mesh.Lines[lineId] = new KoreMeshLine(a, b);
        }

        // Triangles
        int tCount = br.ReadInt32();
        for (int i = 0; i < tCount; i++)
        {
            int triangleId = br.ReadInt32();
            int a = br.ReadInt32();
            int b = br.ReadInt32();
            int c = br.ReadInt32();
            mesh.Triangles[triangleId] = new KoreMeshTriangle(a, b, c);
        }

        // Normals
        int nCount = br.ReadInt32();
        for (int i = 0; i < nCount; i++)
            mesh.Normals[br.ReadInt32()] = new KoreXYZVector(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());

        // UVs
        int uvCount = br.ReadInt32();
        for (int i = 0; i < uvCount; i++)
            mesh.UVs[br.ReadInt32()] = new KoreXYVector(br.ReadDouble(), br.ReadDouble());

        // Vertex colors
        int vcCount = br.ReadInt32();
        for (int i = 0; i < vcCount; i++)
            mesh.VertexColors[br.ReadInt32()] = ReadColor(br);

        // Line colors
        int lcCount = br.ReadInt32();
        for (int i = 0; i < lcCount; i++)
            mesh.LineColors[br.ReadInt32()] = new KoreMeshLineColour(ReadColor(br), ReadColor(br));

        // Materials
        int mCount = br.ReadInt32();
        for (int i = 0; i < mCount; i++)
            mesh.Materials.Add(ReadMaterial(br));

        // Triangle Groups
        int tgCount = br.ReadInt32();
        for (int i = 0; i < tgCount; i++)
        {
            string groupName = br.ReadString();
            KoreMeshTriangleGroup group = ReadTriangleGroup(br);
            mesh.NamedTriangleGroups.Add(groupName, group);
        }

        return mesh;
    }

    // --------------------------------------------------------------------------------------------

    public static bool TryFromBytes(byte[] data, out KoreMeshData mesh)
    {
        // Initialise the return values to a blank mesh default value
        mesh = new KoreMeshData();
        try
        {
            // Read the mesh (as successfully as we can)
            mesh = FromBytes(data);

            // Return true if we successfully read the mesh
            return true;
        }
        catch
        {
            // If we hit an error, return false and the mesh object will remain in whatever state it reached.
            return false;
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Material
    // --------------------------------------------------------------------------------------------

    private static void WriteMaterial(BinaryWriter bw, KoreMeshMaterial material)
    {
        bw.Write(material.Name);                     // Write name as string
        WriteColor(bw, material.BaseColor);          // BaseColor already includes alpha
        bw.Write((float)material.Metallic);
        bw.Write((float)material.Roughness);
    }

    private static KoreMeshMaterial ReadMaterial(BinaryReader br)
    {
        string name = br.ReadString();               // Read name
        KoreColorRGB baseColor = ReadColor(br);      // BaseColor already includes alpha
        float metallic = br.ReadSingle();
        float roughness = br.ReadSingle();

        return new KoreMeshMaterial(name, baseColor, metallic, roughness);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Color
    // --------------------------------------------------------------------------------------------

    private static void WriteColor(BinaryWriter bw, KoreColorRGB c)
    {
        bw.Write((byte)c.R);
        bw.Write((byte)c.G);
        bw.Write((byte)c.B);
        bw.Write((byte)c.A);
    }

    private static KoreColorRGB ReadColor(BinaryReader br)
    {
        return new KoreColorRGB(br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte());
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Triangle Groups
    // --------------------------------------------------------------------------------------------

    private static void WriteTriangleGroup(BinaryWriter bw, KoreMeshTriangleGroup group)
    {
        bw.Write(group.MaterialName);
        bw.Write((int)group.TriangleIds.Count);
        foreach (var triangleId in group.TriangleIds)
        {
            bw.Write((int)triangleId);
        }
    }

    private static KoreMeshTriangleGroup ReadTriangleGroup(BinaryReader br)
    {
        string materialName = br.ReadString();
        int triangleCount = br.ReadInt32();
        List<int> triangleIds = new List<int>();
        for (int i = 0; i < triangleCount; i++)
        {
            triangleIds.Add(br.ReadInt32());
        }
        return new KoreMeshTriangleGroup(materialName, triangleIds);
    }
}