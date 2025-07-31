namespace Craft.MultiTenant;

public class InMemoryStoreOptions<T> where T : class, ITenant, new()
{
    public static string SectionName { get; } = "MultiTenancy:InMemoryStore";

    public bool IsCaseSensitive { get; set; } = false;
    public IList<T> Tenants { get; set; } = [];
}
