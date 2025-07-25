using Craft.Domain;

namespace Craft.TestDataStore.Models;

// Implements IEntity<int> directly for test compatibility
public class TestEntity : IEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
