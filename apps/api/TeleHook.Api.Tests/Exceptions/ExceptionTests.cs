using TeleHook.Api.Exceptions;
using Xunit;

namespace TeleHook.Api.Tests.Exceptions;

public class ExceptionTests
{
    [Fact]
    public void BadRequestException_ShouldCreateWithMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new BadRequestException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.Details);
    }

    [Fact]
    public void BadRequestException_ShouldCreateWithMessageAndDetails()
    {
        // Arrange
        var message = "Test error message";
        var details = new[] { "Detail 1", "Detail 2" };

        // Act
        var exception = new BadRequestException(message, details);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(details, exception.Details);
    }

    [Fact]
    public void ValidationException_ShouldCreateWithErrors()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        Assert.Equal("Validation failed", exception.Message);
        Assert.Equal(errors, exception.Errors);
    }

    [Fact]
    public void PayloadValidationException_ShouldCreateWithErrors()
    {
        // Arrange
        var errors = new[] { "Invalid field", "Missing required field" };

        // Act
        var exception = new PayloadValidationException(errors);

        // Assert
        Assert.Equal("Payload validation failed", exception.Message);
        Assert.Equal(errors, exception.ValidationErrors);
    }

    [Fact]
    public void NotFoundException_ShouldCreateWithResourceTypeAndId()
    {
        // Arrange
        var resourceType = "User";
        var id = 123;

        // Act
        var exception = new NotFoundException(resourceType, id);

        // Assert
        Assert.Equal($"{resourceType} with ID '{id}' was not found", exception.Message);
    }

    [Fact]
    public void NotFoundException_ShouldCreateWithResourceTypeAndStringId()
    {
        // Arrange
        var resourceType = "Webhook";
        var uuid = "550e8400-e29b-41d4-a716-446655440000";

        // Act
        var exception = new NotFoundException(resourceType, uuid);

        // Assert
        Assert.Equal($"{resourceType} with ID '{uuid}' was not found", exception.Message);
    }

    [Fact]
    public void ConflictException_ShouldCreateWithResourceTypeFieldAndValue()
    {
        // Arrange
        var resourceType = "Bot";
        var field = "name";
        var value = "TestBot";

        // Act
        var exception = new ConflictException(resourceType, field, value);

        // Assert
        Assert.Equal($"{resourceType} with {field} '{value}' already exists", exception.Message);
    }

    [Fact]
    public void TelegramApiException_ShouldCreateWithMessage()
    {
        // Arrange
        var message = "Telegram API error";

        // Act
        var exception = new TelegramApiException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.StatusCode);
        Assert.Null(exception.ErrorCode);
    }

    [Fact]
    public void TelegramApiException_ShouldCreateWithStatusCodeAndErrorCode()
    {
        // Arrange
        var message = "Telegram API error";
        var statusCode = 400;
        var errorCode = "bad_request";

        // Act
        var exception = new TelegramApiException(message, statusCode, errorCode);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(statusCode, exception.StatusCode);
        Assert.Equal(errorCode, exception.ErrorCode);
    }

    [Fact]
    public void UnauthorizedException_ShouldCreateWithMessage()
    {
        // Arrange
        var message = "Unauthorized access";

        // Act
        var exception = new UnauthorizedException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ForbiddenException_ShouldCreateWithMessage()
    {
        // Arrange
        var message = "Access forbidden";

        // Act
        var exception = new ForbiddenException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InternalServerErrorException_ShouldCreateWithMessage()
    {
        // Arrange
        var message = "Internal server error occurred";

        // Act
        var exception = new InternalServerErrorException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InternalServerErrorException_ShouldCreateWithMessageAndInnerException()
    {
        // Arrange
        var message = "Internal server error occurred";
        var innerException = new Exception("Database connection failed");

        // Act
        var exception = new InternalServerErrorException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }
}