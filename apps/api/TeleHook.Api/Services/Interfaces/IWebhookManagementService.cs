using TeleHook.Api.DTO;
using TeleHook.Api.Models;

namespace TeleHook.Api.Services.Interfaces;

public interface IWebhookManagementService
{
    Task<IEnumerable<Webhook>> GetAllWebhooksAsync();
    Task<Webhook?> GetWebhookByIdAsync(int id);
    Task<Webhook> CreateWebhookAsync(CreateWebhookDto createWebhookRequest);
    Task<Webhook> UpdateWebhookAsync(int id, UpdateWebhookDto updateWebhookRequest);
    Task DeleteWebhookAsync(int id);
    Task<SecretKeyResult> GenerateSecretKeyAsync(GenerateSecretKeyDto generateSecretKeyRequest);
    SecretKeyResult GenerateSecretKeyForNewWebhook();
    Task<IEnumerable<WebhookLogExportResponse>> ExportWebhookLogsAsync();
}

public class SecretKeyResult
{
    public string Message { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
}
