namespace TeleHook.Api.DTO;

public class CreateBotDto
{
    public string Name { get; set; } = string.Empty;
    public string BotToken { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
}
