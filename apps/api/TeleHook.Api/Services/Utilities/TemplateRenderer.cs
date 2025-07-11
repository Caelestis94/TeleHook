using Scriban;
using TeleHook.Api.DTO;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Utilities;

public class TemplateRenderer : ITemplateService
{
    private readonly IJsonToScribanConverter _jsonConverter;
    private readonly ILogger<TemplateRenderer> _logger;

    public TemplateRenderer(IJsonToScribanConverter jsonConverter,
        ILogger<TemplateRenderer> logger)
    {
        _jsonConverter = jsonConverter;
        _logger = logger;
    }

    public RenderTemplateResponse RenderTemplate(string template, object sampleData)
    {
        _logger.LogDebug("Rendering template with sample data. Template length: {TemplateLength}", template.Length);
        
        try
        {
            var parsedTemplate = Template.Parse(template);
            
            if (parsedTemplate.HasErrors)
            {
                var errors = parsedTemplate.Messages.Select(m => m.Message).ToList();
                _logger.LogWarning("Template parsing failed. Errors: {Errors}", string.Join(", ", errors));
                
                return new RenderTemplateResponse
                {
                    Success = false,
                    Errors = errors,
                    Rendered = string.Empty
                };
            }

            var scriptObject = _jsonConverter.ConvertToScriptObject(sampleData);
            
            var result = parsedTemplate.Render(scriptObject);
            
            _logger.LogInformation("Template rendered successfully. Result length: {ResultLength}", result.Length);
            
            return new RenderTemplateResponse
            {
                Success = true,
                Rendered = result,
                Errors = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template");
            
            return new RenderTemplateResponse
            {
                Success = false,
                Errors = [ex.Message],
                Rendered = string.Empty
            };
        }
    }
}
