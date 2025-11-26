using System.Collections.Concurrent;

namespace Craft.Infrastructure.Emails;

/// <summary>
/// In-memory implementation of email queue for development and testing.
/// </summary>
public class InMemoryEmailQueue : IEmailQueue
{
    private readonly ConcurrentDictionary<string, QueuedEmail> _emails = new();
    private readonly ConcurrentQueue<string> _pendingQueue = new();

    public Task EnqueueAsync(QueuedEmail email, CancellationToken cancellationToken = default)
    {
        _emails[email.Id] = email;
        if (email.Status == EmailStatus.Pending)
            _pendingQueue.Enqueue(email.Id);

        return Task.CompletedTask;
    }

    public Task<QueuedEmail?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        while (_pendingQueue.TryDequeue(out var id))
        {
            if (_emails.TryGetValue(id, out var email) && email.Status == EmailStatus.Pending)
            {
                var now = DateTimeOffset.UtcNow;
                if (!email.ScheduledFor.HasValue || email.ScheduledFor.Value <= now)
                    return Task.FromResult<QueuedEmail?>(email);

                _pendingQueue.Enqueue(id);
                break;
            }
        }

        return Task.FromResult<QueuedEmail?>(null);
    }

    public Task UpdateAsync(QueuedEmail email, CancellationToken cancellationToken = default)
    {
        _emails[email.Id] = email;
        return Task.CompletedTask;
    }

    public Task<QueuedEmail?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _emails.TryGetValue(id, out var email);
        return Task.FromResult(email);
    }

    public Task<IEnumerable<QueuedEmail>> GetByStatusAsync(EmailStatus status, CancellationToken cancellationToken = default)
    {
        var emails = _emails.Values.Where(e => e.Status == status).ToList();
        return Task.FromResult<IEnumerable<QueuedEmail>>(emails);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _emails.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
    {
        var count = _emails.Values.Count(e => e.Status == EmailStatus.Pending);
        return Task.FromResult(count);
    }
}
