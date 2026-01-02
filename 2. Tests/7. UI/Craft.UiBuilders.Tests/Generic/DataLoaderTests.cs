using Bunit;
using Craft.UiBuilders.Generic;
using Craft.UiBuilders.Tests.Base;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Tests.Generic;

/// <summary>
/// Unit tests for the DataLoader component.
/// Tests loading states, error handling, retry functionality, and custom templates.
/// </summary>
public class DataLoaderTests : ComponentTestBase
{
    [Fact]
    public void DataLoader_WhenLoading_ShouldDisplayDefaultSpinner()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        var overlay = cut.FindComponent<MudOverlay>();
        Assert.NotNull(overlay);
        Assert.True(overlay.Instance.Visible);
        
        var spinner = cut.FindComponent<Spinner>();
        Assert.NotNull(spinner);
        
        Assert.DoesNotContain("Content", cut.Markup);
    }

    [Fact]
    public void DataLoader_WhenNotLoadingAndNoError_ShouldDisplayContent()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, false)
            .AddChildContent("<div class=\"content\">Main Content</div>")
        );

        // Assert
        Assert.Contains("Main Content", cut.Markup);
        Assert.Contains("class=\"content\"", cut.Markup);
        
        var overlays = cut.FindComponents<MudOverlay>();
        Assert.Empty(overlays.Where(o => o.Instance.Visible));
    }

    [Fact]
    public void DataLoader_WithCustomLoadingTemplate_ShouldDisplayCustomTemplate()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.Loading, builder => builder.AddMarkupContent(0, "<div class=\"custom-loader\">Loading...</div>"))
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        Assert.Contains("custom-loader", cut.Markup);
        Assert.Contains("Loading...", cut.Markup);
        Assert.DoesNotContain("Content", cut.Markup);
    }

    [Fact]
    public void DataLoader_WhenHasError_ShouldDisplayDefaultErrorAlert()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, true)
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        var alert = cut.FindComponent<MudAlert>();
        Assert.NotNull(alert);
        Assert.Equal(Severity.Error, alert.Instance.Severity);
        
        Assert.Contains("Error", cut.Markup);
        Assert.Contains("Something went wrong while loading the data", cut.Markup);
        Assert.DoesNotContain("Content", cut.Markup);
    }

    [Fact]
    public void DataLoader_WithCustomErrorTemplate_ShouldDisplayCustomError()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, true)
            .Add(p => p.Error, builder => builder.AddMarkupContent(0, "<div class=\"custom-error\">Custom Error Message</div>"))
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        Assert.Contains("custom-error", cut.Markup);
        Assert.Contains("Custom Error Message", cut.Markup);
        Assert.DoesNotContain("Content", cut.Markup);
    }

    [Fact]
    public void DataLoader_WithCustomErrorMessages_ShouldDisplayCustomMessages()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, true)
            .Add(p => p.ErrorTitle, "Connection Failed")
            .Add(p => p.ErrorMessage, "Unable to connect to the server. Please check your internet connection.")
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        Assert.Contains("Connection Failed", cut.Markup);
        Assert.Contains("Unable to connect to the server", cut.Markup);
    }

    [Fact]
    public async Task DataLoader_WithRetryCallback_ShouldDisplayRetryButton()
    {
        // Arrange
        var retryClicked = false;
        
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, true)
            .Add(p => p.OnRetry, EventCallback.Factory.Create(this, () => retryClicked = true))
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        var button = cut.FindComponent<MudButton>();
        Assert.NotNull(button);
        Assert.Contains("Retry", cut.Markup);
    }

    [Fact]
    public async Task DataLoader_ClickingRetryButton_ShouldInvokeCallback()
    {
        // Arrange
        var retryClicked = false;
        
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, true)
            .Add(p => p.OnRetry, EventCallback.Factory.Create(this, () => retryClicked = true))
            .AddChildContent("<div>Content</div>")
        );

        var button = cut.FindComponent<MudButton>();

        // Act
        await button.Instance.OnClick.InvokeAsync();

        // Assert
        Assert.True(retryClicked);
    }

    [Fact]
    public void DataLoader_WithoutRetryCallback_ShouldNotDisplayRetryButton()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, true)
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        var buttons = cut.FindComponents<MudButton>();
        Assert.Empty(buttons);
    }

    [Fact]
    public void DataLoader_WithCustomRetryText_ShouldDisplayCustomText()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, true)
            .Add(p => p.RetryText, "Try Again")
            .Add(p => p.OnRetry, EventCallback.Factory.Create(this, () => { }))
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        Assert.Contains("Try Again", cut.Markup);
    }

    [Fact]
    public void DataLoader_WithCustomLoadingColor_ShouldUseSpecifiedColor()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.LoadingColor, Color.Secondary)
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        var spinner = cut.FindComponent<Spinner>();
        Assert.NotNull(spinner);
        Assert.Equal(Color.Secondary, spinner.Instance.Color);
    }

    [Fact]
    public void DataLoader_ShouldInheritFromCraftComponent()
    {
        // Arrange
        var component = new DataLoader();

        // Assert
        Assert.IsAssignableFrom<Craft.UiComponents.CraftComponent>(component);
    }

    [Theory]
    [InlineData(true, false, false)] // Loading
    [InlineData(false, true, false)] // Error
    [InlineData(false, false, true)] // Success
    public void DataLoader_WithDifferentStates_ShouldRenderCorrectly(bool isLoading, bool hasError, bool showContent)
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, isLoading)
            .Add(p => p.HasError, hasError)
            .AddChildContent("<div class=\"test-content\">Content</div>")
        );

        // Assert
        if (showContent)
        {
            Assert.Contains("test-content", cut.Markup);
        }
        else
        {
            Assert.DoesNotContain("test-content", cut.Markup);
        }
    }

    [Fact]
    public void DataLoader_LoadingState_ShouldHideContent()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .AddChildContent("<div class=\"loaded-content\">Loaded Data</div>")
        );

        // Assert
        Assert.DoesNotContain("loaded-content", cut.Markup);
        var spinners = cut.FindComponents<Spinner>();
        Assert.NotEmpty(spinners);
    }

    [Fact]
    public void DataLoader_LoadedState_ShouldShowContent()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, false)
            .AddChildContent("<div class=\"loaded-content\">Loaded Data</div>")
        );

        // Assert
        Assert.Contains("loaded-content", cut.Markup);
        Assert.Contains("Loaded Data", cut.Markup);
    }

    [Fact]
    public void DataLoader_ErrorState_ShouldShowAlert()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, true)
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        var alert = cut.FindComponent<MudAlert>();
        Assert.NotNull(alert);
        Assert.DoesNotContain("Content", cut.Markup);
    }

    [Fact]
    public void DataLoader_WithComplexContent_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, false)
            .AddChildContent(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "card");
                builder.OpenElement(2, "h1");
                builder.AddContent(3, "Title");
                builder.CloseElement();
                builder.OpenElement(4, "p");
                builder.AddContent(5, "Description");
                builder.CloseElement();
                builder.CloseElement();
            })
        );

        // Assert
        Assert.Contains("class=\"card\"", cut.Markup);
        Assert.Contains("Title", cut.Markup);
        Assert.Contains("Description", cut.Markup);
    }

    [Fact]
    public void DataLoader_WithNestedComponents_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, false)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<Show>(0);
                builder.AddComponentParameter(1, "When", true);
                builder.AddComponentParameter(2, "ChildContent", (RenderFragment)(b =>
                {
                    b.AddContent(0, "Nested content");
                }));
                builder.CloseComponent();
            })
        );

        // Assert
        Assert.Contains("Nested content", cut.Markup);
    }

    [Fact]
    public void DataLoader_StateScenario_Loading_ShouldHideContent()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        Assert.DoesNotContain("Content", cut.Markup);
    }

    [Fact]
    public void DataLoader_StateScenario_Loaded_ShouldShowContent()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, false)
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        Assert.Contains("Content", cut.Markup);
    }

    [Fact]
    public void DataLoader_StateScenario_Error_ShouldShowError()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, true)
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        Assert.DoesNotContain("Content", cut.Markup);
        var alert = cut.FindComponent<MudAlert>();
        Assert.NotNull(alert);
    }

    [Fact]
    public void DataLoader_WithEmptyContent_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() =>
            Render<DataLoader>(parameters => parameters
                .Add(p => p.IsLoading, false)
                .Add(p => p.HasError, false)
            )
        );

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void DataLoader_DefaultErrorMessages_ShouldBeSet()
    {
        // Arrange
        var component = new DataLoader();

        // Assert
        Assert.Equal("Error", component.ErrorTitle);
        Assert.Equal("Something went wrong while loading the data. Please try again.", component.ErrorMessage);
        Assert.Equal("Retry", component.RetryText);
    }

    [Fact]
    public void DataLoader_DefaultLoadingColor_ShouldBePrimary()
    {
        // Arrange
        var component = new DataLoader();

        // Assert
        Assert.Equal(Color.Primary, component.LoadingColor);
    }

    [Theory]
    [InlineData(Color.Primary)]
    [InlineData(Color.Secondary)]
    [InlineData(Color.Success)]
    [InlineData(Color.Error)]
    [InlineData(Color.Warning)]
    [InlineData(Color.Info)]
    public void DataLoader_WithDifferentLoadingColors_ShouldUseCorrectColor(Color color)
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.LoadingColor, color)
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        var spinner = cut.FindComponent<Spinner>();
        Assert.Equal(color, spinner.Instance.Color);
    }

    [Fact]
    public void DataLoader_ErrorWithoutOnRetry_ShouldNotShowRetryButton()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, true)
            .AddChildContent("<div>Content</div>")
        );

        // Assert
        var showComponent = cut.FindComponents<Show>().FirstOrDefault(s => 
            s.Markup.Contains("Retry"));
        
        // The Show component should exist but shouldn't render content
        Assert.DoesNotContain("Retry", cut.Markup.Replace("OnRetry", "")); // Exclude OnRetry parameter name
    }

    [Fact]
    public void DataLoader_UsesIfAndShowComponents_CorrectlyForPattern()
    {
        // Arrange & Act
        var cut = Render<DataLoader>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .Add(p => p.HasError, true)
            .Add(p => p.OnRetry, EventCallback.Factory.Create(this, () => { }))
            .AddChildContent("<div>Content</div>")
        );

        // Assert - Verify component composition
        var ifComponents = cut.FindComponents<If>();
        Assert.NotEmpty(ifComponents); // Should use If component for conditional rendering
        
        var showComponents = cut.FindComponents<Show>();
        Assert.NotEmpty(showComponents); // Should use Show component for retry button
    }
}
