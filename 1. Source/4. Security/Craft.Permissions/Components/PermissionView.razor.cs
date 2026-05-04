using Microsoft.AspNetCore.Components;

namespace Craft.Permissions;

/// <summary>
/// Renders <see cref="Authorized"/> when the current user holds the specified
/// <see cref="Permission"/> code. Optionally renders <see cref="NotAuthorized"/>
/// when the user lacks the permission.
/// </summary>
public partial class PermissionView : ComponentBase
{
    /// <summary>The integer permission code to check.</summary>
    [Parameter, EditorRequired] public int Permission { get; set; }

    /// <summary>Content rendered when the user has the permission.</summary>
    [Parameter, EditorRequired] public RenderFragment Authorized { get; set; } = null!;

    /// <summary>Optional content rendered when the user lacks the permission.</summary>
    [Parameter] public RenderFragment? NotAuthorized { get; set; }
}
