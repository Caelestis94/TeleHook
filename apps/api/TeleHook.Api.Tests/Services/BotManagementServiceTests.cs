using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Domain;
using TeleHook.Api.Services.Interfaces;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class BotManagementServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IBotRepository> _mockBotRepository;
    private readonly Mock<IWebhookRepository> _mockWebhookRepository;
    private readonly Mock<ITelegramService> _mockTelegramService;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly BotManagementService _service;

    public BotManagementServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockBotRepository = new Mock<IBotRepository>();
        _mockWebhookRepository = new Mock<IWebhookRepository>();
        _mockTelegramService = new Mock<ITelegramService>();
        _mockValidationService = new Mock<IValidationService>();
        var mockLogger = new Mock<ILogger<BotManagementService>>();

        // Setup unit of work to return mocked repositories
        _mockUnitOfWork.Setup(u => u.Bots).Returns(_mockBotRepository.Object);
        _mockUnitOfWork.Setup(u => u.Webhooks).Returns(_mockWebhookRepository.Object);

        // Setup default empty collection for delete operations
        _mockWebhookRepository.Setup(x => x.GetByBotIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Webhook>());

        // Setup transaction methods
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitTransactionAsync())
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);

        // Setup validation service to return success by default
        _mockValidationService.Setup(x => x.ValidateAsync(It.IsAny<CreateBotDto>()))
            .ReturnsAsync(new ValidationResult());
        _mockValidationService.Setup(x => x.ValidateAsync(It.IsAny<UpdateBotDto>()))
            .ReturnsAsync(new ValidationResult());

        _service = new BotManagementService(
            _mockUnitOfWork.Object,
            _mockValidationService.Object,
            mockLogger.Object,
            _mockTelegramService.Object
        );
    }

    #region GET Tests

    [Fact]
    public async Task GetAllBotsAsync_ShouldReturnAllBots()
    {
        // Arrange
        var expectedBots = new List<Bot>
        {
            new Bot { Id = 1, Name = "Bot 1", BotToken = "token1", ChatId = "123", HasPassedTest = false },
            new Bot { Id = 2, Name = "Bot 2", BotToken = "token2", ChatId = "456", HasPassedTest = true }
        };

        _mockBotRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(expectedBots);

        // Act
        var result = await _service.GetAllBotsAsync();

        // Assert
        var returnedBots = Assert.IsAssignableFrom<IEnumerable<Bot>>(result);
        Assert.Equal(2, returnedBots.Count());
        Assert.Equal(expectedBots, returnedBots);

        // Verify repository was called
        _mockBotRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWebhooksByBotIdAsync_WithValidId_ShouldReturnWebhooks()
    {
        // Arrange
        var bot = new Bot { Id = 1, Name = "Test Bot", BotToken = "token", ChatId = "123", HasPassedTest = false };

        var webhooks = new List<Webhook>
        {
            new Webhook { Id = 1, Name = "Webhook 1", BotId = 1 },
            new Webhook { Id = 2, Name = "Webhook 2", BotId = 1 }
        };
        _mockWebhookRepository.Setup(r => r.GetByBotIdAsync(1))
            .ReturnsAsync(webhooks);
        _mockBotRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(bot);


        // Act
        var result = await _service.GetBotWebhooksAsync(1);

        // Assert
        var returnedWebhooks = Assert.IsAssignableFrom<IEnumerable<Webhook>>(result);
        Assert.Equal(2, returnedWebhooks.Count());
        Assert.Equal(webhooks, returnedWebhooks);

        // Verify repository was called
        _mockWebhookRepository.Verify(r => r.GetByBotIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetWebhooksByBotIdAsync_WithInvalidId_ShouldReturnBadRequestException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _service.GetBotWebhooksAsync(-1));
        Assert.Equal("Invalid ID provided.", exception.Message);
        _mockBotRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetWebhooksByBotIdAsync_WithNonExistentId_ShouldReturnNotFoundException()
    {
        // Arrange
        _mockBotRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Bot?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.GetBotWebhooksAsync(999));

        // Assert
        Assert.Equal("Bot with ID '999' was not found", exception.Message);
        _mockBotRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
    }


    [Fact]
    public async Task GetBotByIdAsync_WithValidId_ShouldReturnBot()
    {
        // Arrange
        var expectedBot = new Bot
        {
            Id = 1,
            Name = "Test Bot",
            BotToken = "token",
            ChatId = "123",
            HasPassedTest = false
        };

        _mockBotRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(expectedBot);

        // Act
        var result = await _service.GetBotByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Bot", result.Name);
        Assert.Equal(expectedBot, result);

        // Verify repository was called
        _mockBotRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetBotByIdAsync_WithInvalidId_ShouldThrowBadRequestException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _service.GetBotByIdAsync(-1));
        Assert.Equal("Invalid ID provided.", exception.Message);
    }

    [Fact]
    public async Task GetBotByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockBotRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Bot?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.GetBotByIdAsync(999));
        Assert.Equal("Bot with ID '999' was not found", exception.Message);
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task CreateBotAsync_WithValidData_ShouldCreateBot()
    {
        // Arrange
        var request = new CreateBotDto
        {
            Name = "Test Bot",
            BotToken = "123456789:ABCdefGHIjklMNOpqrsTUVwxyz-1234567890",
            ChatId = "123456789"
        };

        var expectedBot = new Bot
        {
            Id = 1,
            Name = request.Name,
            BotToken = request.BotToken,
            ChatId = request.ChatId,
            HasPassedTest = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockBotRepository.Setup(r => r.AddAsync(It.IsAny<Bot>()))
            .ReturnsAsync(expectedBot);

        // Act
        var result = await _service.CreateBotAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Bot", result.Name);
        Assert.Equal(request.BotToken, result.BotToken);
        Assert.Equal(request.ChatId, result.ChatId);
        Assert.False(result.HasPassedTest);

        // Verify repository and unit of work were called
        _mockBotRepository.Verify(r => r.AddAsync(It.IsAny<Bot>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateBotAsync_WithValidationFailure_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateBotDto
        {
            Name = "Test Bot",
            BotToken = "invalid-token",
            ChatId = "123456789"
        };

        _mockValidationService.Setup(x => x.ValidateAsync(request))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("BotToken", "Invalid bot token format")
            }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateBotAsync(request));
        Assert.Contains("Invalid bot token format", exception.Errors);

        // Verify repository was not called due to validation failure
        _mockBotRepository.Verify(r => r.AddAsync(It.IsAny<Bot>()), Times.Never);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task UpdateBotAsync_WithValidData_ShouldUpdateBot()
    {
        // Arrange
        var existingConfig = new Bot
        {
            Id = 1,
            Name = "Old Name",
            BotToken = "old-token",
            ChatId = "old-chat",
            HasPassedTest = true
        };

        _mockBotRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingConfig);

        var request = new UpdateBotDto
        {
            Id = existingConfig.Id,
            Name = "Updated Name",
            BotToken = "new-token",
            ChatId = "new-chat"
        };

        var updatedConfig = new Bot
        {
            Id = existingConfig.Id,
            Name = "Updated Name",
            BotToken = "new-token",
            ChatId = "new-chat",
            HasPassedTest = false // Should reset to false on update
        };

        _mockBotRepository.Setup(r => r.UpdateAsync(It.IsAny<Bot>()))
            .ReturnsAsync(updatedConfig);

        // Act
        var result = await _service.UpdateBotAsync(existingConfig.Id, request);

        // Assert
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("new-token", result.BotToken);
        Assert.Equal("new-chat", result.ChatId);
        Assert.False(result.HasPassedTest); // Should reset to false on update

        // Verify repository methods were called
        _mockBotRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockBotRepository.Verify(r => r.UpdateAsync(It.IsAny<Bot>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBotAsync_WithInvalidId_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new UpdateBotDto
        {
            Id = -1,
            Name = "Test",
            BotToken = "token",
            ChatId = "chat"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _service.UpdateBotAsync(-1, request));
        Assert.Equal("Invalid ID provided.", exception.Message);
    }

    [Fact]
    public async Task UpdateBotAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new UpdateBotDto
        {
            Id = 999,
            Name = "Test",
            BotToken = "token",
            ChatId = "chat"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateBotAsync(999, request));
        Assert.Equal("Bot with ID '999' was not found", exception.Message);
    }

    [Fact]
    public async Task UpdateBotAsync_WithValidationFailure_ShouldThrowValidationException()
    {
        // Arrange
        var existingConfig = new Bot { Id = 1, Name = "Test", BotToken = "token", ChatId = "chat" };
        var request = new UpdateBotDto { Id = existingConfig.Id, Name = "", BotToken = "token", ChatId = "chat" };

        _mockBotRepository.Setup(x => x.GetByIdAsync(existingConfig.Id))
            .ReturnsAsync(existingConfig);

        _mockValidationService.Setup(x => x.ValidateAsync(request))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "Name is required")
            }));

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ValidationException>(() => _service.UpdateBotAsync(existingConfig.Id, request));
        Assert.Contains("Name is required", exception.Errors);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task DeleteBotAsync_WithValidId_ShouldDeleteBot()
    {
        // Arrange
        var config = new Bot { Id = 1, Name = "Test Bot", BotToken = "token", ChatId = "123" };

        _mockBotRepository.Setup(x => x.GetByIdAsync(config.Id))
            .ReturnsAsync(config);
        _mockBotRepository.Setup(x => x.DeleteAsync(config.Id))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteBotAsync(config.Id);

        // Assert
        _mockBotRepository.Verify(x => x.DeleteAsync(config.Id), Times.Once);
        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteBotAsync_WithInvalidId_ShouldThrowBadRequestException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _service.DeleteBotAsync(-1));
        Assert.Equal("Invalid ID provided.", exception.Message);
    }

    [Fact]
    public async Task DeleteBotAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteBotAsync(999));
        Assert.Equal("Bot with ID '999' was not found", exception.Message);
    }

    #endregion

    #region Test Connection Tests

    [Fact]
    public async Task TestBotConnectionAsync_WithValidId_ShouldReturnSuccessAndUpdateTestStatus()
    {
        // Arrange
        var bot = new Bot
        {
            Id = 1,
            Name = "Test Bot",
            BotToken = "token",
            ChatId = "123",
            HasPassedTest = false
        };

        _mockBotRepository.Setup(x => x.GetByIdAsync(bot.Id))
            .ReturnsAsync(bot);
        _mockBotRepository.Setup(x => x.UpdateAsync(It.IsAny<Bot>()))
            .ReturnsAsync(bot);
        _mockTelegramService.Setup(x => x.TestConnectionAsync(bot))
            .ReturnsAsync(TelegramResult.Success());

        // Act
        var result = await _service.TestBotConnectionAsync(bot.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);

        // Verify business logic: test successful, HasPassedTest updated to true
        _mockTelegramService.Verify(x => x.TestConnectionAsync(It.IsAny<Bot>()), Times.Once);
        _mockBotRepository.Verify(x => x.UpdateAsync(It.Is<Bot>(c => c.HasPassedTest == true)), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task TestBotConnectionAsync_WithInvalidId_ShouldThrowBadRequestException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _service.TestBotConnectionAsync(-1));
        Assert.Equal("Invalid ID provided.", exception.Message);
    }

    [Fact]
    public async Task TestBotConnectionAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.TestBotConnectionAsync(999));
        Assert.Equal("Bot with ID '999' was not found", exception.Message);
    }

    #endregion
}
