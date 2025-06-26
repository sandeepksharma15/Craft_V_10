using Craft.Domain;

namespace Craft.TestDataStore.Models;

public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
