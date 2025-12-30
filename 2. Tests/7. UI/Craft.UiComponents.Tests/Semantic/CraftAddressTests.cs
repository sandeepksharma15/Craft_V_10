using Bunit;
using Craft.UiComponents.Semantic;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Tests.Semantic;

public class CraftAddressTests : ComponentTestBase
{
    [Fact]
    public void CraftAddress_ShouldRenderAddressElement()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAddress>(0);
            builder.CloseComponent();
        });

        // Assert
        var address = cut.Find("address");
        Assert.NotNull(address);
    }

    [Fact]
    public void CraftAddress_ShouldRenderWithId()
    {
        // Arrange
        var expectedId = "test-address";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAddress>(0);
            builder.AddAttribute(1, "Id", expectedId);
            builder.CloseComponent();
        });

        // Assert
        var address = cut.Find("address");
        Assert.Equal(expectedId, address.Id);
    }

    [Fact]
    public void CraftAddress_ShouldRenderWithClass()
    {
        // Arrange
        var expectedClass = "custom-class";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAddress>(0);
            builder.AddAttribute(1, "Class", expectedClass);
            builder.CloseComponent();
        });

        // Assert
        var address = cut.Find("address");
        Assert.Contains(expectedClass, address.ClassName);
    }

    [Fact]
    public void CraftAddress_ShouldRenderWithStyle()
    {
        // Arrange
        var expectedStyle = "color: blue;";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAddress>(0);
            builder.AddAttribute(1, "Style", expectedStyle);
            builder.CloseComponent();
        });

        // Assert
        var address = cut.Find("address");
        Assert.Contains(expectedStyle, address.GetAttribute("style"));
    }

    [Fact]
    public void CraftAddress_ShouldRenderWithChildContent()
    {
        // Arrange
        var expectedContent = "123 Main Street, City, State 12345";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAddress>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(0, expectedContent);
            }));
            builder.CloseComponent();
        });

        // Assert
        var address = cut.Find("address");
        Assert.Contains(expectedContent, address.TextContent);
    }

    [Fact]
    public void CraftAddress_ShouldNotHaveRedundantRoleAttribute()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAddress>(0);
            builder.CloseComponent();
        });

        // Assert - address element should not have role attribute (it's implicit)
        var address = cut.Find("address");
        Assert.Null(address.GetAttribute("role"));
    }

    [Fact]
    public void CraftAddress_ShouldRenderWithUserAttributes()
    {
        // Arrange
        var dataAttr = "contact-info";

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAddress>(0);
            builder.AddAttribute(1, "data-type", dataAttr);
            builder.CloseComponent();
        });

        // Assert
        var address = cut.Find("address");
        Assert.Equal(dataAttr, address.GetAttribute("data-type"));
    }

    [Fact]
    public void CraftAddress_ShouldHaveComponentCssClass()
    {
        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CraftAddress>(0);
            builder.CloseComponent();
        });

        // Assert
        var address = cut.Find("address");
        Assert.Contains("craft-address", address.ClassName);
    }
}
