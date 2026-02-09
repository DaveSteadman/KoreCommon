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

public static partial class KoreMeshDataIO
{
    static string startColorName = "start";
    static string endColorName   = "end";
    //static string colorName      = "color";

    // --------------------------------------------------------------------------------------------
    // MARK: ToJson
    // --------------------------------------------------------------------------------------------

    // Save KoreMeshData to JSON (triangles as 3 points, lines as native structure)
    // Usage: string jsonStr = KoreMeshDataIO.ToJson(mesh, dense: true);
    public static string ToJson(KoreMeshData mesh, bool dense = false)
    {
        var obj = new
        {
            vertices             = mesh.Vertices,
            lines                = mesh.Lines,
            triangles            = mesh.Triangles,
            normals              = mesh.Normals,
            uvs                  = mesh.UVs,
            vertexColors         = mesh.VertexColors,
            lineColors           = mesh.LineColors,
            materials            = mesh.Materials,
            namedTriangleGroups  = mesh.NamedTriangleGroups
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = !dense,
            AllowTrailingCommas = true,
            Converters = {
                new Vector3Converter(),
                new Vector2Converter(),
                new ColorConverter(),
                new TriangleConverter(),
                new LineConverter(),
                new KoreMeshLineColourConverter(),
                new KoreMeshMaterialConverter(),
                new KoreMeshTriangleGroupConverter()
            }
        };
        return JsonSerializer.Serialize(obj, options);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: FromJson
    // --------------------------------------------------------------------------------------------

    // Load KoreMeshData from JSON (optimistic: ignore unknowns, default missing)
    public static KoreMeshData FromJson(string json)
    {
        var mesh = new KoreMeshData();
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;

        // --- Vertices ---
        if (root.TryGetProperty("vertices", out var vertsProp) && vertsProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var v in vertsProp.EnumerateObject())
                mesh.Vertices[int.Parse(v.Name)] = Vector3Converter.ReadVector3(v.Value);
        }

        // --- Lines ---
        if (root.TryGetProperty("lines", out var linesProp) && linesProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var l in linesProp.EnumerateObject())
                mesh.Lines[int.Parse(l.Name)] = LineConverter.ReadLine(l.Value);
        }

        // --- Triangles ---
        if (root.TryGetProperty("triangles", out var trisProp) && trisProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var tri in trisProp.EnumerateObject())
                mesh.Triangles[int.Parse(tri.Name)] = TriangleConverter.ReadTriangle(tri.Value);
        }

        // --- Normals ---
        if (root.TryGetProperty("normals", out var normalsProp) && normalsProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var n in normalsProp.EnumerateObject())
                mesh.Normals[int.Parse(n.Name)] = Vector3Converter.ReadVector3(n.Value);
        }

        // --- UVs ---
        if (root.TryGetProperty("uvs", out var uvsProp) && uvsProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var uv in uvsProp.EnumerateObject())
                mesh.UVs[int.Parse(uv.Name)] = Vector2Converter.ReadVector2(uv.Value);
        }

        // --- VertexColors ---
        if (root.TryGetProperty("vertexColors", out var colorsProp) && colorsProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var c in colorsProp.EnumerateObject())
                mesh.VertexColors[int.Parse(c.Name)] = ColorConverter.ReadColor(c.Value);
        }

        // --- LineColors ---
        if (root.TryGetProperty("lineColors", out var lineColorsProp) && lineColorsProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var c in lineColorsProp.EnumerateObject())
                mesh.LineColors[int.Parse(c.Name)] = KoreMeshLineColourConverter.ReadLineColour(c.Value);
        }

        // --- Materials ---
        if (root.TryGetProperty("materials", out var materialsProp))
        {
            if (materialsProp.ValueKind == JsonValueKind.Array)
            {
                // New format: array of materials
                foreach (var m in materialsProp.EnumerateArray())
                {
                    var material = KoreMeshMaterialConverter.ReadMaterial(m);
                    mesh.AddMaterial(material);
                }
            }
        }

        // --- NamedTriangleGroups ---
        if (root.TryGetProperty("namedTriangleGroups", out var groupsProp) && groupsProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var g in groupsProp.EnumerateObject())
                mesh.NamedTriangleGroups[g.Name] = KoreMeshTriangleGroupConverter.ReadTriangleGroup(g.Value);
        }

        return mesh;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Converters
    // --------------------------------------------------------------------------------------------

    private class Vector3Converter : JsonConverter<KoreXYZVector>
    {
        public override KoreXYZVector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadVector3(doc.RootElement);
        }
        public override void Write(Utf8JsonWriter writer, KoreXYZVector value, JsonSerializerOptions options)
        {
            string str = KoreXYZVectorIO.ToString(value);
            writer.WriteStringValue(str);
        }

        public static KoreXYZVector ReadVector3(JsonElement el)
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

    private class Vector2Converter : JsonConverter<KoreXYVector>
    {
        public override KoreXYVector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadVector2(doc.RootElement);
        }
        public override void Write(Utf8JsonWriter writer, KoreXYVector value, JsonSerializerOptions options)
        {
            string str = KoreXYVectorIO.ToStringWithDP(value, 4);
            writer.WriteStringValue(str);
        }
        public static KoreXYVector ReadVector2(JsonElement el)
        {
            string str = el.GetString() ?? "";
            if (!string.IsNullOrEmpty(str))
            {
                return KoreXYVectorIO.FromString(str);
            }

            return KoreXYVector.Zero;
        }
    }

    // --------------------------------------------------------------------------------------------

    private class ColorConverter : JsonConverter<KoreColorRGB>
    {
        public override KoreColorRGB Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadColor(doc.RootElement);
        }
        public override void Write(Utf8JsonWriter writer, KoreColorRGB value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(KoreColorIO.RBGtoHexStringShort(value));
        }
        public static KoreColorRGB ReadColor(JsonElement el)
        {
            string? hex = el.GetString();
            if (hex != null)
                return KoreColorIO.HexStringToRGB(hex);
            return KoreColorRGB.Zero;
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: LineConverter
    // --------------------------------------------------------------------------------------------

    private class LineConverter : JsonConverter<KoreMeshLine>
    {
        public override KoreMeshLine Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadLine(doc.RootElement);
        }

        public override void Write(Utf8JsonWriter writer, KoreMeshLine value, JsonSerializerOptions options)
        {

            string str = $"{value.A}, {value.B}";
            writer.WriteStringValue(str);
        }

        public static KoreMeshLine ReadLine(JsonElement el)
        {
            // read the string representation
            string? str = el.GetString() ?? "";

            // split by comma
            if (!string.IsNullOrEmpty(str))
            {
                var parts = str.Split(',');
                if (parts.Length < 2) throw new FormatException("Invalid KoreMeshLine string format.");

                int pnt1Id = int.Parse(parts[0].Trim());
                int pnt2Id = int.Parse(parts[1].Trim());

                // If KoreMeshLine has color fields, parse them here as needed.
                // For now, just use the two indices.
                return new KoreMeshLine(pnt1Id, pnt2Id);
            }
            return new KoreMeshLine(0, 0);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: TriangleConverter
    // --------------------------------------------------------------------------------------------

    private class TriangleConverter : JsonConverter<KoreMeshTriangle>
    {
        public override KoreMeshTriangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadTriangle(doc.RootElement);
        }

        public override void Write(Utf8JsonWriter writer, KoreMeshTriangle value, JsonSerializerOptions options)
        {
            string str = $"{value.A}, {value.B}, {value.C}";
            writer.WriteStringValue(str);
        }

        public static KoreMeshTriangle ReadTriangle(JsonElement el)
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

                return new KoreMeshTriangle(a, b, c);
            }
            return new KoreMeshTriangle(0, 0, 0);
        }
    }


    // --------------------------------------------------------------------------------------------
    // MARK: LineColourConverter
    // --------------------------------------------------------------------------------------------

    private class KoreMeshLineColourConverter : JsonConverter<KoreMeshLineColour>
    {
        public override KoreMeshLineColour Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadLineColour(doc.RootElement);
        }

        public override void Write(Utf8JsonWriter writer, KoreMeshLineColour value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{startColorName}: {KoreColorIO.RBGtoHexStringShort(value.StartColor)}, {endColorName}: {KoreColorIO.RBGtoHexStringShort(value.EndColor)}");
        }

        public static KoreMeshLineColour ReadLineColour(JsonElement el)
        {
            // read the string representation
            string? str = el.GetString() ?? "";

            // split by comma
            if (!string.IsNullOrEmpty(str))
            {
                var parts = str.Split(',');
                if (parts.Length != 2) throw new FormatException($"Invalid KoreMeshLineColour string format. {str}.");

                string startColorStr = parts[0].Split(':')[1].Trim();
                string endColorStr   = parts[1].Split(':')[1].Trim();

                //Console.WriteLine($"KoreMeshLineColourConverter: ReadLineColour: lineIndex: {lineIndex}, startColorStr: {startColorStr}, endColorStr: {endColorStr}");

                KoreColorRGB startColor = KoreColorIO.HexStringToRGB(startColorStr);
                KoreColorRGB endColor   = KoreColorIO.HexStringToRGB(endColorStr);

                return new KoreMeshLineColour(startColor, endColor);
            }
            return new KoreMeshLineColour(KoreColorRGB.White, KoreColorRGB.White);
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: MaterialConverter
    // --------------------------------------------------------------------------------------------

    private class KoreMeshMaterialConverter : JsonConverter<KoreMeshMaterial>
    {
        public override KoreMeshMaterial Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadMaterial(doc.RootElement);
        }

        public override void Write(Utf8JsonWriter writer, KoreMeshMaterial value, JsonSerializerOptions options)
        {
            // Format: "name: Gold, baseColor: #FFD700, metallic: 1.0, roughness: 0.1, filename: texture.png"
            string baseColorHex = KoreColorIO.RBGtoHexStringShort(value.BaseColor);
            string filenameSection = string.IsNullOrEmpty(value.Filename) ? "" : $", filename: {value.Filename}";
            writer.WriteStringValue($"name: {value.Name}, baseColor: {baseColorHex}, metallic: {value.Metallic:F1}, roughness: {value.Roughness:F1}{filenameSection}");
        }

        public static KoreMeshMaterial ReadMaterial(JsonElement el)
        {
            string? str = el.GetString() ?? "";

            if (!string.IsNullOrEmpty(str))
            {
                // Parse format: "name: Gold, baseColor: #FFD700, metallic: 1.0, roughness: 0.1, filename: texture.png"
                var parts = str.Split(',');
                if (parts.Length < 4 || parts.Length > 5)
                    throw new FormatException($"Invalid KoreMeshMaterial string format. Expected 4-5 parts but got {parts.Length}: {str}");

                // Parse name
                string namePart = parts[0].Split(':')[1].Trim();

                // Parse base color (includes alpha)
                string baseColorPart = parts[1].Split(':')[1].Trim();
                KoreColorRGB baseColor = KoreColorIO.HexStringToRGB(baseColorPart);

                // Parse metallic
                string metallicPart = parts[2].Split(':')[1].Trim();
                float metallic = float.Parse(metallicPart);

                // Parse roughness
                string roughnessPart = parts[3].Split(':')[1].Trim();
                float roughness = float.Parse(roughnessPart);

                // Parse filename (optional)
                string? filename = null;
                if (parts.Length == 5)
                {
                    filename = parts[4].Split(':')[1].Trim();
                }

                return new KoreMeshMaterial(namePart, baseColor, metallic, roughness, filename);
            }

            return KoreMeshMaterialPalette.Find("White");
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: TriangleGroupConverter
    // --------------------------------------------------------------------------------------------

    private class KoreMeshTriangleGroupConverter : JsonConverter<KoreMeshTriangleGroup>
    {
        public override KoreMeshTriangleGroup Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadTriangleGroup(doc.RootElement);
        }

        public override void Write(Utf8JsonWriter writer, KoreMeshTriangleGroup value, JsonSerializerOptions options)
        {
            // Format: "materialName: Gold, triangleIds: [1,2,3,4]"
            string triangleIdsList = string.Join(",", value.TriangleIds);
            writer.WriteStringValue($"materialName: {value.MaterialName}, triangleIds: [{triangleIdsList}]");
        }

        public static KoreMeshTriangleGroup ReadTriangleGroup(JsonElement el)
        {
            string? str = el.GetString() ?? "";

            if (!string.IsNullOrEmpty(str))
            {
                // Parse format: "materialName: Gold, triangleIds: [1,2,3,4]"
                var parts = str.Split(',');
                if (parts.Length < 2)
                    throw new FormatException($"Invalid KoreMeshTriangleGroup string format. Expected at least 2 parts: {str}");

                // Parse materialName
                string materialNamePart = parts[0].Split(':')[1].Trim();
                string materialName = materialNamePart.Trim('"'); // Remove quotes if present

                // Parse triangleIds - everything after "triangleIds: [" and before "]"
                string triangleIdsPart = str.Substring(str.IndexOf('[') + 1);
                triangleIdsPart = triangleIdsPart.Substring(0, triangleIdsPart.LastIndexOf(']'));

                var triangleIds = new List<int>();
                if (!string.IsNullOrWhiteSpace(triangleIdsPart))
                {
                    var idStrings = triangleIdsPart.Split(',');
                    foreach (var idStr in idStrings)
                    {
                        if (int.TryParse(idStr.Trim(), out int id))
                            triangleIds.Add(id);
                    }
                }

                return new KoreMeshTriangleGroup(materialName, triangleIds);
            }

            return new KoreMeshTriangleGroup("", new List<int>());
        }
    }
}

