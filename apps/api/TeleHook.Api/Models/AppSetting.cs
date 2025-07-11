namespace TeleHook.Api.Models;

public class AppSetting
{

    public int Id { get; set; } = 1;
    
    public string LogLevel { get; set; } = "Warning";
    
    public string LogPath { get; set; } = "/app/logs/telehook-.log";
        
    public int LogRetentionDays { get; set; } = 7;
        
    public bool EnableWebhookLogging { get; set; } = true;
        
    public int WebhookLogRetentionDays { get; set; } = 0; // 0 = keep forever
        
    public int StatsDaysInterval { get; set; } = 30;
        
    public string? AdditionalSettings { get; set; } // JSON for future settings
        
    public bool EnableFailureNotifications { get; set; }
        
    public string? NotificationBotToken { get; set; }
    
    public string? NotificationChatId { get; set; }
        
    public string? NotificationTopicId { get; set; }
        
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; } 
}