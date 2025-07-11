using FluentValidation.TestHelper;
using Moq;
using TeleHook.Api.DTO;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Validators;
using Xunit;

namespace TeleHook.Api.Tests.Validators;

public class BotCreateRequestValidatorTests
{
    private readonly Mock<IBotRepository> _mockRepository;
    private readonly BotCreateRequestValidator _validator;

    public BotCreateRequestValidatorTests()
    {
        _mockRepository = new Mock<IBotRepository>();
        _validator = new BotCreateRequestValidator(_mockRepository.Object);
    }

    [Fact]
    public async Task Name_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateBotDto { Name = "" };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    [Fact]
    public async Task Name_WhenNull_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateBotDto { Name = null! };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    [Fact]
    public async Task Name_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('a', 101);
        var request = new CreateBotDto { Name = longName };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must be 100 characters or less");
    }

    [Fact]
    public async Task Name_WhenNotUnique_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateBotDto { Name = "Existing Config" };
        _mockRepository.Setup(x => x.ExistsByNameAsync("Existing Config"))
                      .ReturnsAsync(true);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("A Bot with this name already exists");
    }

    [Fact]
    public async Task BotToken_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateBotDto { BotToken = "" };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.BotToken)
              .WithErrorMessage("BotToken is required");
    }

    [Theory]
    [InlineData("123456789:ABC-DEF_123")]
    [InlineData("987654321:XYZ_abc-789")]
    [InlineData("111:A")]
    public async Task BotToken_WhenValidFormat_ShouldNotHaveValidationError(string botToken)
    {
        // Arrange
        var request = CreateValidRequest();
        request.BotToken = botToken;
        SetupValidRepositoryMocks(request);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.BotToken);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("123456789")]
    [InlineData(":ABC123")]
    [InlineData("123:")]
    [InlineData("abc:123")]
    [InlineData("123:ABC@123")]
    public async Task BotToken_WhenInvalidFormat_ShouldHaveValidationError(string botToken)
    {
        // Arrange
        var request = new CreateBotDto { BotToken = botToken };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.BotToken)
              .WithErrorMessage("BotToken format is invalid. Expected format: 123456789:ABCdefGHI...");
    }

    [Fact]
    public async Task ChatId_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateBotDto { ChatId = "" };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.ChatId)
              .WithErrorMessage("ChatId is required");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("-123456789")]
    [InlineData("0")]
    [InlineData("-1")]
    public async Task ChatId_WhenValidFormat_ShouldNotHaveValidationError(string chatId)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ChatId = chatId;
        SetupValidRepositoryMocks(request);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ChatId);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12.5")]
    [InlineData("12a")]
    [InlineData("a123")]
    [InlineData("1-23")]
    public async Task ChatId_WhenInvalidFormat_ShouldHaveValidationError(string chatId)
    {
        // Arrange
        var request = new CreateBotDto { ChatId = chatId };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.ChatId)
              .WithErrorMessage("ChatId format is invalid. Must be a numeric value");
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

    private CreateBotDto CreateValidRequest()
    {
        return new CreateBotDto
        {
            Name = "Test Config",
            BotToken = "123456789:ABCdefGHI_123-xyz",
            ChatId = "123456789"
        };
    }

    private void SetupValidRepositoryMocks(CreateBotDto dto)
    {
        _mockRepository.Setup(x => x.ExistsByNameAsync(dto.Name))
                      .ReturnsAsync(false);
    }
}

public class BotUpdateRequestValidatorTests
{
    private readonly Mock<IBotRepository> _mockRepository;
    private readonly BotUpdateRequestValidator _validator;

    public BotUpdateRequestValidatorTests()
    {
        _mockRepository = new Mock<IBotRepository>();
        _validator = new BotUpdateRequestValidator(_mockRepository.Object);
    }

    [Fact]
    public async Task Id_WhenZero_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateBotDto { Id = 0 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id must be greater than 0");
    }

    [Fact]
    public async Task Id_WhenDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateBotDto { Id = 999 };
        _mockRepository.Setup(x => x.ExistsAsync(999))
                      .ReturnsAsync(false);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Bot with this Id does not exist");
    }

    [Fact]
    public async Task Name_WhenNotUniqueForUpdate_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateBotDto { Id = 1, Name = "Existing Config" };
        _mockRepository.Setup(x => x.ExistsAsync(1))
                      .ReturnsAsync(true);
        _mockRepository.Setup(x => x.ExistsByNameExcludingIdAsync("Existing Config", 1))
                      .ReturnsAsync(true);

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("A Bot with this name already exists");
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

    private UpdateBotDto CreateValidUpdateRequest()
    {
        return new UpdateBotDto
        {
            Id = 1,
            Name = "Updated Config",
            BotToken = "123456789:ABCdefGHI_123-xyz",
            ChatId = "123456789"
        };
    }

    private void SetupValidUpdateRepositoryMocks(UpdateBotDto updateBotRequest)
    {
        _mockRepository.Setup(x => x.ExistsAsync(updateBotRequest.Id))
                      .ReturnsAsync(true);
        _mockRepository.Setup(x => x.ExistsByNameExcludingIdAsync(updateBotRequest.Name, updateBotRequest.Id))
                      .ReturnsAsync(false);
    }
}
