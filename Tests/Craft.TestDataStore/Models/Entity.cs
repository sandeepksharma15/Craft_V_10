using Craft.Domain;

namespace Craft.TestDataStore.Models;


public class Entity : ISoftDelete
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
