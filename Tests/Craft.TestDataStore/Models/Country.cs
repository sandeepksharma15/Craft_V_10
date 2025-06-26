using Craft.Domain;

namespace Craft.TestDataStore.Models;


public class Country : BaseEntity
{
    public List<Company>? Companies { get; set; }
    public string? Name { get; set; }
}
