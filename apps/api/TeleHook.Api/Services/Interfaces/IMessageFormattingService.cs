using System.Text.Json;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;

namespace TeleHook.Api.Services.Interfaces;

public interface IMessageFormattingService
{
    MessageFormattingResult FormatMessage(Webhook webhook, JsonElement payload);
}
