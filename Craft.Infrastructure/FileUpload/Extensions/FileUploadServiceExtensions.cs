using Craft.Infrastructure.FileUpload;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Extension methods for configuring file upload services.
/// </summary>
public static class FileUploadServiceExtensions
{
    /// <summary>
    /// Adds file upload services to the service collection with configuration from appsettings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing FileUploadOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddFileUploadServices(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddFileUploadServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddFileUploadServices(configuration.GetSection(FileUploadOptions.SectionName));
    }

    /// <summary>
    /// Adds file upload services to the service collection with a configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSection">The configuration section containing FileUploadOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFileUploadServices(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        services.AddOptions<FileUploadOptions>()
            .Bind(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddLogging();

        services.TryAddScoped<IFileStorageProvider, LocalFileStorageProvider>();
        services.TryAddScoped<IFileUploadService, FileUploadService>();

        var options = configurationSection.Get<FileUploadOptions>();
        if (options?.UseTimeLimitedTokens == true)
            services.TryAddSingleton<IFileAccessTokenService, FileAccessTokenService>();

        return services;
    }

    /// <summary>
    /// Adds file upload services with a configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure file upload options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddFileUploadServices(options =>
    /// {
    ///     options.Provider = "local";
    ///     options.BasePath = "Uploads";
    ///     options.DefaultMaxSizeMB = 20;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddFileUploadServices(
        this IServiceCollection services,
        Action<FileUploadOptions> configureOptions)
    {
        services.AddOptions<FileUploadOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddLogging();

        services.TryAddScoped<IFileStorageProvider, LocalFileStorageProvider>();
        services.TryAddScoped<IFileUploadService, FileUploadService>();

        return services;
    }

    /// <summary>
    /// Registers a custom file storage provider.
    /// </summary>
    /// <typeparam name="TProvider">The type of the storage provider.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services
    ///     .AddFileUploadServices(builder.Configuration)
    ///     .AddFileStorageProvider&lt;AzureBlobStorageProvider&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddFileStorageProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IFileStorageProvider
    {
        services.Replace(ServiceDescriptor.Scoped<IFileStorageProvider, TProvider>());
        return services;
    }

    /// <summary>
    /// Registers a virus scanner implementation.
    /// </summary>
    /// <typeparam name="TScanner">The type of the virus scanner.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services
    ///     .AddFileUploadServices(builder.Configuration)
    ///     .AddVirusScanner&lt;ClamAVVirusScanner&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddVirusScanner<TScanner>(this IServiceCollection services)
        where TScanner : class, IVirusScanner
    {
        services.TryAddScoped<IVirusScanner, TScanner>();
        return services;
    }

    /// <summary>
    /// Registers a thumbnail generator implementation.
    /// </summary>
    /// <typeparam name="TGenerator">The type of the thumbnail generator.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services
    ///     .AddFileUploadServices(builder.Configuration)
    ///     .AddThumbnailGenerator&lt;ImageSharpThumbnailGenerator&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddThumbnailGenerator<TGenerator>(this IServiceCollection services)
        where TGenerator : class, IThumbnailGenerator
    {
        services.TryAddScoped<IThumbnailGenerator, TGenerator>();
        return services;
    }

    /// <summary>
    /// Registers a custom file access token service.
    /// </summary>
    /// <typeparam name="TTokenService">The type of the token service.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFileAccessTokenService<TTokenService>(this IServiceCollection services)
        where TTokenService : class, IFileAccessTokenService
    {
        services.Replace(ServiceDescriptor.Singleton<IFileAccessTokenService, TTokenService>());
        return services;
    }
}
