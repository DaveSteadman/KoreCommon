using System;
using System.Security.Cryptography;
using System.Text;

public static class KoreEncryptSting
{
    private const int KeySizeBits = 256;
    private const int IvSizeBytes = 16; // AES block size is always 16 bytes
    private const int Pbkdf2Iterations = 100_000;
    private const int SaltSizeBytes = 16;

    // Encrypts a UTF-8 string using a password, returning a printable Base64 string.
    // - data: The plaintext string to encrypt (UTF-8).
    // - password: The password used for encryption.
    // - Returns: Base64-encoded encrypted string (printable ASCII/UTF-8).
    // - Throws ArgumentNullException if data or password is null.
    // - Throws ArgumentException if data or password is empty.
    public static string Encrypt(string data, string password)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (password == null) throw new ArgumentNullException(nameof(password));
        if (data.Length == 0) throw new ArgumentException("Data cannot be empty.", nameof(data));
        if (password.Length == 0) throw new ArgumentException("Password cannot be empty.", nameof(password));

        // Generate a random salt and IV
        var saltBytes = new byte[SaltSizeBytes];
        var ivBytes = new byte[IvSizeBytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        rng.GetBytes(ivBytes);

        // Derive key using PBKDF2
        byte[] key;
        using (var deriveBytes = new Rfc2898DeriveBytes(password, saltBytes, Pbkdf2Iterations, HashAlgorithmName.SHA256))
        {
            key = deriveBytes.GetBytes(KeySizeBits / 8);
        }

        // Encrypt using AES-CBC
        using var aes = Aes.Create();
        aes.KeySize = KeySizeBits;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = ivBytes;

        byte[] encryptedBytes;
        using (var encryptor = aes.CreateEncryptor())
        using (var ms = new System.IO.MemoryStream())
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var writer = new System.IO.StreamWriter(cs, new UTF8Encoding(false)))
        {
            writer.Write(data);
            cs.FlushFinalBlock();
            encryptedBytes = ms.ToArray();
        }

        // Combine salt + IV + ciphertext into one byte array
        var combined = new byte[SaltSizeBytes + IvSizeBytes + encryptedBytes.Length];
        Buffer.BlockCopy(saltBytes, 0, combined, 0, SaltSizeBytes);
        Buffer.BlockCopy(ivBytes, 0, combined, SaltSizeBytes, IvSizeBytes);
        Buffer.BlockCopy(encryptedBytes, 0, combined, SaltSizeBytes + IvSizeBytes, encryptedBytes.Length);

        return Convert.ToBase64String(combined);
    }

    // Decrypts a Base64-encoded encrypted string (produced by Encrypt) using a password.
    // - encryptedData: The Base64 string to decrypt.
    // - password: The password used for decryption.
    // - Returns: The decrypted UTF-8 string.
    // - Throws ArgumentNullException if encryptedData or password is null.
    // - Throws ArgumentException if encryptedData or password is empty.
    // - Throws CryptographicException on decryption failure (e.g., wrong password).
    public static string Decrypt(string encryptedData, string password)
    {
        if (encryptedData == null) throw new ArgumentNullException(nameof(encryptedData));
        if (password == null) throw new ArgumentNullException(nameof(password));
        if (encryptedData.Length == 0) throw new ArgumentException("Encrypted data cannot be empty.", nameof(encryptedData));

        byte[] combined;
        try
        {
            combined = Convert.FromBase64String(encryptedData);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Encrypted data is not a valid Base64 string.", nameof(encryptedData), ex);
        }

        if (combined.Length < SaltSizeBytes + IvSizeBytes)
        {
            throw new ArgumentException("Encrypted data is too short to contain salt and IV.", nameof(encryptedData));
        }

        // Extract salt, IV, and ciphertext
        var saltBytes = new byte[SaltSizeBytes];
        var ivBytes = new byte[IvSizeBytes];
        var encryptedBytes = new byte[combined.Length - SaltSizeBytes - IvSizeBytes];

        Buffer.BlockCopy(combined, 0, saltBytes, 0, SaltSizeBytes);
        Buffer.BlockCopy(combined, SaltSizeBytes, ivBytes, 0, IvSizeBytes);
        Buffer.BlockCopy(combined, SaltSizeBytes + IvSizeBytes, encryptedBytes, 0, encryptedBytes.Length);

        // Derive key using PBKDF2 (same parameters as encryption)
        byte[] key;
        using (var deriveBytes = new Rfc2898DeriveBytes(password, saltBytes, Pbkdf2Iterations, HashAlgorithmName.SHA256))
        {
            key = deriveBytes.GetBytes(KeySizeBits / 8);
        }

        // Decrypt using AES-CBC
        using var aes = Aes.Create();
        aes.KeySize = KeySizeBits;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = ivBytes;

        string decrypted;
        using (var decryptor = aes.CreateDecryptor())
        using (var ms = new System.IO.MemoryStream(encryptedBytes))
        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        using (var reader = new System.IO.StreamReader(cs, new UTF8Encoding(false)))
        {
            decrypted = reader.ReadToEnd();
        }

        return decrypted;
    }
}
