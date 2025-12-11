using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.MultiTenant.Tests.EventTests;

public class TenantEventsTests
{
    [Fact]
    public async Task OnTenantResolved_IsCalledWhenTenantFound()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        bool eventCalled = false;
        string? resolvedIdentifier = null;
        Type? strategyType = null;

        services
            .AddMultiTenant<Tenant>(options =>
            {
                options.Events.OnTenantResolved = context =>
                {
                    eventCalled = true;
                    resolvedIdentifier = context.Tenant?.Identifier;
                    strategyType = context.StrategyType;
                    return Task.CompletedTask;
                };
            })
            .WithStaticStrategy("initech")
            .WithConfigurationStore();

        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        
        await resolver.ResolveAsync(new DefaultHttpContext());

        Assert.True(eventCalled);
        Assert.Equal("initech", resolvedIdentifier);
        Assert.Equal(typeof(StaticStrategy), strategyType);
    }

    [Fact]
    public async Task OnTenantNotResolved_IsCalledWhenTenantNotFound()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        bool eventCalled = false;
        string? attemptedIdentifier = null;

        services
            .AddMultiTenant<Tenant>(options =>
            {
                options.Events.OnTenantNotResolved = context =>
                {
                    eventCalled = true;
                    attemptedIdentifier = context.Identifier;
                    return Task.CompletedTask;
                };
            })
            .WithStaticStrategy("nonexistent")
            .WithConfigurationStore();

        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        
        await resolver.ResolveAsync(new DefaultHttpContext());

        Assert.True(eventCalled);
        Assert.Equal("nonexistent", attemptedIdentifier);
    }

    [Fact]
    public async Task OnTenantResolved_ReceivesCorrectContext()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        ResolvedContext? capturedContext = null;

        services
            .AddMultiTenant<Tenant>(options =>
            {
                options.Events.OnTenantResolved = context =>
                {
                    capturedContext = context;
                    return Task.CompletedTask;
                };
            })
            .WithStaticStrategy("initech")
            .WithConfigurationStore();

        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        var httpContext = new DefaultHttpContext();
        
        await resolver.ResolveAsync(httpContext);

        Assert.NotNull(capturedContext);
        Assert.Same(httpContext, capturedContext.Context);
        Assert.NotNull(capturedContext.Tenant);
        Assert.NotNull(capturedContext.StrategyType);
        Assert.NotNull(capturedContext.StoreType);
    }

    [Fact]
    public async Task OnTenantNotResolved_ReceivesCorrectContext()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        NotResolvedContext? capturedContext = null;

        services
            .AddMultiTenant<Tenant>(options =>
            {
                options.Events.OnTenantNotResolved = context =>
                {
                    capturedContext = context;
                    return Task.CompletedTask;
                };
            })
            .WithStaticStrategy("nonexistent")
            .WithConfigurationStore();

        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        var httpContext = new DefaultHttpContext();
        
        await resolver.ResolveAsync(httpContext);

        Assert.NotNull(capturedContext);
        Assert.Same(httpContext, capturedContext.Context);
        Assert.Equal("nonexistent", capturedContext.Identifier);
    }

    [Fact]
    public async Task Events_CanThrowExceptions()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services
            .AddMultiTenant<Tenant>(options =>
            {
                options.Events.OnTenantResolved = context =>
                {
                    throw new InvalidOperationException("Custom error");
                };
            })
            .WithStaticStrategy("initech")
            .WithConfigurationStore();

        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await resolver.ResolveAsync(new DefaultHttpContext()));
    }

    [Fact]
    public async Task MultipleEvents_BothAreCalled()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        int resolvedCount = 0;
        int notResolvedCount = 0;

        services
            .AddMultiTenant<Tenant>(options =>
            {
                options.Events.OnTenantResolved = context =>
                {
                    resolvedCount++;
                    return Task.CompletedTask;
                };
                options.Events.OnTenantNotResolved = context =>
                {
                    notResolvedCount++;
                    return Task.CompletedTask;
                };
            })
            .WithStaticStrategy("initech")
            .WithConfigurationStore();

        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<Tenant>>();
        
        await resolver.ResolveAsync(new DefaultHttpContext());
        
        await resolver.ResolveAsync(new DefaultHttpContext());

        Assert.Equal(2, resolvedCount);
        Assert.Equal(0, notResolvedCount);
    }
}
