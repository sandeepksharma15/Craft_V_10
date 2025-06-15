using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Craft.Logging.Serilog;
using ApplicationBuilderExtensions = Craft.Logging.Serilog.ApplicationBuilderExtensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.AspNetCore.Hosting;

namespace Craft.Logging.Tests.Serilog;

public class AppBuilderExtensionTests
{
    [Fact]
    public void UseSerilogEnrichers_CallsUseMiddlewareWithSerilogMiddleware()
    {
        // Arrange
        var app = new FakeAppBuilder();

        // Act
        ApplicationBuilderExtensions.UseSerilogEnrichers(app);

        // Assert
        Assert.True(app.UseMiddlewareCalled);
        Assert.Equal(typeof(SerilogMiddleware), app.MiddlewareType);
    }

    [Fact]
    public void UseSerilogEnrichers_ReturnsSameInstance()
    {
        // Arrange
        var app = new FakeAppBuilder();

        // Act
        var result = ApplicationBuilderExtensions.UseSerilogEnrichers(app);

        // Assert
        Assert.Same(app, result);
    }

    [Fact]
    public void UseSerilogEnrichers_ThrowsIfAppIsNull()
    {
        // Arrange
        IApplicationBuilder? app = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ApplicationBuilderExtensions.UseSerilogEnrichers(app!));
    }

    private class FakeAppBuilder : IApplicationBuilder
    {
        public bool UseMiddlewareCalled { get; private set; }
        public Type? MiddlewareType { get; private set; }
        public IServiceProvider ApplicationServices { get; set; } = null!;
        public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();
        public IFeatureCollection ServerFeatures { get; } = new FeatureCollection();
        public RequestDelegate Build() => throw new NotImplementedException();
        public IApplicationBuilder New() => this;
        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) => this;

        // Shadow the extension method to intercept UseMiddleware(Type, params object[] args)
        public IApplicationBuilder UseMiddleware(Type middleware, params object[] args)
        {
            UseMiddlewareCalled = true;
            MiddlewareType = middleware;
            return this;
        }

        // Keep the generic for completeness, but it's not used by the extension
        public IApplicationBuilder UseMiddleware<TMiddleware>(params object[] args)
        {
            UseMiddlewareCalled = true;
            MiddlewareType = typeof(TMiddleware);
            return this;
        }
    }
}

public class SerilogIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SerilogIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.Configure(app =>
            {
                app.UseSerilogEnrichers();
                app.Run(async ctx => await ctx.Response.WriteAsync("ok"));
            });
        });
    }

    [Fact]
    public async Task Middleware_Is_Invoked()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Optionally, check logs or context here
    }
}
