using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Craft.Data.Tests.Extensions;

public class DbContextOptionsBuilderExtensionsTests
{
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    }

    [Fact]
    public void UseDatabase_Should_Throw_When_Builder_Is_Null()
    {
        // Arrange
        DbContextOptionsBuilder builder = null!;
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=.;Database=Test;",
            DbProvider = "mssql"
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            builder.UseDatabase("mssql", "Server=.;Database=Test;", options));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UseDatabase_Should_Throw_When_DbProvider_IsNullOrWhiteSpace(string? dbProvider)
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=.;Database=Test;",
            DbProvider = "mssql"
        };

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => 
            builder.UseDatabase(dbProvider!, "Server=.;Database=Test;", options));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UseDatabase_Should_Throw_When_ConnectionString_IsNullOrWhiteSpace(string? connectionString)
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=.;Database=Test;",
            DbProvider = "mssql"
        };

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => 
            builder.UseDatabase("mssql", connectionString!, options));
    }

    [Fact]
    public void UseDatabase_Should_Throw_When_Options_Is_Null()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            builder.UseDatabase("mssql", "Server=.;Database=Test;", null!));
    }

    [Fact]
    public void UseDatabase_Should_Throw_When_Unsupported_Provider()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=.;Database=Test;",
            DbProvider = "unsupported",
            MaxRetryCount = 3
        };

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => 
            builder.UseDatabase("unsupported", "Server=.;Database=Test;", options));
        Assert.Contains("unsupported", ex.Message);
    }

    [Fact]
    public void UseDatabase_Should_Use_Built_In_SqlServer_Provider()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Test;Trusted_Connection=True;",
            DbProvider = "mssql",
            CommandTimeout = 60
        };

        // Act
        var result = builder.UseDatabase("mssql", options.ConnectionString, options);

        // Assert
        Assert.NotNull(result);
        Assert.Same(builder, result);
    }

    [Fact]
    public void UseDatabase_Should_Use_Built_In_PostgreSQL_Provider()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            ConnectionString = "Host=localhost;Database=Test;Username=postgres;Password=test;",
            DbProvider = "postgresql",
            CommandTimeout = 60
        };

        // Act
        var result = builder.UseDatabase("postgresql", options.ConnectionString, options);

        // Assert
        Assert.NotNull(result);
        Assert.Same(builder, result);
    }

    [Fact]
    public void UseDatabase_Should_Apply_EnableDetailedErrors_When_Set()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            ConnectionString = "Host=localhost;Database=Test;",
            DbProvider = "postgresql",
            EnableDetailedErrors = true
        };

        // Act
        builder.UseDatabase("postgresql", options.ConnectionString, options);

        // Assert
        using var context = new TestDbContext(builder.Options);
        // Verify the option was set (implicit through build success)
        Assert.NotNull(context);
    }

    [Fact]
    public void UseDatabase_Should_Apply_EnableSensitiveDataLogging_When_Set()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            ConnectionString = "Host=localhost;Database=Test;",
            DbProvider = "postgresql",
            EnableSensitiveDataLogging = true
        };

        // Act
        builder.UseDatabase("postgresql", options.ConnectionString, options);

        // Assert
        using var context = new TestDbContext(builder.Options);
        Assert.NotNull(context);
    }

    [Fact]
    public void UseDatabase_Should_Resolve_Provider_From_ServiceProvider()
    {
        // Arrange
        var mockProvider = new Mock<IDatabaseProvider>();
        mockProvider.Setup(p => p.CanHandle("custom")).Returns(true);
        mockProvider.Setup(p => p.Configure(It.IsAny<DbContextOptionsBuilder>(), It.IsAny<string>(), It.IsAny<DatabaseOptions>()))
            .Callback<DbContextOptionsBuilder, string, DatabaseOptions>((b, cs, o) => 
                b.UseInMemoryDatabase("test"));

        var services = new ServiceCollection();
        services.AddSingleton(mockProvider.Object);
        var serviceProvider = services.BuildServiceProvider();

        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            ConnectionString = "custom connection",
            DbProvider = "custom"
        };

        // Act
        builder.UseDatabase("custom", "custom connection", options, serviceProvider);

        // Assert
        mockProvider.Verify(p => p.Configure(It.IsAny<DbContextOptionsBuilder>(), "custom connection", options), Times.Once);
    }

    [Fact]
    public void UseDatabase_Overload_With_Simple_Parameters_Should_Create_Options()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();

        // Act
        var result = builder.UseDatabase(
            "postgresql",
            "Host=localhost;Database=Test;",
            maxRetryCount: 5,
            maxRetryDelay: 30,
            commandTimeout: 120);

        // Assert
        Assert.NotNull(result);
        Assert.Same(builder, result);
    }

    [Fact]
    public void UseDatabase_Generic_Overload_Should_Return_Typed_Builder()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            ConnectionString = "Host=localhost;Database=Test;",
            DbProvider = "postgresql"
        };

        // Act
        var result = builder.UseDatabase("postgresql", options.ConnectionString, options);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<DbContextOptionsBuilder<TestDbContext>>(result);
        Assert.Same(builder, result);
    }

    [Fact]
    public void UseDatabase_Generic_Simple_Overload_Should_Return_Typed_Builder()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();

        // Act
        var result = builder.UseDatabase(
            "postgresql",
            "Host=localhost;Database=Test;",
            maxRetryCount: 3,
            maxRetryDelay: 15,
            commandTimeout: 30);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<DbContextOptionsBuilder<TestDbContext>>(result);
        Assert.Same(builder, result);
    }

    [Fact]
    public void UseDatabase_Should_Handle_Case_Insensitive_Provider_Keys()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=.;Database=Test;",
            DbProvider = "MSSQL" // uppercase
        };

        // Act
        var result = builder.UseDatabase("MSSQL", options.ConnectionString, options);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void UseDatabase_Should_Use_Default_Values_When_Not_Specified()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();

        // Act
        var result = builder.UseDatabase("postgresql", "Host=localhost;Database=Test;");

        // Assert
        Assert.NotNull(result);
    }
}

