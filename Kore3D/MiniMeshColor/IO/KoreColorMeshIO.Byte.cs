// <fileheader>

using System;
using System.IO;
using System.Collections.Generic;

namespace KoreCommon;

public static partial class KoreColorMeshIO
{
    public enum DataSize { AsDouble, AsFloat };

    // --------------------------------------------------------------------------------------------
    // MARK: ToBytes
    // --------------------------------------------------------------------------------------------

    // Write a structure to bytes. We explicitly cast some of the types, to ensure that the correct
    // types are written to the byte array.

    // Usage: var bytes = KoreColorMeshIO.ToBytes(mesh, DataSize.AsDouble);

    public static byte[] ToBytes(KoreColorMesh mesh, DataSize dataSize = DataSize.AsDouble)
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
            if (dataSize == DataSize.AsDouble)
            {
                bw.Write((double)v.X);
                bw.Write((double)v.Y);
                bw.Write((double)v.Z);
            }
            else
            {
                bw.Write((float)v.X);
                bw.Write((float)v.Y);
                bw.Write((float)v.Z);
            }
        }

        // Triangles
        bw.Write((int)mesh.Triangles.Count);
        foreach (var kvp in mesh.Triangles)
        {
            int triangleId = kvp.Key;
            KoreColorMeshTri t = kvp.Value;
            bw.Write((int)triangleId);

            if (dataSize == DataSize.AsDouble)
            {
                bw.Write((int)t.A);
                bw.Write((int)t.B);
                bw.Write((int)t.C);
            }
            else
            {
                bw.Write((short)t.A);
                bw.Write((short)t.B);
                bw.Write((short)t.C);
            }
            WriteColor(bw, t.Color);
        }

        bw.Flush();
        return ms.ToArray();
    }

    // --------------------------------------------------------------------------------------------
    // MARK: FromBytes
    // --------------------------------------------------------------------------------------------

    public static KoreColorMesh FromBytes(byte[] data, DataSize dataSize = DataSize.AsDouble)
    {
        var mesh = new KoreColorMesh();
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        // Vertices
        int vCount = br.ReadInt32();
        for (int i = 0; i < vCount; i++)
        {
            int vertexId = br.ReadInt32();

            if (dataSize == DataSize.AsDouble)
            {
                double x = br.ReadDouble();
                double y = br.ReadDouble();
                double z = br.ReadDouble();
                mesh.Vertices[vertexId] = new KoreXYZVector(x, y, z);
                continue;
            }
            else
            {
                float x = br.ReadSingle();
                float y = br.ReadSingle();
                float z = br.ReadSingle();
                mesh.Vertices[vertexId] = new KoreXYZVector(x, y, z);
            }
        }

        // Triangles
        int tCount = br.ReadInt32();
        for (int i = 0; i < tCount; i++)
        {
            int triangleId = br.ReadInt32();

            int a, b, c;
            if (dataSize == DataSize.AsDouble)
            {
                a = br.ReadInt32();
                b = br.ReadInt32();
                c = br.ReadInt32();
            }
            else
            {
                a = br.ReadInt16();
                b = br.ReadInt16();
                c = br.ReadInt16();
            }
            KoreColorRGB col = ReadColor(br);
            mesh.Triangles[triangleId] = new KoreColorMeshTri(a, b, c, col);
        }

        return mesh;
    }

    // --------------------------------------------------------------------------------------------

    public static bool TryFromBytes(byte[] data, out KoreColorMesh mesh)
    {
        // Initialise the return values to a blank mesh default value
        mesh = new KoreColorMesh();
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
}