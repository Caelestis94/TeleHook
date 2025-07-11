using TeleHook.Api.Models;

namespace TeleHook.Api.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ValidatePasswordAsync(string email, string password);
    Task<bool> UserExistsAsync();
}