namespace TeleHook.Api.DTO;

public class CaptureSessionDto
{
    public required string SessionId { get; set; }
    public required string CaptureUrl { get; set; } 
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public object? Payload { get; set; }
}