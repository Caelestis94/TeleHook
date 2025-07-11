using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Domain;

public class WebhookProcessingService : IWebhookProcessingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageFormattingService _messageFormattingService;
    private readonly ITelegramService _telegramService;
    private readonly IWebhookLoggingService _loggingService;
    private readonly IFailureNotificationService _failureNotificationService;
    private readonly ILogger<WebhookProcessingService> _logger;

    public WebhookProcessingService(IUnitOfWork unitOfWork,
        IMessageFormattingService messageFormattingService,
        ITelegramService telegramService,
        IWebhookLoggingService loggingService,
        IFailureNotificationService failureNotificationService,
        ILogger<WebhookProcessingService> logger)
    {
        _unitOfWork = unitOfWork;
        _messageFormattingService = messageFormattingService;
        _telegramService = telegramService;
        _loggingService = loggingService;
        _failureNotificationService = failureNotificationService;
        _logger = logger;
    }

    public async Task<WebhookProcessingResult> ProcessWebhookAsync(
        string uuid,
        JsonElement payload,
        HttpRequest request)
    {
        var secretKey = request.Query["secret_key"].FirstOrDefault()
                        ?? request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");


        var stopwatch = Stopwatch.StartNew();
        string? requestId = null;

        Webhook? webhook = null;

        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["WebhookUUID"] = uuid,
            ["RequestMethod"] = request.Method,
            ["UserAgent"] = request.Headers.UserAgent.FirstOrDefault() ?? "unknown",
            ["RemoteIP"] = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });

        _logger.LogInformation("Webhook request received for UUID {WebhookUUID}", uuid);

        try
        {
            if (!Guid.TryParse(uuid, out _))
            {
                _logger.LogWarning("Invalid UUID format provided: {WebhookUUID}", uuid);
                return WebhookProcessingResult.Failure("Invalid UUID format");
            }

            webhook = await _unitOfWork.Webhooks.GetByUuidWithRelationsAsync(uuid);
            if (webhook == null)
            {
                _logger.LogWarning("Webhook not found for UUID '{WebhookUUID}'", uuid);
                return WebhookProcessingResult.Failure($"Webhook endpoint with UUID '{uuid}' was not found", 404);
            }

            var authResult = ValidateWebhookAuthenticationAsync(webhook, secretKey);
            if (!authResult.IsSuccess)
            {
                return WebhookProcessingResult.Failure(authResult.Error!, authResult.StatusCode);
            }

            using var webhookScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["WebhookId"] = webhook.Id,
                ["WebhookName"] = webhook.Name,
                ["IsDisabled"] = webhook.IsDisabled,
            });

            _logger.LogDebug("Found webhook '{WebhookUUID}' (ID: {WebhookId})", webhook.Uuid, webhook.Id);

            requestId = await _loggingService.StartRequestAsync(webhook.Id, request);

            if (webhook.IsDisabled)
            {
                _logger.LogWarning("Webhook '{WebhookUUID}' is disabled, rejecting request", webhook.Uuid);
                await _loggingService.CompleteRequestAsync(requestId, 400, "Webhook is disabled",
                    (int)stopwatch.ElapsedMilliseconds);
                return WebhookProcessingResult.Failure("Webhook is disabled");
            }

            var messageResult = await FormatMessageAsync(webhook, payload, requestId);
            if (!messageResult.IsSuccess)
            {
                _logger.LogWarning("Message formatting failed for webhook '{WebhookUUID}': {Error}",
                    webhook.Uuid, messageResult.Error);

                // Send failure notification
                _ = Task.Run(async () => await _failureNotificationService.SendFailureNotificationAsync(
                    webhook.Name,
                    "Message Formatting",
                    messageResult.Error ?? "Message formatting failed",
                    requestId));

                return WebhookProcessingResult.Failure(
                    messageResult.Error ?? "Message formatting failed", 500);
            }

            var telegramResult = await SendToTelegramAsync(webhook, messageResult.MessageText!, requestId, stopwatch);

            if (telegramResult.IsSuccess)
            {
                _logger.LogInformation(
                    "Webhook '{WebhookUUID}' processed successfully in {ElapsedMs}ms",
                    uuid, stopwatch.ElapsedMilliseconds);
            }

            return telegramResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing webhook '{WebhookUUID}'", uuid);

            if (requestId != null)
            {
                await _loggingService.CompleteRequestAsync(requestId, 500,
                    JsonSerializer.Serialize(new { message = "Internal server error" }),
                    (int)stopwatch.ElapsedMilliseconds);
            }

            // Send failure notification if webhook is available
            if (webhook != null)
            {
                _ = Task.Run(async () => await _failureNotificationService.SendFailureNotificationAsync(
                    webhook.Name,
                    "Processing Error",
                    ex.Message,
                    requestId));
            }

            return WebhookProcessingResult.Failure("Internal server error", 500);
        }
    }

    private WebhookAuthResult ValidateWebhookAuthenticationAsync(Webhook webhook, string? secretKey)
    {
        if (!webhook.IsProtected)
        {
            return WebhookAuthResult.Success();
        }

        if (string.IsNullOrEmpty(webhook.SecretKey))
        {
            _logger.LogWarning("Webhook '{WebhookUUID}' is protected but has no secret key", webhook.Uuid);
            return WebhookAuthResult.Failure("Webhook is protected but has no secret key", 401);
        }

        if (string.IsNullOrEmpty(secretKey))
        {
            _logger.LogWarning("Webhook '{WebhookUUID}' is protected but no secret key provided", webhook.Uuid);
            return WebhookAuthResult.Failure("Webhook is protected, please provide a secret key", 401);
        }

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(secretKey),
                Encoding.UTF8.GetBytes(webhook.SecretKey)))
        {
            _logger.LogWarning("Invalid secret key provided for webhook '{WebhookUUID}'", webhook.Uuid);
            return WebhookAuthResult.Failure("Invalid secret key provided", 401);
        }

        return WebhookAuthResult.Success();
    }

    private async Task<MessageFormattingResult> FormatMessageAsync(
        Webhook webhook,
        JsonElement payload,
        string requestId)
    {
        _logger.LogDebug(
            "Starting message formatting for webhook '{WebhookUUID}' with template: {MessageTemplate}",
            webhook.Uuid, webhook.MessageTemplate);
        _logger.LogDebug("Message formatting input - ParseMode: {ParseMode}, Payload: {Payload}",
            webhook.ParseMode, JsonSerializer.Serialize(payload));

        var messageFormattingResult = _messageFormattingService.FormatMessage(webhook, payload);
        
        if (messageFormattingResult.IsSuccess)
            await _loggingService.LogMessageFormattingAsync(requestId,  messageFormattingResult.MessageText!);

        return messageFormattingResult;
    }

    private async Task<WebhookProcessingResult> SendToTelegramAsync(
        Webhook webhook,
        string messageText,
        string requestId,
        Stopwatch stopwatch)
    {
        _logger.LogDebug("Sending message to Telegram for webhook '{WebhookUUID}'", webhook.Uuid);
        var telegramResult = await _telegramService.SendMessageAsync(webhook, messageText);

        await _loggingService.LogTelegramResponseAsync(requestId, telegramResult.IsSuccess,telegramResult.ResponseMessage);

        if (telegramResult.IsSuccess)
        {
            await _loggingService.CompleteRequestAsync(requestId, 200,
                JsonSerializer.Serialize(new { message = "Message forwarded successfully" }),
                (int)stopwatch.ElapsedMilliseconds);

            return WebhookProcessingResult.Success(new { message = "Message forwarded successfully" });
        }

        await _loggingService.CompleteRequestAsync(requestId, telegramResult.StatusCode,
            JsonSerializer.Serialize(new
                { message = "Telegram API error occurred", details = new[] { telegramResult.Error } }),
            (int)stopwatch.ElapsedMilliseconds);

        // Send failure notification
        _ = Task.Run(async () => await _failureNotificationService.SendFailureNotificationAsync(
            webhook.Name,
            "Telegram API Error",
            telegramResult.Error ?? "Unknown error",
            requestId));

        return WebhookProcessingResult.Failure("Telegram API error occurred", telegramResult.StatusCode);
    }
}