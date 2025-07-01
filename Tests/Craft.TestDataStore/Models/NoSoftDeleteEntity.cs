using Craft.Domain;

namespace Craft.TestDataStore.Models;

public class NoSoftDeleteEntity : IEntity, IModel
{
    public KeyType Id { get; set; }

    public string? Desc { get; set; }
}
