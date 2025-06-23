namespace Craft.Security;

public record RefreshTokenRequest(string JwtToken, string RefreshToken, string ipAddress) : IRefreshTokenRequest;
