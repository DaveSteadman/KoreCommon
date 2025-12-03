// <fileheader>

using System;
using KoreCommon.Util;

namespace KoreCommon.UnitTest;


public static class KoreTestEmojiUtil
{
    public static void RunTests(KoreTestLog testLog)
    {
        TestGet(testLog);
        TestConcat(testLog);
        TestPrefix(testLog);
        TestSuffix(testLog);
    }

    private static void TestGet(KoreTestLog testLog)
    {
        // Test that Get returns correct emoji for each descriptor
        testLog.AddResult("Get(Check) returns ✅", EmojiUtil.Get(EmojiDescriptor.Check) == "✅");
        testLog.AddResult("Get(Cross) returns ❌", EmojiUtil.Get(EmojiDescriptor.Cross) == "❌");
        testLog.AddResult("Get(Warning) returns ⚠️", EmojiUtil.Get(EmojiDescriptor.Warning) == "⚠️");
        testLog.AddResult("Get(Info) returns ℹ️", EmojiUtil.Get(EmojiDescriptor.Info) == "ℹ️");
        testLog.AddResult("Get(Question) returns ❓", EmojiUtil.Get(EmojiDescriptor.Question) == "❓");
        testLog.AddResult("Get(Star) returns ⭐", EmojiUtil.Get(EmojiDescriptor.Star) == "⭐");
        testLog.AddResult("Get(Sparkles) returns ✨", EmojiUtil.Get(EmojiDescriptor.Sparkles) == "✨");
        testLog.AddResult("Get(Fire) returns 🔥", EmojiUtil.Get(EmojiDescriptor.Fire) == "🔥");
        testLog.AddResult("Get(Heart) returns ❤️", EmojiUtil.Get(EmojiDescriptor.Heart) == "❤️");
        testLog.AddResult("Get(ThumbsUp) returns 👍", EmojiUtil.Get(EmojiDescriptor.ThumbsUp) == "👍");
        testLog.AddResult("Get(ThumbsDown) returns 👎", EmojiUtil.Get(EmojiDescriptor.ThumbsDown) == "👎");
        testLog.AddResult("Get(Smile) returns 🙂", EmojiUtil.Get(EmojiDescriptor.Smile) == "🙂");
        testLog.AddResult("Get(Grin) returns 😄", EmojiUtil.Get(EmojiDescriptor.Grin) == "😄");
        testLog.AddResult("Get(Sad) returns ☹️", EmojiUtil.Get(EmojiDescriptor.Sad) == "☹️");
        testLog.AddResult("Get(Cry) returns 😢", EmojiUtil.Get(EmojiDescriptor.Cry) == "😢");
        testLog.AddResult("Get(Angry) returns 😠", EmojiUtil.Get(EmojiDescriptor.Angry) == "😠");
        testLog.AddResult("Get(Party) returns 🥳", EmojiUtil.Get(EmojiDescriptor.Party) == "🥳");
        testLog.AddResult("Get(Rocket) returns 🚀", EmojiUtil.Get(EmojiDescriptor.Rocket) == "🚀");
        testLog.AddResult("Get(Hourglass) returns ⏳", EmojiUtil.Get(EmojiDescriptor.Hourglass) == "⏳");
        testLog.AddResult("Get(LightBulb) returns 💡", EmojiUtil.Get(EmojiDescriptor.LightBulb) == "💡");
        testLog.AddResult("Get(Bug) returns 🐛", EmojiUtil.Get(EmojiDescriptor.Bug) == "🐛");
        testLog.AddResult("Get(Hammer) returns 🔨", EmojiUtil.Get(EmojiDescriptor.Hammer) == "🔨");
        testLog.AddResult("Get(Package) returns 📦", EmojiUtil.Get(EmojiDescriptor.Package) == "📦");
        testLog.AddResult("Get(None) returns empty string", EmojiUtil.Get(EmojiDescriptor.None) == string.Empty);
    }

    private static void TestConcat(KoreTestLog testLog)
    {
        // Test concatenation of multiple emojis
        string result1 = EmojiUtil.Concat(EmojiDescriptor.Check, EmojiDescriptor.Cross, EmojiDescriptor.Warning);
        testLog.AddResult("Concat(Check, Cross, Warning) returns ✅❌⚠️", result1 == "✅❌⚠️");

        string result2 = EmojiUtil.Concat(EmojiDescriptor.Fire, EmojiDescriptor.Rocket);
        testLog.AddResult("Concat(Fire, Rocket) returns 🔥🚀", result2 == "🔥🚀");

        // Test empty cases
        testLog.AddResult("Concat() returns empty string", EmojiUtil.Concat() == string.Empty);
        testLog.AddResult("Concat(null) returns empty string", EmojiUtil.Concat(null!) == string.Empty);
        
        // Test with None
        string result3 = EmojiUtil.Concat(EmojiDescriptor.Check, EmojiDescriptor.None, EmojiDescriptor.Cross);
        testLog.AddResult("Concat(Check, None, Cross) returns ✅❌", result3 == "✅❌");
    }

    private static void TestPrefix(KoreTestLog testLog)
    {
        // Test prefixing message with emoji
        string result1 = EmojiUtil.Prefix("Success", EmojiDescriptor.Check);
        testLog.AddResult("Prefix('Success', Check) returns '✅ Success'", result1 == "✅ Success");

        string result2 = EmojiUtil.Prefix("Error occurred", EmojiDescriptor.Cross);
        testLog.AddResult("Prefix('Error occurred', Cross) returns '❌ Error occurred'", result2 == "❌ Error occurred");

        // Test custom separator
        string result3 = EmojiUtil.Prefix("Warning", EmojiDescriptor.Warning, ": ");
        testLog.AddResult("Prefix('Warning', Warning, ': ') returns '⚠️: Warning'", result3 == "⚠️: Warning");

        // Test edge cases
        testLog.AddResult("Prefix(null, Check) returns '✅'", EmojiUtil.Prefix(null!, EmojiDescriptor.Check) == "✅");
        testLog.AddResult("Prefix('', Check) returns '✅'", EmojiUtil.Prefix("", EmojiDescriptor.Check) == "✅");
        testLog.AddResult("Prefix('Message', None) returns 'Message'", EmojiUtil.Prefix("Message", EmojiDescriptor.None) == "Message");
    }

    private static void TestSuffix(KoreTestLog testLog)
    {
        // Test suffixing message with emoji
        string result1 = EmojiUtil.Suffix("Great job", EmojiDescriptor.ThumbsUp);
        testLog.AddResult("Suffix('Great job', ThumbsUp) returns 'Great job 👍'", result1 == "Great job 👍");

        string result2 = EmojiUtil.Suffix("Completed", EmojiDescriptor.Check);
        testLog.AddResult("Suffix('Completed', Check) returns 'Completed ✅'", result2 == "Completed ✅");

        // Test custom separator
        string result3 = EmojiUtil.Suffix("Done", EmojiDescriptor.Sparkles, "");
        testLog.AddResult("Suffix('Done', Sparkles, '') returns 'Done✨'", result3 == "Done✨");

        // Test edge cases
        testLog.AddResult("Suffix(null, Heart) returns '❤️'", EmojiUtil.Suffix(null!, EmojiDescriptor.Heart) == "❤️");
        testLog.AddResult("Suffix('', Heart) returns '❤️'", EmojiUtil.Suffix("", EmojiDescriptor.Heart) == "❤️");
        testLog.AddResult("Suffix('Message', None) returns 'Message'", EmojiUtil.Suffix("Message", EmojiDescriptor.None) == "Message");
    }
}
