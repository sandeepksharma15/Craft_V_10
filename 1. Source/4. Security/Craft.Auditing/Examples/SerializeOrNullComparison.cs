namespace Craft.Auditing.Examples;

/// <summary>
/// Example comparing the old and new SerializeOrNull implementation.
/// This demonstrates the improvements in readability and functionality.
/// </summary>
public static class SerializeOrNullComparison
{
    /*
     * OLD IMPLEMENTATION (Complex, hard to read):
     * ===========================================
     * 
     * private static string? SerializeOrNull<T>(T value)
     * {
     *     return value is ICollection<object> collection && collection.Count == 0
     *         ? null
     *         : value is IDictionary<string, object> dict && dict.Count == 0 ? null : value is null ? null : JsonSerializer.Serialize(value);
     * }
     * 
     * Issues:
     * - Nested ternary operators make it hard to read
     * - No control over JSON serialization behavior
     * - Could fail with circular references
     * - Could fail with deep object graphs
     * - Hard to debug
     * - Hard to extend with new conditions
     * 
     * 
     * NEW IMPLEMENTATION (Clean, maintainable):
     * ==========================================
     * 
     * private static readonly JsonSerializerOptions SerializerOptions = new()
     * {
     *     WriteIndented = false,                                    // Minimize JSON size
     *     DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,  // Reduce payload
     *     ReferenceHandler = ReferenceHandler.IgnoreCycles,        // Handle circular refs
     *     MaxDepth = 32                                             // Prevent stack overflow
     * };
     * 
     * private static string? SerializeOrNull<T>(T value)
     * {
     *     if (value is null)
     *         return null;
     * 
     *     if (value is ICollection<object> collection && collection.Count == 0)
     *         return null;
     * 
     *     if (value is IDictionary<string, object> dict && dict.Count == 0)
     *         return null;
     * 
     *     return JsonSerializer.Serialize(value, SerializerOptions);
     * }
     * 
     * Benefits:
     * ? Clear early-return pattern
     * ? Each condition is on its own line
     * ? Easy to add new conditions
     * ? Handles circular references safely
     * ? Prevents stack overflow with deep graphs
     * ? Optimized JSON output
     * ? Reuses JsonSerializerOptions (performance)
     * ? Easy to debug and understand
     * ? XML documentation added
     */

    /// <summary>
    /// Example scenarios that benefit from the improved implementation:
    /// </summary>
    public class ExampleScenarios
    {
        // Scenario 1: Circular Reference (Would crash old implementation)
        public class Parent
        {
            public string Name { get; set; } = string.Empty;
            public Child? Child { get; set; }
        }

        public class Child
        {
            public string Name { get; set; } = string.Empty;
            public Parent? Parent { get; set; }  // Circular reference
        }

        // The new implementation handles this gracefully with ReferenceHandler.IgnoreCycles

        // Scenario 2: Deep Object Graph (Could stack overflow in old implementation)
        public class DeepNode
        {
            public string Value { get; set; } = string.Empty;
            public DeepNode? Next { get; set; }
        }

        // The new implementation protects against this with MaxDepth = 32

        // Scenario 3: Empty Collections (Handled by both, but clearer in new)
        public Dictionary<string, object> EmptyDict = new();
        public List<object> EmptyList = new();

        // The new implementation makes it obvious these return null

        // Scenario 4: Null Values (Handled by both, but clearer in new)
        public Dictionary<string, object>? NullDict = null;

        // The new implementation checks null first, making it more efficient
    }
}
