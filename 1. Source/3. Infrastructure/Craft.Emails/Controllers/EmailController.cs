using Microsoft.Extensions.Logging;

namespace Craft.Emails;

/// <summary>
/// Concrete email controller registered automatically when the host application calls
/// <c>AddEmailApi()</c>.
/// </summary>
/// <remarks>
/// This class contains no logic. Override individual endpoints by deriving directly from
/// <see cref="EmailControllerBase"/> in the host application and avoiding duplicate route registration.
/// </remarks>
public class EmailController(IMailService mailService, ILogger<EmailControllerBase> logger)
    : EmailControllerBase(mailService, logger);
