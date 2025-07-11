using Microsoft.EntityFrameworkCore;
using TeleHook.Api.Data;
using TeleHook.Api.DTO;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Repositories;

public class WebhookLogRepository : Repository<WebhookLog>, IWebhookLogRepository
{
    public WebhookLogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WebhookLog>> GetByWebhookIdAsync(int webhookId)
    {
        return await _dbSet
            .Where(wl => wl.WebhookId == webhookId)
            .OrderByDescending(wl => wl.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<WebhookLog>> GetByRequestIdAsync(string requestId)
    {
        return await _dbSet
            .Where(wl => wl.RequestId == requestId)
            .OrderByDescending(wl => wl.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<WebhookLog>> GetRecentLogsAsync(int count = 100)
    {
        return await _dbSet
            .Include(wl => wl.Webhook)
            .OrderByDescending(wl => wl.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<WebhookLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(wl => wl.Webhook)
            .Where(wl => wl.CreatedAt >= startDate && wl.CreatedAt <= endDate)
            .OrderByDescending(wl => wl.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<WebhookLog>> GetFilteredLogsAsync(
        int? webhookId = null,
        int? statusCode = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? searchTerm = null,
        int limit = 100)
    {
        var query = _dbSet
            .Include(l => l.Webhook)
            .OrderByDescending(l => l.CreatedAt)
            .AsQueryable();

        if (webhookId.HasValue)
            query = query.Where(l => l.WebhookId == webhookId.Value);

        if (statusCode.HasValue)
        {
            // Filter by status code ranges instead of exact matches
            query = statusCode.Value switch
            {
                200 => query.Where(l => l.ResponseStatusCode >= 200 && l.ResponseStatusCode < 300), // 2xx Success
                400 => query.Where(l => l.ResponseStatusCode >= 400 && l.ResponseStatusCode < 500), // 4xx Client Error
                500 => query.Where(l => l.ResponseStatusCode >= 500), // 5xx Server Error
                _ => query.Where(l => l.ResponseStatusCode == statusCode.Value) // Fallback for specific codes
            };
        }

        if (dateFrom.HasValue)
            query = query.Where(l => l.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(l => l.CreatedAt <= dateTo.Value);

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(l =>
                l.RequestId.Contains(searchTerm) ||
                (l.MessageFormatted != null && l.MessageFormatted.Contains(searchTerm)));

        return await query.Take(limit).ToListAsync();
    }

    public async Task<int> DeleteLogsOlderThanAsync(DateTime cutoffDate)
    {
        var logsToDelete = await _dbSet
            .Where(wl => wl.CreatedAt < cutoffDate)
            .ToListAsync();

        if (logsToDelete.Count > 0)
        {
            _dbSet.RemoveRange(logsToDelete);
        }

        return logsToDelete.Count;
    }

    public async Task<IEnumerable<WebhookLogExportResponse>> GetAllLogsForExportAsync()
    {
        return await _dbSet
            .Join(_context.Webhooks,
                log => log.WebhookId,
                webhook => webhook.Id,
                (log, webhook) => new WebhookLogExportResponse
                {
                    Id = log.Id,
                    WebhookId = log.WebhookId,
                    WebhookName = webhook.Name,
                    RequestId = log.RequestId,
                    HttpMethod = log.HttpMethod,
                    RequestUrl = log.RequestUrl,
                    RequestHeaders = log.RequestHeaders,
                    RequestBody = log.RequestBody,
                    ResponseStatusCode = log.ResponseStatusCode,
                    ResponseBody = log.ResponseBody,
                    ProcessingTimeMs = log.ProcessingTimeMs,
                    PayloadValidated = log.PayloadValidated,
                    ValidationErrors = log.ValidationErrors,
                    MessageFormatted = log.MessageFormatted,
                    TelegramSent = log.TelegramSent,
                    TelegramResponse = log.TelegramResponse,
                    CreatedAt = log.CreatedAt
                })
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync();
    }
}
