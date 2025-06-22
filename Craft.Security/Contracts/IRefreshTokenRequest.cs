namespace Craft.Security.Contracts;

public interface IRefreshTokenRequest
{
    string ipAddress { get; init; }
    string JwtToken { get; init; }
    string RefreshToken { get; init; }

    void Deconstruct(out string JwtToken, out string RefreshToken, out string ipAddress);
    bool Equals(object? obj);
    bool Equals(RefreshTokenRequest? other);
    int GetHashCode();
    string ToString();
}
