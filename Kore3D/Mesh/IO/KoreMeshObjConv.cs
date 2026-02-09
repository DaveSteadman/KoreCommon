// <fileheader>

using System;
using System.Numerics;
using KoreCommon;

#nullable enable

// Conversion utilities between KoreMeshData coordinate system and OBJ coordinate system.
//
// COORDINATE SYSTEMS:
// - KoreMeshData: X+ right, Y+ up, Z+ forward (right-handed, Z+ forward)
//   UV: bottom-left origin (0,0), top-right (1,1), V+ goes up
//   Triangle winding: CW (clockwise) when viewed from outside
// - OBJ format: X+ right, Y+ up, Z+ forward (right-handed)
//   UV: bottom-left origin (0,0), top-right (1,1), V+ goes up
//   Triangle winding: CCW (counter-clockwise) when viewed from outside
//
// CONVERSIONS NEEDED:
// - Position/Normals: Direct copy (coordinate systems match)
// - UVs: Direct copy (both use bottom-left origin)
// - Triangle winding: Convert CW to CCW by swapping vertices
//
// Reference: OBJ format specification - Wavefront Technologies
public static class KoreMeshObjConv
{
    // --------------------------------------------------------------------------------------------
    // MARK: Position 
    // --------------------------------------------------------------------------------------------

    // Convert KoreXYZVector position to OBJ coordinates.
    // Direct conversion - both coordinate systems are identical
    public static (double X, double Y, double Z) PositionKoreToObj(KoreXYZVector pos)
    {
        return (pos.X, pos.Y, pos.Z);
    }

    // Convert OBJ position back to KoreXYZVector.
    // Direct conversion - both coordinate systems are identical
    public static KoreXYZVector PositionObjToKore(double x, double y, double z)
    {
        return new KoreXYZVector(x, y, z);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Normal
    // --------------------------------------------------------------------------------------------

    // Convert KoreXYZVector normal to OBJ coordinates.
    // Direct conversion - both coordinate systems are identical
    public static (double X, double Y, double Z) NormalKoreToObj(KoreXYZVector normal)
    {
        return (normal.X, normal.Y, normal.Z);
    }

    // Convert OBJ normal back to KoreXYZVector.
    // Direct conversion - both coordinate systems are identical
    public static KoreXYZVector NormalObjToKore(double x, double y, double z)
    {
        return new KoreXYZVector(x, y, z);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: UV Conversions
    // --------------------------------------------------------------------------------------------

    // Convert KoreXYVector UV to OBJ coordinates.
    // Direct conversion - both use bottom-left origin
    public static (double U, double V) UVKoreToObj(KoreXYVector uv)
    {
        return (uv.X, uv.Y);
    }

    // Convert OBJ UV back to KoreXYVector.
    // Direct conversion - both use bottom-left origin
    public static KoreXYVector UVObjToKore(double u, double v)
    {
        return new KoreXYVector(u, v);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Color Conversions
    // --------------------------------------------------------------------------------------------

    // Convert KoreColorRGB to OBJ material color components.
    // Returns RGB values in 0-1 range for MTL files.
    public static (float R, float G, float B) ColorKoreToObjRGB(KoreColorRGB color)
    {
        return (color.Rf, color.Gf, color.Bf);
    }

    // Convert KoreColorRGB to OBJ material color components with alpha.
    // Returns RGBA values in 0-1 range for MTL files.
    public static (float R, float G, float B, float A) ColorKoreToObjRGBA(KoreColorRGB color)
    {
        return (color.Rf, color.Gf, color.Bf, color.Af);
    }

    // Convert OBJ RGB color back to KoreColorRGB.
    // Assumes full alpha (1.0).
    public static KoreColorRGB ColorObjToKoreRGB(float r, float g, float b)
    {
        return new KoreColorRGB(r, g, b, 1.0f);
    }

    // Convert OBJ RGBA color back to KoreColorRGB.
    public static KoreColorRGB ColorObjToKoreRGBA(float r, float g, float b, float a)
    {
        return new KoreColorRGB(r, g, b, a);
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Triangle Winding
    // --------------------------------------------------------------------------------------------

    // Convert triangle indices for OBJ format.
    // Convert between winding conventions: KoreMeshData uses CW, OBJ uses CCW
    // KoreMeshData CW (A,B,C) → OBJ CCW (A,C,B) 
    public static (int, int, int) ConvertTriangleWinding(int a, int b, int c)
    {
        return (a, c, b); // Swap B and C to convert CW to CCW
    }

    // Convert triangle indices from OBJ back to KoreMeshData format.
    // OBJ CCW (A,B,C) → KoreMeshData CW (A,C,B)
    public static (int, int, int) ConvertTriangleWindingFromObj(int a, int b, int c)
    {
        return (a, c, b); // Swap B and C to convert CCW to CW
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Material Properties
    // --------------------------------------------------------------------------------------------

    // Convert material roughness to OBJ shininess (Ns).
    // OBJ uses shininess (1-200), KoreMeshData uses roughness (0-1)
    public static float RoughnessToObjShininess(float roughness)
    {
        // Convert roughness (0=smooth, 1=rough) to shininess (1=rough, 200=smooth)
        float shininess = 1.0f + (1.0f - Math.Clamp(roughness, 0.0f, 1.0f)) * 199.0f;
        return shininess;
    }

    // Convert OBJ shininess back to roughness.
    public static float ObjShininessToRoughness(float shininess)
    {
        // Convert shininess (1-200) to roughness (1-0)
        float normalizedShininess = Math.Clamp(shininess, 1.0f, 200.0f);
        float roughness = 1.0f - ((normalizedShininess - 1.0f) / 199.0f);
        return Math.Clamp(roughness, 0.0f, 1.0f);
    }

    // Convert metallic value to OBJ specular intensity.
    // OBJ specular (Ks) represents metallic-like behavior
    public static float MetallicToObjSpecular(float metallic)
    {
        return Math.Clamp(metallic, 0.0f, 1.0f);
    }

    // Convert OBJ specular intensity back to metallic approximation.
    public static float ObjSpecularToMetallic(float specularR, float specularG, float specularB)
    {
        // Estimate metallic behavior from average specular intensity
        return Math.Clamp((specularR + specularG + specularB) / 3.0f, 0.0f, 1.0f);
    }

    // Convert transparency values between formats.
    // KoreMeshData uses alpha (0=transparent, 1=opaque)
    // OBJ uses 'd' for alpha (0=transparent, 1=opaque) and 'Tr' for transparency (1=transparent, 0=opaque)
    public static float AlphaToObjTransparency(float alpha)
    {
        return 1.0f - Math.Clamp(alpha, 0.0f, 1.0f);
    }

    public static float ObjTransparencyToAlpha(float transparency)
    {
        return 1.0f - Math.Clamp(transparency, 0.0f, 1.0f);
    }
}
