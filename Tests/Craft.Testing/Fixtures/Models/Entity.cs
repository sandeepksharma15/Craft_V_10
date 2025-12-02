using Craft.Domain;

namespace Craft.Testing.Fixtures;

public class Entity : ISoftDelete
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
