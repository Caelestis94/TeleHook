using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Background;

public class PayloadCaptureCleanupService : BackgroundService
{
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);
    private readonly IPayloadCaptureQueue _payloadCaptureQueue;
    private readonly ILogger<PayloadCaptureCleanupService> _logger;

    public PayloadCaptureCleanupService(IPayloadCaptureQueue payloadCaptureQueue,
        ILogger<PayloadCaptureCleanupService> logger)
    {
        _payloadCaptureQueue = payloadCaptureQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payload capture cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var expiredSessions = _payloadCaptureQueue.GetExpiredSessions().ToList();

                if (expiredSessions.Any())
                {
                    _logger.LogInformation("Cleaning up {Count} expired sessions", expiredSessions.Count);
                    _payloadCaptureQueue.RemoveExpiredSessions();
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Payload capture cleanup service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during payload capture cleanup");

                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Payload capture cleanup service stopped during error recovery");
                    break;
                }
            }
        }

        _logger.LogInformation("Payload capture cleanup service stopped");
    }
}
