using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Middleware;
using TeleHook.Api.Middleware.Attributes;
using TeleHook.Api.Repositories.Interfaces;
using Xunit;

namespace TeleHook.Api.Tests.Middlewares;

public class ApiKeyAuthenticationMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ApiKeyAuthenticationMiddleware _middleware;
    private readonly Mock<ILogger<ApiKeyAuthenticationMiddleware>> _mockLogger;
    private const string ValidApiKey = "test-api-key-123";

    public ApiKeyAuthenticationMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<ApiKeyAuthenticationMiddleware>>();
        var mockConfiguration = new Mock<IConfiguration>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        // Setup default configuration
        mockConfiguration.Setup(x => x["Security:ApiKey"])
            .Returns(ValidApiKey);

        _middleware = new ApiKeyAuthenticationMiddleware(_mockNext.Object, mockConfiguration.Object,
            _mockLogger.Object);
    }

    private DefaultHttpContext CreateHttpContext(string path, string? apiKey = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        
        if (apiKey != null)
        {
            context.Request.Headers["X-API-KEY"] = apiKey;
        }

        return context;
    }

    private DefaultHttpContext CreateHttpContextWithAnonymousEndpoint(string path, string? apiKey = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        
        if (apiKey != null)
        {
            context.Request.Headers["X-API-KEY"] = apiKey;
        }

        // Create real endpoint with AllowAnonymous attribute
        var metadata = new EndpointMetadataCollection(new AllowAnonymousAttribute());
        var endpoint = new Endpoint(
            requestDelegate: (context) => Task.CompletedTask,
            metadata: metadata,
            displayName: "Test Endpoint");
        
        context.SetEndpoint(endpoint);

        return context;
    }

    #region Skip Authentication Tests

    [Fact]
    public async Task InvokeAsync_WithTriggerEndpoint_SkipsAuthentication()
    {
        // Arrange
        var context = CreateHttpContextWithAnonymousEndpoint("/api/trigger/some-uuid");

        // Act
        await _middleware.InvokeAsync(context, _mockUnitOfWork.Object);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Theory]
    [InlineData("/api/trigger/")]
    [InlineData("/api/trigger/abc123")]
    [InlineData("/api/TRIGGER/xyz789")]
    public async Task InvokeAsync_WithTriggerEndpointVariations_SkipsAuthentication(string path)
    {
        // Arrange
        var context = CreateHttpContextWithAnonymousEndpoint(path);

        // Act
        await _middleware.InvokeAsync(context, _mockUnitOfWork.Object);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Theory]
    [InlineData("/swagger")]
    [InlineData("/swagger/index.html")]
    [InlineData("/health")]
    [InlineData("/")]
    [InlineData("/some-other-path")]
    public async Task InvokeAsync_WithNonApiRoutes_SkipsAuthentication(string path)
    {
        // Arrange
        var context = CreateHttpContext(path);

        // Act
        await _middleware.InvokeAsync(context, _mockUnitOfWork.Object);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithNullPath_SkipsAuthentication()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = PathString.Empty;

        // Act
        await _middleware.InvokeAsync(context, _mockUnitOfWork.Object);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    #endregion

    #region API Key Validation Tests

    [Fact]
    public async Task InvokeAsync_WithValidApiKey_CallsNext()
    {
        // Arrange
        var context = CreateHttpContext("/api/webhooks", ValidApiKey);

        // Act
        await _middleware.InvokeAsync(context, _mockUnitOfWork.Object);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithMissingApiKey_ThrowsUnauthorizedException()
    {
        // Arrange
        var context = CreateHttpContext("/api/webhooks");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _middleware.InvokeAsync(context, _mockUnitOfWork.Object));
        
        Assert.Equal("API Key is required", exception.Message);
        _mockNext.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidApiKey_ThrowsUnauthorizedException()
    {
        // Arrange
        var context = CreateHttpContext("/api/webhooks", "invalid-api-key");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _middleware.InvokeAsync(context, _mockUnitOfWork.Object));
        
        Assert.Equal("Invalid API Key", exception.Message);
        _mockNext.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyApiKey_ThrowsUnauthorizedException()
    {
        // Arrange
        var context = CreateHttpContext("/api/webhooks", "");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _middleware.InvokeAsync(context, _mockUnitOfWork.Object));
        
        Assert.Equal("Invalid API Key", exception.Message);
        _mockNext.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespaceApiKey_ThrowsUnauthorizedException()
    {
        // Arrange
        var context = CreateHttpContext("/api/webhooks", "   ");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _middleware.InvokeAsync(context, _mockUnitOfWork.Object));
        
        Assert.Equal("Invalid API Key", exception.Message);
        _mockNext.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public async Task InvokeAsync_WithMissingConfiguredApiKey_ThrowsInternalServerErrorException()
    {
        // Arrange
        var mockConfigWithoutApiKey = new Mock<IConfiguration>();
        mockConfigWithoutApiKey.Setup(x => x["Security:ApiKey"])
            .Returns((string)null);

        var middleware = new ApiKeyAuthenticationMiddleware(_mockNext.Object, mockConfigWithoutApiKey.Object,
            _mockLogger.Object);
        var context = CreateHttpContext("/api/webhooks", ValidApiKey);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InternalServerErrorException>(() =>
            middleware.InvokeAsync(context, _mockUnitOfWork.Object));
        
        Assert.Equal("API Key not configured", exception.Message);
        _mockNext.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyConfiguredApiKey_ThrowsInternalServerErrorException()
    {
        // Arrange
        var mockConfigWithEmptyApiKey = new Mock<IConfiguration>();
        mockConfigWithEmptyApiKey.Setup(x => x["Security:ApiKey"])
            .Returns("");

        var middleware = new ApiKeyAuthenticationMiddleware(_mockNext.Object, mockConfigWithEmptyApiKey.Object,
            _mockLogger.Object);
        var context = CreateHttpContext("/api/webhooks", ValidApiKey);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InternalServerErrorException>(() =>
            middleware.InvokeAsync(context, _mockUnitOfWork.Object));
        
        Assert.Equal("API Key not configured", exception.Message);
        _mockNext.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public async Task InvokeAsync_ApiKeyComparisonIsCaseSensitive()
    {
        // Arrange
        var context = CreateHttpContext("/api/webhooks", ValidApiKey.ToUpper());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _middleware.InvokeAsync(context, _mockUnitOfWork.Object));
        
        Assert.Equal("Invalid API Key", exception.Message);
        _mockNext.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Theory]
    [InlineData("/API/webhooks")]
    [InlineData("/Api/Webhooks")]
    [InlineData("/api/WEBHOOKS")]
    public async Task InvokeAsync_PathComparisonIsCaseInsensitive(string path)
    {
        // Arrange
        var context = CreateHttpContext(path, ValidApiKey);

        // Act
        await _middleware.InvokeAsync(context, _mockUnitOfWork.Object);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    #endregion

    #region Header Name Tests

    [Fact]
    public async Task InvokeAsync_WithDifferentHeaderName_ThrowsUnauthorizedException()
    {
        // Arrange
        var context = CreateHttpContext("/api/webhooks");
        context.Request.Headers["API-KEY"] = ValidApiKey; // Wrong header name

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _middleware.InvokeAsync(context, _mockUnitOfWork.Object));
        
        Assert.Equal("API Key is required", exception.Message);
    }

    [Fact]
    public async Task InvokeAsync_HeaderNameIsCaseInsensitive()
    {
        // Arrange
        var context = CreateHttpContext("/api/webhooks");
        context.Request.Headers["x-api-key"] = ValidApiKey; // Lowercase header name

        // Act
        await _middleware.InvokeAsync(context, _mockUnitOfWork.Object);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    #endregion

    #region Multiple API Endpoints Tests

    [Theory]
    [InlineData("/api/webhooks")]
    [InlineData("/api/bot")]
    [InlineData("/api/schema")]
    [InlineData("/api/user")]
    [InlineData("/api/template")]
    public async Task InvokeAsync_WithValidApiKeyOnDifferentEndpoints_CallsNext(string path)
    {
        // Arrange
        var context = CreateHttpContext(path, ValidApiKey);

        // Act
        await _middleware.InvokeAsync(context, _mockUnitOfWork.Object);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Theory]
    [InlineData("/api/webhooks")]
    [InlineData("/api/bot")]
    [InlineData("/api/schema")]
    [InlineData("/api/user")]
    [InlineData("/api/template")]
    public async Task InvokeAsync_WithoutApiKeyOnDifferentEndpoints_ThrowsUnauthorizedException(string path)
    {
        // Arrange
        var context = CreateHttpContext(path);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _middleware.InvokeAsync(context, _mockUnitOfWork.Object));
        
        Assert.Equal("API Key is required", exception.Message);
    }

    #endregion
}