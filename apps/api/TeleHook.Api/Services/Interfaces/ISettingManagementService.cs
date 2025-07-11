using TeleHook.Api.DTO;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;

namespace TeleHook.Api.Services.Interfaces;

public interface ISettingManagementService
{
    Task<AppSetting> GetSettingsAsync();
    Task<SettingsUpdatedResponse> UpdateSettingsAsync(AppSettingDto updateSettingsRequest);
    Task<NotificationTestResult> TestNotificationAsync();
}