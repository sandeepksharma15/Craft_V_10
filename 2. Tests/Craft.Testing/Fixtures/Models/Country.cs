using Craft.Domain;

namespace Craft.Testing.Fixtures;

public class Country : BaseEntity
{
    public List<Company>? Companies { get; set; }
    public string? Name { get; set; }
}
