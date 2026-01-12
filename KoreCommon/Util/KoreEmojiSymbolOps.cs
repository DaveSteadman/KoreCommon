// <fileheader>

using System.Collections.Generic;

namespace KoreCommon;

#nullable enable

// Example Usage:
// string successSymbol = KoreEmojiSymbolOps.GetSymbol(KoreEmojiType.Success);

// Enum representing common status symbols and emoji characters for use in status strings
public enum KoreEmojiType
{
    // Status indicators
    Success,
    Error,
    Warning,
    Info,
    Question,

    // Progress indicators
    Action,
    InProgress,
    Pending,
    Complete,
    Failed,

    // Arrows
    ArrowRight,
    ArrowLeft,
    ArrowUp,
    ArrowDown,

    // Traffic light status (progress reporting)
    TrafficLightRed,
    TrafficLightOrange,
    TrafficLightYellow,
    TrafficLightGreen,
    TrafficLightBlue,
    TrafficLightPurple,
    TrafficLightWhite,
    TrafficLightBlack,

    // Common symbols
    Star,
    StarFilled,
    Bullet,

    // Time and clock
    Clock,
    Hourglass,

    // Misc
    Gear,
    Folder,
    File,
    Lock,
    Key,

    // Computer symbols
    Laptop,
    Desktop,
    Database,
    Notebook,
    Server,
    HardDrive,

    // Communication and thinking
    Brain,
    SpeechBubble
}

public static class KoreEmojiSymbolOps
{
    private static readonly Dictionary<KoreEmojiType, string> SymbolDictionary = new Dictionary<KoreEmojiType, string>
    {
        // Status indicators
        { KoreEmojiType.Success,         "✅" },  // U+2705 White check mark in green box
        { KoreEmojiType.Error,           "❌" },  // U+274C Red cross mark
        { KoreEmojiType.Warning,         "⚠️" },   // U+26A0 Warning sign (emoji variant)
        { KoreEmojiType.Info,            "ℹ️" },   // U+2139 Information (emoji variant)
        { KoreEmojiType.Question,        "❓" },  // U+2753 Red question mark

        // Progress indicators
        { KoreEmojiType.Action,          "▶️" },   // U+25B6 Black right-pointing triangle (emoji variant)
        { KoreEmojiType.InProgress,      "🔄" },  // U+1F504 Counterclockwise arrows button (refresh/processing)
        { KoreEmojiType.Pending,         "⏸️" },   // U+23F8 Pause button
        { KoreEmojiType.Complete,        "🎉" },  // U+1F389 Party popper (celebration)
        { KoreEmojiType.Failed,          "🚫" },  // U+1F6AB Prohibited sign

        // Arrows
        { KoreEmojiType.ArrowRight,      "➡️" },   // U+27A1 Right arrow (emoji variant)
        { KoreEmojiType.ArrowLeft,       "⬅️" },   // U+2B05 Left arrow (emoji variant)
        { KoreEmojiType.ArrowUp,         "⬆️" },   // U+2B06 Up arrow (emoji variant)
        { KoreEmojiType.ArrowDown,       "⬇️" },   // U+2B07 Down arrow (emoji variant)

        // Traffic light status (progress reporting)
        { KoreEmojiType.TrafficLightRed,     "🔴" },  // U+1F534 Red circle - error/stopped
        { KoreEmojiType.TrafficLightOrange,  "🟠" },  // U+1F7E0 Orange circle - attention needed
        { KoreEmojiType.TrafficLightYellow,  "🟡" },  // U+1F7E1 Yellow circle - warning/caution
        { KoreEmojiType.TrafficLightGreen,   "🟢" },  // U+1F7E2 Green circle - success/go
        { KoreEmojiType.TrafficLightBlue,    "🔵" },  // U+1F535 Blue circle - info/processing
        { KoreEmojiType.TrafficLightPurple,  "🟣" },  // U+1F7E3 Purple circle - special status
        { KoreEmojiType.TrafficLightWhite,   "⚪" },  // U+26AA White circle - inactive/default
        { KoreEmojiType.TrafficLightBlack,   "⚫" },  // U+26AB Black circle - disabled/off

        // Common symbols
        { KoreEmojiType.Star,            "⭐" },  // U+2B50 White medium star
        { KoreEmojiType.StarFilled,      "🌟" },  // U+1F31F Glowing star
        { KoreEmojiType.Bullet,          "🔹" },  // U+1F539 Small blue diamond

        // Time and clock
        { KoreEmojiType.Clock,           "🕐" },  // U+1F550 Clock face one o'clock
        { KoreEmojiType.Hourglass,       "⏳" },  // U+23F3 Hourglass with flowing sand

        // Misc
        { KoreEmojiType.Gear,            "⚙️" },   // U+2699 Gear (emoji variant)
        { KoreEmojiType.Folder,          "📁" },  // U+1F4C1 File folder
        { KoreEmojiType.File,            "📄" },  // U+1F4C4 Page facing up
        { KoreEmojiType.Lock,            "🔒" },  // U+1F512 Locked
        { KoreEmojiType.Key,             "🔑" },  // U+1F511 Key

        // Computer symbols
        { KoreEmojiType.Laptop,          "💻" },  // U+1F4BB Laptop computer
        { KoreEmojiType.Desktop,         "🖥️" },   // U+1F5A5 Desktop computer
        { KoreEmojiType.Database,        "🗄️" },   // U+1F5C4 File cabinet (database)
        { KoreEmojiType.Notebook,        "📓" },  // U+1F4D3 Notebook
        { KoreEmojiType.Server,          "🖧" },  // U+1F5A7 Network/server
        { KoreEmojiType.HardDrive,       "💾" },  // U+1F4BE Floppy disk (storage)

        // Communication and thinking
        { KoreEmojiType.Brain,           "🧠" },  // U+1F9E0 Brain
        { KoreEmojiType.SpeechBubble,    "💬" }   // U+1F4AC Speech balloon
    };

    // Get the UTF-8 character for a given status symbol
    // Usage: string symbol = KoreEmojiSymbolOps.GetSymbol(KoreStatusSymbol.Success);
    public static string GetSymbol(KoreEmojiType symbol)
    {
        if (SymbolDictionary.TryGetValue(symbol, out string? value))
            return value;

        return "❓"; // Fallback
    }
}
