# Craft.Security

A comprehensive security library for .NET 10 applications providing JWT authentication, token management, user context services, and identity management.

## Features

- **JWT Token Management**: Complete JWT token generation, validation, and revocation
- **Token Blacklisting**: Support for revoking tokens with automatic cleanup
- **Current User Services**: Access authenticated user information in API and UI contexts
- **Identity Models**: Extended ASP.NET Core Identity models with soft delete and audit support
- **Claims Extensions**: Convenient extension methods for working with claims
- **OAuth Support**: Google authentication configuration options
- **Refresh Tokens**: Secure refresh token management
- **Login History**: Track user login attempts and sessions

## Installation

Add the package reference to your project:

```xml
<PackageReference Include="Craft.Security" Version="1.0.0" />
```

## Configuration

### JWT Settings

Add JWT configuration to your `appsettings.json`:

```json
{
  "SecuritySettings": {
    "JwtSettings": {
      "IssuerSigningKey": "your-secret-key-at-least-32-characters-long",
      "ValidIssuer": "YourIssuer",
      "ValidAudiences": ["YourAudience"],
      "TokenExpirationInMinutes": 60,
      "RefreshTokenExpirationInDays": 7,
      "ValidateAudience": true,
      "ValidateIssuer": true,
      "ValidateIssuerSigningKey": true,
      "ValidateLifetime": true,
      "RequireExpirationTime": true,
      "RequireHttpsMetaData": true,
      "RequireSignedTokens": true,
      "SaveToken": true,
      "IncludeErrorDetails": true,
      "ClockSkew": 5
    }
  }
}
```

### Google Authentication (Optional)

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "ProjectId": "your-project-id",
      "AuthUri": "https://accounts.google.com/o/oauth2/auth",
      "TokenUri": "https://oauth2.googleapis.com/token",
      "AuthProviderX509CertUrl": "https://www.googleapis.com/oauth2/v1/certs",
      "RedirectUris": ["https://localhost:5001/signin-google"],
      "JavascriptOrigins": ["https://localhost:5001"]
    }
  }
}
```

## Registration

### Basic Setup

```csharp
using Craft.Security;
using Craft.Security.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add JWT authentication
builder.Services.ConfigureJwt(builder.Configuration);

// Add token management with blacklist and cleanup service
builder.Services.AddTokenManagement();

// Add Craft Security services (includes TokenManager)
builder.Services.AddCraftSecurity();

// For API projects
builder.Services.AddCurrentApiUser();

// For Blazor UI projects
builder.Services.AddCurrentUiUser();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

## Usage

### Token Manager

Generate and manage JWT tokens:

```csharp
public class AuthenticationService
{
    private readonly ITokenManager _tokenManager;

    public AuthenticationService(ITokenManager tokenManager)
    {
        _tokenManager = tokenManager;
    }

    public JwtAuthResponse Login(string userId, string email, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        // Generate both JWT and refresh token
        return _tokenManager.GenerateJwtTokens(claims);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        // Validates token and checks if revoked
        return await _tokenManager.ValidateTokenAsync(token);
    }

    public async Task LogoutAsync(string token)
    {
        // Revoke the token
        await _tokenManager.RevokeTokenAsync(token);
    }
}
```

### Current User Service

Access authenticated user information:

```csharp
public class UserProfileService
{
    private readonly ICurrentUser _currentUser;

    public UserProfileService(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public UserProfile GetCurrentUserProfile()
    {
        if (!_currentUser.IsAuthenticated())
            throw new UnauthorizedAccessException();

        return new UserProfile
        {
            Id = _currentUser.GetId(),
            Email = _currentUser.GetEmail(),
            FullName = _currentUser.GetFullName(),
            Role = _currentUser.GetRole(),
            Tenant = _currentUser.GetTenant(),
            ImageUrl = _currentUser.GetImageUrl()
        };
    }
}
```

### Refresh Token Flow

```csharp
public class TokenRefreshService
{
    private readonly ITokenManager _tokenManager;
    private readonly UserManager<CraftUser> _userManager;

    public async Task<JwtAuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Validate the expired token
        var principal = _tokenManager.GetPrincipalFromExpiredToken(request.JwtToken);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new SecurityTokenException("Invalid token");

        var user = await _userManager.FindByIdAsync(userId);
        
        // Verify refresh token from database
        // ... validation logic ...

        // Generate new tokens
        var claims = await GetUserClaimsAsync(user);
        return _tokenManager.GenerateJwtTokens(claims);
    }
}
```

### Custom Token Blacklist

Implement a distributed blacklist for multi-server scenarios:

```csharp
public class RedisTokenBlacklist : ITokenBlacklist
{
    private readonly IConnectionMultiplexer _redis;

    public RedisTokenBlacklist(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task AddAsync(string token, DateTime expiration, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var timeToLive = expiration - DateTime.UtcNow;
        await db.StringSetAsync(token, "1", timeToLive);
    }

    public async Task<bool> IsBlacklistedAsync(string token, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(token);
    }

    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        // Redis automatically expires keys
        return 0;
    }
}

// Register in DI
builder.Services.AddSingleton<ITokenBlacklist, RedisTokenBlacklist>();
```

### Claims Extensions

Work with claims easily:

```csharp
public class ClaimsService
{
    public void ProcessUser(ClaimsPrincipal principal)
    {
        var userId = principal.GetUserId();
        var email = principal.GetEmail();
        var fullName = principal.GetFullName();
        var firstName = principal.GetFirstName();
        var lastName = principal.GetLastName();
        var role = principal.GetRole();
        var tenant = principal.GetTenant();
        var permissions = principal.GetPermissions();
        var imageUrl = principal.GetImageUrl();
        var phoneNumber = principal.GetMobileNumber();
        var jwtToken = principal.GetJwtToken();
    }
}
```

### Entity Models

Use provided entity models in your DbContext:

```csharp
public class ApplicationDbContext : IdentityDbContext<CraftUser, CraftRole, long>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<LoginHistory> LoginHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure entities
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.UserId);
        });

        builder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
        });
    }
}
```

### Bearer Token Handler

Automatically attach bearer tokens from cookies to HTTP requests:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();

builder.Services.AddHttpClient("AuthenticatedClient")
    .AddHttpMessageHandler<BearerTokenHandler>();
```

## Models

### CraftUser

Extended `IdentityUser` with additional properties:

- `FirstName`, `LastName`, `FullName`
- `DialCode`, `PhoneNumber`
- `Gender`, `Title` (HonorificType)
- `ImageUrl`
- `IsActive`, `IsDeleted` (soft delete support)
- `Activate()`, `Deactivate()` methods

### CraftRole

Extended `IdentityRole` with:

- `Description`
- `IsActive`, `IsDeleted`
- `Activate()`, `Deactivate()` methods

### RefreshToken

Manages refresh tokens with:

- `Token`, `ExpiryTime`
- `UserId` foreign key
- Soft delete support

### LoginHistory

Tracks login attempts:

- `LastIpAddress`, `LastLoginOn`
- `Provider` (authentication provider)
- `UserId` foreign key

## Custom Claims

The `CraftClaims` class defines standard claim types:

- `CraftClaims.Id` / `CraftClaims.UserId`
- `CraftClaims.Fullname`
- `CraftClaims.ImageUrl`
- `CraftClaims.JwtToken`
- `CraftClaims.Permissions`
- `CraftClaims.Role`
- `CraftClaims.Tenant`

## Security Best Practices

1. **Secret Key**: Use a strong, randomly generated secret key (minimum 32 characters)
2. **HTTPS**: Always use HTTPS in production (`RequireHttpsMetaData: true`)
3. **Token Expiration**: Set appropriate expiration times (access tokens: 15-60 minutes, refresh tokens: 7-30 days)
4. **Token Revocation**: Implement token revocation for logout and security events
5. **Refresh Tokens**: Store refresh tokens securely in the database, not in JWT
6. **Claims**: Don't store sensitive information in JWT claims (they're base64 encoded, not encrypted)
7. **Clock Skew**: Configure clock skew to handle time synchronization issues between servers

## Token Blacklist

The library includes an in-memory token blacklist suitable for single-server deployments:

- Automatically removes expired tokens every hour
- Thread-safe concurrent dictionary implementation
- Hashes tokens using SHA256 before storage

For distributed systems, implement `ITokenBlacklist` using Redis, database, or other distributed cache.

## Advanced Scenarios

### Custom User Provider

Implement custom user providers for different contexts:

```csharp
public class CustomUserProvider : ICurrentUserProvider
{
    public ClaimsPrincipal? GetUser()
    {
        // Custom logic to retrieve user
        return customPrincipal;
    }
}

builder.Services.AddScoped<ICurrentUserProvider, CustomUserProvider>();
```

### Multi-Tenant Support

Use the `Tenant` claim for multi-tenant applications:

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, userId),
    new Claim(CraftClaims.Tenant, tenantId)
};

// Access in services
var tenantId = _currentUser.GetTenant();
```

### Permission-Based Authorization

Store permissions in claims:

```csharp
var permissions = string.Join(",", new[] { "users.read", "users.write", "reports.read" });
var claims = new List<Claim>
{
    new Claim(CraftClaims.Permissions, permissions)
};

// Check permissions
var userPermissions = principal.GetPermissions()?.Split(',') ?? [];
var canWrite = userPermissions.Contains("users.write");
```

## Testing

The library includes comprehensive unit tests. To run tests:

```bash
dotnet test Craft.Security.Tests.csproj
```

All components are designed with testability in mind:

- Interfaces for all major services
- TimeProvider for time-dependent logic
- Dependency injection support

## Dependencies

- .NET 10.0
- Microsoft.AspNetCore.App (framework reference)
- Microsoft.AspNetCore.Authentication.JwtBearer 10.0.1
- System.IdentityModel.Tokens.Jwt 8.15.0
- Craft.Core
- Craft.Domain
- Craft.Auditing

## License

This project is part of the Craft framework.

## Contributing

Contributions are welcome! Please ensure:

1. All new code has corresponding unit tests
2. Code follows the established patterns and conventions
3. XML documentation comments are provided for public APIs
4. Changes are backward compatible when possible

## Support

For issues, questions, or contributions, please visit the project repository.
