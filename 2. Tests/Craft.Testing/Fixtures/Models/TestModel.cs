using Craft.Domain;

namespace Craft.Testing.Fixtures;

public record TestModel : BaseModel
{
    public string Name { get; set; } = string.Empty;
}
