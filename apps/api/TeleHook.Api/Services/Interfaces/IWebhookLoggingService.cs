namespace TeleHook.Api.Services.Interfaces;

public interface IWebhookLoggingService
{
    Task<string> StartRequestAsync(int webhookId, HttpRequest request);
    Task LogValidationResultAsync(string requestId, bool isValid, List<string>? errors = null);
    Task LogMessageFormattingAsync(string requestId, string formattedMessage);
    Task LogTelegramResponseAsync(string requestId, bool sent, string? telegramResponse = null);
    Task CompleteRequestAsync(string requestId, int statusCode, string? responseBody, int processingTimeMs);
}
