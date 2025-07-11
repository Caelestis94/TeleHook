using Microsoft.EntityFrameworkCore;
using TeleHook.Api.Data;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Repositories;

public class BotRepository : Repository<Bot>, IBotRepository
{
    public BotRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbSet.AnyAsync(tc => tc.Name == name);
    }
    
    public async Task<bool> ExistsByNameExcludingIdAsync(string name, int excludeId)
    {
        return await _dbSet.AnyAsync(tc => tc.Name == name && tc.Id != excludeId);
    }
}
