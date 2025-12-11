using Craft.Domain;

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

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        // Arrange
        var user = new CraftUser<Guid> { IsActive = false };

        // Act
        user.Activate();

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var user = new CraftUser<Guid> { IsActive = true };

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
    }

    [Fact]
    public void Gender_CanBeSetAndRetrieved()
    {
        // Arrange
        var user = new CraftUser<Guid>();

        // Act
        user.Gender = GenderType.Male;

        // Assert
        Assert.Equal(GenderType.Male, user.Gender);
    }

    [Fact]
    public void Title_CanBeSetAndRetrieved()
    {
        // Arrange
        var user = new CraftUser<Guid>();

        // Act
        user.Title = HonorificType.Mr;

        // Assert
        Assert.Equal(HonorificType.Mr, user.Title);
    }

    [Fact]
    public void NonGenericCraftUser_WorksWithKeyType()
    {
        // Arrange & Act
        var user = new CraftUser
        {
            Id = 123,
            UserName = "testuser",
            Email = "test@example.com"
        };

        // Assert
        Assert.Equal(123, user.Id);
        Assert.Equal("testuser", user.UserName);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public void IsDeleted_CanBeSetAndRetrieved()
    {
        // Arrange
        var user = new CraftUser<Guid>();

        // Act
        user.IsDeleted = true;

        // Assert
        Assert.True(user.IsDeleted);
    }

    [Fact]
    public void AllProperties_CanBeSetAndRetrieved()
    {
        // Arrange
        var user = new CraftUser<Guid>
        {
            UserName = "johndoe",
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe",
            DialCode = "+1",
            PhoneNumber = "5551234567",
            Gender = GenderType.Male,
            Title = HonorificType.Mr,
            ImageUrl = "https://example.com/image.jpg",
            IsActive = true,
            IsDeleted = false
        };

        // Assert
        Assert.Equal("johndoe", user.UserName);
        Assert.Equal("john@example.com", user.Email);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("+1", user.DialCode);
        Assert.Equal("5551234567", user.PhoneNumber);
        Assert.Equal(GenderType.Male, user.Gender);
        Assert.Equal(HonorificType.Mr, user.Title);
        Assert.Equal("https://example.com/image.jpg", user.ImageUrl);
        Assert.True(user.IsActive);
        Assert.False(user.IsDeleted);
        Assert.Equal("John Doe", user.FullName);
    }
}
