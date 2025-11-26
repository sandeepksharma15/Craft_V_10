# Craft.Infrastructure.Emails - Quick Reference Guide

> **Version:** 2.0+ | **Target Framework:** .NET 10

## ?? Table of Contents

1. [Quick Start](#-quick-start)
2. [Configuration](#-configuration)
3. [Email Providers](#-email-providers)
4. [Sending Emails](#-sending-emails)
5. [Email Templates](#-email-templates)
6. [Email Queue & Background Processing](#-email-queue--background-processing)
7. [Template Caching](#-template-caching)
8. [Email Preview & Testing](#-email-preview--testing)
9. [Retry Mechanism](#-retry-mechanism)
10. [Features](#-features)
11. [Migration from v1.x](#-migration-from-v1x)
12. [Advanced Scenarios](#-advanced-scenarios)
13. [Testing](#-testing)
14. [Troubleshooting](#-troubleshooting)

---

## ?? Quick Start

### Basic Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add email services with automatic validation
builder.Services.AddEmailServices(builder.Configuration);

var app = builder.Build();
app.Run();
```

### Configuration (appsettings.json)

```json
{
  "EmailOptions": {
    "Provider": "smtp",
    "From": "noreply@example.com",
    "DisplayName": "My Application",
    "EnableQueue": true,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 30,
    "EnableTracking": true,
    "EnableTemplateCache": true,
    "TemplateCacheDurationMinutes": 60,
    "TemplatesPath": "Email Templates",
    "Smtp": {
      "Host": "smtp.example.com",
      "Port": 587,
      "UserName": "your-username",
      "Password": "your-password",
      "EnableSsl": true,
      "TimeoutSeconds": 30
    }
  }
}
```

### Send Your First Email

```csharp
public class WelcomeService
{
    private readonly IMailService _mailService;

    public WelcomeService(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task SendWelcomeEmail(string email, string name)
    {
        var request = new MailRequest(
            to: new List<string> { email },
            subject: "Welcome to Our App!",
            body: $"<h1>Welcome {name}!</h1><p>Thanks for joining us.</p>");

        var result = await _mailService.SendAsync(request);

        if (result.IsSuccess)
            Console.WriteLine("Email sent successfully!");
    }
}
```

---

## ?? Configuration

### EmailOptions Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Provider` | string | "smtp" | Email provider to use ("smtp", "mock") |
| `From` | string | Required | Default sender email address |
| `DisplayName` | string | null | Default sender display name |
| `EnableQueue` | bool | true | Enable background email queue processing |
| `MaxRetryAttempts` | int | 3 | Maximum retry attempts for failed emails (0-10) |
| `RetryDelaySeconds` | int | 30 | Delay between retry attempts (1-3600) |
| `EnableTracking` | bool | true | Enable email tracking (future feature) |
| `EnableTemplateCache` | bool | true | Cache compiled templates for performance |
| `TemplateCacheDurationMinutes` | int | 60 | Template cache duration (1-1440) |
| `TemplatesPath` | string | "Email Templates" | Path to email templates folder |
| `Smtp` | SmtpSettings | null | SMTP-specific configuration |

### SmtpSettings Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Host` | string | Required | SMTP server address |
| `Port` | int | 587 | SMTP port number (1-65535) |
| `UserName` | string | null | SMTP authentication username |
| `Password` | string | null | SMTP authentication password |
| `EnableSsl` | bool | true | Use SSL/TLS encryption |
| `TimeoutSeconds` | int | 30 | SMTP operation timeout (1-300) |

### Configuration Methods

#### Method 1: From appsettings.json (Recommended)

```csharp
builder.Services.AddEmailServices(builder.Configuration);
```

#### Method 2: From Configuration Section

```csharp
builder.Services.AddEmailServices(
    builder.Configuration.GetSection("EmailOptions"));
```

#### Method 3: Programmatic Configuration

```csharp
builder.Services.AddEmailServices(options =>
{
    options.Provider = "smtp";
    options.From = "noreply@example.com";
    options.DisplayName = "My App";
    options.EnableQueue = true;
    options.Smtp = new SmtpSettings
    {
        Host = "smtp.gmail.com",
        Port = 587,
        UserName = "your-email@gmail.com",
        Password = "your-app-password",
        EnableSsl = true
    };
});
```

---

## ?? Email Providers

### Built-in Providers

#### 1. SMTP Provider (Production)

```json
{
  "EmailOptions": {
    "Provider": "smtp",
    "Smtp": {
      "Host": "smtp.example.com",
      "Port": 587,
      "UserName": "user",
      "Password": "pass",
      "EnableSsl": true
    }
  }
}
```

#### 2. Mock Provider (Development/Testing)

```json
{
  "EmailOptions": {
    "Provider": "mock",
    "From": "test@example.com"
  }
}
```

The mock provider logs emails instead of sending them:

```
[Information] MOCK EMAIL: To=user@example.com, Subject=Welcome, HasBody=True
[Debug] Email Body Preview: <h1>Welcome!</h1>...
```

### Custom Email Providers

Create a custom provider by implementing `IEmailProvider`:

```csharp
public class SendGridEmailProvider : IEmailProvider
{
    private readonly ILogger<SendGridEmailProvider> _logger;
    private readonly string _apiKey;

    public SendGridEmailProvider(
        IConfiguration configuration,
        ILogger<SendGridEmailProvider> logger)
    {
        _apiKey = configuration["SendGrid:ApiKey"] ?? "";
        _logger = logger;
    }

    public string Name => "sendgrid";

    public bool IsConfigured() => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<EmailResult> SendAsync(
        MailRequest request,
        CancellationToken cancellationToken = default)
    {
        // SendGrid implementation
        try
        {
            // Use SendGrid SDK to send email
            return EmailResult.Success("message-id");
        }
        catch (Exception ex)
        {
            return EmailResult.Failure(ex.Message, ex);
        }
    }
}
```

Register the custom provider:

```csharp
builder.Services
    .AddEmailServices(builder.Configuration)
    .AddEmailProvider<SendGridEmailProvider>();
```

### Provider Factory

Access providers dynamically:

```csharp
public class EmailController
{
    private readonly IEmailProviderFactory _providerFactory;

    public EmailController(IEmailProviderFactory providerFactory)
    {
        _providerFactory = providerFactory;
    }

    public async Task SendWithSpecificProvider(string providerName)
    {
        var provider = _providerFactory.GetProvider(providerName);
        var request = new MailRequest(
            to: new List<string> { "user@example.com" },
            subject: "Test",
            body: "Hello");

        var result = await provider.SendAsync(request);
    }

    public void ListProviders()
    {
        var providers = _providerFactory.GetAllProviders();
        foreach (var provider in providers)
        {
            Console.WriteLine($"Provider: {provider.Name}, Configured: {provider.IsConfigured()}");
        }
    }
}
```

---

## ?? Sending Emails

### Immediate Send

```csharp
var result = await _mailService.SendAsync(new MailRequest(
    to: new List<string> { "user@example.com" },
    subject: "Hello",
    body: "<h1>Welcome!</h1>"));

if (result.IsSuccess)
    Console.WriteLine($"Email sent at {result.SentAt}");
else
    Console.WriteLine($"Failed: {result.ErrorMessage}");
```

### Queue for Background Sending

```csharp
var emailId = await _mailService.QueueAsync(
    request: new MailRequest(
        to: new List<string> { "user@example.com" },
        subject: "Newsletter",
        body: "<p>Our latest news...</p>"),
    priority: EmailPriority.Normal);

Console.WriteLine($"Email queued with ID: {emailId}");
```

### Schedule for Future Delivery

```csharp
var scheduledFor = DateTimeOffset.UtcNow.AddHours(24);

var emailId = await _mailService.QueueAsync(
    request: new MailRequest(
        to: new List<string> { "user@example.com" },
        subject: "Reminder",
        body: "<p>Don't forget!</p>"),
    scheduledFor: scheduledFor);
```

### Email with Attachments

```csharp
var attachments = new Dictionary<string, byte[]>
{
    ["invoice.pdf"] = File.ReadAllBytes("path/to/invoice.pdf"),
    ["logo.png"] = File.ReadAllBytes("path/to/logo.png")
};

var request = new MailRequest(
    to: new List<string> { "user@example.com" },
    subject: "Your Invoice",
    body: "<p>Please find your invoice attached.</p>",
    attachmentData: attachments);

await _mailService.SendAsync(request);
```

### Email with CC and BCC

```csharp
var request = new MailRequest(
    to: new List<string> { "primary@example.com" },
    subject: "Team Update",
    body: "<p>Team update...</p>",
    cc: new List<string> { "manager@example.com" },
    bcc: new List<string> { "audit@example.com" });

await _mailService.SendAsync(request);
```

### Email Priority

```csharp
// High priority - sent first
await _mailService.QueueAsync(request, priority: EmailPriority.High);

// Normal priority (default)
await _mailService.QueueAsync(request, priority: EmailPriority.Normal);

// Low priority - sent last
await _mailService.QueueAsync(request, priority: EmailPriority.Low);
```

---

## ?? Email Templates

### Template Structure

Create templates in the `Email Templates` folder (configurable):

```
YourProject/
  Email Templates/
    Welcome.cshtml
    PasswordReset.cshtml
    OrderConfirmation.cshtml
```

### Basic Template (Welcome.cshtml)

```html
@model WelcomeEmailModel

<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        .header { background-color: #007bff; color: white; padding: 20px; }
        .content { padding: 20px; }
    </style>
</head>
<body>
    <div class="header">
        <h1>Welcome @Model.Name!</h1>
    </div>
    <div class="content">
        <p>Thank you for joining @Model.AppName.</p>
        <p>Your account is now active and ready to use.</p>
        <a href="@Model.LoginUrl">Login Now</a>
    </div>
</body>
</html>
```

### Template Model

```csharp
public class WelcomeEmailModel
{
    public string Name { get; set; } = "";
    public string AppName { get; set; } = "";
    public string LoginUrl { get; set; } = "";
}
```

### Send Email with Template

```csharp
var model = new WelcomeEmailModel
{
    Name = "John Doe",
    AppName = "My Application",
    LoginUrl = "https://example.com/login"
};

var result = await _mailService.SendTemplateAsync(
    to: new List<string> { "john@example.com" },
    subject: "Welcome to My Application",
    templateName: "Welcome",
    model: model);
```

### Queue Email with Template

```csharp
var emailId = await _mailService.QueueTemplateAsync(
    to: new List<string> { "user@example.com" },
    subject: "Welcome!",
    templateName: "Welcome",
    model: new WelcomeEmailModel { Name = "John" },
    priority: EmailPriority.High);
```

### Advanced Template Features

#### Conditionals

```html
@model OrderConfirmationModel

<h1>Order Confirmation</h1>
<p>Order #@Model.OrderNumber</p>

@if (Model.HasDiscount)
{
    <p style="color: green;">Discount Applied: @Model.DiscountPercent%</p>
}

@if (Model.Items.Count > 0)
{
    <ul>
    @foreach (var item in Model.Items)
    {
        <li>@item.Name - $@item.Price</li>
    }
    </ul>
}
```

#### Layouts and Partials

While RazorEngineCore doesn't support layouts directly, you can use helper methods:

```csharp
public class EmailLayoutHelper
{
    public static string GetHeader(string title)
    {
        return $@"
        <div class='header'>
            <h1>{title}</h1>
        </div>";
    }

    public static string GetFooter()
    {
        return @"
        <div class='footer'>
            <p>&copy; 2024 My Company</p>
        </div>";
    }
}
```

---

## ?? Email Queue & Background Processing

### How It Works

1. **Queue Email**: Email is added to the queue with status `Pending`
2. **Background Processor**: `EmailQueueProcessor` runs every 5 seconds
3. **Dequeue**: Picks the next pending email (respects priority and schedule)
4. **Send**: Attempts to send the email
5. **Retry**: On failure, retries based on `MaxRetryAttempts`
6. **Update Status**: Marks as `Sent`, `Failed`, or `FailedPermanently`

### Email Status Lifecycle

```
Pending ? Sending ? Sent (Success)
                  ? Failed ? Sending (Retry)
                  ? FailedPermanently (Max retries exceeded)

Scheduled ? Pending (When scheduled time arrives)
```

### Queue Email

```csharp
var emailId = await _mailService.QueueAsync(request);
```

### Check Email Status

```csharp
var queuedEmail = await _mailService.GetQueuedEmailAsync(emailId);

if (queuedEmail != null)
{
    Console.WriteLine($"Status: {queuedEmail.Status}");
    Console.WriteLine($"Attempts: {queuedEmail.Attempts}");
    Console.WriteLine($"Queued At: {queuedEmail.QueuedAt}");
    
    if (queuedEmail.Status == EmailStatus.Sent)
        Console.WriteLine($"Sent At: {queuedEmail.SentAt}");
    
    if (queuedEmail.Status == EmailStatus.Failed || 
        queuedEmail.Status == EmailStatus.FailedPermanently)
        Console.WriteLine($"Error: {queuedEmail.ErrorMessage}");
}
```

### Custom Email Queue

Implement `IEmailQueue` for database-backed queues:

```csharp
public class DatabaseEmailQueue : IEmailQueue
{
    private readonly AppDbContext _context;

    public DatabaseEmailQueue(AppDbContext context)
    {
        _context = context;
    }

    public async Task EnqueueAsync(QueuedEmail email, CancellationToken cancellationToken = default)
    {
        // Save to database
        _context.QueuedEmails.Add(email);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<QueuedEmail?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        // Get next pending email from database
        return await _context.QueuedEmails
            .Where(e => e.Status == EmailStatus.Pending)
            .OrderByDescending(e => e.Priority)
            .ThenBy(e => e.QueuedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // Implement other methods...
}
```

Register the custom queue:

```csharp
builder.Services
    .AddEmailServices(builder.Configuration)
    .AddEmailQueue<DatabaseEmailQueue>();
```

### Disable Queue Processing

```json
{
  "EmailOptions": {
    "EnableQueue": false
  }
}
```

When disabled, `QueueAsync` still works but emails won't be processed automatically. You'll need to manually process the queue or use `SendAsync` instead.

---

## ?? Template Caching

### Why Cache Templates?

- **Performance**: Compiling Razor templates is expensive
- **Scalability**: Reduces CPU usage in high-volume scenarios
- **Consistency**: Same template version served during cache lifetime

### Enable/Disable Caching

```json
{
  "EmailOptions": {
    "EnableTemplateCache": true,
    "TemplateCacheDurationMinutes": 60
  }
}
```

### Cache Behavior

- Templates are compiled on first use
- Cached for configured duration (default: 60 minutes)
- Cache key: `EmailTemplate_{templateName}`
- Automatic expiration after cache duration

### Clear Cache Programmatically

```csharp
public class TemplateManagementService
{
    private readonly IEmailTemplateRenderer _templateRenderer;

    public TemplateManagementService(IEmailTemplateRenderer templateRenderer)
    {
        _templateRenderer = templateRenderer;
    }

    public void RefreshTemplates()
    {
        _templateRenderer.ClearCache();
        Console.WriteLine("Template cache cleared");
    }
}
```

### Cache in Production

**Recommended Settings:**

```json
{
  "EmailOptions": {
    "EnableTemplateCache": true,
    "TemplateCacheDurationMinutes": 1440
  }
}
```

**Development Settings:**

```json
{
  "EmailOptions": {
    "EnableTemplateCache": false
  }
}
```

---

## ?? Email Preview & Testing

### Preview Template Without Sending

```csharp
public class EmailPreviewController : ControllerBase
{
    private readonly IMailService _mailService;

    [HttpGet("preview/{templateName}")]
    public async Task<IActionResult> PreviewTemplate(string templateName)
    {
        var model = new WelcomeEmailModel
        {
            Name = "Preview User",
            AppName = "My App",
            LoginUrl = "https://example.com/login"
        };

        var html = await _mailService.PreviewTemplateAsync(templateName, model);
        
        return Content(html, "text/html");
    }
}
```

### Test Email Sending

```csharp
[HttpPost("test-email")]
public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
{
    var mailRequest = new MailRequest(
        to: new List<string> { request.Email },
        subject: "Test Email",
        body: "<h1>This is a test email</h1><p>If you received this, email is working!</p>");

    var result = await _mailService.SendAsync(mailRequest);

    if (result.IsSuccess)
        return Ok(new { message = "Test email sent successfully" });
    else
        return BadRequest(new { error = result.ErrorMessage });
}
```

### Check Template Existence

```csharp
public class TemplateValidator
{
    private readonly IEmailTemplateRenderer _templateRenderer;

    public bool ValidateTemplate(string templateName)
    {
        return _templateRenderer.TemplateExists(templateName);
    }

    public List<string> ValidateAllTemplates(string[] templateNames)
    {
        return templateNames
            .Where(name => !_templateRenderer.TemplateExists(name))
            .ToList();
    }
}
```

### Mock Provider for Development

Use the mock provider to test email logic without sending real emails:

```json
{
  "EmailOptions": {
    "Provider": "mock",
    "From": "test@example.com"
  }
}
```

All emails will be logged instead of sent:

```
[Information] MOCK EMAIL: To=user@example.com, Subject=Welcome, HasBody=True
[Debug] Email Body Preview: <h1>Welcome John!</h1><p>Thank you...
```

---

## ?? Retry Mechanism

### How It Works

1. **First Attempt**: Email is sent immediately
2. **Failure Detection**: Provider returns `EmailResult` with `IsSuccess = false`
3. **Retry Logic**: 
   - If attempts < `MaxRetryAttempts`, wait `RetryDelaySeconds * attempt`
   - Retry sending
4. **Permanent Failure**: After max attempts, mark as `FailedPermanently`

### Configuration

```json
{
  "EmailOptions": {
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 30
  }
}
```

### Retry Schedule Example

With `MaxRetryAttempts = 3` and `RetryDelaySeconds = 30`:

- **Attempt 1**: Immediate (0s delay)
- **Attempt 2**: After 30 seconds (30s * 1)
- **Attempt 3**: After 60 seconds (30s * 2)
- **Attempt 4**: After 90 seconds (30s * 3)

Total: 4 attempts over ~3 minutes

### Exponential Backoff

For exponential backoff, modify `RetryDelaySeconds` calculation:

```csharp
// In MailService.SendWithRetryAsync
var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt) * _options.RetryDelaySeconds);
```

This gives: 30s, 60s, 120s, 240s...

### Disable Retries

```json
{
  "EmailOptions": {
    "MaxRetryAttempts": 0
  }
}
```

### Retry Logging

```
[Warning] Email send failed on attempt 1 of 4: SMTP connection failed
[Information] Retrying email send in 30 seconds
[Warning] Email send failed on attempt 2 of 4: SMTP connection failed
[Information] Retrying email send in 60 seconds
[Information] Email sent successfully on attempt 3 of 4
```

---

## ? Features

### ? Implemented Features

- ? **Multiple Email Providers**: SMTP, Mock, extensible for SendGrid/SES/etc.
- ? **Provider Factory Pattern**: Dynamic provider selection
- ? **Background Email Queue**: Async email processing with `EmailQueueProcessor`
- ? **Retry Mechanism**: Automatic retry with configurable attempts and delays
- ? **Email Tracking**: Queue status tracking (Pending, Sending, Sent, Failed)
- ? **Template System**: Razor-based templates with model binding
- ? **Template Caching**: High-performance template compilation caching
- ? **Email Priorities**: High, Normal, Low priority queuing
- ? **Scheduled Emails**: Queue emails for future delivery
- ? **Email Preview**: Preview templates without sending
- ? **Configuration Validation**: Automatic validation at startup
- ? **Comprehensive Logging**: Detailed operation logging
- ? **Attachments**: Support for email attachments
- ? **CC/BCC**: Carbon copy and blind carbon copy support
- ? **Custom Headers**: Add custom email headers
- ? **Extension Methods**: Clean DI registration with `AddEmailServices()`
- ? **Mock Provider**: Development/testing without real email sending
- ? **In-Memory Queue**: Default queue implementation (replaceable)

### ?? Future Features

- ?? **Email Open Tracking**: Track when emails are opened
- ?? **Click Tracking**: Track link clicks in emails
- ?? **Analytics Dashboard**: Email metrics and reporting
- ?? **Localization**: Multi-language template support
- ?? **Template Versioning**: Version control for templates
- ?? **Database Queue**: Persistent queue with EF Core
- ?? **Bulk Sending**: Optimized bulk email operations
- ?? **Email Signing**: DKIM/SPF support
- ?? **Inbound Emails**: Handle incoming emails
- ?? **SMS Provider**: Extend to SMS notifications

---

## ?? Migration from v1.x

### Breaking Changes

The following classes are now **obsolete** and will be removed in a future version:

- `EmailTemplateService` ? Use `IEmailTemplateRenderer`
- `IEmailTemplateService` ? Use `IEmailTemplateRenderer`
- `SmtpMailService` ? Use `SmtpEmailProvider` with `IMailService`
- `MailSettings` ? Use `EmailOptions` with `SmtpSettings`

### Migration Steps

#### Step 1: Update Configuration

**Old (v1.x):**

```json
{
  "MailSettings": {
    "From": "noreply@example.com",
    "DisplayName": "My App",
    "Host": "smtp.example.com",
    "Port": 587,
    "UserName": "user",
    "Password": "pass",
    "EnableSSL": true
  }
}
```

**New (v2.0+):**

```json
{
  "EmailOptions": {
    "Provider": "smtp",
    "From": "noreply@example.com",
    "DisplayName": "My App",
    "EnableQueue": true,
    "Smtp": {
      "Host": "smtp.example.com",
      "Port": 587,
      "UserName": "user",
      "Password": "pass",
      "EnableSsl": true
    }
  }
}
```

#### Step 2: Update DI Registration

**Old (v1.x):**

```csharp
builder.Services.Configure<MailSettings>(
    builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<IMailService, SmtpMailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
```

**New (v2.0+):**

```csharp
builder.Services.AddEmailServices(builder.Configuration);
```

#### Step 3: Update Service Injection

**Old (v1.x):**

```csharp
public class UserService
{
    private readonly IMailService _mailService;
    private readonly IEmailTemplateService _templateService;

    public UserService(IMailService mailService, IEmailTemplateService templateService)
    {
        _mailService = mailService;
        _templateService = templateService;
    }

    public async Task SendWelcomeEmail(string email)
    {
        var template = _templateService.GenerateEmailTemplate(
            "Welcome",
            new { Name = "User" });

        var request = new MailRequest(
            to: new List<string> { email },
            subject: "Welcome",
            body: template);

        await _mailService.SendAsync(request);
    }
}
```

**New (v2.0+):**

```csharp
public class UserService
{
    private readonly IMailService _mailService;

    public UserService(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task SendWelcomeEmail(string email)
    {
        await _mailService.SendTemplateAsync(
            to: new List<string> { email },
            subject: "Welcome",
            templateName: "Welcome",
            model: new { Name = "User" });
    }
}
```

#### Step 4: Update Method Signatures

**Old `IMailService.SendAsync` returned `Task`:**

```csharp
await _mailService.SendAsync(request);
```

**New `IMailService.SendAsync` returns `Task<EmailResult>`:**

```csharp
var result = await _mailService.SendAsync(request);
if (result.IsSuccess)
    Console.WriteLine("Email sent!");
```

### Compatibility Mode

The old classes are still available but marked as `[Obsolete]`. You'll see compiler warnings:

```
Warning CS0618: 'EmailTemplateService' is obsolete: 'This class is deprecated. Use IEmailTemplateRenderer and MailService instead.'
```

**Recommended**: Migrate to v2.0 API as soon as possible.

---

## ?? Advanced Scenarios

### Multiple Email Providers

Use different providers for different scenarios:

```csharp
public class NotificationService
{
    private readonly IEmailProviderFactory _providerFactory;

    public async Task SendTransactionalEmail(string to)
    {
        var provider = _providerFactory.GetProvider("smtp");
        var result = await provider.SendAsync(new MailRequest(
            to: new List<string> { to },
            subject: "Transaction Confirmed",
            body: "..."));
    }

    public async Task SendMarketingEmail(string to)
    {
        var provider = _providerFactory.GetProvider("sendgrid");
        var result = await provider.SendAsync(new MailRequest(
            to: new List<string> { to },
            subject: "Special Offer",
            body: "..."));
    }
}
```

### Custom Template Renderer

Replace the default Razor renderer with a custom implementation:

```csharp
public class CustomTemplateRenderer : IEmailTemplateRenderer
{
    public async Task<string> RenderAsync<T>(string templateName, T model, CancellationToken cancellationToken = default)
    {
        // Custom rendering logic (e.g., Scriban, Liquid)
        return await Task.FromResult("<html>...</html>");
    }

    // Implement other methods...
}
```

Register it:

```csharp
builder.Services
    .AddEmailServices(builder.Configuration)
    .AddEmailTemplateRenderer<CustomTemplateRenderer>();
```

### Environment-Specific Configuration

```csharp
var emailSection = builder.Configuration.GetSection("EmailOptions");

if (builder.Environment.IsDevelopment())
{
    emailSection["Provider"] = "mock";
}

builder.Services.AddEmailServices(emailSection);
```

### Dynamic From Address

```csharp
public class MultiTenantEmailService
{
    private readonly IMailService _mailService;
    private readonly ITenant _currentTenant;

    public async Task SendTenantEmail(string to, string subject, string body)
    {
        var request = new MailRequest(
            to: new List<string> { to },
            subject: subject,
            body: body,
            from: _currentTenant.Email,  // Dynamic from address
            displayName: _currentTenant.Name);

        await _mailService.SendAsync(request);
    }
}
```

### Batch Email Sending

```csharp
public class BatchEmailService
{
    private readonly IMailService _mailService;

    public async Task SendNewsletterToAll(List<string> recipients, string subject, string templateName, object model)
    {
        var tasks = recipients.Select(email =>
            _mailService.QueueTemplateAsync(
                to: new List<string> { email },
                subject: subject,
                templateName: templateName,
                model: model,
                priority: EmailPriority.Low));

        var emailIds = await Task.WhenAll(tasks);
        
        Console.WriteLine($"Queued {emailIds.Length} emails");
    }
}
```

### Email with Inline Images

```csharp
var imageBytes = File.ReadAllBytes("logo.png");
var base64Image = Convert.ToBase64String(imageBytes);

var body = $@"
<html>
<body>
    <img src='data:image/png;base64,{base64Image}' />
    <p>Email content...</p>
</body>
</html>";

await _mailService.SendAsync(new MailRequest(
    to: new List<string> { "user@example.com" },
    subject: "With Image",
    body: body));
```

---

## ?? Testing

### Unit Tests

Create comprehensive unit tests for email services:

```csharp
public class MailServiceTests
{
    private readonly Mock<IEmailProviderFactory> _providerFactoryMock;
    private readonly Mock<IEmailTemplateRenderer> _templateRendererMock;
    private readonly Mock<IEmailQueue> _emailQueueMock;
    private readonly Mock<IEmailProvider> _emailProviderMock;
    private readonly Mock<ILogger<MailService>> _loggerMock;
    private readonly EmailOptions _options;
    private readonly MailService _mailService;

    public MailServiceTests()
    {
        _providerFactoryMock = new Mock<IEmailProviderFactory>();
        _templateRendererMock = new Mock<IEmailTemplateRenderer>();
        _emailQueueMock = new Mock<IEmailQueue>();
        _emailProviderMock = new Mock<IEmailProvider>();
        _loggerMock = new Mock<ILogger<MailService>>();
        
        _options = new EmailOptions
        {
            Provider = "smtp",
            From = "test@example.com",
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 1
        };

        _providerFactoryMock
            .Setup(x => x.GetDefaultProvider())
            .Returns(_emailProviderMock.Object);

        var optionsMock = new Mock<IOptions<EmailOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);

        _mailService = new MailService(
            _providerFactoryMock.Object,
            _templateRendererMock.Object,
            _emailQueueMock.Object,
            optionsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SendAsync_Success_ReturnsSuccessResult()
    {
        // Arrange
        var request = new MailRequest(
            to: new List<string> { "test@example.com" },
            subject: "Test",
            body: "Body");

        _emailProviderMock
            .Setup(x => x.SendAsync(It.IsAny<MailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("message-id"));

        // Act
        var result = await _mailService.SendAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("message-id", result.MessageId);
    }

    [Fact]
    public async Task SendAsync_Failure_RetriesAndReturnsFailure()
    {
        // Arrange
        var request = new MailRequest(
            to: new List<string> { "test@example.com" },
            subject: "Test",
            body: "Body");

        _emailProviderMock
            .Setup(x => x.SendAsync(It.IsAny<MailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Failure("Connection failed"));

        // Act
        var result = await _mailService.SendAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        _emailProviderMock.Verify(
            x => x.SendAsync(It.IsAny<MailRequest>(), It.IsAny<CancellationToken>()),
            Times.Exactly(4)); // 1 initial + 3 retries
    }

    [Fact]
    public async Task QueueAsync_AddsToQueue_ReturnsEmailId()
    {
        // Arrange
        var request = new MailRequest(
            to: new List<string> { "test@example.com" },
            subject: "Test",
            body: "Body");

        // Act
        var emailId = await _mailService.QueueAsync(request);

        // Assert
        Assert.NotNull(emailId);
        _emailQueueMock.Verify(
            x => x.EnqueueAsync(It.IsAny<QueuedEmail>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendTemplateAsync_RendersTemplate_SendsEmail()
    {
        // Arrange
        var model = new { Name = "Test" };
        var renderedHtml = "<html>Test</html>";

        _templateRendererMock
            .Setup(x => x.RenderAsync("Welcome", model, It.IsAny<CancellationToken>()))
            .ReturnsAsync(renderedHtml);

        _emailProviderMock
            .Setup(x => x.SendAsync(It.IsAny<MailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success());

        // Act
        var result = await _mailService.SendTemplateAsync(
            to: new List<string> { "test@example.com" },
            subject: "Welcome",
            templateName: "Welcome",
            model: model);

        // Assert
        Assert.True(result.IsSuccess);
        _templateRendererMock.Verify(
            x => x.RenderAsync("Welcome", model, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

### Integration Tests

Test with real SMTP server or test containers:

```csharp
public class EmailIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EmailIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SendEmail_WithMockProvider_LogsEmail()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/email/send", new
        {
            to = new[] { "test@example.com" },
            subject = "Test",
            body = "Test body"
        });

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

### Test Email Template Rendering

```csharp
public class TemplateRenderingTests
{
    [Fact]
    public async Task RenderAsync_WithValidTemplate_ReturnsHtml()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<RazorEmailTemplateRenderer>>();
        var options = Options.Create(new EmailOptions
        {
            TemplatesPath = "Email Templates",
            EnableTemplateCache = true
        });

        var renderer = new RazorEmailTemplateRenderer(options, cache, logger.Object);

        var model = new { Name = "Test User" };

        // Act
        var html = await renderer.RenderAsync("Welcome", model);

        // Assert
        Assert.Contains("Test User", html);
        Assert.StartsWith("<!DOCTYPE html>", html.Trim());
    }
}
```

### Mock Provider for Testing

The built-in `MockEmailProvider` is perfect for testing:

```csharp
// appsettings.Testing.json
{
  "EmailOptions": {
    "Provider": "mock",
    "From": "test@example.com"
  }
}
```

All emails will be logged but not sent.

---

## ?? Troubleshooting

### Issue: "Email provider 'smtp' is not registered"

**Cause**: Email services not properly registered

**Solution**:
```csharp
builder.Services.AddEmailServices(builder.Configuration);
```

---

### Issue: "Template file not found"

**Cause**: Template file doesn't exist or wrong path

**Solution**:
1. Check template exists: `Email Templates/Welcome.cshtml`
2. Verify `TemplatesPath` in configuration
3. Check file is copied to output directory (Build Action: Content, Copy Always)

---

### Issue: "SMTP authentication failed"

**Cause**: Wrong SMTP credentials or SSL settings

**Solution**:
1. Verify SMTP credentials
2. Check `EnableSsl` setting
3. Try different ports (25, 587, 465)
4. Enable "Less secure apps" for Gmail
5. Use app-specific password for Gmail

---

### Issue: "Email queue not processing"

**Cause**: Queue processing disabled or error in background service

**Solution**:
1. Check `EnableQueue` is `true`
2. Check logs for `EmailQueueProcessor` errors
3. Verify `EmailQueueProcessor` is registered as hosted service

---

### Issue: "Template compilation error"

**Cause**: Razor syntax error in template

**Solution**:
1. Check template syntax
2. Ensure model properties match template
3. Test template with preview endpoint
4. Check logs for compilation errors

---

### Issue: "Emails sent but not received"

**Cause**: Spam filter, wrong email address, or SMTP configuration

**Solution**:
1. Check spam/junk folder
2. Verify recipient email address
3. Check SMTP server logs
4. Test with mock provider first
5. Verify SPF/DKIM records

---

### Issue: "High memory usage with template caching"

**Cause**: Too many templates cached or cache duration too long

**Solution**:
```json
{
  "EmailOptions": {
    "EnableTemplateCache": true,
    "TemplateCacheDurationMinutes": 30  // Reduce from 60
  }
}
```

Or manually clear cache:
```csharp
_templateRenderer.ClearCache();
```

---

### Issue: "Slow email sending"

**Cause**: Synchronous sending or SMTP timeout

**Solution**:
1. Use queue for non-critical emails:
   ```csharp
   await _mailService.QueueAsync(request);
   ```
2. Increase SMTP timeout:
   ```json
   {
     "EmailOptions": {
       "Smtp": {
         "TimeoutSeconds": 60
       }
     }
   }
   ```

---

### Common Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| `Provider not configured` | Missing SMTP settings | Add SMTP configuration |
| `Validation failed for EmailOptions` | Invalid configuration | Check all required fields |
| `Template not found` | Missing template file | Add template file |
| `Connection refused` | Wrong SMTP host/port | Verify SMTP settings |
| `Authentication failed` | Wrong credentials | Check username/password |
| `Unable to connect to remote server` | Firewall/network issue | Check network connectivity |

---

## ?? Additional Resources

### Official Documentation
- [MailKit Documentation](https://github.com/jstedfast/MailKit)
- [RazorEngineCore Documentation](https://github.com/adoconnection/RazorEngineCore)
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)

### Related Craft Modules
- **Craft.Infrastructure** - Infrastructure services
- **Craft.Core** - Core abstractions
- **Craft.Logging** - Logging with Serilog

### Sample Templates
- Create templates in: `YourProject/Email Templates/`
- Use `.cshtml` extension
- Follow Razor syntax
- Use strongly-typed models

---

## ?? What's New in v2.0

### ?? New Features

1. **Email Provider Abstraction**
   - `IEmailProvider` interface
   - Multiple provider support (SMTP, Mock, extensible)
   - `IEmailProviderFactory` for dynamic provider selection

2. **Background Queue Processing**
   - `IEmailQueue` interface
   - `InMemoryEmailQueue` implementation
   - `EmailQueueProcessor` background service
   - Email priorities (High, Normal, Low)
   - Scheduled emails

3. **Retry Mechanism**
   - Configurable retry attempts
   - Configurable retry delays
   - Exponential backoff support
   - Detailed retry logging

4. **Email Tracking**
   - Queue status tracking
   - Email history
   - `QueuedEmail` model with full lifecycle

5. **Template Improvements**
   - `IEmailTemplateRenderer` interface
   - Template caching with `IMemoryCache`
   - Template existence checking
   - Template preview support

6. **Configuration Validation**
   - `IValidatableObject` implementation
   - Automatic validation at startup
   - Detailed error messages

7. **Extension Methods**
   - `AddEmailServices()` for DI registration
   - `AddEmailProvider<T>()` for custom providers
   - `AddEmailQueue<T>()` for custom queues
   - `AddEmailTemplateRenderer<T>()` for custom renderers

8. **Mock Provider**
   - Built-in mock provider for testing
   - Logs emails instead of sending
   - Perfect for development

### ?? Improvements

- ? Better error handling with `EmailResult`
- ? Comprehensive logging throughout
- ? Async/await best practices
- ? Cancellation token support
- ? Memory-efficient template caching
- ? Configurable timeouts
- ? SSL/TLS configuration
- ? UTF-8 encoding by default

### ?? Breaking Changes

- `IMailService.SendAsync` now returns `Task<EmailResult>` (was `Task`)
- Configuration moved from `MailSettings` to `EmailOptions`
- Template service split into `IEmailTemplateRenderer`

### ??? Deprecated

- `EmailTemplateService` (use `IEmailTemplateRenderer`)
- `IEmailTemplateService` (use `IEmailTemplateRenderer`)
- `SmtpMailService` (use `MailService` with `SmtpEmailProvider`)
- `MailSettings` (use `EmailOptions`)

---

## ?? Quick Tips

- ? Use mock provider in development
- ? Enable queue for non-critical emails
- ? Enable template caching in production
- ? Use retry mechanism for reliability
- ? Check template existence before sending
- ? Preview templates in development
- ? Use strongly-typed models for templates
- ? Monitor queue status in production
- ?? Don't use plain text passwords in appsettings.json (use secrets)
- ?? Don't disable SSL in production
- ?? Don't set retry attempts too high

---

**Last Updated:** January 2025  
**Version:** 2.0  
**Target Framework:** .NET 10  
**Status:** ? Production Ready

---

**Need help?** Check the [Troubleshooting](#-troubleshooting) section or enable debug logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Craft.Infrastructure.Emails": "Debug"
    }
  }
}
```
