using Microsoft.Extensions.Logging;
using Moq;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Domain;
using TeleHook.Api.Services.Interfaces;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class PayloadCaptureManagementServiceTests
{
    private readonly Mock<IPayloadCaptureQueue> _mockPayloadCaptureQueue;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<PayloadCaptureManagementService>> _mockLogger;
    private readonly PayloadCaptureManagementService _service;

    public PayloadCaptureManagementServiceTests()
    {
        _mockPayloadCaptureQueue = new Mock<IPayloadCaptureQueue>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<PayloadCaptureManagementService>>();

        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);

        _service = new PayloadCaptureManagementService(
            _mockPayloadCaptureQueue.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateSessionAsync_WithValidUserId_ShouldReturnCaptureSessionDto()
    {
        // Arrange
        var userId = 123;
        var sessionId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, Username = "testuser" };
        var captureSession = new CaptureSession
        {
            Id = sessionId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsCompleted = false
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockPayloadCaptureQueue.Setup(x => x.CreateSession(userId))
            .Returns(sessionId);
        _mockPayloadCaptureQueue.Setup(x => x.GetSession(sessionId))
            .Returns(captureSession);

        // Act
        var result = await _service.CreateSessionAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sessionId, result.SessionId);
        Assert.Equal("/api/payload/capture/" + sessionId, result.CaptureUrl);
        Assert.Equal("Waiting", result.Status);
        Assert.Equal(captureSession.CreatedAt, result.CreatedAt);
        Assert.Equal(captureSession.ExpiresAt, result.ExpiresAt);
        Assert.Null(result.Payload);
    }

    [Fact]
    public async Task CreateSessionAsync_WithInvalidUserId_ShouldThrowBadRequestException()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _service.CreateSessionAsync(userId));

        Assert.Contains("999", exception.Message);
    }

    [Fact]
    public async Task GetStatusAsync_WithValidSessionId_ShouldReturnWaitingStatus()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var captureSession = new CaptureSession
        {
            Id = sessionId,
            UserId = 123,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsCompleted = false
        };

        _mockPayloadCaptureQueue.Setup(x => x.GetSession(sessionId))
            .Returns(captureSession);

        // Act
        var result = await _service.GetStatusAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sessionId, result.SessionId);
        Assert.Equal("Waiting", result.Status);
        Assert.Null(result.Payload);
    }

    [Fact]
    public async Task GetStatusAsync_WithCompletedSession_ShouldReturnCompletedStatusWithPayload()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var payload = new { message = "test payload" };
        var captureSession = new CaptureSession
        {
            Id = sessionId,
            UserId = 123,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsCompleted = true,
            CapturedPayload = payload
        };

        _mockPayloadCaptureQueue.Setup(x => x.GetSession(sessionId))
            .Returns(captureSession);

        // Act
        var result = await _service.GetStatusAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sessionId, result.SessionId);
        Assert.Equal("Completed", result.Status);
        Assert.Equal(payload, result.Payload);
    }

    [Fact]
    public async Task GetStatusAsync_WithInvalidSessionId_ShouldThrowNotFoundException()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        _mockPayloadCaptureQueue.Setup(x => x.GetSession(sessionId))
            .Returns((CaptureSession?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetStatusAsync(sessionId));

        Assert.Contains(sessionId, exception.Message);
    }

    [Fact]
    public async Task GetStatusAsync_WithExpiredSession_ShouldThrowNotFoundException()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var captureSession = new CaptureSession
        {
            Id = sessionId,
            UserId = 123,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
            IsCompleted = false
        };

        _mockPayloadCaptureQueue.Setup(x => x.GetSession(sessionId))
            .Returns(captureSession);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetStatusAsync(sessionId));

        Assert.Contains("not found", exception.Message);
        _mockPayloadCaptureQueue.Verify(x => x.RemoveSession(sessionId), Times.Once);
    }

    [Fact]
    public async Task CancelSessionAsync_WithValidSessionId_ShouldReturnCancelledSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        _mockPayloadCaptureQueue.Setup(x => x.CancelSession(sessionId))
            .Returns(SessionOperationResult.SessionCancelled);

        // Act
        var result = await _service.CancelSessionAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sessionId, result.SessionId);
        Assert.Equal("Cancelled", result.Status);
        Assert.True(result.ExpiresAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task CancelSessionAsync_WithInvalidSessionId_ShouldThrowNotFoundException()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        _mockPayloadCaptureQueue.Setup(x => x.CancelSession(sessionId))
            .Returns(SessionOperationResult.SessionNotFound);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CancelSessionAsync(sessionId));

        Assert.Contains(sessionId, exception.Message);
    }

    [Fact]
    public async Task CancelSessionAsync_WithCompletedSession_ShouldThrowConflictException()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        _mockPayloadCaptureQueue.Setup(x => x.CancelSession(sessionId))
            .Returns(SessionOperationResult.SessionAlreadyCompleted);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _service.CancelSessionAsync(sessionId));

        Assert.Contains("already completed", exception.Message);
    }

    [Fact]
    public async Task CompleteSessionAsync_WithValidPayload_ShouldReturnTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var payload = new { message = "test payload" };

        _mockPayloadCaptureQueue.Setup(x => x.CompleteSession(sessionId, payload))
            .Returns(SessionOperationResult.Success);

        // Act
        var result = await _service.CompleteSessionAsync(sessionId, payload);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CompleteSessionAsync_WithInvalidSessionId_ShouldThrowNotFoundException()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var payload = new { message = "test payload" };

        _mockPayloadCaptureQueue.Setup(x => x.CompleteSession(sessionId, payload))
            .Returns(SessionOperationResult.SessionNotFound);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CompleteSessionAsync(sessionId, payload));

        Assert.Contains(sessionId, exception.Message);
    }

    [Fact]
    public async Task CompleteSessionAsync_WithExpiredSession_ShouldThrowNotFoundException()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var payload = new { message = "test payload" };

        _mockPayloadCaptureQueue.Setup(x => x.CompleteSession(sessionId, payload))
            .Returns(SessionOperationResult.SessionExpired);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CompleteSessionAsync(sessionId, payload));

        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task CompleteSessionAsync_WithCompletedSession_ShouldThrowConflictException()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var payload = new { message = "test payload" };

        _mockPayloadCaptureQueue.Setup(x => x.CompleteSession(sessionId, payload))
            .Returns(SessionOperationResult.SessionAlreadyCompleted);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _service.CompleteSessionAsync(sessionId, payload));

        Assert.Contains("already completed", exception.Message);
    }

    [Fact]
    public async Task CompleteSessionAsync_WithNullPayload_ShouldThrowBadRequestException()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        object? payload = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _service.CompleteSessionAsync(sessionId, payload!));

        Assert.Contains("Invalid payload", exception.Message);
    }

    [Fact]
    public async Task CompleteSessionAsync_WithLargePayload_ShouldThrowBadRequestException()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        // Create a payload larger than 1MB
        var largeString = new string('x', 1024 * 1024 + 1);
        var payload = new { data = largeString };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _service.CompleteSessionAsync(sessionId, payload));

        Assert.Contains("Invalid payload", exception.Message);
    }

    [Fact]
    public async Task CompleteSessionAsync_WithCircularReference_ShouldThrowBadRequestException()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var payload = new TestObjectWithCircularReference();
        payload.Self = payload; // Create circular reference

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _service.CompleteSessionAsync(sessionId, payload));

        Assert.Contains("Invalid payload", exception.Message);
    }

    [Theory]
    [InlineData("simple string")]
    [InlineData(42)]
    [InlineData(true)]
    [InlineData(3.14)]
    public async Task CompleteSessionAsync_WithSimplePayloads_ShouldReturnTrue(object payload)
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();

        _mockPayloadCaptureQueue.Setup(x => x.CompleteSession(sessionId, payload))
            .Returns(SessionOperationResult.Success);

        // Act
        var result = await _service.CompleteSessionAsync(sessionId, payload);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CompleteSessionAsync_WithComplexValidPayload_ShouldReturnTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var payload = new
        {
            user = new { id = 123, name = "test" },
            data = new[] { 1, 2, 3 },
            timestamp = DateTime.UtcNow,
            metadata = new Dictionary<string, object>
            {
                { "source", "webhook" },
                { "version", 1.0 }
            }
        };

        _mockPayloadCaptureQueue.Setup(x => x.CompleteSession(sessionId, payload))
            .Returns(SessionOperationResult.Success);

        // Act
        var result = await _service.CompleteSessionAsync(sessionId, payload);

        // Assert
        Assert.True(result);
    }

    private class TestObjectWithCircularReference
    {
        public TestObjectWithCircularReference? Self { get; set; }
    }
}
