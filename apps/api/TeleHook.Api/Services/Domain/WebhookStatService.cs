using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Domain;

public class WebhookStatService : IWebhookStatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WebhookStatService> _logger;
    private readonly AppSettingDto _appSetting;

    public WebhookStatService(IUnitOfWork unitOfWork, ILogger<WebhookStatService> logger,AppSettingDto appSetting)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _appSetting = appSetting;
    }

    public async Task UpdateStatsAsync(int webhookId, int statusCode, int processingTimeMs,
        bool payloadValidated, bool telegramSent)
    {
        try
        {
            var today = DateTime.UtcNow.Date;

            await UpdateDailyStatsAsync(today, webhookId, statusCode, processingTimeMs, payloadValidated,
                telegramSent);

            await UpdateDailyStatsAsync(today, null, statusCode, processingTimeMs, payloadValidated, telegramSent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update webhook stats");
            throw new InternalServerErrorException("Failed to update webhook statistics", ex);
        }
    }

    public async Task UpdateDailyStatsAsync(DateTime date, int? webhookId, int statusCode,
        int processingTimeMs, bool payloadValidated, bool telegramSent)
    {
        try
        {
            var stats = await _unitOfWork.WebhookStats.GetByDateAndWebhookAsync(date, webhookId);

            if (stats == null)
            {
                stats = new WebhookStat
                {
                    Date = date,
                    WebhookId = webhookId,
                    TotalRequests = 1,
                    SuccessfulRequests = statusCode >= 200 && statusCode < 300 ? 1 : 0,
                    FailedRequests = statusCode >= 400 ? 1 : 0,
                    ValidationFailures = !payloadValidated ? 1 : 0,
                    TelegramFailures = !telegramSent ? 1 : 0,
                    TotalProcessingTimeMs = processingTimeMs,
                    AvgProcessingTimeMs = processingTimeMs,
                    MinProcessingTimeMs = processingTimeMs,
                    MaxProcessingTimeMs = processingTimeMs,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.WebhookStats.AddAsync(stats);
            }
            else
            {
                stats.TotalRequests++;

                if (statusCode >= 200 && statusCode < 300)
                    stats.SuccessfulRequests++;

                if (statusCode >= 400)
                    stats.FailedRequests++;

                if (!payloadValidated)
                    stats.ValidationFailures++;

                if (!telegramSent)
                    stats.TelegramFailures++;

                stats.TotalProcessingTimeMs += processingTimeMs;
                stats.AvgProcessingTimeMs = (int)(stats.TotalProcessingTimeMs / stats.TotalRequests);
                stats.MinProcessingTimeMs = Math.Min(stats.MinProcessingTimeMs, processingTimeMs);
                stats.MaxProcessingTimeMs = Math.Max(stats.MaxProcessingTimeMs, processingTimeMs);
                stats.UpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update daily stats");
            throw new InternalServerErrorException("Failed to update daily statistics", ex);
        }
    }

    public async Task<OverviewStatsResponse> GetOverviewStatsAsync()
    {
        try
        {
            var intervalDays = _appSetting.StatsDaysInterval > 0 ? _appSetting.StatsDaysInterval : -30;
            
            var interval = DateTime.UtcNow.AddDays(-intervalDays).Date;
            var today = DateTime.UtcNow.Date;

            var globalStats = (await _unitOfWork.WebhookStats.GetGlobalStatsAsync(interval, today)).ToList();

            var todayStats = globalStats.FirstOrDefault(s => s.Date == today);

            var totalRequests = globalStats.Sum(s => s.TotalRequests);
            var totalSuccessful = globalStats.Sum(s => s.SuccessfulRequests);
            var totalFailed = globalStats.Sum(s => s.FailedRequests);
            var avgProcessingTime = globalStats.Any() ? globalStats.Average(s => s.AvgProcessingTimeMs) : 0;

            var webhookStats = await _unitOfWork.WebhookStats.GetTopWebhookStatsAsync(interval, today, 5);

            var dailyTrend = globalStats
                .OrderBy(s => s.Date)
                .Select(s => new
                {
                    Date = s.Date.ToString("yyyy-MM-dd"), 
                    Requests = s.TotalRequests,
                    SuccessRate = s.TotalRequests > 0 ? (double)s.SuccessfulRequests / s.TotalRequests * 100 : 0,
                    AvgProcessingTime = s.AvgProcessingTimeMs
                })
                .ToList();

            return new OverviewStatsResponse
            {
                Summary = new OverviewSummary
                {
                    TotalRequests = totalRequests,
                    SuccessRate = totalRequests > 0 ? (double)totalSuccessful / totalRequests * 100 : 0,
                    FailedRequests = totalFailed,
                    AvgProcessingTime = Math.Round(avgProcessingTime, 0),
                    TodayRequests = todayStats?.TotalRequests ?? 0
                },
                TopWebhooks = webhookStats,
                DailyTrend = dailyTrend.Select(d => new DailyTrendItem
                {
                    Date = d.Date,
                    Requests = d.Requests,
                    SuccessRate = d.SuccessRate,
                    AvgProcessingTime = d.AvgProcessingTime
                }),
                Period = $"Last {intervalDays} days",
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get overview stats");
            throw new InternalServerErrorException("Failed to retrieve overview statistics", ex);
        }
    }

    public async Task<WebhookStatsResponse> GetWebhookStatsAsync(int webhookId, int days = 30)
    {
        try
        {
        
            var startDate = DateTime.UtcNow.AddDays(-days).Date;
            var endDate = DateTime.UtcNow.Date;

            var stats = (await _unitOfWork.WebhookStats.GetByDateRangeAsync(startDate, endDate))
                .Where(s => s.WebhookId == webhookId)
                .OrderBy(s => s.Date)
                .ToList();

            var totalRequests = stats.Sum(s => s.TotalRequests);
            var totalSuccessful = stats.Sum(s => s.SuccessfulRequests);

            return new WebhookStatsResponse
            {
                WebhookId = webhookId,
                TotalRequests = totalRequests,
                SuccessRate = totalRequests > 0 ? (double)totalSuccessful / totalRequests * 100 : 0,
                AvgProcessingTime = stats.Any() ? stats.Average(s => s.AvgProcessingTimeMs) : 0,
                DailyStats = stats.Select(s => new DailyStatItem
                {
                    Date = s.Date.ToString("yyyy-MM-dd"),
                    Requests = s.TotalRequests,
                    SuccessRate = s.TotalRequests > 0 ? (double)s.SuccessfulRequests / s.TotalRequests * 100 : 0,
                    AvgProcessingTime = s.AvgProcessingTimeMs
                })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get webhook stats for webhook '{WebhookId}'", webhookId);
            throw new InternalServerErrorException($"Failed to retrieve statistics for webhook {webhookId}", ex);
        }

    }
}