using Bunit;
using Craft.UiComponents;
using Craft.UiComponents.Enums;
using Craft.UiComponents.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;
using MudBlazor;
using MudBlazor.Services;

namespace Craft.UiBuilders.Tests.Base;

/// <summary>
/// Base class for Craft UI Builder component tests that provides common test infrastructure
/// including mock services registration and MudBlazor services.
/// </summary>
public abstract class ComponentTestBase : TestContext
{
    protected Mock<IThemeService> MockThemeService { get; }

    protected ComponentTestBase()
    {
        // Create and configure mock theme service
        MockThemeService = new Mock<IThemeService>();
        MockThemeService.Setup(x => x.CurrentTheme).Returns(Theme.Light);
        MockThemeService.Setup(x => x.IsDarkMode).Returns(false);

        // Register the mock service in the bUnit service collection
        Services.AddSingleton(MockThemeService.Object);

        // Register a null logger to satisfy the ILogger<CraftComponent> dependency
        Services.AddSingleton<ILogger<CraftComponent>>(NullLogger<CraftComponent>.Instance);

        // Register MudBlazor services required for MudBlazor components
        Services.AddMudServices();

        // Configure JSInterop
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
