using Scriban;

namespace TeleHook.Api.Services.Interfaces;

public interface ITemplateParsingService
{
    Template GetTemplate(int webhookId);
    Task RefreshTemplateAsync(int webhookId);
    Task InitializeAsync();
}
