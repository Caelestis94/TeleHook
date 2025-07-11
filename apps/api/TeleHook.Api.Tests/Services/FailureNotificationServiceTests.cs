using Microsoft.Extensions.Logging;
using Moq;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Infrastructure;
using TeleHook.Api.Services.Interfaces;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class FailureNotificationServiceTests
{
    private readonly Mock<ITelegramService> _mockTelegramService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IAppSettingRepository> _mockAppSettingRepo;
    private readonly Mock<ILogger<FailureNotificationService>> _mockLogger;
    private readonly FailureNotificationService _service;

    public FailureNotificationServiceTests()
    {
        _mockTelegramService = new Mock<ITelegramService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockAppSettingRepo = new Mock<IAppSettingRepository>();
        _mockLogger = new Mock<ILogger<FailureNotificationService>>();

        _mockUnitOfWork.Setup(x => x.AppSettings).Returns(_mockAppSettingRepo.Object);

        _service = new FailureNotificationService(
            _mockTelegramService.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenNotificationsDisabled_ShouldSkipNotification()
    {
        // Arrange
        var settings = new AppSetting { EnableFailureNotifications = false };
        _mockAppSettingRepo.Setup(x => x.GetSettingsAsync()).ReturnsAsync(settings);

        // Act
        await _service.SendFailureNotificationAsync("TestWebhook", "TestFailure", "Test error");

        // Assert
        _mockTelegramService.Verify(
            x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenSettingsIncomplete_ShouldNotSendNotification()
    {
        // Arrange
        var settings = new AppSetting 
        { 
            EnableFailureNotifications = true,
            NotificationBotToken = "test-token",
            NotificationChatId = null // Missing chat ID
        };
        _mockAppSettingRepo.Setup(x => x.GetSettingsAsync()).ReturnsAsync(settings);

        // Act
        await _service.SendFailureNotificationAsync("TestWebhook", "TestFailure", "Test error");

        // Assert
        _mockTelegramService.Verify(
            x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenSettingsComplete_ShouldSendNotification()
    {
        // Arrange
        var settings = new AppSetting 
        { 
            EnableFailureNotifications = true,
            NotificationBotToken = "123456789:ABCdefGHIjklMNOpqrSTUvwxyz123456789",
            NotificationChatId = "-123456789",
            NotificationTopicId = "456"
        };
        _mockAppSettingRepo.Setup(x => x.GetSettingsAsync()).ReturnsAsync(settings);
        _mockTelegramService.Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<string>()))
            .ReturnsAsync(TelegramResult.Success());

        // Act
        await _service.SendFailureNotificationAsync("TestWebhook", "Telegram API Error", "Bot token invalid", "req-123");

        // Assert
        _mockTelegramService.Verify(
            x => x.SendMessageAsync(
                settings.NotificationBotToken,
                settings.NotificationChatId,
                It.Is<string>(msg => msg.Contains("TestWebhook") && msg.Contains("Telegram API Error") && msg.Contains("Bot token invalid") && msg.Contains("req-123")),
                "MarkdownV2",
                true,
                false,
                settings.NotificationTopicId),
            Times.Once);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenTelegramServiceThrows_ShouldLogErrorAndNotThrow()
    {
        // Arrange
        var settings = new AppSetting 
        { 
            EnableFailureNotifications = true,
            NotificationBotToken = "test-token",
            NotificationChatId = "123456789"
        };
        _mockAppSettingRepo.Setup(x => x.GetSettingsAsync()).ReturnsAsync(settings);
        _mockTelegramService.Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Telegram error"));

        // Act & Assert
        await _service.SendFailureNotificationAsync("TestWebhook", "TestFailure", "Test error");

        // Should not throw exception
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error sending failure notification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task IsNotificationEnabledAsync_WhenEnabled_ShouldReturnTrue()
    {
        // Arrange
        var settings = new AppSetting { EnableFailureNotifications = true };
        _mockAppSettingRepo.Setup(x => x.GetSettingsAsync()).ReturnsAsync(settings);

        // Act
        var result = await _service.IsNotificationEnabledAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsNotificationEnabledAsync_WhenDisabled_ShouldReturnFalse()
    {
        // Arrange
        var settings = new AppSetting { EnableFailureNotifications = false };
        _mockAppSettingRepo.Setup(x => x.GetSettingsAsync()).ReturnsAsync(settings);

        // Act
        var result = await _service.IsNotificationEnabledAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsNotificationEnabledAsync_WhenSettingsNull_ShouldReturnFalse()
    {
        // Arrange
        _mockAppSettingRepo.Setup(x => x.GetSettingsAsync()).ReturnsAsync((AppSetting)null);

        // Act
        var result = await _service.IsNotificationEnabledAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task TestNotificationAsync_WhenSettingsValid_ShouldReturnTrue()
    {
        // Arrange
        var settings = new AppSetting 
        { 
            NotificationBotToken = "test-token",
            NotificationChatId = "123456789"
        };
        _mockAppSettingRepo.Setup(x => x.GetSettingsAsync()).ReturnsAsync(settings);
        _mockTelegramService.Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<string>()))
            .ReturnsAsync(TelegramResult.Success());

        // Act
        var result = await _service.TestNotificationAsync();

        // Assert
        Assert.True(result.IsSuccess);
        _mockTelegramService.Verify(
            x => x.SendMessageAsync(
                settings.NotificationBotToken,
                settings.NotificationChatId,
                It.Is<string>(msg => msg.Contains("Test Notification")),
                "MarkdownV2",
                true,
                false,
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task TestNotificationAsync_WhenSettingsInvalid_ShouldReturnFalse()
    {
        // Arrange
        var settings = new AppSetting 
        { 
            NotificationBotToken = null,
            NotificationChatId = "123456789"
        };
        _mockAppSettingRepo.Setup(x => x.GetSettingsAsync()).ReturnsAsync(settings);

        // Act
        var result = await _service.TestNotificationAsync();

        // Assert
        Assert.False(result.IsSuccess);
        _mockTelegramService.Verify(
            x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task TestNotificationAsync_WhenTelegramServiceFails_ShouldReturnFalse()
    {
        // Arrange
        var settings = new AppSetting 
        { 
            NotificationBotToken = "test-token",
            NotificationChatId = "123456789"
        };
        _mockAppSettingRepo.Setup(x => x.GetSettingsAsync()).ReturnsAsync(settings);
        _mockTelegramService.Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Telegram error"));

        // Act
        var result = await _service.TestNotificationAsync();

        // Assert
        Assert.False(result.IsSuccess);
    }
}