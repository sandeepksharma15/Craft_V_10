using Npgsql;

namespace Craft.Data.Tests.ConnectionStringHandlers;

public class PostgreSqlConnectionStringHandlerTests
{
    private const string Mask = "*******";
    private readonly PostgreSqlConnectionStringHandler _sut = new();

    // ---------- Build tests ----------

    [Fact]
    public void Build_Should_Return_Normalized_ConnectionString()
    {
        // Arrange
        var input = "Host=localhost;Database=TestDb;Username=app;Password=secret;";

        // Act
        var result = _sut.Build(input);

        // Assert
        var builder = new NpgsqlConnectionStringBuilder(result);
        Assert.Equal("localhost", builder.Host);
        Assert.Equal("TestDb", builder.Database);
        Assert.Equal("app", builder.Username);
        Assert.Equal("secret", builder.Password);
        Assert.Equal(builder.ConnectionString, result);
    }

    [Fact]
    public void Build_Should_Throw_ArgumentException_When_Invalid_ConnectionString()
    {
        var input = "Host==localhost;Database";
        var ex = Assert.Throws<ArgumentException>(() => _sut.Build(input));
        Assert.Contains("Invalid PostgreSQL connection string", ex.Message);
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
    public void Mask_Should_Mask_Username_And_Password_When_Present()
    {
        var cs = "Host=localhost;Database=TestDb;Username=app;Password=SecretPwd;";
        var masked = _sut.Mask(cs);
        var builder = new NpgsqlConnectionStringBuilder(masked);
        Assert.Equal(Mask, builder.Username);
        Assert.Equal(Mask, builder.Password);
    }

    [Fact]
    public void Mask_Should_Mask_Password_Even_When_Username_Empty()
    {
        var cs = "Host=localhost;Database=TestDb;Password=SecretPwd;";
        var masked = _sut.Mask(cs);
        var builder = new NpgsqlConnectionStringBuilder(masked);
        Assert.True(string.IsNullOrEmpty(builder.Username));
        Assert.Equal(Mask, builder.Password);
    }

    [Fact]
    public void Mask_Should_Add_Masked_Password_When_No_Credentials_Present()
    {
        var cs = "Host=localhost;Database=TestDb;";
        var masked = _sut.Mask(cs);
        var builder = new NpgsqlConnectionStringBuilder(masked);
        Assert.True(string.IsNullOrEmpty(builder.Username));
        Assert.Equal(Mask, builder.Password); // Always enforced
    }

    [Fact]
    public void Mask_Should_Mask_Password_When_Password_Empty()
    {
        var cs = "Host=localhost;Database=TestDb;Username=app;Password=;";
        var masked = _sut.Mask(cs);
        var builder = new NpgsqlConnectionStringBuilder(masked);
        Assert.Equal(Mask, builder.Username);
        Assert.Equal(Mask, builder.Password);
    }

    [Fact]
    public void Mask_Should_Return_Original_When_Invalid_ConnectionString()
    {
        var cs = "Host==localhost;Database";
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
        var cs = "Host=localhost;Database=TestDb;Username=app;Password=secret;";
        var isValid = _sut.Validate(cs);
        Assert.True(isValid);
    }

    [Fact]
    public void Validate_Should_Return_False_For_Invalid_ConnectionString()
    {
        var cs = "Host==localhost;Database";
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
