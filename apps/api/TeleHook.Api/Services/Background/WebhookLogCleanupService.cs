using TeleHook.Api.DTO;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Services.Background;

public class WebhookLogCleanupService : BackgroundService
{
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);
    private readonly AppSettingDto _appSetting;
    private readonly ILogger<WebhookLogCleanupService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public WebhookLogCleanupService(ILogger<WebhookLogCleanupService> logger,
        AppSettingDto appSetting,
        IServiceScopeFactory serviceScopeFactory)
    {
        _appSetting = appSetting;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Webhook log cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync(stoppingToken);
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Webhook log cleanup service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during webhook log cleanup");

                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Webhook log cleanup service stopped during error recovery");
                    break;
                }
            }
        }

        _logger.LogInformation("Webhook log cleanup service stopped");
    }

    private async Task PerformCleanupAsync(CancellationToken cancellationToken)
    {
        if (_appSetting.WebhookLogRetentionDays <= 0)
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var cutoffDate = DateTime.UtcNow.AddDays(-_appSetting.WebhookLogRetentionDays);

        _logger.LogDebug("Starting webhook log cleanup for logs older than {CutoffDate}", cutoffDate);

        var deletedCount = await unitOfWork.WebhookLogs.DeleteLogsOlderThanAsync(cutoffDate);

        if (deletedCount > 0)
        {
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {DeletedCount} webhook logs older than {CutoffDate}", deletedCount, cutoffDate);
        }
        else
        {
            _logger.LogDebug("No webhook logs found for cleanup");
        }
    }
}
