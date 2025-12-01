using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Craft.Security.Tokens;

public static class JwtExtensions
{
    /// <summary>
    /// Configures JWT authentication with the specified configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureJwt(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddOptions<JwtSettings>()
            .BindConfiguration($"SecuritySettings:{nameof(JwtSettings)}")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtSettings = config.GetSection($"SecuritySettings:{nameof(JwtSettings)}").Get<JwtSettings>();

        if (jwtSettings is null)
            throw new InvalidOperationException("JwtSettings configuration is missing or invalid");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                byte[] key = Encoding.ASCII.GetBytes(jwtSettings.IssuerSigningKey);

                options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetaData;
                options.SaveToken = jwtSettings.SaveToken;
                options.IncludeErrorDetails = jwtSettings.IncludeErrorDetails;
                options.Validate(JwtBearerDefaults.AuthenticationScheme);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidAudiences = jwtSettings.ValidAudiences,
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidateLifetime = jwtSettings.ValidateLifetime,
                    RequireExpirationTime = jwtSettings.RequireExpirationTime,
                    RequireSignedTokens = jwtSettings.RequireSignedTokens,
                    ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkew)
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var tokenManager = context.HttpContext.RequestServices.GetRequiredService<ITokenManager>();
                        var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

                        if (await tokenManager.IsTokenRevokedAsync(token))
                        {
                            context.Fail("Token has been revoked");
                        }
                    }
                };
            });

        return services;
    }

    /// <summary>
    /// Adds token management services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTokenManagement(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        
        services.AddSingleton<ITokenBlacklist, InMemoryTokenBlacklist>();
        
        services.AddHostedService<TokenBlacklistCleanupService>();

        return services;
    }
}
