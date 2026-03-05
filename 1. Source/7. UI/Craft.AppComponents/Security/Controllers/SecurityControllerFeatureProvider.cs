using System.Reflection;
using Craft.Domain;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Craft.AppComponents.Security;

/// <summary>
/// Dynamically registers the closed-generic <see cref="UserController{TUser}"/> and
/// <see cref="RoleController{TRole}"/> for the supplied concrete entity types, so that
/// the host application does not need to define its own security controllers.
/// </summary>
internal sealed class SecurityControllerFeatureProvider<TUser, TRole> : IApplicationFeatureProvider<ControllerFeature>
    where TUser : class, IEntity<KeyType>, IModel<KeyType>, new()
    where TRole : class, IEntity<KeyType>, IModel<KeyType>, new()
{
    /// <inheritdoc />
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        feature.Controllers.Add(typeof(RoleController<TRole>).GetTypeInfo());
        feature.Controllers.Add(typeof(UserController<TUser>).GetTypeInfo());
    }
}
