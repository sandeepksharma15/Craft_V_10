using Microsoft.Data.SqlClient;

namespace Craft.Data.Tests.ConnectionStringHandlers;

public class MsSqlConnectionStringHandlerTests
{
    private const string Mask = "*******";
    private readonly MsSqlConnectionStringHandler _sut = new();

    // ---------- Build tests ----------

    [Fact]
    public void Build_Should_Set_ConnectTimeout_When_Not_Present_And_CommandTimeout_Positive()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=.;Database=TestDb;Integrated Security=true;",
            CommandTimeout = 77
        };

        // Act
        var result = _sut.Build(options);

        // Assert
        var builder = new SqlConnectionStringBuilder(result);
        Assert.Equal(77, builder.ConnectTimeout);
        Assert.Equal(".", builder.DataSource);
        Assert.Equal("TestDb", builder.InitialCatalog);
    }

    [Fact]
    public void Build_Should_Override_Existing_ConnectTimeout_When_CommandTimeout_Positive()
    {
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=.;Database=TestDb;Connect Timeout=5;Integrated Security=true;",
            CommandTimeout = 123
        };

        var result = _sut.Build(options);

        var builder = new SqlConnectionStringBuilder(result);
        Assert.Equal(123, builder.ConnectTimeout);
    }

    [Fact]
    public void Build_Should_Not_Change_Timeout_When_CommandTimeout_Is_Zero()
    {
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=.;Database=TestDb;Connect Timeout=42;Integrated Security=true;",
            CommandTimeout = 0
        };

        var result = _sut.Build(options);

        var builder = new SqlConnectionStringBuilder(result);
        Assert.Equal(42, builder.ConnectTimeout);
    }

    [Fact]
    public void Build_Should_Not_Change_Timeout_When_CommandTimeout_Is_Negative()
    {
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=.;Database=TestDb;Connect Timeout=31;Integrated Security=true;",
            CommandTimeout = -10
        };

        var result = _sut.Build(options);

        var builder = new SqlConnectionStringBuilder(result);
        Assert.Equal(31, builder.ConnectTimeout);
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
            ConnectionString = "Server==;Database",
            CommandTimeout = 10
        };

        var ex = Assert.Throws<ArgumentException>(() => _sut.Build(options));
        Assert.Contains("Invalid SQL Server connection string", ex.Message);
    }

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
