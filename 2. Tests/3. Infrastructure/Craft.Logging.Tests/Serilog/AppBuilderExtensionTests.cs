using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using ApplicationBuilderExtensions = Craft.Logging.Serilog.ApplicationBuilderExtensions;

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
        Assert.Throws<NullReferenceException>(() => ApplicationBuilderExtensions.UseSerilogEnrichers(app!));
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
        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            UseMiddlewareCalled = true;
            //MiddlewareType = middleware;

            return this;
        }
    }
}
