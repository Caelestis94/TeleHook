using System.Text.Json;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Domain;

public class MessageFormattingService : IMessageFormattingService
{
    private readonly ITelegramMessageEscaper _escaper;
    private readonly IJsonToScribanConverter _jsonConverter;
    private readonly ILogger<MessageFormattingService> _logger;
    private readonly ITemplateParsingService _templateParsingService;

    public MessageFormattingService(ITelegramMessageEscaper escaper,
        IJsonToScribanConverter jsonConverter,
        ILogger<MessageFormattingService> logger,
        ITemplateParsingService templateParsingService)
    {
        _escaper = escaper;
        _jsonConverter = jsonConverter;
        _logger = logger;
        _templateParsingService = templateParsingService;
    }

    public MessageFormattingResult FormatMessage(Webhook webhook, JsonElement payload)
    {
        _logger.LogDebug("Starting message formatting with Scriban template engine. Parse mode: {ParseMode}", webhook.ParseMode);
        _logger.LogDebug("Template: {Template}", webhook.MessageTemplate);
        _logger.LogDebug("Payload JSON: {Payload}", payload.ToString());

        try
        {
            var parsedTemplate = _templateParsingService.GetTemplate(webhook.Id);

            var scriptObject = _jsonConverter.ConvertToScriptObject(payload);

            var result = parsedTemplate.Render(scriptObject);
            _logger.LogDebug("Template rendered successfully: {Result}", result);

            if (string.IsNullOrWhiteSpace(result))
            {
                _logger.LogWarning("Formatted message is empty after escaping. Returning default message.");
                result = $"No data available to display, please check the provided template and payload for webhook '{webhook.Name}'.";
            }

            result = result.Replace("\\n", "\n");

            result = _escaper.EscapeForParseMode(result, webhook.ParseMode);

            _logger.LogInformation("Message formatting completed successfully");
            _logger.LogDebug("Message formatting completed for webhook '{WebhookUUID}', length: {MessageLength}",
                webhook.Uuid, result.Length);
            _logger.LogDebug("Formatted message text: {MessageText}", result);

            return MessageFormattingResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format message template for webhook {WebhookUUID}. Template: {Template}, ParseMode: {ParseMode}",
                webhook.Uuid, webhook.MessageTemplate, webhook.ParseMode);

            return MessageFormattingResult.Failure($"Failed to format message template: {ex.Message}");
        }
    }
}
