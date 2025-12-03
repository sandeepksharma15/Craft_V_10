namespace Craft.Cache;

/// <summary>
/// Cache statistics for monitoring and diagnostics.
/// </summary>
public class CacheStats
{
    private long _hits;
    private long _misses;
    private long _sets;
    private long _removes;

    /// <summary>
    /// Gets or sets the total number of cache hits.
    /// </summary>
    public long Hits 
    { 
        get => Interlocked.Read(ref _hits);
        set => Interlocked.Exchange(ref _hits, value);
    }

    /// <summary>
    /// Gets or sets the total number of cache misses.
    /// </summary>
    public long Misses 
    { 
        get => Interlocked.Read(ref _misses);
        set => Interlocked.Exchange(ref _misses, value);
    }

    /// <summary>
    /// Gets or sets the total number of cache sets.
    /// </summary>
    public long Sets 
    { 
        get => Interlocked.Read(ref _sets);
        set => Interlocked.Exchange(ref _sets, value);
    }

    /// <summary>
    /// Gets or sets the total number of cache removes.
    /// </summary>
    public long Removes 
    { 
        get => Interlocked.Read(ref _removes);
        set => Interlocked.Exchange(ref _removes, value);
    }

    /// <summary>
    /// Gets or sets the approximate number of entries in the cache.
    /// </summary>
    public long EntryCount { get; set; }

    /// <summary>
    /// Gets the cache hit ratio (0.0 to 1.0).
    /// </summary>
    public double HitRatio => TotalRequests > 0 ? (double)Hits / TotalRequests : 0;

    /// <summary>
    /// Gets the total number of cache requests (hits + misses).
    /// </summary>
    public long TotalRequests => Hits + Misses;

    /// <summary>
    /// Gets or sets the timestamp when stats were captured.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets access to the internal hits field for Interlocked operations.
    /// </summary>
    internal ref long HitsRef => ref _hits;

    /// <summary>
    /// Gets access to the internal misses field for Interlocked operations.
    /// </summary>
    internal ref long MissesRef => ref _misses;

    /// <summary>
    /// Gets access to the internal sets field for Interlocked operations.
    /// </summary>
    internal ref long SetsRef => ref _sets;

    /// <summary>
    /// Gets access to the internal removes field for Interlocked operations.
    /// </summary>
    internal ref long RemovesRef => ref _removes;

    /// <summary>
    /// Resets all statistics to zero.
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
        Interlocked.Exchange(ref _sets, 0);
        Interlocked.Exchange(ref _removes, 0);
        EntryCount = 0;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Returns a string representation of the cache statistics.
    /// </summary>
    public override string ToString()
    {
        return $"Hits: {Hits}, Misses: {Misses}, HitRatio: {HitRatio:P2}, Sets: {Sets}, Removes: {Removes}, Entries: {EntryCount}";
    }
}

