using Microsoft.EntityFrameworkCore;
using TeleHook.Api.Data;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        var user = await GetByEmailAsync(email);
        if (user == null) return false;

        // Use BCrypt to verify password
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<bool> UserExistsAsync()
    {
        return await _dbSet.AnyAsync();
    }
}
