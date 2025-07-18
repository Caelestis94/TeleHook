using System.Collections.Concurrent;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Infrastructure;

public class PayloadCaptureService : IPayloadCaptureQueue
{
    private readonly ConcurrentDictionary<string, CaptureSession> _sessions = new();


    public string CreateSession(int userId)
    {
        var sessionId = Guid.NewGuid().ToString();

        var session = new CaptureSession
        {
            Id = sessionId,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        _sessions.GetOrAdd(sessionId, session);

        return sessionId;
    }

    public CaptureSession? GetSession(string sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var session) ? session : null;
    }

    public SessionOperationResult CompleteSession(string sessionId, object payload)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return SessionOperationResult.SessionNotFound;

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _sessions.TryRemove(sessionId, out _);
            return SessionOperationResult.SessionExpired;
        }

        if (session.IsCompleted)
            return SessionOperationResult.SessionAlreadyCompleted;

        session.CapturedPayload = payload;
        session.IsCompleted = true;

        return SessionOperationResult.Success;
    }

    public SessionOperationResult CancelSession(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return SessionOperationResult.SessionNotFound;

        if (session.IsCompleted)
            return SessionOperationResult.SessionAlreadyCompleted;

        _sessions.TryRemove(sessionId, out _);

        return SessionOperationResult.SessionCancelled;
    }

    public bool RemoveSession(string sessionId)
    {
        return _sessions.TryRemove(sessionId, out _);
    }

    public IEnumerable<CaptureSession> GetExpiredSessions()
    {
        var now = DateTime.UtcNow;
        return _sessions.Values
            .Where(session => session.ExpiresAt < now)
            .ToList();
    }

    public void RemoveExpiredSessions()
    {
        var now = DateTime.UtcNow;
        var expiredSessionIds = _sessions
            .Where(kvp => kvp.Value.ExpiresAt < now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var sessionId in expiredSessionIds)
        {
            _sessions.TryRemove(sessionId, out _);
        }
    }

}

