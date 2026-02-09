using SkiaSharp;
using System.Collections.Generic;

namespace KorePlotter.NatoSymbol;

public static class KoreNatoSymbolColorPalette
{
    public static readonly Dictionary<string, SKColor> Colors = new Dictionary<string, SKColor>
    {
        { "Black",        new SKColor(0,   0,   0)   },
        { "MidGrey",      new SKColor(128, 128, 128) },
        { "OffWhiteGrey", new SKColor(239, 239, 239) },
        { "White",        new SKColor(255, 255, 255) },

        { "Red",          new SKColor(255, 0,   0)   },
        { "Green",        new SKColor(0,   170, 0)   },
        { "Blue",         new SKColor(0,   0,   255) },
        { "Yellow",       new SKColor(255, 255, 0)   },
        { "Cyan",         new SKColor(0,   255, 255) },
        { "NeonGreen",    new SKColor(0,   255, 0)   },

        { "CrystalBlue",  new SKColor(128, 224, 255) },
        { "LightYellow",  new SKColor(255, 255, 128) },
        { "BambooGreen",  new SKColor(170, 255, 170) },
        { "Salmon",       new SKColor(255, 128, 128) }
    };

    public static SKColor BackgroundFriendly { get; } = Colors["CrystalBlue"]; // NatoSymbolColorPalette.BackgroundFriendly
    public static SKColor BackgroundHostile { get; }  = Colors["Salmon"]; // NatoSymbolColorPalette.BackgroundHostile
    public static SKColor BackgroundNeutral { get; }  = Colors["BambooGreen"]; // NatoSymbolColorPalette.BackgroundNeutral
    public static SKColor BackgroundUnknown { get; }  = Colors["LightYellow"]; // NatoSymbolColorPalette.BackgroundUnknown

    public static SKColor Black { get; } = Colors["Black"]; // NatoSymbolColorPalette.Black
    public static SKColor White { get; } = Colors["White"]; // NatoSymbolColorPalette.White

    public static SKColor BackgroundColor { get; } = Colors["Black"]; // NatoSymbolColorPalette.BackgroundColor
    public static SKColor BorderColor { get; } = Colors["Black"]; // NatoSymbolColorPalette.BorderColor
    public static SKColor AccentColor { get; } = Colors["Black"]; // NatoSymbolColorPalette.AccentColor

}


