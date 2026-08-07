using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.Emails;

/// <summary>
/// Reusable, overridable API controller that exposes email operations to client applications.
/// </summary>
/// <remarks>
/// Derive from this class to customize individual actions while reusing the default library behavior.
/// </remarks>
[Route("api/email")]
[ApiController]
public abstract class EmailControllerBase : ControllerBase
{
    private readonly IMailService _mailService;
    private readonly ILogger<EmailControllerBase> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="EmailControllerBase"/>.
    /// </summary>
    /// <param name="mailService">The email service used to send messages.</param>
    /// <param name="logger">Logger for this controller.</param>
    protected EmailControllerBase(IMailService mailService, ILogger<EmailControllerBase> logger)
    {
        _mailService = mailService;
        _logger = logger;
    }

    /// <summary>
    /// Sends an email immediately.
    /// </summary>
    /// <param name="request">The email request containing recipients, subject, and message content.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> containing an <see cref="EmailResult"/> on success;
    /// <see cref="ObjectResult"/> with status 500 when the email provider reports a failure.
    /// </returns>
    [Authorize]
    [HttpPost("send")]
    [ProducesResponseType(typeof(EmailResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EmailResult), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public virtual async Task<IActionResult> SendAsync([FromBody] MailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _mailService.SendAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "[EmailController] Email send failed for {Recipients} with subject '{Subject}': {ErrorMessage}",
                string.Join(", ", request.To),
                request.Subject,
                result.ErrorMessage);

            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        _logger.LogInformation(
            "[EmailController] Email sent successfully to {Recipients} with subject '{Subject}'",
            string.Join(", ", request.To),
            request.Subject);

        return Ok(result);
    }
}
