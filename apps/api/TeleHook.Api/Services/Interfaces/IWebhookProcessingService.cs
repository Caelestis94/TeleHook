using System.Text.Json;
using TeleHook.Api.Models.Results;

namespace TeleHook.Api.Services.Interfaces;

public interface IWebhookProcessingService
{
    Task<WebhookProcessingResult> ProcessWebhookAsync(
        string uuid, 
        JsonElement payload, 
        HttpRequest request);
}

