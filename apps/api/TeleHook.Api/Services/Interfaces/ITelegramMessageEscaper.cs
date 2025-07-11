namespace TeleHook.Api.Services.Interfaces;

public interface ITelegramMessageEscaper
{
    string EscapeForParseMode(string text, string parseMode);
}