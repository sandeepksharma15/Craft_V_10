namespace Craft.Emails;

public class MailRequest(List<string> to, string subject, string? body = null, string? from = null,
    string? displayName = null, string? replyTo = null, string? replyToName = null, List<string>? bcc = null,
    List<string>? cc = null, IDictionary<string, byte[]>? attachmentData = null,
    IDictionary<string, string>? headers = null)
{
    public IDictionary<string, byte[]> AttachmentData { get; } = attachmentData ?? new Dictionary<string, byte[]>();
    public List<string> Bcc { get; } = bcc ?? [];
    public string? Body { get; } = body;
    public List<string> Cc { get; } = cc ?? [];
    public string? DisplayName { get; } = displayName;
    public string? From { get; } = from;
    public IDictionary<string, string> Headers { get; } = headers ?? new Dictionary<string, string>();
    public string? ReplyTo { get; } = replyTo;
    public string? ReplyToName { get; } = replyToName;
    public string Subject { get; } = subject;
    public List<string> To { get; } = to;
}
