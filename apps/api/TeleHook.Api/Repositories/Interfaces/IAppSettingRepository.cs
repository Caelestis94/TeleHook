using TeleHook.Api.Models;

namespace TeleHook.Api.Repositories.Interfaces;

public interface IAppSettingRepository : IRepository<AppSetting>
{
    Task<bool> SettingsExistAsync();
    
    Task<AppSetting?> GetSettingsAsync();
}