using System;
using System.Collections.Generic;
using Craft.Core; // For IDbContext
using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Craft.Data.Tests.DbFactory;

// ReSharper disable InconsistentNaming
public class TenantDbContextFactoryTests
{
    #region Test Doubles / Helpers

    // Simple current tenant implementation (ICurrentTenant inherits ITenant)
    private sealed class TestCurrentTenant : Tenant, ICurrentTenant { }

    // Simple dummy DbContext so that ActivatorUtilities can construct it (parameterless ctor)
    private sealed class DummyDbContext : DbContext, IDbContext { }

    private static TenantDbContextFactory<DummyDbContext> CreateFactory(
        TestCurrentTenant tenant,
        IEnumerable<IDatabaseProvider> providers,
        DatabaseOptions dbOptions)
    {
        var services = new ServiceCollection();
        // Register any dependencies required to create DummyDbContext (none -> parameterless)
        services.AddTransient<DummyDbContext>();
        services.AddSingleton<IOptions<DatabaseOptions>>(Options.Create(dbOptions));
        var sp = services.BuildServiceProvider();

        return new TenantDbContextFactory<DummyDbContext>(tenant, providers, sp, Options.Create(dbOptions));
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

        var sut = CreateFactory(tenant, new[] { nonMatchMock.Object, matchMock.Object }, dbOptions);

        // Act
        _ = sut.CreateDbContext();

        // Assert
        Assert.Empty(nonMatchCalls); // configure never called
        Assert.Single(matchCalls);
        matchMock.Verify(p => p.CanHandle("npgsql"), Times.AtLeastOnce);
    }

    #endregion
}
