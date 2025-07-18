using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TeleHook.Api.Models;
using TeleHook.Api.Services.Infrastructure;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class TelegramServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly TelegramService _service;

    public TelegramServiceTests()
    {
        var mockLogger = new Mock<ILogger<TelegramService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _service = new TelegramService(httpClient, mockLogger.Object);
    }

    [Fact]
    public async Task SendMessageAsync_WithValidWebhook_ShouldReturnTrue()
    {
        // Arrange
        var webhook = CreateTestWebhook();
        var messageText = "Test message";

        var responseContent = JsonSerializer.Serialize(new { ok = true, result = new { message_id = 123 } });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.SendMessageAsync(webhook, messageText);

        // Assert
        Assert.True(result.IsSuccess);
        VerifyHttpRequest("sendMessage", webhook.Bot.BotToken);
    }

    [Fact]
    public async Task SendMessageAsync_WithTelegramApiError_ShouldThrowTelegramApiException()
    {
        // Arrange
        var webhook = CreateTestWebhook();
        var messageText = "Test message";

        var errorContent = JsonSerializer.Serialize(new
        {
            ok = false,
            error_code = 400,
            description = "Bad Request: chat not found"
        });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorContent, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var result = await _service.SendMessageAsync(webhook, messageText);

        Assert.Contains("Bad Request: chat not found", result.Error);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task SendMessageAsync_WithHttpRequestException_ShouldReturnTelegramFailureResult()
    {
        // Arrange
        var webhook = CreateTestWebhook();
        var messageText = "Test message";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        var result = await _service.SendMessageAsync(webhook, messageText);

        Assert.Contains("HTTP request failed", result.Error);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SendMessageAsync_WithTimeout_ShouldReturnTelegramFailureResult()
    {
        // Arrange
        var webhook = CreateTestWebhook();
        var messageText = "Test message";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        var result = await _service.SendMessageAsync(webhook, messageText);

        Assert.Contains("Request timeout", result.Error);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SendMessageAsync_WithTopicId_ShouldIncludeMessageThreadId()
    {
        // Arrange
        var webhook = CreateTestWebhook();
        webhook.TopicId = "123";
        var messageText = "Test message";

        var responseContent = JsonSerializer.Serialize(new { ok = true, result = new { message_id = 123 } });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        HttpRequestMessage? capturedRequest = null;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(httpResponse);

        // Act
        await _service.SendMessageAsync(webhook, messageText);

        // Assert
        Assert.NotNull(capturedRequest);
        var requestContent = await capturedRequest.Content!.ReadAsStringAsync();
        var payload = JsonSerializer.Deserialize<JsonElement>(requestContent);

        Assert.True(payload.TryGetProperty("message_thread_id", out var threadId));
        Assert.Equal("123", threadId.GetString());
    }

    [Fact]
    public async Task SendMessageAsync_WithoutTopicId_ShouldNotIncludeMessageThreadId()
    {
        // Arrange
        var webhook = CreateTestWebhook();
        webhook.TopicId = "";
        var messageText = "Test message";

        var responseContent = JsonSerializer.Serialize(new { ok = true, result = new { message_id = 123 } });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        HttpRequestMessage? capturedRequest = null;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(httpResponse);

        // Act
        await _service.SendMessageAsync(webhook, messageText);

        // Assert
        Assert.NotNull(capturedRequest);
        var requestContent = await capturedRequest.Content!.ReadAsStringAsync();
        var payload = JsonSerializer.Deserialize<JsonElement>(requestContent);

        Assert.False(payload.TryGetProperty("message_thread_id", out _));
    }

    [Fact]
    public async Task TestConnectionAsync_WithValidConfig_ShouldSucceed()
    {
        // Arrange
        var config = CreateTestBot();

        var responseContent = JsonSerializer.Serialize(new
        {
            ok = true,
            result = new
            {
                id = 123456789,
                username = "test_bot",
                first_name = "Test Bot"
            }
        });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert (should not throw)
        await _service.TestConnectionAsync(config);

        VerifyHttpRequest("getMe", config.BotToken);
    }

    [Fact]
    public async Task TestConnectionAsync_WithInvalidToken_ShouldReturnFailureResult()
    {
        // Arrange
        var config = CreateTestBot();

        var errorContent = JsonSerializer.Serialize(new
        {
            ok = false,
            error_code = 401,
            description = "Unauthorized"
        });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(errorContent, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var result = await _service.TestConnectionAsync(config);

        Assert.Contains("Unauthorized", result.Error);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task TestConnectionAsync_WithOkFalse_ShouldReturnTelegramFailureResult()
    {
        // Arrange
        var config = CreateTestBot();

        var responseContent = JsonSerializer.Serialize(new { ok = false });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var exception = await _service.TestConnectionAsync(config);

        Assert.Contains("ok=false", exception.Error);
        Assert.False(exception.IsSuccess);
        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public async Task TestConnectionAsync_WithHttpRequestException_ShouldReturnTelegramFailureResult()
    {
        // Arrange
        var config = CreateTestBot();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("DNS resolution failed"));

        // Act & Assert
        var result = await _service.TestConnectionAsync(config);

        Assert.Contains("HTTP request failed", result.Error);
        Assert.False(result.IsSuccess);
        Assert.Equal(502, result.StatusCode);
    }

    private Webhook CreateTestWebhook()
    {
        return new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            Uuid = "test-uuid",
            TopicId = "",
            ParseMode = "HTML",
            DisableWebPagePreview = true,
            DisableNotification = false,
            Bot = CreateTestBot()
        };
    }

    private Bot CreateTestBot()
    {
        return new Bot
        {
            Id = 1,
            Name = "Test Config",
            BotToken = "123456789:ABCdefGHIjklMNOpqrsTUVwxyz",
            ChatId = "-1001234567890"
        };
    }

    private void VerifyHttpRequest(string expectedMethod, string expectedToken)
    {
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains($"bot{expectedToken}/{expectedMethod}")),
                ItExpr.IsAny<CancellationToken>());
    }
}
