namespace TeleHook.Api.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IWebhookRepository Webhooks { get; }
    IBotRepository Bots { get; }
    IWebhookLogRepository WebhookLogs { get; }
    IWebhookStatRepository WebhookStats { get; }
    IUserRepository Users { get; }
    IAppSettingRepository AppSettings { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
