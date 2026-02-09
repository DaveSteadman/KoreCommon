// <fileheader>

using System;

#nullable enable

namespace KoreCommon;

// KoreMeshMaterial: A record struct to hold material properties for 3D geometry.
// Designed to be compatible with GLTF material properties and Godot's StandardMaterial3D.
// Keeps materials as simple data containers that rendering layers can interpret.
// Note: Alpha/transparency is handled by the BaseColor's A channel.
// Name field enables GLTF round-trip and semantic material identification.

public struct KoreMeshMaterial : IEquatable<KoreMeshMaterial>
{
    public string       Name      { get; set; }     // Material name for GLTF export/import and identification
    public string?      Filename  { get; set; }     // Optional texture filename - if not null or empty, overrides BaseColor
    public KoreColorRGB BaseColor { get; set; }     // Includes RGBA - alpha channel handles transparency
    public float        Metallic  { get; set; }     // 0 = dielectric (plastic/wood), 1 = metallic
    public float        Roughness { get; set; }     // 0 = mirror smooth, 1 = completely rough

    // --------------------------------------------------------------------------------------------
    // MARK: Constructors
    // --------------------------------------------------------------------------------------------

    public KoreMeshMaterial(string name, KoreColorRGB baseColor, float metallic = 0.0f, float roughness = 0.7f, string? filename = null)
    {
        Name      = name;
        Filename  = filename;
        BaseColor = baseColor;
        Metallic  = metallic;
        Roughness = roughness;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Default materials
    // --------------------------------------------------------------------------------------------

    public static KoreMeshMaterial White => KoreMeshMaterialPalette.GetMaterial("MattWhite");

    // --------------------------------------------------------------------------------------------
    // MARK: Helper Methods
    // --------------------------------------------------------------------------------------------

    // Create a material with just a color (using default material properties)
    public static KoreMeshMaterial FromColor(string name, KoreColorRGB color)
    {
        return new KoreMeshMaterial(name, color);
    }

    // Create a material with just a color (anonymous)
    public static KoreMeshMaterial FromColor(KoreColorRGB color)
    {
        return new KoreMeshMaterial("Anonymous", color);
    }

    // Create a material from a filename with fallback color
    // Usage: KoreMeshMaterial.FromTexture("MyTexture", "path/to/texture.png", KoreMeshMaterial.White.BaseColor)
    public static KoreMeshMaterial FromTexture(string name, string filename, KoreColorRGB fallbackColor)
    {
        return new KoreMeshMaterial(name, fallbackColor, filename: filename);
    }

    // Create a transparent version of this material
    public KoreMeshMaterial WithAlpha(float alpha)
    {
        // Create new color with specified alpha
        var newColor = new KoreColorRGB(BaseColor.Rf, BaseColor.Gf, BaseColor.Bf, alpha);
        return this with { BaseColor = newColor };
    }

    // Create a metallic version of this material
    public KoreMeshMaterial AsMetallic(float metallic = 1.0f, float roughness = 0.2f)
    {
        return this with { Metallic = metallic, Roughness = roughness };
    }

    // Create a plastic/matte version of this material
    public KoreMeshMaterial AsPlastic(float roughness = 0.8f)
    {
        return this with { Metallic = 0.0f, Roughness = roughness };
    }

    // Check if this material is transparent
    public bool IsTransparent => BaseColor.IsTransparent;

    // Check if this material is metallic
    public bool IsMetallic => Metallic > 0.5f;

    // --------------------------------------------------------------------------------------------
    // MARK: String Representation
    // --------------------------------------------------------------------------------------------

    public override string ToString()
    {
        string filenameStr = !string.IsNullOrEmpty(Filename) ? $", Tex:{Filename}" : "";
        if (BaseColor.IsTransparent)
            return $"Material({Name}, {BaseColor}, M:{Metallic:F1}, R:{Roughness:F1}{filenameStr})";
        else
            return $"Material({Name}, {BaseColor}, M:{Metallic:F1}, R:{Roughness:F1}{filenameStr})";
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Equals
    // --------------------------------------------------------------------------------------------

    public bool Equals(KoreMeshMaterial other)
    {
        bool baseColorMatch = BaseColor.Equals(other.BaseColor);
        bool metallicMatch  = KoreValueUtils.EqualsWithinTolerance(Metallic, other.Metallic);
        bool roughnessMatch = KoreValueUtils.EqualsWithinTolerance(Roughness, other.Roughness);

        return Name == other.Name &&
               Filename == other.Filename &&
               baseColorMatch &&
               metallicMatch &&
               roughnessMatch;
    }

    public override bool Equals(object? obj)
    {
        return obj is KoreMeshMaterial other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Filename, BaseColor, Metallic, Roughness);
    }

}
