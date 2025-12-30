using Bunit;
using Craft.UiComponents;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Craft.UiComponents.Tests.BaseComponents;

/// <summary>
/// Tests for CraftInteractiveComponent event handling capabilities.
/// Tests all mouse, keyboard, focus, and touch event callbacks.
/// </summary>
public class CraftInteractiveComponentTests : ComponentTestBase
{
    #region Mouse Events Tests

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnDoubleClick()
    {
        // Arrange
        MouseEventArgs? receivedArgs = null;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnDoubleClick, EventCallback.Factory.Create<MouseEventArgs>(
                this, args => receivedArgs = args)));

        cut.Find("div").DoubleClick();

        // Assert
        Assert.NotNull(receivedArgs);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnMouseEnter()
    {
        // Arrange
        var eventFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnMouseEnter, EventCallback.Factory.Create<MouseEventArgs>(
                this, _ => eventFired = true)));

        cut.Find("div").MouseEnter();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnMouseLeave()
    {
        // Arrange
        var eventFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnMouseLeave, EventCallback.Factory.Create<MouseEventArgs>(
                this, _ => eventFired = true)));

        cut.Find("div").MouseLeave();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnMouseDown()
    {
        // Arrange
        var eventFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnMouseDown, EventCallback.Factory.Create<MouseEventArgs>(
                this, _ => eventFired = true)));

        cut.Find("div").MouseDown();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnMouseUp()
    {
        // Arrange
        var eventFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnMouseUp, EventCallback.Factory.Create<MouseEventArgs>(
                this, _ => eventFired = true)));

        cut.Find("div").MouseUp();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnMouseMove()
    {
        // Arrange
        var moveCount = 0;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnMouseMove, EventCallback.Factory.Create<MouseEventArgs>(
                this, _ => moveCount++)));

        cut.Find("div").MouseMove();
        cut.Find("div").MouseMove();

        // Assert
        Assert.Equal(2, moveCount);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnContextMenu()
    {
        // Arrange
        var eventFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnContextMenu, EventCallback.Factory.Create<MouseEventArgs>(
                this, _ => eventFired = true)));

        cut.Find("div").ContextMenu();

        // Assert
        Assert.True(eventFired);
    }

    #endregion

    #region Keyboard Events Tests

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnKeyDown()
    {
        // Arrange
        KeyboardEventArgs? receivedArgs = null;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnKeyDown, EventCallback.Factory.Create<KeyboardEventArgs>(
                this, args => receivedArgs = args)));

        cut.Find("div").KeyDown("Enter");

        // Assert
        Assert.NotNull(receivedArgs);
        Assert.Equal("Enter", receivedArgs.Key);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnKeyUp()
    {
        // Arrange
        var eventFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnKeyUp, EventCallback.Factory.Create<KeyboardEventArgs>(
                this, _ => eventFired = true)));

        cut.Find("div").KeyUp("Escape");

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnKeyPress()
    {
        // Arrange
        var pressCount = 0;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnKeyPress, EventCallback.Factory.Create<KeyboardEventArgs>(
                this, _ => pressCount++)));

        cut.Find("div").KeyPress("a");
        cut.Find("div").KeyPress("b");
        cut.Find("div").KeyPress("c");

        // Assert
        Assert.Equal(3, pressCount);
    }

    #endregion

    #region Focus Events Tests

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnFocus()
    {
        // Arrange
        FocusEventArgs? receivedArgs = null;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnFocus, EventCallback.Factory.Create<FocusEventArgs>(
                this, args => receivedArgs = args)));

        cut.Find("div").Focus();

        // Assert
        Assert.NotNull(receivedArgs);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnBlur()
    {
        // Arrange
        var eventFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnBlur, EventCallback.Factory.Create<FocusEventArgs>(
                this, _ => eventFired = true)));

        cut.Find("div").Blur();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleFocusCycle()
    {
        // Arrange
        var focusCount = 0;
        var blurCount = 0;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnFocus, EventCallback.Factory.Create<FocusEventArgs>(this, _ => focusCount++))
            .Add(p => p.OnBlur, EventCallback.Factory.Create<FocusEventArgs>(this, _ => blurCount++)));

        cut.Find("div").Focus();
        cut.Find("div").Blur();
        cut.Find("div").Focus();

        // Assert
        Assert.Equal(2, focusCount);
        Assert.Equal(1, blurCount);
    }

    #endregion

    #region Touch Events Tests

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnTouchStart()
    {
        // Arrange
        TouchEventArgs? receivedArgs = null;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnTouchStart, EventCallback.Factory.Create<TouchEventArgs>(
                this, args => receivedArgs = args)));

        cut.Find("div").TouchStart();

        // Assert
        Assert.NotNull(receivedArgs);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnTouchEnd()
    {
        // Arrange
        var eventFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnTouchEnd, EventCallback.Factory.Create<TouchEventArgs>(
                this, _ => eventFired = true)));

        cut.Find("div").TouchEnd();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnTouchMove()
    {
        // Arrange
        var moveCount = 0;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnTouchMove, EventCallback.Factory.Create<TouchEventArgs>(
                this, _ => moveCount++)));

        cut.Find("div").TouchMove();
        cut.Find("div").TouchMove();
        cut.Find("div").TouchMove();

        // Assert
        Assert.Equal(3, moveCount);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleOnTouchCancel()
    {
        // Arrange
        var eventFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnTouchCancel, EventCallback.Factory.Create<TouchEventArgs>(
                this, _ => eventFired = true)));

        cut.Find("div").TouchCancel();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleCompleteTouchGesture()
    {
        // Arrange
        var startFired = false;
        var moveFired = false;
        var endFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnTouchStart, EventCallback.Factory.Create<TouchEventArgs>(this, _ => startFired = true))
            .Add(p => p.OnTouchMove, EventCallback.Factory.Create<TouchEventArgs>(this, _ => moveFired = true))
            .Add(p => p.OnTouchEnd, EventCallback.Factory.Create<TouchEventArgs>(this, _ => endFired = true)));

        cut.Find("div").TouchStart();
        cut.Find("div").TouchMove();
        cut.Find("div").TouchEnd();

        // Assert
        Assert.True(startFired);
        Assert.True(moveFired);
        Assert.True(endFired);
    }

    #endregion

    #region Multiple Event Handlers Tests

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleMultipleMouseEvents()
    {
        // Arrange
        var enterCount = 0;
        var moveCount = 0;
        var leaveCount = 0;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnMouseEnter, EventCallback.Factory.Create<MouseEventArgs>(this, _ => enterCount++))
            .Add(p => p.OnMouseMove, EventCallback.Factory.Create<MouseEventArgs>(this, _ => moveCount++))
            .Add(p => p.OnMouseLeave, EventCallback.Factory.Create<MouseEventArgs>(this, _ => leaveCount++)));

        cut.Find("div").MouseEnter();
        cut.Find("div").MouseMove();
        cut.Find("div").MouseMove();
        cut.Find("div").MouseLeave();

        // Assert
        Assert.Equal(1, enterCount);
        Assert.Equal(2, moveCount);
        Assert.Equal(1, leaveCount);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleMultipleKeyboardEvents()
    {
        // Arrange
        var downCount = 0;
        var pressCount = 0;
        var upCount = 0;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnKeyDown, EventCallback.Factory.Create<KeyboardEventArgs>(this, _ => downCount++))
            .Add(p => p.OnKeyPress, EventCallback.Factory.Create<KeyboardEventArgs>(this, _ => pressCount++))
            .Add(p => p.OnKeyUp, EventCallback.Factory.Create<KeyboardEventArgs>(this, _ => upCount++)));

        cut.Find("div").KeyDown("Enter");
        cut.Find("div").KeyPress("Enter");
        cut.Find("div").KeyUp("Enter");

        // Assert
        Assert.Equal(1, downCount);
        Assert.Equal(1, pressCount);
        Assert.Equal(1, upCount);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldHandleMultipleEventTypesSimultaneously()
    {
        // Arrange
        var mouseEventFired = false;
        var keyEventFired = false;
        var focusEventFired = false;
        var touchEventFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnMouseEnter, EventCallback.Factory.Create<MouseEventArgs>(this, _ => mouseEventFired = true))
            .Add(p => p.OnKeyDown, EventCallback.Factory.Create<KeyboardEventArgs>(this, _ => keyEventFired = true))
            .Add(p => p.OnFocus, EventCallback.Factory.Create<FocusEventArgs>(this, _ => focusEventFired = true))
            .Add(p => p.OnTouchStart, EventCallback.Factory.Create<TouchEventArgs>(this, _ => touchEventFired = true)));

        cut.Find("div").MouseEnter();
        cut.Find("div").KeyDown("Tab");
        cut.Find("div").Focus();
        cut.Find("div").TouchStart();

        // Assert
        Assert.True(mouseEventFired);
        Assert.True(keyEventFired);
        Assert.True(focusEventFired);
        Assert.True(touchEventFired);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void CraftInteractiveComponent_ShouldInheritCraftComponentFeatures()
    {
        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.Id, "test-interactive-id")
            .Add(p => p.Class, "custom-interactive-class")
            .Add(p => p.Disabled, true));

        // Assert
        Assert.Contains("test-interactive-id", cut.Markup);
        Assert.Contains("custom-interactive-class", cut.Markup);
        Assert.Contains("craft-disabled", cut.Markup);
    }

    [Fact]
    public void CraftInteractiveComponent_ShouldSupportBaseClassOnClick()
    {
        // Arrange
        var baseClickFired = false;

        // Act
        var cut = Render<TestCraftInteractiveComponent>(parameters => parameters
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(
                this, _ => baseClickFired = true)));

        cut.Find("div").Click();

        // Assert
        Assert.True(baseClickFired);
    }

    #endregion
}

/// <summary>
/// Test implementation of CraftInteractiveComponent for testing purposes.
/// </summary>
internal class TestCraftInteractiveComponent : CraftInteractiveComponent
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", Id);
        builder.AddAttribute(2, "class", BuildCssClass());
        
        var style = BuildStyle();
        if (!string.IsNullOrEmpty(style))
        {
            builder.AddAttribute(3, "style", style);
        }

        builder.AddMultipleAttributes(4, UserAttributes);

        // Base click event
        if (OnClick.HasDelegate)
        {
            builder.AddAttribute(5, "onclick", EventCallback.Factory.Create<MouseEventArgs>(
                this, async args => await HandleClickAsync(args)));
        }

        // Mouse events
        if (OnDoubleClick.HasDelegate)
            builder.AddAttribute(6, "ondblclick", OnDoubleClick);
        if (OnMouseEnter.HasDelegate)
            builder.AddAttribute(7, "onmouseenter", OnMouseEnter);
        if (OnMouseLeave.HasDelegate)
            builder.AddAttribute(8, "onmouseleave", OnMouseLeave);
        if (OnMouseDown.HasDelegate)
            builder.AddAttribute(9, "onmousedown", OnMouseDown);
        if (OnMouseUp.HasDelegate)
            builder.AddAttribute(10, "onmouseup", OnMouseUp);
        if (OnMouseMove.HasDelegate)
            builder.AddAttribute(11, "onmousemove", OnMouseMove);
        if (OnContextMenu.HasDelegate)
            builder.AddAttribute(12, "oncontextmenu", OnContextMenu);

        // Keyboard events
        if (OnKeyDown.HasDelegate)
            builder.AddAttribute(13, "onkeydown", OnKeyDown);
        if (OnKeyUp.HasDelegate)
            builder.AddAttribute(14, "onkeyup", OnKeyUp);
        if (OnKeyPress.HasDelegate)
            builder.AddAttribute(15, "onkeypress", OnKeyPress);

        // Focus events
        if (OnFocus.HasDelegate)
            builder.AddAttribute(16, "onfocus", OnFocus);
        if (OnBlur.HasDelegate)
            builder.AddAttribute(17, "onblur", OnBlur);

        // Touch events
        if (OnTouchStart.HasDelegate)
            builder.AddAttribute(18, "ontouchstart", OnTouchStart);
        if (OnTouchEnd.HasDelegate)
            builder.AddAttribute(19, "ontouchend", OnTouchEnd);
        if (OnTouchMove.HasDelegate)
            builder.AddAttribute(20, "ontouchmove", OnTouchMove);
        if (OnTouchCancel.HasDelegate)
            builder.AddAttribute(21, "ontouchcancel", OnTouchCancel);

        builder.AddContent(22, ChildContent);
        builder.CloseElement();
    }
}
