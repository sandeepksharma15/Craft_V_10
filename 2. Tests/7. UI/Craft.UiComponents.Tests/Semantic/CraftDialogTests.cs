using Bunit;
using Craft.UiComponents.Semantic;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftDialogTests : ComponentTestBase
{
    [Fact]
    public void CraftDialog_ShouldRenderDialogElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        Assert.NotNull(dialog);
    }

    [Fact]
    public void CraftDialog_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "confirmation-dialog";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        Assert.Equal(expectedId, dialog.Id);
    }

    [Fact]
    public void CraftDialog_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "modal-dialog";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        Assert.Contains(expectedClass, dialog.ClassName);
    }

    [Fact]
    public void CraftDialog_ShouldHaveComponentCssClass()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        Assert.Contains("craft-dialog", dialog.ClassName);
    }

    [Fact]
    public void CraftDialog_IsModalTrueShouldAddModalClass()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.AddAttribute(1, "IsModal", true);
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        Assert.Contains("craft-dialog-modal", dialog.ClassName);
    }

    [Fact]
    public void CraftDialog_IsModalFalseShouldNotAddModalClass()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.AddAttribute(1, "IsModal", false);
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        Assert.DoesNotContain("craft-dialog-modal", dialog.ClassName);
    }

    [Fact]
    public void CraftDialog_ShouldRenderWithChildContent()
    {
        // Arrange
        var expectedContent = "Are you sure you want to continue?";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        Assert.Contains(expectedContent, dialog.TextContent);
    }

    [Fact]
    public void CraftDialog_IsOpenTrueShouldSetOpenAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.AddAttribute(1, "IsOpen", true);
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        Assert.True(dialog.HasAttribute("open"));
    }

    [Fact]
    public void CraftDialog_IsOpenFalseShouldNotSetOpenAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.AddAttribute(1, "IsOpen", false);
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        Assert.False(dialog.HasAttribute("open"));
    }

    [Fact]
    public void CraftDialog_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var ariaLabel = "Confirmation Dialog";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.AddAttribute(1, "aria-label", ariaLabel);
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        Assert.Equal(ariaLabel, dialog.GetAttribute("aria-label"));
    }

    [Fact]
    public void CraftDialog_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "width: 500px;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        Assert.Contains(expectedStyle, dialog.GetAttribute("style"));
    }

    [Fact]
    public void CraftDialog_ShouldRenderWithComplexContent()
    {
        // Arrange
        var titleText = "Warning";
        var messageText = "This action cannot be undone.";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftDialog>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(0, "h2");
                childBuilder.AddContent(1, titleText);
                childBuilder.CloseElement();
                childBuilder.OpenElement(2, "p");
                childBuilder.AddContent(3, messageText);
                childBuilder.CloseElement();
            }));
            builder.CloseComponent();
        });

        // Assert
        var dialog = cut.Find("dialog");
        var h2 = dialog.QuerySelector("h2");
        var p = dialog.QuerySelector("p");
        Assert.NotNull(h2);
        Assert.NotNull(p);
        Assert.Equal(titleText, h2.TextContent);
        Assert.Equal(messageText, p.TextContent);
    }
}
