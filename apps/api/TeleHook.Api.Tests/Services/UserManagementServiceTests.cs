using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Domain;
using TeleHook.Api.Services.Interfaces;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class UserManagementServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<ILogger<UserManagementService>> _mockLogger;
    private readonly UserManagementService _service;

    public UserManagementServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockValidationService = new Mock<IValidationService>();
        _mockLogger = new Mock<ILogger<UserManagementService>>();

        // Setup unit of work to return mocked repository
        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);

        // Setup validation service to return success by default
        _mockValidationService.Setup(x => x.ValidateAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(new ValidationResult());
        _mockValidationService.Setup(x => x.ValidateAsync(It.IsAny<UpdateUserDto>()))
            .ReturnsAsync(new ValidationResult());
        _mockValidationService.Setup(x => x.ValidateAsync(It.IsAny<EmailPasswordSignInDto>()))
            .ReturnsAsync(new ValidationResult());
        _mockValidationService.Setup(x => x.ValidateAsync(It.IsAny<OidcSignInDto>()))
            .ReturnsAsync(new ValidationResult());

        _service = new UserManagementService(_mockLogger.Object, _mockUnitOfWork.Object, _mockValidationService.Object);
    }

    #region SetupIsRequiredAsync Tests

    [Fact]
    public async Task SetupIsRequiredAsync_WhenNoUsersExist_ReturnsTrue()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.UserExistsAsync()).ReturnsAsync(false);

        // Act
        var result = await _service.SetupIsRequiredAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SetupIsRequiredAsync_WhenUsersExist_ReturnsFalse()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.UserExistsAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.SetupIsRequiredAsync();

        // Assert
        Assert.False(result);
    }

    #endregion

    #region CreateAdminUserAsync Tests

    [Fact]
    public async Task CreateAdminUserAsync_WhenNoUsersExist_CreatesAdminUser()
    {
        // Arrange
        var request = new CreateUserDto
        {
            Email = "admin@test.com",
            Username = "admin",
            Password = "password123",
            FirstName = "Admin",
            LastName = "User"
        };

        _mockUserRepository.Setup(x => x.UserExistsAsync()).ReturnsAsync(false);

        // Act
        var result = await _service.CreateAdminUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Username, result.Username);
        Assert.Equal("admin", result.Role);
        Assert.Empty(result.PasswordHash); // Should be cleared before return
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAdminUserAsync_WhenUserAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        var request = new CreateUserDto
        {
            Email = "admin@test.com",
            Username = "admin",
            Password = "password123"
        };

        _mockUserRepository.Setup(x => x.UserExistsAsync()).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAdminUserAsync(request));
    }

    [Fact]
    public async Task CreateAdminUserAsync_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateUserDto
        {
            Email = "invalid-email",
            Username = "",
            Password = "123"
        };

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Email", "Invalid email format"));

        _mockUserRepository.Setup(x => x.UserExistsAsync()).ReturnsAsync(false);
        _mockValidationService.Setup(x => x.ValidateAsync(request))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAdminUserAsync(request));
    }

    #endregion

    #region SignInAsync Tests

    [Fact]
    public async Task SignInAsync_WithValidCredentials_ReturnsUser()
    {
        // Arrange
        var request = new EmailPasswordSignInDto
        {
            Email = "user@test.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Email = "user@test.com",
            Username = "testuser",
            PasswordHash = "hashedpassword"
        };

        _mockUserRepository.Setup(x => x.ValidatePasswordAsync(request.Email, request.Password))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _service.SignInAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Empty(result.PasswordHash); // Should be cleared
    }

    [Fact]
    public async Task SignInAsync_WithEmptyEmail_ThrowsBadRequestException()
    {
        // Arrange
        var request = new EmailPasswordSignInDto
        {
            Email = "",
            Password = "password123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.SignInAsync(request));
    }

    [Fact]
    public async Task SignInAsync_WithEmptyPassword_ThrowsBadRequestException()
    {
        // Arrange
        var request = new EmailPasswordSignInDto
        {
            Email = "user@test.com",
            Password = ""
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.SignInAsync(request));
    }

    [Fact]
    public async Task SignInAsync_WithInvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new EmailPasswordSignInDto
        {
            Email = "user@test.com",
            Password = "wrongpassword"
        };

        _mockUserRepository.Setup(x => x.ValidatePasswordAsync(request.Email, request.Password))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.SignInAsync(request));
    }

    [Fact]
    public async Task SignInAsync_WithNonExistentUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new EmailPasswordSignInDto
        {
            Email = "nonexistent@test.com",
            Password = "password123"
        };

        _mockUserRepository.Setup(x => x.ValidatePasswordAsync(request.Email, request.Password))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.SignInAsync(request));
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Email = "user@test.com",
            Username = "testuser",
            PasswordHash = "hashedpassword"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Empty(result.PasswordHash); // Should be cleared
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ThrowsBadRequestException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.GetUserByIdAsync(0));
        await Assert.ThrowsAsync<BadRequestException>(() => _service.GetUserByIdAsync(-1));
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentUser_ThrowsNotFoundException()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetUserByIdAsync(userId));
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_WithValidData_UpdatesUser()
    {
        // Arrange
        var userId = 1;
        var updateRequest = new UpdateUserDto
        {
            Email = "updated@test.com",
            FirstName = "Updated",
            LastName = "User",
            Username = "updateduser",
            Password = "newpassword123"
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "old@test.com",
            Username = "olduser",
            FirstName = "Old",
            LastName = "User",
            PasswordHash = "oldhashedpassword"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _service.UpdateUserAsync(updateRequest, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateRequest.Email, result.Email);
        Assert.Equal(updateRequest.Username, result.Username);
        Assert.Equal(updateRequest.FirstName, result.FirstName);
        Assert.Equal(updateRequest.LastName, result.LastName);
        Assert.Empty(result.PasswordHash); // Should be cleared
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithPartialData_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var userId = 1;
        var updateRequest = new UpdateUserDto
        {
            FirstName = "Updated",
            // Other fields are null
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "user@test.com",
            Username = "user",
            FirstName = "Original",
            LastName = "User",
            PasswordHash = "hashedpassword"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _service.UpdateUserAsync(updateRequest, userId);

        // Assert
        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("user@test.com", result.Email); // Should remain unchanged
        Assert.Equal("user", result.Username); // Should remain unchanged
        Assert.Equal("User", result.LastName); // Should remain unchanged
    }

    [Fact]
    public async Task UpdateUserAsync_WithInvalidId_ThrowsBadRequestException()
    {
        // Arrange
        var updateRequest = new UpdateUserDto { FirstName = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.UpdateUserAsync(updateRequest, 0));
        await Assert.ThrowsAsync<BadRequestException>(() => _service.UpdateUserAsync(updateRequest, -1));
    }

    [Fact]
    public async Task UpdateUserAsync_WithNonExistentUser_ThrowsNotFoundException()
    {
        // Arrange
        var userId = 999;
        var updateRequest = new UpdateUserDto { FirstName = "Test" };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateUserAsync(updateRequest, userId));
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidationError_ThrowsValidationException()
    {
        // Arrange
        var userId = 1;
        var updateRequest = new UpdateUserDto { Email = "invalid-email" };
        var existingUser = new User { Id = userId };

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Email", "Invalid email format"));

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockValidationService.Setup(x => x.ValidateAsync(updateRequest))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.UpdateUserAsync(updateRequest, userId));
    }

    #endregion

    #region OidcSignInAsync Tests

    [Fact]
    public async Task OidcSignInAsync_WithNewUserAndNoExistingUsers_CreatesAdminUser()
    {
        // Arrange
        var request = new OidcSignInDto
        {
            Email = "newuser@test.com",
            Username = "newuser",
            FirstName = "New",
            LastName = "User",
            OidcId = "oidc123"
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User)null);
        _mockUserRepository.Setup(x => x.UserExistsAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _service.OidcSignInAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal("admin", result.Role);
        Assert.Equal("oidc", result.AuthProvider);
        Assert.Equal(request.OidcId, result.OidcId);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task OidcSignInAsync_WithExistingUser_UpdatesOidcInfo()
    {
        // Arrange
        var request = new OidcSignInDto
        {
            Email = "existing@test.com",
            OidcId = "oidc123",
            FirstName = "Updated",
            LastName = "Name"
        };

        var existingUser = new User
        {
            Id = 1,
            Email = "existing@test.com",
            Username = "existing",
            FirstName = "Original",
            LastName = "User"
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _service.OidcSignInAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.OidcId, result.OidcId);
        Assert.Equal("oidc", result.AuthProvider);
        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("Name", result.LastName);
        Assert.Empty(result.PasswordHash);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task OidcSignInAsync_WithMismatchedOidcId_ThrowsBadRequestException()
    {
        // Arrange
        var request = new OidcSignInDto
        {
            Email = "user@test.com",
            OidcId = "oidc456",
            FirstName = "Test",
            LastName = "User",
            Username = "testuser",
        };

        var existingUser = new User
        {
            Email = "user@test.com",
            OidcId = "oidc123" // Different OIDC ID
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.OidcSignInAsync(request));
    }

    [Fact]
    public async Task OidcSignInAsync_WithMismatchedEmail_ThrowsBadRequestException()
    {
        // Arrange
        var request = new OidcSignInDto
        {
            Email = "different@test.com",
            OidcId = "oidc123"
        };

        var existingUser = new User
        {
            Email = "original@test.com", // Different email
            OidcId = "oidc123"
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.OidcSignInAsync(request));
    }

    [Fact]
    public async Task OidcSignInAsync_WithValidationError_ThrowsValidationException()
    {
        // Arrange
        var request = new OidcSignInDto
        {
            Email = "invalid-email",
            OidcId = "oidc123"
        };

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Email", "Invalid email format"));

        _mockValidationService.Setup(x => x.ValidateAsync(request))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.OidcSignInAsync(request));
    }

    #endregion
}