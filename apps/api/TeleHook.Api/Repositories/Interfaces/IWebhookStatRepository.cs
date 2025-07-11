using TeleHook.Api.Models;

namespace TeleHook.Api.Repositories.Interfaces;

public interface IWebhookStatRepository : IRepository<WebhookStat>
{
    Task<WebhookStat?> GetByDateAndWebhookAsync(DateTime date, int? webhookId);
    Task<IEnumerable<WebhookStat>> GetByWebhookIdAsync(int webhookId);
    Task<IEnumerable<WebhookStat>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<WebhookStat>> GetGlobalStatsAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<object>> GetTopWebhookStatsAsync(DateTime startDate, DateTime endDate, int take = 5);
}
