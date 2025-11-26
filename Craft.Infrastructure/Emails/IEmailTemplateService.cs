namespace Craft.Infrastructure.Emails;

public interface IEmailTemplateService
{
    string GenerateEmailTemplate<T>(string templateName, T mailTemplateModel);
}
