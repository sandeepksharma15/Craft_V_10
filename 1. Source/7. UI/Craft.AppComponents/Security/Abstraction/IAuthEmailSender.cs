using Microsoft.AspNetCore.Identity;

namespace Craft.AppComponents.Security;

/// <summary>
/// Extends the ASP.NET Core Identity email sender contract with auth-specific capabilities.
/// </summary>
/// <typeparam name="TUser">The application user type.</typeparam>
public interface IAuthEmailSender<TUser> : IEmailSender<TUser>
    where TUser : class
{
    /// <summary>
    /// Gets a value indicating whether this sender is configured to dispatch real auth emails.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Sends a welcome email after successful registration.
    /// </summary>
    /// <param name="user">The newly registered user.</param>
    /// <param name="email">The recipient email address.</param>
    Task SendWelcomeEmailAsync(TUser user, string email);
}
