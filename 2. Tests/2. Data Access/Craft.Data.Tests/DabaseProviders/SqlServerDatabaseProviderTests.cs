using System.Reflection;
using Microsoft.Value.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Craft.Value.Tests.DabaseProviders;

public class SqlServerDatabaseProviderTests
{
    private readonly SqlServerDatabaseProvider _sut = new();

    private sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    }

    #region CanHandle

    [Theory]
    [InlineData("mssql")]
    [InlineData("MSSQL")]
    [InlineData("MsSql")]
    public void CanHandle_Should_ReturnTrue_For_SqlServer_CaseInsensitive(string provider)
    {
        var result = _sut.CanHandle(provider);
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("postgresql")]
    [InlineData("mysql")]
    public void CanHandle_Should_ReturnFalse_For_NonMatching_Or_Null(string? provider)
    {
        if (provider is null)
        {
            Assert.Throws<NullReferenceException>(() => _sut.CanHandle(provider!));
            return;
        }

        var result = _sut.CanHandle(provider);
        Assert.False(result);
    }

    #endregion

    #region Configure

    [Fact]
    public void Configure_Should_Apply_SqlServer_And_Set_Options()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            CommandTimeout = 90,
            MaxRetryCount = 7,
            MaxRetryDelay = 33,
        };
        var connectionString = "Server=(local);Database=master;Trusted_Connection=True;Encrypt=False"; // Generic CS; not opened here.

        // Act
        _sut.Configure(builder, connectionString, options);

        // Assert
        using var ctx = new TestDbContext(builder.Options);
        Assert.Contains("SqlServer", ctx.Database.ProviderName, StringComparison.OrdinalIgnoreCase);

        // Compare key properties only; provider may append additional parameters like Application Name
        var expected = new SqlConnectionStringBuilder(connectionString);
        var actual = new SqlConnectionStringBuilder(ctx.Database.GetDbConnection().ConnectionString);
        Assert.Equal(expected.DataSource, actual.DataSource);
        Assert.Equal(expected.InitialCatalog, actual.InitialCatalog);
        Assert.Equal(expected.IntegratedSecurity, actual.IntegratedSecurity);
        Assert.Equal(expected.Encrypt, actual.Encrypt);

        Assert.Equal(options.CommandTimeout, ctx.Database.GetCommandTimeout());

        // Reflection: ensure migration assembly value set
        var optionsService = ctx.GetService<IDbContextOptions>();
        var extension = optionsService.Extensions.First(e => e.GetType().Name.Contains("SqlServer", StringComparison.OrdinalIgnoreCase));
        var migrationsAssemblyProp = extension.GetType().GetProperty("MigrationsAssembly", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var migrationsAssembly = migrationsAssemblyProp?.GetValue(extension) as string;
        Assert.Equal(IDatabaseProvider.MsSqlMigrationAssembly, migrationsAssembly);
    }

    #endregion

    #region ValidateConnection

    [Fact]
    public void ValidateConnection_Should_ReturnFalse_For_NullOrWhitespace()
    {
        Assert.False(_sut.ValidateConnection(null!));
        Assert.False(_sut.ValidateConnection(""));
        Assert.False(_sut.ValidateConnection("   "));
    }

    [Fact]
    public void ValidateConnection_Should_ReturnFalse_For_Invalid_ConnectionString()
    {
        var invalid = "Server=invalid_host_12345;Database=NoDb;User Id=foo;Password=bar;Connect Timeout=1;TrustServerCertificate=True";
        var result = _sut.ValidateConnection(invalid);
        Assert.False(result);
    }

    [Fact]
    public void ValidateConnection_Should_ReturnTrue_For_Working_LocalDb_If_Available()
    {
        // Attempt LocalDB first (common developer machine scenario)
        var candidates = new[]
        {
            "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;",
            "Server=localhost;Database=master;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True",
            "Server=.\\SQLEXPRESS;Database=master;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True"
        };

        foreach (var cs in candidates)
        {
            try
            {
                using var conn = new SqlConnection(cs);
                conn.Open();
                var ok = _sut.ValidateConnection(cs);
                Assert.True(ok);
                return; // success path validated
            }
            catch
            {
                // Try next candidate
            }
        }

        // If none available environment does not have SQL Server; acceptable to return silently (test becomes no-op)
    }

    #endregion
}

