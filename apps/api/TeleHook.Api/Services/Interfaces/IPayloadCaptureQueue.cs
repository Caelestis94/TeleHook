using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;

namespace TeleHook.Api.Services.Interfaces;

public interface IPayloadCaptureQueue
{
    string CreateSession(int userId);
    CaptureSession? GetSession(string sessionId);
    SessionOperationResult CompleteSession(string sessionId, object payload);
    SessionOperationResult CancelSession(string sessionId);
    IEnumerable<CaptureSession> GetExpiredSessions();
    bool RemoveSession(string sessionId);
    void RemoveExpiredSessions();
}
