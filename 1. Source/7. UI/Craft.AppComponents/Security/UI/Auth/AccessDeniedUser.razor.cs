using Microsoft.AspNetCore.Components;

namespace Craft.AppComponents.Security;

/// <summary>
/// A self-contained access-denied card displayed when an authenticated user
/// lacks the required role or policy to view a page.
/// </summary>
public partial class AccessDeniedUser : ComponentBase
{
    /// <summary>Where the "Go Home" button navigates. Defaults to <c>"/"</c>.</summary>
    [Parameter] public string HomeHref { get; set; } = "/";

    /// <summary>
    /// Where the "Sign in with a different account" link navigates.
    /// Leave <see langword="null"/> to hide the link.
    /// </summary>
    [Parameter] public string? LoginHref { get; set; }

    /// <summary>Card header title. Defaults to <c>"Access Denied"</c>.</summary>
    [Parameter] public string Title { get; set; } = "Access Denied";
}
