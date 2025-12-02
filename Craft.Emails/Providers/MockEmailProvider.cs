using Microsoft.Extensions.Logging;

namespace Craft.Emails;

/// <summary>
/// Mock email provider for testing and development.
/// Logs emails instead of sending them.
/// </summary>
public class MockEmailProvider : IEmailProvider
{
    private readonly ILogger<MockEmailProvider> _logger;

    public MockEmailProvider(ILogger<MockEmailProvider> logger)
    {
        _logger = logger;
    }

    public string Name => "mock";

    public bool IsConfigured() => true;

    public Task<EmailResult> SendAsync(MailRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "MOCK EMAIL: To={To}, Subject={Subject}, HasBody={HasBody}",
            string.Join(", ", request.To),
            request.Subject,
            !string.IsNullOrWhiteSpace(request.Body));

        if (!string.IsNullOrWhiteSpace(request.Body))
            _logger.LogDebug("Email Body Preview: {BodyPreview}",
                request.Body.Length > 200 ? request.Body[..200] + "..." : request.Body);

        return Task.FromResult(EmailResult.Success($"mock-{Guid.NewGuid():N}"));
    }
}
