using System.Collections.Concurrent;
using Scriban;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Domain;

public class TemplateParsingService : ITemplateParsingService
{
    private readonly ConcurrentDictionary<int, Template> _compiledTemplates = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TemplateParsingService> _logger;

    public TemplateParsingService(IServiceProvider serviceProvider, ILogger<TemplateParsingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing template parsing service...");
        
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        var webhooks = await unitOfWork.Webhooks.GetAllAsync();
        
        foreach (var webhook in webhooks)
        {
            try
            {
                var template = Template.Parse(webhook.MessageTemplate);
                _compiledTemplates[webhook.Id] = template;
                _logger.LogDebug("Compiled template for webhook {WebhookId}", webhook.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compile template for webhook {WebhookId}", webhook.Id);
            }
        }
        
        _logger.LogInformation("Compiled {Count} templates", _compiledTemplates.Count);
    }

    public Template GetTemplate(int webhookId)
    {
        if (_compiledTemplates.TryGetValue(webhookId, out var template))
        {
            return template;
        }
        
        throw new BadRequestException($"Template not found for webhook {webhookId}");
    }

    public async Task RefreshTemplateAsync(int webhookId)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        var webhook = await unitOfWork.Webhooks.GetByIdAsync(webhookId);
        if (webhook != null)
        {
            var template = Template.Parse(webhook.MessageTemplate);
            _compiledTemplates[webhookId] = template;
            _logger.LogDebug("Refreshed template for webhook {WebhookId}", webhookId);
        }
    }
}