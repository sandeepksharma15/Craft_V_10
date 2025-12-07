namespace Craft.Configuration.Abstractions;

/// <summary>
/// Provides methods for encrypting and decrypting configuration values.
/// </summary>
public interface IConfigurationEncryption
{
    /// <summary>
    /// Encrypts a plain text value.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted value.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted value.
    /// </summary>
    /// <param name="encryptedText">The encrypted text to decrypt.</param>
    /// <returns>The decrypted plain text value.</returns>
    string Decrypt(string encryptedText);

    /// <summary>
    /// Determines if a value is encrypted based on the configured prefix.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is encrypted; otherwise, false.</returns>
    bool IsEncrypted(string? value);

    /// <summary>
    /// Gets the encryption prefix used to identify encrypted values.
    /// </summary>
    string EncryptionPrefix { get; }
}
