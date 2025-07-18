using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Scriban;
using TeleHook.Api.Controllers;
using TeleHook.Api.DTO;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services;
using TeleHook.Api.Services.Domain;
using TeleHook.Api.Services.Infrastructure;
using TeleHook.Api.Services.Interfaces;
using TeleHook.Api.Services.Utilities;
using Xunit;

namespace TeleHook.Api.Tests.Integration;

/// <summary>
/// Integration tests for webhook processing using real services but mocked repositories
/// This approach tests the actual service interactions while controlling data access
/// </summary>
public class WebhookIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IWebhookRepository> _mockWebhookRepository;
    private readonly Mock<ITelegramService> _mockTelegramService;
    private readonly Mock<IWebhookLoggingService> _mockLoggingService;
    private readonly Mock<ITemplateParsingService> _mockTemplateParsingService;
    private readonly Mock<IFailureNotificationService> _mockFailureNotificationService;
    private readonly WebhookController _controller;

    public WebhookIntegrationTests()
    {
        var appSettings = new AppSettingDto()
        {
            LogRetentionDays = 7,
            EnableWebhookLogging = true,
            WebhookLogRetentionDays = 30,
            StatsDaysInterval = 30,
            LogLevel = "Debug"
        };

        // Create a service collection with real services
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        // Add helper services
        services.AddTransient<TelegramMessageEscaper>();
        services.AddTransient<ITelegramMessageEscaper, TelegramMessageEscaper>();
        services.AddTransient<IJsonToScribanConverter, JsonToScribanConverter>();

        // Add real services with their interfaces
        services.AddTransient<IMessageFormattingService, MessageFormattingService>();
        services.AddTransient<IValidationService, ValidationService>();
        services.AddTransient<IWebhookStatService, WebhookStatService>();
        services.AddTransient<IWebhookProcessingService, WebhookProcessingService>();
        services.AddTransient<IWebhookManagementService, WebhookManagementService>();

        // Create mocks for external dependencies
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockWebhookRepository = new Mock<IWebhookRepository>();
        _mockTelegramService = new Mock<ITelegramService>();
        _mockLoggingService = new Mock<IWebhookLoggingService>();
        _mockTemplateParsingService = new Mock<ITemplateParsingService>();
        _mockFailureNotificationService = new Mock<IFailureNotificationService>();

        // Register mocks as singletons
        services.AddSingleton(_mockUnitOfWork.Object);
        services.AddSingleton(_mockTelegramService.Object);
        services.AddSingleton(_mockLoggingService.Object);
        services.AddSingleton(_mockTemplateParsingService.Object);
        services.AddSingleton(_mockFailureNotificationService.Object);
        services.AddSingleton(appSettings);
        // Build service provider
        _serviceProvider = services.BuildServiceProvider();

        // Setup unit of work to return mocked repository
        _mockUnitOfWork.Setup(x => x.Webhooks).Returns(_mockWebhookRepository.Object);

        // Create controller with real and mocked services
        _controller = new WebhookController(
            _mockUnitOfWork.Object,
            _serviceProvider.GetRequiredService<IWebhookStatService>(),
            _serviceProvider.GetRequiredService<IWebhookManagementService>(),
            _serviceProvider.GetRequiredService<IWebhookProcessingService>()
        );
    }

    [Fact]
    public async Task ProcessWebhook_WithValidPayload_ShouldReturnOkAndSendMessage()
    {
        // Arrange
        var uuid = "550e8400-e29b-41d4-a716-446655440000";
        var payloadJson = """{"event": "test", "data": {"message": "Hello World"}}""";
        var requestId = "test-request-id-123";

        var bot = new Bot
        {
            Id = 1,
            Name = "Test Config",
            BotToken = "test-token",
            ChatId = "123456789"
        };


        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            Uuid = uuid,
            BotId = bot.Id,
            MessageTemplate = "Event: {{ event }}",
            ParseMode = "MarkdownV2",
            IsDisabled = false,
            Bot = bot,
        };

        // Setup repository mock
        _mockWebhookRepository.Setup(x => x.GetByUuidWithRelationsAsync(uuid))
            .ReturnsAsync(webhook);
        _mockWebhookRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Webhook>() { webhook });

        // Setup template parsing service mock
        var parsedTemplate = Template.Parse(webhook.MessageTemplate);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id))
            .Returns(parsedTemplate);

        // Setup external service mocks
        _mockLoggingService.Setup(x => x.StartRequestAsync(webhook.Id, It.IsAny<HttpRequest>()))
            .ReturnsAsync(requestId);
        _mockLoggingService.Setup(x => x.LogValidationResultAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<List<string>>()))
            .Returns(Task.CompletedTask);
        _mockLoggingService.Setup(x => x.LogMessageFormattingAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockLoggingService.Setup(x => x.LogTelegramResponseAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockLoggingService.Setup(x => x.CompleteRequestAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _mockTelegramService.Setup(x => x.SendMessageAsync(It.IsAny<Webhook>(), It.IsAny<string>()))
            .ReturnsAsync(TelegramResult.Success());

        // Setup HTTP context
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payloadJson));
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.Method = "POST";
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await _controller.Post(uuid, JsonDocument.Parse(payloadJson).RootElement);

        // Assert
        Assert.IsType<ObjectResult>(result);
        var okResult = (ObjectResult)result;
        Assert.Equal(200, okResult.StatusCode);

        // Check that the result contains the expected message structure
        var resultValue = okResult.Value;
        Assert.NotNull(resultValue);

        // The controller returns an anonymous object with a message property
        var messageProperty = resultValue.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal("Message forwarded successfully", messageProperty.GetValue(resultValue));

        // Verify interactions
        _mockWebhookRepository.Verify(x => x.GetByUuidWithRelationsAsync(uuid), Times.Once);
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.IsAny<Webhook>(), It.IsAny<string>()), Times.Once);
        _mockLoggingService.Verify(x => x.StartRequestAsync(webhook.Id, It.IsAny<HttpRequest>()), Times.Once);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
