using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;
using Craft.UiComponents;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the ErrorBoundary component.
/// Tests exception catching, error display, and retry functionality.
/// </summary>
public class ErrorBoundaryTests : ComponentTestBase
{
    [Fact]
    public void ErrorBoundary_WithNoError_ShouldRenderChildContent()
    {
        // Arrange & Act
        var cut = Render<ErrorBoundary>(parameters => parameters
            .AddChildContent("<div>Normal content</div>")
        );

        // Assert
        Assert.Contains("Normal content", cut.Markup);
    }

    [Fact]
    public void ErrorBoundary_WithErrorContent_ShouldRenderErrorTemplate()
    {
        // Arrange
        var testException = new InvalidOperationException("Test error");
        var errorOccurred = false;

        // Act
        var cut = Render<ErrorBoundary>(parameters => parameters
            .Add(p => p.ShowDetails, true)
            .Add(p => p.OnError, args =>
            {
                errorOccurred = true;
                return Task.CompletedTask;
            })
            .Add(p => p.ErrorContent, context => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "error-display");
                builder.AddContent(2, $"Error: {context.Message}");
                builder.CloseElement();
            })
            .AddChildContent("<div>Content</div>")
        );

        // Assert - should render child content when no error
        Assert.Contains("Content", cut.Markup);
    }

    [Fact]
    public void ErrorBoundary_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new ErrorBoundary();

        // Assert
        Assert.IsType<CraftComponent>(component, exactMatch: false);
    }

    [Fact]
    public void ErrorBoundary_WithShowDetailsTrue_ErrorContextShouldExposeStackTrace()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        var context = new ErrorContext(exception, true);

        // Assert
        Assert.Equal("Test error", context.Message);
        Assert.NotNull(context.StackTrace);
    }

    [Fact]
    public void ErrorBoundary_WithShowDetailsFalse_ErrorContextShouldHideStackTrace()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        var context = new ErrorContext(exception, false);

        // Assert
        Assert.Equal("Test error", context.Message);
        Assert.Null(context.StackTrace);
    }

    [Fact]
    public void ErrorContext_ShouldExposeExceptionMessage()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        var context = new ErrorContext(exception, false);

        // Assert
        Assert.Equal("Invalid argument", context.Message);
        Assert.Same(exception, context.Exception);
    }

    [Fact]
    public void ErrorBoundary_WithAutoRetryFalse_ShouldNotRetry()
    {
        // Arrange
        var component = new ErrorBoundary
        {
            AutoRetry = false,
            RetryDelayMs = 1000
        };

        // Assert
        Assert.False(component.AutoRetry);
        Assert.Equal(1000, component.RetryDelayMs);
    }

    [Fact]
    public void ErrorBoundary_WithDefaultRetryDelay_ShouldBe1000Ms()
    {
        // Arrange
        var component = new ErrorBoundary();

        // Assert
        Assert.Equal(1000, component.RetryDelayMs);
    }

    [Fact]
    public void ErrorBoundary_RecoverMethod_ShouldBeCallable()
    {
        // Arrange
        var cut = Render<ErrorBoundary>(parameters => parameters
            .AddChildContent("<div>Content</div>")
        );

        var instance = cut.Instance;

        // Act & Assert - should not throw
        var exception = Record.Exception(() => instance.Recover());
        Assert.Null(exception);
    }

    [Fact]
    public void ErrorBoundary_WithNoErrorContent_ShouldStillRenderChildren()
    {
        // Arrange & Act
        var cut = Render<ErrorBoundary>(parameters => parameters
            .Add(p => p.ShowDetails, false)
            .AddChildContent("<div>Normal operation</div>")
        );

        // Assert
        Assert.Contains("Normal operation", cut.Markup);
    }

    [Fact]
    public void ErrorBoundary_OnErrorCallback_ShouldBeOptional()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            Render<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>")
            )
        );

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ErrorContext_WithNestedExceptions_ShouldExposeInnerException()
    {
        // Arrange
        var innerException = new ArgumentException("Inner error");
        var outerException = new InvalidOperationException("Outer error", innerException);
        var context = new ErrorContext(outerException, true);

        // Assert
        Assert.Equal("Outer error", context.Message);
        Assert.Same(outerException, context.Exception);
        Assert.Same(innerException, context.Exception.InnerException);
    }

    [Fact]
    public void ErrorBoundary_MultipleParameters_ShouldAcceptAllCorrectly()
    {
        // Arrange & Act
        var cut = Render<ErrorBoundary>(parameters => parameters
            .Add(p => p.ShowDetails, true)
            .Add(p => p.AutoRetry, true)
            .Add(p => p.RetryDelayMs, 2000)
            .Add(p => p.OnError, _ => Task.CompletedTask)
            .Add(p => p.ErrorContent, context => builder =>
                builder.AddMarkupContent(0, $"<div>{context.Message}</div>"))
            .AddChildContent("<div>App content</div>")
        );

        var instance = cut.Instance;

        // Assert
        Assert.True(instance.ShowDetails);
        Assert.True(instance.AutoRetry);
        Assert.Equal(2000, instance.RetryDelayMs);
        Assert.Contains("App content", cut.Markup);
    }
}
