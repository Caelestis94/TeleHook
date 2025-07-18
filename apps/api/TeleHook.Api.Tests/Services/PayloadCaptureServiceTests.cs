using TeleHook.Api.Models.Results;
using TeleHook.Api.Services.Infrastructure;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class PayloadCaptureServiceTests
{
    private readonly PayloadCaptureService _service;

    public PayloadCaptureServiceTests()
    {
        _service = new PayloadCaptureService();
    }

    [Fact]
    public void CreateSession_ShouldReturnSessionId()
    {
        // Arrange
        var userId = 123;

        // Act
        var sessionId = _service.CreateSession(userId);

        // Assert
        Assert.False(string.IsNullOrEmpty(sessionId));
        Assert.True(Guid.TryParse(sessionId, out _));
    }

    [Fact]
    public void CreateSession_ShouldCreateSessionWithCorrectProperties()
    {
        // Arrange
        var userId = 123;
        var beforeCreate = DateTime.UtcNow;

        // Act
        var sessionId = _service.CreateSession(userId);
        var session = _service.GetSession(sessionId);

        // Assert
        Assert.NotNull(session);
        Assert.Equal(sessionId, session.Id);
        Assert.Equal(userId, session.UserId);
        Assert.False(session.IsCompleted);
        Assert.True(session.CreatedAt >= beforeCreate);
        Assert.True(session.ExpiresAt > session.CreatedAt);
        Assert.Equal(5, Math.Round((session.ExpiresAt - session.CreatedAt).TotalMinutes));
    }

    [Fact]
    public void GetSession_WithInvalidSessionId_ShouldReturnNull()
    {
        // Arrange
        var invalidSessionId = Guid.NewGuid().ToString();

        // Act
        var session = _service.GetSession(invalidSessionId);

        // Assert
        Assert.Null(session);
    }

    [Fact]
    public void CompleteSession_WithValidSession_ShouldReturnSuccess()
    {
        // Arrange
        var userId = 123;
        var payload = new { message = "test payload" };
        var sessionId = _service.CreateSession(userId);

        // Act
        var result = _service.CompleteSession(sessionId, payload);

        // Assert
        Assert.Equal(SessionOperationResult.Success, result);

        var session = _service.GetSession(sessionId);
        Assert.NotNull(session);
        Assert.True(session.IsCompleted);
        Assert.Equal(payload, session.CapturedPayload);
    }

    [Fact]
    public void CompleteSession_WithInvalidSessionId_ShouldReturnSessionNotFound()
    {
        // Arrange
        var invalidSessionId = Guid.NewGuid().ToString();
        var payload = new { message = "test payload" };

        // Act
        var result = _service.CompleteSession(invalidSessionId, payload);

        // Assert
        Assert.Equal(SessionOperationResult.SessionNotFound, result);
    }

    [Fact]
    public void CompleteSession_WithExpiredSession_ShouldReturnSessionExpired()
    {
        // Arrange
        var userId = 123;
        var payload = new { message = "test payload" };
        var sessionId = _service.CreateSession(userId);

        // Manually expire the session
        var session = _service.GetSession(sessionId);
        session!.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        // Act
        var result = _service.CompleteSession(sessionId, payload);

        // Assert
        Assert.Equal(SessionOperationResult.SessionExpired, result);

        // Session should be removed
        var removedSession = _service.GetSession(sessionId);
        Assert.Null(removedSession);
    }

    [Fact]
    public void CompleteSession_WithAlreadyCompletedSession_ShouldReturnSessionAlreadyCompleted()
    {
        // Arrange
        var userId = 123;
        var payload1 = new { message = "first payload" };
        var payload2 = new { message = "second payload" };
        var sessionId = _service.CreateSession(userId);

        // Complete the session first time
        _service.CompleteSession(sessionId, payload1);

        // Act - try to complete again
        var result = _service.CompleteSession(sessionId, payload2);

        // Assert
        Assert.Equal(SessionOperationResult.SessionAlreadyCompleted, result);

        // Verify original payload is preserved
        var session = _service.GetSession(sessionId);
        Assert.NotNull(session);
        Assert.Equal(payload1, session.CapturedPayload);
    }

    [Fact]
    public void CancelSession_WithValidSession_ShouldReturnSessionCancelled()
    {
        // Arrange
        var userId = 123;
        var sessionId = _service.CreateSession(userId);

        // Act
        var result = _service.CancelSession(sessionId);

        // Assert
        Assert.Equal(SessionOperationResult.SessionCancelled, result);

        // Session should be removed
        var session = _service.GetSession(sessionId);
        Assert.Null(session);
    }

    [Fact]
    public void CancelSession_WithInvalidSessionId_ShouldReturnSessionNotFound()
    {
        // Arrange
        var invalidSessionId = Guid.NewGuid().ToString();

        // Act
        var result = _service.CancelSession(invalidSessionId);

        // Assert
        Assert.Equal(SessionOperationResult.SessionNotFound, result);
    }

    [Fact]
    public void CancelSession_WithCompletedSession_ShouldReturnSessionAlreadyCompleted()
    {
        // Arrange
        var userId = 123;
        var payload = new { message = "test payload" };
        var sessionId = _service.CreateSession(userId);
        _service.CompleteSession(sessionId, payload);

        // Act
        var result = _service.CancelSession(sessionId);

        // Assert
        Assert.Equal(SessionOperationResult.SessionAlreadyCompleted, result);

        // Session should still exist
        var session = _service.GetSession(sessionId);
        Assert.NotNull(session);
        Assert.True(session.IsCompleted);
    }

    [Fact]
    public void RemoveSession_WithValidSession_ShouldReturnTrueAndRemoveSession()
    {
        // Arrange
        var userId = 123;
        var sessionId = _service.CreateSession(userId);

        // Act
        var result = _service.RemoveSession(sessionId);

        // Assert
        Assert.True(result);

        var session = _service.GetSession(sessionId);
        Assert.Null(session);
    }

    [Fact]
    public void RemoveSession_WithInvalidSessionId_ShouldReturnFalse()
    {
        // Arrange
        var invalidSessionId = Guid.NewGuid().ToString();

        // Act
        var result = _service.RemoveSession(invalidSessionId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetExpiredSessions_ShouldReturnOnlyExpiredSessions()
    {
        // Arrange
        var userId = 123;
        var validSessionId = _service.CreateSession(userId);
        var expiredSessionId = _service.CreateSession(userId);

        // Manually expire one session
        var expiredSession = _service.GetSession(expiredSessionId);
        expiredSession!.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        // Act
        var expiredSessions = _service.GetExpiredSessions().ToList();

        // Assert
        Assert.Single(expiredSessions);
        Assert.Equal(expiredSessionId, expiredSessions[0].Id);
    }

    [Fact]
    public void RemoveExpiredSessions_ShouldRemoveOnlyExpiredSessions()
    {
        // Arrange
        var userId = 123;
        var validSessionId = _service.CreateSession(userId);
        var expiredSessionId = _service.CreateSession(userId);

        // Manually expire one session
        var expiredSession = _service.GetSession(expiredSessionId);
        expiredSession!.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        // Act
        _service.RemoveExpiredSessions();

        // Assert
        var validSession = _service.GetSession(validSessionId);
        var removedSession = _service.GetSession(expiredSessionId);

        Assert.NotNull(validSession);
        Assert.Null(removedSession);
    }

    [Fact]
    public void ConcurrentOperations_ShouldNotCauseRaceConditions()
    {
        // Arrange
        var userId = 123;
        var sessionId = _service.CreateSession(userId);
        var payload = new { message = "test payload" };

        var tasks = new List<Task<SessionOperationResult>>();

        // Act - try to complete the same session multiple times concurrently
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => _service.CompleteSession(sessionId, payload)));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        var results = tasks.Select(t => t.Result).ToList();

        // Only one should succeed
        Assert.Single(results.Where(r => r == SessionOperationResult.Success));

        // Others should return SessionAlreadyCompleted
        Assert.Equal(9, results.Count(r => r == SessionOperationResult.SessionAlreadyCompleted));

        // Session should be completed
        var session = _service.GetSession(sessionId);
        Assert.NotNull(session);
        Assert.True(session.IsCompleted);
    }
}
