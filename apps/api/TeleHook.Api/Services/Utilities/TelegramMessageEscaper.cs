using TeleHook.Api.Helpers;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Utilities;

/// <summary>
/// Service for escaping text messages according to Telegram's parse mode requirements.
/// Uses the strategy pattern with different escaper implementations.
/// </summary>
public class TelegramMessageEscaper : ITelegramMessageEscaper
{
    private readonly MarkdownV2Escaper _markdownV2Escaper = new();
    private readonly MarkdownEscaper _markdownEscaper = new();
    private readonly HtmlEscaper _htmlEscaper = new();

    public string EscapeForParseMode(string text, string parseMode)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return parseMode.ToLower() switch
        {
            "markdownv2" => _markdownV2Escaper.Escape(text),
            "markdown" => _markdownEscaper.Escape(text),
            "html" => _htmlEscaper.Escape(text),
            _ => text
        };
    }
}
