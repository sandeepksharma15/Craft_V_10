namespace Craft.Infrastructure.Emails;

public interface IMailService
{
    Task SendAsync(MailRequest request, CancellationToken cancellationToken = default);
}
