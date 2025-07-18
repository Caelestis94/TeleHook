using System.Text;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Infrastructure;

public class FailureNotificationService : IFailureNotificationService
{
    private readonly ITelegramService _telegramService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FailureNotificationService> _logger;

    public FailureNotificationService(ITelegramService telegramService, IUnitOfWork unitOfWork,
        ILogger<FailureNotificationService> logger)
    {
        _telegramService = telegramService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SendFailureNotificationAsync(string webhookName, string failureType, string errorMessage,
        string? requestId = null)
    {
        try
        {
            if (!await IsNotificationEnabledAsync())
            {
                _logger.LogDebug(
                    "Failure notifications are disabled, skipping notification for webhook '{WebhookName}'",
                    webhookName);
                return;
            }

            var settings = await _unitOfWork.AppSettings.GetSettingsAsync();
            if (settings == null || string.IsNullOrEmpty(settings.NotificationBotToken) ||
                string.IsNullOrEmpty(settings.NotificationChatId))
            {
                _logger.LogWarning("Notification settings incomplete - bot token or chat ID missing");
                return;
            }

            var messageText = FormatFailureMessage(webhookName, failureType, errorMessage, requestId);

            _logger.LogDebug("Sending failure notification for webhook '{WebhookName}', failure type: '{FailureType}'",
                webhookName, failureType);

            await _telegramService.SendMessageAsync(
                settings.NotificationBotToken,
                settings.NotificationChatId,
                messageText,
                "MarkdownV2",
                true, // disable web page preview
                false, // don't disable notification
                settings.NotificationTopicId);

            _logger.LogInformation("Failure notification sent successfully for webhook '{WebhookName}'", webhookName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending failure notification for webhook '{WebhookName}'", webhookName);
        }
    }

    public async Task<bool> IsNotificationEnabledAsync()
    {
        try
        {
            var settings = await _unitOfWork.AppSettings.GetSettingsAsync();
            return settings?.EnableFailureNotifications == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking notification enabled status");
            return false;
        }
    }

    public async Task<NotificationTestResult> TestNotificationAsync()
    {
        try
        {
            var settings = await _unitOfWork.AppSettings.GetSettingsAsync();
            if (settings == null || string.IsNullOrEmpty(settings.NotificationBotToken) ||
                string.IsNullOrEmpty(settings.NotificationChatId))
            {
                _logger.LogWarning("Notification settings incomplete - bot token or chat ID missing");
                return NotificationTestResult.Failure("Notification settings are incomplete.");
            }

            var messageText =
                "🔔 *Test Notification*\n\nThis is a test notification from TeleHook\\. Your failure notification system is working correctly\\.";

            await _telegramService.SendMessageAsync(
                settings.NotificationBotToken,
                settings.NotificationChatId,
                messageText,
                "MarkdownV2",
                true, // disable web page preview
                false, // don't disable notification
                settings.NotificationTopicId);

            _logger.LogInformation("Test notification sent successfully");
            return NotificationTestResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test notification");
            return NotificationTestResult.Failure("Failed to send test notification: " + ex.Message);
        }
    }

    private static string FormatFailureMessage(string webhookName, string failureType, string errorMessage,
        string? requestId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
        var message = new StringBuilder();

        message.AppendLine("🚨 *Webhook Failure Alert*");
        message.AppendLine();
        message.AppendLine($"*Webhook:* `{webhookName}`");
        message.AppendLine($"*Failure Type:* `{failureType}`");
        message.AppendLine($"*Time:* `{timestamp}`");

        if (!string.IsNullOrEmpty(requestId))
        {
            message.AppendLine($"*Request ID:* `{requestId}`");
        }

        message.AppendLine();
        message.AppendLine("*Error Details:*");
        message.AppendLine($"```");
        message.AppendLine(errorMessage);
        message.AppendLine($"```");

        return message.ToString();
    }
}
