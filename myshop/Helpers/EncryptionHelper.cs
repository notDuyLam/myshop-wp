using System;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;

namespace myshop.Helpers;

public static class EncryptionHelper
{
    private const string ProtectionDescriptor = "LOCAL=user";

    /// <summary>
    /// Mã hóa một chuỗi text
    /// </summary>
    public static async Task<string> EncryptAsync(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        var provider = new DataProtectionProvider(ProtectionDescriptor);
        var buffer = CryptographicBuffer.ConvertStringToBinary(plainText, BinaryStringEncoding.Utf8);
        var encryptedBuffer = await provider.ProtectAsync(buffer);
        return CryptographicBuffer.EncodeToBase64String(encryptedBuffer);
    }

    /// <summary>
    /// Giải mã một chuỗi đã được mã hóa
    /// </summary>
    public static async Task<string> DecryptAsync(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        try
        {
            var provider = new DataProtectionProvider(ProtectionDescriptor);
            var buffer = CryptographicBuffer.DecodeFromBase64String(encryptedText);
            var decryptedBuffer = await provider.UnprotectAsync(buffer);
            return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, decryptedBuffer);
        }
        catch
        {
            return string.Empty;
        }
    }
}

