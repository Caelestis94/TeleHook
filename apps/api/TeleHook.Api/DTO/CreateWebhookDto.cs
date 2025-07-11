namespace TeleHook.Api.DTO;

public class CreateWebhookDto
{
    public string Name { get; set; } = string.Empty;
    public int BotId { get; set; }
    public string? TopicId { get; set; }
    public string MessageTemplate { get; set; } = string.Empty;
    public string ParseMode { get; set; } = "MarkdownV2";
    public bool DisableWebPagePreview { get; set; } = true;
    public bool DisableNotification { get; set; } = false;
    public bool IsDisabled { get; set; } = false;
    public bool IsProtected { get; set; } = false;
    public string? SecretKey { get; set; } = null;
    public string PayloadSample { get; set; } = "{}";
}