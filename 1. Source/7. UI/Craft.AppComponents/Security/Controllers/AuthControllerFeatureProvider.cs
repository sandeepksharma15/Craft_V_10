using System.Reflection;
using Craft.Security;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Craft.AppComponents.Security;

/// <summary>
/// Dynamically registers the closed-generic <see cref="AuthController{TUser}"/> for the
/// supplied concrete user type, so that the host application does not need to define its
/// own auth controller class.
/// </summary>
/// <typeparam name="TUser">The application user entity type.</typeparam>
internal sealed class AuthControllerFeatureProvider<TUser> : IApplicationFeatureProvider<ControllerFeature>
    where TUser : CraftUser<KeyType>
{
    /// <inheritdoc />
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        => feature.Controllers.Add(typeof(AuthController<TUser>).GetTypeInfo());
}
