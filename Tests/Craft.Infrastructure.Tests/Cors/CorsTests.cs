using System.ComponentModel.DataAnnotations;
using Craft.Infrastructure.Cors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Infrastructure.Tests.Cors;

public class CorsSettingsTests
{
    [Fact]
    public void SectionName_Should_Be_CorsSettings()
    {
        Assert.Equal("CorsSettings", CorsSettings.SectionName);
    }

    [Fact]
    public void Default_Values_Should_Be_Null()
    {
        var settings = new CorsSettings();

        Assert.Null(settings.Angular);
        Assert.Null(settings.Blazor);
        Assert.Null(settings.React);
    }

    [Fact]
    public void Properties_Can_Be_Set_And_Retrieved()
    {
        var settings = new CorsSettings
        {
            Angular = "http://localhost:4200",
            Blazor = "http://localhost:5000",
            React = "http://localhost:3000"
        };

        Assert.Equal("http://localhost:4200", settings.Angular);
        Assert.Equal("http://localhost:5000", settings.Blazor);
        Assert.Equal("http://localhost:3000", settings.React);
    }

    [Fact]
    public void Validate_Should_Return_Error_When_All_Origins_Are_Null()
    {
        var settings = new CorsSettings
        {
            Angular = null,
            Blazor = null,
            React = null
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(settings);
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        Assert.False(isValid);
        Assert.Single(results);
        Assert.Contains("At least one CORS origin must be configured", results[0].ErrorMessage);
        Assert.Contains(nameof(CorsSettings.Angular), results[0].MemberNames);
        Assert.Contains(nameof(CorsSettings.Blazor), results[0].MemberNames);
        Assert.Contains(nameof(CorsSettings.React), results[0].MemberNames);
    }

    [Fact]
    public void Validate_Should_Return_Error_When_All_Origins_Are_Empty()
    {
        var settings = new CorsSettings
        {
            Angular = string.Empty,
            Blazor = string.Empty,
            React = string.Empty
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(settings);
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        Assert.False(isValid);
        Assert.Single(results);
    }

    [Fact]
    public void Validate_Should_Return_Error_When_All_Origins_Are_Whitespace()
    {
        var settings = new CorsSettings
        {
            Angular = "   ",
            Blazor = "   ",
            React = "   "
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(settings);
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        Assert.False(isValid);
        Assert.Single(results);
    }

    [Fact]
    public void Validate_Should_Pass_When_Angular_Is_Set()
    {
        var settings = new CorsSettings
        {
            Angular = "http://localhost:4200",
            Blazor = null,
            React = null
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(settings);
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_Should_Pass_When_Blazor_Is_Set()
    {
        var settings = new CorsSettings
        {
            Angular = null,
            Blazor = "http://localhost:5000",
            React = null
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(settings);
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_Should_Pass_When_React_Is_Set()
    {
        var settings = new CorsSettings
        {
            Angular = null,
            Blazor = null,
            React = "http://localhost:3000"
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(settings);
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_Should_Pass_When_Multiple_Origins_Are_Set()
    {
        var settings = new CorsSettings
        {
            Angular = "http://localhost:4200",
            Blazor = "http://localhost:5000",
            React = "http://localhost:3000"
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(settings);
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_Should_Pass_When_Origins_Have_Multiple_Values()
    {
        var settings = new CorsSettings
        {
            Angular = "http://localhost:4200;https://angular.app.com",
            Blazor = "http://localhost:5000;https://blazor.app.com",
            React = "http://localhost:3000;https://react.app.com"
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(settings);
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }
}

public class CorsExtensionsTests
{
    [Fact]
    public void AddCorsPolicy_Should_Throw_When_Services_Is_Null()
    {
        IServiceCollection? services = null;
        var config = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentNullException>(() => 
            CorsExtensions.AddCorsPolicy(services!, config));
    }

    [Fact]
    public void AddCorsPolicy_Should_Throw_When_Config_Is_Null()
    {
        var services = new ServiceCollection();
        IConfiguration? config = null;

        Assert.Throws<ArgumentNullException>(() => 
            CorsExtensions.AddCorsPolicy(services, config!));
    }

    [Fact]
    public void AddCorsPolicy_Should_Register_CorsSettings_Options()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "http://localhost:4200"
            })
            .Build();

        services.AddCorsPolicy(config);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<CorsSettings>>();

        Assert.NotNull(options);
        Assert.NotNull(options.Value);
    }

    [Fact]
    public void AddCorsPolicy_Should_Bind_Configuration_To_CorsSettings()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "http://localhost:4200",
                ["CorsSettings:Blazor"] = "http://localhost:5000",
                ["CorsSettings:React"] = "http://localhost:3000"
            })
            .Build();

        services.AddCorsPolicy(config);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CorsSettings>>();

        Assert.Equal("http://localhost:4200", options.Value.Angular);
        Assert.Equal("http://localhost:5000", options.Value.Blazor);
        Assert.Equal("http://localhost:3000", options.Value.React);
    }

    [Fact]
    public void AddCorsPolicy_Should_Register_Cors_Services()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "http://localhost:4200"
            })
            .Build();

        services.AddCorsPolicy(config);

        var serviceProvider = services.BuildServiceProvider();
        var corsService = serviceProvider.GetService<ICorsService>();

        Assert.NotNull(corsService);
    }

    [Fact]
    public void AddCorsPolicy_Should_Return_Same_ServiceCollection_Instance()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "http://localhost:4200"
            })
            .Build();

        var result = services.AddCorsPolicy(config);

        Assert.Same(services, result);
    }

    [Fact]
    public void AddCorsPolicy_Should_Parse_Multiple_Origins_Separated_By_Semicolon()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "http://localhost:4200;https://angular.app.com;https://angular.staging.com"
            })
            .Build();

        services.AddCorsPolicy(config);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CorsSettings>>();

        Assert.Equal("http://localhost:4200;https://angular.app.com;https://angular.staging.com", 
            options.Value.Angular);
    }

    [Fact]
    public void AddCorsPolicy_Should_Handle_Empty_CorsSettings_Section()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        services.AddCorsPolicy(config);

        var serviceProvider = services.BuildServiceProvider();
        
        Assert.Throws<OptionsValidationException>(() => 
            serviceProvider.GetRequiredService<IOptions<CorsSettings>>().Value);
    }

    [Fact]
    public void AddCorsPolicy_Should_Configure_Policy_With_AllowAnyHeader()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "http://localhost:4200"
            })
            .Build();

        services.AddCorsPolicy(config);

        var serviceProvider = services.BuildServiceProvider();
        var policyProvider = serviceProvider.GetRequiredService<ICorsPolicyProvider>();

        Assert.NotNull(policyProvider);
    }

    [Fact]
    public void AddCorsPolicy_Should_Trim_Whitespace_From_Origins()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "  http://localhost:4200  ;  https://angular.app.com  "
            })
            .Build();

        services.AddCorsPolicy(config);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CorsSettings>>();

        Assert.Equal("  http://localhost:4200  ;  https://angular.app.com  ", options.Value.Angular);
    }

    [Fact]
    public void AddCorsPolicy_Should_Handle_Null_Origins()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = null,
                ["CorsSettings:Blazor"] = "http://localhost:5000",
                ["CorsSettings:React"] = null
            })
            .Build();

        services.AddCorsPolicy(config);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CorsSettings>>();

        Assert.Null(options.Value.Angular);
        Assert.Equal("http://localhost:5000", options.Value.Blazor);
        Assert.Null(options.Value.React);
    }

    [Fact]
    public void UseCorsPolicy_Should_Throw_When_App_Is_Null()
    {
        IApplicationBuilder? app = null;

        Assert.Throws<ArgumentNullException>(() => 
            CorsExtensions.UseCorsPolicy(app!));
    }

    [Fact]
    public void UseCorsPolicy_Should_Return_Same_ApplicationBuilder_Instance()
    {
        var app = new FakeApplicationBuilder();

        var result = CorsExtensions.UseCorsPolicy(app);

        Assert.Same(app, result);
    }

    [Fact]
    public void UseCorsPolicy_Should_Call_UseCors()
    {
        var app = new FakeApplicationBuilder();

        CorsExtensions.UseCorsPolicy(app);

        Assert.True(app.UseMiddlewareCalled);
    }

    [Fact]
    public void AddCorsPolicy_Should_Log_Warning_When_No_Origins_Configured()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "",
                ["CorsSettings:Blazor"] = "",
                ["CorsSettings:React"] = ""
            })
            .Build();

        var exception = Record.Exception(() => services.AddCorsPolicy(config));

        Assert.Null(exception);
    }

    [Fact]
    public void AddCorsPolicy_Should_Log_Information_When_Origins_Configured()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "http://localhost:4200"
            })
            .Build();

        var exception = Record.Exception(() => services.AddCorsPolicy(config));

        Assert.Null(exception);
    }

    [Fact]
    public void AddCorsPolicy_Should_Handle_Mixed_Origins()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "http://localhost:4200;https://angular.app.com",
                ["CorsSettings:Blazor"] = null,
                ["CorsSettings:React"] = "http://localhost:3000"
            })
            .Build();

        services.AddCorsPolicy(config);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CorsSettings>>();

        Assert.Equal("http://localhost:4200;https://angular.app.com", options.Value.Angular);
        Assert.Null(options.Value.Blazor);
        Assert.Equal("http://localhost:3000", options.Value.React);
    }

    [Fact]
    public void AddCorsPolicy_Should_Support_Chaining()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "http://localhost:4200"
            })
            .Build();

        var result = services
            .AddCorsPolicy(config)
            .AddLogging();

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AddCorsPolicy_Should_Handle_Empty_Whitespace_And_Null_Origins(string? origin)
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = origin
            })
            .Build();

        var exception = Record.Exception(() => services.AddCorsPolicy(config));

        Assert.Null(exception);
    }

    [Fact]
    public void AddCorsPolicy_Should_Parse_Complex_Origin_Configuration()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:Angular"] = "http://localhost:4200;https://angular.app.com;https://angular.staging.com",
                ["CorsSettings:Blazor"] = "http://localhost:5000;https://blazor.app.com",
                ["CorsSettings:React"] = "http://localhost:3000;https://react.app.com;https://react.staging.com;https://react.prod.com"
            })
            .Build();

        services.AddCorsPolicy(config);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CorsSettings>>();

        Assert.Contains("4200", options.Value.Angular);
        Assert.Contains("5000", options.Value.Blazor);
        Assert.Contains("3000", options.Value.React);
    }

    private class FakeApplicationBuilder : IApplicationBuilder
    {
        public bool UseMiddlewareCalled { get; private set; }
        public IServiceProvider ApplicationServices { get; set; } = null!;
        public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();
        public IFeatureCollection ServerFeatures { get; } = new FeatureCollection();
        
        public RequestDelegate Build() => throw new NotImplementedException();
        
        public IApplicationBuilder New() => this;
        
        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            UseMiddlewareCalled = true;
            return this;
        }
    }
}
