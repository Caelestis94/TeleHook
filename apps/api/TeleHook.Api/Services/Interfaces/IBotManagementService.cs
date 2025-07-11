using TeleHook.Api.DTO;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;

namespace TeleHook.Api.Services.Interfaces;

public interface IBotManagementService
{
    Task<IEnumerable<Bot>> GetAllBotsAsync();
    Task<Bot?> GetBotByIdAsync(int id);
    Task<Bot> CreateBotAsync(CreateBotDto dto);
    Task<Bot> UpdateBotAsync(int id, UpdateBotDto updateBotRequest);
    Task DeleteBotAsync(int id);
    Task<BotTestResult> TestBotConnectionAsync(int id);
    Task<IEnumerable<Webhook>> GetBotWebhooksAsync(int id);
}

