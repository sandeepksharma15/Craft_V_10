using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Craft.Notifications;

/// <summary>
/// Microsoft Teams webhook notification provider.
/// </summary>
public class TeamsWebhookNotificationProvider : NotificationProviderBase
{
    private readonly IHttpClientFactory? _httpClientFactory;

    public TeamsWebhookNotificationProvider(
        ILogger<TeamsWebhookNotificationProvider> logger,
        NotificationOptions options,
        IHttpClientFactory? httpClientFactory = null)
        : base(logger, options)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc/>
    public override NotificationChannel Channel => NotificationChannel.Webhook;

    /// <inheritdoc/>
    public override string Name => "TeamsWebhook";

    /// <inheritdoc/>
    public override int Priority => 21; // Lower priority than generic webhook

    /// <inheritdoc/>
    public override async Task<NotificationDeliveryResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        LogDeliveryStart(notification);

        var webhookUrl = GetTeamsWebhookUrl(notification);
        if (string.IsNullOrEmpty(webhookUrl))
        {
            var error = "Teams webhook URL not configured";
            Logger.LogWarning("Teams webhook delivery failed: {Error}", error);
            return NotificationDeliveryResult.Failure(notification.Id, Channel, error);
        }

        var (result, durationMs) = await MeasureAsync<(bool, string?)>(async () =>
        {
            try
            {
                using var httpClient = _httpClientFactory?.CreateClient() ?? new HttpClient();
                
                var payload = CreateTeamsMessageCard(notification);
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
                Logger.LogError(ex, "Teams webhook delivery failed for notification {NotificationId}", notification.Id);
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
               !string.IsNullOrEmpty(GetTeamsWebhookUrl(notification));
    }

    /// <summary>
    /// Gets the Teams webhook URL for the notification.
    /// Priority: notification metadata > global config
    /// </summary>
    private string? GetTeamsWebhookUrl(Notification notification)
    {
        // Check notification metadata for Teams webhook URL
        var metadata = notification.GetMetadata();
        if (metadata?.TryGetValue("TeamsWebhookUrl", out var metadataUrl) == true)
            return metadataUrl?.ToString();

        // Fall back to global configuration
        return Options.TeamsWebhookUrl;
    }

    /// <summary>
    /// Creates a Teams MessageCard payload from the notification.
    /// Uses the legacy MessageCard format for compatibility.
    /// </summary>
    /// <remarks>
    /// For Adaptive Cards, you would need to modify this structure.
    /// See: https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/connectors-using
    /// </remarks>
    private object CreateTeamsMessageCard(Notification notification)
    {
        var themeColor = GetThemeColor(notification.Type);
        var sections = new List<object>
        {
            new
            {
                activityTitle = notification.Title,
                activitySubtitle = $"Priority: {notification.Priority}",
                activityImage = notification.ImageUrl,
                text = notification.Message,
                facts = GetFacts(notification)
            }
        };

        var potentialActions = new List<object>();
        if (!string.IsNullOrEmpty(notification.ActionUrl))
        {
            potentialActions.Add(new
            {
                type = "OpenUri",
                name = "View Details",
                targets = new[]
                {
                    new { os = "default", uri = notification.ActionUrl }
                }
            });
        }

        return new
        {
            type = "MessageCard",
            context = "https://schema.org/extensions",
            summary = notification.Title,
            themeColor,
            sections,
            potentialAction = potentialActions.Count > 0 ? potentialActions : null
        };
    }

    /// <summary>
    /// Gets additional facts to display in the Teams message.
    /// </summary>
    private List<object> GetFacts(Notification notification)
    {
        var facts = new List<object>
        {
            new { name = "Type", value = notification.Type.ToString() },
            new { name = "Priority", value = notification.Priority.ToString() }
        };

        if (!string.IsNullOrEmpty(notification.Category))
            facts.Add(new { name = "Category", value = notification.Category });

        if (!string.IsNullOrEmpty(notification.RecipientUserId))
            facts.Add(new { name = "Recipient", value = notification.RecipientUserId });

        facts.Add(new { name = "Timestamp", value = notification.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC") });

        return facts;
    }

    /// <summary>
    /// Gets the theme color based on notification type.
    /// </summary>
    private string GetThemeColor(NotificationType type)
    {
        return type switch
        {
            NotificationType.Success => "00FF00", // Green
            NotificationType.Warning => "FFA500", // Orange
            NotificationType.Error => "FF0000",   // Red
            NotificationType.Alert => "FF0000",   // Red
            NotificationType.Info => "0078D4",    // Blue
            NotificationType.System => "808080",  // Gray
            _ => "0078D4"                         // Default Blue
        };
    }
}
