using System.Collections.Concurrent;
using Craft.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Craft.MultiTenant.Stores;

public class ConfigurationStore<T> : ITenantStore<T> where T : class, ITenant, IEntity, new()
{
    private readonly IConfigurationSection _section;

    private ConcurrentDictionary<string, T>? _tenantMap;

    public static string SectionName { get; } = "MultiTenancy:ConfigurationStore";

    public ConfigurationStore(IConfiguration configuration) : this(configuration, SectionName)
    {
    }

    public ConfigurationStore(IConfiguration configuration, string sectionName)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        ArgumentException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));

        _section = configuration.GetSection(sectionName);

        if (!_section.Exists())
            throw new MultiTenantException("Section name provided to the Configuration Store is invalid");

        UpdateTenantMap();

        ChangeToken.OnChange(() => _section.GetReloadToken(), UpdateTenantMap);
    }

    private void UpdateTenantMap()
    {
        TenantConfigData values = new();

        var newMap = new ConcurrentDictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        var tenants = _section.GetSection("Tenants").GetChildren();

        foreach (var tenantSection in tenants)
        {
            values = _section.GetSection("Defaults")
                .Get<TenantConfigData>(options => options.BindNonPublicProperties = true) ?? new TenantConfigData();

            tenantSection.Bind(values, options => options.BindNonPublicProperties = true);

            T newTenant = new()
            {
                Id = long.Parse(values?.Id!),
                Identifier = values?.Identifier!,
                Name = values?.Name!,
                ConnectionString = values?.ConnectionString!
            };

            newMap.TryAdd(newTenant.Identifier!, newTenant);
        }

        _tenantMap = newMap;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_tenantMap?.Select(x => x.Value).ToList() ?? []);
    }

    public async Task<T?> GetAsync(KeyType id, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return id == default
            ? throw new ArgumentNullException(nameof(id))
            : await Task.FromResult(_tenantMap?.SingleOrDefault(kv => kv.Value.Id == id).Value);
    }

    public async Task<T?> GetByIdentifierAsync(string identifier, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        return _tenantMap is null ? null : await Task.FromResult(_tenantMap.TryGetValue(identifier, out var result) ? result : null);
    }

    public async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_tenantMap?.Count ?? 0);
    }

    public async Task<T?> GetHostAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_tenantMap?.SingleOrDefault(kv => kv.Value.Type is TenantType.Host).Value);
    }

    #region CRUD Members Not Implemented

    public Task<T?> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T?> DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Private Classes

    private sealed class TenantConfigData
    {
        public string? ConnectionString { get; set; }
        public string? Id { get; set; }
        public string? Identifier { get; set; }
        public string? Name { get; set; }
        public TenantType Type { get; set; }
    }

    #endregion
}
