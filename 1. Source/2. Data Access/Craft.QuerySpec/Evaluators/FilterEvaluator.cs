namespace Craft.QuerySpec;

/// <summary>
/// Applies only filtering evaluators. This is useful for count queries where pagination,
/// ordering, includes, and projection must not affect the result, while both Where and Search
/// criteria must still be honoured.
/// </summary>
public sealed class FilterEvaluator : QueryEvaluator
{
    private FilterEvaluator()
        : base([
            WhereEvaluator.Instance,
            SearchEvaluator.Instance
        ])
    {
    }

    public static new FilterEvaluator Instance { get; } = new();
}
