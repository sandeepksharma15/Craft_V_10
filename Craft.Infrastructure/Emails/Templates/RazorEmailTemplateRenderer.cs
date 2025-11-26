using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RazorEngineCore;

namespace Craft.Infrastructure.Emails;

/// <summary>
/// Email template renderer using RazorEngineCore with caching support.
/// </summary>
public class RazorEmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly EmailOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RazorEmailTemplateRenderer> _logger;
    private readonly IRazorEngine _razorEngine;

    public RazorEmailTemplateRenderer(
        IOptions<EmailOptions> options,
        IMemoryCache cache,
        ILogger<RazorEmailTemplateRenderer> logger)
    {
        _options = options.Value;
        _cache = cache;
        _logger = logger;
        _razorEngine = new RazorEngine();
    }

    public async Task<string> RenderAsync<T>(string templateName, T model, CancellationToken cancellationToken = default)
    {
        var templateContent = await LoadTemplateAsync(templateName, cancellationToken);
        var compiledTemplate = GetOrCompileTemplate(templateName, templateContent);

        try
        {
            var result = await compiledTemplate.RunAsync(model);
            _logger.LogDebug("Successfully rendered template '{TemplateName}'", templateName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template '{TemplateName}'", templateName);
            throw;
        }
    }

    public Task<string> RenderAsync(string templateName, CancellationToken cancellationToken = default)
    {
        return RenderAsync(templateName, new { }, cancellationToken);
    }

    public bool TemplateExists(string templateName)
    {
        var filePath = GetTemplatePath(templateName);
        return File.Exists(filePath);
    }

    public void ClearCache()
    {
        if (_cache is MemoryCache memCache)
        {
            memCache.Compact(1.0);
            _logger.LogInformation("Template cache cleared");
        }
    }

    private async Task<string> LoadTemplateAsync(string templateName, CancellationToken cancellationToken)
    {
        var filePath = GetTemplatePath(templateName);

        if (!File.Exists(filePath))
        {
            _logger.LogError("Template file not found: {FilePath}", filePath);
            throw new FileNotFoundException($"Email template '{templateName}' not found at path: {filePath}");
        }

        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var sr = new StreamReader(fs, Encoding.UTF8);
            var content = await sr.ReadToEndAsync(cancellationToken);

            _logger.LogDebug("Loaded template '{TemplateName}' from {FilePath}", templateName, filePath);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load template '{TemplateName}' from {FilePath}", templateName, filePath);
            throw;
        }
    }

    private IRazorEngineCompiledTemplate GetOrCompileTemplate(string templateName, string templateContent)
    {
        if (!_options.EnableTemplateCache)
            return CompileTemplate(templateContent);

        var cacheKey = $"EmailTemplate_{templateName}";

        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.TemplateCacheDurationMinutes);
            _logger.LogDebug("Compiling and caching template '{TemplateName}'", templateName);
            return CompileTemplate(templateContent);
        })!;
    }

    private IRazorEngineCompiledTemplate CompileTemplate(string templateContent)
    {
        try
        {
            return _razorEngine.Compile(templateContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compile template");
            throw new InvalidOperationException("Failed to compile email template", ex);
        }
    }

    private string GetTemplatePath(string templateName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var templatesFolder = Path.Combine(baseDirectory, _options.TemplatesPath);
        return Path.Combine(templatesFolder, $"{templateName}.cshtml");
    }
}
