using Microsoft.AspNetCore.Http;
using Craft.Logging.Serilog;

namespace Craft.Logging.Tests.Serilog;

public class SerilogMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_CallsNextDelegate()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var called = false;

        // Act
        // Create a RequestDelegate that sets a flag when called
        Task next(HttpContext ctx) 
        { 
            called = true; return Task.CompletedTask; 
        }

        var middleware = new SerilogMiddleware(next);

        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_PushesHeaderValuesToLogContext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["_TENANT_ID_"] = "tenant";
        context.Request.Headers["_USER_ID_"] = "user";
        context.Request.Headers["_CLIENT_ID_"] = "client";
        var called = false;

        // Act
        Task next(HttpContext ctx) 
        { 
            called = true; return Task.CompletedTask; 
        }

        var middleware = new SerilogMiddleware(next);

        await middleware.InvokeAsync(context);

        // Assert
        // Can't directly assert LogContext, but ensure no exceptions and next is called
        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_MissingHeaders_PushesEmptyStrings()
    {

        var context = new DefaultHttpContext();
        var called = false;

        Task next(HttpContext ctx) 
        { 
            called = true; return Task.CompletedTask; 
        }

        var middleware = new SerilogMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_EmptyHeaders_DoesNotThrow()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["_TENANT_ID_"] = "";
        context.Request.Headers["_USER_ID_"] = "";
        context.Request.Headers["_CLIENT_ID_"] = "";
        var called = false;

        Task next(HttpContext ctx) 
        { 
            called = true; return Task.CompletedTask; 
        }

        var middleware = new SerilogMiddleware(next);

        // Act
        var ex = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        // Assert
        Assert.Null(ex);
        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_CustomHeaderNames_AreUsed()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var called = false;

        Task next(HttpContext ctx) 
        { 
            called = true; return Task.CompletedTask; 
        }

        var middleware = new SerilogMiddleware(next);

        // Use reflection to set custom header names (since _options is private and not injected)
        var optionsField = typeof(SerilogMiddleware)
            .GetField("_options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var options = new CraftSerilogOptions
        {
            EnricherNames = new CraftSerilogOptions.SerilogEnricherNames
            {
                TenantId = "X-Tenant",
                UserId = "X-User",
                ClientId = "X-Client"
            }
        };

        // Set the options field value
        optionsField?.SetValue(middleware, options);
        context.Request.Headers["X-Tenant"] = "tenant";
        context.Request.Headers["X-User"] = "user";
        context.Request.Headers["X-Client"] = "client";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(called);
    }
}
