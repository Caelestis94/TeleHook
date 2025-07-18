using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Middleware;
using TeleHook.Api.Models;
using Xunit;

namespace TeleHook.Api.Tests.Middlewares;

public class GlobalExceptionMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _mockLogger = new();
    private readonly DefaultHttpContext _httpContext = new()
    {
        Response =
        {
            Body = new MemoryStream()
        }
    };

    [Fact]
    public async Task InvokeAsync_ShouldHandleValidationException()
    {
        // Arrange
        var errors = new[] { "Field is required", "Invalid format" };
        var exception = new ValidationException(errors);
        var middleware = new GlobalExceptionMiddleware(
            context => throw exception,
            _mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Validation failed", response.Message);
        Assert.Equal(errors, response.Details);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleNotFoundException()
    {
        // Arrange
        var exception = new NotFoundException("User", 123);
        var middleware = new GlobalExceptionMiddleware(
            context => throw exception,
            _mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("User with ID '123' was not found", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleConflictException()
    {
        // Arrange
        var exception = new ConflictException("Bot", "name", "TestBot");
        var middleware = new GlobalExceptionMiddleware(
            context => throw exception,
            _mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.Conflict, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Bot with name 'TestBot' already exists", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleBadRequestException()
    {
        // Arrange
        var details = new[] { "Invalid input" };
        var exception = new BadRequestException("Bad request", details);
        var middleware = new GlobalExceptionMiddleware(
            context => throw exception,
            _mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Bad request", response.Message);
        Assert.Equal(details, response.Details);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleTelegramApiException()
    {
        // Arrange
        var exception = new TelegramApiException("Telegram API error");
        var middleware = new GlobalExceptionMiddleware(
            context => throw exception,
            _mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadGateway, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal("Telegram API error occurred", response.Message);
        Assert.Contains("Telegram API error", response.Details!);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandlePayloadValidationException()
    {
        // Arrange
        var validationErrors = new[] { "Invalid payload", "Missing field" };
        var exception = new PayloadValidationException(validationErrors);
        var middleware = new GlobalExceptionMiddleware(
            context => throw exception,
            _mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Payload validation failed", response.Message);
        Assert.Equal(validationErrors, response.Details);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleInternalServerErrorException()
    {
        // Arrange
        var exception = new InternalServerErrorException("Database failure");
        var middleware = new GlobalExceptionMiddleware(
            context => throw exception,
            _mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("Database failure", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleUnauthorizedException()
    {
        // Arrange
        var exception = new UnauthorizedException("Token expired");
        var middleware = new GlobalExceptionMiddleware(
            context => throw exception,
            _mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.Unauthorized, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleForbiddenException()
    {
        // Arrange
        var exception = new ForbiddenException("Access denied");
        var middleware = new GlobalExceptionMiddleware(
            context => throw exception,
            _mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.Forbidden, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleGenericException()
    {
        // Arrange
        var exception = new Exception("Unexpected error");
        var middleware = new GlobalExceptionMiddleware(
            context => throw exception,
            _mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("An internal server error occurred", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_ShouldIncludeTraceId()
    {
        // Arrange
        var traceId = "test-trace-id";
        _httpContext.TraceIdentifier = traceId;
        var exception = new BadRequestException("Test error");
        var middleware = new GlobalExceptionMiddleware(
            context => throw exception,
            _mockLogger.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(response);
        Assert.Equal(traceId, response.TraceId);
    }
}
