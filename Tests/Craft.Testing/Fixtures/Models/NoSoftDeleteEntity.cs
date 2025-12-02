using Craft.Domain;

namespace Craft.Testing.Fixtures;

public class NoSoftDeleteEntity : IEntity, IModel
{
    public KeyType Id { get; set; }

    public string? Desc { get; set; }
}
