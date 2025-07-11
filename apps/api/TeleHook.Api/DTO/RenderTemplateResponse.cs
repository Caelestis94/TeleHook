namespace TeleHook.Api.DTO;

public class RenderTemplateResponse
{
    public string Rendered { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<string>? Errors { get; set; }
}
