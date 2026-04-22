using Craft.QuerySpec;
using Craft.Security;
using Microsoft.AspNetCore.Components;

namespace Craft.AppComponents.Security;

public partial class UsersList<TUser>
    where TUser : class, ICraftUser, new()
{
    /// <summary>The HTTP service used to load and delete users.</summary>
    [Parameter, EditorRequired] public IHttpService<TUser>? HttpService { get; set; }

    /// <summary>Raised when the user clicks the Add button. The parent page handles navigation to the add route.</summary>
    [Parameter] public EventCallback OnAddRequested { get; set; }

    /// <summary>Raised when the user clicks Edit on a user. The parent page handles navigation to the edit route.</summary>
    [Parameter] public EventCallback<TUser> OnEditRequested { get; set; }

    /// <summary>Raised when the user clicks View on a user. The parent page handles navigation to the view route.</summary>
    [Parameter] public EventCallback<TUser> OnViewRequested { get; set; }

    private async Task HandleAddAsync()
    {
        if (OnAddRequested.HasDelegate)
            await OnAddRequested.InvokeAsync();
    }

    private async Task HandleEditAsync(TUser user)
    {
        if (OnEditRequested.HasDelegate)
            await OnEditRequested.InvokeAsync(user);
    }

    private async Task HandleViewAsync(TUser user)
    {
        if (OnViewRequested.HasDelegate)
            await OnViewRequested.InvokeAsync(user);
    }

    private async Task HandleDeleteAsync(TUser user)
    {
        if (HttpService != null && (user.Id != default))
            await HttpService.DeleteAsync(user.Id);
    }
}
