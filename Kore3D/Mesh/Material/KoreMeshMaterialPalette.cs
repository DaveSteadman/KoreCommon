// <fileheader>

using System;
using System.Collections.Generic;

#nullable enable

namespace KoreCommon;

// KoreMeshMaterialPalette: A static class containing predefined materials accessible by name.
// Similar to KoreColorPalette and location palettes, provides a named dictionary of common materials.
// Useful for consistent material usage across projects and easy material referencing by string names.

public static class KoreMeshMaterialPalette
{
    public static readonly string DefaultMaterialName = "DefaultMaterial"; // Default material name KoreMeshMaterialPalette.DefaultMaterialName
    public static readonly KoreMeshMaterial DefaultMaterial = new(DefaultMaterialName, KoreColorRGB.White, 0.0f, 0.8f); // KoreMeshMaterialPalette.DefaultMaterial

    private static readonly List<KoreMeshMaterial> MaterialsList =
    [
        // ----------------------------------------------------------------------------------------
        // MARK: Basic Colors
        // ----------------------------------------------------------------------------------------

        new ("MattWhite",    new KoreColorRGB(255, 255, 255), 0.0f, 1.0f),
        new ("MattGray",     new KoreColorRGB(128, 128, 128), 0.0f, 1.0f),
        new ("MattBlack",    new KoreColorRGB(  0,   0,   0), 0.0f, 1.0f),

        new ("MattRed",      new KoreColorRGB(255,   0,   0), 0.0f, 1.0f),
        new ("MattGreen",    new KoreColorRGB(  0, 255,   0), 0.0f, 1.0f),
        new ("MattBlue",     new KoreColorRGB(  0,   0, 255), 0.0f, 1.0f),
        new ("MattYellow",   new KoreColorRGB(255, 255,   0), 0.0f, 1.0f),
        new ("MattMagenta",  new KoreColorRGB(255,   0, 255), 0.0f, 1.0f),
        new ("MattCyan",     new KoreColorRGB(  0, 255, 255), 0.0f, 1.0f),
        new ("MattOrange",   new KoreColorRGB(255, 165,   0), 0.0f, 1.0f),

        new ("MattDarkRed",  new KoreColorRGB(130,   0,   0), 0.0f, 1.0f),

        // ----------------------------------------------------------------------------------------
        // MARK: Metallic Materials
        // ----------------------------------------------------------------------------------------

        new ("Metal",        new KoreColorRGB(128, 128, 128), 1.0f, 0.2f),
        new ("Gold",         new KoreColorRGB(255, 214,   0), 1.0f, 0.1f),
        new ("Silver",       new KoreColorRGB(242, 242, 242), 1.0f, 0.1f),
        new ("Copper",       new KoreColorRGB(184, 115,  51), 1.0f, 0.2f),
        new ("Bronze",       new KoreColorRGB(163, 128,  61), 1.0f, 0.3f),
        new ("Steel",        new KoreColorRGB(179, 179, 179), 1.0f, 0.2f),
        new ("Iron",         new KoreColorRGB(143, 145, 148), 1.0f, 0.4f),
        new ("Aluminum",     new KoreColorRGB(232, 235, 235), 1.0f, 0.1f),

        // ----------------------------------------------------------------------------------------
        // MARK: Plastic Materials
        // ----------------------------------------------------------------------------------------

        new ("PlasticRed",    new KoreColorRGB(255,   0,   0), 0.0f, 0.8f),
        new ("PlasticBlue",   new KoreColorRGB(  0,   0, 255), 0.0f, 0.8f),
        new ("PlasticGreen",  new KoreColorRGB(  0, 255,   0), 0.0f, 0.8f),
        new ("PlasticWhite",  new KoreColorRGB(250, 250, 250), 0.0f, 0.8f),
        new ("PlasticBlack",  new KoreColorRGB(  0,   0,   0), 0.0f, 0.8f),
        new ("PlasticYellow", new KoreColorRGB(255, 255,   0), 0.0f, 0.8f),

        // ----------------------------------------------------------------------------------------
        // MARK: Stone Materials
        // ----------------------------------------------------------------------------------------

        new ("Marble",       new KoreColorRGB(237, 237, 237), 0.0f, 0.1f),
        new ("Granite",      new KoreColorRGB(102, 102, 102), 0.0f, 0.8f),
        new ("Sandstone",    new KoreColorRGB(194, 179, 128), 0.0f, 0.9f),
        new ("Concrete",     new KoreColorRGB(140, 140, 140), 0.0f, 0.9f),

        // ----------------------------------------------------------------------------------------
        // MARK: Transparent Materials
        // ----------------------------------------------------------------------------------------

        new ("Glass",        new KoreColorRGB(250, 250, 250, 100), 0.0f, 0.0f),
        new ("SmokedGlass",  new KoreColorRGB( 77,  77,  77, 128), 0.0f, 0.1f),
        new ("BlueGlass",    new KoreColorRGB( 51, 102, 204, 102), 0.0f, 0.0f),
        new ("GreenGlass",   new KoreColorRGB( 51, 204, 102, 102), 0.0f, 0.0f),
        new ("Water",        new KoreColorRGB(  0, 102, 204, 179), 0.0f, 0.0f),
        new ("Ice",          new KoreColorRGB(230, 242, 255, 204), 0.0f, 0.0f),

        // ----------------------------------------------------------------------------------------
        // MARK: Fabric Materials
        // ----------------------------------------------------------------------------------------

        new ("Cotton",       new KoreColorRGB(242, 242, 230), 0.0f, 1.0f),
        new ("Silk",         new KoreColorRGB(230, 217, 204), 0.0f, 0.2f),
        new ("Leather",      new KoreColorRGB(115,  77,  38), 0.0f, 0.7f),
        new ("Rubber",       new KoreColorRGB( 38,  38,  38), 0.0f, 0.9f),

        // ----------------------------------------------------------------------------------------
        // MARK: Special Effect Materials
        // ----------------------------------------------------------------------------------------

        new ("Chrome",       new KoreColorRGB(242, 242, 242), 1.0f, 0.0f),
        new ("Mirror",       new KoreColorRGB(250, 250, 250), 1.0f, 0.0f),
        new ("Ceramic",      new KoreColorRGB(250, 250, 250), 0.0f, 0.1f),
        new ("Porcelain",    new KoreColorRGB(250, 250, 245), 0.0f, 0.0f),
    ];

    // --------------------------------------------------------------------------------------------
    // MARK: Helper Methods
    // --------------------------------------------------------------------------------------------

    // Find material by name, returns MattWhite if not found
    public static KoreMeshMaterial Find(string name)
    {
        foreach (var material in MaterialsList)
        {
            if (material.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return material;
        }

        // Return default MattWhite material if not found
        return DefaultMaterial;
    }

    // --------------------------------------------------------------------------------------------

    // Get material by name, returns White if not found (backward compatibility)
    public static KoreMeshMaterial GetMaterial(string name)
    {
        return Find(name);
    }

    // --------------------------------------------------------------------------------------------

    // Check if material exists in palette
    public static bool HasMaterial(string name)
    {
        foreach (var material in MaterialsList)
        {
            if (material.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    // --------------------------------------------------------------------------------------------

    // Get all material names
    public static string[] GetMaterialNames()
    {
        var names = new string[MaterialsList.Count];
        for (int i = 0; i < MaterialsList.Count; i++)
        {
            names[i] = MaterialsList[i].Name;
        }
        Array.Sort(names);
        return names;
    }

}
