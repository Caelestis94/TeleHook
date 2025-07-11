using Microsoft.Extensions.Logging;
using Moq;
using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Domain;
using TeleHook.Api.Services.Infrastructure;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class WebhookStatServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IWebhookStatRepository> _mockWebhookStatRepository;
    private readonly Mock<ILogger<WebhookStatService>> _mockLogger;
    private readonly WebhookStatService _service;

    public WebhookStatServiceTests()
    {
        var appSetting = new AppSettingDto
        {
            LogLevel = "Warning",
            LogRetentionDays = 7,
            EnableWebhookLogging = true,
            WebhookLogRetentionDays = 0,
            StatsDaysInterval = 30
        };
        
        
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockWebhookStatRepository = new Mock<IWebhookStatRepository>();
        _mockLogger = new Mock<ILogger<WebhookStatService>>();

        _mockUnitOfWork.Setup(x => x.WebhookStats).Returns(_mockWebhookStatRepository.Object);

        _service = new WebhookStatService(_mockUnitOfWork.Object, _mockLogger.Object,appSetting);
    }

    [Fact]
    public async Task UpdateStatsAsync_ShouldUpdateBothWebhookAndGlobalStats()
    {
        // Arrange
        var webhookId = 1;
        var statusCode = 200;
        var processingTimeMs = 150;
        var payloadValidated = true;
        var telegramSent = true;

        // Act
        await _service.UpdateStatsAsync(webhookId, statusCode, processingTimeMs, payloadValidated, telegramSent);

        // Assert - Should call UpdateDailyStatsAsync twice (endpoint-specific and global)
        _mockWebhookStatRepository.Verify(x => x.GetByDateAndWebhookAsync(It.IsAny<DateTime>(), webhookId), Times.Once);
        _mockWebhookStatRepository.Verify(x => x.GetByDateAndWebhookAsync(It.IsAny<DateTime>(), null), Times.Once);
    }

    [Fact]
    public async Task UpdateDailyStatsAsync_WhenStatsDoNotExist_ShouldCreateNewStats()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var webhookId = 1;
        var statusCode = 200;
        var processingTimeMs = 100;

        _mockWebhookStatRepository.Setup(x => x.GetByDateAndWebhookAsync(date, webhookId))
                                  .ReturnsAsync((WebhookStat?)null);

        // Act
        await _service.UpdateDailyStatsAsync(date, webhookId, statusCode, processingTimeMs, true, true);

        // Assert
        _mockWebhookStatRepository.Verify(x => x.AddAsync(It.Is<WebhookStat>(s =>
            s.Date == date &&
            s.WebhookId == webhookId &&
            s.TotalRequests == 1 &&
            s.SuccessfulRequests == 1 &&
            s.FailedRequests == 0 &&
            s.ValidationFailures == 0 &&
            s.TelegramFailures == 0 &&
            s.TotalProcessingTimeMs == processingTimeMs &&
            s.AvgProcessingTimeMs == processingTimeMs &&
            s.MinProcessingTimeMs == processingTimeMs &&
            s.MaxProcessingTimeMs == processingTimeMs
        )), Times.Once);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateDailyStatsAsync_WhenStatsExist_ShouldUpdateExistingStats()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var webhookId = 1;
        var existingStats = new WebhookStat
        {
            Date = date,
            WebhookId = webhookId,
            TotalRequests = 5,
            SuccessfulRequests = 4,
            FailedRequests = 1,
            ValidationFailures = 0,
            TelegramFailures = 1,
            TotalProcessingTimeMs = 500,
            AvgProcessingTimeMs = 100,
            MinProcessingTimeMs = 80,
            MaxProcessingTimeMs = 120
        };

        _mockWebhookStatRepository.Setup(x => x.GetByDateAndWebhookAsync(date, webhookId))
                                  .ReturnsAsync(existingStats);

        // Act
        await _service.UpdateDailyStatsAsync(date, webhookId, 200, 150, true, true);

        // Assert
        Assert.Equal(6, existingStats.TotalRequests);
        Assert.Equal(5, existingStats.SuccessfulRequests);
        Assert.Equal(1, existingStats.FailedRequests);
        Assert.Equal(0, existingStats.ValidationFailures);
        Assert.Equal(1, existingStats.TelegramFailures);
        Assert.Equal(650, existingStats.TotalProcessingTimeMs);
        Assert.Equal(108, existingStats.AvgProcessingTimeMs); // 650 / 6
        Assert.Equal(80, existingStats.MinProcessingTimeMs);
        Assert.Equal(150, existingStats.MaxProcessingTimeMs);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData(200, true, false)] // Success
    [InlineData(299, true, false)] // Success
    [InlineData(300, false, false)] // Neither success nor failure
    [InlineData(400, false, true)] // Failure
    [InlineData(500, false, true)] // Failure
    public async Task UpdateDailyStatsAsync_ShouldCategorizeStatusCodesCorrectly(int statusCode, bool expectedSuccess, bool expectedFailure)
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        _mockWebhookStatRepository.Setup(x => x.GetByDateAndWebhookAsync(date, null))
                                  .ReturnsAsync((WebhookStat?)null);

        // Act
        await _service.UpdateDailyStatsAsync(date, null, statusCode, 100, true, true);

        // Assert
        _mockWebhookStatRepository.Verify(x => x.AddAsync(It.Is<WebhookStat>(s =>
            s.SuccessfulRequests == (expectedSuccess ? 1 : 0) &&
            s.FailedRequests == (expectedFailure ? 1 : 0)
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateDailyStatsAsync_ShouldIncrementValidationFailures_WhenPayloadNotValidated()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        _mockWebhookStatRepository.Setup(x => x.GetByDateAndWebhookAsync(date, null))
                                  .ReturnsAsync((WebhookStat?)null);

        // Act
        await _service.UpdateDailyStatsAsync(date, null, 200, 100, false, true);

        // Assert
        _mockWebhookStatRepository.Verify(x => x.AddAsync(It.Is<WebhookStat>(s =>
            s.ValidationFailures == 1
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateDailyStatsAsync_ShouldIncrementTelegramFailures_WhenTelegramNotSent()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        _mockWebhookStatRepository.Setup(x => x.GetByDateAndWebhookAsync(date, null))
                                  .ReturnsAsync((WebhookStat?)null);

        // Act
        await _service.UpdateDailyStatsAsync(date, null, 200, 100, true, false);

        // Assert
        _mockWebhookStatRepository.Verify(x => x.AddAsync(It.Is<WebhookStat>(s =>
            s.TelegramFailures == 1
        )), Times.Once);
    }

    [Fact]
    public async Task GetOverviewStatsAsync_ShouldReturnCorrectOverviewStructure()
    {
        // Arrange
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30).Date;
        var today = DateTime.UtcNow.Date;

        var globalStats = new List<WebhookStat>
        {
            new() { Date = today, TotalRequests = 10, SuccessfulRequests = 8, FailedRequests = 2, AvgProcessingTimeMs = 100 },
            new() { Date = today.AddDays(-1), TotalRequests = 15, SuccessfulRequests = 12, FailedRequests = 3, AvgProcessingTimeMs = 120 }
        };

        var webhookStats = new List<object>
        {
            new { WebhookId = 1, Name = "Test Webhook", TotalRequests = 20 }
        };

        _mockWebhookStatRepository.Setup(x => x.GetGlobalStatsAsync(thirtyDaysAgo, today))
                                  .ReturnsAsync(globalStats);
        _mockWebhookStatRepository.Setup(x => x.GetTopWebhookStatsAsync(thirtyDaysAgo, today, 5))
                                  .ReturnsAsync(webhookStats);

        // Act
        var result = await _service.GetOverviewStatsAsync();

        // Assert
        Assert.NotNull(result);
        var resultDict = result as dynamic;
        Assert.NotNull(resultDict);
    }

    [Fact]
    public async Task GetWebhookStatsAsync_ShouldReturnWebhookSpecificStats()
    {
        // Arrange
        var webhookId = 1;
        var days = 30;
        var startDate = DateTime.UtcNow.AddDays(-days).Date;
        var endDate = DateTime.UtcNow.Date;

        var stats = new List<WebhookStat>
        {
            new() 
            { 
                Date = endDate, 
                WebhookId = webhookId, 
                TotalRequests = 10, 
                SuccessfulRequests = 8, 
                AvgProcessingTimeMs = 100 
            },
            new() 
            { 
                Date = endDate.AddDays(-1), 
                WebhookId = webhookId, 
                TotalRequests = 5, 
                SuccessfulRequests = 4, 
                AvgProcessingTimeMs = 120 
            }
        };

        _mockWebhookStatRepository.Setup(x => x.GetByDateRangeAsync(startDate, endDate))
                                  .ReturnsAsync(stats);

        // Act
        var result = await _service.GetWebhookStatsAsync(webhookId, days);

        // Assert
        Assert.NotNull(result);
        var resultDict = result as dynamic;
        Assert.NotNull(resultDict);
    }

    [Fact]
    public async Task GetOverviewStatsAsync_WhenExceptionOccurs_ShouldThrowInternalServerErrorException()
    {
        // Arrange
        _mockWebhookStatRepository.Setup(x => x.GetGlobalStatsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                                  .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InternalServerErrorException>(
            () => _service.GetOverviewStatsAsync());

        Assert.Equal("Failed to retrieve overview statistics", exception.Message);
    }

    [Fact]
    public async Task GetWebhookStatsAsync_WhenExceptionOccurs_ShouldThrowInternalServerErrorException()
    {
        // Arrange
        var webhookId = 1;
        _mockWebhookStatRepository.Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                                  .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InternalServerErrorException>(
            () => _service.GetWebhookStatsAsync(webhookId));

        Assert.Equal($"Failed to retrieve statistics for webhook {webhookId}", exception.Message);
    }

    [Fact]
    public async Task UpdateStatsAsync_WhenExceptionOccurs_ShouldThrowExceptionAndLogError()
    {
        // Arrange
        _mockWebhookStatRepository.Setup(x => x.GetByDateAndWebhookAsync(It.IsAny<DateTime>(), It.IsAny<int?>()))
                                  .ThrowsAsync(new Exception("Database error"));

        // Act
var exception = await Assert.ThrowsAsync<InternalServerErrorException>(
            () => _service.UpdateStatsAsync(1, 200, 100, true, true));
        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to update daily stats")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
