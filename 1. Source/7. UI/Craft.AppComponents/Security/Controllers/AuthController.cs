using Craft.Core;
using Craft.Security;
using Craft.Security.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Security;

/// <summary>
/// Closed-generic concrete auth controller registered automatically via
/// <see cref="AuthControllerFeatureProvider{TUser}"/> when the host application calls
/// <c>AddAuthApi&lt;TUser&gt;()</c>.
/// </summary>
/// <typeparam name="TUser">The application user entity type.</typeparam>
/// <remarks>
/// This class contains no logic — all endpoint implementations live in
/// <see cref="AuthControllerBase{TUser,TKey}"/>. Override individual endpoints
/// by deriving directly from <see cref="AuthControllerBase{TUser,TKey}"/> in the
/// host application and <b>not</b> calling <c>AddAuthApi&lt;TUser&gt;()</c> (which
/// would produce a duplicate <c>api/auth</c> route and cause an MVC startup error).
/// </remarks>
[ApiController]
public class AuthController<TUser>(
    UserManager<TUser> userManager,
    ITokenManager tokenManager,
    ITokenBlacklist tokenBlacklist,
    IDbContext dbContext,
    ILogger<AuthControllerBase<TUser, KeyType>> logger)
    : AuthControllerBase<TUser, KeyType>(userManager, tokenManager, tokenBlacklist, dbContext, logger)
    where TUser : CraftUser<KeyType>;
