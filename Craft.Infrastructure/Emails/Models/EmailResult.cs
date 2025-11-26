namespace Craft.Infrastructure.Emails;

/// <summary>
/// Represents the result of an email send operation.
/// </summary>
public class EmailResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the email was sent successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the error message if the send operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the exception that occurred during the send operation.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the message ID returned by the email provider.
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the email was sent.
    /// </summary>
    public DateTimeOffset SentAt { get; set; }

    /// <summary>
    /// Creates a successful email result.
    /// </summary>
    public static EmailResult Success(string? messageId = null)
    {
        return new EmailResult
        {
            IsSuccess = true,
            MessageId = messageId,
            SentAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed email result.
    /// </summary>
    public static EmailResult Failure(string errorMessage, Exception? exception = null)
    {
        return new EmailResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Exception = exception,
            SentAt = DateTimeOffset.UtcNow
        };
    }
}
