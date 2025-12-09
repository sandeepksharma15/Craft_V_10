namespace Craft.Security.Tests.Models;

public class CraftUserTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var user = new CraftUser<Guid>();
        Assert.True(user.IsActive);
        Assert.False(user.IsDeleted);
        Assert.Null(user.UserName);
        Assert.Null(user.Email);
        Assert.Null(user.FirstName);
        Assert.Null(user.LastName);
        Assert.Null(user.DialCode);
        Assert.Null(user.PhoneNumber);
        Assert.Null(user.PasswordHash);
        Assert.NotNull(user.SecurityStamp);
        Assert.True(Guid.TryParse(user.SecurityStamp, out _));
    }

    [Fact]
    public void ParameterizedConstructor_SetsProperties()
    {
        var user = new CraftUser<Guid>("user1", "mail@x.com", "John", "Smith", "+1", "123456", "pw");
        Assert.Equal("user1", user.UserName);
        Assert.Equal("mail@x.com", user.Email);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Smith", user.LastName);
        Assert.Equal("+1", user.DialCode);
        Assert.Equal("123456", user.PhoneNumber);
        Assert.Equal("pw", user.PasswordHash);
    }

    [Theory]
    [InlineData("John", "Smith", "John Smith")]
    [InlineData(null, "Smith", " Smith")]
    [InlineData("John", null, "John ")]
    [InlineData(null, null, " ")]
    public void FullName_ReturnsExpected(string? first, string? last, string expected)
    {
        var user = new CraftUser<Guid> { FirstName = first, LastName = last };
        Assert.Equal(expected, user.FullName);
    }

    [Fact]
    public void CanSetAndGet_DialCode()
    {
        var user = new CraftUser<Guid>
        {
            DialCode = "+44"
        };
        Assert.Equal("+44", user.DialCode);
    }

    [Fact]
    public void CanSetAndGet_ImageUrl()
    {
        var user = new CraftUser<Guid>
        {
            ImageUrl = "http://img.com/u.png"
        };
        Assert.Equal("http://img.com/u.png", user.ImageUrl);
    }
}
