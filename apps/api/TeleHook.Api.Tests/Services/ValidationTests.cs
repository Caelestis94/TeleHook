using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using TeleHook.Api.DTO;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Domain;
using TeleHook.Api.Services.Interfaces;
using Xunit;
using ValidationException = TeleHook.Api.Exceptions.ValidationException;

namespace TeleHook.Api.Tests.Services;

/// <summary>
/// These tests validate the validation layer across all controllers using the repository pattern.
/// Tests focus on ensuring consistent validation behavior across the application.
/// </summary>
public class ValidationTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IWebhookRepository> _mockWebhookRepository;
    private readonly Mock<IBotRepository> _mockBotRepository;
    private readonly Mock<ILogger<UserManagementService>> _mockUserLogger;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly WebhookManagementService _webhookManagementService;
    private readonly BotManagementService _botManagementService;
    private readonly UserManagementService _userManagementService;

    public ValidationTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockWebhookRepository = new Mock<IWebhookRepository>();
        _mockBotRepository = new Mock<IBotRepository>();
        var mockTelegramService = new Mock<ITelegramService>();
        var mockWebhookLogger = new Mock<ILogger<WebhookManagementService>>();
        var mockBotLogger = new Mock<ILogger<BotManagementService>>();
        _mockUserLogger = new Mock<ILogger<UserManagementService>>();
        _mockValidationService = new Mock<IValidationService>();
        var mockTemplateParsingService = new Mock<ITemplateParsingService>();

        // Setup unit of work to return the mocked repositories
        _mockUnitOfWork.Setup(x => x.Webhooks).Returns(_mockWebhookRepository.Object);
        _mockUnitOfWork.Setup(x => x.Bots).Returns(_mockBotRepository.Object);

        // Setup validation failures and success scenarios
        SetupValidationBehavior();

        _webhookManagementService = new WebhookManagementService(
            _mockUnitOfWork.Object,
            _mockValidationService.Object,
            mockWebhookLogger.Object,
            mockTemplateParsingService.Object
        );

        _botManagementService = new BotManagementService(
            _mockUnitOfWork.Object,
            _mockValidationService.Object,
            mockBotLogger.Object,
            mockTelegramService.Object
        );

    }

    private void SetupValidationBehavior()
    {
        // Setup validation failures for empty/null names
        _mockValidationService.Setup(x => x.ValidateAsync(It.Is<CreateWebhookDto>(r => string.IsNullOrEmpty(r.Name))))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        _mockValidationService.Setup(x => x.ValidateAsync(It.Is<CreateWebhookDto>(r => string.IsNullOrEmpty(r.MessageTemplate))))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("MessageTemplate", "MessageTemplate is required") }));

        _mockValidationService.Setup(x => x.ValidateAsync(It.Is<CreateBotDto>(r => r.BotToken == "invalid-token")))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("BotToken", "BotToken format is invalid") }));

        _mockValidationService.Setup(x => x.ValidateAsync(It.Is<CreateBotDto>(r => r.ChatId == "invalid-chat-id")))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("ChatId", "ChatId format is invalid") }));

        _mockValidationService.Setup(x => x.ValidateAsync(It.Is<CreateBotDto>(r => r.Name.Length > 100)))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name must be 100 characters or less") }));

        // Setup validation for duplicate name scenario
        _mockValidationService.Setup(x => x.ValidateAsync(It.Is<CreateWebhookDto>(r => r.Name == "Duplicate Name")))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "A webhook endpoint with this name already exists") }));

        // Setup validation success for valid requests by default
        _mockValidationService.Setup(x => x.ValidateAsync(It.Is<CreateWebhookDto>(r =>
            !string.IsNullOrEmpty(r.Name) && !string.IsNullOrEmpty(r.MessageTemplate) && r.Name != "Duplicate Name")))
            .ReturnsAsync(new ValidationResult());

        _mockValidationService.Setup(x => x.ValidateAsync(It.IsAny<UpdateWebhookDto>()))
            .ReturnsAsync(new ValidationResult());

        _mockValidationService.Setup(x => x.ValidateAsync(It.Is<CreateBotDto>(r =>
            r.BotToken != "invalid-token" && r.ChatId != "invalid-chat-id" && r.Name.Length <= 100)))
            .ReturnsAsync(new ValidationResult());

        _mockValidationService.Setup(x => x.ValidateAsync(It.IsAny<UpdateBotDto>()))
            .ReturnsAsync(new ValidationResult());

    }

    #region Webhook Validation Tests

    [Fact]
    public async Task CreateWebhook_WithEmptyName_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateWebhookDto
        {
            Name = "", // Empty name should be invalid
            BotId = 1,
            TopicId = null,
            MessageTemplate = "Test"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _webhookManagementService.CreateWebhookAsync(request));
        Assert.Contains("Name is required", exception.Errors);
        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockWebhookRepository.Verify(x => x.AddAsync(It.IsAny<Webhook>()), Times.Never);
    }

    [Fact]
    public async Task CreateWebhook_WithNullName_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateWebhookDto
        {
            Name = null!, // Null name should be invalid
            BotId = 1,
            TopicId = null,
            MessageTemplate = "Test"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _webhookManagementService.CreateWebhookAsync(request));
        Assert.Contains("Name is required", exception.Errors);
        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockWebhookRepository.Verify(x => x.AddAsync(It.IsAny<Webhook>()), Times.Never);
    }

    [Fact]
    public async Task CreateWebhook_WithEmptyMessageTemplate_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateWebhookDto
        {
            Name = "Valid Name",
            BotId = 1,
            TopicId = null,
            MessageTemplate = "" // Empty message template should be invalid
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _webhookManagementService.CreateWebhookAsync(request));
        Assert.Contains("MessageTemplate is required", exception.Errors);
        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockWebhookRepository.Verify(x => x.AddAsync(It.IsAny<Webhook>()), Times.Never);
    }

    [Fact]
    public async Task CreateWebhook_WithDuplicateName_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateWebhookDto
        {
            Name = "Duplicate Name", // Should conflict with existing
            BotId = 1,
            TopicId = null,
            MessageTemplate = "Test"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _webhookManagementService.CreateWebhookAsync(request));
        Assert.Contains("A webhook endpoint with this name already exists", exception.Errors);
        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockWebhookRepository.Verify(x => x.AddAsync(It.IsAny<Webhook>()), Times.Never);
    }

    #endregion

    #region Bot Validation Tests

    [Fact]
    public async Task CreateBot_WithInvalidBotToken_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateBotDto
        {
            Name = "Valid Name",
            BotToken = "invalid-token", // Should validate bot token format
            ChatId = "123456789"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _botManagementService.CreateBotAsync(request));
        Assert.Contains("BotToken format is invalid", exception.Errors);
        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockBotRepository.Verify(x => x.AddAsync(It.IsAny<Bot>()), Times.Never);
    }

    [Fact]
    public async Task CreateBot_WithInvalidChatId_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateBotDto
        {
            Name = "Valid Name",
            BotToken = "123456789:ABCdefGHIjklMNOpqrsTUVwxyz-1234567890",
            ChatId = "invalid-chat-id" // Should validate chat ID format
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _botManagementService.CreateBotAsync(request));
        Assert.Contains("ChatId format is invalid", exception.Errors);
        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockBotRepository.Verify(x => x.AddAsync(It.IsAny<Bot>()), Times.Never);
    }

    [Fact]
    public async Task CreateBot_WithTooLongName_ShouldThrowValidationException()
    {
        // Arrange
        var longName = new string('a', 101); // Assuming 100 char limit
        var request = new CreateBotDto
        {
            Name = longName,
            BotToken = "123456789:ABCdefGHIjklMNOpqrsTUVwxyz-1234567890",
            ChatId = "123456789"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _botManagementService.CreateBotAsync(request));
        Assert.Contains("Name must be 100 characters or less", exception.Errors);
        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockBotRepository.Verify(x => x.AddAsync(It.IsAny<Bot>()), Times.Never);
    }

    #endregion

    #region User Validation Tests



    #endregion

    #region Cross-Cutting Validation Tests

    [Fact]
    public async Task Webhook_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateWebhookDto
        {
            Name = "Valid Name",
            BotId = 1,
            TopicId = null,
            MessageTemplate = "Valid message template"
        };

        var createdWebhook = new Webhook
        {
            Id = 1,
            Name = request.Name,
            Uuid = "test-uuid",
            BotId = request.BotId,
            MessageTemplate = request.MessageTemplate
        };

        _mockWebhookRepository.Setup(x => x.AddAsync(It.IsAny<Webhook>()))
            .ReturnsAsync(createdWebhook);

        // Act
        await _webhookManagementService.CreateWebhookAsync(request);

        // Assert
        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockWebhookRepository.Verify(x => x.AddAsync(It.IsAny<Webhook>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Bot_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateBotDto
        {
            Name = "Valid Name",
            BotToken = "123456789:ABCdefGHIjklMNOpqrsTUVwxyz-1234567890",
            ChatId = "123456789"
        };

        var createdConfig = new Bot
        {
            Id = 1,
            Name = request.Name,
            BotToken = request.BotToken,
            ChatId = request.ChatId
        };

        _mockBotRepository.Setup(x => x.AddAsync(It.IsAny<Bot>()))
            .ReturnsAsync(createdConfig);

        // Act
        await _botManagementService.CreateBotAsync(request);

        // Assert
        _mockValidationService.Verify(x => x.ValidateAsync(request), Times.Once);
        _mockBotRepository.Verify(x => x.AddAsync(It.IsAny<Bot>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    #endregion
}
