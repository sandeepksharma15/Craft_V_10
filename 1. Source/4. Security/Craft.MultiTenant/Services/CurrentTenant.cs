using Craft.Core;

namespace Craft.MultiTenant;

/// <summary>
/// Service that provides access to the current tenant's information by wrapping
/// the tenant context accessor and exposing essential tenant properties.
/// </summary>
/// <typeparam name="TKey">The type used to represent the tenant's ID.</typeparam>
public class CurrentTenant<TKey> : ICurrentTenant<TKey>
{
    private readonly ITenantContextAccessor _contextAccessor;

    public CurrentTenant(ITenantContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    private ITenant<TKey>? CurrentTenantInfo
        => _contextAccessor.TenantContext?.Tenant as ITenant<TKey>;

    public TKey Id => CurrentTenantInfo != null ? CurrentTenantInfo.Id : default!;

    public string? Identifier => CurrentTenantInfo?.Identifier;

    public string? Name => CurrentTenantInfo?.Name;

    public bool IsAvailable => _contextAccessor.TenantContext?.HasResolvedTenant ?? false;

    public bool IsActive => CurrentTenantInfo?.IsActive ?? false;

    public TKey GetId() => Id;
}

/// <summary>
/// Default implementation of ICurrentTenant using KeyType as the key type.
/// </summary>
public class CurrentTenant : CurrentTenant<KeyType>, ICurrentTenant
{
    public CurrentTenant(ITenantContextAccessor contextAccessor) : base(contextAccessor)
    {
    }
}
