using System.Collections;
using Microsoft.Extensions.Logging;

namespace Craft.Utilities.Managers;

public class ObserverManager<TId, T>(ILogger logger) : IEnumerable<T>
    where T : class
    where TId : notnull
{
    public int Count => _observers.Count;

    public IDictionary<TId, T> Observers
        => _observers.ToDictionary(_ => _.Key, _ => _.Value.Observer);

    private Dictionary<TId, ObserverEntry> _observers { get; } = [];

    public void Clear() => _observers.Clear();

    public IEnumerator<T> GetEnumerator()
        => _observers.Select(observer => observer.Value.Observer).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public async Task NotifyAsync(Func<T, Task> notification, Func<TId, T, bool>? predicate = null)
    {
        var defunctObservers = default(List<TId>);

        foreach (var observer in _observers)
        {
            // Skip observers which don't match the provided predicate.
            if (predicate != null && !predicate(observer.Key, observer.Value.Observer))
                continue;

            try
            {
                await notification(observer.Value.Observer);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error notifying observer");
                defunctObservers ??= [];
                defunctObservers.Add(observer.Key);
            }
        }

        // Remove any observers that errored out
        defunctObservers?.ForEach(id => Unsubscribe(id));
    }

    public void Subscribe(TId id, T observer)
    {
        if (_observers.TryGetValue(id, out var entry))
        {
            entry.Observer = observer;

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Updating entry for {Id}/{Observer}. {Count} total observers.", id, observer, _observers.Count);
        }
        else
        {
            _observers[id] = new ObserverEntry(observer);

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Adding entry for {Id}/{Observer}. {Count} total observers after add.", id, observer, _observers.Count);
        }
    }

    public void Unsubscribe(TId key)
    {
        _observers.Remove(key, out _);

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Observer unsubscribed");
    }

    private sealed class ObserverEntry(T observer)
    {
        public T Observer { get; set; } = observer;
    }
}
