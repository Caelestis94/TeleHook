using Microsoft.EntityFrameworkCore;
using TeleHook.Api.Data;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Repositories;

public class WebhookStatRepository : Repository<WebhookStat>, IWebhookStatRepository
{
    public WebhookStatRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<WebhookStat?> GetByDateAndWebhookAsync(DateTime date, int? webhookId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Date == date && s.WebhookId == webhookId);
    }

    public async Task<IEnumerable<WebhookStat>> GetByWebhookIdAsync(int webhookId)
    {
        return await _dbSet
            .Where(ws => ws.WebhookId == webhookId)
            .OrderByDescending(ws => ws.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<WebhookStat>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(ws => ws.Webhook)
            .Where(ws => ws.Date >= startDate && ws.Date <= endDate)
            .OrderByDescending(ws => ws.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<WebhookStat>> GetGlobalStatsAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(ws => ws.WebhookId == null && ws.Date >= startDate && ws.Date <= endDate)
            .OrderByDescending(ws => ws.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetTopWebhookStatsAsync(DateTime startDate, DateTime endDate, int take = 5)
    {
        return await _dbSet
            .Where(s => s.WebhookId != null && s.Date >= startDate && s.Date <= endDate)
            .Include(s => s.Webhook)
            .GroupBy(s => new { WebhookId = s.WebhookId, s.Webhook!.Name })
            .Select(g => new
            {
                WebhookId = g.Key.WebhookId,
                WebhookName = g.Key.Name,
                TotalRequests = g.Sum(s => s.TotalRequests),
                SuccessRate = g.Sum(s => s.TotalRequests) > 0
                    ? (double)g.Sum(s => s.SuccessfulRequests) / g.Sum(s => s.TotalRequests) * 100
                    : 0
            })
            .OrderByDescending(e => e.TotalRequests)
            .Take(take)
            .ToListAsync();
    }
}
