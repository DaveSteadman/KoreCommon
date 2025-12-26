// <fileheader>

using System.Collections.Generic;

namespace KoreCommon;


// Usage: KoreColorRGB myCol = KoreColorPalette.Colors["MutedCyan"];

public static class KoreColorPalette
{
    public static KoreColorRGB DefaultColor = new KoreColorRGB(255, 255, 255); // KoreColorPalette.DefaultColor
    public static string DefaultColorName = "White";

    public static readonly Dictionary<string, KoreColorRGB> Colors = new Dictionary<string, KoreColorRGB>
    {
        // Primary colors (1s & 0s)
        { "Red",         new KoreColorRGB(255,   0,   0) },
        { "Green",       new KoreColorRGB(  0, 255,   0) },
        { "Blue",        new KoreColorRGB(  0,   0, 255) },
        { "Yellow",      new KoreColorRGB(255, 255,   0) },
        { "Magenta",     new KoreColorRGB(255,   0, 255) },
        { "Cyan",        new KoreColorRGB(  0, 255, 255) },

        // Monochrome colors
        { "Black",       new KoreColorRGB(  0,   0,   0) },
        { "DarkGrey",    new KoreColorRGB( 64,  64,  64) },
        { "Grey",        new KoreColorRGB(128, 128, 128) },
        { "LightGrey",   new KoreColorRGB(192, 192, 192) },
        { "OffWhite",    new KoreColorRGB(230, 230, 230) },
        { "White",       new KoreColorRGB(255, 255, 255) },

        { "Grey05pct",   new KoreColorRGB( 13,  13,  13) },
        { "Grey10pct",   new KoreColorRGB( 26,  26,  26) },
        { "Grey15pct",   new KoreColorRGB( 38,  38,  38) },
        { "Grey20pct",   new KoreColorRGB( 51,  51,  51) },
        { "Grey25pct",   new KoreColorRGB( 64,  64,  64) },
        { "Grey30pct",   new KoreColorRGB( 77,  77,  77) },
        { "Grey35pct",   new KoreColorRGB( 89,  89,  89) },
        { "Grey40pct",   new KoreColorRGB(102, 102, 102) },
        { "Grey45pct",   new KoreColorRGB(115, 115, 115) },
        { "Grey50pct",   new KoreColorRGB(128, 128, 128) },
        { "Grey55pct",   new KoreColorRGB(140, 140, 140) },
        { "Grey60pct",   new KoreColorRGB(153, 153, 153) },
        { "Grey65pct",   new KoreColorRGB(166, 166, 166) },
        { "Grey70pct",   new KoreColorRGB(179, 179, 179) },
        { "Grey75pct",   new KoreColorRGB(192, 192, 192) },
        { "Grey80pct",   new KoreColorRGB(204, 204, 204) },
        { "Grey85pct",   new KoreColorRGB(217, 217, 217) },
        { "Grey90pct",   new KoreColorRGB(230, 230, 230) },
        { "Grey95pct",   new KoreColorRGB(242, 242, 242) },

        // Light color pastels
        { "LightRed",      new KoreColorRGB(255, 179, 179) },
        { "LightGreen",    new KoreColorRGB(179, 255, 179) },
        { "LightBlue",     new KoreColorRGB(179, 179, 255) },
        { "LightYellow",   new KoreColorRGB(255, 255, 179) },
        { "LightCyan",     new KoreColorRGB(179, 255, 255) },
        { "LightMagenta",  new KoreColorRGB(255, 179, 255) },
        { "LightOrange",   new KoreColorRGB(255, 217, 179) },
        { "LightPurple",   new KoreColorRGB(217, 179, 255) },

        // Dark versions
        { "DarkRed",     new KoreColorRGB( 64,   0,   0) },
        { "DarkGreen",   new KoreColorRGB(  0,  64,   0) },
        { "DarkBlue",    new KoreColorRGB(  0,   0,  64) },
        { "DarkYellow",  new KoreColorRGB( 64,  64,   0) },
        { "DarkCyan",    new KoreColorRGB(  0,  64,  64) },
        { "DarkMagenta", new KoreColorRGB( 64,   0,  64) },

        // Secondary and midtone colors
        { "Navy",        new KoreColorRGB(  0,   0, 128) },
        { "MidGreen",    new KoreColorRGB(  0, 128,   0) },
        { "Teal",        new KoreColorRGB(  0, 128, 128) },
        { "Azure",       new KoreColorRGB(  0, 128, 255) },
        { "SpringGreen", new KoreColorRGB(  0, 255, 128) },
        { "Maroon",      new KoreColorRGB(128,   0,   0) },
        { "Purple",      new KoreColorRGB(128,   0, 128) },
        { "Violet",      new KoreColorRGB(128,   0, 255) },
        { "Olive",       new KoreColorRGB(128, 128,   0) },
        { "MidBlue",     new KoreColorRGB(128, 128, 255) },
        { "Chartreuse",  new KoreColorRGB(128, 255,   0) },
        { "MidCyan",     new KoreColorRGB(128, 255, 255) },
        { "Rose",        new KoreColorRGB(255,   0, 128) },
        { "Orange",      new KoreColorRGB(255, 128,   0) },

        // Notable named colors
        { "Gold",        new KoreColorRGB(255, 214,   0) },
        { "Silver",      new KoreColorRGB(192, 192, 192) },
        { "Bronze",      new KoreColorRGB(204, 133,  64) },
        { "Brown",       new KoreColorRGB(153, 102,  51) },
        { "Pink",        new KoreColorRGB(255, 191, 204) },
        { "Lime",        new KoreColorRGB(191, 255,   0) },
        { "Indigo",      new KoreColorRGB( 74,   0, 130) },
        { "Copper",      new KoreColorRGB(184, 115,  51) },
        { "Coral",       new KoreColorRGB(255, 128,  79) },
        { "Lavender",    new KoreColorRGB(230, 230, 255) },
        { "Peach",       new KoreColorRGB(255, 217, 179) },
        { "Mint",        new KoreColorRGB(179, 255, 179) },

        // Off/Muted colors
        { "MutedYellow", new KoreColorRGB(204, 204,  77) },
        { "MutedGreen",  new KoreColorRGB( 77, 204,  77) },
        { "MutedBlue",   new KoreColorRGB( 77,  77, 230) },
        { "MutedRed",    new KoreColorRGB(204,  77,  77) },
        { "MutedPurple", new KoreColorRGB(204,  77, 204) },
        { "MutedCyan",   new KoreColorRGB( 77, 204, 204) },
        { "MutedOrange", new KoreColorRGB(204, 128,  77) },
        { "DustyPink",   new KoreColorRGB(204, 128, 128) },
        { "MutedBrown",  new KoreColorRGB(204, 153, 102) },
        { "MutedLime",   new KoreColorRGB(128, 204,  77) },
        { "MutedIndigo", new KoreColorRGB( 77,   0, 128) },
        { "MutedGold",   new KoreColorRGB(204, 179,   0) },
        { "MutedSilver", new KoreColorRGB(153, 153, 153) },
        { "MutedBronze", new KoreColorRGB(179, 128,  77) }
    };

    // Looks for the named string in the palette, returns DefaultColor if not found
    // Usage: KoreColorRGB col = KoreColorPalette.Find("Cyan");
    public static KoreColorRGB Find(string name)
    {
        // Case-insensitive search through the colors dictionary
        foreach (var kvp in Colors)
        {
            if (string.Equals(kvp.Key, name, System.StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        return DefaultColor; // Default color
    }

    public static KoreColorList ToColorList()
    {
        var list = new KoreColorList();
        foreach (var col in Colors.Values)
            list.AddColor(col);
        return list;
    }

    // --------------------------------------------------------------------------------------------

    // Pick a random color from the palette
    // Usage: KoreColorRGB randCol = KoreColorPalette.RandomColor();

    public static KoreColorRGB RandomColor()
    {
        var rand = new System.Random();
        int index = rand.Next(Colors.Count);
        foreach (var color in Colors.Values)
        {
            if (index == 0)
                return color;
            index--;
        }
        return DefaultColor; // Fallback, should not reach here
    }

    // --------------------------------------------------------------------------------------------

    // Get the name and color of the closest match in the palette to the given color
    // Usage: (string name, KoreColorRGB col) = KoreColorPalette.ClosestColor(targetColor);

    public static (string, KoreColorRGB) ClosestColor(KoreColorRGB targetColor)
    {
        // Initialise our working variables off the first list entry
        float closestDist = KoreColorOps.ColorDistance(KoreColorPalette.Colors["White"], targetColor);
        string closestName = KoreColorPalette.DefaultColorName;

        foreach (var kvp in Colors)
        {
            string colorName = kvp.Key;
            KoreColorRGB colorValue = kvp.Value;

            float currDist = KoreColorOps.ColorDistance(colorValue, targetColor);
            if (currDist < closestDist)
            {
                closestDist = currDist;
                closestName = colorName;
            }
        }
        return (closestName, Colors[closestName]);
    }

}
