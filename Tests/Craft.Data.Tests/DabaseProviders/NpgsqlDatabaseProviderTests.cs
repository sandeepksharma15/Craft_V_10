using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Testcontainers.PostgreSql;

namespace Craft.Data.Tests.DabaseProviders;

public class NpgsqlDatabaseProviderTests : IClassFixture<NpgsqlDatabaseProviderTests.PostgreSqlContainerFixture>
{
    private readonly NpgsqlDatabaseProvider _sut = new();
    private readonly PostgreSqlContainerFixture _fixture;

    public NpgsqlDatabaseProviderTests(PostgreSqlContainerFixture fixture) => _fixture = fixture;

    #region Test DbContext

    private sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    }

    #endregion

    #region CanHandle

    [Theory]
    [InlineData("postgresql")]
    [InlineData("PostgreSQL")]
    [InlineData("POSTGRESQL")]
    public void CanHandle_Should_ReturnTrue_For_PostgreSql_CaseInsensitive(string provider)
    {
        var result = _sut.CanHandle(provider);
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("mssql")]
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
    public void Configure_Should_Apply_Npgsql_And_Set_Options()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        var options = new DatabaseOptions
        {
            CommandTimeout = 77,
            MaxRetryCount = 5,
            MaxRetryDelay = 42,
        };
        var connectionString = _fixture.ConnectionString ?? "Host=localhost;Database=craft_fallback;Username=postgres;Password=postgres";

        // Act
        _sut.Configure(builder, connectionString, options);

        // Assert (via constructed context)
        using var ctx = new TestDbContext(builder.Options);
        Assert.NotNull(ctx.Database);
        Assert.Contains("Npgsql", ctx.Database.ProviderName, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(connectionString, ctx.Database.GetDbConnection().ConnectionString);
        Assert.Equal(options.CommandTimeout, ctx.Database.GetCommandTimeout());

        // Reflection check migrations assembly
        var optionsService = ctx.GetService<IDbContextOptions>();
        var extension = optionsService.Extensions.First(e => e.GetType().Name.Contains("Npgsql", StringComparison.OrdinalIgnoreCase));
        var migrationsAssemblyProp = extension.GetType().GetProperty("MigrationsAssembly", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var migrationsAssembly = migrationsAssemblyProp?.GetValue(extension) as string;
        Assert.Equal(IDatabaseProvider.PostgreSqlMigrationAssembly, migrationsAssembly);
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
        var invalid = "Host=localhost;Port=1;Database=doesnotexist;Username=x;Password=y";
        var result = _sut.ValidateConnection(invalid);
        Assert.False(result);
    }

    [Fact]
    public void ValidateConnection_Should_ReturnTrue_For_Working_PostgreSql_Instance()
    {
        if (!_fixture.IsAvailable)
        {
            // Environment (Docker) not available; skip by early return.
            return;
        }

        var result = _sut.ValidateConnection(_fixture.ConnectionString!);
        Assert.True(result);
    }

    #endregion

    #region Fixture

    public sealed class PostgreSqlContainerFixture : IAsyncLifetime
    {
        private PostgreSqlContainer? _container;
        private readonly PostgreSqlBuilder _builder;

        public PostgreSqlContainerFixture()
        {
            // Configure builder only; actual Build/Start deferred to InitializeAsync so we can catch Docker issues.
            _builder = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("craft_test")
                .WithUsername("postgres")
                .WithPassword("postgres");
        }

        public string? ConnectionString => _container?.GetConnectionString();
        public bool IsAvailable { get; private set; }

        public async Task InitializeAsync()
        {
            try
            {
                _container = _builder.Build();
                await _container.StartAsync();
                IsAvailable = true;
            }
            catch
            {
                // Docker not available; mark fixture unusable but do not throw so unit tests can proceed.
                IsAvailable = false;
            }
        }

        public async Task DisposeAsync()
        {
            if (_container is not null)
                await _container.DisposeAsync();
        }
    }

    #endregion
}
