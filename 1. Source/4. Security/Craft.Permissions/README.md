# Craft.Permissions

A reusable, optional Blazor permissions library for .NET 10 applications built on Craft.  
Provides role-based permission assignment, per-user session caching, and Blazor UI components — all wired into the `Craft.Data` feature system.

---

## Overview

| Layer | What it does |
|---|---|
| **Server** | Persists role → permission-code mappings in `ID_RolePermissions` via EF Core. Exposes an API controller. |
| **Blazor client** | Loads the current user's permissions on login; caches them per session. |
| **UI components** | `<PermissionView>` and `<RoleOrPermissionView>` for conditional rendering. |
| **Management UI** | `<RolePermissionsEdit>` for assigning permissions to roles. |

Permissions are integer codes defined by the consuming application as constants. `Craft.Permissions` never owns the code values — the app does.

---

## Installation

Add a project reference to `Craft.Permissions` from:
- Your **server / API project**
- Your **Blazor client project**

---

## Step 1 — Define permission codes

In your application (e.g. a shared `MyApp.Shared` project), define your permissions as constants:

```csharp
public static class Permissions
{
	public static class Students
	{
		public const int View   = 1001;
		public const int Create = 1002;
		public const int Edit   = 1003;
		public const int Delete = 1004;
	}

	public static class Reports
	{
		public const int View   = 2001;
		public const int Export = 2002;
	}
}
```

> **Rule:** Every code must be a unique integer across the entire application. A startup validator will throw if duplicates are registered.

---

## Step 2 — Configure the DbContext

In your application's `DbContext` constructor, call `AddPermissions()`:

```csharp
public class AppDbContext : BaseIdentityDbContext<AppDbContext, AppUser, AppRole, KeyType>
{
	public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
		: base(options, currentUser)
	{
		Features
			.AddSoftDelete()
			.AddAuditTrail()
			.AddConcurrency()
			.AddVersionTracking()
			.AddIdentity<AppUser, AppRole, KeyType>()
			.AddPermissions();   // <-- registers ID_RolePermissions table
	}
}
```

`AddPermissions()` is an extension from `Craft.Permissions` on `DbContextFeatureCollection`. It:
- Contributes a `DbSet<RolePermission>` to the context automatically
- Creates the `ID_RolePermissions` table
- Adds a **unique composite index** on `(RoleId, PermissionCode)`
- Adds a supporting index on `RoleId` for fast per-role lookups

After adding, create and apply a migration:

```bash
dotnet ef migrations add AddPermissions
dotnet ef database update
```

---

## Step 3 — Register server-side services

In your server / API `Program.cs`:

```csharp
builder.Services
	.AddCraftPermissions<AppUser>()
	.RegisterPermissions(
		new PermissionDefinition(Permissions.Students.View,   "View Students",   "Students"),
		new PermissionDefinition(Permissions.Students.Create, "Create Students", "Students"),
		new PermissionDefinition(Permissions.Students.Edit,   "Edit Students",   "Students"),
		new PermissionDefinition(Permissions.Students.Delete, "Delete Students", "Students"),
		new PermissionDefinition(Permissions.Reports.View,    "View Reports",    "Reports"),
		new PermissionDefinition(Permissions.Reports.Export,  "Export Reports",  "Reports")
	);
```

`AddCraftPermissions<TUser>()` registers:
- `IPermissionDefinitionRegistry` — singleton in-memory registry of permission metadata
- `IRolePermissionRepository` — scoped EF Core repository (resolves the registered `DbContext`)
- `PermissionStartupValidatorService` — hosted service that validates uniqueness of all registered codes at startup

Also map the controller in your API:

```csharp
app.MapControllers(); // PermissionsController is picked up automatically
```

The API surface exposed is:

| Method | Route | Description |
|---|---|---|
| `GET` | `api/permissions/user` | Returns permission codes for the current user (union of all roles) |
| `GET` | `api/permissions/role/{roleId}` | Returns permission codes for a role |
| `POST` | `api/permissions/role/{roleId}/{code}` | Assigns a permission to a role |
| `DELETE` | `api/permissions/role/{roleId}/{code}` | Revokes a permission from a role |
| `PUT` | `api/permissions/role/{roleId}` | Replaces all permissions for a role |

---

## Step 4 — Register Blazor client services

In your Blazor project's `Program.cs`:

```csharp
builder.Services
	.AddCraftPermissionsUi(new Uri("https://myapi"))
	.RegisterPermissions(
		// same definitions as server-side
		new PermissionDefinition(Permissions.Students.View, "View Students", "Students"),
		// ...
	);
```

`AddCraftPermissionsUi(Uri)` registers:
- `IPermissionDefinitionRegistry` — singleton
- `PermissionSessionCache` — scoped; implements both `IPermissionSessionCache` and `IPermissionChecker`
- `IUserPermissionsHttpService` / `IRolePermissionHttpService` — HTTP clients for the API
- `PermissionAuthStateListener` — loads permissions on login, clears on logout

**Initialize the auth listener** in your root component (e.g. `MainLayout.razor.cs` or `App.razor.cs`):

```csharp
[Inject] private PermissionAuthStateListener PermissionListener { get; set; } = null!;

protected override async Task OnInitializedAsync()
{
	await PermissionListener.InitializeAsync();
}
```

---

## Step 5 — Gate UI with components

### `<PermissionView>` — show content when the user has a permission

```razor
<PermissionView Permission="@Permissions.Students.Delete">
	<Authorized>
		<MudButton Color="Color.Error" OnClick="DeleteStudent">Delete</MudButton>
	</Authorized>
	<NotAuthorized>
		<MudText Typo="Typo.caption">You cannot delete students.</MudText>
	</NotAuthorized>
</PermissionView>
```

`NotAuthorized` is optional — omit it to simply hide the content silently.

### `<RoleOrPermissionView>` — show content when in a role OR has a permission

```razor
<RoleOrPermissionView Roles="Admin,Manager" Permission="@Permissions.Reports.Export">
	<Authorized>
		<MudButton>Export</MudButton>
	</Authorized>
</RoleOrPermissionView>
```

`Roles` is a comma-separated string of role names. Access is granted if the user is in **any** of the roles **or** holds the specified permission.

### Check permissions in code

Inject `IPermissionChecker` anywhere in your Blazor component or service:

```csharp
[Inject] private IPermissionChecker Permissions { get; set; } = null!;

bool canDelete = Permissions.HasPermission(MyApp.Permissions.Students.Delete);
bool canEdit   = Permissions.HasAnyPermission(MyApp.Permissions.Students.Edit,
											   MyApp.Permissions.Students.Create);
```

---

## Step 6 — Manage permissions in the UI

Use the `<RolePermissionsEdit>` component to build an admin page for assigning permissions to roles:

```razor
@page "/admin/roles/{RoleId:long}/permissions"

<RolePermissionsEdit RoleId="@RoleId" />

@code {
	[Parameter] public long RoleId { get; set; }
}
```

The component groups permissions by their `PermissionDefinition.Group`, displays them as toggles, and persists changes via the API on save.

---

## Architecture summary

```
App startup
  ├── Server: AddCraftPermissions<TUser>().RegisterPermissions(...)
  │     ├── IPermissionDefinitionRegistry  (singleton)
  │     ├── IRolePermissionRepository      (scoped → DbContext.Set<RolePermission>)
  │     └── PermissionStartupValidatorService (validates unique codes on app start)
  │
  └── Blazor: AddCraftPermissionsUi(apiUri).RegisterPermissions(...)
		├── IPermissionDefinitionRegistry  (singleton)
		├── PermissionSessionCache         (scoped — IPermissionSessionCache + IPermissionChecker)
		├── IUserPermissionsHttpService    (scoped)
		├── IRolePermissionHttpService     (scoped)
		└── PermissionAuthStateListener    (scoped — loads/clears cache on auth state change)

Database
  └── ID_RolePermissions
		├── PK: Id
		├── RoleId          (FK → your role table)
		├── PermissionCode  (integer, app-defined)
		└── UNIQUE (RoleId, PermissionCode)
```

---

## Key design decisions

- **Integer codes, not strings.** Avoids string allocation and enables O(1) `HashSet<int>` lookups in the session cache.
- **App owns the constants.** `Craft.Permissions` never ships permission values — the consuming app defines them and registers metadata via `PermissionDefinition`.
- **Feature-based DbContext integration.** `AddPermissions()` follows the same `IDbContextFeature` + `IDbSetProvider` pattern as `AddAuditTrail()`, `AddRefreshTokens()`, etc. No manual `DbSet` declaration or `IPermissionDbContext` implementation is required.
- **Optional and reusable.** The library has no opinion on your business domain. Add it to any Craft-based application with a single `AddPermissions()` call on the DbContext and a matching `AddCraftPermissions()` in DI.
