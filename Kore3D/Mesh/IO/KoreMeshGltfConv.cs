// <fileheader>


using System;
using System.Numerics;
using KoreCommon;

#nullable enable

// Conversion utilities between KoreMeshData coordinate system and glTF coordinate system.
//
// COORDINATE SYSTEMS:
// - KoreMeshData: X+ right, Y+ up, Z+ forward (right-handed, traditional RW)
//   UV: bottom-left origin (0,0), top-right (1,1), V+ goes up
// - glTF 2.0: X+ right, Y+ up, Z+ forward (right-handed)
//   UV: top-left origin (0,0), bottom-right (1,1), V+ goes down
//
// CONVERSIONS NEEDED:
// - Position/Normals: No coordinate flip needed (both Z+ forward)
// - UVs: V axis flip (KoreMeshData bottom-left origin → glTF top-left origin)
// - Triangle winding: No change needed
//
// Reference: glTF 2.0 Specification Section 3.6.2.2 "Coordinate System and Units"
public static class KoreMeshGltfConv
{
    // --------------------------------------------------------------------------------------------
    // MARK: Position 
    // --------------------------------------------------------------------------------------------

    // Convert KoreXYZVector position to glTF Vector3.
    // Direct conversion - both coordinate systems are Z+ forward
    public static Vector3 PositionKoreToGltf(KoreXYZVector pos)
    {
        return new Vector3((float)pos.X, (float)pos.Y, (float)pos.Z);
    }

    // Convert glTF Vector3 position back to KoreXYZVector.
    // Direct conversion - both coordinate systems are Z+ forward
    public static KoreXYZVector PositionGltfToKore(Vector3 pos)
    {
        return new KoreXYZVector(pos.X, pos.Y, pos.Z);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Normal
    // --------------------------------------------------------------------------------------------

    // Convert KoreXYZVector normal to glTF Vector3.
    // Direct conversion - both coordinate systems are Z+ forward
    public static Vector3 NormalKoreToGltf(KoreXYZVector normal)
    {
        return new Vector3((float)normal.X, (float)normal.Y, (float)normal.Z);
    }

    // Convert glTF Vector3 normal back to KoreXYZVector.
    // Direct conversion - both coordinate systems are Z+ forward
    public static KoreXYZVector NormalGltfToKore(Vector3 normal)
    {
        return new KoreXYZVector(normal.X, normal.Y, normal.Z);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: UV Conversions
    // --------------------------------------------------------------------------------------------

    // Convert KoreXYVector UV to glTF Vector2.
    // Flip V axis: KoreMeshData bottom-left (0,0) → glTF top-left (0,0)
    public static Vector2 UVKoreToGltf(KoreXYVector uv)
    {
        return new Vector2((float)uv.X, (float)uv.Y);
    }

    // Convert glTF Vector2 UV back to KoreXYVector.
    // Flip V axis: glTF top-left (0,0) → KoreMeshData bottom-left (0,0)
    public static KoreXYVector UVGltfToKore(Vector2 uv)
    {
        return new KoreXYVector(uv.X, uv.Y);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Color Conversions
    // --------------------------------------------------------------------------------------------

    // Convert KoreColorRGB to glTF color components.
    // Returns RGB values in 0-1 range for glTF materials.
    public static Vector3 ColorKoreToGltfRGB(KoreColorRGB color)
    {
        return new Vector3(color.Rf, color.Gf, color.Bf);
    }

    // Convert KoreColorRGB to glTF color components with alpha.
    // Returns RGBA values in 0-1 range for glTF materials.
    public static Vector4 ColorKoreToGltfRGBA(KoreColorRGB color)
    {
        return new Vector4(color.Rf, color.Gf, color.Bf, color.Af);
    }

    // Convert glTF RGB color back to KoreColorRGB.
    // Assumes full alpha (1.0).
    public static KoreColorRGB ColorGltfToKoreRGB(Vector3 rgb)
    {
        return new KoreColorRGB(rgb.X, rgb.Y, rgb.Z, 1.0f);
    }

    // Convert glTF RGBA color back to KoreColorRGB.
    public static KoreColorRGB ColorGltfToKoreRGBA(Vector4 rgba)
    {
        return new KoreColorRGB(rgba.X, rgba.Y, rgba.Z, rgba.W);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Triangle Winding
    // --------------------------------------------------------------------------------------------

    // Convert triangle indices for glTF.
    // Convert between winding conventions: KoreMeshData uses CW, glTF uses CCW
    // KoreMeshData CW (A,B,C) → glTF CCW (A,C,B) 
    public static (int, int, int) ConvertTriangleWinding(int a, int b, int c)
    {
        return (a, c, b); // Swap B and C to convert CW to CCW
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Material Properties
    // --------------------------------------------------------------------------------------------

    // Convert material roughness value for glTF PBR.
    // glTF uses roughness (0=mirror, 1=rough), some systems use smoothness.
    public static float RoughnessToGltf(float roughness)
    {
        return Math.Clamp(roughness, 0.0f, 1.0f);
    }

    // Convert material metallic value for glTF PBR.
    // glTF metallic: 0=dielectric, 1=metallic
    public static float MetallicToGltf(float metallic)
    {
        return Math.Clamp(metallic, 0.0f, 1.0f);
    }

    // Convert emissive strength for glTF.
    // glTF emissive factor is RGB multiplier, typically 0-1 but can exceed for HDR.
    public static Vector3 EmissiveKoreToGltf(KoreColorRGB emissive, float strength = 1.0f)
    {
        return new Vector3(
            emissive.Rf * strength,
            emissive.Gf * strength,
            emissive.Bf * strength);
    }
}



