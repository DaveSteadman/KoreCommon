// <fileheader>

using System;
using System.Collections.Generic;
using System.Text;

namespace KoreCommon.Util;


public enum EmojiDescriptor
{
    None = 0,
    Check,
    Cross,
    Warning,
    Info,
    Question,
    Star,
    Sparkles,
    Fire,
    Heart,
    ThumbsUp,
    ThumbsDown,
    Smile,
    Grin,
    Sad,
    Cry,
    Angry,
    Party,
    Rocket,
    Hourglass,
    LightBulb,
    Bug,
    Hammer,
    Package
}


public static class EmojiUtil
{
    private static readonly IReadOnlyDictionary<EmojiDescriptor, string> Map =
        new Dictionary<EmojiDescriptor, string>
        {
            { EmojiDescriptor.None,        string.Empty },
            { EmojiDescriptor.Check,       "✅" },
            { EmojiDescriptor.Cross,       "❌" },
            { EmojiDescriptor.Warning,     "⚠️" },
            { EmojiDescriptor.Info,        "ℹ️" },
            { EmojiDescriptor.Question,    "❓" },
            { EmojiDescriptor.Star,        "⭐" },
            { EmojiDescriptor.Sparkles,    "✨" },
            { EmojiDescriptor.Fire,        "🔥" },
            { EmojiDescriptor.Heart,       "❤️" },
            { EmojiDescriptor.ThumbsUp,    "👍" },
            { EmojiDescriptor.ThumbsDown,  "👎" },
            { EmojiDescriptor.Smile,       "🙂" },
            { EmojiDescriptor.Grin,        "😄" },
            { EmojiDescriptor.Sad,         "☹️" },
            { EmojiDescriptor.Cry,         "😢" },
            { EmojiDescriptor.Angry,       "😠" },
            { EmojiDescriptor.Party,       "🥳" },
            { EmojiDescriptor.Rocket,      "🚀" },
            { EmojiDescriptor.Hourglass,   "⏳" },
            { EmojiDescriptor.LightBulb,   "💡" },
            { EmojiDescriptor.Bug,         "🐛" },
            { EmojiDescriptor.Hammer,      "🔨" },
            { EmojiDescriptor.Package,     "📦" }
        };

    public static string Get(EmojiDescriptor emoji)
    {
        return Map.TryGetValue(emoji, out var value) ? value : string.Empty;
    }

    public static string Concat(params EmojiDescriptor[] emojis)
    {
        if (emojis == null || emojis.Length == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var e in emojis)
            sb.Append(Get(e));

        return sb.ToString();
    }

    public static string Prefix(string message, EmojiDescriptor emoji, string separator = " ")
    {
        var e = Get(emoji);
        if (string.IsNullOrEmpty(e))
            return message ?? string.Empty;

        if (string.IsNullOrEmpty(message))
            return e;

        return e + separator + message;
    }

    public static string Suffix(string message, EmojiDescriptor emoji, string separator = " ")
    {
        var e = Get(emoji);
        if (string.IsNullOrEmpty(e))
            return message ?? string.Empty;

        if (string.IsNullOrEmpty(message))
            return e;

        return message + separator + e;
    }
}
