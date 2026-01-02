using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Craft.Components.Navigations;

// Please Set this in your MainLayout.razor.cs Or App.razor.cs file For "GoBack" This To Work
// [Inject] private IJSRuntime _jSRuntime { get; set; }
// protected override void OnAfterRender(bool firstRender)
// {
//     if (firstRender)
//     {
//         NavigationExtensions.SetJsRuntime(_jSRuntime);
//     }
// }

public static class NavigationExtensions
{
    private static IJSRuntime _jSRuntime;

#pragma warning disable RCS1175, IDE0060 // Unused 'this' parameter.

    public static async Task GoBack(this NavigationManager navigationManager)
#pragma warning restore RCS1175, IDE0060 // Unused 'this' parameter.
    {
        await _jSRuntime.InvokeVoidAsync("history.back");
    }

    public static void SetJsRuntime(IJSRuntime jSRuntime)
    {
        _jSRuntime = jSRuntime;
    }
}
