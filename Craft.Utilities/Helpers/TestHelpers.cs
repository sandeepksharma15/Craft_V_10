using System.Reflection;
using Xunit;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Craft.Utilities.Helpers;

public static class TestHelpers
{
    /// <summary>
    /// Helper Method to Compare If Two Objects Have The Same Set Of Data (Deep Comparison)
    /// </summary>
    /// <typeparam name="T">The Base Type Of Two Objects</typeparam>
    /// <param name="one">One Object Derived From T</param>
    /// <param name="another">Another Object Derived From T</param>
    public static void AreTheySame<T>(T one, T another)
    {
        ArgumentNullException.ThrowIfNull(one, nameof(one));
        ArgumentNullException.ThrowIfNull(another, nameof(another));

        var visited = new HashSet<(int, int)>();

        AreTheySameRecursive(one!, another!, visited);
    }

    private static void AreTheySameRecursive(object one, object another, HashSet<(int, int)> visited)
    {
        if (ReferenceEquals(one, another)) 
            return;

        // Will fail if not both null
        if (one == null || another == null)
        {
            Assert.Equal(one, another); 
            return;
        }

        var ids = (RuntimeHelpers.GetHashCode(one), RuntimeHelpers.GetHashCode(another));

        // Prevent Cycles
        if (visited.Contains(ids)) 
            return; 

        visited.Add(ids);

        var type = one!.GetType();

        if (type != another!.GetType()) Assert.Equal(type, another.GetType());

        // Handle value types and string
        if (type.IsValueType || type == typeof(string))
        {
            Assert.Equal(one, another);
            return;
        }

        // Handle collections (IEnumerable but not string)
        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            Assert.Equivalent(one, another, strict: false);
            return;
        }

        // For complex objects, recursively compare public instance properties
        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead) 
                continue;

            object? oneValue = prop.GetValue(one);
            object? anotherValue = prop.GetValue(another);

            if (oneValue == null && anotherValue == null) 
                continue;

            if (oneValue == null || anotherValue == null) 
                Assert.Equal(oneValue, anotherValue);
            else 
                AreTheySameRecursive(oneValue, anotherValue, visited);
        }

        // Now compare public instance fields
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            object? oneValue = field.GetValue(one);
            object? anotherValue = field.GetValue(another);

            if (oneValue == null && anotherValue == null)
                continue;

            if (oneValue == null || anotherValue == null)
                Assert.Equal(oneValue, anotherValue);
            else
                AreTheySameRecursive(oneValue, anotherValue, visited);
        }
    }

    /// <summary>
    /// Helper Method to Compare If Another Object Is Having Same Set Of Data As This One
    /// </summary>
    /// <typeparam name="T">The Base Type</typeparam>
    /// <param name="one">THIS Object Derived From T</param>
    /// <param name="another">Another Object Derived From T</param>
    public static void ShouldBeSameAs<T>(this T? one, T? another)
    {
        if (one == null && another == null) return;
        if (one == null || another == null) throw new ArgumentException("One of the objects is null.");

        AreTheySame(one!, another!);
    }
}
