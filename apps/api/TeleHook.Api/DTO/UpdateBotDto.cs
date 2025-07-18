namespace TeleHook.Api.DTO;

public class UpdateBotDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BotToken { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
}
