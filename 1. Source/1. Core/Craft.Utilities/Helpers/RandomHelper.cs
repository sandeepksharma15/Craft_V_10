namespace Craft.Utilities.Helpers;

/// <summary>
/// A shortcut to use <see cref="Random"/> class.
/// Also provides some useful methods.
/// </summary>
public static class RandomHelper
{
    /// <summary>
    /// Generates a randomized list from given enumerable.
    /// </summary>
    /// <typeparam name="T">Type of items in the list</typeparam>
    /// <param name="items">items</param>
    public static List<T> GenerateRandomizedList<T>(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        var currentList = new List<T>(items);
        var randomList = new List<T>(currentList.Count);

        while (currentList.Count != 0)
        {
            var randomIndex = GetRandom(0, currentList.Count);
            randomList.Add(currentList[randomIndex]);
            currentList.RemoveAt(randomIndex);
        }

        return randomList;
    }

    /// <summary>
    /// Returns a random number within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
    /// <returns>
    /// A 32-bit signed integer greater than or equal to minValue and less than maxValue;
    /// that is, the range of return values includes minValue but not maxValue.
    /// If minValue equals maxValue, minValue is returned.
    /// </returns>
    public static int GetRandom(int minValue, int maxValue)
        => Random.Shared.Next(minValue, maxValue);

    /// <summary>
    /// Returns a nonnegative random number less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to zero.</param>
    /// <returns>
    /// A 32-bit signed integer greater than or equal to zero, and less than maxValue;
    /// that is, the range of return values ordinarily includes zero but not maxValue.
    /// However, if maxValue equals zero, maxValue is returned.
    /// </returns>
    public static int GetRandom(int maxValue)
        => Random.Shared.Next(maxValue);

    /// <summary>
    /// Returns a nonnegative random number.
    /// </summary>
    /// <returns>A 32-bit signed integer greater than or equal to zero and less than <see cref="int.MaxValue"/>.</returns>
    public static int GetRandom()
        => Random.Shared.Next();

    /// <summary>
    /// Gets random of given objects.
    /// </summary>
    /// <typeparam name="T">Type of the objects</typeparam>
    /// <param name="objs">List of object to select a random one</param>
    public static T GetRandomOf<T>(params T[] objs)
    {
        return objs is null || objs.Length == 0
            ? throw new ArgumentException("Value cannot be an empty collection.", nameof(objs))
            : objs[GetRandom(0, objs.Length)];
    }

    /// <summary>
    /// Gets random item from the given list.
    /// </summary>
    /// <typeparam name="T">Type of the objects</typeparam>
    /// <param name="list">List of object to select a random one</param>
    public static T GetRandomOfList<T>(IList<T> list)
    {
        return list is null || list.Count == 0
            ? throw new ArgumentException("Value cannot be an empty collection.", nameof(list))
            : list[GetRandom(0, list.Count)];
    }
}
