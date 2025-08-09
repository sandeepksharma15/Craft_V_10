using System.Security.Cryptography;

namespace Craft.Utilities.Services;

public class KeySafeService : IKeySafeService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public KeySafeService()
    {
        string keyString = Environment.GetEnvironmentVariable("AES_ENCRYPTION_KEY")
             ?? throw new InvalidOperationException("Encryption Key not found");
        string ivString = Environment.GetEnvironmentVariable("AES_ENCRYPTION_IV")
            ?? throw new InvalidOperationException("Encryption IV not found");

        _key = Convert.FromBase64String(keyString);
        _iv = Convert.FromBase64String(ivString);

        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption Key must be 32 bytes for AES-256");

        if (_iv.Length != 16)
            throw new InvalidOperationException("Encryption IV must be 16 bytes");
    }

    public string Decrypt(string cipherText)
        => Decrypt(cipherText, _key, _iv);

    public string Encrypt(string plainText)
        => Encrypt(plainText, _key, _iv);

    public static string Encrypt(string plainText, byte[] key, byte[] iv)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty.", nameof(plainText));

        if (key == null || key.Length != 32)
            throw new ArgumentException("Encryption Key must be 32 bytes for AES-256.", nameof(key));

        if (iv == null || iv.Length != 16)
            throw new ArgumentException("Encryption IV must be 16 bytes.", nameof(iv));

        try
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using (var writer = new StreamWriter(cs))
            {
                writer.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Encryption failed. The provided key and IV may be incorrect.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred during encryption.", ex);
        }
    }

    public static string Decrypt(string cipherText, byte[] key, byte[] iv)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("Cipher text cannot be null or empty.", nameof(cipherText));

        if (key == null || key.Length != 32)
            throw new ArgumentException("Encryption Key must be 32 bytes for AES-256.", nameof(key));

        if (iv == null || iv.Length != 16)
            throw new ArgumentException("Encryption IV must be 16 bytes.", nameof(iv));

        try
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);

            return reader.ReadToEnd();
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Cipher text is not in a valid Base64 format.", nameof(cipherText), ex);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Decryption failed. The provided key and IV may be incorrect.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred during decryption.", ex);
        }
    }

    public string GetIV() => Convert.ToBase64String(_iv);

    public string GetKey() => Convert.ToBase64String(_key);
}

public interface IKeySafeService
{
    string GetKey();
    string GetIV();

    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
