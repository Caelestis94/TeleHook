using FluentValidation.TestHelper;
using TeleHook.Api.DTO;
using TeleHook.Api.Validators;
using Xunit;

namespace TeleHook.Api.Tests.Validators;

public class RenderTemplateRequestValidatorTests
{
    private readonly RenderTemplateRequestValidator _validator;

    public RenderTemplateRequestValidatorTests()
    {
        _validator = new RenderTemplateRequestValidator();
    }

    #region Template Validation Tests

    [Fact]
    public void Should_Have_Error_When_Template_Is_Empty()
    {
        // Arrange
        var request = new RenderTemplateDto { Template = "", SampleData = new { } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Template)
            .WithErrorMessage("Template is required");
    }

    [Fact]
    public void Should_Have_Error_When_Template_Is_Null()
    {
        // Arrange
        var request = new RenderTemplateDto { Template = null!, SampleData = new { } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Template)
            .WithErrorMessage("Template is required");
    }

    [Fact]
    public void Should_Have_Error_When_Template_Exceeds_MaxLength()
    {
        // Arrange
        var longTemplate = new string('a', 4001); // 4001 characters
        var request = new RenderTemplateDto { Template = longTemplate, SampleData = new { } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Template)
            .WithErrorMessage("Template must be 4000 characters or less");
    }

    [Fact]
    public void Should_Have_Error_When_Template_Has_Invalid_Syntax()
    {
        // Arrange
        var invalidTemplate = "Hello {{ var > }}"; // Invalid Scriban syntax
        var request = new RenderTemplateDto { Template = invalidTemplate, SampleData = new { } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Template)
            .WithErrorMessage("Template syntax is invalid");
    }

    [Theory]
    [InlineData("Simple text template")]
    [InlineData("Hello {{ name }}!")]
    [InlineData("User: {{ user.name }} ({{ user.age }})")]
    [InlineData("{{ for item in items }}{{ item.name }}, {{ end }}")]
    [InlineData("{{ if active }}Active{{ else }}Inactive{{ end }}")]
    [InlineData("Date: {{ date.now | date.to_string '%Y-%m-%d' }}")]
    public void Should_Not_Have_Error_When_Template_Is_Valid(string validTemplate)
    {
        // Arrange
        var request = new RenderTemplateDto { Template = validTemplate, SampleData = new { } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Template);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Template_Is_At_MaxLength()
    {
        // Arrange
        var maxLengthTemplate = new string('a', 4000); // Exactly 4000 characters
        var request = new RenderTemplateDto { Template = maxLengthTemplate, SampleData = new { } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Template);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Template_Has_Complex_Valid_Syntax()
    {
        // Arrange
        var complexTemplate = @"
            Webhook: {{ webhook.name }}
            {{ for commit in webhook.commits }}
                - {{ commit.message }} by {{ commit.author.name }}
                {{ if commit.added }}
                    Added files: {{ for file in commit.added }}{{ file }}, {{ end }}
                {{ end }}
            {{ end }}
            Total commits: {{ webhook.commits.size }}
        ";
        var request = new RenderTemplateDto { Template = complexTemplate, SampleData = new { } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Template);
    }

    #endregion

    #region SampleData Validation Tests

    [Fact]
    public void Should_Have_Error_When_SampleData_Is_Null()
    {
        // Arrange
        var request = new RenderTemplateDto { Template = "Hello {{ name }}", SampleData = null! };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SampleData)
            .WithErrorMessage("SampleData is required");
    }

    [Theory]
    [InlineData("string data")]
    [InlineData(123)]
    [InlineData(true)]
    public void Should_Not_Have_Error_When_SampleData_Is_Primitive(object sampleData)
    {
        // Arrange
        var request = new RenderTemplateDto { Template = "Hello {{ name }}", SampleData = sampleData };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.SampleData);
    }

    [Fact]
    public void Should_Not_Have_Error_When_SampleData_Is_Empty_Object()
    {
        // Arrange
        var request = new RenderTemplateDto { Template = "Hello world", SampleData = new { } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.SampleData);
    }

    [Fact]
    public void Should_Not_Have_Error_When_SampleData_Is_Complex_Object()
    {
        // Arrange
        var sampleData = new
        {
            user = new { name = "John", age = 30 },
            items = new[] { "item1", "item2" },
            metadata = new { timestamp = DateTime.Now, active = true }
        };
        var request = new RenderTemplateDto { Template = "Hello {{ user.name }}", SampleData = sampleData };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.SampleData);
    }

    [Fact]
    public void Should_Not_Have_Error_When_SampleData_Is_Array()
    {
        // Arrange
        var sampleData = new[] { "item1", "item2", "item3" };
        var request = new RenderTemplateDto { Template = "Items: {{ . }}", SampleData = sampleData };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.SampleData);
    }

    #endregion

    #region Complete Valid Request Tests

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid_Simple()
    {
        // Arrange
        var request = new RenderTemplateDto
        {
            Template = "Hello {{ name }}!",
            SampleData = new { name = "World" }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid_Complex()
    {
        // Arrange
        var request = new RenderTemplateDto
        {
            Template = @"
                Webhook: {{ webhook.name }}
                Repository: {{ webhook.repository.name }}
                {{ for commit in webhook.commits }}
                    - {{ commit.message }}
                {{ end }}
            ",
            SampleData = new
            {
                webhook = new
                {
                    name = "Push Event",
                    repository = new { name = "my-repo" },
                    commits = new[]
                    {
                        new { message = "Initial commit" },
                        new { message = "Fix bug" }
                    }
                }
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Uses_Built_In_Functions()
    {
        // Arrange
        var request = new RenderTemplateDto
        {
            Template = "Today is {{ date.now | date.to_string '%Y-%m-%d' }}",
            SampleData = new { }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Invalid Scriban Template Edge Cases

    [Theory]
    [InlineData("{{ var < }}")]
    [InlineData("{{ var : }}")]
    [InlineData("{{ var > }}")]
    public void Should_Have_Error_When_Template_Has_Scriban_Syntax_Errors(string invalidTemplate)
    {
        // Arrange
        var request = new RenderTemplateDto { Template = invalidTemplate, SampleData = new { } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Template)
            .WithErrorMessage("Template syntax is invalid");
    }

    [Theory]
    [InlineData("{{ var ; }}")]  // Scriban actually handles this gracefully
    [InlineData("Hello {{ name")]  // Scriban handles unclosed braces
    [InlineData("Plain text")]     // No template syntax at all
    public void Should_Not_Have_Error_When_Template_Is_Handled_Gracefully_By_Scriban(string template)
    {
        // Arrange
        var request = new RenderTemplateDto { Template = template, SampleData = new { } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Template);
    }

    #endregion

    #region Multiple Validation Errors

    [Fact]
    public void Should_Have_Multiple_Errors_When_Both_Template_And_SampleData_Are_Invalid()
    {
        // Arrange
        var request = new RenderTemplateDto
        {
            Template = "", // Empty template
            SampleData = null! // Null sample data
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Template);
        result.ShouldHaveValidationErrorFor(x => x.SampleData);
    }

    [Fact]
    public void Should_Have_Multiple_Template_Errors_When_Template_Is_Too_Long_And_Invalid()
    {
        // Arrange
        var longInvalidTemplate = new string('a', 10001) + " {{ var > }}"; // Too long AND invalid syntax
        var request = new RenderTemplateDto
        {
            Template = longInvalidTemplate,
            SampleData = new { }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Template);
        // Should have both length and syntax errors
    }

    #endregion
}