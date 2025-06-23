namespace Craft.Security.Tests.Models;

public class RefreshTokenRequestTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var req = new RefreshTokenRequest("jwt", "refresh", "1.2.3.4");
        Assert.Equal("jwt", req.JwtToken);
        Assert.Equal("refresh", req.RefreshToken);
        Assert.Equal("1.2.3.4", req.ipAddress);
    }

    [Fact]
    public void Deconstruct_ReturnsExpectedValues()
    {
        var req = new RefreshTokenRequest("jwt", "refresh", "ip");
        req.Deconstruct(out var jwt, out var refresh, out var ip);
        Assert.Equal("jwt", jwt);
        Assert.Equal("refresh", refresh);
        Assert.Equal("ip", ip);
    }

    [Fact]
    public void Equality_MembersWithSameValues_AreEqual()
    {
        var a = new RefreshTokenRequest("jwt", "refresh", "ip");
        var b = new RefreshTokenRequest("jwt", "refresh", "ip");
        Assert.Equal(a, b);
        Assert.True(a.Equals(b));
        Assert.True(a.Equals((object)b));
    }

    [Fact]
    public void Inequality_MembersWithDifferentValues_AreNotEqual()
    {
        var a = new RefreshTokenRequest("jwt", "refresh", "ip");
        var b = new RefreshTokenRequest("jwt2", "refresh", "ip");
        var c = new RefreshTokenRequest("jwt", "refresh2", "ip");
        var d = new RefreshTokenRequest("jwt", "refresh", "ip2");
        Assert.NotEqual(a, b);
        Assert.NotEqual(a, c);
        Assert.NotEqual(a, d);
    }

    [Fact]
    public void GetHashCode_SameForEqualObjects()
    {
        var a = new RefreshTokenRequest("jwt", "refresh", "ip");
        var b = new RefreshTokenRequest("jwt", "refresh", "ip");
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_ContainsPropertyValues()
    {
        var req = new RefreshTokenRequest("jwt", "refresh", "ip");
        var str = req.ToString();
        Assert.Contains("jwt", str);
        Assert.Contains("refresh", str);
        Assert.Contains("ip", str);
    }
}
