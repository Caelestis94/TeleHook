using Microsoft.EntityFrameworkCore;
using TeleHook.Api.Data;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Repositories;

public class WebhookRepository : Repository<Webhook>, IWebhookRepository
{
    public WebhookRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Webhook?> GetByUuidAsync(string uuid)
    {
        return await _dbSet.FirstOrDefaultAsync(we => we.Uuid == uuid);
    }

    public async Task<Webhook?> GetByIdWithRelationsAsync(int id)
    {
        return await _dbSet
            .Include(e => e.Bot)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Webhook?> GetByUuidWithRelationsAsync(string uuid)
    {
        return await _dbSet
            .Include(e => e.Bot)
            .FirstOrDefaultAsync(e => e.Uuid == uuid);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbSet.AnyAsync(e => e.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string name, int excludeId)
    {
        return await _dbSet.AnyAsync(we => we.Name.ToLower() == name.ToLower() && we.Id != excludeId);
    }

    public async Task<IEnumerable<Webhook>> GetWithRelationsAsync()
    {
        return await _dbSet
            .Include(e => e.Bot)
            .ToListAsync();
    }

    public async Task<IEnumerable<Webhook>> GetActiveWebhooksAsync()
    {
        return await _dbSet
            .Include(e => e.Bot)
            .Where(e => !e.IsDisabled)
            .ToListAsync();
    }

    public async Task<IEnumerable<Webhook>> GetByBotIdAsync(int botId)
    {
        return await _dbSet
            .Where(we => we.BotId == botId)
            .ToListAsync();
    }

}
