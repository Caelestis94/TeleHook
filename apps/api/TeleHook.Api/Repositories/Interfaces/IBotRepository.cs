using TeleHook.Api.Models;

namespace TeleHook.Api.Repositories.Interfaces;

public interface IBotRepository : IRepository<Bot>
{
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsByNameExcludingIdAsync(string name, int excludeId);
}
