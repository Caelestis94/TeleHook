using Microsoft.EntityFrameworkCore.Storage;
using TeleHook.Api.Data;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        AppDbContext context,
        IWebhookRepository webhooks,
        IBotRepository bots,
        IWebhookLogRepository webhookLogs,
        IWebhookStatRepository webhookStats,
        IUserRepository users,
        IAppSettingRepository appSettings)
    {
        _context = context;
        Webhooks = webhooks;
        Bots = bots;
        WebhookLogs = webhookLogs;
        WebhookStats = webhookStats;
        Users = users;
        AppSettings = appSettings;
    }

    public IWebhookRepository Webhooks { get; }
    public IBotRepository Bots { get; }
    public IWebhookLogRepository WebhookLogs { get; }
    public IWebhookStatRepository WebhookStats { get; }
    public IUserRepository Users { get; }
    public IAppSettingRepository AppSettings { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
