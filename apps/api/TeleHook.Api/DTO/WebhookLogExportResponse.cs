namespace TeleHook.Api.DTO;

public class WebhookLogExportResponse
{
    public int Id { get; set; }
    public int WebhookId { get; set; }
    public string WebhookName { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "POST";
    public string RequestUrl { get; set; } = string.Empty;
    public string? RequestHeaders { get; set; }
    public string? RequestBody { get; set; }
    public int ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public int ProcessingTimeMs { get; set; }
    public bool PayloadValidated { get; set; } = true;
    public string? ValidationErrors { get; set; }
    public string? MessageFormatted { get; set; }
    public bool TelegramSent { get; set; }
    public string? TelegramResponse { get; set; }
    public DateTime CreatedAt { get; set; }
}