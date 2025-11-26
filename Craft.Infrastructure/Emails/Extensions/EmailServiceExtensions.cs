using Craft.Infrastructure.Emails;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Extension methods for configuring email services.
/// </summary>
public static class EmailServiceExtensions
{
    /// <summary>
    /// Adds email services to the service collection with configuration from appsettings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing EmailOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddEmailServices(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddEmailServices(configuration.GetSection(EmailOptions.SectionName));
    }

    /// <summary>
    /// Adds email services to the service collection with a configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSection">The configuration section containing EmailOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        services.AddOptions<EmailOptions>()
            .Bind(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddLogging();
        services.AddMemoryCache();

        services.TryAddSingleton<IEmailQueue, InMemoryEmailQueue>();
        services.TryAddSingleton<IEmailTemplateRenderer, RazorEmailTemplateRenderer>();

        services.TryAddTransient<IEmailProvider, SmtpEmailProvider>();
        services.TryAddTransient<IEmailProvider, MockEmailProvider>();
        services.TryAddSingleton<IEmailProviderFactory, EmailProviderFactory>();

        services.TryAddScoped<IMailService, MailService>();

        var options = configurationSection.Get<EmailOptions>();
        if (options?.EnableQueue == true)
            services.AddHostedService<EmailQueueProcessor>();

        return services;
    }

    /// <summary>
    /// Adds email services with a configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure email options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddEmailServices(options =>
    /// {
    ///     options.Provider = "smtp";
    ///     options.From = "noreply@example.com";
    ///     options.Smtp = new SmtpSettings
    ///     {
    ///         Host = "smtp.example.com",
    ///         Port = 587
    ///     };
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        Action<EmailOptions> configureOptions)
    {
        services.AddOptions<EmailOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddLogging();
        services.AddMemoryCache();

        services.TryAddSingleton<IEmailQueue, InMemoryEmailQueue>();
        services.TryAddSingleton<IEmailTemplateRenderer, RazorEmailTemplateRenderer>();

        services.TryAddTransient<IEmailProvider, SmtpEmailProvider>();
        services.TryAddTransient<IEmailProvider, MockEmailProvider>();
        services.TryAddSingleton<IEmailProviderFactory, EmailProviderFactory>();

        services.TryAddScoped<IMailService, MailService>();

        return services;
    }

    /// <summary>
    /// Registers a custom email provider.
    /// </summary>
    /// <typeparam name="TProvider">The type of the email provider.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services
    ///     .AddEmailServices(builder.Configuration)
    ///     .AddEmailProvider&lt;SendGridEmailProvider&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddEmailProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IEmailProvider
    {
        services.AddTransient<IEmailProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Registers a custom email queue implementation.
    /// </summary>
    /// <typeparam name="TQueue">The type of the email queue.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services
    ///     .AddEmailServices(builder.Configuration)
    ///     .AddEmailQueue&lt;DatabaseEmailQueue&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddEmailQueue<TQueue>(this IServiceCollection services)
        where TQueue : class, IEmailQueue
    {
        services.Replace(ServiceDescriptor.Singleton<IEmailQueue, TQueue>());
        return services;
    }

    /// <summary>
    /// Registers a custom email template renderer.
    /// </summary>
    /// <typeparam name="TRenderer">The type of the template renderer.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEmailTemplateRenderer<TRenderer>(this IServiceCollection services)
        where TRenderer : class, IEmailTemplateRenderer
    {
        services.Replace(ServiceDescriptor.Singleton<IEmailTemplateRenderer, TRenderer>());
        return services;
    }
}
