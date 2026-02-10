using Microsoft.Data.SqlClient;

namespace Craft.Data.Tests.ConnectionStringHandlers;

public class MsSqlConnectionStringHandlerTests
{
    private const string Mask = "*******";
    private readonly SqlServerConnectionStringHandler _sut = new();

    // ---------- Build tests ----------

    [Fact]
    public void Build_Should_Return_Normalized_ConnectionString()
    {
        // Arrange
        var input = "Server=.;Database=TestDb;Integrated Security=true;";

        // Act
        var result = _sut.Build(input);

        // Assert
        var builder = new SqlConnectionStringBuilder(result);
        Assert.Equal(".", builder.DataSource);
        Assert.Equal("TestDb", builder.InitialCatalog);
        // Re-building from builder should produce identical string (normalization check)
        Assert.Equal(builder.ConnectionString, result);
    }

    [Fact]
    public void Build_Should_Throw_ArgumentException_When_Invalid_ConnectionString()
    {
        var input = "Server==;Database";
        var ex = Assert.Throws<ArgumentException>(() => _sut.Build(input));
        Assert.Contains("Invalid SQL Server connection string", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Build_Should_Throw_ArgumentException_When_NullOrWhitespace(string? cs)
        => Assert.ThrowsAny<ArgumentException>(() => _sut.Build(cs!));

    // ---------- Mask tests ----------

    [Fact]
    public void Mask_Should_Not_Mask_When_IntegratedSecurity_And_No_Credentials()
    {
        var cs = "Server=.;Database=TestDb;Integrated Security=true;";
        var masked = _sut.Mask(cs);
        var builder = new SqlConnectionStringBuilder(masked);
        Assert.True(builder.IntegratedSecurity);
        Assert.True(string.IsNullOrEmpty(builder.UserID));
        Assert.True(string.IsNullOrEmpty(builder.Password));
        Assert.DoesNotContain(Mask, masked);
    }

    [Fact]
    public void Mask_Should_Not_Mask_Password_When_IntegratedSecurity()
    {
        var cs = "Server=.;Database=TestDb;Integrated Security=true;Password=ActualPwd;";
        var masked = _sut.Mask(cs);
        var builder = new SqlConnectionStringBuilder(masked);
        Assert.Equal("ActualPwd", builder.Password);
        Assert.DoesNotContain(Mask, masked);
    }

    [Fact]
    public void Mask_Should_Not_Mask_UserId_When_IntegratedSecurity()
    {
        var cs = "Server=.;Database=TestDb;Integrated Security=true;User ID=SomeUser;";
        var masked = _sut.Mask(cs);
        var builder = new SqlConnectionStringBuilder(masked);
        Assert.Equal("SomeUser", builder.UserID);
        Assert.DoesNotContain(Mask, masked);
    }

    [Fact]
    public void Mask_Should_Mask_UserId_And_Password_For_SqlAuth()
    {
        var cs = "Server=.;Database=TestDb;User ID=AppUser;Password=Secret;";
        var masked = _sut.Mask(cs);
        var builder = new SqlConnectionStringBuilder(masked);
        Assert.Equal(Mask, builder.UserID);
        Assert.Equal(Mask, builder.Password);
    }

    [Fact]
    public void Mask_Should_Mask_Password_When_Password_Empty_For_SqlAuth()
    {
        var cs = "Server=.;Database=TestDb;User ID=AppUser;Password=;";
        var masked = _sut.Mask(cs);
        var builder = new SqlConnectionStringBuilder(masked);
        Assert.Equal(Mask, builder.Password);
        Assert.Equal(Mask, builder.UserID);
    }

    [Fact]
    public void Mask_Should_Return_Original_When_Invalid_ConnectionString()
    {
        var cs = "Server==;Database";
        var masked = _sut.Mask(cs);
        Assert.Equal(cs, masked);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Mask_Should_Throw_For_NullOrWhitespace(string? cs)
        => Assert.ThrowsAny<ArgumentException>(() => _sut.Mask(cs!));

    // ---------- Validate tests ----------

    [Fact]
    public void Validate_Should_Return_True_For_Valid_ConnectionString()
    {
        var cs = "Server=.;Database=TestDb;Integrated Security=true;";
        var isValid = _sut.Validate(cs);
        Assert.True(isValid);
    }

    [Fact]
    public void Validate_Should_Return_False_For_Invalid_ConnectionString()
    {
        var cs = "Server==;Database";
        var isValid = _sut.Validate(cs);
        Assert.False(isValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_Should_Throw_For_NullOrWhitespace(string? cs)
        => Assert.ThrowsAny<ArgumentException>(() => _sut.Validate(cs!));
}

