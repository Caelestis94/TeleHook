using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Services.Domain;

namespace TeleHook.Api.Services.Interfaces;

public interface ITelegramService
{
    Task<TelegramResult> SendMessageAsync(Webhook webhook, string messageText);
    Task<TelegramResult> SendMessageAsync(string botToken, string chatId, string messageText, string? parseMode = null, bool? disableWebPagePreview = null, bool? disableNotification = null, string? topicId = null);
    Task<TelegramResult> TestConnectionAsync(Bot bot);
}
