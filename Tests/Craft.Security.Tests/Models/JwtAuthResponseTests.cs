using System.Text.Json;

namespace Craft.Security.Tests.Models;

public class JwtAuthResponseTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var dt = new DateTime(2024, 1, 1, 12, 0, 0);
        var jwt = new JwtAuthResponse("jwt", "refresh", dt);
        Assert.Equal("jwt", jwt.JwtToken);
        Assert.Equal("refresh", jwt.RefreshToken);
        Assert.Equal(dt, jwt.RefreshTokenExpiryTime);
    }

    [Fact]
    public void Empty_ReturnsEmptyInstance()
    {
        var empty = JwtAuthResponse.Empty;
        Assert.Equal(string.Empty, empty.JwtToken);
        Assert.Equal(string.Empty, empty.RefreshToken);
        Assert.Equal(DateTime.MinValue, empty.RefreshTokenExpiryTime);
        Assert.True(empty.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalse_WhenAnyFieldIsNotEmpty()
    {
        var dt = DateTime.UtcNow;
        Assert.False(new JwtAuthResponse("jwt", "", DateTime.MinValue).IsEmpty);
        Assert.False(new JwtAuthResponse("", "refresh", DateTime.MinValue).IsEmpty);
        Assert.False(new JwtAuthResponse("", "", dt).IsEmpty);
    }

    [Fact]
    public void GetAuthResult_ParsesValidJson()
    {
        var dt = new DateTime(2024, 1, 1, 12, 0, 0);
        var json = $"{{\"jwtToken\":\"jwt\",\"refreshToken\":\"refresh\",\"refreshTokenExpiryTime\":\"{dt:O}\"}}";
        var result = JwtAuthResponse.GetAuthResult(json);
        Assert.Equal("jwt", result.JwtToken);
        Assert.Equal("refresh", result.RefreshToken);
        Assert.Equal(dt, result.RefreshTokenExpiryTime);
    }

    [Fact]
    public void GetAuthResult_ReturnsEmpty_OnNullOrEmptyJson()
    {
        Assert.True(JwtAuthResponse.GetAuthResult((string)null!).IsEmpty);
        Assert.True(JwtAuthResponse.GetAuthResult("").IsEmpty);
    }

    [Fact]
    public void GetAuthResult_ReturnsEmpty_OnInvalidJson()
    {
        Assert.Throws<JsonException>(() => JwtAuthResponse.GetAuthResult("not a json"));
    }

    [Fact]
    public void GetAuthResult_ObjectOverload_ParsesStringifiedJson()
    {
        var dt = new DateTime(2024, 1, 1, 12, 0, 0);
        var json = $"{{\"jwtToken\":\"jwt\",\"refreshToken\":\"refresh\",\"refreshTokenExpiryTime\":\"{dt:O}\"}}";
        object obj = json;
        var result = JwtAuthResponse.GetAuthResult(obj);
        Assert.Equal("jwt", result.JwtToken);
        Assert.Equal("refresh", result.RefreshToken);
        Assert.Equal(dt, result.RefreshTokenExpiryTime);
    }

    [Fact]
    public void GetAuthResult_ObjectOverload_ReturnsEmpty_OnNull()
    {
        Assert.True(JwtAuthResponse.GetAuthResult((object)null!).IsEmpty);
    }

    [Fact]
    public void ToRefreshTokenRequest_ReturnsExpectedRequest()
    {
        var jwt = new JwtAuthResponse("jwt", "refresh", DateTime.UtcNow);
        var req = jwt.ToRefreshTokenRequest("1.2.3.4");
        Assert.Equal("jwt", req.JwtToken);
        Assert.Equal("refresh", req.RefreshToken);
        Assert.Equal("1.2.3.4", req.ipAddress);
    }
}
