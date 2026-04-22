# ManageUsers Components

Reusable, route-agnostic Blazor components for displaying and managing users and roles.
All components are generic, constrained to the appropriate Craft security interface,
and designed to be dropped into any parent page — the parent owns all routing decisions.

---

## RolesList\<TRole\>

Displays a paginated, searchable grid of roles.

### Type Constraint

```
TRole : class, ICraftRole, new()
```

### Parameters

| Parameter           | Type                      | Required | Description                                                  |
|---------------------|---------------------------|----------|--------------------------------------------------------------|
| `HttpService`       | `IHttpService<TRole>`     | ✅ Yes   | Service used to load and delete roles.                       |
| `OnAddRequested`    | `EventCallback`           | No       | Raised when the user clicks **Add**. Parent handles routing. |
| `OnEditRequested`   | `EventCallback<TRole>`    | No       | Raised when the user clicks **Edit**. Passes the selected role. |
| `OnViewRequested`   | `EventCallback<TRole>`    | No       | Raised when the user clicks **View**. Passes the selected role. |

### Columns

| Column        | Visible | Sortable | Searchable |
|---------------|---------|----------|------------|
| Id            | ❌ No   | ✅ Yes   | No         |
| Name          | ✅ Yes  | ✅ Yes   | ✅ Yes     |
| Description   | ✅ Yes  | ✅ Yes   | No         |
| IsActive      | ✅ Yes  | ✅ Yes   | No         |

### Usage Example

```razor
@inject IHttpService<AppRole> RoleService
@inject NavigationManager Nav

<RolesList TRole="AppRole"
           HttpService="@RoleService"
           OnAddRequested="@(() => Nav.NavigateTo(AppRoutes.AddRole))"
           OnEditRequested="@(r => Nav.NavigateTo(AppRoutes.BuildEditRoleRoute(r.Id)))"
           OnViewRequested="@(r => Nav.NavigateTo(AppRoutes.BuildViewRoleRoute(r.Id)))" />
```

> If you do not provide a callback for `OnAddRequested`, `OnEditRequested`, or `OnViewRequested`,
> the corresponding button is still rendered but clicking it is a no-op. Wire up only the actions you need.

---

## UsersList\<TUser\>

Displays a paginated, searchable grid of users.

### Type Constraint

```
TUser : class, ICraftUser, new()
```

### Parameters

| Parameter           | Type                      | Required | Description                                                  |
|---------------------|---------------------------|----------|--------------------------------------------------------------|
| `HttpService`       | `IHttpService<TUser>`     | ✅ Yes   | Service used to load and delete users.                       |
| `OnAddRequested`    | `EventCallback`           | No       | Raised when the user clicks **Add**. Parent handles routing. |
| `OnEditRequested`   | `EventCallback<TUser>`    | No       | Raised when the user clicks **Edit**. Passes the selected user. |
| `OnViewRequested`   | `EventCallback<TUser>`    | No       | Raised when the user clicks **View**. Passes the selected user. |

### Columns

| Column      | Visible | Sortable | Searchable |
|-------------|---------|----------|------------|
| Id          | ❌ No   | ✅ Yes   | No         |
| UserName    | ✅ Yes  | ✅ Yes   | ✅ Yes     |
| Email       | ✅ Yes  | ✅ Yes   | ✅ Yes     |
| FirstName   | ✅ Yes  | ✅ Yes   | No         |
| LastName    | ✅ Yes  | ✅ Yes   | No         |
| IsActive    | ✅ Yes  | ✅ Yes   | No         |

### Usage Example

```razor
@inject IHttpService<AppUser> UserService
@inject NavigationManager Nav

<UsersList TUser="AppUser"
           HttpService="@UserService"
           OnAddRequested="@(() => Nav.NavigateTo(AppRoutes.AddUser))"
           OnEditRequested="@(u => Nav.NavigateTo(AppRoutes.BuildEditUserRoute(u.Id)))"
           OnViewRequested="@(u => Nav.NavigateTo(AppRoutes.BuildViewUserRoute(u.Id)))" />
```

---

## Notes

- **Delete** is handled internally by both components via `HttpService.DeleteAsync`. It does not require a callback — the underlying repository performs a soft-delete (`IsDeleted = true`).
- **No `@attribute [Route(...)]`** — both components are designed to be embedded inside a parent page that owns the route.
- **Navigation** is fully delegated to the parent via `EventCallback`. The components have zero knowledge of routes, keeping them reusable across different host pages.
- The `IHttpService<T>` must be registered in DI and passed in as a parameter from the parent page's code-behind.

---

## RolesEdit\<TRole, TRoleVM, TRoleDTO\>

A self-contained Add / Edit / View form for roles. Handles loading, saving, validation display, and spinner state internally. Navigation (save success and cancel) is delegated to the parent via `EventCallback`.

### Type Constraints

```
TRole    : class, ICraftRole, new()   — entity type
TRoleVM  : class, ICraftRole, new()   — view model type (used in the form)
TRoleDTO : class, ICraftRole, new()   — DTO type (used by the HTTP service)
```

### Parameters

| Parameter          | Type                                               | Required | Description                                                              |
|--------------------|----------------------------------------------------|----------|--------------------------------------------------------------------------|
| `HttpService`      | `IHttpService<TRole, TRoleVM, TRoleDTO, KeyType>`  | ✅ Yes   | Service used to load, create, and update roles.                          |
| `RoleId`           | `KeyType?`                                         | No       | ID of the role to load. When `null` / `default`, the form is in Add mode.|
| `IsViewMode`       | `bool`                                             | No       | When `true`, all fields are read-only and Save is hidden.                |
| `OnSaveSuccess`    | `EventCallback<TRole>`                             | No       | Raised after a successful save. Parent handles post-save navigation.     |
| `OnCancelRequested`| `EventCallback`                                    | No       | Raised when the user clicks Cancel or Back. Parent handles navigation.   |

### Form Fields

| Field         | Type       | Required | Read-only in View Mode |
|---------------|------------|----------|------------------------|
| Name          | `string`   | ✅ Yes   | ✅ Yes                 |
| Description   | `string`   | No       | ✅ Yes                 |
| IsActive      | `bool`     | No       | ✅ Yes                 |

### Modes

| Mode   | Condition                          | Behaviour                                  |
|--------|------------------------------------|--------------------------------------------|
| Add    | `RoleId` is `null` or `default`    | `AddAsync` called on submit; `IsActive` defaults to `true`. |
| Edit   | `RoleId` has a value, `IsViewMode = false` | Entity loaded, `UpdateAsync` called on submit. |
| View   | `RoleId` has a value, `IsViewMode = true`  | Entity loaded, all fields read-only, Save hidden. |

### Usage Examples

**Add page:**

```razor
@inject IHttpService<AppRole, AppRoleVM, AppRoleDTO, KeyType> RoleService
@inject NavigationManager Nav

<RolesEdit TRole="AppRole" TRoleVM="AppRoleVM" TRoleDTO="AppRoleDTO"
           HttpService="@RoleService"
           OnSaveSuccess="@(_ => Nav.NavigateTo(AppRoutes.RolesList, replace: true))"
           OnCancelRequested="@(() => Nav.NavigateTo(AppRoutes.RolesList, replace: true))" />
```

**Edit page (route provides the ID):**

```razor
@inject IHttpService<AppRole, AppRoleVM, AppRoleDTO, KeyType> RoleService
@inject NavigationManager Nav

<RolesEdit TRole="AppRole" TRoleVM="AppRoleVM" TRoleDTO="AppRoleDTO"
           HttpService="@RoleService"
           RoleId="@RoleId"
           OnSaveSuccess="@(_ => Nav.NavigateTo(AppRoutes.RolesList, replace: true))"
           OnCancelRequested="@(() => Nav.NavigateTo(AppRoutes.RolesList, replace: true))" />

@code {
    [Parameter] public KeyType RoleId { get; set; }
}
```

**View page (read-only):**

```razor
<RolesEdit TRole="AppRole" TRoleVM="AppRoleVM" TRoleDTO="AppRoleDTO"
           HttpService="@RoleService"
           RoleId="@RoleId"
           IsViewMode="true"
           OnCancelRequested="@(() => Nav.NavigateTo(AppRoutes.RolesList, replace: true))" />
```

> `OnSaveSuccess` is not required in View mode — Save is hidden when `IsViewMode = true`.

---

## RolesLookup\<TRole\>

A generic `MudSelect` dropdown for picking a single role. Loads all active roles via `IHttpService<TRole>` passed in as a parameter.

### Type Constraint

```
TRole : class, ICraftRole, new()
```

### Parameters

| Parameter               | Type                      | Required | Default                  | Description                              |
|-------------------------|---------------------------|----------|--------------------------|------------------------------------------|
| `HttpService`           | `IHttpService<TRole>`     | ✅ Yes   | —                        | Service used to load the roles list.     |
| `SelectedRoleId`        | `KeyType?`                | No       | `null`                   | The currently selected role ID.          |
| `SelectedRoleIdChanged` | `EventCallback<KeyType?>` | No       | —                        | Two-way binding callback.                |
| `Label`                 | `string`                  | No       | `"Role"`                 | Dropdown label.                          |
| `Placeholder`           | `string`                  | No       | `"Select a role"`        | Placeholder when nothing is selected.    |
| `Variant`               | `Variant`                 | No       | `Variant.Outlined`       | MudBlazor variant.                       |
| `Required`              | `bool`                    | No       | `false`                  | Marks the field as required.             |
| `ReadOnly`              | `bool`                    | No       | `false`                  | Prevents selection.                      |
| `Disabled`              | `bool`                    | No       | `false`                  | Disables the dropdown.                   |
| `Class`                 | `string?`                 | No       | `null`                   | Additional CSS classes.                  |

### Usage Example

```razor
@inject IHttpService<AppRole> RoleService

<RolesLookup TRole="AppRole"
             HttpService="@RoleService"
             @bind-SelectedRoleId="_selectedRoleId"
             Label="Role"
             Required="true"
             Class="mb-3" />
```

---

## UsersEdit\<TUser, TUserVM, TUserDTO, TRole\>

A self-contained Add / Edit / View form for users. Handles loading, saving, validation display, and spinner state internally. Navigation and role assignment are delegated to the parent via `EventCallback`.

### Type Constraints

```
TUser    : class, ICraftUser, new()   — entity type
TUserVM  : class, ICraftUser, new()   — view model type (used in the form)
TUserDTO : class, ICraftUser, new()   — DTO type (used by the HTTP service)
TRole    : class, ICraftRole, new()   — role entity type (used by RolesLookup)
```

### Parameters

| Parameter            | Type                                                | Required | Description                                                                            |
|----------------------|-----------------------------------------------------|----------|----------------------------------------------------------------------------------------|
| `HttpService`        | `IHttpService<TUser, TUserVM, TUserDTO, KeyType>`   | ✅ Yes   | Service used to load, create, and update users.                                        |
| `RoleHttpService`    | `IHttpService<TRole>`                               | No       | Service used to populate the Role dropdown. When `null`, the Role field is hidden.     |
| `UserId`             | `KeyType?`                                          | No       | ID of the user to load. When `null` / `default`, the form is in Add mode.              |
| `IsViewMode`         | `bool`                                              | No       | When `true`, all fields are read-only and Save is hidden.                              |
| `OnSaveSuccess`      | `EventCallback<TUser>`                              | No       | Raised after a successful save. Parent handles post-save navigation.                   |
| `OnCancelRequested`  | `EventCallback`                                     | No       | Raised when the user clicks Cancel or Back. Parent handles navigation.                 |
| `OnRoleSelected`     | `EventCallback<KeyType?>`                           | No       | Raised on save with the selected role ID — parent handles role assignment separately.  |

### Form Fields

| Field         | Type     | Required | Notes                                           |
|---------------|----------|----------|-------------------------------------------------|
| Email / Username | `string` | ✅ Yes | Bound to `Email`. `UserName` is set to the same value before saving. |
| First Name    | `string` | No       | —                                               |
| Last Name     | `string` | No       | —                                               |
| Role          | `KeyType?` | No     | Shown only when `RoleHttpService` is provided.  |
| IsActive      | `bool`   | No       | Toggle; defaults to `true` on Add.              |

### Modes

| Mode   | Condition                                      | Behaviour                                      |
|--------|------------------------------------------------|------------------------------------------------|
| Add    | `UserId` is `null` or `default`                | `AddAsync` called on submit; `IsActive` defaults to `true`. |
| Edit   | `UserId` has a value, `IsViewMode = false`     | Entity loaded, `UpdateAsync` called on submit. |
| View   | `UserId` has a value, `IsViewMode = true`      | Entity loaded, all fields read-only, Save hidden. |

### Role Assignment

Role assignment is intentionally **not handled inside `UsersEdit`** — the component only surfaces the selected role ID via `OnRoleSelected`. The parent page is responsible for calling its own role-assignment service after save. This keeps the component decoupled from identity management details.

### Usage Examples

**Add page:**

```razor
@inject IHttpService<AppUser, AppUserVM, AppUserDTO, KeyType> UserService
@inject IHttpService<AppRole> RoleService
@inject NavigationManager Nav

<UsersEdit TUser="AppUser" TUserVM="AppUserVM" TUserDTO="AppUserDTO" TRole="AppRole"
           HttpService="@UserService"
           RoleHttpService="@RoleService"
           OnRoleSelected="@(roleId => _pendingRoleId = roleId)"
           OnSaveSuccess="@(u => AssignRoleAndNavigateAsync(u, _pendingRoleId))"
           OnCancelRequested="@(() => Nav.NavigateTo(AppRoutes.UsersList, replace: true))" />
```

**Edit page:**

```razor
<UsersEdit TUser="AppUser" TUserVM="AppUserVM" TUserDTO="AppUserDTO" TRole="AppRole"
           HttpService="@UserService"
           RoleHttpService="@RoleService"
           UserId="@UserId"
           OnRoleSelected="@(roleId => _pendingRoleId = roleId)"
           OnSaveSuccess="@(u => AssignRoleAndNavigateAsync(u, _pendingRoleId))"
           OnCancelRequested="@(() => Nav.NavigateTo(AppRoutes.UsersList, replace: true))" />

@code {
    [Parameter] public KeyType UserId { get; set; }
    private KeyType? _pendingRoleId;
}
```

**View page (read-only):**

```razor
<UsersEdit TUser="AppUser" TUserVM="AppUserVM" TUserDTO="AppUserDTO" TRole="AppRole"
           HttpService="@UserService"
           RoleHttpService="@RoleService"
           UserId="@UserId"
           IsViewMode="true"
           OnCancelRequested="@(() => Nav.NavigateTo(AppRoutes.UsersList, replace: true))" />
```

> `OnSaveSuccess` and `OnRoleSelected` are not required in View mode — Save is hidden when `IsViewMode = true`.

---

## UsersLookup\<TUser\>

A generic `MudSelect` dropdown for picking a single user. Displays `FirstName LastName` as the item label, falling back to `UserName` when no name is set. Loads all users via `IHttpService<TUser>` passed in as a parameter.

### Type Constraint

```
TUser : class, ICraftUser, new()
```

### Display Text Resolution

| Condition | Displayed as |
|---|---|
| `FirstName` and/or `LastName` present | `"FirstName LastName"` (trimmed) |
| Both names empty/null | `UserName` |
| `UserName` also null | ID as string (fallback) |

### Parameters

| Parameter               | Type                      | Required | Default           | Description                              |
|-------------------------|---------------------------|----------|-------------------|------------------------------------------|
| `HttpService`           | `IHttpService<TUser>`     | ✅ Yes   | —                 | Service used to load the users list.     |
| `SelectedUserId`        | `KeyType?`                | No       | `null`            | The currently selected user ID.          |
| `SelectedUserIdChanged` | `EventCallback<KeyType?>` | No       | —                 | Two-way binding callback.                |
| `Label`                 | `string`                  | No       | `"User"`          | Dropdown label.                          |
| `Placeholder`           | `string`                  | No       | `"Select a user"` | Placeholder when nothing is selected.    |
| `Variant`               | `Variant`                 | No       | `Variant.Outlined`| MudBlazor variant.                       |
| `Required`              | `bool`                    | No       | `false`           | Marks the field as required.             |
| `ReadOnly`              | `bool`                    | No       | `false`           | Prevents selection.                      |
| `Disabled`              | `bool`                    | No       | `false`           | Disables the dropdown.                   |
| `Class`                 | `string?`                 | No       | `null`            | Additional CSS classes.                  |

### Usage Example

```razor
@inject IHttpService<AppUser> UserService

<UsersLookup TUser="AppUser"
             HttpService="@UserService"
             @bind-SelectedUserId="_selectedUserId"
             Label="Assigned User"
             Required="true"
             Class="mb-3" />
```
