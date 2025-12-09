# Email Services Quick Start Guide

## 1. Installation

The email services are part of `Craft.Infrastructure` package. No additional installation required.

## 2. Configuration

Add to your `appsettings.json`:

```json
{
  "EmailOptions": {
    "Provider": "smtp",
    "From": "noreply@yourapp.com",
    "DisplayName": "Your App Name",
    "EnableQueue": true,
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "UserName": "your-email@gmail.com",
      "Password": "your-app-password",
      "EnableSsl": true
    }
  }
}
```

### For Development (Mock Provider)

```json
{
  "EmailOptions": {
    "Provider": "mock",
    "From": "dev@yourapp.com"
  }
}
```

## 3. Register Services

In your `Program.cs`:

```csharp
// Add email services
builder.Services.AddEmailServices(builder.Configuration);

var app = builder.Build();
app.Run();
```

## 4. Send Your First Email

### Basic Email

```csharp
public class UserService
{
    private readonly IMailService _mailService;

    public UserService(IMailService mailService)
    {
        _mailService = mailService;
    }

    public async Task SendWelcomeEmail(string userEmail, string userName)
    {
        var request = new MailRequest(
            to: new List<string> { userEmail },
            subject: "Welcome!",
            body: $"<h1>Welcome {userName}!</h1>");

        var result = await _mailService.SendAsync(request);
        
        if (!result.IsSuccess)
            _logger.LogError("Failed to send email: {Error}", result.ErrorMessage);
    }
}
```

### Email with Template

Create `Email Templates/Welcome.cshtml`:

```html
@model WelcomeEmailModel

<!DOCTYPE html>
<html>
<body>
    <h1>Welcome @Model.Name!</h1>
    <p>Thanks for joining @Model.AppName.</p>
    <a href="@Model.LoginUrl">Login Now</a>
</body>
</html>
```

Create model class:

```csharp
public class WelcomeEmailModel
{
    public string Name { get; set; } = "";
    public string AppName { get; set; } = "";
    public string LoginUrl { get; set; } = "";
}
```

Send templated email:

```csharp
public async Task SendWelcomeEmail(string userEmail, string userName)
{
    var model = new WelcomeEmailModel
    {
        Name = userName,
        AppName = "My App",
        LoginUrl = "https://myapp.com/login"
    };

    var result = await _mailService.SendTemplateAsync(
        to: new List<string> { userEmail },
        subject: "Welcome to My App!",
        templateName: "Welcome",
        model: model);
}
```

### Queue Email for Background Sending

```csharp
public async Task SendNewsletter(string userEmail)
{
    var request = new MailRequest(
        to: new List<string> { userEmail },
        subject: "Monthly Newsletter",
        body: "<p>Newsletter content...</p>");

    // Queue with low priority
    var emailId = await _mailService.QueueAsync(
        request,
        priority: EmailPriority.Low);

    _logger.LogInformation("Newsletter queued with ID: {EmailId}", emailId);
}
```

### Schedule Email

```csharp
public async Task ScheduleReminder(string userEmail)
{
    var scheduledFor = DateTimeOffset.UtcNow.AddDays(7);

    var emailId = await _mailService.QueueAsync(
        request: new MailRequest(
            to: new List<string> { userEmail },
            subject: "Weekly Reminder",
            body: "<p>Your weekly reminder...</p>"),
        scheduledFor: scheduledFor);
}
```

## 5. Preview Templates

```csharp
public async Task<string> PreviewWelcomeEmail()
{
    var model = new WelcomeEmailModel
    {
        Name = "Preview User",
        AppName = "My App",
        LoginUrl = "https://myapp.com/login"
    };

    return await _mailService.PreviewTemplateAsync("Welcome", model);
}
```

## 6. Check Email Status

```csharp
public async Task CheckEmailStatus(string emailId)
{
    var queuedEmail = await _mailService.GetQueuedEmailAsync(emailId);

    if (queuedEmail != null)
    {
        Console.WriteLine($"Status: {queuedEmail.Status}");
        Console.WriteLine($"Attempts: {queuedEmail.Attempts}");
        
        if (queuedEmail.Status == EmailStatus.Sent)
            Console.WriteLine($"Sent at: {queuedEmail.SentAt}");
    }
}
```

## 7. Common Configurations

### Gmail SMTP

```json
{
  "EmailOptions": {
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "UserName": "your-email@gmail.com",
      "Password": "your-app-password",
      "EnableSsl": true
    }
  }
}
```

**Note**: Use App Password, not your Gmail password. Enable 2FA and generate app password at: https://myaccount.google.com/apppasswords

### Office 365 SMTP

```json
{
  "EmailOptions": {
    "Smtp": {
      "Host": "smtp.office365.com",
      "Port": 587,
      "UserName": "your-email@outlook.com",
      "Password": "your-password",
      "EnableSsl": true
    }
  }
}
```

### SendGrid (Custom Provider)

```csharp
// Install SendGrid SDK
// dotnet add package SendGrid

public class SendGridEmailProvider : IEmailProvider
{
    public string Name => "sendgrid";
    
    public async Task<EmailResult> SendAsync(MailRequest request, CancellationToken cancellationToken)
    {
        // Use SendGrid SDK
        var client = new SendGridClient(apiKey);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg, cancellationToken);
        
        return response.IsSuccessStatusCode 
            ? EmailResult.Success() 
            : EmailResult.Failure("SendGrid error");
    }
}

// Register in Program.cs
builder.Services
    .AddEmailServices(builder.Configuration)
    .AddEmailProvider<SendGridEmailProvider>();
```

## 8. Environment-Specific Configuration

### appsettings.Development.json

```json
{
  "EmailOptions": {
    "Provider": "mock"
  }
}
```

### appsettings.Production.json

```json
{
  "EmailOptions": {
    "Provider": "smtp",
    "EnableQueue": true,
    "MaxRetryAttempts": 5,
    "EnableTemplateCache": true
  }
}
```

## 9. Testing

### Unit Test with Mock Provider

```csharp
[Fact]
public async Task SendEmail_WithMockProvider_ReturnsSuccess()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddEmailServices(options =>
    {
        options.Provider = "mock";
        options.From = "test@example.com";
        options.EnableQueue = false;
    });

    var serviceProvider = services.BuildServiceProvider();
    var mailService = serviceProvider.GetRequiredService<IMailService>();

    // Act
    var result = await mailService.SendAsync(new MailRequest(
        to: new List<string> { "user@example.com" },
        subject: "Test",
        body: "Body"));

    // Assert
    Assert.True(result.IsSuccess);
}
```

## 10. Troubleshooting

### Emails not sending?

1. Check provider is configured correctly
2. Verify SMTP credentials
3. Check firewall/network settings
4. Enable debug logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Craft.Infrastructure.Emails": "Debug"
    }
  }
}
```

### Template not found?

1. Ensure template file exists in `Email Templates` folder
2. Check file has `.cshtml` extension
3. Verify template name matches (case-sensitive)
4. Set Build Action to "Content" and Copy to Output Directory to "Copy if newer"

### SMTP authentication failed?

1. Verify username and password
2. Check SSL/TLS settings
3. Try different ports (25, 587, 465)
4. For Gmail: Use App Password, not regular password
5. For Office 365: Enable SMTP AUTH in admin center

## Need More Help?

See the [comprehensive README](./README.md) for detailed documentation on all features.
