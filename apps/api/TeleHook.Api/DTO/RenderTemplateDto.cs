namespace TeleHook.Api.DTO;

public class RenderTemplateDto
{
    public string Template { get; set; } = string.Empty;
    public object SampleData { get; set; } = new();
}
