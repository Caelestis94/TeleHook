using TeleHook.Api.Models;

namespace TeleHook.Api.Repositories.Interfaces;

public interface IWebhookRepository : IRepository<Webhook>
{
    Task<Webhook?> GetByUuidAsync(string uuid);
    Task<Webhook?> GetByIdWithRelationsAsync(int id);
    Task<Webhook?> GetByUuidWithRelationsAsync(string uuid);
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsByNameExcludingIdAsync(string name, int excludeId);
    Task<IEnumerable<Webhook>> GetWithRelationsAsync();
    Task<IEnumerable<Webhook>> GetActiveWebhooksAsync();
    Task<IEnumerable<Webhook>> GetByBotIdAsync(int botId);
}
