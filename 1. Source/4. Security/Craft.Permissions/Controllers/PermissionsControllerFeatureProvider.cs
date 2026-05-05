using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Craft.Permissions;

/// <summary>
/// Dynamically registers the closed-generic <see cref="PermissionsController{TUser}"/> for the
/// supplied concrete user type, so that the host application does not need to define its own
/// permissions controller class.
/// </summary>
/// <typeparam name="TUser">The application user type (must derive from <see cref="IdentityUser{TKey}"/>).</typeparam>
internal sealed class PermissionsControllerFeatureProvider<TUser> : IApplicationFeatureProvider<ControllerFeature>
    where TUser : IdentityUser<KeyType>
{
    /// <inheritdoc />
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        => feature.Controllers.Add(typeof(PermissionsController<TUser>).GetTypeInfo());
}
