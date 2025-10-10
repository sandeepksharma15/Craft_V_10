using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.Security;

/// <summary>
/// Represents the response object containing authentication information.
/// </summary>
public record JwtAuthResponse(
    [property: JsonPropertyName("jwtToken")] string JwtToken,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("refreshTokenExpiryTime")] DateTime RefreshTokenExpiryTime)
{
    public static JwtAuthResponse Empty
        => new(string.Empty, string.Empty, DateTime.MinValue);

    public bool IsEmpty
        => JwtToken.IsNullOrEmpty() && RefreshToken.IsNullOrEmpty() && RefreshTokenExpiryTime == DateTime.MinValue;

    public static JwtAuthResponse GetAuthResult(string jsonData)
    {
        if (jsonData.IsNullOrEmpty())
            return Empty;

        var authResult = JsonSerializer.Deserialize<JwtAuthResponse>(jsonData);

        return authResult ?? Empty;
    }

    public static JwtAuthResponse GetAuthResult(object jsonData)
    {
        return jsonData == null ? Empty : GetAuthResult(jsonData.ToString()!);
    }

    public RefreshTokenRequest ToRefreshTokenRequest(string ipAddress)
        => new(JwtToken, RefreshToken, ipAddress);
}
