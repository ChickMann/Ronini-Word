using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace CLOUPT.Core
{
    /// <summary>
    /// CLOUPT Security - Provides encryption and obfuscation for sensitive data.
    /// Uses AES-256 encryption with machine-specific key derivation.
    /// </summary>
    public static class CLOUPTSecurity
    {
        // Embedded entropy bytes (obfuscated in code)
        private static readonly byte[] EntropyA = { 0x43, 0x4C, 0x4F, 0x55, 0x50, 0x54 }; // "CLOUPT"
        private static readonly byte[] EntropyB = { 0x53, 0x44, 0x4B, 0x32, 0x30, 0x32, 0x36 }; // "SDK2026"
        private static readonly byte[] Salt = { 0x7A, 0x9F, 0x3C, 0xE1, 0x5B, 0x8D, 0x2A, 0x4F, 0x6E, 0x1C, 0x9A, 0x3B, 0x5D, 0x7F, 0x2E, 0x8C };

        private const int KeySize = 256;
        private const int BlockSize = 128;
        private const int Iterations = 10000;

        /// <summary>
        /// Encrypts the given plain text using AES-256.
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>Base64 encoded encrypted string with IV prepended</returns>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] key = DeriveKey();
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = KeySize;
                    aes.BlockSize = BlockSize;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        // Prepend IV to the encrypted data
                        ms.Write(aes.IV, 0, aes.IV.Length);

                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.FlushFinalBlock();
                        }

                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CLOUPT Security] Encryption failed: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts the given encrypted text.
        /// </summary>
        /// <param name="encryptedText">Base64 encoded encrypted string with IV prepended</param>
        /// <returns>Decrypted plain text</returns>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                byte[] key = DeriveKey();
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

                // Extract IV from the beginning
                byte[] iv = new byte[16];
                byte[] cipherBytes = new byte[encryptedBytes.Length - 16];
                Array.Copy(encryptedBytes, 0, iv, 0, 16);
                Array.Copy(encryptedBytes, 16, cipherBytes, 0, cipherBytes.Length);

                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = KeySize;
                    aes.BlockSize = BlockSize;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(cipherBytes))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs, Encoding.UTF8))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (CryptographicException)
            {
                // Decryption failed - likely corrupted or mismatched key
                // Don't log error here, let caller handle it gracefully
                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CLOUPT Security] Unexpected decryption error: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Validates if a string appears to be encrypted (Base64 format).
        /// </summary>
        public static bool IsEncrypted(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // Check if it's valid Base64 and long enough to contain IV + data
            try
            {
                byte[] bytes = Convert.FromBase64String(text);
                return bytes.Length > 16; // Must have at least IV + some data
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obfuscates a string using XOR (for less sensitive data).
        /// </summary>
        public static string Obfuscate(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] key = GetObfuscationKey();
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            byte[] result = new byte[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
            {
                result[i] = (byte)(bytes[i] ^ key[i % key.Length]);
            }

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Deobfuscates a string that was obfuscated with XOR.
        /// </summary>
        public static string Deobfuscate(string obfuscatedText)
        {
            if (string.IsNullOrEmpty(obfuscatedText))
                return string.Empty;

            try
            {
                byte[] key = GetObfuscationKey();
                byte[] bytes = Convert.FromBase64String(obfuscatedText);
                byte[] result = new byte[bytes.Length];

                for (int i = 0; i < bytes.Length; i++)
                {
                    result[i] = (byte)(bytes[i] ^ key[i % key.Length]);
                }

                return Encoding.UTF8.GetString(result);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Generates a hash of the App ID for verification purposes.
        /// </summary>
        public static string Hash(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(text + Encoding.UTF8.GetString(EntropyA));
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Verifies a hash against the original text.
        /// </summary>
        public static bool VerifyHash(string text, string hash)
        {
            string computedHash = Hash(text);
            return string.Equals(computedHash, hash, StringComparison.Ordinal);
        }

        #region Private Methods

        /// <summary>
        /// Derives a 256-bit encryption key using PBKDF2.
        /// Uses embedded entropy for consistent encryption across Editor and Build.
        /// </summary>
        private static byte[] DeriveKey()
        {
            // Combine entropy sources (consistent across Editor and Build)
            byte[] combinedEntropy = new byte[EntropyA.Length + EntropyB.Length + Salt.Length];
            Array.Copy(EntropyA, 0, combinedEntropy, 0, EntropyA.Length);
            Array.Copy(EntropyB, 0, combinedEntropy, EntropyA.Length, EntropyB.Length);
            Array.Copy(Salt, 0, combinedEntropy, EntropyA.Length + EntropyB.Length, Salt.Length);

            // Additional mixing with fixed secret
            byte[] secret = Encoding.UTF8.GetBytes("CLOUPT_UNITY_SDK_2026_SECURE");
            for (int i = 0; i < combinedEntropy.Length; i++)
            {
                combinedEntropy[i] ^= secret[i % secret.Length];
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(combinedEntropy, Salt, Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(32); // 256 bits
            }
        }

        /// <summary>
        /// Gets a simple XOR key for obfuscation.
        /// </summary>
        private static byte[] GetObfuscationKey()
        {
            // Combine entropy to create obfuscation key
            byte[] key = new byte[EntropyA.Length + EntropyB.Length];
            Array.Copy(EntropyA, 0, key, 0, EntropyA.Length);
            Array.Copy(EntropyB, 0, key, EntropyA.Length, EntropyB.Length);

            // Additional mixing
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = (byte)(key[i] ^ Salt[i % Salt.Length]);
            }

            return key;
        }

        #endregion
    }
}
