using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Craft.Permissions.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCraftPermissions_RegistersPermissionDefinitionRegistry_AsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddCraftPermissions<IdentityUser<long>>();

        var provider = services.BuildServiceProvider();
        var registry1 = provider.GetRequiredService<IPermissionDefinitionRegistry>();
        var registry2 = provider.GetRequiredService<IPermissionDefinitionRegistry>();

        Assert.Same(registry1, registry2);
    }

    [Fact]
    public void AddCraftPermissions_RegistersRolePermissionRepository()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddCraftPermissions<IdentityUser<long>>();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRolePermissionRepository));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddCraftPermissions_RegistersPermissionStartupValidatorService()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddCraftPermissions<IdentityUser<long>>();

        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IHostedService) &&
            d.ImplementationType == typeof(PermissionStartupValidatorService));

        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddCraftPermissionsUi_RegistersPermissionSessionCache_AsScoped()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<HttpClient>();

        services.AddCraftPermissionsUi(new Uri("https://localhost/api/permissions"));

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(PermissionSessionCache));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddCraftPermissionsUi_RegistersIPermissionChecker_AsScoped()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<HttpClient>();

        services.AddCraftPermissionsUi(new Uri("https://localhost/api/permissions"));

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPermissionChecker));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddCraftPermissionsUi_RegistersPermissionAuthStateListener_AsScoped()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<HttpClient>();

        services.AddCraftPermissionsUi(new Uri("https://localhost/api/permissions"));

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(PermissionAuthStateListener));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddCraftPermissionsUi_ThrowsArgumentNullException_WhenBaseUrlIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddCraftPermissionsUi(null!));
    }

    [Fact]
    public void RegisterPermissions_ThrowsInvalidOperationException_WhenDuplicateCodeRegistered()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddCraftPermissions<IdentityUser<long>>()
            .RegisterPermissions(
                new PermissionDefinition(1001, "Permission A", "Group1"),
                new PermissionDefinition(1001, "Duplicate A", "Group1"));

        var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() =>
            provider.GetServices<IStartupValidator>().ToList().ForEach(v => v.Validate()));
    }
}
