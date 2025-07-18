using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using TeleHook.Api.DTO;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Domain;
using TeleHook.Api.Services.Interfaces;
using Xunit;

namespace TeleHook.Api.Tests.Services;

/// <summary>
/// Comprehensive tests for the WebhookProcessingService
/// This is the main functionality of the entire application - the meat and potatoes!
/// </summary>
public class WebhookProcessingTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IWebhookRepository> _mockWebhookRepository;
    private readonly Mock<IMessageFormattingService> _mockMessageFormattingService;
    private readonly Mock<ITelegramService> _mockTelegramService;
    private readonly Mock<IWebhookLoggingService> _mockLoggingService;
    private readonly Mock<IFailureNotificationService> _mockFailureNotificationService;
    private readonly Mock<ILogger<WebhookProcessingService>> _mockLogger;
    private readonly WebhookProcessingService _service;

    public WebhookProcessingTests()
    {
        var appSetting = new AppSettingDto
        {
            LogLevel = "Information",
            LogRetentionDays = 7,
            EnableWebhookLogging = true,
            WebhookLogRetentionDays = 30,
            StatsDaysInterval = 30,
            EnableFailureNotifications = true,
            NotificationBotToken = "your-telegram-bot-token",
            NotificationTopicId = "10",
            NotificationChatId = "123456789",
        };


        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockWebhookRepository = new Mock<IWebhookRepository>();
        _mockMessageFormattingService = new Mock<IMessageFormattingService>();
        _mockTelegramService = new Mock<ITelegramService>();
        _mockLoggingService = new Mock<IWebhookLoggingService>();
        _mockFailureNotificationService = new Mock<IFailureNotificationService>();
        _mockLogger = new Mock<ILogger<WebhookProcessingService>>();

        // Setup unit of work to return the mocked repository
        _mockUnitOfWork.Setup(x => x.Webhooks).Returns(_mockWebhookRepository.Object);

        // Make the logger completely passive - setup all logging methods to do nothing
        _mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Verifiable();

        // Setup IsEnabled to return false for all log levels (optional - makes logging even more passive)
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(false);


        _service = new WebhookProcessingService(
            _mockUnitOfWork.Object,
            _mockMessageFormattingService.Object,
            _mockTelegramService.Object,
            _mockLoggingService.Object,
            _mockFailureNotificationService.Object,
            _mockLogger.Object
        );
    }

    private Mock<HttpRequest> CreateMockHttpRequest(string method = "POST")
    {
        var httpRequest = new Mock<HttpRequest>();
        var httpContext = new Mock<HttpContext>();
        var connection = new Mock<ConnectionInfo>();
        var headers = new HeaderDictionary();
        var query = new QueryCollection();

        httpRequest.Setup(x => x.Method).Returns(method);
        httpRequest.Setup(x => x.Headers).Returns(headers);
        httpRequest.Setup(x => x.Query).Returns(query);
        httpRequest.Setup(x => x.HttpContext).Returns(httpContext.Object);
        httpContext.Setup(x => x.Connection).Returns(connection.Object);

        return httpRequest;
    }

    #region Core Webhook Processing Tests

    [Fact]
    public async Task ProcessWebhookAsync_WithValidPayload_ShouldReturnSuccessResult()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440000";
        var payloadJson = """{"event": "test", "data": {"message": "Hello World"}}""";
        var formattedMessage = "Test Event: Hello World";
        var requestId = "test-request-id-123";

        var bot = new Bot
        {
            Id = 1,
            Name = "Test Config",
            BotToken = "test-token",
            ChatId = "123456789"
        };

        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            Uuid = uuid,
            BotId = bot.Id,
            MessageTemplate = "Event: {{ event }}",
            ParseMode = "MarkdownV2",
            IsDisabled = false,
            IsProtected = false,
            Bot = bot,
        };

        // Setup repository to return the webhook
        _mockWebhookRepository.Setup(x => x.GetByUuidWithRelationsAsync(uuid))
            .ReturnsAsync(webhook);

        // Setup message formatting
        _mockMessageFormattingService.Setup(x => x.FormatMessage(webhook, It.IsAny<JsonElement>()))
            .Returns(MessageFormattingResult.Success(formattedMessage));

        // Setup logging service
        _mockLoggingService.Setup(x => x.StartRequestAsync(webhook.Id, It.IsAny<HttpRequest>()))
            .ReturnsAsync(requestId);
        _mockLoggingService.Setup(x => x.LogValidationResultAsync(requestId, true, It.IsAny<List<string>>()))
            .Returns(Task.CompletedTask);
        _mockLoggingService.Setup(x => x.LogMessageFormattingAsync(requestId, formattedMessage))
            .Returns(Task.CompletedTask);
        _mockLoggingService.Setup(x => x.LogTelegramResponseAsync(requestId, true, It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockLoggingService.Setup(x => x.CompleteRequestAsync(requestId, 200, It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Setup Telegram service to succeed
        _mockTelegramService.Setup(x => x.SendMessageAsync(webhook, formattedMessage))
            .ReturnsAsync(TelegramResult.Success());

        var httpRequest = CreateMockHttpRequest();

        // Act
        var result = await _service.ProcessWebhookAsync(uuid, JsonDocument.Parse(payloadJson).RootElement, httpRequest.Object);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Response);

        // Check that the result contains the expected message structure
        var messageProperty = result.Response.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal("Message forwarded successfully", messageProperty.GetValue(result.Response));

        // Verify all services were called correctly
        _mockWebhookRepository.Verify(x => x.GetByUuidWithRelationsAsync(uuid), Times.Once);
        _mockMessageFormattingService.Verify(x => x.FormatMessage(webhook, It.IsAny<JsonElement>()), Times.Once);
        _mockTelegramService.Verify(x => x.SendMessageAsync(webhook, formattedMessage), Times.Once);
        _mockLoggingService.Verify(x => x.StartRequestAsync(webhook.Id, It.IsAny<HttpRequest>()), Times.Once);
        _mockLoggingService.Verify(x => x.CompleteRequestAsync(requestId, 200, It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }


    [Fact]
    public async Task ProcessWebhookAsync_WithInvalidUuid_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidUuid = "non-existent-uuid"; // Invalid GUID format
        var httpRequest = CreateMockHttpRequest();

        // Act
        var result = await _service.ProcessWebhookAsync(invalidUuid, new JsonElement(), httpRequest.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Invalid UUID format", result.Error);

        // Repository should not be called for invalid UUID format
        _mockWebhookRepository.Verify(x => x.GetByUuidWithRelationsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithNonExistentValidUuid_ShouldReturnNotFound()
    {
        // Arrange
        var validButNonExistentUuid = "550e8400-e29b-41d4-a716-446655440999";

        _mockWebhookRepository.Setup(x => x.GetByUuidWithRelationsAsync(validButNonExistentUuid))
            .ReturnsAsync((Webhook?)null);

        var httpRequest = CreateMockHttpRequest();

        // Act
        var result = await _service.ProcessWebhookAsync(validButNonExistentUuid, new JsonElement(), httpRequest.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("was not found", result.Error);

        _mockWebhookRepository.Verify(x => x.GetByUuidWithRelationsAsync(validButNonExistentUuid), Times.Once);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithDisabledWebhook_ShouldReturnBadRequest()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440001";
        var requestId = "test-request-id";

        var webhook = new Webhook
        {
            Id = 1,
            Name = "Disabled Webhook",
            Uuid = uuid,
            IsDisabled = true,
            IsProtected = false,
        };

        _mockWebhookRepository.Setup(x => x.GetByUuidWithRelationsAsync(uuid))
            .ReturnsAsync(webhook);

        _mockLoggingService.Setup(x => x.StartRequestAsync(webhook.Id, It.IsAny<HttpRequest>()))
            .ReturnsAsync(requestId);
        _mockLoggingService.Setup(x => x.CompleteRequestAsync(requestId, 400, It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var httpRequest = CreateMockHttpRequest();

        // Act
        var result = await _service.ProcessWebhookAsync(uuid, new JsonElement(), httpRequest.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Webhook is disabled", result.Error);

        _mockWebhookRepository.Verify(x => x.GetByUuidWithRelationsAsync(uuid), Times.Once);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithTelegramServiceFailure_ShouldReturnInternalServerError()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440003";
        var payloadJson = """{"event": "test", "data": {"message": "Hello World"}}""";
        var formattedMessage = "Test Event: Hello World";
        var requestId = "test-request-id-789";

        var bot = new Bot
        {
            Id = 1,
            Name = "Test Config",
            BotToken = "test-token",
            ChatId = "123456789"
        };

        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            Uuid = uuid,
            BotId = bot.Id,
            MessageTemplate = "Event: {{ event }}",
            IsDisabled = false,
            IsProtected = false,
            Bot = bot,
        };

        // Setup repository to return the webhook
        _mockWebhookRepository.Setup(x => x.GetByUuidWithRelationsAsync(uuid))
            .ReturnsAsync(webhook);

        // Setup message formatting
        _mockMessageFormattingService.Setup(x => x.FormatMessage(webhook, It.IsAny<JsonElement>()))
            .Returns(MessageFormattingResult.Success(formattedMessage));

        // Setup logging service
        _mockLoggingService.Setup(x => x.StartRequestAsync(webhook.Id, It.IsAny<HttpRequest>()))
            .ReturnsAsync(requestId);
        _mockLoggingService.Setup(x => x.LogValidationResultAsync(requestId, true, It.IsAny<List<string>>()))
            .Returns(Task.CompletedTask);
        _mockLoggingService.Setup(x => x.LogMessageFormattingAsync(requestId, formattedMessage))
            .Returns(Task.CompletedTask);
        _mockLoggingService.Setup(x => x.CompleteRequestAsync(requestId, 502, It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Setup Telegram service to return false (failure)
        _mockTelegramService.Setup(x => x.SendMessageAsync(webhook, formattedMessage))
            .ReturnsAsync(TelegramResult.Failure("Failed to send message to Telegram", 502));

        var httpRequest = CreateMockHttpRequest();

        // Act
        var result = await _service.ProcessWebhookAsync(uuid, JsonDocument.Parse(payloadJson).RootElement, httpRequest.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(502, result.StatusCode);
        Assert.Equal("Telegram API error occurred", result.Error);

        // Verify all steps up to Telegram were called
        _mockMessageFormattingService.Verify(x => x.FormatMessage(webhook, It.IsAny<JsonElement>()), Times.Once);
        _mockTelegramService.Verify(x => x.SendMessageAsync(webhook, formattedMessage), Times.Once);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithProtectedWebhookAndValidKey_ShouldReturnSuccess()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440010";
        var secretKey = "my-secret-key";
        var payloadJson = """{"event": "test"}""";
        var formattedMessage = "Test message";
        var requestId = "test-request-id";

        var bot = new Bot
        {
            Id = 1,
            Name = "Test Config",
            BotToken = "test-token",
            ChatId = "123456789"
        };

        var webhook = new Webhook
        {
            Id = 1,
            Name = "Protected Webhook",
            Uuid = uuid,
            BotId = bot.Id,
            MessageTemplate = "Event: {{ event }}",
            IsDisabled = false,
            IsProtected = true,
            SecretKey = secretKey,
            Bot = bot,
        };

        // Setup mocks
        _mockWebhookRepository.Setup(x => x.GetByUuidWithRelationsAsync(uuid))
            .ReturnsAsync(webhook);
        _mockMessageFormattingService.Setup(x => x.FormatMessage(It.IsAny<Webhook>(), It.IsAny<JsonElement>()))
            .Returns(MessageFormattingResult.Success(formattedMessage));
        _mockTelegramService.Setup(x => x.SendMessageAsync(It.IsAny<Webhook>(), It.IsAny<string>()))
            .ReturnsAsync(TelegramResult.Success());
        _mockLoggingService.Setup(x => x.StartRequestAsync(It.IsAny<int>(), It.IsAny<HttpRequest>()))
            .ReturnsAsync(requestId);
        _mockLoggingService.Setup(x => x.LogValidationResultAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>()))
            .Returns(Task.CompletedTask);
        _mockLoggingService.Setup(x => x.LogMessageFormattingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockLoggingService.Setup(x => x.LogTelegramResponseAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockLoggingService.Setup(x => x.CompleteRequestAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Create request with secret key in query
        var httpRequest = CreateMockHttpRequest();
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["secret_key"] = secretKey
        });
        httpRequest.Setup(x => x.Query).Returns(query);

        // Act
        var result = await _service.ProcessWebhookAsync(uuid, JsonDocument.Parse(payloadJson).RootElement, httpRequest.Object);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithProtectedWebhookAndInvalidKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440011";
        var secretKey = "my-secret-key";
        var wrongKey = "wrong-key";

        var webhook = new Webhook
        {
            Id = 1,
            Name = "Protected Webhook",
            Uuid = uuid,
            IsDisabled = false,
            IsProtected = true,
            SecretKey = secretKey,
        };

        _mockWebhookRepository.Setup(x => x.GetByUuidWithRelationsAsync(uuid))
            .ReturnsAsync(webhook);

        // Create request with wrong secret key
        var httpRequest = CreateMockHttpRequest();
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["secret_key"] = wrongKey
        });
        httpRequest.Setup(x => x.Query).Returns(query);

        // Act
        var result = await _service.ProcessWebhookAsync(uuid, new JsonElement(), httpRequest.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(401, result.StatusCode);
        Assert.Equal("Invalid secret key provided", result.Error);
    }

    #endregion

    #region Failure Notification Tests

    [Fact]
    public async Task ProcessWebhookAsync_WithMessageFormattingFailure_ShouldSendFailureNotification()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440010";
        var requestId = "test-request-id-notification";

        // CREATE A VALID JSON ELEMENT INSTEAD OF new JsonElement()
        var payloadJson = """{"event": "test", "data": {"message": "Hello World"}}""";
        var payload = JsonDocument.Parse(payloadJson).RootElement;

        var bot = new Bot
        {
            Id = 1,
            Name = "Test Bot",
            BotToken = "test-token",
            ChatId = "123456789"
        };

        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            Uuid = uuid,
            IsDisabled = false,
            IsProtected = false,
            Bot = bot
        };

        _mockWebhookRepository.Setup(x => x.GetByUuidWithRelationsAsync(uuid))
            .ReturnsAsync(webhook);

        _mockLoggingService.Setup(x => x.StartRequestAsync(webhook.Id, It.IsAny<HttpRequest>()))
            .ReturnsAsync(requestId);

        _mockLoggingService.Setup(x => x.LogValidationResultAsync(requestId, true, It.IsAny<List<string>>()))
            .Returns(Task.CompletedTask);

        _mockLoggingService.Setup(x => x.CompleteRequestAsync(requestId, 500, It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Setup message formatting to fail
        _mockMessageFormattingService.Setup(x => x.FormatMessage(webhook, It.IsAny<JsonElement>()))
            .Throws(new Exception("Template parsing error"));

        var httpRequest = CreateMockHttpRequest();

        // Act - USE THE VALID PAYLOAD
        var result = await _service.ProcessWebhookAsync(uuid, payload, httpRequest.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(500, result.StatusCode);

        // Give fire-and-forget notification time to execute
        await Task.Delay(100);

        // Verify notification was sent
        _mockFailureNotificationService.Verify(
            x => x.SendFailureNotificationAsync(
                "Test Webhook",
                "Processing Error",
                "Template parsing error",
                requestId),
            Times.Once);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithGeneralException_ShouldSendFailureNotification()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440013";
        var requestId = "test-request-id-notification4";

        // CREATE A VALID JSON ELEMENT
        var payloadJson = """{"event": "test", "data": {"message": "Hello World"}}""";
        var payload = JsonDocument.Parse(payloadJson).RootElement;

        var bot = new Bot
        {
            Id = 1,
            Name = "Test Bot",
            BotToken = "test-token",
            ChatId = "123456789"
        };

        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            Uuid = uuid,
            IsDisabled = false,
            IsProtected = false,
            Bot = bot
        };

        _mockWebhookRepository.Setup(x => x.GetByUuidWithRelationsAsync(uuid))
            .ReturnsAsync(webhook);

        _mockLoggingService.Setup(x => x.StartRequestAsync(webhook.Id, It.IsAny<HttpRequest>()))
            .ReturnsAsync(requestId);

        _mockLoggingService.Setup(x => x.LogValidationResultAsync(requestId, true, It.IsAny<List<string>>()))
            .Returns(Task.CompletedTask);

        // Setup unexpected exception
        var unexpectedException = new Exception("Unexpected system error");
        _mockMessageFormattingService.Setup(x => x.FormatMessage(webhook, It.IsAny<JsonElement>()))
            .Throws(unexpectedException);

        _mockLoggingService.Setup(x => x.CompleteRequestAsync(requestId, 500, It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var httpRequest = CreateMockHttpRequest();

        var result = await _service.ProcessWebhookAsync(uuid, payload, httpRequest.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(500, result.StatusCode);

        // Give fire-and-forget notification time to execute
        await Task.Delay(100);

        // Verify notification was sent
        _mockFailureNotificationService.Verify(
            x => x.SendFailureNotificationAsync(
                "Test Webhook",
                "Processing Error",
                "Unexpected system error",
                requestId),
            Times.Once);
    }
    [Fact]
    public async Task ProcessWebhookAsync_WithSuccessfulDelivery_ShouldNotSendFailureNotification()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440013";
        var requestId = "test-request-id-notification4";

        // Create valid JsonElement
        var payloadJson = """{"event": "test", "data": {"message": "Hello World"}}""";
        var payload = JsonDocument.Parse(payloadJson).RootElement;
        var formattedMessage = "Test formatted message";

        var bot = new Bot
        {
            Id = 1,
            Name = "Test Bot",
            BotToken = "test-token",
            ChatId = "123456789"
        };

        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            Uuid = uuid,
            IsDisabled = false,
            IsProtected = false,
            Bot = bot
        };

        _mockWebhookRepository.Setup(x => x.GetByUuidWithRelationsAsync(uuid))
            .ReturnsAsync(webhook);

        _mockLoggingService.Setup(x => x.StartRequestAsync(webhook.Id, It.IsAny<HttpRequest>()))
            .ReturnsAsync(requestId);

        _mockLoggingService.Setup(x => x.LogValidationResultAsync(requestId, true, It.IsAny<List<string>>()))
            .Returns(Task.CompletedTask);

        _mockMessageFormattingService.Setup(x => x.FormatMessage(webhook, payload))
            .Returns(MessageFormattingResult.Success(formattedMessage));

        _mockLoggingService.Setup(x => x.LogMessageFormattingAsync(requestId, formattedMessage))
            .Returns(Task.CompletedTask);

        // Setup successful Telegram delivery
        _mockTelegramService.Setup(x => x.SendMessageAsync(webhook, formattedMessage))
            .ReturnsAsync(TelegramResult.Success());

        _mockLoggingService.Setup(x => x.LogTelegramResponseAsync(requestId, true, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockLoggingService.Setup(x => x.CompleteRequestAsync(requestId, 200, It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);


        var httpRequest = CreateMockHttpRequest();

        var result = await _service.ProcessWebhookAsync(uuid, payload, httpRequest.Object);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(200, result.StatusCode);

        _mockFailureNotificationService.Verify(
            x => x.SendFailureNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }
    #endregion
}
