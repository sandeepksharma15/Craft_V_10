using Craft.Core;

namespace Craft.MultiTenant;

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

public class CurrentTenant : CurrentTenant<KeyType>, ICurrentTenant
{
    public CurrentTenant(ITenantContextAccessor contextAccessor) : base(contextAccessor)
    {
    }
}
