using TeleHook.Api.DTO;

namespace TeleHook.Api.Services.Interfaces;

public interface ITemplateService
{
    RenderTemplateResponse RenderTemplate(string template, object sampleData);
}
