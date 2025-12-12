using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Craft.Notifications;

/// <summary>
/// Generic webhook notification provider for HTTP callbacks.
/// </summary>
public class WebhookNotificationProvider : NotificationProviderBase
{
    private readonly IHttpClientFactory? _httpClientFactory;

    public WebhookNotificationProvider(
        ILogger<WebhookNotificationProvider> logger,
        NotificationOptions options,
        IHttpClientFactory? httpClientFactory = null)
        : base(logger, options)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc/>
    public override NotificationChannel Channel => NotificationChannel.Webhook;

    /// <inheritdoc/>
    public override string Name => "Webhook";

    /// <inheritdoc/>
    public override int Priority => 20;

    /// <inheritdoc/>
    public override async Task<NotificationDeliveryResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        LogDeliveryStart(notification);

        var webhookUrl = GetWebhookUrl(notification);
        if (string.IsNullOrEmpty(webhookUrl))
        {
            var error = "Webhook URL not configured";
            Logger.LogWarning("Webhook delivery failed: {Error}", error);
            return NotificationDeliveryResult.Failure(notification.Id, Channel, error);
        }

        var (result, durationMs) = await MeasureAsync<(bool, string?)>(async () =>
        {
            try
            {
                using var httpClient = _httpClientFactory?.CreateClient() ?? new HttpClient();
                
                var payload = CreateWebhookPayload(notification);
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PostAsync(webhookUrl, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return (true, responseContent);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Webhook delivery failed for notification {NotificationId}", notification.Id);
                return (false, ex.Message);
            }
        });

        if (result.Item1)
        {
            LogDeliverySuccess(notification, durationMs);
            return NotificationDeliveryResult.Success(
                notification.Id,
                Channel,
                result.Item2,
                durationMs);
        }

        var errorMessage = result.Item2 ?? "Unknown error";
        LogDeliveryFailure(notification, errorMessage, durationMs);
        return NotificationDeliveryResult.Failure(
            notification.Id,
            Channel,
            errorMessage,
            durationMs);
    }

    /// <inheritdoc/>
    public override bool CanDeliver(Notification notification)
    {
        return base.CanDeliver(notification) &&
               !string.IsNullOrEmpty(GetWebhookUrl(notification));
    }

    /// <summary>
    /// Gets the webhook URL for the notification.
    /// Priority: notification metadata > action URL > default config
    /// </summary>
    private string? GetWebhookUrl(Notification notification)
    {
        // Check notification metadata for webhook URL
        var metadata = notification.GetMetadata();
        if (metadata?.TryGetValue("WebhookUrl", out var metadataUrl) == true)
            return metadataUrl?.ToString();

        // Use action URL if it looks like a webhook
        if (!string.IsNullOrEmpty(notification.ActionUrl) &&
            notification.ActionUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return notification.ActionUrl;

        return null;
    }

    /// <summary>
    /// Creates the webhook payload from the notification.
    /// </summary>
    protected virtual object CreateWebhookPayload(Notification notification)
    {
        return new
        {
            id = notification.Id,
            title = notification.Title,
            message = notification.Message,
            type = notification.Type.ToString(),
            priority = notification.Priority.ToString(),
            category = notification.Category,
            recipientUserId = notification.RecipientUserId,
            actionUrl = notification.ActionUrl,
            imageUrl = notification.ImageUrl,
            metadata = notification.GetMetadata(),
            timestamp = notification.CreatedAt
        };
    }
}
