using FluentValidation.TestHelper;
using TeleHook.Api.DTO;
using TeleHook.Api.Validators;
using Xunit;

namespace TeleHook.Api.Tests.Validators;

public class UserRequestValidatorTests
{
    #region CreateUserRequestValidator Tests

    public class CreateUserRequestValidatorTests
    {
        private readonly CreateUserRequestValidator _validator;

        public CreateUserRequestValidatorTests()
        {
            _validator = new CreateUserRequestValidator();
        }

        #region Email Validation Tests

        [Fact]
        public void Should_Have_Error_When_Email_Is_Empty()
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = "",
                Username = "testuser",
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage("Email is required");
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Null()
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = null!,
                Username = "testuser",
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage("Email is required");
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@example.com")]
        [InlineData("user@")]
        public void Should_Have_Error_When_Email_Is_Invalid(string invalidEmail)
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = invalidEmail,
                Username = "testuser",
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage("Invalid email format");
        }

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("test.email@domain.co.uk")]
        [InlineData("user+tag@example.org")]
        [InlineData("firstname.lastname@company.com")]
        public void Should_Not_Have_Error_When_Email_Is_Valid(string validEmail)
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = validEmail,
                Username = "testuser",
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        #endregion

        #region Username Validation Tests

        [Fact]
        public void Should_Have_Error_When_Username_Is_Empty()
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "",
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Username)
                .WithErrorMessage("Username is required");
        }

        [Fact]
        public void Should_Have_Error_When_Username_Is_Null()
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = null!,
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Username)
                .WithErrorMessage("Username is required");
        }

        [Fact]
        public void Should_Have_Error_When_Username_Exceeds_MaxLength()
        {
            // Arrange
            var longUsername = new string('a', 51); // 51 characters
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = longUsername,
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Username)
                .WithErrorMessage("Username must be 50 characters or less");
        }

        [Theory]
        [InlineData("user")]
        [InlineData("testuser123")]
        [InlineData("user_name")]
        [InlineData("user-name")]
        [InlineData("a")] // Single character
        public void Should_Not_Have_Error_When_Username_Is_Valid(string validUsername)
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = validUsername,
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Username);
        }

        #endregion

        #region Password Validation Tests

        [Fact]
        public void Should_Have_Error_When_Password_Is_Empty()
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = ""
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password is required");
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Null()
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = null!
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password is required");
        }

        [Theory]
        [InlineData("1234567")] // 7 characters
        [InlineData("short")]   // 5 characters
        [InlineData("abc")]     // 3 characters
        public void Should_Have_Error_When_Password_Is_Too_Short(string shortPassword)
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = shortPassword
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password must be at least 8 characters long");
        }

        [Theory]
        [InlineData("password123")]
        [InlineData("12345678")]
        [InlineData("VeryLongPasswordWithSpecialCharacters!@#")]
        [InlineData("SimplePass")]
        public void Should_Not_Have_Error_When_Password_Is_Valid(string validPassword)
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = validPassword
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        #endregion

        #region FirstName Validation Tests (Optional)

        [Fact]
        public void Should_Not_Have_Error_When_FirstName_Is_Null()
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = "password123",
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
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = "password123",
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
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = "password123",
                FirstName = longFirstName
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FirstName)
                .WithErrorMessage("First name must be 50 characters or less");
        }

        [Theory]
        [InlineData("John")]
        [InlineData("Jane")]
        [InlineData("María José")]
        [InlineData("O'Connor")]
        public void Should_Not_Have_Error_When_FirstName_Is_Valid(string validFirstName)
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = "password123",
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
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = "password123",
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
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = "password123",
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
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = "password123",
                LastName = longLastName
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.LastName)
                .WithErrorMessage("Last name must be 50 characters or less");
        }

        [Theory]
        [InlineData("Doe")]
        [InlineData("Smith")]
        [InlineData("Van Der Berg")]
        [InlineData("O'Connor")]
        public void Should_Not_Have_Error_When_LastName_Is_Valid(string validLastName)
        {
            // Arrange
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = "password123",
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
            var request = new CreateUserDto
            {
                Email = "john.doe@example.com",
                Username = "johndoe",
                Password = "securepassword123",
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
            var request = new CreateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion
    }

    #endregion

    #region UpdateUserRequestValidator Tests

    public class UpdateUserRequestValidatorTests
    {
        private readonly UpdateUserRequestValidator _validator;

        public UpdateUserRequestValidatorTests()
        {
            _validator = new UpdateUserRequestValidator();
        }

        #region Email Validation Tests

        [Fact]
        public void Should_Have_Error_When_Email_Is_Empty()
        {
            // Arrange
            var request = new UpdateUserDto
            {
                Email = "",
                Username = "testuser",
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage("Email is required");
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Null()
        {
            // Arrange
            var request = new UpdateUserDto
            {
                Email = null!,
                Username = "testuser",
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage("Email is required");
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@example.com")]
        [InlineData("user@")]
        public void Should_Have_Error_When_Email_Is_Invalid(string invalidEmail)
        {
            // Arrange
            var request = new UpdateUserDto
            {
                Email = invalidEmail,
                Username = "testuser",
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage("Invalid email format");
        }

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("updated.email@domain.com")]
        public void Should_Not_Have_Error_When_Email_Is_Valid(string validEmail)
        {
            // Arrange
            var request = new UpdateUserDto
            {
                Email = validEmail,
                Username = "testuser",
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        #endregion

        #region Username Validation Tests

        [Fact]
        public void Should_Have_Error_When_Username_Is_Empty()
        {
            // Arrange
            var request = new UpdateUserDto
            {
                Email = "user@example.com",
                Username = "",
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Username)
                .WithErrorMessage("Username is required");
        }

        [Fact]
        public void Should_Have_Error_When_Username_Exceeds_MaxLength()
        {
            // Arrange
            var longUsername = new string('a', 51); // 51 characters
            var request = new UpdateUserDto
            {
                Email = "user@example.com",
                Username = longUsername,
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Username)
                .WithErrorMessage("Username must be 50 characters or less");
        }

        [Theory]
        [InlineData("updateduser")]
        [InlineData("newusername123")]
        public void Should_Not_Have_Error_When_Username_Is_Valid(string validUsername)
        {
            // Arrange
            var request = new UpdateUserDto
            {
                Email = "user@example.com",
                Username = validUsername,
                Password = "password123"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Username);
        }

        #endregion

        #region Password Validation Tests


        [Theory]
        [InlineData("1234567")] // 7 characters
        [InlineData("short")]   // 5 characters
        public void Should_Have_Error_When_Password_Is_Too_Short(string shortPassword)
        {
            // Arrange
            var request = new UpdateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = shortPassword
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password must be at least 8 characters long");
        }

        [Theory]
        [InlineData("newpassword123")]
        [InlineData("updatedPassword!")]
        public void Should_Not_Have_Error_When_Password_Is_Valid(string validPassword)
        {
            // Arrange
            var request = new UpdateUserDto
            {
                Email = "user@example.com",
                Username = "testuser",
                Password = validPassword
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        #endregion

        #region Complete Valid Request Tests

        [Fact]
        public void Should_Not_Have_Errors_When_Request_Is_Valid()
        {
            // Arrange
            var request = new UpdateUserDto
            {
                Email = "updated@example.com",
                Username = "updateduser",
                Password = "newpassword123",
                FirstName = "Updated",
                LastName = "User"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion
    }

    #endregion
}
