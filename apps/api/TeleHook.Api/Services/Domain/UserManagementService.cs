using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Domain;

public class UserManagementService : IUserManagementService
{
    private readonly ILogger<UserManagementService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;

    public UserManagementService(ILogger<UserManagementService> logger,
        IUnitOfWork unitOfWork,
        IValidationService validationService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _validationService = validationService;
    }

    public async Task<bool> SetupIsRequiredAsync()
    {
        var userExists = await _unitOfWork.Users.UserExistsAsync();

        if (!userExists)
        {
            _logger.LogInformation("No users found, setup is required.");
        }
        else
        {
            _logger.LogInformation("Users already exist, setup is not required.");
        }

        return !userExists;
    }

    public async Task<User> CreateAdminUserAsync(CreateUserDto createUserDto)
    {
        if (await _unitOfWork.Users.UserExistsAsync())
        {
            throw new ConflictException("An initial user already exists. Setup is not required.");
        }

        _logger.LogDebug("Creating admin user with email '{Email}' and username '{Username}'",
            createUserDto.Email, createUserDto.Username);

        var validationResult = await _validationService.ValidateAsync(createUserDto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed for user creation: {Errors}", string.Join(", ", errors));
            throw new ValidationException(errors);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        var newUser = new User
        {
            Email = createUserDto.Email,
            Username = createUserDto.Username,
            PasswordHash = passwordHash,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Role = "admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();

        newUser.PasswordHash = string.Empty;

        _logger.LogInformation("Admin user created with email '{Email}' and username '{Username}'",
            newUser.Email, newUser.Username);

        return newUser;
    }

    public async Task<User> SignInAsync(EmailPasswordSignInDto signInRequest)
    {
        if (string.IsNullOrEmpty(signInRequest.Email) || string.IsNullOrEmpty(signInRequest.Password))
        {
            _logger.LogWarning("Login attempt with empty email or password");
            throw new BadRequestException("Email and password are required");
        }

        var isValid = await _unitOfWork.Users.ValidatePasswordAsync(signInRequest.Email, signInRequest.Password);

        if (!isValid)
        {
            _logger.LogWarning("Login attempt with invalid credentials for email: {Email}", signInRequest.Email);
            throw new UnauthorizedException("Invalid email or password");
        }

        var user = await _unitOfWork.Users.GetByEmailAsync(signInRequest.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt with non-existing email: {Email}", signInRequest.Email);
            throw new UnauthorizedException("Invalid email or password");
        }

        user.PasswordHash = string.Empty;

        return user;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid user ID '{UserId}' provided", userId);
            throw new BadRequestException("Invalid user ID provided.");
        }
        _logger.LogDebug("Retrieving user with ID '{UserId}'", userId);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("User with ID '{UserId}' not found", userId);
            throw new NotFoundException($"User", userId);
        }

        user.PasswordHash = string.Empty;

        return user;
    }

    public async Task<User> UpdateUserAsync(UpdateUserDto updateUserRequest, int userId)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid user ID '{UserId}' provided for user update", userId);
            throw new BadRequestException("Invalid user ID provided.");
        }

        _logger.LogDebug("Updating user with ID '{UserId}'", userId);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID '{UserId}' not found for update", userId);
            throw new NotFoundException("User", userId);
        }

        var validationResult = await _validationService.ValidateAsync(updateUserRequest);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed for user update: {Errors}", string.Join(", ", errors));
            throw new ValidationException(errors);
        }

        user.Email = updateUserRequest.Email ?? user.Email;
        user.FirstName = updateUserRequest.FirstName ?? user.FirstName;
        user.LastName = updateUserRequest.LastName ?? user.LastName;
        user.Username = updateUserRequest.Username ?? user.Username;

        if (!string.IsNullOrEmpty(updateUserRequest.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserRequest.Password);
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        user.PasswordHash = string.Empty;

        _logger.LogInformation("User with ID '{UserId}' updated successfully", userId);

        return user;
    }

    public async Task<User> OidcSignInAsync(OidcSignInDto oidcSignInRequest)
    {
        _logger.LogDebug("Processing OIDC signin request for email '{Email}'", oidcSignInRequest.Email);

        var validationResult = await _validationService.ValidateAsync(oidcSignInRequest);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed for OIDC signin: {Errors}", string.Join(", ", errors));
            throw new ValidationException(errors);
        }

        var existingUser = await _unitOfWork.Users.GetByEmailAsync(oidcSignInRequest.Email);

        if (existingUser == null)
        {
            var userExists = await _unitOfWork.Users.UserExistsAsync();

            if (!userExists)
            {
                _logger.LogDebug("Creating initial admin user for OIDC signin with email '{Email}'",
                    oidcSignInRequest.Email);

                var newUser = new User
                {
                    Email = oidcSignInRequest.Email,
                    Username = oidcSignInRequest.Username ?? oidcSignInRequest.Email.Split('@')[0],
                    FirstName = oidcSignInRequest.FirstName ?? "",
                    LastName = oidcSignInRequest.LastName ?? "",
                    Role = "admin",
                    OidcId = oidcSignInRequest.OidcId,
                    AuthProvider = "oidc",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created initial admin user for OIDC signin with email '{Email}'",
                    newUser.Email);
                return newUser;
            }
        }

        if (!string.IsNullOrEmpty(existingUser.OidcId) && existingUser.OidcId != oidcSignInRequest.OidcId)
        {
            _logger.LogWarning("OIDC ID mismatch for email: {Email}", oidcSignInRequest.Email);
            throw new BadRequestException("OIDC ID already linked to another account.");
        }

        if (existingUser.Email != oidcSignInRequest.Email)
        {
            _logger.LogWarning("Email mismatch for OIDC signin: {Email}", oidcSignInRequest.Email);
            throw new BadRequestException("Email does not match the existing user.");
        }

        existingUser.OidcId = oidcSignInRequest.OidcId;
        existingUser.AuthProvider = "oidc";
        existingUser.FirstName =
            !string.IsNullOrEmpty(oidcSignInRequest.FirstName) ? oidcSignInRequest.FirstName : existingUser.FirstName;
        existingUser.LastName = !string.IsNullOrEmpty(oidcSignInRequest.LastName)
            ? oidcSignInRequest.LastName
            : existingUser.LastName;
        existingUser.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(existingUser);
        await _unitOfWork.SaveChangesAsync();

        existingUser.PasswordHash = string.Empty;

        _logger.LogInformation("OIDC signin successful for user with email '{Email}'", existingUser.Email);
        return existingUser;
    }
}
