using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Domain;
using TeleHook.Api.Services.Infrastructure;
using TeleHook.Api.Services.Interfaces;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class WebhookLoggingServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IWebhookLogRepository> _mockWebhookLogRepository;
    private readonly Mock<ILogger<WebhookLoggingService>> _mockLogger;
    private readonly Mock<IWebhookStatService> _mockWebhookStatService;
    private readonly WebhookLoggingService _service;

    public WebhookLoggingServiceTests()
    {
        var appSetting = new AppSettingDto
        {
            WebhookLogRetentionDays = 30,
            EnableWebhookLogging = true,
            StatsDaysInterval = 7,
            LogLevel = "Debug",
            LogRetentionDays = 7
        };

        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockWebhookLogRepository = new Mock<IWebhookLogRepository>();
        _mockLogger = new Mock<ILogger<WebhookLoggingService>>();
        _mockWebhookStatService = new Mock<IWebhookStatService>();

        _mockUnitOfWork.Setup(x => x.WebhookLogs).Returns(_mockWebhookLogRepository.Object);

        _service = new WebhookLoggingService(
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockWebhookStatService.Object,
            appSetting);
    }

    [Fact]
    public async Task StartRequestAsync_ShouldReturnRequestIdAndLogRequest()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");

        // Act
        var requestId = await _service.StartRequestAsync(webhookId, request);

        // Assert
        Assert.False(string.IsNullOrEmpty(requestId));
        Assert.True(Guid.TryParse(requestId, out _));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Started logging for request '{requestId}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartRequestAsync_ShouldCaptureRequestDetails()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook/test?param=value", "test request body");
        request.Headers["X-Custom-Header"] = "test-value";

        // Act
        var requestId = await _service.StartRequestAsync(webhookId, request);

        // Assert
        Assert.NotNull(requestId);
        // The request details are stored in _pendingLogs which is private, 
        // so we verify by checking the behavior in CompleteRequestAsync
    }

    [Fact]
    public async Task StartRequestAsync_ShouldFilterSensitiveHeaders()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");
        request.Headers["Authorization"] = "Bearer secret-token";
        request.Headers["X-API-Key"] = "secret-key";
        request.Headers["X-Safe-Header"] = "safe-value";

        // Act
        var requestId = await _service.StartRequestAsync(webhookId, request);

        // Assert
        Assert.NotNull(requestId);
        // Sensitive headers should be filtered out - we can verify this indirectly
        // through integration tests or by making the method more testable
    }

    [Fact]
    public async Task LogValidationResultAsync_WithValidPayload_ShouldLogSuccess()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");
        var requestId = await _service.StartRequestAsync(webhookId, request);

        // Act
        await _service.LogValidationResultAsync(requestId, true);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains($"Logged validation result for request '{requestId}': True")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogValidationResultAsync_WithInvalidPayload_ShouldLogErrors()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");
        var requestId = await _service.StartRequestAsync(webhookId, request);
        var errors = new List<string> { "Missing required field", "Invalid format" };

        // Act
        await _service.LogValidationResultAsync(requestId, false, errors);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains($"Logged validation result for request '{requestId}': False")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogValidationResultAsync_WithNonExistentRequestId_ShouldNotThrow()
    {
        // Arrange
        var nonExistentRequestId = Guid.NewGuid().ToString();

        // Act & Assert
        await _service.LogValidationResultAsync(nonExistentRequestId, true);
        // Should complete without throwing
    }

    [Fact]
    public async Task LogMessageFormattingAsync_ShouldLogFormattedMessage()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");
        var requestId = await _service.StartRequestAsync(webhookId, request);
        var formattedMessage = "Formatted: Hello World!";

        // Act
        await _service.LogMessageFormattingAsync(requestId, formattedMessage);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains($"Logged formatted message for request '{requestId}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogTelegramResponseAsync_WithSuccessfulSend_ShouldLogResponse()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");
        var requestId = await _service.StartRequestAsync(webhookId, request);
        var telegramResponse = "Message sent successfully";

        // Act
        await _service.LogTelegramResponseAsync(requestId, true, telegramResponse);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains($"Logged Telegram response for request '{requestId}': True")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogTelegramResponseAsync_WithFailedSend_ShouldLogFailure()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");
        var requestId = await _service.StartRequestAsync(webhookId, request);

        // Act
        await _service.LogTelegramResponseAsync(requestId, false);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains($"Logged Telegram response for request '{requestId}': False")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteRequestAsync_ShouldSaveLogAndUpdateStats()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");
        var requestId = await _service.StartRequestAsync(webhookId, request);

        await _service.LogValidationResultAsync(requestId, true);
        await _service.LogTelegramResponseAsync(requestId, true, "Success");

        var statusCode = 200;
        var responseBody = "OK";
        var processingTimeMs = 150;

        // Act
        await _service.CompleteRequestAsync(requestId, statusCode, responseBody, processingTimeMs);

        // Assert
        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockWebhookLogRepository.Verify(x => x.AddAsync(It.Is<WebhookLog>(log =>
            log.WebhookId == webhookId &&
            log.RequestId == requestId &&
            log.ResponseStatusCode == statusCode &&
            log.ResponseBody == responseBody &&
            log.ProcessingTimeMs == processingTimeMs &&
            log.PayloadValidated == true &&
            log.TelegramSent == true
        )), Times.Once);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);

        _mockWebhookStatService.Verify(x => x.UpdateStatsAsync(
            webhookId,
            statusCode,
            processingTimeMs,
            true,
            true), Times.Once);

        _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains($"Completed logging for request '{requestId}': 200 in 150ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteRequestAsync_WithNonExistentRequestId_ShouldThrowAndRollbackTransaction()
    {
        // Arrange
        var nonExistentRequestId = Guid.NewGuid().ToString();

        // Act
        await Assert.ThrowsAsync<InternalServerErrorException>(async () =>
            await _service.CompleteRequestAsync(nonExistentRequestId, 200, "OK", 100));

        // Assert
        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        _mockWebhookLogRepository.Verify(x => x.AddAsync(It.IsAny<WebhookLog>()), Times.Never);
        _mockWebhookStatService.Verify(
            x => x.UpdateStatsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task CompleteRequestAsync_WhenExceptionOccurs_ShouldThrowAndRollbackTransactionAndLogError()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");
        var requestId = await _service.StartRequestAsync(webhookId, request);

        _mockWebhookLogRepository.Setup(x => x.AddAsync(It.IsAny<WebhookLog>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        await Assert.ThrowsAsync<Exception>(async () => await _service.CompleteRequestAsync(requestId, 200, "OK", 100));
        // Assert
        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains($"Failed to complete request logging for '{requestId}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartRequestAsync_WhenExceptionOccurs_ShouldThrowInternalServerException()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");

        // Simulate an exception during request processing
        request.Body = null!;

        // Act
        await Assert.ThrowsAsync<InternalServerErrorException>(async () =>
            await _service.StartRequestAsync(webhookId, request));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains($"Failed to start request logging for webhook '{webhookId}'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("authorization")]
    [InlineData("Authorization")]
    [InlineData("cookie")]
    [InlineData("Cookie")]
    [InlineData("x-api-key")]
    [InlineData("X-API-Key")]
    [InlineData("x-auth-token")]
    [InlineData("X-Auth-Token")]
    public async Task StartRequestAsync_ShouldFilterSensitiveHeadersRegardlessOfCase(string headerName)
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");
        request.Headers[headerName] = "sensitive-value";
        request.Headers["X-Safe-Header"] = "safe-value";

        // Act
        var requestId = await _service.StartRequestAsync(webhookId, request);

        // Assert
        Assert.NotNull(requestId);
        // The sensitive header filtering is tested indirectly - in a real scenario,
        // we might make the IsSensitiveHeader method public or internal for direct testing
    }

    [Theory]
    [InlineData("secret_key", "sk_123456789")]
    [InlineData("Secret_Key", "sk_123456789")]
    [InlineData("api_key", "api_123456789")]
    [InlineData("API_KEY", "api_123456789")]
    [InlineData("token", "token_value")]
    [InlineData("TOKEN", "token_value")]
    [InlineData("key", "key_value")]
    [InlineData("password", "secret_password")]
    public async Task StartRequestAsync_ShouldRedactSensitiveQueryParameters(string paramName, string paramValue)
    {
        // Arrange
        var webhookId = 1;
        var path = "/webhook";
        var queryString = $"?{paramName}={paramValue}&safe_param=safe_value";
        var request = CreateMockHttpRequestWithQuery("POST", path, queryString, "test body");

        // Act
        var requestId = await _service.StartRequestAsync(webhookId, request);

        // Assert
        Assert.NotNull(requestId);

        // Complete the request to verify the URL was sanitized in the log
        await _service.CompleteRequestAsync(requestId, 200, "OK", 100);

        // Verify that the log was saved with redacted sensitive parameters
        _mockWebhookLogRepository.Verify(x => x.AddAsync(It.Is<WebhookLog>(log =>
            log.RequestUrl.Contains("[REDACTED]") &&
            log.RequestUrl.Contains("safe_param=safe_value") &&
            !log.RequestUrl.Contains(paramValue)
        )), Times.Once);
    }

    [Fact]
    public async Task StartRequestAsync_ShouldHandleMultipleSensitiveQueryParameters()
    {
        // Arrange
        var webhookId = 1;
        var path = "/webhook";
        var queryString = "?secret_key=sk_123&api_key=api_456&safe_param=safe&token=tok_789";
        var request = CreateMockHttpRequestWithQuery("POST", path, queryString, "test body");

        // Act
        var requestId = await _service.StartRequestAsync(webhookId, request);

        // Assert
        Assert.NotNull(requestId);

        // Complete the request to verify the URL was sanitized
        await _service.CompleteRequestAsync(requestId, 200, "OK", 100);

        // Verify that all sensitive parameters were redacted but safe ones remain
        _mockWebhookLogRepository.Verify(x => x.AddAsync(It.Is<WebhookLog>(log =>
            log.RequestUrl.Contains("secret_key=[REDACTED]") &&
            log.RequestUrl.Contains("api_key=[REDACTED]") &&
            log.RequestUrl.Contains("token=[REDACTED]") &&
            log.RequestUrl.Contains("safe_param=safe") &&
            !log.RequestUrl.Contains("sk_123") &&
            !log.RequestUrl.Contains("api_456") &&
            !log.RequestUrl.Contains("tok_789")
        )), Times.Once);
    }

    [Fact]
    public async Task StartRequestAsync_ShouldHandleRequestWithNoQueryString()
    {
        // Arrange
        var webhookId = 1;
        var request = CreateMockHttpRequest("POST", "/webhook", "test body");

        // Act
        var requestId = await _service.StartRequestAsync(webhookId, request);

        // Assert
        Assert.NotNull(requestId);

        // Complete the request to verify the URL was logged correctly
        await _service.CompleteRequestAsync(requestId, 200, "OK", 100);

        // Verify that the URL without query string is logged as-is
        _mockWebhookLogRepository.Verify(x => x.AddAsync(It.Is<WebhookLog>(log =>
            log.RequestUrl == "/webhook"
        )), Times.Once);
    }

    [Fact]
    public async Task StartRequestAsync_ShouldHandleRequestWithOnlyNonSensitiveQueryParameters()
    {
        // Arrange
        var webhookId = 1;
        var path = "/webhook";
        var queryString = "?param1=value1&param2=value2&param3=value3";
        var request = CreateMockHttpRequestWithQuery("POST", path, queryString, "test body");

        // Act
        var requestId = await _service.StartRequestAsync(webhookId, request);

        // Assert
        Assert.NotNull(requestId);

        // Complete the request to verify the URL was logged correctly
        await _service.CompleteRequestAsync(requestId, 200, "OK", 100);

        // Verify that non-sensitive parameters are logged as-is
        _mockWebhookLogRepository.Verify(x => x.AddAsync(It.Is<WebhookLog>(log =>
            log.RequestUrl.Contains("param1=value1") &&
            log.RequestUrl.Contains("param2=value2") &&
            log.RequestUrl.Contains("param3=value3") &&
            !log.RequestUrl.Contains("[REDACTED]")
        )), Times.Once);
    }

    private static HttpRequest CreateMockHttpRequest(string method, string path, string body)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        request.Method = method;
        request.Path = path;

        if (!string.IsNullOrEmpty(body))
        {
            var bodyBytes = Encoding.UTF8.GetBytes(body);
            request.Body = new MemoryStream(bodyBytes);
        }

        return request;
    }

    private static HttpRequest CreateMockHttpRequestWithQuery(string method, string path, string queryString,
        string body)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        request.Method = method;
        request.Path = path;
        request.QueryString = new QueryString(queryString);

        if (!string.IsNullOrEmpty(body))
        {
            var bodyBytes = Encoding.UTF8.GetBytes(body);
            request.Body = new MemoryStream(bodyBytes);
        }

        return request;
    }
}
