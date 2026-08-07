using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Craft.Emails;

/// <summary>
/// Registers the library-provided <see cref="EmailController"/> so the host application
/// does not need to define its own email controller class.
/// </summary>
internal sealed class EmailControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    /// <inheritdoc />
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        if (feature.Controllers.Any(controller => controller.AsType() == typeof(EmailController)))
            return;

        feature.Controllers.Add(typeof(EmailController).GetTypeInfo());
    }
}
