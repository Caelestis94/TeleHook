using System.Text.Json;
using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Domain;

public class PayloadCaptureManagementService : IPayloadCaptureManagementService
{
    private readonly IPayloadCaptureQueue _payloadCaptureQueue;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PayloadCaptureManagementService> _logger;

    public PayloadCaptureManagementService(IPayloadCaptureQueue payloadCaptureQueue,
        IUnitOfWork unitOfWork,
        ILogger<PayloadCaptureManagementService> logger)
    {
        _payloadCaptureQueue = payloadCaptureQueue;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private const string CaptureUrlFormat = "/api/payload/capture/{0}";

    public async Task<CaptureSessionDto> CreateSessionAsync(int userId)
    {
        _logger.LogDebug("Creating capture session for user {UserId}", userId);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            throw new BadRequestException($"User with {userId} not found");
        }

        var sessionId = _payloadCaptureQueue.CreateSession(userId);

        var session = _payloadCaptureQueue.GetSession(sessionId)!;

        _logger.LogInformation("Created session {SessionId}", sessionId);
        return new CaptureSessionDto
        {
            SessionId = sessionId,
            CaptureUrl = string.Format(CaptureUrlFormat, sessionId),
            ExpiresAt = session.ExpiresAt,
            CreatedAt = session.CreatedAt,
            Status = nameof(CaptureSessionStatus.Waiting)
        };
    }

    public Task<CaptureSessionDto> GetStatusAsync(string sessionId)
    {
        _logger.LogDebug("Getting status for session {SessionId}", sessionId);
        var session = _payloadCaptureQueue.GetSession(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found", sessionId);
            throw new NotFoundException($"Session", sessionId);
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Session {SessionId} expired", sessionId);
            _payloadCaptureQueue.RemoveSession(sessionId);
            throw new NotFoundException($"Session", sessionId);
        }

        var status = session.IsCompleted ? nameof(CaptureSessionStatus.Completed) : nameof(CaptureSessionStatus.Waiting);

        _logger.LogInformation("Session {SessionId} status: {Status}", sessionId, status);
        return Task.FromResult(new CaptureSessionDto
        {
            SessionId = sessionId,
            CaptureUrl = string.Format(CaptureUrlFormat, sessionId),
            ExpiresAt = session.ExpiresAt,
            CreatedAt = session.CreatedAt,
            Payload = session.CapturedPayload,
            Status = status,
        });
    }

    public Task<CaptureSessionDto> CancelSessionAsync(string sessionId)
    {
        var result = _payloadCaptureQueue.CancelSession(sessionId);
        _logger.LogDebug("Cancelling session {SessionId}, result: {Result}", sessionId, result);

        return result switch
        {
            SessionOperationResult.SessionCancelled => Task.FromResult(new CaptureSessionDto
            {
                SessionId = sessionId,
                CaptureUrl = string.Format(CaptureUrlFormat, sessionId),
                ExpiresAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Status = nameof(CaptureSessionStatus.Cancelled)
            }),
            SessionOperationResult.SessionNotFound => throw new NotFoundException($"Session", sessionId),
            SessionOperationResult.SessionAlreadyCompleted => throw new ConflictException($"Session {sessionId} is already completed"),
            _ => throw new InternalServerErrorException("Unexpected cancellation result")
        };
    }

    public Task<bool> CompleteSessionAsync(string sessionId, object payload)
    {
        if (!IsValidPayload(payload))
        {
            _logger.LogWarning("Invalid payload for session {SessionId}", sessionId);
            throw new BadRequestException("Invalid payload format or size");
        }

        var result = _payloadCaptureQueue.CompleteSession(sessionId, payload);

        switch (result)
        {
            case SessionOperationResult.Success:
                _logger.LogInformation("Session {SessionId} completed successfully", sessionId);
                return Task.FromResult(true);
            case SessionOperationResult.SessionNotFound:
                _logger.LogWarning("Session {SessionId} not found", sessionId);
                throw new NotFoundException($"Session", sessionId);
            case SessionOperationResult.SessionAlreadyCompleted:
                _logger.LogWarning("Session {SessionId} already completed", sessionId);
                throw new ConflictException($"Session {sessionId} is already completed");
            case SessionOperationResult.SessionExpired:
                _logger.LogWarning("Session {SessionId} expired", sessionId);
                throw new NotFoundException($"Session", sessionId);
            default:
                throw new InternalServerErrorException("Unexpected completion result");
        }
    }

    private bool IsValidPayload(object payload)
    {
        if (payload == null)
            return false;

        try
        {
            var json = JsonSerializer.Serialize(payload);

            // Check payload size (limit to 1MB)
            if (json.Length > 1024 * 1024)
            {
                _logger.LogWarning("Payload too large: {Size} bytes", json.Length);
                return false;
            }

            // Ensure it's valid JSON by attempting to parse it back
            JsonSerializer.Deserialize<object>(json);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Invalid payload: {Error}", ex.Message);
            return false;
        }
    }

    private enum CaptureSessionStatus
    {
        Waiting,
        Completed,
        Cancelled
    }
}
