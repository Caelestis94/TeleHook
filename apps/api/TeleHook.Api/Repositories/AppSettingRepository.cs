using Microsoft.EntityFrameworkCore;
using TeleHook.Api.Data;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Repositories;


public class AppSettingRepository  : Repository<AppSetting>, IAppSettingRepository
{
    public AppSettingRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> SettingsExistAsync()
    {
        return await _dbSet.AnyAsync();
    }

    public Task<AppSetting?> GetSettingsAsync()
    {
        return _dbSet.FirstOrDefaultAsync(s=> s.Id == 1);
    }
}