using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Domain;

public class WebhookLoggingService : IWebhookLoggingService
{
    private readonly Dictionary<string, WebhookLog> _pendingLogs = new();
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WebhookLoggingService> _logger;
    private readonly IWebhookStatService _webhookStatService;
    private readonly AppSettingDto _appSetting;

    public WebhookLoggingService(IUnitOfWork unitOfWork,
        ILogger<WebhookLoggingService> logger,
        IWebhookStatService webhookStatService,
        AppSettingDto appSetting)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _webhookStatService = webhookStatService;
        _appSetting = appSetting;
    }

    public async Task<string> StartRequestAsync(int webhookId, HttpRequest request)
    {
        var requestId = Guid.NewGuid().ToString();

        try
        {
            var requestBody = "";
            if (request.Body.CanRead)
            {
                request.EnableBuffering();
                request.Body.Position = 0;

                using var reader = new StreamReader(request.Body, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();

                request.Body.Position = 0;
            }

            var headers = request.Headers
                .Where(h => !IsSensitiveHeader(h.Key))
                .ToDictionary(h => h.Key, h => h.Value.ToString());
            var headersJson = JsonSerializer.Serialize(headers);
            
            var sanitizedUrl = SanitizeUrl(request.Path, request.QueryString);

            var log = new WebhookLog
            {
                WebhookId = webhookId,
                RequestId = requestId,
                HttpMethod = request.Method,
                RequestUrl = sanitizedUrl,
                RequestHeaders = headersJson,
                RequestBody = requestBody,
                ResponseStatusCode = 0,
                ProcessingTimeMs = 0,
                PayloadValidated = true, 
                TelegramSent = false,
                CreatedAt = DateTime.UtcNow
            };

            _pendingLogs[requestId] = log;

            _logger.LogDebug("Started logging for request '{RequestId}' on webhook '{WebhookId}'",
                requestId, webhookId);

            return requestId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start request logging for webhook '{WebhookId}'", webhookId);
            throw new InternalServerErrorException($"Failed to start request logging for webhook '{webhookId}'", ex);
        }
    }

    [Obsolete("Part of payload against schema validation pipeline, no longer used.")]
    public async Task LogValidationResultAsync(string requestId, bool isValid, List<string>? errors = null)
    {
        try
        {
            if (_pendingLogs.TryGetValue(requestId, out var log))
            {
                log.PayloadValidated = true; // Default to true, schema/payload validation no longer a feature.
                if (errors?.Any() == true) log.ValidationErrors = JsonSerializer.Serialize(errors);
            }

            _logger.LogDebug("Logged validation result for request '{RequestId}': {IsValid}",
                requestId, isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log validation result for request '{RequestId}'", requestId);
        }

        await Task.CompletedTask;
    }

    public async Task LogMessageFormattingAsync(string requestId, string formattedMessage)
    {
        try
        {
            if (_pendingLogs.TryGetValue(requestId, out var log)) log.MessageFormatted = formattedMessage;

            _logger.LogDebug("Logged formatted message for request '{RequestId}'", requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log formatted message for request '{RequestId}'", requestId);
        }

        await Task.CompletedTask;
    }

    public async Task LogTelegramResponseAsync(string requestId, bool sent, string? telegramResponse = null)
    {
        try
        {
            if (_pendingLogs.TryGetValue(requestId, out var log))
            {
                log.TelegramSent = sent;
                log.TelegramResponse = telegramResponse;
            }

            _logger.LogDebug("Logged Telegram response for request '{RequestId}': {Sent}",
                requestId, sent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log Telegram response for request '{RequestId}'", requestId);
        }

        await Task.CompletedTask;
    }

    public async Task CompleteRequestAsync(string requestId, int statusCode, string? responseBody, int processingTimeMs)
    {
        try
        {
            _logger.LogDebug("Completing request logging for '{RequestId}' with status code {StatusCode}",
                requestId, statusCode);
            await _unitOfWork.BeginTransactionAsync();
            
            if (_pendingLogs.TryGetValue(requestId, out var log))
            {
                log.ResponseStatusCode = statusCode;
                log.ResponseBody = responseBody;
                log.ProcessingTimeMs = processingTimeMs;

                // If webhook logging is enabled, save the log
                if (_appSetting.EnableWebhookLogging)
                {
                    await _unitOfWork.WebhookLogs.AddAsync(log);
                    await _unitOfWork.SaveChangesAsync();
                }
                
                await _webhookStatService.UpdateStatsAsync(
                    log.WebhookId,
                    statusCode,
                    processingTimeMs,
                    log.PayloadValidated,
                    log.TelegramSent);

                _pendingLogs.Remove(requestId);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Completed logging for request '{RequestId}': {StatusCode} in {ProcessingTime}ms",
                    requestId, statusCode, processingTimeMs);
            }
            else
            {
                throw new InternalServerErrorException($"Request '{requestId}' not found in pending logs");
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Failed to complete request logging for '{RequestId}'", requestId);
            throw;
        }
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[] { "authorization", "cookie", "x-api-key", "x-auth-token" };
        return sensitiveHeaders.Contains(headerName.ToLowerInvariant());
    }

    private static string SanitizeUrl(string path, QueryString queryString)
    {
        if (!queryString.HasValue)
            return path;

        var query = QueryHelpers.ParseQuery(queryString.Value);
        var sanitizedQuery = new Dictionary<string, string>();

        foreach (var kvp in query)
        {
            if (IsSensitiveQueryParameter(kvp.Key))
            {
                sanitizedQuery[kvp.Key] = "[REDACTED]";
            }
            else
            {
                sanitizedQuery[kvp.Key] = kvp.Value.ToString();
            }
        }

        var sanitizedQueryString = string.Join("&", 
            sanitizedQuery.Select(kvp => $"{kvp.Key}={kvp.Value}"));

        return $"{path}?{sanitizedQueryString}";
    }

    private static bool IsSensitiveQueryParameter(string paramName)
    {
        var sensitiveParams = new[] { "secret_key", "api_key", "token", "key", "password" };
        return sensitiveParams.Contains(paramName.ToLowerInvariant());
    }
}