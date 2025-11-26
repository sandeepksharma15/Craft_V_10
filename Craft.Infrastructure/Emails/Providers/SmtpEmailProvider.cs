using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Craft.Infrastructure.Emails;

/// <summary>
/// SMTP email provider implementation using MailKit.
/// </summary>
public class SmtpEmailProvider : IEmailProvider
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailProvider> _logger;

    public SmtpEmailProvider(IOptions<EmailOptions> options, ILogger<SmtpEmailProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public string Name => "smtp";

    public bool IsConfigured()
    {
        return _options.Smtp != null
            && !string.IsNullOrWhiteSpace(_options.Smtp.Host)
            && _options.Smtp.Port > 0;
    }

    public async Task<EmailResult> SendAsync(MailRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured())
            return EmailResult.Failure("SMTP provider is not properly configured");

        try
        {
            var email = BuildMimeMessage(request);

            using var smtp = new SmtpClient();
            smtp.Timeout = _options.Smtp!.TimeoutSeconds * 1000;

            await smtp.ConnectAsync(
                _options.Smtp.Host,
                _options.Smtp.Port,
                _options.Smtp.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.Smtp.UserName) &&
                !string.IsNullOrWhiteSpace(_options.Smtp.Password))
            {
                await smtp.AuthenticateAsync(_options.Smtp.UserName, _options.Smtp.Password, cancellationToken);
            }

            var response = await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully via SMTP. Response: {Response}", response);

            return EmailResult.Success(email.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP");
            return EmailResult.Failure(ex.Message, ex);
        }
    }

    private MimeMessage BuildMimeMessage(MailRequest request)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(
            request.DisplayName ?? _options.DisplayName,
            request.From ?? _options.From));

        foreach (var address in request.To)
            email.To.Add(MailboxAddress.Parse(address));

        if (!string.IsNullOrEmpty(request.ReplyTo))
            email.ReplyTo.Add(new MailboxAddress(request.ReplyToName, request.ReplyTo));

        if (request.Bcc != null)
            foreach (var address in request.Bcc.Where(bcc => !string.IsNullOrWhiteSpace(bcc)))
                email.Bcc.Add(MailboxAddress.Parse(address.Trim()));

        if (request.Cc != null)
            foreach (var address in request.Cc.Where(cc => !string.IsNullOrWhiteSpace(cc)))
                email.Cc.Add(MailboxAddress.Parse(address.Trim()));

        if (request.Headers != null)
            foreach (var header in request.Headers)
                email.Headers.Add(header.Key, header.Value);

        var builder = new BodyBuilder();
        email.Sender = new MailboxAddress(
            request.DisplayName ?? _options.DisplayName,
            request.From ?? _options.From);
        email.Subject = request.Subject;
        builder.HtmlBody = request.Body;

        if (request.AttachmentData != null)
            foreach (var attachment in request.AttachmentData)
                builder.Attachments.Add(attachment.Key, attachment.Value);

        email.Body = builder.ToMessageBody();

        return email;
    }
}
