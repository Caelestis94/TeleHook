namespace TeleHook.Api.Models;

public class WebhookStat
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int? WebhookId { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public int ValidationFailures { get; set; }
    public int TelegramFailures { get; set; }
    public long TotalProcessingTimeMs { get; set; }
    public int AvgProcessingTimeMs { get; set; }
    public int MinProcessingTimeMs { get; set; }
    public int MaxProcessingTimeMs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Webhook? Webhook { get; set; }
}