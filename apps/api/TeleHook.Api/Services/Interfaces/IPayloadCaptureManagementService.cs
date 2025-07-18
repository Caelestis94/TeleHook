using TeleHook.Api.DTO;

namespace TeleHook.Api.Services.Interfaces;

public interface IPayloadCaptureManagementService
{
    Task<CaptureSessionDto> CreateSessionAsync(int userId);
    Task<CaptureSessionDto> GetStatusAsync(string sessionId);
    Task<CaptureSessionDto> CancelSessionAsync(string sessionId);
    Task<bool> CompleteSessionAsync(string sessionId, object payload);
}
