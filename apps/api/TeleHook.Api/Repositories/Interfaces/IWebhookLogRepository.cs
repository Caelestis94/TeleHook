using TeleHook.Api.DTO;
using TeleHook.Api.Models;

namespace TeleHook.Api.Repositories.Interfaces;

public interface IWebhookLogRepository : IRepository<WebhookLog>
{
    Task<IEnumerable<WebhookLog>> GetByWebhookIdAsync(int webhookId);
    Task<IEnumerable<WebhookLog>> GetByRequestIdAsync(string requestId);
    Task<IEnumerable<WebhookLog>> GetRecentLogsAsync(int count = 100);
    Task<IEnumerable<WebhookLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<WebhookLog>> GetFilteredLogsAsync(
        int? webhookId = null,
        int? statusCode = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? searchTerm = null,
        int limit = 100);
    Task<int> DeleteLogsOlderThanAsync(DateTime cutoffDate);
    Task<IEnumerable<WebhookLogExportResponse>> GetAllLogsForExportAsync();
}
