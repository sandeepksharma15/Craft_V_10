namespace Craft.Emails;

/// <summary>
/// Defines the contract for email queue storage and retrieval.
/// </summary>
public interface IEmailQueue
{
    /// <summary>
    /// Enqueues an email for sending.
    /// </summary>
    /// <param name="email">The queued email to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task EnqueueAsync(QueuedEmail email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues the next pending email to send.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The next email to send, or null if the queue is empty.</returns>
    Task<QueuedEmail?> DequeueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a queued email.
    /// </summary>
    /// <param name="email">The email to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(QueuedEmail email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a queued email by ID.
    /// </summary>
    /// <param name="id">The email ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<QueuedEmail?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all queued emails with a specific status.
    /// </summary>
    /// <param name="status">The email status to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IEnumerable<QueuedEmail>> GetByStatusAsync(EmailStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a queued email.
    /// </summary>
    /// <param name="id">The email ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of pending emails in the queue.
    /// </summary>
    Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);
}
