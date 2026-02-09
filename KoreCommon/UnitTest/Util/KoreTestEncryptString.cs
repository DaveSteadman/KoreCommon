// <fileheader>

using System;
using System.Security.Cryptography;

using KoreCommon;
namespace KoreCommon.UnitTest;

#nullable enable

public static class KoreTestEncryptString
{
    // Usage: KoreTestEncryptString.RunTests(testLog);
    public static void RunTests(KoreTestLog testLog)
    {
        try
        {
            TestBasicEncryptionDecryption(testLog);
            TestEmptyStringHandling(testLog);
            TestSpecialCharacters(testLog);
            TestLargeStrings(testLog);
            TestWrongPassword(testLog);
            TestInvalidBase64(testLog);
            TestMultipleRoundTrips(testLog);
        }
        catch (Exception ex)
        {
            testLog.AddResult("KoreTestEncryptString RunTests // Exception: ", false, ex.Message);
            return;
        }
    }

    // Test basic encryption and decryption round-trip
    private static void TestBasicEncryptionDecryption(KoreTestLog testLog)
    {
        testLog.AddComment("=== Basic Encryption/Decryption Tests ===");

        string originalData = "Hello, World!";
        string password = "MySecretPassword123";

        string encrypted = KoreEncryptSting.Encrypt(originalData, password);
        testLog.AddComment($"Encrypted data (Base64): {encrypted.Substring(0, Math.Min(50, encrypted.Length))}...");

        string decrypted = KoreEncryptSting.Decrypt(encrypted, password);

        testLog.AddResult(
            "Basic round-trip: Original == Decrypted",
            originalData == decrypted,
            $"Original: '{originalData}', Decrypted: '{decrypted}'"
        );

        testLog.AddResult("Encrypted string is not empty", !string.IsNullOrEmpty(encrypted));
        testLog.AddResult("Encrypted string is different from original", encrypted != originalData);
    }

    // Test that empty strings throw appropriate exceptions
    private static void TestEmptyStringHandling(KoreTestLog testLog)
    {
        testLog.AddComment("=== Empty String Handling Tests ===");

        bool emptyDataThrows = false;
        try
        {
            KoreEncryptSting.Encrypt("", "password");
        }
        catch (ArgumentException)
        {
            emptyDataThrows = true;
        }
        testLog.AddResult("Empty data throws ArgumentException", emptyDataThrows);

        bool emptyPasswordThrows = false;
        try
        {
            KoreEncryptSting.Encrypt("data", "");
        }
        catch (ArgumentException)
        {
            emptyPasswordThrows = true;
        }
        testLog.AddResult("Empty password throws ArgumentException", emptyPasswordThrows);

        // bool nullDataThrows = false;
        // try
        // {
        //     KoreEncryptSting.Encrypt(null, "password");
        // }
        // catch (ArgumentNullException)
        // {
        //     nullDataThrows = true;
        // }
        // testLog.AddResult("Null data throws ArgumentNullException", nullDataThrows);

        // bool nullPasswordThrows = false;
        // try
        // {
        //     KoreEncryptSting.Encrypt("data", null);
        // }
        // catch (ArgumentNullException)
        // {
        //     nullPasswordThrows = true;
        // }
        // testLog.AddResult("Null password throws ArgumentNullException", nullPasswordThrows);
    }

    // Test encryption/decryption with special characters
    private static void TestSpecialCharacters(KoreTestLog testLog)
    {
        testLog.AddComment("=== Special Characters Tests ===");

        string[] testStrings = new string[]
        {
            "Unicode: 你好世界 🌍 🎉",
            "Symbols: !@#$%^&*()_+-=[]{}|;:',.<>?",
            "Newlines:\nLine1\nLine2\nLine3",
            "Tabs:\tTabbed\tData\tHere",
            "Mixed: \"Quote's\" and (parens) & symbols!"
        };

        string password = "TestPassword456";

        foreach (string original in testStrings)
        {
            string encrypted = KoreEncryptSting.Encrypt(original, password);
            string decrypted = KoreEncryptSting.Decrypt(encrypted, password);

            testLog.AddResult(
                $"Special chars round-trip: {original.Substring(0, Math.Min(20, original.Length))}...",
                original == decrypted
            );
        }
    }

    // Test with larger strings
    private static void TestLargeStrings(KoreTestLog testLog)
    {
        testLog.AddComment("=== Large String Tests ===");

        // Create a large string (10KB)
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 1000; i++)
            sb.AppendLine($"This is line {i} of test data with some content to make it realistic.");

        string largeData = sb.ToString();

        string password = "LargeDataPassword789";
        string encrypted = KoreEncryptSting.Encrypt(largeData, password);
        string decrypted = KoreEncryptSting.Decrypt(encrypted, password);

        testLog.AddResult(
            $"Large string round-trip ({largeData.Length} chars)",
            largeData == decrypted,
            $"Original length: {largeData.Length}, Decrypted length: {decrypted.Length}"
        );
    }

    // Test that wrong password fails decryption
    private static void TestWrongPassword(KoreTestLog testLog)
    {
        testLog.AddComment("=== Wrong Password Tests ===");

        string originalData = "Sensitive Data";
        string correctPassword = "CorrectPassword";
        string wrongPassword = "WrongPassword";

        string encrypted = KoreEncryptSting.Encrypt(originalData, correctPassword);

        bool wrongPasswordThrows = false;
        try
        {
            string decrypted = KoreEncryptSting.Decrypt(encrypted, wrongPassword);
        }
        catch (CryptographicException)
        {
            wrongPasswordThrows = true;
        }
        testLog.AddResult("Wrong password throws CryptographicException", wrongPasswordThrows);

        // Test with correct password still works
        string correctDecrypted = KoreEncryptSting.Decrypt(encrypted, correctPassword);
        testLog.AddResult(
            "Correct password after wrong attempt succeeds",
            originalData == correctDecrypted
        );
    }

    // Test that invalid Base64 throws appropriate exception
    private static void TestInvalidBase64(KoreTestLog testLog)
    {
        testLog.AddComment("=== Invalid Base64 Tests ===");

        bool invalidBase64Throws = false;
        try
        {
            KoreEncryptSting.Decrypt("This is not valid Base64!@#$", "password");
        }
        catch (ArgumentException)
        {
            invalidBase64Throws = true;
        }
        testLog.AddResult("Invalid Base64 throws ArgumentException", invalidBase64Throws);

        bool tooShortDataThrows = false;
        try
        {
            // Valid Base64 but too short to contain salt + IV
            KoreEncryptSting.Decrypt("YWJjZA==", "password");
        }
        catch (ArgumentException)
        {
            tooShortDataThrows = true;
        }
        testLog.AddResult("Too-short encrypted data throws ArgumentException", tooShortDataThrows);
    }

    // Test multiple encryption/decryption cycles
    private static void TestMultipleRoundTrips(KoreTestLog testLog)
    {
        testLog.AddComment("=== Multiple Round-Trip Tests ===");

        string originalData = "Data for multiple round trips";
        string password = "MultiRoundPassword";

        // Encrypt and decrypt 5 times in a row
        string currentData = originalData;
        for (int i = 0; i < 5; i++)
        {
            string encrypted = KoreEncryptSting.Encrypt(currentData, password);
            currentData = KoreEncryptSting.Decrypt(encrypted, password);
        }

        testLog.AddResult(
            "Multiple round-trips (5x): Original == Final",
            originalData == currentData
        );

        // Test that same data encrypted twice produces different ciphertext (due to random IV/salt)
        string encrypted1 = KoreEncryptSting.Encrypt(originalData, password);
        string encrypted2 = KoreEncryptSting.Encrypt(originalData, password);

        testLog.AddResult(
            "Same data encrypted twice produces different ciphertext",
            encrypted1 != encrypted2,
            "This ensures random IV and salt are being used"
        );

        // But both should decrypt to the same original data
        string decrypted1 = KoreEncryptSting.Decrypt(encrypted1, password);
        string decrypted2 = KoreEncryptSting.Decrypt(encrypted2, password);

        testLog.AddResult(
            "Both ciphertexts decrypt to original data",
            decrypted1 == originalData && decrypted2 == originalData
        );
    }
}
