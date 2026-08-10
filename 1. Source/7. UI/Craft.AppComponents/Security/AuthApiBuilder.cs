using Craft.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Craft.AppComponents.Security;

/// <summary>
/// Provides fluent customization for auth API registrations.
/// </summary>
/// <typeparam name="TUser">The application user entity type.</typeparam>
public sealed class AuthApiBuilder<TUser>
    where TUser : CraftUser<KeyType>, new()
{
    private readonly IServiceCollection _services;

    internal AuthApiBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _services = services;
    }

    /// <summary>
    /// Replaces the default no-op auth email sender with the specified implementation.
    /// </summary>
    /// <typeparam name="TEmailSender">The email sender implementation to use for auth flows.</typeparam>
    /// <returns>The builder for further auth-specific customization.</returns>
    public AuthApiBuilder<TUser> WithEmailSender<TEmailSender>()
        where TEmailSender : class, IEmailSender<TUser>
    {
        _services.RemoveAll<IEmailSender<TUser>>();
        _services.AddScoped<IEmailSender<TUser>, TEmailSender>();

        return this;
    }
}
