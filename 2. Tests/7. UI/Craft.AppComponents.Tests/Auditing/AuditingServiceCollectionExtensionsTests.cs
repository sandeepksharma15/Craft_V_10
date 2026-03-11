using Craft.AppComponents.Auditing;
using Craft.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Tests.Auditing;

/// <summary>
/// Unit tests for <see cref="ServiceCollectionExtensions"/> (Auditing) verifying DI registrations.
/// Uses ServiceDescriptor inspection to avoid requiring full infrastructure resolution.
/// </summary>
public class AuditingServiceCollectionExtensionsTests
{
    // Satisfies: class, ICraftUser (non-generic alias for ICraftUser<KeyType>)
    private sealed class TestAuditUser : CraftUser<KeyType>, ICraftUser { }

    // ── AddAuditTrailApi() ──────────────────────────────────────────────────────

    [Fact]
    public void AddAuditTrailApi_RegistersIAuditTrailRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuditTrailApi();

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IAuditTrailRepository));
    }

    [Fact]
    public void AddAuditTrailApi_RegistersRepositoryAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuditTrailApi();

        // Assert
        var descriptor = services.Single(sd => sd.ServiceType == typeof(IAuditTrailRepository));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    // ── AddAuditTrailApi<TUser>() ───────────────────────────────────────────────

    [Fact]
    public void AddAuditTrailApiGeneric_RegistersIAuditUserResolver()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuditTrailApi<TestAuditUser>();

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IAuditUserResolver));
    }

    [Fact]
    public void AddAuditTrailApiGeneric_RegistersIAuditTrailRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuditTrailApi<TestAuditUser>();

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IAuditTrailRepository));
    }

    [Fact]
    public void AddAuditTrailApiGeneric_RegistersResolverAsCraftUserAuditResolver()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuditTrailApi<TestAuditUser>();

        // Assert
        var descriptor = services.Single(sd => sd.ServiceType == typeof(IAuditUserResolver));
        Assert.Equal(typeof(CraftUserAuditResolver<TestAuditUser>), descriptor.ImplementationType);
    }

    // ── AddAuditTrailUI() ───────────────────────────────────────────────────────

    [Fact]
    public void AddAuditTrailUI_RegistersIAuditTrailHttpService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuditTrailUI(sp => new HttpClient(), "https://api");

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IAuditTrailHttpService));
    }

    [Fact]
    public void AddAuditTrailUI_RegistersServiceAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuditTrailUI(sp => new HttpClient(), "https://api");

        // Assert
        var descriptor = services.Single(sd => sd.ServiceType == typeof(IAuditTrailHttpService));
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void AddAuditTrailUI_ResolvesIAuditTrailHttpService_WithRequiredDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ILogger<AuditTrailHttpService>>());
        services.AddAuditTrailUI(sp => new HttpClient(), "https://api");
        var provider = services.BuildServiceProvider();

        // Act
        var service = provider.GetService<IAuditTrailHttpService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<AuditTrailHttpService>(service);
    }
}
