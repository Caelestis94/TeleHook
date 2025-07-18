using TeleHook.Api.DTO;

namespace TeleHook.Api.Services.Interfaces;

public interface IWebhookStatService
{
    Task UpdateStatsAsync(int webhookId, int statusCode, int processingTimeMs, bool payloadValidated,
        bool telegramSent);

    Task UpdateDailyStatsAsync(DateTime date, int? webhookId, int statusCode, int processingTimeMs,
        bool payloadValidated, bool telegramSent);

    Task<OverviewStatsResponse> GetOverviewStatsAsync();
    Task<WebhookStatsResponse> GetWebhookStatsAsync(int webhookId, int days = 30);
}
