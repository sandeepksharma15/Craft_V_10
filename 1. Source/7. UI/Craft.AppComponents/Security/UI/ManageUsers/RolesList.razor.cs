using System.ComponentModel.DataAnnotations;
using Craft.QuerySpec;
using Craft.Security;
using Microsoft.AspNetCore.Components;

namespace Craft.AppComponents.Security;

public partial class RolesList<TRole>
    where TRole : class, ICraftRole, new()
{
    /// <summary>The HTTP service used to load and delete roles.</summary>
    [Parameter, EditorRequired] public IHttpService<TRole>? HttpService { get; set; }

    /// <summary>Raised when the user clicks the Add button. The parent page handles navigation to the add route.</summary>
    [Parameter] public EventCallback OnAddRequested { get; set; }

    /// <summary>Raised when the user clicks Edit on a role. The parent page handles navigation to the edit route.</summary>
    [Parameter] public EventCallback<TRole> OnEditRequested { get; set; }

    /// <summary>Raised when the user clicks View on a role. The parent page handles navigation to the view route.</summary>
    [Parameter] public EventCallback<TRole> OnViewRequested { get; set; }

    /// <summary>Raised when the user clicks Permissions on a role. The parent page handles navigation to the permissions route.</summary>
    [Parameter] public EventCallback<TRole> OnPermissionsRequested { get; set; }

    private async Task HandleAddAsync()
    {
        if (OnAddRequested.HasDelegate)
            await OnAddRequested.InvokeAsync();
    }

    private async Task HandleEditAsync(TRole role)
    {
        if (OnEditRequested.HasDelegate)
            await OnEditRequested.InvokeAsync(role);
    }

    private async Task HandleViewAsync(TRole role)
    {
        if (OnViewRequested.HasDelegate)
            await OnViewRequested.InvokeAsync(role);
    }

    private async Task HandlePermissionsAsync(TRole role)
    {
        if (OnPermissionsRequested.HasDelegate)
            await OnPermissionsRequested.InvokeAsync(role);
    }

    private async Task HandleDeleteAsync(TRole role)
    {
        if (HttpService != null && (role.Id != default))
            await HttpService.DeleteAsync(role.Id);
    }
}
