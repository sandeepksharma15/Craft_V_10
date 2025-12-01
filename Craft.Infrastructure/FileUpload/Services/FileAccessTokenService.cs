using System.Security.Cryptography;
using System.Text;

namespace Craft.Infrastructure.FileUpload;

/// <summary>
/// Simple implementation of file access token service using HMAC-SHA256.
/// </summary>
public class FileAccessTokenService : IFileAccessTokenService
{
    private readonly byte[] _secretKey;

    public FileAccessTokenService()
    {
        _secretKey = GenerateSecretKey();
    }

    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(string fileId, int expirationMinutes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileId);

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
        var payload = $"{fileId}|{expiresAt:O}";
        var signature = ComputeSignature(payload);
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{payload}|{signature}"));

        return (token, expiresAt);
    }

    public bool ValidateToken(string token, string fileId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileId);

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split('|');

            if (parts.Length != 3)
                return false;

            var tokenFileId = parts[0];
            var expiresAtStr = parts[1];
            var signature = parts[2];

            if (tokenFileId != fileId)
                return false;

            if (!DateTimeOffset.TryParse(expiresAtStr, out var expiresAt))
                return false;

            if (DateTimeOffset.UtcNow > expiresAt)
                return false;

            var payload = $"{tokenFileId}|{expiresAtStr}";
            var expectedSignature = ComputeSignature(payload);

            return signature == expectedSignature;
        }
        catch
        {
            return false;
        }
    }

    private string ComputeSignature(string payload)
    {
        using var hmac = new HMACSHA256(_secretKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }

    private static byte[] GenerateSecretKey()
    {
        var key = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);
        return key;
    }
}
