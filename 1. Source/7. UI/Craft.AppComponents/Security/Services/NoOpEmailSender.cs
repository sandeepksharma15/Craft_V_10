using Microsoft.AspNetCore.Identity;

namespace Craft.AppComponents.Security;

/// <summary>
/// Default no-op email sender registered when the host application does not supply its own
/// <see cref="IEmailSender{TUser}"/> implementation.
/// Replace it by calling <c>AddAuthApi&lt;TUser&gt;().WithEmailSender&lt;TEmailSender&gt;()</c>
/// so auth flows can send real emails without redefining the controller or repository.
/// </summary>
/// <typeparam name="TUser">The application user type.</typeparam>
internal sealed class NoOpEmailSender<TUser> : IAuthEmailSender<TUser>
    where TUser : class
{
    /// <inheritdoc />
    public bool IsEnabled => false;

    /// <inheritdoc />
    public Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task SendWelcomeEmailAsync(TUser user, string email)
        => Task.CompletedTask;
}
