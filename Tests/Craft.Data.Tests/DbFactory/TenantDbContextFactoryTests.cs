using Craft.Core; // For IDbContext
using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Data.Tests.DbFactory;

// ReSharper disable InconsistentNaming
public class TenantDbContextFactoryTests
{
    #region Test Doubles / Helpers

    // Simple current tenant implementation (ICurrentTenant inherits ITenant)
    private sealed class TestCurrentTenant : Tenant, ICurrentTenant { }

    // Dummy DbContext that accepts configured options so we can inspect them
    private sealed class DummyDbContext : DbContext, IDbContext
    {
        public DummyDbContext(DbContextOptions<DummyDbContext> options) : base(options) { }
    }

    // Test database provider that uses EF Core InMemory provider and captures invocation data
    private sealed class InMemoryTestDatabaseProvider : IDatabaseProvider
    {
        public string? LastConnectionString { get; private set; }
        public DatabaseOptions? LastOptions { get; private set; }
        public bool CanHandle(string dbProvider) => string.Equals(dbProvider, "inmem", StringComparison.OrdinalIgnoreCase);
        public void Configure(DbContextOptionsBuilder builder, string connectionString, DatabaseOptions options)
        {
            LastConnectionString = connectionString;
            LastOptions = options;
            builder.UseInMemoryDatabase(connectionString); // connectionString doubles as store name
        }
        public bool ValidateConnection(string connectionString) => true;
    }

    private static TenantDbContextFactory<DummyDbContext> CreateFactory(
        TestCurrentTenant tenant,
        IEnumerable<IDatabaseProvider> providers,
        DatabaseOptions dbOptions,
        MultiTenantOptions? mtOptions = null)
    {
        mtOptions ??= new MultiTenantOptions { IsEnabled = true };

        var services = new ServiceCollection();
        services.AddTransient<DummyDbContext>();
        services.AddSingleton<IOptions<DatabaseOptions>>(Options.Create(dbOptions));
        services.AddSingleton<IOptions<MultiTenantOptions>>(Options.Create(mtOptions));
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var logger = sp.GetRequiredService<ILogger<TenantDbContextFactory<DummyDbContext>>>();

        return new TenantDbContextFactory<DummyDbContext>(
            tenant, 
            providers, 
            sp, 
            Options.Create(dbOptions), 
            Options.Create(mtOptions),
            logger);
    }

    private static (Mock<IDatabaseProvider> providerMock, List<(DbContextOptionsBuilder builder, string cs, DatabaseOptions opts)> calls)
        CreateProviderMock(string handledProvider)
    {
        var calls = new List<(DbContextOptionsBuilder, string, DatabaseOptions)>();
        var mock = new Mock<IDatabaseProvider>(MockBehavior.Strict);
        mock.Setup(p => p.CanHandle(It.IsAny<string>()))
            .Returns<string>(p => string.Equals(p, handledProvider, StringComparison.OrdinalIgnoreCase));
        mock.Setup(p => p.Configure(It.IsAny<DbContextOptionsBuilder>(), It.IsAny<string>(), It.IsAny<DatabaseOptions>()))
            .Callback<DbContextOptionsBuilder, string, DatabaseOptions>((b, cs, o) => calls.Add((b, cs, o)));
        return (mock, calls);
    }

    private static TestCurrentTenant BuildTenant(TenantDbType dbType, string? cs = null, string? provider = null) =>
        new() { DbType = dbType, ConnectionString = cs ?? string.Empty, DbProvider = provider ?? string.Empty };

    #endregion

    #region Shared

    [Fact]
    public void CreateDbContext_Shared_Uses_Defaults_And_Configures_Provider()
    {
        // Arrange
        var tenant = BuildTenant(TenantDbType.Shared);
        var dbOptions = new DatabaseOptions { ConnectionString = "SHARED_CS", DbProvider = "mssql" };
        var (providerMock, calls) = CreateProviderMock("mssql");
        var sut = CreateFactory(tenant, [providerMock.Object], dbOptions);

        // Act
        var ctx = sut.CreateDbContext();

        // Assert
        Assert.NotNull(ctx);
        Assert.Single(calls);
        var call = calls[0];
        Assert.Equal("SHARED_CS", call.cs);
        Assert.Same(dbOptions, call.opts);
        providerMock.Verify(p => p.Configure(It.IsAny<DbContextOptionsBuilder>(), "SHARED_CS", dbOptions), Times.Once);
    }

    [Fact]
    public void CreateDbContext_MultiTenancyDisabled_Uses_Defaults()
    {
        // Arrange
        var tenant = BuildTenant(TenantDbType.PerTenant, cs: "TENANT_CS", provider: "npgsql");
        var dbOptions = new DatabaseOptions { ConnectionString = "DEFAULT_SHARED", DbProvider = "mssql" };
        var (providerMock, calls) = CreateProviderMock("mssql");
        var sut = CreateFactory(tenant, [providerMock.Object], dbOptions, new MultiTenantOptions { IsEnabled = false });

        // Act
        _ = sut.CreateDbContext();

        // Assert
        Assert.Single(calls);
        Assert.Equal("DEFAULT_SHARED", calls[0].cs); // Should ignore tenant specific when disabled
    }

    #endregion

    #region PerTenant

    [Fact]
    public void CreateDbContext_PerTenant_NoConnectionString_Throws()
    {
        // Arrange
        var tenant = BuildTenant(TenantDbType.PerTenant, cs: "");
        var dbOptions = new DatabaseOptions { ConnectionString = "DEF", DbProvider = "mssql" };
        var (providerMock, _) = CreateProviderMock("mssql");
        var sut = CreateFactory(tenant, [providerMock.Object], dbOptions);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => sut.CreateDbContext());
        Assert.Contains("PerTenant", ex.Message, StringComparison.OrdinalIgnoreCase);
        providerMock.Verify(p => p.Configure(It.IsAny<DbContextOptionsBuilder>(), It.IsAny<string>(), It.IsAny<DatabaseOptions>()), Times.Never);
    }

    [Fact]
    public void CreateDbContext_PerTenant_WithTenantProvider_Uses_TenantValues()
    {
        // Arrange
        var tenant = BuildTenant(TenantDbType.PerTenant, cs: "TENANT_CS", provider: "npgsql");
        var dbOptions = new DatabaseOptions { ConnectionString = "DEF", DbProvider = "mssql" };
        var (providerMock, calls) = CreateProviderMock("npgsql");
        var sut = CreateFactory(tenant, [providerMock.Object], dbOptions);

        // Act
        _ = sut.CreateDbContext();

        // Assert
        Assert.Single(calls);
        Assert.Equal("TENANT_CS", calls[0].cs);
        providerMock.Verify(p => p.CanHandle("npgsql"), Times.AtLeastOnce);
    }

    [Fact]
    public void CreateDbContext_PerTenant_NoTenantProvider_FallsBack_To_DefaultProvider()
    {
        // Arrange
        var tenant = BuildTenant(TenantDbType.PerTenant, cs: "TENANT_CS", provider: null); // provider empty -> fallback
        var dbOptions = new DatabaseOptions { ConnectionString = "DEF", DbProvider = "mssql" };
        var (providerMock, calls) = CreateProviderMock("mssql");
        var sut = CreateFactory(tenant, [providerMock.Object], dbOptions);

        // Act
        _ = sut.CreateDbContext();

        // Assert
        Assert.Single(calls);
        Assert.Equal("TENANT_CS", calls[0].cs);
        providerMock.Verify(p => p.CanHandle("mssql"), Times.AtLeastOnce);
    }

    #endregion

    #region Hybrid

    [Fact]
    public void CreateDbContext_Hybrid_WithTenantConnection_Uses_TenantConnection()
    {
        // Arrange
        var tenant = BuildTenant(TenantDbType.Hybrid, cs: "TENANT_CS", provider: "mssql");
        var dbOptions = new DatabaseOptions { ConnectionString = "DEF_CS", DbProvider = "mssql" };
        var (providerMock, calls) = CreateProviderMock("mssql");
        var sut = CreateFactory(tenant, [providerMock.Object], dbOptions);

        // Act
        _ = sut.CreateDbContext();

        // Assert
        Assert.Single(calls);
        Assert.Equal("TENANT_CS", calls[0].cs);
    }

    [Fact]
    public void CreateDbContext_Hybrid_NoTenantConnection_FallsBack_To_Shared()
    {
        // Arrange
        var tenant = BuildTenant(TenantDbType.Hybrid, cs: ""); // empty -> shared path
        var dbOptions = new DatabaseOptions { ConnectionString = "SHARED_CS", DbProvider = "mssql" };
        var (providerMock, calls) = CreateProviderMock("mssql");
        var sut = CreateFactory(tenant, [providerMock.Object], dbOptions);

        // Act
        _ = sut.CreateDbContext();

        // Assert
        Assert.Single(calls);
        Assert.Equal("SHARED_CS", calls[0].cs);
    }

    #endregion

    #region Unsupported Provider / DbType

    [Fact]
    public void CreateDbContext_NoMatchingProvider_Throws_NotSupportedException()
    {
        // Arrange
        var tenant = BuildTenant(TenantDbType.Shared); // uses defaults provider key "mssql"
        var dbOptions = new DatabaseOptions { ConnectionString = "CS", DbProvider = "mssql" };
        // provider mock that reports false for any CanHandle
        var providerMock = new Mock<IDatabaseProvider>();
        providerMock.Setup(p => p.CanHandle(It.IsAny<string>())).Returns(false);
        var sut = CreateFactory(tenant, [providerMock.Object], dbOptions);

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => sut.CreateDbContext());
        Assert.Contains("mssql", ex.Message, StringComparison.OrdinalIgnoreCase); // message uses defaults.DbProvider
    }

    [Fact]
    public void CreateDbContext_UnsupportedDbType_Throws()
    {
        // Arrange
        var tenant = BuildTenant(TenantDbType.None); // will hit default:
        var dbOptions = new DatabaseOptions { ConnectionString = "CS", DbProvider = "mssql" };
        var (providerMock, _) = CreateProviderMock("mssql");
        var sut = CreateFactory(tenant, [providerMock.Object], dbOptions);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => sut.CreateDbContext());
        Assert.Contains("Unsupported DbType", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Multiple Providers_Selection

    [Fact]
    public void CreateDbContext_MultipleProviders_Selects_FirstMatching()
    {
        // Arrange
        var tenant = BuildTenant(TenantDbType.Shared);
        var dbOptions = new DatabaseOptions { ConnectionString = "CS", DbProvider = "npgsql" };

        var (nonMatchMock, nonMatchCalls) = CreateProviderMock("mssql"); // will not match npgsql
        var (matchMock, matchCalls) = CreateProviderMock("npgsql");

        var sut = CreateFactory(tenant, [nonMatchMock.Object, matchMock.Object], dbOptions);

        // Act
        _ = sut.CreateDbContext();

        // Assert
        Assert.Empty(nonMatchCalls); // configure never called
        Assert.Single(matchCalls);
        matchMock.Verify(p => p.CanHandle("npgsql"), Times.AtLeastOnce);
    }

    #endregion

    #region Options Application

    [Fact]
    public void CreateDbContext_Applies_Provider_And_Options_Flags()
    {
        // Arrange shared tenant scenario
        var tenant = BuildTenant(TenantDbType.Shared);
        var dbOptions = new DatabaseOptions
        {
            ConnectionString = "InMemDb-Test",
            DbProvider = "inmem",
            EnableDetailedErrors = true,
            EnableSensitiveDataLogging = true
        };
        var testProvider = new InMemoryTestDatabaseProvider();
        var sut = CreateFactory(tenant, [testProvider], dbOptions);

        // Act
        var ctx = sut.CreateDbContext();

        // Assert provider configure invoked with connection string
        Assert.Equal("InMemDb-Test", testProvider.LastConnectionString);
        Assert.Same(dbOptions, testProvider.LastOptions);

        // Assert context was created successfully
        Assert.NotNull(ctx);

        // Verify the database provider is InMemory by checking if Database.IsInMemory() returns true
        Assert.True(ctx.Database.IsInMemory());

        // Retrieve CoreOptionsExtension to ensure options were built (presence implies builder executed)
        var dbContextOptions = ctx.GetService<IDbContextOptions>();
        var coreExt = dbContextOptions.FindExtension<CoreOptionsExtension>();
        Assert.NotNull(coreExt);
    }

    #endregion
}
