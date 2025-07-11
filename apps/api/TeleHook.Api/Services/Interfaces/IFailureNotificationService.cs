using TeleHook.Api.Models.Results;

namespace TeleHook.Api.Services.Interfaces;

public interface IFailureNotificationService
{
    Task SendFailureNotificationAsync(string webhookName, string failureType, string errorMessage, string? requestId = null);
    Task<bool> IsNotificationEnabledAsync();
    Task<NotificationTestResult> TestNotificationAsync();
}