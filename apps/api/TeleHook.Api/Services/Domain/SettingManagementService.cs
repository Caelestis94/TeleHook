using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Domain;

public class SettingManagementService : ISettingManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly AppSettingDto _appSetting;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<SettingManagementService> _logger;
    private readonly IFailureNotificationService _failureNotificationService;

    public SettingManagementService(IUnitOfWork unitOfWork,
        IValidationService validationService,
        AppSettingDto appSetting,
        IHostApplicationLifetime hostApplicationLifetime,
        IFailureNotificationService failureNotificationService,
        ILogger<SettingManagementService> logger)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _appSetting = appSetting;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
        _failureNotificationService = failureNotificationService;
    }

    public async Task<AppSetting> GetSettingsAsync()
    {
        try
        {
            AppSetting settings;

            var settingsExists = await _unitOfWork.AppSettings.SettingsExistAsync();

            if (!settingsExists)
            {
                _logger.LogWarning("Settings not found, returning default settings");
                settings = new AppSetting
                {
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await _unitOfWork.AppSettings.AddAsync(settings);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                settings = (await _unitOfWork.AppSettings.GetSettingsAsync())!;
            }

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get settings");
            throw;
        }
    }

    public async Task<SettingsUpdatedResponse> UpdateSettingsAsync(AppSettingDto updateSettingsRequest)
    {
        try
        {
            var validationResult = await _validationService.ValidateAsync(updateSettingsRequest);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Validation failed for setting update: {Errors}", string.Join(", ", errors));
                throw new ValidationException(errors);
            }

            var settings = await GetSettingsAsync();

            bool restartRequired = settings.LogLevel != updateSettingsRequest.LogLevel;
            var oldLogLevel = settings.LogLevel;


            settings.LogRetentionDays = updateSettingsRequest.LogRetentionDays;
            settings.LogLevel = updateSettingsRequest.LogLevel;
            settings.LogPath = updateSettingsRequest.LogPath;
            settings.EnableWebhookLogging = updateSettingsRequest.EnableWebhookLogging;
            settings.StatsDaysInterval = updateSettingsRequest.StatsDaysInterval;
            settings.WebhookLogRetentionDays = updateSettingsRequest.WebhookLogRetentionDays;
            settings.EnableFailureNotifications = updateSettingsRequest.EnableFailureNotifications;
            settings.NotificationBotToken = updateSettingsRequest.NotificationBotToken;
            settings.NotificationChatId = updateSettingsRequest.NotificationChatId;
            settings.NotificationTopicId = updateSettingsRequest.NotificationTopicId;
            settings.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.AppSettings.UpdateAsync(settings);
            await _unitOfWork.SaveChangesAsync();

            _appSetting.LogRetentionDays = updateSettingsRequest.LogRetentionDays;
            _appSetting.LogLevel = updateSettingsRequest.LogLevel;
            _appSetting.LogPath = updateSettingsRequest.LogPath;
            _appSetting.EnableWebhookLogging = updateSettingsRequest.EnableWebhookLogging;
            _appSetting.StatsDaysInterval = updateSettingsRequest.StatsDaysInterval;
            _appSetting.WebhookLogRetentionDays = updateSettingsRequest.WebhookLogRetentionDays;
            _appSetting.EnableFailureNotifications = updateSettingsRequest.EnableFailureNotifications;
            _appSetting.NotificationBotToken = updateSettingsRequest.NotificationBotToken;
            _appSetting.NotificationChatId = updateSettingsRequest.NotificationChatId;
            _appSetting.NotificationTopicId = updateSettingsRequest.NotificationTopicId;

            _logger.LogInformation("Settings updated successfully");

            if (restartRequired)
            {
                _logger.LogWarning("Log level changed from {OldLevel} to {NewLevel}, restarting application...",
                    oldLogLevel, updateSettingsRequest.LogLevel);

                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    _logger.LogInformation("Shutting down for restart...");
                    _hostApplicationLifetime.StopApplication();
                });
            }

            return new SettingsUpdatedResponse()
            {
                IsRestartRequired = restartRequired,
                Setting = settings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update settings");
            throw;
        }
    }

    public async Task<NotificationTestResult> TestNotificationAsync()
    {
        return await _failureNotificationService.TestNotificationAsync();
    }
}