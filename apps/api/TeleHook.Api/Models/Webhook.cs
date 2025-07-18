namespace TeleHook.Api.Models;

public class Webhook
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Uuid { get; set; }
    public int BotId { get; set; }
    public string? TopicId { get; set; }
    public string MessageTemplate { get; set; } 
    public string ParseMode { get; set; } = "MarkdownV2";
    public bool DisableWebPagePreview { get; set; } = true;
    public bool DisableNotification { get; set; } = false;
    public bool IsDisabled { get; set; } = false;
    public bool IsProtected { get; set; } = false;
    
    public string? SecretKey { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public string PayloadSample { get; set; } = string.Empty;

    // Navigation properties
    public Bot Bot { get; set; }
}
