using FluentValidation.Results;
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

public class WebhookManagementServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IWebhookRepository> _mockWebhookRepository;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<ITemplateParsingService> _mockTemplateParsingService;
    private readonly WebhookManagementService _service;

    public WebhookManagementServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockWebhookRepository = new Mock<IWebhookRepository>();
        var mockWebhookLogRepository = new Mock<IWebhookLogRepository>();
        var mockWebhookStatRepository = new Mock<IWebhookStatRepository>();
        _mockValidationService = new Mock<IValidationService>();
        var mockLogger = new Mock<ILogger<WebhookManagementService>>();
        _mockTemplateParsingService = new Mock<ITemplateParsingService>();

        // Setup template parsing service
        _mockTemplateParsingService.Setup(x => x.RefreshTemplateAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Setup unit of work to return the mocked repositories
        _mockUnitOfWork.Setup(x => x.Webhooks)
            .Returns(_mockWebhookRepository.Object);
        _mockUnitOfWork.Setup(x => x.WebhookLogs)
            .Returns(mockWebhookLogRepository.Object);
        _mockUnitOfWork.Setup(x => x.WebhookStats)
            .Returns(mockWebhookStatRepository.Object);

        // Setup default empty collections for delete operations
        mockWebhookLogRepository.Setup(x => x.GetByWebhookIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<WebhookLog>());
        mockWebhookStatRepository.Setup(x => x.GetByWebhookIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<WebhookStat>());

        // Setup transaction methods
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitTransactionAsync())
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);

        // Setup validation service to return success by default
        _mockValidationService.Setup(x => x.ValidateAsync(It.IsAny<CreateWebhookDto>()))
            .ReturnsAsync(new ValidationResult());
        _mockValidationService.Setup(x => x.ValidateAsync(It.IsAny<UpdateWebhookDto>()))
            .ReturnsAsync(new ValidationResult());

        _service = new WebhookManagementService(
            _mockUnitOfWork.Object,
            _mockValidationService.Object,
            mockLogger.Object,
            _mockTemplateParsingService.Object
        );
    }

    #region GET Tests

    [Fact]
    public async Task GetAllWebhooksAsync_ShouldReturnAllWebhooks()
    {
        // Arrange
        var webhooks = new List<Webhook>
        {
            new Webhook { Id = 1, Name = "Webhook 1", Uuid = "uuid1", MessageTemplate = "Test 1" },
            new Webhook { Id = 2, Name = "Webhook 2", Uuid = "uuid2", MessageTemplate = "Test 2" }
        };

        _mockWebhookRepository.Setup(x => x.GetWithRelationsAsync())
            .ReturnsAsync(webhooks);

        // Act
        var result = await _service.GetAllWebhooksAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _mockWebhookRepository.Verify(x => x.GetWithRelationsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetWebhookByIdAsync_WithValidId_ShouldReturnWebhook()
    {
        // Arrange
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            Uuid = "test-uuid",
            MessageTemplate = "Test message",
            ParseMode = "MarkdownV2"
        };

        _mockWebhookRepository.Setup(x => x.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(webhook);

        // Act
        var result = await _service.GetWebhookByIdAsync(1);

        // Assert
        Assert.Equal(webhook.Id, result.Id);
        Assert.Equal(webhook.Name, result.Name);
        _mockWebhookRepository.Verify(x => x.GetByIdWithRelationsAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetWebhookByIdAsync_WithInvalidId_ShouldThrowBadRequestException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _service.GetWebhookByIdAsync(-1));
        Assert.Equal("Invalid ID", exception.Message);
        _mockWebhookRepository.Verify(x => x.GetByIdWithRelationsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetWebhookByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockWebhookRepository.Setup(x => x.GetByIdWithRelationsAsync(999))
            .ReturnsAsync((Webhook?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.GetWebhookByIdAsync(999));
        Assert.Equal("Webhook with ID '999' was not found", exception.Message);
        _mockWebhookRepository.Verify(x => x.GetByIdWithRelationsAsync(999), Times.Once);
    }

    #endregion

    #region POST Tests

    [Fact]
    public async Task CreateWebhookAsync_WithValidData_ShouldCreateWebhook()
    {
        // Arrange
        var request = new CreateWebhookDto
        {
            Name = "Test Webhook",
            BotId = 1,
            TopicId = null,
            MessageTemplate = "Test message",
            ParseMode = "MarkdownV2"
        };

        var createdWebhook = new Webhook
        {
            Id = 1,
            Name = request.Name,
            Uuid = "generated-uuid",
            BotId = request.BotId,
            PayloadSample = "{}",
            MessageTemplate = request.MessageTemplate,
            ParseMode = request.ParseMode,
            IsDisabled = false,
            CreatedAt = DateTime.UtcNow
        };

        _mockWebhookRepository.Setup(x => x.AddAsync(It.IsAny<Webhook>()))
            .ReturnsAsync(createdWebhook);

        // Act
        var result = await _service.CreateWebhookAsync(request);

        // Assert
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.MessageTemplate, result.MessageTemplate);
        Assert.NotNull(result.Uuid);
        Assert.False(result.IsDisabled);

        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockWebhookRepository.Verify(x => x.AddAsync(It.IsAny<Webhook>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockTemplateParsingService.Verify(x => x.RefreshTemplateAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task CreateWebhookAsync_WithValidationFailure_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateWebhookDto
        {
            Name = "",
            BotId = 1,
            MessageTemplate = "Test"
        };

        _mockValidationService.Setup(x => x.ValidateAsync(request))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "Name is required")
            }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateWebhookAsync(request));
        Assert.Contains("Name is required", exception.Errors);
        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockWebhookRepository.Verify(x => x.AddAsync(It.IsAny<Webhook>()), Times.Never);
    }

    #endregion

    #region PUT Tests

    [Fact]
    public async Task UpdateWebhookAsync_WithValidData_ShouldUpdateWebhook()
    {
        // Arrange
        var existingWebhook = new Webhook
        {
            Id = 1,
            Name = "Old Name",
            Uuid = "test-uuid",
            BotId = 1,
            PayloadSample = "{}",
            MessageTemplate = "Old message",
            IsDisabled = false
        };

        var request = new UpdateWebhookDto
        {
            Id = 1,
            Name = "Updated Name",
            TopicId = null,
            BotId = 1,
            PayloadSample = "{}",
            MessageTemplate = "Updated message",
            IsDisabled = true
        };

        var updatedWebhook = new Webhook
        {
            Id = 1,
            Name = request.Name,
            Uuid = existingWebhook.Uuid,
            BotId = request.BotId,
            PayloadSample = request.PayloadSample,
            MessageTemplate = request.MessageTemplate,
            IsDisabled = request.IsDisabled
        };

        _mockWebhookRepository.Setup(x => x.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(existingWebhook);
        _mockWebhookRepository.Setup(x => x.UpdateAsync(It.IsAny<Webhook>()))
            .ReturnsAsync(updatedWebhook);

        // Act
        var result = await _service.UpdateWebhookAsync(1, request);

        // Assert
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated message", result.MessageTemplate);
        Assert.True(result.IsDisabled);

        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockWebhookRepository.Verify(x => x.GetByIdWithRelationsAsync(1), Times.Once);
        _mockWebhookRepository.Verify(x => x.UpdateAsync(It.IsAny<Webhook>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockTemplateParsingService.Verify(x => x.RefreshTemplateAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task UpdateWebhookAsync_WithInvalidId_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new UpdateWebhookDto
        {
            Id = -1,
            Name = "Test",
            BotId = 1,
            MessageTemplate = "Test"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _service.UpdateWebhookAsync(-1, request));
        Assert.Equal("Invalid ID provided.", exception.Message);
    }

    [Fact]
    public async Task UpdateWebhookAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new UpdateWebhookDto
        {
            Id = 999,
            Name = "Test",
            BotId = 1,
            MessageTemplate = "Test"
        };

        _mockWebhookRepository.Setup(x => x.GetByIdWithRelationsAsync(999))
            .ReturnsAsync((Webhook?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateWebhookAsync(999, request));
        Assert.Equal("Webhook with ID '999' was not found", exception.Message);
        _mockWebhookRepository.Verify(x => x.GetByIdWithRelationsAsync(999), Times.Once);
    }

    [Fact]
    public async Task UpdateWebhookAsync_WithValidationFailure_ShouldThrowValidationException()
    {
        // Arrange
        var existingWebhook = new Webhook { Id = 1, Name = "Test", Uuid = "uuid" };
        var request = new UpdateWebhookDto
        {
            Id = 1,
            Name = "",
            BotId = 1,
            MessageTemplate = "Test"
        };

        _mockWebhookRepository.Setup(x => x.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(existingWebhook);
        _mockValidationService.Setup(x => x.ValidateAsync(request))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "Name is required")
            }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.UpdateWebhookAsync(1, request));
        Assert.Contains("Name is required", exception.Errors);
        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockWebhookRepository.Verify(x => x.UpdateAsync(It.IsAny<Webhook>()), Times.Never);
    }

    #endregion

    #region DELETE Tests

    [Fact]
    public async Task DeleteWebhookAsync_WithValidId_ShouldDeleteWebhook()
    {
        // Arrange
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            Uuid = "test-uuid"
        };

        _mockWebhookRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(webhook);
        _mockWebhookRepository.Setup(x => x.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteWebhookAsync(1);

        // Assert
        _mockWebhookRepository.Verify(x => x.GetByIdAsync(1), Times.Once);
        _mockWebhookRepository.Verify(x => x.DeleteAsync(1), Times.Once);
        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteWebhookAsync_WithInvalidId_ShouldThrowBadRequestException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _service.DeleteWebhookAsync(0));
        Assert.Equal("Invalid ID provided.", exception.Message);
    }

    [Fact]
    public async Task DeleteWebhookAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockWebhookRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Webhook?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteWebhookAsync(999));
        Assert.Equal("Webhook with ID '999' was not found", exception.Message);
        _mockWebhookRepository.Verify(x => x.GetByIdAsync(999), Times.Once);
        _mockWebhookRepository.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region GenerateSecretKeyAsync Tests

    [Fact]
    public async Task GenerateSecretKeyAsync_WithValidId_ShouldGenerateAndUpdateSecretKey()
    {
        // Arrange
        var request = new GenerateSecretKeyDto { WebhookId = 1 };
        var webhook = new Webhook
        {
            Id = 1,
            Uuid = "test-uuid",
            Name = "Test Webhook",
            SecretKey = "old-key"
        };

        _mockWebhookRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(webhook);
        _mockWebhookRepository.Setup(x => x.UpdateAsync(It.IsAny<Webhook>()))
            .ReturnsAsync((Webhook w) => w);

        // Act
        var result = await _service.GenerateSecretKeyAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Secret key generated successfully", result.Message);
        Assert.StartsWith("sk_", result.SecretKey);
        Assert.Equal(51, result.SecretKey.Length); // "sk_" + 48 hex characters
        Assert.NotEqual("old-key", webhook.SecretKey);
        Assert.Equal(result.SecretKey, webhook.SecretKey);

        _mockWebhookRepository.Verify(x => x.GetByIdAsync(1), Times.Once);
        _mockWebhookRepository.Verify(x => x.UpdateAsync(webhook), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GenerateSecretKeyAsync_WithInvalidId_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new GenerateSecretKeyDto { WebhookId = 0 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _service.GenerateSecretKeyAsync(request));
        Assert.Equal("Invalid ID provided.", exception.Message);

        _mockWebhookRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockWebhookRepository.Verify(x => x.UpdateAsync(It.IsAny<Webhook>()), Times.Never);
    }

    [Fact]
    public async Task GenerateSecretKeyAsync_WithNegativeId_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new GenerateSecretKeyDto { WebhookId = -1 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _service.GenerateSecretKeyAsync(request));
        Assert.Equal("Invalid ID provided.", exception.Message);

        _mockWebhookRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockWebhookRepository.Verify(x => x.UpdateAsync(It.IsAny<Webhook>()), Times.Never);
    }

    [Fact]
    public async Task GenerateSecretKeyAsync_WithNonExistentWebhook_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new GenerateSecretKeyDto { WebhookId = 999 };

        _mockWebhookRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Webhook?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.GenerateSecretKeyAsync(request));
        Assert.Equal("Webhook with ID '999' was not found", exception.Message);

        _mockWebhookRepository.Verify(x => x.GetByIdAsync(999), Times.Once);
        _mockWebhookRepository.Verify(x => x.UpdateAsync(It.IsAny<Webhook>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GenerateSecretKeyAsync_ShouldGenerateUniqueKeys()
    {
        // Arrange
        var request1 = new GenerateSecretKeyDto { WebhookId = 1 };
        var request2 = new GenerateSecretKeyDto { WebhookId = 2 };

        var webhook1 = new Webhook { Id = 1, Uuid = "uuid1", Name = "Webhook 1" };
        var webhook2 = new Webhook { Id = 2, Uuid = "uuid2", Name = "Webhook 2" };

        _mockWebhookRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(webhook1);
        _mockWebhookRepository.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(webhook2);

        // Act
        var result1 = await _service.GenerateSecretKeyAsync(request1);
        var result2 = await _service.GenerateSecretKeyAsync(request2);

        // Assert
        Assert.NotEqual(result1.SecretKey, result2.SecretKey);
        Assert.StartsWith("sk_", result1.SecretKey);
        Assert.StartsWith("sk_", result2.SecretKey);
    }

    #endregion

    #region GenerateSecretKeyForNewWebhook Tests

    [Fact]
    public void GenerateSecretKeyForNewWebhook_ShouldGenerateValidSecretKey()
    {
        // Act
        var result = _service.GenerateSecretKeyForNewWebhook();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Secret key generated successfully for new webhook", result.Message);
        Assert.StartsWith("sk_", result.SecretKey);
        Assert.Equal(51, result.SecretKey.Length); // "sk_" + 48 hex characters

        // Verify key contains only valid hex characters after "sk_" prefix
        var hexPart = result.SecretKey.Substring(3);
        Assert.True(hexPart.All(c => "0123456789abcdef".Contains(c)));
    }

    [Fact]
    public void GenerateSecretKeyForNewWebhook_ShouldGenerateUniqueKeys()
    {
        // Act
        var result1 = _service.GenerateSecretKeyForNewWebhook();
        var result2 = _service.GenerateSecretKeyForNewWebhook();
        var result3 = _service.GenerateSecretKeyForNewWebhook();

        // Assert
        Assert.NotEqual(result1.SecretKey, result2.SecretKey);
        Assert.NotEqual(result1.SecretKey, result3.SecretKey);
        Assert.NotEqual(result2.SecretKey, result3.SecretKey);

        // All should start with "sk_"
        Assert.StartsWith("sk_", result1.SecretKey);
        Assert.StartsWith("sk_", result2.SecretKey);
        Assert.StartsWith("sk_", result3.SecretKey);
    }

    [Fact]
    public void GenerateSecretKeyForNewWebhook_ShouldNotAccessDatabase()
    {
        // Act
        var result = _service.GenerateSecretKeyForNewWebhook();

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("sk_", result.SecretKey);

        // Verify no database operations were performed
        _mockWebhookRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockWebhookRepository.Verify(x => x.UpdateAsync(It.IsAny<Webhook>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion
}
