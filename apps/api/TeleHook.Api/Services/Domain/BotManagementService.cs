using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Domain;

public class BotManagementService : IBotManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly ILogger<BotManagementService> _logger;
    private readonly ITelegramService _telegramService;

    public BotManagementService(IUnitOfWork unitOfWork,
        IValidationService validationService,
        ILogger<BotManagementService> logger,
        ITelegramService telegramService)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _logger = logger;
        _telegramService = telegramService;
    }

    public async Task<IEnumerable<Bot>> GetAllBotsAsync()
    {
        return await _unitOfWork.Bots.GetAllAsync();
    }

    public async Task<Bot?> GetBotByIdAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID {Id} provided for bot", id);
            throw new BadRequestException("Invalid ID provided.");
        }

        _logger.LogDebug("Fetching bot with ID '{Id}'", id);

        var bot = await _unitOfWork.Bots.GetByIdAsync(id);

        if (bot == null)
        {
            _logger.LogWarning("Failed to find bot with ID '{Id}'", id);
            throw new NotFoundException("Bot", id);
        }

        return bot;
    }

    public async Task<Bot> CreateBotAsync(CreateBotDto dto)
    {
        var tokenSuffix = dto.BotToken?.Length >= 4
            ? dto.BotToken.Substring(dto.BotToken.Length - 4)
            : "****";
        _logger.LogDebug("Creating bot configuration with token ending in ...{TokenSuffix}", tokenSuffix);

        var validationResult = await _validationService.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed for Bot creation: {Errors}", string.Join(", ", errors));
            throw new ValidationException(errors);
        }

        var newBot = new Bot
        {
            Name = dto.Name,
            BotToken = dto.BotToken,
            ChatId = dto.ChatId,
            HasPassedTest = false
        };

        await _unitOfWork.Bots.AddAsync(newBot);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Bot '{Name}' created (ID: {Id})", newBot.Name, newBot.Id);

        return newBot;
    }

    public async Task<Bot> UpdateBotAsync(int id, UpdateBotDto updateBotRequest)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID '{Id}' provided for bot update", id);
            throw new BadRequestException("Invalid ID provided.");
        }

        _logger.LogDebug("Updating Telegram bot configuration with ID '{Id}'", id);

        var botToUpdate = await _unitOfWork.Bots.GetByIdAsync(id);
        if (botToUpdate == null)
        {
            _logger.LogWarning("Failed to find bot to update with ID '{Id}'", id);
            throw new NotFoundException("Bot", id);
        }

        var validationResult = await _validationService.ValidateAsync(updateBotRequest);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed for Bot update: {Errors}", string.Join(", ", errors));
            throw new ValidationException(errors);
        }

        botToUpdate.Name = updateBotRequest.Name;
        botToUpdate.ChatId = updateBotRequest.ChatId;
        botToUpdate.BotToken = updateBotRequest.BotToken;
        botToUpdate.HasPassedTest = false; // Reset test status on update

        await _unitOfWork.Bots.UpdateAsync(botToUpdate);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Bot '{Name}' updated (ID: {Id})", botToUpdate.Name, id);
        return botToUpdate;
    }

    public async Task DeleteBotAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID '{Id}' provided for bot deletion", id);
            throw new BadRequestException("Invalid ID provided.");
        }

        _logger.LogDebug("Deleting bot with ID '{Id}'", id);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var existingBot = await _unitOfWork.Bots.GetByIdAsync(id);
            if (existingBot == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning("Bot with ID '{Id}' does not exist", id);
                throw new NotFoundException("Bot", id);
            }

            var dependentWebhooks = await _unitOfWork.Webhooks.GetByBotIdAsync(id);

            if (dependentWebhooks.Any())
            {
                await _unitOfWork.RollbackTransactionAsync();
                var webhooksNames = string.Join(", ", dependentWebhooks.Select(e => e.Name));
                _logger.LogWarning("Cannot delete Bot '{ConfigName}' - it's used by webhooks: {WebhookNames}",
                    existingBot.Name, webhooksNames);
                throw new ConflictException("Bot",
                    $"Cannot delete Telegram bot '{existingBot.Name}'. It is currently used by webhooks : {webhooksNames}",
                    existingBot.Name);
            }

            await _unitOfWork.Bots.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Bot '{Name}' deleted (ID: {Id})", existingBot.Name, id);
        }
        catch (BaseException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError("Error deleting Bot with ID {Id}", id);
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Unexpected error deleting Bot with ID {Id}", id);
            throw new InternalServerErrorException(
                "An unexpected error occurred while deleting the Telegram configuration.", ex);
        }
    }

    public async Task<BotTestResult> TestBotConnectionAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID '{Id}' provided for Telegram bot configuration test", id);
            throw new BadRequestException("Invalid ID provided.");
        }

        var bot = await _unitOfWork.Bots.GetByIdAsync(id);
        if (bot == null)
        {
            _logger.LogWarning("Telegram bot configuration with ID '{Id}' does not exist", id);
            throw new NotFoundException("Bot", id);
        }

        var testResult = await _telegramService.TestConnectionAsync(bot);

        if (!testResult.IsSuccess)
        {
            bot.HasPassedTest = false;
            _logger.LogWarning("Telegram bot connection test failed for bot ID '{Id}': {Error}", id, testResult.Error);
        }
        else
        {
            bot.HasPassedTest = true;
            _logger.LogInformation("Telegram bot connection test passed for bot ID '{Id}'", id);
        }
        await _unitOfWork.Bots.UpdateAsync(bot);
        await _unitOfWork.SaveChangesAsync();
        
        return testResult.IsSuccess 
            ? BotTestResult.Success() 
            : BotTestResult.Failure(testResult.Error);

    }

    public async Task<IEnumerable<Webhook>> GetBotWebhooksAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID '{Id}' provided for Telegram bot configuration", id);
            throw new BadRequestException("Invalid ID provided.");
        }

        _logger.LogDebug("Fetching webhooks for Telegram bot configuration with ID '{Id}'", id);

        var bot = await _unitOfWork.Bots.GetByIdAsync(id);
        if (bot == null)
        {
            _logger.LogWarning("Telegram bot configuration with ID '{Id}' does not exist", id);
            throw new NotFoundException("Bot", id);
        }

        var webhooks = await _unitOfWork.Webhooks.GetByBotIdAsync(id);

        return webhooks;
    }
}