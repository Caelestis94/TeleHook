namespace TeleHook.Api.DTO;

/// <summary>
/// Response DTO for webhook-specific statistics.
/// </summary>
public class WebhookStatsResponse
{
    public int WebhookId { get; init; }
    public int TotalRequests { get; init; }
    public double SuccessRate { get; init; }
    public double AvgProcessingTime { get; init; }
    public IEnumerable<DailyStatItem> DailyStats { get; init; } = null!;
}

public class DailyStatItem
{
    public string Date { get; init; } = string.Empty;
    public int Requests { get; init; }
    public double SuccessRate { get; init; }
    public double AvgProcessingTime { get; init; }
}
