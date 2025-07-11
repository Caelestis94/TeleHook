namespace TeleHook.Api.Models;

public class CaptureSession
{
    public string Id { get; set; }
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public object? CapturedPayload { get; set; }
    public bool IsCompleted { get; set; }
}
