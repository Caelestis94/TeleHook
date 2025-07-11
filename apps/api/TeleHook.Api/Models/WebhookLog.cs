namespace TeleHook.Api.Models;

public class WebhookLog
{
    public int Id { get; init; }
    public int WebhookId { get; init; }
    public string RequestId { get; init; } = string.Empty;
    public string HttpMethod { get; init; } = "POST";
    public string RequestUrl { get; init; } = string.Empty;
    public string? RequestHeaders { get; init; }
    public string? RequestBody { get; init; }
    public int ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public int ProcessingTimeMs { get; set; }
    /// <summary>
    /// Obselete, used for payload against schema validation, feature removed. Leaving for possible re-introduction later.
    /// </summary>
    public bool PayloadValidated { get; set; } = true;
    public string? ValidationErrors { get; set; }
    public string? MessageFormatted { get; set; }
    public bool TelegramSent { get; set; }
    public string? TelegramResponse { get; set; }
    public DateTime CreatedAt { get; init; }

    public Webhook? Webhook { get; init; }
}