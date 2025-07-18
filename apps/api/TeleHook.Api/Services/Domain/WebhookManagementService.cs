using System.Security.Cryptography;
using TeleHook.Api.DTO;
using TeleHook.Api.Exceptions;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Domain;

public class WebhookManagementService : IWebhookManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly ILogger<WebhookManagementService> _logger;
    private readonly ITemplateParsingService _templateParsingService;

    public WebhookManagementService(IUnitOfWork unitOfWork,
        IValidationService validationService,
        ILogger<WebhookManagementService> logger,
        ITemplateParsingService templateParsingService)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _logger = logger;
        _templateParsingService = templateParsingService;
    }

    public async Task<IEnumerable<Webhook>> GetAllWebhooksAsync()
    {
        return await _unitOfWork.Webhooks.GetWithRelationsAsync();
    }

    public async Task<Webhook?> GetWebhookByIdAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID '{WebhookId}' provided for webhook retrieval", id);
            throw new BadRequestException("Invalid ID");
        }

        _logger.LogDebug("Fetching webhook configuration with ID '{WebhookId}'", id);

        var webhook = await _unitOfWork.Webhooks.GetByIdWithRelationsAsync(id);

        if (webhook == null)
        {
            _logger.LogWarning("Failed to find webhook with ID '{WebhookId}'", id);
            throw new NotFoundException("Webhook", id);
        }

        return webhook;
    }

    public async Task<Webhook> CreateWebhookAsync(CreateWebhookDto createWebhookRequest)
    {
        _logger.LogDebug("Creating new webhook '{WebhookName}'", createWebhookRequest.Name);

        var validationResult = await _validationService.ValidateAsync(createWebhookRequest);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed for webhook creation: {Errors}", string.Join(", ", errors));
            throw new ValidationException(errors);
        }

        var webhook = new Webhook
        {
            Name = createWebhookRequest.Name,
            Uuid = Guid.NewGuid().ToString(),
            BotId = createWebhookRequest.BotId,
            TopicId = createWebhookRequest.TopicId,
            PayloadSample = createWebhookRequest.PayloadSample,
            MessageTemplate = createWebhookRequest.MessageTemplate,
            ParseMode = createWebhookRequest.ParseMode,
            DisableWebPagePreview = createWebhookRequest.DisableWebPagePreview,
            DisableNotification = createWebhookRequest.DisableNotification,
            IsDisabled = createWebhookRequest.IsDisabled,
            CreatedAt = DateTime.UtcNow,
            IsProtected = createWebhookRequest.IsProtected,
            SecretKey = createWebhookRequest.SecretKey ?? string.Empty
        };

        await _unitOfWork.Webhooks.AddAsync(webhook);
        await _unitOfWork.SaveChangesAsync();

        await _templateParsingService.RefreshTemplateAsync(webhook.Id);

        _logger.LogInformation(
            "Successfully created webhook '{WebhookUUID}' with ID '{WebhookId}'",
            webhook.Uuid, webhook.Id);

        return webhook;
    }

    public async Task<Webhook> UpdateWebhookAsync(int id, UpdateWebhookDto updateWebhookRequest)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID '{WebhookId}' provided for webhook update", id);
            throw new BadRequestException("Invalid ID provided.");
        }

        _logger.LogDebug("Updating webhook with ID '{WebhookId}'", id);

        var webhookToUpdate = await _unitOfWork.Webhooks.GetByIdWithRelationsAsync(id);
        if (webhookToUpdate == null)
        {
            _logger.LogWarning("Failed to update webhook: webhook with ID '{WebhookId}' not found", id);
            throw new NotFoundException("Webhook", id);
        }

        var validationResult = await _validationService.ValidateAsync(updateWebhookRequest);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed for webhook update: {Errors}", string.Join(", ", errors));
            throw new ValidationException(errors);
        }

        var oldName = webhookToUpdate.Name;
        var oldIsDisabled = webhookToUpdate.IsDisabled;

        webhookToUpdate.Name = updateWebhookRequest.Name;
        webhookToUpdate.BotId = updateWebhookRequest.BotId;
        webhookToUpdate.TopicId = updateWebhookRequest.TopicId;
        webhookToUpdate.PayloadSample = updateWebhookRequest.PayloadSample;
        webhookToUpdate.MessageTemplate = updateWebhookRequest.MessageTemplate;
        webhookToUpdate.ParseMode = updateWebhookRequest.ParseMode;
        webhookToUpdate.DisableWebPagePreview = updateWebhookRequest.DisableWebPagePreview;
        webhookToUpdate.DisableNotification = updateWebhookRequest.DisableNotification;
        webhookToUpdate.IsDisabled = updateWebhookRequest.IsDisabled;
        webhookToUpdate.IsProtected = updateWebhookRequest.IsProtected;
        webhookToUpdate.SecretKey = updateWebhookRequest.SecretKey;

        await _unitOfWork.Webhooks.UpdateAsync(webhookToUpdate);
        await _unitOfWork.SaveChangesAsync();

        await _templateParsingService.RefreshTemplateAsync(webhookToUpdate.Id);

        _logger.LogInformation(
            "Successfully updated webhook '{WebhookUUID}' (ID: {WebhookId}). Name changed: '{NameChanged}', Status changed: '{StatusChanged}'",
            webhookToUpdate.Uuid, id, oldName != webhookToUpdate.Name,
            oldIsDisabled != webhookToUpdate.IsDisabled);

        return webhookToUpdate;
    }

    public async Task DeleteWebhookAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID '{WebhookId}' provided for webhook deletion", id);
            throw new BadRequestException("Invalid ID provided.");
        }

        _logger.LogDebug("Deleting webhook with ID {WebhookId}", id);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var webhook = await _unitOfWork.Webhooks.GetByIdAsync(id);
            if (webhook == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning("Failed to delete webhook: webhook with ID '{WebhookId}' not found", id);
                throw new NotFoundException("Webhook", id);
            }

            var webhookName = webhook.Name;
            var webhookUuid = webhook.Uuid;

            // Delete related logs first (cascading delete)
            var logs = await _unitOfWork.WebhookLogs.GetByWebhookIdAsync(id);

            foreach (var log in logs)
            {
                await _unitOfWork.WebhookLogs.DeleteAsync(log.Id);
            }

            // Delete related stats (cascading delete)  
            var stats = await _unitOfWork.WebhookStats.GetByWebhookIdAsync(id);
            foreach (var stat in stats)
            {
                await _unitOfWork.WebhookStats.DeleteAsync(stat.Id);
            }

            await _unitOfWork.Webhooks.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "Successfully deleted webhook '{WebhookName}' (ID: {WebhookId}, UUID: {WebhookUUID}) with {LogCount} logs and {StatCount} stats",
                webhookName, id, webhookUuid, logs.Count(), stats.Count());
        }
        catch (BaseException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError("Error deleting webhook with ID {WebhookId}", id);
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error deleting webhook with ID {WebhookId}", id);
            throw;
        }
    }

    public async Task<SecretKeyResult> GenerateSecretKeyAsync(GenerateSecretKeyDto generateSecretKeyRequest)
    {
        var id = generateSecretKeyRequest.WebhookId;

        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID '{WebhookId}' provided for webhook secret key generation", id);
            throw new BadRequestException("Invalid ID provided.");
        }

        _logger.LogDebug("Generating secret key for webhook with ID '{WebhookId}'", id);

        var bytes = new byte[24];
        RandomNumberGenerator.Fill(bytes);
        var key = "sk_" + Convert.ToHexString(bytes).ToLower();

        var webhook = await _unitOfWork.Webhooks.GetByIdAsync(id);
        if (webhook == null)
        {
            _logger.LogWarning("Failed to find webhook with ID '{WebhookId}' for secret key generation", id);
            throw new NotFoundException("Webhook", id);
        }

        webhook.SecretKey = key;
        await _unitOfWork.Webhooks.UpdateAsync(webhook);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully generated secret key for webhook '{WebhookUUID}' (ID: {WebhookId})",
            webhook.Uuid, id);

        return new SecretKeyResult
        {
            Message = "Secret key generated successfully",
            SecretKey = key
        };
    }

    public SecretKeyResult GenerateSecretKeyForNewWebhook()
    {
        _logger.LogDebug("Generating secret key for new webhook (creation flow)");

        var bytes = new byte[24];
        RandomNumberGenerator.Fill(bytes);
        var key = "sk_" + Convert.ToHexString(bytes).ToLower();

        _logger.LogInformation("Secret key generated successfully for new webhook");

        return new SecretKeyResult
        {
            Message = "Secret key generated successfully for new webhook",
            SecretKey = key
        };
    }

    public async Task<IEnumerable<WebhookLogExportResponse>> ExportWebhookLogsAsync()
    {
        _logger.LogDebug("Exporting all webhook logs");

        var logs = await _unitOfWork.WebhookLogs.GetAllLogsForExportAsync();

        _logger.LogInformation("Successfully exported {LogCount} webhook logs", logs.Count());

        return logs;
    }
}
