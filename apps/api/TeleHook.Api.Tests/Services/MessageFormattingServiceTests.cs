using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Scriban;
using TeleHook.Api.Models;
using TeleHook.Api.Services;
using TeleHook.Api.Services.Domain;
using TeleHook.Api.Services.Interfaces;
using TeleHook.Api.Services.Utilities;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class MessageFormattingServiceTests
{
    private readonly MessageFormattingService _service;
    private readonly Mock<ITemplateParsingService> _mockTemplateParsingService;

    public MessageFormattingServiceTests()
    {
        var mockLogger = new Mock<ILogger<MessageFormattingService>>();
        var telegramEscaper = new TelegramMessageEscaper();
        var jsonConverter = new JsonToScribanConverter();
        _mockTemplateParsingService = new Mock<ITemplateParsingService>();
        _service = new MessageFormattingService(telegramEscaper, jsonConverter, mockLogger.Object, _mockTemplateParsingService.Object);
    }

    [Fact]
    public void FormatMessage_WithSimpleTemplate_ReplacesVariables()
    {
        // Arrange
        var template = "Hello {{name}}, you are {{age}} years old!";
        var payload = JsonDocument.Parse("""{"name": "John", "age": 25}""").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "None"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello John, you are 25 years old!", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithNestedObject_ReplacesNestedVariables()
    {
        // Arrange
        var template = "User: {{user.name}}, Email: {{user.email}}, City: {{user.address.city}}";
        var payload = JsonDocument.Parse("""
        {
            "user": {
                "name": "Alice",
                "email": "alice@example.com",
                "address": {
                    "city": "New York"
                }
            }
        }
        """).RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "None"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("User: Alice, Email: alice@example.com, City: New York", result.MessageText);

    }

    [Fact]
    public void FormatMessage_WithArray_ReplacesArrayElements()
    {
        // Arrange
        var template = "Items: {{items[0]}}, {{items[1]}}, {{items[2]}}";
        var payload = JsonDocument.Parse("""{"items": ["apple", "banana", "cherry"]}""").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "None"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Items: apple, banana, cherry", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithMarkdownV2ParseMode_EscapesSpecialCharacters()
    {
        // Arrange
        var template = "Message: {{message}}";
        var payload = JsonDocument.Parse("""{"message": "Hello_world! This is a *test* with [links](http://example.com)"}""").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "MarkdownV2"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        // Should escape underscores, asterisks, square brackets, parentheses
        Assert.True(result.IsSuccess);
        Assert.Contains("Hello\\_world\\!", result.MessageText);
        Assert.Contains("*test*", result.MessageText);
        Assert.Contains("[links]", result.MessageText);
        Assert.Contains("(http://example.com)", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithMarkdownParseMode_EscapesSpecialCharacters()
    {
        // Arrange
        var template = "Message: {{message}}";
        var payload = JsonDocument.Parse("""{"message": "Hello_world! This is a *test* with [links](http://example.com)"}""").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "Markdown"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        // Should escape underscores and asterisks but not exclamation marks
        Assert.True(result.IsSuccess);
        Assert.Contains("Hello\\_world!", result.MessageText);
        Assert.Contains("*test*", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithHtmlParseMode_EscapesHtmlCharacters()
    {
        // Arrange
        var template = "Message: {{message}}";
        var payload = JsonDocument.Parse("""{"message": "<script>alert('test')</script> & \"quotes\""}""").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "HTML"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("&lt;script&gt;", result.MessageText);
        Assert.Contains("&lt;/script&gt;", result.MessageText);
        Assert.Contains("&amp;", result.MessageText);
        // Note: quotes are not escaped by EscapeHtml method
        Assert.Contains("\"quotes\"", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithNoneParseMode_DoesNotEscapeCharacters()
    {
        // Arrange
        var template = "Message: {{message}}";
        var payload = JsonDocument.Parse("""{"message": "Hello_world! *test* <tag>"}""").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "None"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Message: Hello_world! *test* <tag>", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithMissingVariable_RendersAsEmpty()
    {
        // Arrange
        var template = "Hello {{name}}, you have {{points}} points and {{missing}} items";
        var payload = JsonDocument.Parse("""{"name": "John", "points": 100}""").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "None"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello John, you have 100 points and  items", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithEmptyTemplate_ReturnsEmptyString()
    {
        // Arrange
        var template = "";
        var payload = JsonDocument.Parse("""{"name": "John"}""").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Webhook Name",
            MessageTemplate = template,
            ParseMode = "None"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("No data available to display, please check the provided template and payload for webhook 'Webhook Name'.", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithBadTemplate_ThowsBadRequestException()
    {
        // Arrange
        var template = "{{ name < }}";
        var payload = JsonDocument.Parse("""{"name": "John"}""").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "None"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to format message template: ", result.Error);

    }

    [Fact]
    public void FormatMessage_WithEmptyPayload_RendersVariablesAsEmpty()
    {
        // Arrange
        var template = "Hello {{name}}, you are {{age}} years old!";
        var payload = JsonDocument.Parse("{}").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "None"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello , you are  years old!", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithComplexNestedStructure_ReplacesCorrectly()
    {
        // Arrange
        var template = "Repo: {{repository.name}}, Author: {{repository.owner.login}}, Branch: {{ref}}";
        var payload = JsonDocument.Parse("""
        {
            "ref": "refs/heads/main",
            "repository": {
                "name": "my-repo",
                "owner": {
                    "login": "john-doe"
                }
            }
        }
        """).RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "None"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Repo: my-repo, Author: john-doe, Branch: refs/heads/main", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithSpecialCharactersInMarkdownV2_EscapesCorrectly()
    {
        // Arrange
        var template = "Special chars: {{text}}";
        var payload = JsonDocument.Parse("""{"text": "Test_with*special+chars-and=more!"}""").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "MarkdownV2"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("Test\\_with\\*special\\+chars\\-and\\=more\\!", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithArrayOfObjects_ReplacesObjectProperties()
    {
        // Arrange
        var template = "First user: {{users[0].name}}, Second user: {{users[1].name}}";
        var payload = JsonDocument.Parse("""
        {
            "users": [
                {"name": "Alice", "id": 1},
                {"name": "Bob", "id": 2}
            ]
        }
        """).RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "None"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("First user: Alice, Second user: Bob", result.MessageText);
    }

    [Fact]
    public void FormatMessage_WithFloatingPointNumber_FormatsCorrectly()
    {
        // Arrange
        var template = "Price: ${{price}}";
        var payload = JsonDocument.Parse("""{"price": 19.99}""").RootElement;
        var webhook = new Webhook
        {
            Id = 1,
            Name = "Test Webhook",
            MessageTemplate = template,
            ParseMode = "None"
        };

        var parsedTemplate = Template.Parse(template);
        _mockTemplateParsingService.Setup(x => x.GetTemplate(webhook.Id)).Returns(parsedTemplate);

        // Act
        var result = _service.FormatMessage(webhook, payload);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Price: $19.99", result.MessageText);
    }
}
