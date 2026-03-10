using Microsoft.AspNetCore.Identity;

namespace Craft.AppComponents.Security;

/// <summary>
/// Default no-op email sender registered when the host application does not supply its own
/// <see cref="IEmailSender{TUser}"/> implementation.
/// Replace it by registering a concrete <see cref="IEmailSender{TUser}"/> <em>before</em>
/// calling <c>AddAuthApi&lt;TUser&gt;()</c> — the <c>TryAdd</c> registration used here
/// means an existing registration always wins.
/// </summary>
/// <typeparam name="TUser">The application user type.</typeparam>
internal sealed class NoOpEmailSender<TUser> : IEmailSender<TUser>
    where TUser : class
{
    /// <inheritdoc />
    public Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
        => Task.CompletedTask;
}
