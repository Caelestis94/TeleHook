using FluentValidation.Results;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Domain;
using TeleHook.Api.Services.Interfaces;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class SettingManagementServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IAppSettingRepository> _mockAppSettingRepository;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
    private readonly Mock<IFailureNotificationService> _mockFailureNotificationService;
    private readonly Mock<ILogger<SettingManagementService>> _mockLogger;
    private readonly AppSettingDto _appSettingDto;
    private readonly SettingManagementService _service;

    public SettingManagementServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockAppSettingRepository = new Mock<IAppSettingRepository>();
        _mockValidationService = new Mock<IValidationService>();
        _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();
        _mockLogger = new Mock<ILogger<SettingManagementService>>();
        _mockFailureNotificationService = new Mock<IFailureNotificationService>();
        _appSettingDto = new AppSettingDto
        {
            LogLevel = "Warning",
            LogPath = "/app/logs/telehook-.log",
            LogRetentionDays = 7,
            EnableWebhookLogging = true,
            WebhookLogRetentionDays = 0,
            StatsDaysInterval = 30
        };

        _mockUnitOfWork.Setup(u => u.AppSettings).Returns(_mockAppSettingRepository.Object);

        _service = new SettingManagementService(
            _mockUnitOfWork.Object,
            _mockValidationService.Object,
            _appSettingDto,
            _mockHostApplicationLifetime.Object,
            _mockFailureNotificationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetSettingsAsync_WhenSettingsExist_ShouldReturnSettings()
    {
        // Arrange
        var existingSettings = new AppSetting
        {
            Id = 1,
            LogLevel = "Information",
            LogRetentionDays = 14,
            EnableWebhookLogging = true,
            WebhookLogRetentionDays = 30,
            StatsDaysInterval = 30,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockAppSettingRepository.Setup(x => x.SettingsExistAsync())
            .ReturnsAsync(true);
        _mockAppSettingRepository.Setup(x => x.GetSettingsAsync())
            .ReturnsAsync(existingSettings);

        // Act
        var result = await _service.GetSettingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingSettings.Id, result.Id);
        Assert.Equal(existingSettings.LogLevel, result.LogLevel);
        Assert.Equal(existingSettings.LogRetentionDays, result.LogRetentionDays);
        Assert.Equal(existingSettings.EnableWebhookLogging, result.EnableWebhookLogging);
        Assert.Equal(existingSettings.WebhookLogRetentionDays, result.WebhookLogRetentionDays);
        Assert.Equal(existingSettings.StatsDaysInterval, result.StatsDaysInterval);
    }

    [Fact]
    public async Task GetSettingsAsync_WhenSettingsDoNotExist_ShouldCreateAndReturnDefaultSettings()
    {
        // Arrange
        _mockAppSettingRepository.Setup(x => x.SettingsExistAsync())
            .ReturnsAsync(false);
        _mockAppSettingRepository.Setup(x => x.AddAsync(It.IsAny<AppSetting>()))
            .ReturnsAsync((AppSetting)null!);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.GetSettingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Warning", result.LogLevel);
        Assert.Equal(7, result.LogRetentionDays);
        Assert.True(result.EnableWebhookLogging);
        Assert.Equal(0, result.WebhookLogRetentionDays);
        Assert.Equal(30, result.StatsDaysInterval);
        Assert.True(result.CreatedAt > DateTime.MinValue);
        Assert.True(result.UpdatedAt > DateTime.MinValue);

        // Verify repository calls
        _mockAppSettingRepository.Verify(x => x.AddAsync(It.IsAny<AppSetting>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetSettingsAsync_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database error");
        _mockAppSettingRepository.Setup(x => x.SettingsExistAsync())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetSettingsAsync());
        Assert.Equal(expectedException.Message, exception.Message);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to get settings")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenValidationFails_ShouldThrowValidationException()
    {
        // Arrange
        var updateRequest = new AppSettingDto
        {
            LogLevel = "InvalidLevel",
            LogRetentionDays = 0,
            EnableWebhookLogging = false,
            WebhookLogRetentionDays = -1,
            StatsDaysInterval = 0
        };

        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("LogLevel", "LogLevel is invalid"),
            new ValidationFailure("LogRetentionDays", "LogRetentionDays must be at least 1")
        });

        _mockValidationService.Setup(x => x.ValidateAsync(updateRequest))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.UpdateSettingsAsync(updateRequest));
        Assert.Contains("LogLevel is invalid", exception.Errors);
        Assert.Contains("LogRetentionDays must be at least 1", exception.Errors);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation failed for setting update")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenValid_ShouldUpdateSettingsAndReturnResult()
    {
        // Arrange
        var updateRequest = new AppSettingDto
        {
            LogLevel = "Information",
            LogRetentionDays = 14,
            EnableWebhookLogging = false,
            WebhookLogRetentionDays = 60,
            StatsDaysInterval = 60
        };

        var originalUpdatedAt = DateTime.UtcNow.AddDays(-1);
        var existingSettings = new AppSetting
        {
            Id = 1,
            LogLevel = "Warning",
            LogRetentionDays = 7,
            EnableWebhookLogging = true,
            WebhookLogRetentionDays = 0,
            StatsDaysInterval = 30,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = originalUpdatedAt
        };

        _mockValidationService.Setup(x => x.ValidateAsync(updateRequest))
            .ReturnsAsync(new ValidationResult());
        _mockAppSettingRepository.Setup(x => x.SettingsExistAsync())
            .ReturnsAsync(true);
        _mockAppSettingRepository.Setup(x => x.GetSettingsAsync())
            .ReturnsAsync(existingSettings);
        _mockAppSettingRepository.Setup(x => x.UpdateAsync(It.IsAny<AppSetting>()))
            .ReturnsAsync((AppSetting settings) => settings);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateSettingsAsync(updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateRequest.LogLevel, result.Setting.LogLevel);
        Assert.Equal(updateRequest.LogRetentionDays, result.Setting.LogRetentionDays);
        Assert.Equal(updateRequest.EnableWebhookLogging, result.Setting.EnableWebhookLogging);
        Assert.Equal(updateRequest.WebhookLogRetentionDays, result.Setting.WebhookLogRetentionDays);
        Assert.Equal(updateRequest.StatsDaysInterval, result.Setting.StatsDaysInterval);
        Assert.True(result.Setting.UpdatedAt > originalUpdatedAt);

        // Verify singleton DTO was updated
        Assert.Equal(updateRequest.LogLevel, _appSettingDto.LogLevel);
        Assert.Equal(updateRequest.LogRetentionDays, _appSettingDto.LogRetentionDays);
        Assert.Equal(updateRequest.EnableWebhookLogging, _appSettingDto.EnableWebhookLogging);
        Assert.Equal(updateRequest.WebhookLogRetentionDays, _appSettingDto.WebhookLogRetentionDays);
        Assert.Equal(updateRequest.StatsDaysInterval, _appSettingDto.StatsDaysInterval);

        // Verify repository calls
        _mockAppSettingRepository.Verify(x => x.UpdateAsync(It.IsAny<AppSetting>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Settings updated successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var updateRequest = new AppSettingDto
        {
            LogLevel = "Information",
            LogRetentionDays = 14,
            EnableWebhookLogging = false,
            WebhookLogRetentionDays = 60,
            StatsDaysInterval = 60
        };

        var expectedException = new InvalidOperationException("Database error");
        _mockValidationService.Setup(x => x.ValidateAsync(updateRequest))
            .ReturnsAsync(new ValidationResult());
        _mockAppSettingRepository.Setup(x => x.SettingsExistAsync())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateSettingsAsync(updateRequest));
        Assert.Equal(expectedException.Message, exception.Message);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to update settings")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}