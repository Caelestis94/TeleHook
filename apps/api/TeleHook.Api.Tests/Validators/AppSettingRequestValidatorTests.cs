using FluentValidation.TestHelper;
using TeleHook.Api.DTO;
using TeleHook.Api.Validators;
using Xunit;

namespace TeleHook.Api.Tests.Validators;

public class AppSettingRequestValidatorTests
{
    private readonly AppSettingRequestValidator _validator;

    public AppSettingRequestValidatorTests()
    {
        _validator = new AppSettingRequestValidator();
    }

    [Fact]
    public async Task LogLevel_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto { LogLevel = "" };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.LogLevel)
              .WithErrorMessage("LogLevel is required");
    }

    [Fact]
    public async Task LogLevel_WhenNull_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto { LogLevel = null! };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.LogLevel)
              .WithErrorMessage("LogLevel is required");
    }

    [Fact]
    public async Task LogLevel_WhenInvalid_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto { LogLevel = "InvalidLevel" };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.LogLevel)
              .WithErrorMessage("LogLevel must be one of: Trace, Debug, Information, Warning, Error, Critical, None");
    }

    [Theory]
    [InlineData("Trace")]
    [InlineData("Debug")]
    [InlineData("Information")]
    [InlineData("Warning")]
    [InlineData("Error")]
    [InlineData("Critical")]
    [InlineData("None")]
    public async Task LogLevel_WhenValid_ShouldNotHaveValidationError(string logLevel)
    {
        // Arrange
        var request = new AppSettingDto { LogLevel = logLevel };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.LogLevel);
    }

    [Fact]
    public async Task LogRetentionDays_WhenLessThanOne_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto { LogRetentionDays = 0 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.LogRetentionDays)
              .WithErrorMessage("LogRetentionDays must be at least 1 day");
    }

    [Fact]
    public async Task LogRetentionDays_WhenGreaterThan365_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto { LogRetentionDays = 366 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.LogRetentionDays)
              .WithErrorMessage("LogRetentionDays cannot exceed 365 days");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(365)]
    public async Task LogRetentionDays_WhenValid_ShouldNotHaveValidationError(int days)
    {
        // Arrange
        var request = new AppSettingDto { LogRetentionDays = days };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.LogRetentionDays);
    }

    [Fact]
    public async Task WebhookLogRetentionDays_WhenNegative_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto { WebhookLogRetentionDays = -1 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.WebhookLogRetentionDays)
              .WithErrorMessage("WebhookLogRetentionDays must be 0 or greater (0 = keep forever)");
    }

    [Fact]
    public async Task WebhookLogRetentionDays_WhenGreaterThan365_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto { WebhookLogRetentionDays = 366 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.WebhookLogRetentionDays)
              .WithErrorMessage("WebhookLogRetentionDays cannot exceed 365 days");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(365)]
    public async Task WebhookLogRetentionDays_WhenValid_ShouldNotHaveValidationError(int days)
    {
        // Arrange
        var request = new AppSettingDto { WebhookLogRetentionDays = days };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.WebhookLogRetentionDays);
    }

    [Fact]
    public async Task StatsDaysInterval_WhenLessThanOne_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto { StatsDaysInterval = 0 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.StatsDaysInterval)
              .WithErrorMessage("StatsDaysInterval must be at least 1 day");
    }

    [Fact]
    public async Task StatsDaysInterval_WhenGreaterThan365_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto { StatsDaysInterval = 366 };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.StatsDaysInterval)
              .WithErrorMessage("StatsDaysInterval cannot exceed 365 days");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(365)]
    public async Task StatsDaysInterval_WhenValid_ShouldNotHaveValidationError(int days)
    {
        // Arrange
        var request = new AppSettingDto { StatsDaysInterval = days };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.StatsDaysInterval);
    }

    [Fact]
    public async Task ValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new AppSettingDto
        {
            LogLevel = "Information",
            LogRetentionDays = 7,
            EnableWebhookLogging = true,
            WebhookLogRetentionDays = 30,
            StatsDaysInterval = 30
        };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // Notification Tests
    [Fact]
    public async Task NotificationBotToken_WhenNotificationsEnabledAndTokenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto
        {
            LogLevel = "Information",
            EnableFailureNotifications = true,
            NotificationBotToken = "",
            NotificationChatId = "123456789"
        };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.NotificationBotToken)
              .WithErrorMessage("Notification bot token is required when failure notifications are enabled");
    }

    [Fact]
    public async Task NotificationChatId_WhenNotificationsEnabledAndChatIdEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto
        {
            LogLevel = "Information",
            EnableFailureNotifications = true,
            NotificationBotToken = "123456789:ABCdefGHIjklMNOpqrSTUvwxyz123456789",
            NotificationChatId = ""
        };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.NotificationChatId)
              .WithErrorMessage("Notification chat ID is required when failure notifications are enabled");
    }

    [Fact]
    public async Task NotificationBotToken_WhenNotificationsDisabled_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new AppSettingDto
        {
            LogLevel = "Information",
            EnableFailureNotifications = false,
            NotificationBotToken = "",
            NotificationChatId = ""
        };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.NotificationBotToken);
        result.ShouldNotHaveValidationErrorFor(x => x.NotificationChatId);
    }

    [Theory]
    [InlineData("123456789:ABCdefGHIjklMNOpqrSTUvwxyz123456789")]
    [InlineData("987654321:XYZabcDEFghiJKLmnoPQRstUVwxYZ987654321")]
    public async Task NotificationBotToken_WhenValidFormat_ShouldNotHaveValidationError(string token)
    {
        // Arrange
        var request = new AppSettingDto
        {
            LogLevel = "Information",
            EnableFailureNotifications = true,
            NotificationBotToken = token,
            NotificationChatId = "123456789"
        };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.NotificationBotToken);
    }

    [Theory]
    [InlineData("invalid-token")]
    [InlineData("123456789")]
    [InlineData("123456789:")]
    [InlineData(":ABCdefGHIjklMNOpqrSTUvwxyz123456789")]

    public async Task NotificationBotToken_WhenInvalidFormat_ShouldHaveValidationError(string token)
    {
        // Arrange
        var request = new AppSettingDto
        {
            LogLevel = "Information",
            EnableFailureNotifications = true,
            NotificationBotToken = token,
            NotificationChatId = "123456789"
        };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.NotificationBotToken)
              .WithErrorMessage("Notification bot token must be a valid Telegram bot token format");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("-123456789")]
    [InlineData("987654321")]
    public async Task NotificationChatId_WhenValidFormat_ShouldNotHaveValidationError(string chatId)
    {
        // Arrange
        var request = new AppSettingDto
        {
            LogLevel = "Information",
            EnableFailureNotifications = true,
            NotificationBotToken = "123456789:ABCdefGHIjklMNOpqrSTUvwxyz123456789",
            NotificationChatId = chatId
        };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.NotificationChatId);
    }

    [Theory]
    [InlineData("invalid-chat-id")]
    [InlineData("123abc")]
    [InlineData("")]
    public async Task NotificationChatId_WhenInvalidFormat_ShouldHaveValidationError(string chatId)
    {
        // Arrange
        var request = new AppSettingDto
        {
            LogLevel = "Information",
            EnableFailureNotifications = true,
            NotificationBotToken = "123456789:ABCdefGHIjklMNOpqrSTUvwxyz123456789",
            NotificationChatId = chatId
        };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.NotificationChatId)
              .WithErrorMessage("Notification chat ID must be a valid chat ID format");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("456789")]
    [InlineData(null)]
    [InlineData("")]
    public async Task NotificationTopicId_WhenValidFormat_ShouldNotHaveValidationError(string topicId)
    {
        // Arrange
        var request = new AppSettingDto
        {
            LogLevel = "Information",
            EnableFailureNotifications = true,
            NotificationBotToken = "123456789:ABCdefGHIjklMNOpqrSTUvwxyz123456789",
            NotificationChatId = "123456789",
            NotificationTopicId = topicId
        };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveValidationErrorFor(x => x.NotificationTopicId);
    }

    [Theory]
    [InlineData("invalid-topic")]
    [InlineData("123abc")]
    [InlineData("-123")]
    public async Task NotificationTopicId_WhenInvalidFormat_ShouldHaveValidationError(string topicId)
    {
        // Arrange
        var request = new AppSettingDto
        {
            LogLevel = "Information",
            EnableFailureNotifications = true,
            NotificationBotToken = "123456789:ABCdefGHIjklMNOpqrSTUvwxyz123456789",
            NotificationChatId = "123456789",
            NotificationTopicId = topicId
        };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.NotificationTopicId)
              .WithErrorMessage("Notification topic ID must be a valid topic ID format");
    }

    [Fact]
    public async Task NotificationSettings_WhenCompleteAndEnabled_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new AppSettingDto
        {
            LogLevel = "Information",
            LogRetentionDays = 7,
            EnableWebhookLogging = true,
            WebhookLogRetentionDays = 30,
            StatsDaysInterval = 30,
            EnableFailureNotifications = true,
            NotificationBotToken = "123456789:ABCdefGHIjklMNOpqrSTUvwxyz123456789",
            NotificationChatId = "-123456789",
            NotificationTopicId = "456"
        };

        // Act & Assert
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}