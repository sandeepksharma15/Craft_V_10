using Microsoft.AspNetCore.Components;

namespace Craft.Permissions;

/// <summary>
/// Renders <see cref="Authorized"/> when the current user satisfies <b>either</b>:
/// <list type="bullet">
///   <item>They are in at least one of the comma-separated <see cref="Roles"/>, OR</item>
///   <item>They hold the specified <see cref="Permission"/> code.</item>
/// </list>
/// Both <see cref="Roles"/> and <see cref="Permission"/> are optional; if neither is provided
/// the content is always rendered.
/// </summary>
public partial class RoleOrPermissionView : ComponentBase
{
    /// <summary>
    /// Comma-separated role names to check (e.g. <c>"Admin,Manager"</c>).
    /// Leave <see langword="null"/> or empty to skip the role check.
    /// </summary>
    [Parameter] public string? Roles { get; set; }

    /// <summary>
    /// The integer permission code to check.
    /// Leave <see langword="null"/> to skip the permission check.
    /// </summary>
    [Parameter] public int? Permission { get; set; }

    /// <summary>Content rendered when the authorization check passes.</summary>
    [Parameter, EditorRequired] public RenderFragment Authorized { get; set; } = null!;

    /// <summary>Optional content rendered when the check fails.</summary>
    [Parameter] public RenderFragment? NotAuthorized { get; set; }
}
