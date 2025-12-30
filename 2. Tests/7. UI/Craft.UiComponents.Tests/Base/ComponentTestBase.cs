using Bunit;
using Craft.UiComponents;
using Craft.UiComponents.Enums;
using Craft.UiComponents.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;

namespace Craft.UiComponents.Tests.Base;

/// <summary>
/// Base class for Craft UI component tests that provides common test infrastructure
/// including mock services registration.
/// </summary>
public abstract class ComponentTestBase : BunitContext
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
        
        // Configure JSInterop
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
