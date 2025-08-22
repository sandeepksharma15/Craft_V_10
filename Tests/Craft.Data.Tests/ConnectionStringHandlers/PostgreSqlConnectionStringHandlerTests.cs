using System;
using Npgsql;

namespace Craft.Data.Tests.ConnectionStringHandlers;

public class PostgreSqlConnectionStringHandlerTests
{
    private const string Mask = "*******";
    private readonly PostgreSqlConnectionStringHandler _sut = new();

    // ---------- Build tests ----------

    [Fact]
    public void Build_Should_Set_Timeout_When_Not_Present_And_CommandTimeout_Positive()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            ConnectionString = "Host=localhost;Database=TestDb;Username=app;Password=secret;",
            CommandTimeout = 77
        };

        // Act
        var result = _sut.Build(options);

        // Assert
        var builder = new NpgsqlConnectionStringBuilder(result);
        Assert.Equal(77, builder.Timeout);
        Assert.Equal("localhost", builder.Host);
        Assert.Equal("TestDb", builder.Database);
    }

    [Fact]
    public void Build_Should_Override_Existing_Timeout_When_CommandTimeout_Positive()
    {
        var options = new DatabaseOptions
        {
            ConnectionString = "Host=localhost;Database=TestDb;Timeout=5;Username=app;Password=secret;",
            CommandTimeout = 123
        };

        var result = _sut.Build(options);

        var builder = new NpgsqlConnectionStringBuilder(result);
        Assert.Equal(123, builder.Timeout);
    }

    [Fact]
    public void Build_Should_Not_Change_Timeout_When_CommandTimeout_Is_Zero()
    {
        var options = new DatabaseOptions
        {
            ConnectionString = "Host=localhost;Database=TestDb;Timeout=42;Username=app;Password=secret;",
            CommandTimeout = 0
        };

        var result = _sut.Build(options);

        var builder = new NpgsqlConnectionStringBuilder(result);
        Assert.Equal(42, builder.Timeout);
    }

    [Fact]
    public void Build_Should_Not_Change_Timeout_When_CommandTimeout_Is_Negative()
    {
        var options = new DatabaseOptions
        {
            ConnectionString = "Host=localhost;Database=TestDb;Timeout=31;Username=app;Password=secret;",
            CommandTimeout = -10
        };

        var result = _sut.Build(options);

        var builder = new NpgsqlConnectionStringBuilder(result);
        Assert.Equal(31, builder.Timeout);
    }

    [Fact]
    public void Build_Should_Leave_Timeout_Default_When_Not_In_Base_And_CommandTimeout_NonPositive()
    {
        var options = new DatabaseOptions
        {
            ConnectionString = "Host=localhost;Database=TestDb;Username=app;Password=secret;",
            CommandTimeout = 0
        };

        var result = _sut.Build(options);

        var builder = new NpgsqlConnectionStringBuilder(result);
        // Npgsql default is 15 seconds (at time of writing); ensure not overridden to 0
        Assert.Equal(15, builder.Timeout);
    }

    [Fact]
    public void Build_Should_Throw_ArgumentNullException_When_Options_Null()
        => Assert.Throws<ArgumentNullException>(() => _sut.Build(null!));

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Build_Should_Throw_ArgumentException_When_ConnectionString_NullOrWhitespace(string cs)
    {
        var options = new DatabaseOptions { ConnectionString = cs, CommandTimeout = 10 };
        Assert.Throws<ArgumentException>(() => _sut.Build(options));
    }

    [Fact]
    public void Build_Should_Throw_ArgumentException_When_Invalid_Base_ConnectionString()
    {
        var options = new DatabaseOptions
        {
            ConnectionString = "Host==localhost;Database",
            CommandTimeout = 10
        };

        var ex = Assert.Throws<ArgumentException>(() => _sut.Build(options));
        Assert.Contains("Invalid PostgreSQL connection string", ex.Message);
    }

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
        // Username not set originally so should still be empty / not masked
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
