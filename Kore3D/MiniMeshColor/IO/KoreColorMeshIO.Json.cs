// <fileheader>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace KoreCommon;

// Functions to serialize and deserialize KoreMeshData to/from JSON format.
// Note that some elements are stored in custom string formats, prioritizing human-readability over strict JSON compliance.
// - If a user is after a higher performance serialization, they should use the binary format instead of text.

public static partial class KoreColorMeshIO
{
    // --------------------------------------------------------------------------------------------
    // MARK: ToJson
    // --------------------------------------------------------------------------------------------

    // Save KoreMeshData to JSON (triangles as 3 points, lines as native structure)
    // Usage: string jsonStr = KoreMeshDataIO.ToJson(mesh, dense: true);
    public static string ToJson(KoreColorMesh mesh, bool dense = false)
    {
        var obj = new
        {
            vertices = mesh.Vertices,
            triangles = mesh.Triangles
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = !dense,
            AllowTrailingCommas = true,
            Converters = {
                new KoreColorMeshVector3Converter(),
                new KoreColorMeshTriConverter()
            }
        };
        return JsonSerializer.Serialize(obj, options);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: FromJson
    // --------------------------------------------------------------------------------------------

    // Load KoreColorMesh from JSON (optimistic: ignore unknowns, default missing)
    public static KoreColorMesh FromJson(string json)
    {
        var mesh = new KoreColorMesh();
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;

        // --- Vertices ---
        if (root.TryGetProperty("vertices", out var vertsProp) && vertsProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var v in vertsProp.EnumerateObject())
                mesh.Vertices[int.Parse(v.Name)] = KoreColorMeshVector3Converter.ReadElement(v.Value);
        }

        // --- Triangles ---
        if (root.TryGetProperty("triangles", out var trisProp) && trisProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var tri in trisProp.EnumerateObject())
                mesh.Triangles[int.Parse(tri.Name)] = KoreColorMeshTriConverter.ReadTriangle(tri.Value);
        }

        return mesh;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: CONVERTERS
    // --------------------------------------------------------------------------------------------

    private class KoreColorMeshVector3Converter : JsonConverter<KoreXYZVector>
    {
        public override KoreXYZVector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadElement(doc.RootElement);
        }
        public override void Write(Utf8JsonWriter writer, KoreXYZVector value, JsonSerializerOptions options)
        {
            string str = KoreXYZVectorIO.ToString(value);
            writer.WriteStringValue(str);
        }

        public static KoreXYZVector ReadElement(JsonElement el)
        {
            // if (el.ValueKind == JsonValueKind.Array && el.GetArrayLength() == 3)
            //     return new KoreXYZVector(el[0].GetSingle(), el[1].GetSingle(), el[2].GetSingle());
            // return KoreXYZVector.Zero;

            string str = el.GetString() ?? "";
            if (!string.IsNullOrEmpty(str))
            {
                return KoreXYZVectorIO.FromString(str);
            }

            return KoreXYZVector.Zero;
        }
    }

    // --------------------------------------------------------------------------------------------

    private class KoreColorMeshColorConverter : JsonConverter<KoreColorRGB>
    {
        public override KoreColorRGB Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadElement(doc.RootElement);
        }
        public override void Write(Utf8JsonWriter writer, KoreColorRGB value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(KoreColorIO.RBGtoHexStringShort(value));
        }
        public static KoreColorRGB ReadElement(JsonElement el)
        {
            string? hex = el.GetString();
            if (hex != null)
                return KoreColorIO.HexStringToRGB(hex);
            return KoreColorRGB.Zero;
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: TriangleConverter
    // --------------------------------------------------------------------------------------------

    private class KoreColorMeshTriConverter : JsonConverter<KoreColorMeshTri>
    {
        public override KoreColorMeshTri Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadTriangle(doc.RootElement);
        }

        public override void Write(Utf8JsonWriter writer, KoreColorMeshTri value, JsonSerializerOptions options)
        {
            string strColor = KoreColorIO.RBGtoHexStringShort(value.Color);
            string str = $"{value.A}, {value.B}, {value.C}, {strColor}";
            writer.WriteStringValue(str);
        }

        public static KoreColorMeshTri ReadTriangle(JsonElement el)
        {
            // read the string representation
            string? str = el.GetString() ?? "";

            // split by comma
            if (!string.IsNullOrEmpty(str))
            {
                var parts = str.Split(',');
                if (parts.Length != 3) throw new FormatException("Invalid KoreMeshTriangle string format.");

                int a = int.Parse(parts[0]);
                int b = int.Parse(parts[1]);
                int c = int.Parse(parts[2]);
                KoreColorRGB color = KoreColorIO.HexStringToRGB(parts[3].Trim());

                return new KoreColorMeshTri(a, b, c, color);
            }
            return new KoreColorMeshTri(-1, -1, -1, KoreColorRGB.Zero);
        }
    }


}

