# Craft.AppComponents.Security

A full-stack authentication and user-management module for .NET 10 / Blazor applications. It ships ready-made API controllers, EF Core–backed repositories, typed HTTP services, and a suite of drop-in MudBlazor Razor components — all wired together with a small set of DI extension methods.

---

## Table of Contents

- [Features](#features)
- [Architecture Overview](#architecture-overview)
- [Installation](#installation)
- [Registration](#registration)
  - [API Layer](#api-layer)
  - [Blazor UI Layer](#blazor-ui-layer)
- [API Layer](#api-layer-1)
  - [AuthControllerBase](#authcontrollerbase)
  - [UserController & RoleController](#usercontroller--rolecontroller)
  - [IAuthRepository](#iauthrepository)
  - [Custom E-mail Sender](#custom-e-mail-sender)
- [Blazor UI Components](#blazor-ui-components)
  - [LoginUser](#loginuser)
  - [RegisterUser](#registeruser)
  - [ChangePasswordUser](#changepassworduser)
  - [ForgotPasswordUser](#forgotpassworduser)
  - [NavAuthUser](#navauthoruser)
  - [AccessDeniedUser](#accessdenieduser)
  - [RedirectToLogin](#redirecttologin)
  - [RedirectToAccessDenied](#redirecttoaccessdenied)
- [HTTP Services](#http-services)
  - [IAuthHttpService](#iauthhttpservice)
  - [IUsersHttpService & IRolesHttpService](#iusershttpservice--iroleshttpservice)
- [Customisation](#customisation)
  - [Override Individual Endpoints](#override-individual-endpoints)
  - [Provide a Real E-mail Sender](#provide-a-real-e-mail-sender)
- [Dependencies](#dependencies)

---

## Features

- **Zero-boilerplate auth API** — `AddAuthApi<TUser>()` registers a fully working `POST /api/auth/*` controller with no extra code in the host project.
- **CRUD controllers for users and roles** — `AddSecurityApi<TUser, TRole>()` auto-registers `UserController` and `RoleController` via dynamic feature providers.
- **EF Core–backed repositories** — `AuthRepository`, `UsersRepository`, and `RolesRepository` handle all database work, including refresh-token lifecycle management and login history.
- **JWT + refresh-token authentication** — login, token refresh, and logout (with server-side token revocation) built in.
- **Full password flow** — register, change password, forgot password, and reset password.
- **Typed HTTP services** — Blazor-side `IAuthHttpService<TUserVM>`, `IUsersHttpService`, and `IRolesHttpService` communicate with the API over `HttpClient`.
- **Drop-in MudBlazor components** — `LoginUser`, `RegisterUser`, `ChangePasswordUser`, `ForgotPasswordUser`, `NavAuthUser`, and `AccessDeniedUser` can be placed on any Blazor page with minimal parameters.
- **Redirect helpers** — `RedirectToLogin` and `RedirectToAccessDenied` handle unauthenticated and unauthorised navigation reliably for both initial load and SPA navigation.
- **No-op e-mail sender** — a `NoOpEmailSender<TUser>` is registered by default so the project compiles immediately; swap it out for a real implementation when needed.

---

## Architecture Overview

```
┌──────────────────────────────────────────┐
│              Blazor UI Layer             │
│  Components · IAuthHttpService           │
│  IUsersHttpService · IRolesHttpService   │
└──────────────┬───────────────────────────┘
               │ HTTP (JSON)
┌──────────────▼───────────────────────────┐
│             API Layer                    │
│  AuthControllerBase (api/auth/*)         │
│  UserController   (api/User/*)           │
│  RoleController   (api/Role/*)           │
└──────────────┬───────────────────────────┘
               │
┌──────────────▼───────────────────────────┐
│           Repository Layer               │
│  AuthRepository · UsersRepository        │
│  RolesRepository                         │
│  (ASP.NET Core Identity + EF Core)       │
└──────────────────────────────────────────┘
```

---

## Installation

Add a project or package reference to `Craft.AppComponents`:

```xml
<ProjectReference Include="..\Craft.AppComponents\Craft.AppComponents.csproj" />
```

Add the namespace to `_Imports.razor` in the Blazor project:

```razor
@using Craft.AppComponents.Security
```

---

## Registration

### API Layer

Call these methods inside `Program.cs` (or `Startup.ConfigureServices`) of your **API** project.

#### `AddAuthApi<TUser>()`

Registers a `POST /api/auth/*` controller for login, refresh, logout, register, change-password, forgot-password, and reset-password. Use this when you do **not** have your own auth controller class.

```csharp
builder.Services.AddAuthApi<AppUser>();
```

> **Important:** Do not call `AddAuthApi` when you have a class that already derives from `AuthControllerBase` — doing so registers two controllers on the same `api/auth` route and causes an MVC startup error.

#### `AddSecurityApi<TUser, TRole>()`

Registers `GET/POST/PUT/DELETE /api/User/*` and `GET/POST/PUT/DELETE /api/Role/*` controllers for full CRUD management of users and roles.

```csharp
builder.Services.AddSecurityApi<AppUser, AppRole>();
```

**Full API setup example:**

```csharp
// Program.cs (API project)
builder.Services
    .AddAuthApi<AppUser>()
    .AddSecurityApi<AppUser, AppRole>();
```

### Blazor UI Layer

Call these methods inside `Program.cs` of your **Blazor** project.

#### `AddAuthUI<TUserVM>()`

Registers `IAuthHttpService<TUserVM>` (backed by `AuthHttpService<TUserVM>`) for the auth components.

```csharp
builder.Services.AddAuthUI<AppUserVM>(
    httpClientFactory: sp => sp.GetRequiredService<HttpClient>(),
    baseAddress: "https+http://myapi");
```

#### `AddSecurityUI<TUser, TUserVM, TUserDTO, TRole, TRoleVM, TRoleDTO>()`

Registers `IUsersHttpService`, `IRolesHttpService`, and their simplified variants for user and role management components.

```csharp
builder.Services.AddSecurityUI<AppUser, AppUserVM, AppUserDTO, AppRole, AppRoleVM, AppRoleDTO>(
    httpClientFactory: sp => sp.GetRequiredService<HttpClient>(),
    baseAddress: "https+http://myapi");
```

**Full Blazor setup example:**

```csharp
// Program.cs (Blazor project)
builder.Services
    .AddAuthUI<AppUserVM>(
        sp => sp.GetRequiredService<HttpClient>(),
        "https+http://myapi")
    .AddSecurityUI<AppUser, AppUserVM, AppUserDTO, AppRole, AppRoleVM, AppRoleDTO>(
        sp => sp.GetRequiredService<HttpClient>(),
        "https+http://myapi");
```

---

## API Layer

### AuthControllerBase

`AuthControllerBase` is an abstract `[ApiController]` that lives at `[Route("api/auth")]`. All methods are `virtual`, so derived controllers can selectively override them.

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `api/auth/login` | `POST` | Anonymous | Authenticate with e-mail + password; returns `JwtAuthResponse`. |
| `api/auth/refresh` | `POST` | Anonymous | Exchange an expired access token + refresh token for a new pair. |
| `api/auth/logout` | `POST` | Required | Revoke the bearer token and clear all refresh tokens for the user. |
| `api/auth/register` | `POST` | Anonymous | Create a new user account. |
| `api/auth/change-password` | `POST` | Required | Change the current user's password. |
| `api/auth/forgot-password` | `POST` | Anonymous | Send a password-reset e-mail link. Always returns `204` (never reveals whether the address exists). |
| `api/auth/reset-password` | `POST` | Anonymous | Reset password using the token from the forgot-password e-mail. |

### UserController & RoleController

Both controllers inherit from `EntityController<T, T, KeyType>` (from `Craft.Controllers`) and expose standard CRUD endpoints at `api/User` and `api/Role` respectively. They are registered dynamically via feature providers — no concrete controller class is needed in the host app.

### IAuthRepository

`IAuthRepository` is the single abstraction consumed by `AuthControllerBase`. The built-in `AuthRepository<TUser>` implements it using ASP.NET Core Identity's `UserManager<TUser>` and `SignInManager<TUser>`, plus `ITokenManager` from `Craft.Security`.

Implement `IAuthRepository` directly when you need non-Identity authentication (e.g. LDAP, external IdP), then register your implementation **before** calling `AddAuthApi<TUser>()`:

```csharp
builder.Services.AddScoped<IAuthRepository, MyCustomAuthRepository>();
builder.Services.AddAuthApi<AppUser>(); // will not overwrite the existing registration
```

### Custom E-mail Sender

`NoOpEmailSender<TUser>` is registered as the default `IEmailSender<TUser>`. It silently discards all e-mail calls so that registration and forgot-password flows compile immediately without requiring an SMTP configuration. Replace it by registering your own implementation **before** calling `AddAuthApi<TUser>()`:

```csharp
// Registered first — TryAddScoped inside AddAuthApi will not overwrite this
builder.Services.AddScoped<IEmailSender<AppUser>, SmtpEmailSender<AppUser>>();
builder.Services.AddAuthApi<AppUser>();
```

---

## Blazor UI Components

All components are in the `Craft.AppComponents.Security` namespace and use **MudBlazor** for styling. They are self-contained: they inject their own services, display snackbar feedback, and handle loading/error states internally.

### LoginUser

A sign-in card with e-mail, password (toggle visibility), optional "Remember me" checkbox, and a "Forgot password?" link.

```razor
<LoginUser TUser="AppUserVM"
           SignInEndpoint="/auth/sign-in"
           ForgotPasswordHref="/forgot-password"
           RegisterHref="/register"
           OnSuccess="HandleLoginSuccess" />
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnSuccess` | `EventCallback<JwtAuthResponse>` | — | Fired after successful login with the token response. |
| `OnError` | `EventCallback<string>` | — | Fired on failure with the error message. |
| `SignInEndpoint` | `string?` | `null` | Cookie-setting endpoint; navigates to `{endpoint}?token=...&returnUrl=...` after `OnSuccess`. Leave `null` to handle navigation inside `OnSuccess`. |
| `HomeHref` | `string` | `"/"` | Fallback redirect when no `returnUrl` is present. |
| `ForgotPasswordHref` | `string?` | `null` | Shows a "Forgot password?" link when set. |
| `RegisterHref` | `string` | `"/register"` | Href for the "Register" link. |
| `ShowRememberMe` | `bool` | `true` | Shows or hides the "Remember me" checkbox. |
| `Title` | `string` | `"Sign In"` | Card header text. |
| `AdditionalFields` | `RenderFragment?` | `null` | Extra fields injected inside the form before the submit button. |

### RegisterUser

A registration card with e-mail, username, password, and confirm-password fields.

```razor
<RegisterUser TUser="AppUserVM"
              LoginHref="/login"
              OnSuccess="@(() => Nav.NavigateTo("/login"))" />
```

Supply `CreateRequest` when your user model has fields beyond the four standard ones:

```razor
<RegisterUser TUser="AppUserVM"
              CreateRequest="@(form => new AppUserVM { Email = form.Email, UserName = form.UserName,
                                                        Password = form.Password, PhoneNumber = _phoneNumber })"
              OnSuccess="@(() => Nav.NavigateTo("/login"))" />
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `CreateRequest` | `Func<RegisterFormModel, TUser>?` | `null` | Maps the form model to `TUser`. When `null`, the four standard fields are copied automatically. |
| `OnSuccess` | `EventCallback` | — | Fired after successful registration. |
| `OnError` | `EventCallback<string>` | — | Fired on failure with the error message. |
| `LoginHref` | `string` | `"/login"` | Href for the "Sign In" link below the form. |
| `Title` | `string` | `"Create Account"` | Card header text. |
| `AdditionalFields` | `RenderFragment?` | `null` | Extra fields injected after "Confirm Password" and before the submit button. |

### ChangePasswordUser

A change-password card for authenticated users. Reads the current user ID from the `ClaimTypes.NameIdentifier` claim automatically.

```razor
<ChangePasswordUser TUser="AppUserVM" ReturnHref="/profile" />
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OnSuccess` | `EventCallback` | — | Fired after a successful password change. |
| `OnError` | `EventCallback<string>` | — | Fired on failure with the error message. |
| `ReturnHref` | `string?` | `"/"` | "Cancel" / "Continue" link. Set to `null` to hide it. |
| `Title` | `string` | `"Change Password"` | Card header text. |

### ForgotPasswordUser

A forgot-password card. Builds the reset-password callback URL automatically from `NavigationManager.BaseUri` + `ResetPasswordHref`.

```razor
<ForgotPasswordUser TUser="AppUserVM"
                    ResetPasswordHref="/reset-password"
                    LoginHref="/login" />
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ResetPasswordHref` | `string` | `"/reset-password"` | Relative path to the reset-password page, appended to `BaseUri`. |
| `OnSuccess` | `EventCallback` | — | Fired after the reset link is dispatched. |
| `OnError` | `EventCallback<string>` | — | Fired on failure with the error message. |
| `LoginHref` | `string` | `"/login"` | Href for the "Sign In" link. |
| `Title` | `string` | `"Forgot Password"` | Card header text. |

### NavAuthUser

A navigation component for `MudNavMenu`. Renders Login + Register links when the user is anonymous, and a collapsible user group with Logout (and optional Change Password) when authenticated.

```razor
<MudNavMenu>
    <NavAuthUser TUser="AppUserVM"
                 SignOutEndpoint="/auth/sign-out"
                 ChangePasswordHref="/change-password" />
</MudNavMenu>
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `LoginHref` | `string` | `"/login"` | Login nav link and post-logout redirect destination. |
| `RegisterHref` | `string` | `"/register"` | Register nav link. |
| `SignOutEndpoint` | `string?` | `"/auth/sign-out"` | Cookie-clearing endpoint; navigates to `{endpoint}?returnUrl={LoginHref}` after logout. Set to `null` to handle navigation inside `OnLogout`. |
| `OnLogout` | `EventCallback` | — | Fired after `LogoutAsync` completes and before any internal navigation. |
| `ChangePasswordHref` | `string?` | `null` | Shows a "Change Password" link when set. |
| `LoginIcon` | `string` | `Login` (Material) | MudBlazor icon for the Login link. |
| `RegisterIcon` | `string` | `PersonAdd` (Material) | MudBlazor icon for the Register link. |
| `LogoutIcon` | `string` | `Logout` (Material) | MudBlazor icon for the Logout link. |
| `ChangePasswordIcon` | `string` | `Lock` (Material) | MudBlazor icon for the Change Password link. |
| `UserGroupIcon` | `string` | `AccountCircle` (Material) | Icon for the authenticated user nav group. |

### AccessDeniedUser

An access-denied card displayed when an authenticated user lacks the required role or policy.

```razor
<AccessDeniedUser HomeHref="/" LoginHref="/login" />
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `HomeHref` | `string` | `"/"` | "Go Home" button destination. |
| `LoginHref` | `string?` | `null` | "Sign in with a different account" link. Set to `null` to hide it. |
| `Title` | `string` | `"Access Denied"` | Card header text. |

### RedirectToLogin

A render-only component that immediately redirects unauthenticated users to the login page, appending the current URL as a `returnUrl` query-string parameter.

```razor
<!-- In Routes.razor NotAuthorized template -->
@if (context.User.Identity?.IsAuthenticated == true)
{
    <RedirectToAccessDenied />
}
else
{
    <RedirectToLogin />
}
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `LoginRoute` | `string` | `"/login"` | The login page route. |

### RedirectToAccessDenied

A render-only component that immediately redirects authenticated-but-unauthorised users to the access-denied page.

```razor
<RedirectToAccessDenied AccessDeniedRoute="/access-denied" />
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `AccessDeniedRoute` | `string` | `"/access-denied"` | The access-denied page route. |

---

## HTTP Services

### IAuthHttpService

`IAuthHttpService<TUserVM>` is the client-side counterpart of `AuthControllerBase`. It is injected into all auth UI components.

| Method | API endpoint | Description |
|---|---|---|
| `LoginAsync` | `POST /api/auth/login` | Returns `ServiceResult<JwtAuthResponse>`. |
| `RefreshAsync` | `POST /api/auth/refresh` | Returns `ServiceResult<JwtAuthResponse>`. |
| `LogoutAsync` | `POST /api/auth/logout` | Returns `ServiceResult`. |
| `RegisterAsync` | `POST /api/auth/register` | Returns `ServiceResult`. |
| `ChangePasswordAsync` | `POST /api/auth/change-password` | Returns `ServiceResult`. |
| `ForgotPasswordAsync` | `POST /api/auth/forgot-password` | Returns `ServiceResult`. |
| `ResetPasswordAsync` | `POST /api/auth/reset-password` | Returns `ServiceResult`. |

Register via `AddAuthUI<TUserVM>()` — see [Registration](#registration).

### IUsersHttpService & IRolesHttpService

Typed HTTP services that communicate with `api/User` and `api/Role` respectively. Both extend `IHttpService<T, ViewT, DataTransferT, KeyType>` from `Craft.HttpServices` for standard CRUD, and are registered with three interface aliases (strongly-typed, keyed, and simplified) so any injection style works:

```csharp
// All three resolve the same UsersHttpService instance
IUsersHttpService<AppUser, AppUserVM, AppUserDTO, KeyType> typedService;
IHttpService<AppUser, AppUserVM, AppUserDTO, KeyType>      keyedService;
IHttpService<AppUser>                                       listService;
```

Register via `AddSecurityUI<...>()` — see [Registration](#registration).

---

## Customisation

### Override Individual Endpoints

Derive from `AuthControllerBase` in the host application when you need to customise one or more auth endpoints. **Do not** call `AddAuthApi<TUser>()` in this case — the derived class is registered automatically by MVC's controller discovery.

```csharp
[ApiController]
public class AppAuthController(IAuthRepository repo, ILogger<AuthControllerBase> logger)
    : AuthControllerBase(repo, logger)
{
    // Override only what you need
    public override async Task<IActionResult> LoginAsync(
        [FromBody] UserLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // Custom pre-login logic (e.g. rate limiting, tenant resolution)
        var result = await base.LoginAsync(request, cancellationToken);
        // Custom post-login logic
        return result;
    }
}
```

### Provide a Real E-mail Sender

Implement `IEmailSender<TUser>` from `Microsoft.AspNetCore.Identity` and register it before calling `AddAuthApi<TUser>()`:

```csharp
// Register your implementation first
builder.Services.AddScoped<IEmailSender<AppUser>, SendGridEmailSender<AppUser>>();

// AddAuthApi uses TryAddScoped, so the above registration is kept
builder.Services.AddAuthApi<AppUser>();
```

---

## Dependencies

| Dependency | Purpose |
|---|---|
| `Craft.Security` | JWT token management, identity models, claims helpers |
| `Craft.Repositories` | `BaseRepository<T, TKey>`, `IBaseRepository<T, TKey>` |
| `Craft.QuerySpec` | Specification-pattern query helpers used by user/role repositories |
| `Craft.HttpServices` | `HttpServiceBase`, `IHttpService<T>` — base for all HTTP clients |
| `Craft.Controllers` | `EntityController<T, ViewT, TKey>` — base for `UserController` / `RoleController` |
| `MudBlazor` | Component library used by all UI components |
| `Microsoft.AspNetCore.Identity` | `UserManager`, `SignInManager`, `IEmailSender<TUser>` |
| `Microsoft.EntityFrameworkCore` | Database access inside repositories |
