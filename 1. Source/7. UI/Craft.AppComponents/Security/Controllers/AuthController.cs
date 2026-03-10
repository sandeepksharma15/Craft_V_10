using Craft.Security;
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
/// <see cref="AuthControllerBase"/>. Override individual endpoints by deriving
/// directly from <see cref="AuthControllerBase"/> in the host application and
/// <b>not</b> calling <c>AddAuthApi&lt;TUser&gt;()</c> (which would produce a
/// duplicate <c>api/auth</c> route and cause an MVC startup error).
/// </remarks>
[ApiController]
public class AuthController<TUser>(IAuthRepository authRepository, ILogger<AuthControllerBase> logger)
    : AuthControllerBase(authRepository, logger)
    where TUser : CraftUser<KeyType>;
