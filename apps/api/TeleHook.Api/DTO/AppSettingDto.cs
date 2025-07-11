namespace TeleHook.Api.DTO;

public class AppSettingDto
{
    public string LogLevel { get; set; } = "Warning";
    public string LogPath { get; set; } = "/app/logs/telehook-.log";
    public int LogRetentionDays { get; set; } = 7;
    public bool EnableWebhookLogging { get; set; } = true;
    public int WebhookLogRetentionDays { get; set; }
    public int StatsDaysInterval { get; set; } = 30;
    public bool EnableFailureNotifications { get; set; }
    public string? NotificationBotToken { get; set; }
    public string? NotificationChatId { get; set; }
    public string? NotificationTopicId { get; set; }
}