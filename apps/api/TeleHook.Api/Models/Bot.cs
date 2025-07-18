namespace TeleHook.Api.Models;

/// <summary>
///  A Telegram bot configuration.
/// </summary>
public class Bot
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string BotToken { get; set; }
    public string ChatId { get; set; }
    public bool HasPassedTest { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
