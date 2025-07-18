using FluentValidation.TestHelper;
using Moq;
using TeleHook.Api.DTO;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Validators;
using Xunit;

namespace TeleHook.Api.Tests.Validators;

public class WebhookCreateRequestValidatorTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IWebhookRepository> _mockWebhookRepository;
    private readonly Mock<IBotRepository> _mockBotRepository;
    private readonly WebhookCreateRequestValidator _validator;

    public WebhookCreateRequestValidatorTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockWebhookRepository = new Mock<IWebhookRepository>();
        _mockBotRepository = new Mock<IBotRepository>();

        _mockUnitOfWork.Setup(x => x.Webhooks).Returns(_mockWebhookRepository.Object);
        _mockUnitOfWork.Setup(x => x.Bots).Returns(_mockBotRepository.Object);

        _validator = new WebhookCreateRequestValidator(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Name_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateWebhookDto { Name = "" };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    [Fact]
    public async Task Name_WhenNull_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateWebhookDto { Name = null! };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    [Fact]
    public async Task Name_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new CreateWebhookDto { Name = longName };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must be 200 characters or less");
    }

    [Fact]
    public async Task Name_WhenNotUnique_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateWebhookDto { Name = "Existing Name" };
        _mockWebhookRepository.Setup(x => x.ExistsByNameAsync("Existing Name"))
                                     .ReturnsAsync(true);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("A webhook with this name already exists");
    }

    [Fact]
    public async Task Name_WhenValidAndUnique_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        _mockWebhookRepository.Setup(x => x.ExistsByNameAsync(request.Name))
                                     .ReturnsAsync(false);
        _mockBotRepository.Setup(x => x.ExistsAsync(request.BotId))
                                    .ReturnsAsync(true);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task BotId_WhenZero_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateWebhookDto { BotId = 0 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.BotId)
              .WithErrorMessage("BotId must be greater than 0");
    }

    [Fact]
    public async Task BotId_WhenNegative_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateWebhookDto { BotId = -1 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.BotId)
              .WithErrorMessage("BotId must be greater than 0");
    }

    [Fact]
    public async Task BotId_WhenDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateWebhookDto { BotId = 999 };
        _mockBotRepository.Setup(x => x.ExistsAsync(999))
                                    .ReturnsAsync(false);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.BotId)
              .WithErrorMessage("The specified Bot does not exist");
    }

    [Fact]
    public async Task MessageTemplate_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateWebhookDto { MessageTemplate = "" };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.MessageTemplate)
              .WithErrorMessage("MessageTemplate is required");
    }

    [Fact]
    public async Task MessageTemplate_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longTemplate = new string('a', 4001);
        var request = new CreateWebhookDto { MessageTemplate = longTemplate };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.MessageTemplate)
              .WithErrorMessage("MessageTemplate must be 4000 characters or less");
    }

    [Fact]
    public async Task ParseMode_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateWebhookDto { ParseMode = "" };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.ParseMode)
              .WithErrorMessage("ParseMode is required");
    }

    [Theory]
    [InlineData("Markdown")]
    [InlineData("MarkdownV2")]
    [InlineData("HTML")]
    public async Task ParseMode_WhenValid_ShouldNotHaveValidationError(string parseMode)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ParseMode = parseMode;
        SetupValidRepositoryMocks(request);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ParseMode);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("TEXT")]
    [InlineData("markdown")]
    [InlineData("html")]
    public async Task ParseMode_WhenInvalid_ShouldHaveValidationError(string parseMode)
    {
        // Arrange
        var request = new CreateWebhookDto { ParseMode = parseMode };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.ParseMode)
              .WithErrorMessage("ParseMode must be one of: Markdown, MarkdownV2, HTML");
    }

    [Fact]
    public async Task TopicId_WhenEmpty_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TopicId = "";
        SetupValidRepositoryMocks(request);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.TopicId);
    }

    [Fact]
    public async Task TopicId_WhenNull_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TopicId = null;
        SetupValidRepositoryMocks(request);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.TopicId);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("0")]
    [InlineData("999999")]
    public async Task TopicId_WhenNumeric_ShouldNotHaveValidationError(string topicId)
    {
        // Arrange
        var request = CreateValidRequest();
        request.TopicId = topicId;
        SetupValidRepositoryMocks(request);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.TopicId);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12a")]
    [InlineData("1.5")]
    [InlineData("test123")]
    public async Task TopicId_WhenNotNumeric_ShouldHaveValidationError(string topicId)
    {
        // Arrange
        var request = new CreateWebhookDto { TopicId = topicId };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.TopicId)
              .WithErrorMessage("TopicId must be numeric");
    }

    [Fact]
    public async Task ValidRequest_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var request = CreateValidRequest();
        SetupValidRepositoryMocks(request);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private CreateWebhookDto CreateValidRequest()
    {
        return new CreateWebhookDto
        {
            Name = "Test Webhook",
            BotId = 1,
            MessageTemplate = "Test message: {{data}}",
            ParseMode = "Markdown",
            TopicId = "123"
        };
    }

    private void SetupValidRepositoryMocks(CreateWebhookDto createWebhookRequest)
    {
        _mockWebhookRepository.Setup(x => x.ExistsByNameAsync(createWebhookRequest.Name))
                                     .ReturnsAsync(false);
        _mockBotRepository.Setup(x => x.ExistsAsync(createWebhookRequest.BotId))
                                    .ReturnsAsync(true);
    }
}

public class WebhookUpdateRequestValidatorTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IWebhookRepository> _mockWebhookRepository;
    private readonly Mock<IBotRepository> _mockBotRepository;
    private readonly WebhookUpdateRequestValidator _validator;

    public WebhookUpdateRequestValidatorTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockWebhookRepository = new Mock<IWebhookRepository>();
        _mockBotRepository = new Mock<IBotRepository>();

        _mockUnitOfWork.Setup(x => x.Webhooks).Returns(_mockWebhookRepository.Object);
        _mockUnitOfWork.Setup(x => x.Bots).Returns(_mockBotRepository.Object);

        _validator = new WebhookUpdateRequestValidator(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Id_WhenZero_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateWebhookDto { Id = 0 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id must be greater than 0");
    }

    [Fact]
    public async Task Id_WhenNegative_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateWebhookDto { Id = -1 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id must be greater than 0");
    }

    [Fact]
    public async Task Id_WhenDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateWebhookDto { Id = 999 };
        _mockWebhookRepository.Setup(x => x.ExistsAsync(999))
                                     .ReturnsAsync(false);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Webhook with this Id does not exist");
    }

    [Fact]
    public async Task Name_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateWebhookDto { Name = "" };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    [Fact]
    public async Task Name_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new UpdateWebhookDto { Name = longName };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must be 200 characters or less");
    }

    [Fact]
    public async Task Name_WhenNotUniqueForUpdate_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateWebhookDto { Id = 1, Name = "Existing Name" };
        _mockWebhookRepository.Setup(x => x.ExistsAsync(1))
                                     .ReturnsAsync(true);
        _mockWebhookRepository.Setup(x => x.ExistsByNameExcludingIdAsync("Existing Name", 1))
                                     .ReturnsAsync(true);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("A webhook with this name already exists");
    }

    [Fact]
    public async Task BotId_WhenZero_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateWebhookDto { BotId = 0 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.BotId)
              .WithErrorMessage("BotId must be greater than 0");
    }

    [Fact]
    public async Task BotId_WhenDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateWebhookDto { BotId = 999 };
        _mockBotRepository.Setup(x => x.ExistsAsync(999))
                                    .ReturnsAsync(false);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.BotId)
              .WithErrorMessage("The specified Bot does not exist");
    }

    [Fact]
    public async Task MessageTemplate_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateWebhookDto { MessageTemplate = "" };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.MessageTemplate)
              .WithErrorMessage("MessageTemplate is required");
    }

    [Fact]
    public async Task MessageTemplate_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longTemplate = new string('a', 4001);
        var request = new UpdateWebhookDto { MessageTemplate = longTemplate };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.MessageTemplate)
              .WithErrorMessage("MessageTemplate must be 4000 characters or less");
    }

    [Fact]
    public async Task ParseMode_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateWebhookDto { ParseMode = "" };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.ParseMode)
              .WithErrorMessage("ParseMode is required");
    }

    [Theory]
    [InlineData("Markdown")]
    [InlineData("MarkdownV2")]
    [InlineData("HTML")]
    public async Task ParseMode_WhenValid_ShouldNotHaveValidationError(string parseMode)
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        request.ParseMode = parseMode;
        SetupValidUpdateRepositoryMocks(request);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ParseMode);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("TEXT")]
    [InlineData("markdown")]
    public async Task ParseMode_WhenInvalid_ShouldHaveValidationError(string parseMode)
    {
        // Arrange
        var request = new UpdateWebhookDto { ParseMode = parseMode };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.ParseMode)
              .WithErrorMessage("ParseMode must be one of: Markdown, MarkdownV2, HTML");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("0")]
    [InlineData("999999")]
    public async Task TopicId_WhenNumeric_ShouldNotHaveValidationError(string topicId)
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        request.TopicId = topicId;
        SetupValidUpdateRepositoryMocks(request);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.TopicId);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12a")]
    [InlineData("1.5")]
    public async Task TopicId_WhenNotNumeric_ShouldHaveValidationError(string topicId)
    {
        // Arrange
        var request = new UpdateWebhookDto { TopicId = topicId };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.TopicId)
              .WithErrorMessage("TopicId must be numeric");
    }

    [Fact]
    public async Task ValidUpdateRequest_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        SetupValidUpdateRepositoryMocks(request);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private UpdateWebhookDto CreateValidUpdateRequest()
    {
        return new UpdateWebhookDto
        {
            Id = 1,
            Name = "Updated Webhook",
            BotId = 1,
            MessageTemplate = "Updated message: {{data}}",
            ParseMode = "Markdown",
            TopicId = "123"
        };
    }

    private void SetupValidUpdateRepositoryMocks(UpdateWebhookDto updateWebhookRequest)
    {
        _mockWebhookRepository.Setup(x => x.ExistsAsync(updateWebhookRequest.Id))
                                     .ReturnsAsync(true);
        _mockWebhookRepository.Setup(x => x.ExistsByNameExcludingIdAsync(updateWebhookRequest.Name, updateWebhookRequest.Id))
                                     .ReturnsAsync(false);
        _mockBotRepository.Setup(x => x.ExistsAsync(updateWebhookRequest.BotId))
                                    .ReturnsAsync(true);
    }
}
