using Microsoft.EntityFrameworkCore.Metadata;

namespace Craft.QuerySpec;

/// <summary>
/// Supplies EF Core model metadata to evaluators that need to reason about how domain properties are persisted.
/// </summary>
public sealed class QueryEvaluationContext
{
    public QueryEvaluationContext(IReadOnlyEntityType rootEntityType)
    {
        ArgumentNullException.ThrowIfNull(rootEntityType);
        RootEntityType = rootEntityType;
    }

    /// <summary>
    /// Gets the EF Core metadata for the query root entity.
    /// </summary>
    public IReadOnlyEntityType RootEntityType { get; }
}
