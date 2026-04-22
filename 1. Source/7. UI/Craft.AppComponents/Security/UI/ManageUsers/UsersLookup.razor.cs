using Craft.QuerySpec;
using Craft.Security;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.AppComponents.Security;

public partial class UsersLookup<TUser>
    where TUser : class, ICraftUser, new()
{
    private KeyType? _selectedUserId;

    /// <summary>The HTTP service used to load users for the dropdown.</summary>
    [Parameter, EditorRequired] public IHttpService<TUser>? HttpService { get; set; }

    /// <summary>The currently selected user ID.</summary>
    [Parameter]
    public KeyType? SelectedUserId
    {
        get => _selectedUserId;
        set
        {
            if (_selectedUserId != value)
            {
                _selectedUserId = value;
                SelectedUserIdChanged.InvokeAsync(value);
            }
        }
    }

    /// <summary>Event callback invoked when the selected user changes.</summary>
    [Parameter] public EventCallback<KeyType?> SelectedUserIdChanged { get; set; }

    /// <summary>Label for the dropdown.</summary>
    [Parameter] public string Label { get; set; } = "User";

    /// <summary>Placeholder text when no user is selected.</summary>
    [Parameter] public string Placeholder { get; set; } = "Select a user";

    /// <summary>Variant for the MudSelect.</summary>
    [Parameter] public Variant Variant { get; set; } = Variant.Outlined;

    /// <summary>Whether the field is required.</summary>
    [Parameter] public bool Required { get; set; } = false;

    /// <summary>Whether the dropdown is read-only.</summary>
    [Parameter] public bool ReadOnly { get; set; } = false;

    /// <summary>Whether the dropdown is disabled.</summary>
    [Parameter] public bool Disabled { get; set; } = false;

    /// <summary>Additional CSS classes.</summary>
    [Parameter] public string? Class { get; set; }

    private IReadOnlyList<TUser>? _users;

    protected override async Task OnInitializedAsync()
    {
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            var result = await HttpService!.GetAllAsync();

            _users = result.IsSuccess && result.Value != null
                ? result.Value
                : [];
        }
        catch (Exception)
        {
            _users = [];
        }
    }

    /// <summary>
    /// Builds display text from FirstName + LastName, falling back to UserName, then the ID.
    /// </summary>
    private static string GetUserDisplayText(TUser user)
    {
        var fullName = string.Join(" ", new[] { user.FirstName, user.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        return !string.IsNullOrWhiteSpace(fullName)
            ? fullName
            : user.UserName ?? string.Empty;
    }

    private string? GetUserDisplayText(KeyType? userId)
    {
        if (userId == null || _users == null)
            return null;

        var user = _users.FirstOrDefault(u => u.Id == userId);

        return user is not null
            ? GetUserDisplayText(user)
            : userId.ToString();
    }
}
