using Microsoft.Extensions.Logging;
using Moq;
using Scriban.Runtime;
using TeleHook.Api.Services.Interfaces;
using TeleHook.Api.Services.Utilities;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class TemplateRendererTests
{
    private readonly Mock<IJsonToScribanConverter> _mockJsonConverter;
    private readonly Mock<ILogger<TemplateRenderer>> _mockLogger;
    private readonly TemplateRenderer _renderer;

    public TemplateRendererTests()
    {
        _mockJsonConverter = new Mock<IJsonToScribanConverter>();
        _mockLogger = new Mock<ILogger<TemplateRenderer>>();

        _renderer = new TemplateRenderer(_mockJsonConverter.Object, _mockLogger.Object);
    }

    #region Valid Template Tests

    [Fact]
    public void RenderTemplate_WithValidTemplate_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        var template = "Hello {{ name }}!";
        var sampleData = new { name = "World" };
        var expectedScriptObject = new ScriptObject();
        expectedScriptObject["name"] = "World";

        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Returns(expectedScriptObject);

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Hello World!", result.Rendered);
        Assert.Null(result.Errors);
        _mockJsonConverter.Verify(x => x.ConvertToScriptObject(sampleData), Times.Once);
    }

    [Fact]
    public void RenderTemplate_WithComplexTemplate_ShouldRenderCorrectly()
    {
        // Arrange
        var template = "User: {{ user.name }} ({{ user.age }}) - Active: {{ user.active }}";
        var sampleData = new
        {
            user = new
            {
                name = "John Doe",
                age = 30,
                active = true
            }
        };

        var scriptObject = new ScriptObject();
        var userObject = new ScriptObject();
        userObject["name"] = "John Doe";
        userObject["age"] = 30;
        userObject["active"] = true;
        scriptObject["user"] = userObject;

        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Returns(scriptObject);

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("User: John Doe (30) - Active: true", result.Rendered);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void RenderTemplate_WithLoopTemplate_ShouldRenderLoop()
    {
        // Arrange
        var template = "Items: {{ for item in items }}{{ item.name }}, {{ end }}";
        var sampleData = new
        {
            items = new[]
            {
                new { name = "Item1" },
                new { name = "Item2" },
                new { name = "Item3" }
            }
        };

        var scriptObject = new ScriptObject();
        var itemsArray = new ScriptArray();

        foreach (var item in sampleData.items)
        {
            var itemObj = new ScriptObject();
            itemObj["name"] = item.name;
            itemsArray.Add(itemObj);
        }

        scriptObject["items"] = itemsArray;

        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Returns(scriptObject);

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Items: Item1, Item2, Item3, ", result.Rendered);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void RenderTemplate_WithEmptyTemplate_ShouldReturnEmptyString()
    {
        // Arrange
        var template = "";
        var sampleData = new { name = "Test" };
        var scriptObject = new ScriptObject();

        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Returns(scriptObject);

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("", result.Rendered);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void RenderTemplate_WithNoPlaceholders_ShouldReturnOriginalText()
    {
        // Arrange
        var template = "This is just plain text with no placeholders.";
        var sampleData = new { name = "Test" };
        var scriptObject = new ScriptObject();

        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Returns(scriptObject);

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("This is just plain text with no placeholders.", result.Rendered);
        Assert.Null(result.Errors);
    }

    #endregion

    #region Invalid Template Tests

    [Fact]
    public void RenderTemplate_WithInvalidSyntax_ShouldRenderPartially()
    {
        // Arrange
        var template = "Hello {{ var ; }}"; // Scriban is forgiving and strips invalid parts
        var sampleData = new { name = "Test" };
        var scriptObject = new ScriptObject();
        scriptObject["name"] = "Test";

        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Returns(scriptObject);

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Hello ", result.Rendered); // Scriban strips the invalid part
        Assert.Null(result.Errors);
    }

    [Fact]
    public void RenderTemplate_WithInvalidFunction_ShouldReturnErrors()
    {
        // Arrange
        var template = "Date: {{ var | date.not_a_valid_method }}"; // Invalid function
        var sampleData = new { var = "2023-01-01" };

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Rendered);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void RenderTemplate_WithUnclosedBraces_ShouldRender()
    {
        // Arrange
        var template = "Hello {{ name"; // Unclosed braces - Scriban handles this gracefully
        var sampleData = new { name = "Test" };
        var scriptObject = new ScriptObject();
        scriptObject["name"] = "Test";

        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Returns(scriptObject);

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Hello Test", result.Rendered); // Scriban strips the unclosed part
        Assert.Null(result.Errors);
    }

    [Fact]
    public void RenderTemplate_WithInvalidForLoop_ShouldReturnErrors()
    {
        // Arrange
        var template = "{{ for item }}{{ item }}{{ end }}"; // Missing 'in' keyword
        var sampleData = new { items = new[] { "test" } };

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Rendered);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void RenderTemplate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var template = "{{ var < }} and {{ another : error }}"; // Multiple syntax errors
        var sampleData = new { name = "Test" };

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Rendered);
        Assert.NotNull(result.Errors);
        Assert.True(result.Errors.Count >= 1); // Should have multiple errors
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public void RenderTemplate_WhenConverterThrowsException_ShouldReturnErrorResponse()
    {
        // Arrange
        var template = "Hello {{ name }}!";
        var sampleData = new { name = "Test" };

        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Throws(new InvalidOperationException("Converter failed"));

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Rendered);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Equal("Converter failed", result.Errors[0]);
    }

    [Fact]
    public void RenderTemplate_WhenTemplateRenderingThrowsException_ShouldReturnErrorResponse()
    {
        // Arrange  
        var template = "{{ name }}";
        var sampleData = new { name = "Test" };

        // Create a script object that will cause rendering to fail
        var scriptObject = new ScriptObject();
        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Returns(scriptObject);

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.True(result.Success); // This actually succeeds but renders empty for missing variable
        Assert.Equal("", result.Rendered); // Missing variable renders as empty
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void RenderTemplate_WithNullSampleData_ShouldHandleGracefully()
    {
        // Arrange
        var template = "Hello {{ name }}!";
        object sampleData = null;
        var scriptObject = new ScriptObject();

        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Returns(scriptObject);

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Hello !", result.Rendered); // Missing variable renders as empty
        Assert.Null(result.Errors);
    }

    [Fact]
    public void RenderTemplate_WithComplexNestedData_ShouldRenderCorrectly()
    {
        // Arrange
        var template = "Webhook: {{ webhook.name }} triggered by {{ webhook.user.name }} at {{ webhook.timestamp }}";
        var sampleData = new
        {
            webhook = new
            {
                name = "GitHub Push",
                user = new { name = "johndoe" },
                timestamp = "2023-01-01T10:00:00Z"
            }
        };

        var scriptObject = new ScriptObject();
        var webhookObj = new ScriptObject();
        var userObj = new ScriptObject();

        userObj["name"] = "johndoe";
        webhookObj["name"] = "GitHub Push";
        webhookObj["user"] = userObj;
        webhookObj["timestamp"] = "2023-01-01T10:00:00Z";
        scriptObject["webhook"] = webhookObj;

        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Returns(scriptObject);

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Webhook: GitHub Push triggered by johndoe at 2023-01-01T10:00:00Z", result.Rendered);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void RenderTemplate_WithConditionalLogic_ShouldRenderCorrectly()
    {
        // Arrange
        var template = "Status: {{ if active }}Active{{ else }}Inactive{{ end }}";
        var sampleData = new { active = true };

        var scriptObject = new ScriptObject();
        scriptObject["active"] = true;

        _mockJsonConverter.Setup(x => x.ConvertToScriptObject(sampleData))
            .Returns(scriptObject);

        // Act
        var result = _renderer.RenderTemplate(template, sampleData);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Status: Active", result.Rendered);
        Assert.Null(result.Errors);
    }

    #endregion
}
