namespace Craft.Emails;

/// <summary>
/// Defines the contract for email template rendering.
/// </summary>
public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Renders an email template with the provided model.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="templateName">The name of the template.</param>
    /// <param name="model">The model data for the template.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The rendered HTML content.</returns>
    Task<string> RenderAsync<T>(string templateName, T model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders an email template without a model.
    /// </summary>
    /// <param name="templateName">The name of the template.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The rendered HTML content.</returns>
    Task<string> RenderAsync(string templateName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a template exists.
    /// </summary>
    /// <param name="templateName">The name of the template.</param>
    /// <returns>True if the template exists, false otherwise.</returns>
    bool TemplateExists(string templateName);

    /// <summary>
    /// Clears the template cache (if caching is enabled).
    /// </summary>
    void ClearCache();
}
