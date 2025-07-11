using TeleHook.Api.DTO;
using TeleHook.Api.Models;

namespace TeleHook.Api.Services.Interfaces;

public interface IUserManagementService
{
    Task<bool> SetupIsRequiredAsync();
    Task<User> CreateAdminUserAsync(CreateUserDto createUserDto);
    Task<User> SignInAsync(EmailPasswordSignInDto signInRequest);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User> UpdateUserAsync(UpdateUserDto updateUserRequest, int userId);
    Task<User> OidcSignInAsync(OidcSignInDto oidcSignInRequest);
}