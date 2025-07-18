namespace TeleHook.Api.DTO;

/// <summary>
/// Response DTO for overview statistics across all webhooks.
/// </summary>
public class OverviewStatsResponse
{
    public OverviewSummary Summary { get; init; } = null!;
    public IEnumerable<object> TopWebhooks { get; init; } = null!;
    public IEnumerable<DailyTrendItem> DailyTrend { get; init; } = null!;
    public string Period { get; init; } = string.Empty;
}

public class OverviewSummary
{
    public int TotalRequests { get; init; }
    public double SuccessRate { get; init; }
    public int FailedRequests { get; init; }
    public double AvgProcessingTime { get; init; }
    public int TodayRequests { get; init; }
}

public class DailyTrendItem
{
    public string Date { get; init; } = string.Empty;
    public int Requests { get; init; }
    public double SuccessRate { get; init; }
    public double AvgProcessingTime { get; init; }
}
