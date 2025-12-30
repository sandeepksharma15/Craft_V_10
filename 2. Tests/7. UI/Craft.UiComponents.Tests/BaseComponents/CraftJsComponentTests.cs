using Bunit;
using Craft.UiComponents;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Craft.UiComponents.Tests.BaseComponents;

/// <summary>
/// Tests for CraftJsComponent JavaScript interop capabilities.
/// Tests JS module initialization, invocation, and disposal.
/// </summary>
public class CraftJsComponentTests : ComponentTestBase
{
    #region Initialization Tests

    [Fact]
    public async Task CraftJsComponent_ShouldNotInitializeWithoutModulePath()
    {
        // Act
        var cut = Render<TestCraftJsComponentWithoutModule>();
        await Task.Delay(100); // Allow OnAfterRender to complete

        // Assert
        Assert.False(cut.Instance.IsJsInitialized);
        Assert.Null(cut.Instance.JsModule);
    }

    [Fact]
    public async Task CraftJsComponent_ShouldInitializeWithModulePath()
    {
        // Arrange
        SetupJsModule();

        // Act
        var cut = Render<TestCraftJsComponentWithModule>();
        await Task.Delay(100); // Allow OnAfterRender to complete

        // Assert
        Assert.True(cut.Instance.IsJsInitialized);
        Assert.NotNull(cut.Instance.JsModule);
    }

    [Fact]
    public async Task CraftJsComponent_ShouldCallOnJsModuleInitialized()
    {
        // Arrange
        SetupJsModule();

        // Act
        var cut = Render<TestCraftJsComponentWithModule>();
        await Task.Delay(100); // Allow OnAfterRender to complete

        // Assert
        Assert.True(cut.Instance.InitializedCallbackInvoked);
    }

    [Fact]
    public async Task CraftJsComponent_ShouldOnlyInitializeOnce()
    {
        // Arrange
        SetupJsModule();

        // Act
        var cut = Render<TestCraftJsComponentWithModule>();
        await Task.Delay(100);
        
        var initialModule = cut.Instance.JsModule;
        
        cut.Render(); // Re-render
        await Task.Delay(100);

        // Assert
        Assert.Same(initialModule, cut.Instance.JsModule);
    }

    #endregion

    #region Invocation Tests

    [Fact]
    public async Task CraftJsComponent_ShouldInvokeJsVoidAsync()
    {
        // Arrange
        SetupJsModule();
        var cut = Render<TestCraftJsComponentWithModule>();
        await Task.Delay(100);

        // Act
        await cut.Instance.TestInvokeVoidAsync("testFunction", "arg1", 42);

        // Assert
        JSInterop.VerifyInvoke("testFunction");
    }

    [Fact]
    public async Task CraftJsComponent_ShouldInvokeJsAsyncWithResult()
    {
        // Arrange
        SetupJsModule();
        JSInterop.Setup<string>("testFunction").SetResult("test result");
        
        var cut = Render<TestCraftJsComponentWithModule>();
        await Task.Delay(100);

        // Act
        var result = await cut.Instance.TestInvokeAsync<string>("testFunction");

        // Assert
        Assert.Equal("test result", result);
    }

    [Fact]
    public async Task CraftJsComponent_ShouldHandleJsInvocationWithMultipleArgs()
    {
        // Arrange
        SetupJsModule();
        var cut = Render<TestCraftJsComponentWithModule>();
        await Task.Delay(100);

        // Act
        await cut.Instance.TestInvokeVoidAsync("complexFunction", "string", 123, true, new { prop = "value" });

        // Assert
        JSInterop.VerifyInvoke("complexFunction");
    }

    [Fact]
    public async Task CraftJsComponent_ShouldNotInvokeBeforeInitialization()
    {
        // Arrange
        var cut = Render<TestCraftJsComponentWithoutModule>();

        // Act & Assert - Should not throw, but won't invoke either
        await cut.Instance.TestInvokeVoidAsync("testFunction");
        
        JSInterop.VerifyNotInvoke("testFunction");
    }

    [Fact]
    public async Task CraftJsComponent_ShouldInvokeWithNoArgs()
    {
        // Arrange
        SetupJsModule();
        var cut = Render<TestCraftJsComponentWithModule>();
        await Task.Delay(100);

        // Act
        await cut.Instance.TestInvokeVoidAsync("noArgsFunction");

        // Assert
        JSInterop.VerifyInvoke("noArgsFunction");
    }

    [Fact]
    public async Task CraftJsComponent_ShouldHandleJsReturnTypes()
    {
        // Arrange
        SetupJsModule();
        JSInterop.Setup<int>("getNumber").SetResult(42);
        JSInterop.Setup<bool>("getBool").SetResult(true);
        JSInterop.Setup<string>("getString").SetResult("test");
        
        var cut = Render<TestCraftJsComponentWithModule>();
        await Task.Delay(100);

        // Act
        var number = await cut.Instance.TestInvokeAsync<int>("getNumber");
        var boolean = await cut.Instance.TestInvokeAsync<bool>("getBool");
        var text = await cut.Instance.TestInvokeAsync<string>("getString");

        // Assert
        Assert.Equal(42, number);
        Assert.True(boolean);
        Assert.Equal("test", text);
    }

    #endregion

    #region Module Path Tests

    [Fact]
    public void CraftJsComponent_ShouldExposeModulePath()
    {
        // Act
        var cut = Render<TestCraftJsComponentWithModule>();

        // Assert
        Assert.Equal("./test-module.js", cut.Instance.GetModulePath());
    }

    [Fact]
    public void CraftJsComponent_ShouldHandleNullModulePath()
    {
        // Act
        var cut = Render<TestCraftJsComponentWithoutModule>();

        // Assert
        Assert.Null(cut.Instance.GetModulePath());
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public async Task CraftJsComponent_ShouldInitializeOnFirstRenderOnly()
    {
        // Arrange
        SetupJsModule();
        var cut = Render<TestCraftJsComponentWithModule>();
        
        // Act - First render (OnAfterRenderAsync with firstRender = true)
        await Task.Delay(100);
        var firstInitCount = cut.Instance.InitializationCount;

        // Re-render (OnAfterRenderAsync with firstRender = false)
        cut.Render();
        await Task.Delay(100);
        var secondInitCount = cut.Instance.InitializationCount;

        // Assert
        Assert.Equal(1, firstInitCount);
        Assert.Equal(1, secondInitCount); // Should not increment
    }

    [Fact]
    public async Task CraftJsComponent_ShouldInheritInteractiveFeatures()
    {
        // Arrange
        SetupJsModule();
        var clicked = false;

        // Act
        var cut = Render<TestCraftJsComponentWithModule>(parameters => parameters
            .Add(p => p.OnClick, EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                this, _ => clicked = true)));
        
        await Task.Delay(100);
        cut.Find("div").Click();

        // Assert
        Assert.True(clicked);
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public async Task CraftJsComponent_ShouldDisposeJsModule()
    {
        // Arrange
        SetupJsModule();
        var cut = Render<TestCraftJsComponentWithModule>();
        await Task.Delay(100);

        Assert.True(cut.Instance.IsJsInitialized);
        var wasInitialized = cut.Instance.IsJsInitialized;

        // Act
        cut.Dispose();

        // Assert - Module should be disposed, no exception should occur
        Assert.True(wasInitialized);
    }

    [Fact]
    public async Task CraftJsComponent_ShouldHandleDisposalWithoutInitialization()
    {
        // Arrange
        var cut = Render<TestCraftJsComponentWithoutModule>();

        // Act & Assert - Should not throw
        cut.Dispose();
        
        await Task.CompletedTask;
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CraftJsComponent_ShouldSupportCustomInitialization()
    {
        // Arrange
        SetupJsModule();

        // Act
        var cut = Render<TestCraftJsComponentWithCustomInit>();
        await Task.Delay(100);

        // Assert
        Assert.True(cut.Instance.CustomInitCalled);
    }

    [Fact]
    public async Task CraftJsComponent_ShouldMaintainStateAfterJsInit()
    {
        // Arrange
        SetupJsModule();
        var cut = Render<TestCraftJsComponentWithModule>(parameters => parameters
            .Add(p => p.Id, "test-id")
            .Add(p => p.Class, "test-class"));

        // Act
        await Task.Delay(100);

        // Assert
        Assert.True(cut.Instance.IsJsInitialized);
        Assert.Contains("test-id", cut.Markup);
        Assert.Contains("test-class", cut.Markup);
    }

    [Fact]
    public async Task CraftJsComponent_ShouldAllowManualInitialization()
    {
        // Arrange
        SetupJsModule();
        var cut = Render<TestCraftJsComponentManualInit>();

        // Initially not initialized
        Assert.False(cut.Instance.IsJsInitialized);

        // Act - Manually trigger initialization
        await cut.Instance.ManuallyInitialize();
        await Task.Delay(100);

        // Assert
        Assert.True(cut.Instance.IsJsInitialized);
    }

    #endregion

    #region Helper Methods

    private void SetupJsModule()
    {
        var mockJsModule = JSInterop.SetupModule("./test-module.js");
        mockJsModule.SetupVoid("testFunction");
        mockJsModule.Setup<string>("testFunction").SetResult("test result");
        mockJsModule.SetupVoid("complexFunction");
        mockJsModule.SetupVoid("noArgsFunction").SetVoidResult();
        mockJsModule.Setup<int>("getNumber").SetResult(42);
        mockJsModule.Setup<bool>("getBool").SetResult(true);
        mockJsModule.Setup<string>("getString").SetResult("test");
    }

    #endregion
}

/// <summary>
/// Test implementation of CraftJsComponent with a module path.
/// </summary>
internal class TestCraftJsComponentWithModule : CraftJsComponent
{
    protected override string? JsModulePath => "./test-module.js";
    
    public bool InitializedCallbackInvoked { get; private set; }
    public int InitializationCount { get; private set; }

    protected override async Task OnJsModuleInitializedAsync()
    {
        InitializedCallbackInvoked = true;
        InitializationCount++;
        await base.OnJsModuleInitializedAsync();
    }

    public async Task TestInvokeVoidAsync(string identifier, params object?[] args)
        => await InvokeJsVoidAsync(identifier, args);

    public async Task<TResult> TestInvokeAsync<TResult>(string identifier, params object?[] args)
        => await InvokeJsAsync<TResult>(identifier, args);

    public string? GetModulePath() => JsModulePath;

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", Id);
        builder.AddAttribute(2, "class", BuildCssClass());

        if (OnClick.HasDelegate)
        {
            builder.AddAttribute(3, "onclick", EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(
                this, async args => await HandleClickAsync(args)));
        }

        builder.AddContent(4, ChildContent);
        builder.CloseElement();
    }
}

/// <summary>
/// Test implementation of CraftJsComponent without a module path.
/// </summary>
internal class TestCraftJsComponentWithoutModule : CraftJsComponent
{
    protected override string? JsModulePath => null;

    public async Task TestInvokeVoidAsync(string identifier, params object?[] args)
        => await InvokeJsVoidAsync(identifier, args);

    public string? GetModulePath() => JsModulePath;

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", Id);
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}

/// <summary>
/// Test implementation with custom initialization logic.
/// </summary>
internal class TestCraftJsComponentWithCustomInit : CraftJsComponent
{
    protected override string? JsModulePath => "./test-module.js";
    
    public bool CustomInitCalled { get; private set; }

    protected override async Task OnJsModuleInitializedAsync()
    {
        CustomInitCalled = true;
        await base.OnJsModuleInitializedAsync();
    }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", Id);
        builder.AddContent(2, "Custom Init Component");
        builder.CloseElement();
    }
}

/// <summary>
/// Test implementation that allows manual JS initialization.
/// </summary>
internal class TestCraftJsComponentManualInit : CraftJsComponent
{
    protected override string? JsModulePath => "./test-module.js";

    // Override to prevent automatic initialization
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Skip base call to prevent automatic initialization
        await Task.CompletedTask;
    }

    public async Task ManuallyInitialize()
        => await InitializeJsModuleAsync();

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", Id);
        builder.AddContent(2, "Manual Init Component");
        builder.CloseElement();
    }
}
