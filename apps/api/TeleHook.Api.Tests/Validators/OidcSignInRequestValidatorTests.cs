using FluentValidation.TestHelper;
using TeleHook.Api.DTO;
using TeleHook.Api.Validators;
using Xunit;

namespace TeleHook.Api.Tests.Validators;

public class OidcSignInRequestValidatorTests
{
    private readonly OidcSignInRequestValidator _validator;

    public OidcSignInRequestValidatorTests()
    {
        _validator = new OidcSignInRequestValidator();
    }

    #region Email Validation Tests

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var request = new OidcSignInDto { Email = "", OidcId = "test-oidc-id" };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required for OIDC signin");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Null()
    {
        // Arrange
        var request = new OidcSignInDto { Email = null!, OidcId = "test-oidc-id" };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required for OIDC signin");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Should_Have_Error_When_Email_Is_Invalid(string invalidEmail)
    {
        // Arrange
        var request = new OidcSignInDto { Email = invalidEmail, OidcId = "test-oidc-id" };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be a valid email address");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email@domain.co.uk")]
    [InlineData("user+tag@example.org")]
    [InlineData("firstname.lastname@company.com")]
    public void Should_Not_Have_Error_When_Email_Is_Valid(string validEmail)
    {
        // Arrange
        var request = new OidcSignInDto { Email = validEmail, OidcId = "test-oidc-id" };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region OidcId Validation Tests

    [Fact]
    public void Should_Have_Error_When_OidcId_Is_Empty()
    {
        // Arrange
        var request = new OidcSignInDto { Email = "user@example.com", OidcId = "" };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.OidcId)
            .WithErrorMessage("OIDC ID is required for OIDC signin");
    }

    [Fact]
    public void Should_Have_Error_When_OidcId_Is_Null()
    {
        // Arrange
        var request = new OidcSignInDto { Email = "user@example.com", OidcId = null! };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.OidcId)
            .WithErrorMessage("OIDC ID is required for OIDC signin");
    }

    [Theory]
    [InlineData("google_12345")]
    [InlineData("auth0|user123")]
    [InlineData("azure_ad_b2c_user_456")]
    [InlineData("simple-oidc-id")]
    public void Should_Not_Have_Error_When_OidcId_Is_Valid(string validOidcId)
    {
        // Arrange
        var request = new OidcSignInDto { Email = "user@example.com", OidcId = validOidcId };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.OidcId);
    }

    #endregion

    #region Username Validation Tests (Optional)

    [Fact]
    public void Should_Not_Have_Error_When_Username_Is_Null()
    {
        // Arrange
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            Username = null 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Username_Is_Empty()
    {
        // Arrange
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            Username = "" 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Have_Error_When_Username_Is_Too_Short()
    {
        // Arrange
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            Username = "a" 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username must be at least 2 characters long");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("user")]
    [InlineData("john_doe")]
    [InlineData("test-user-123")]
    public void Should_Not_Have_Error_When_Username_Is_Valid(string validUsername)
    {
        // Arrange
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            Username = validUsername 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    #endregion

    #region FirstName Validation Tests (Optional)

    [Fact]
    public void Should_Not_Have_Error_When_FirstName_Is_Null()
    {
        // Arrange
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            FirstName = null 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_FirstName_Is_Empty()
    {
        // Arrange
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            FirstName = "" 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Exceeds_MaxLength()
    {
        // Arrange
        var longFirstName = new string('a', 51); // 51 characters
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            FirstName = longFirstName 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("John")]
    [InlineData("Jane")]
    [InlineData("A")]
    [InlineData("María José")]
    public void Should_Not_Have_Error_When_FirstName_Is_Valid(string validFirstName)
    {
        // Arrange
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            FirstName = validFirstName 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    #endregion

    #region LastName Validation Tests (Optional)

    [Fact]
    public void Should_Not_Have_Error_When_LastName_Is_Null()
    {
        // Arrange
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            LastName = null 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_LastName_Is_Empty()
    {
        // Arrange
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            LastName = "" 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_Have_Error_When_LastName_Exceeds_MaxLength()
    {
        // Arrange
        var longLastName = new string('b', 51); // 51 characters
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            LastName = longLastName 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("Doe")]
    [InlineData("Smith")]
    [InlineData("O'Connor")]
    [InlineData("Van Der Berg")]
    public void Should_Not_Have_Error_When_LastName_Is_Valid(string validLastName)
    {
        // Arrange
        var request = new OidcSignInDto 
        { 
            Email = "user@example.com", 
            OidcId = "test-oidc-id",
            LastName = validLastName 
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    #endregion

    #region Complete Valid Request Tests

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid_With_All_Fields()
    {
        // Arrange
        var request = new OidcSignInDto
        {
            Email = "john.doe@example.com",
            OidcId = "google_12345",
            Username = "johndoe",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid_With_Required_Fields_Only()
    {
        // Arrange
        var request = new OidcSignInDto
        {
            Email = "user@example.com",
            OidcId = "auth0_user123"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}