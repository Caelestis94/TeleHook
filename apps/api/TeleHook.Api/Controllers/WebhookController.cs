using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TeleHook.Api.DTO;
using TeleHook.Api.Middleware.Attributes;
using TeleHook.Api.Models;
using TeleHook.Api.Repositories.Interfaces;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebhookStatService _webhookStatService;
    private readonly IWebhookManagementService _webhookManagementService;
    private readonly IWebhookProcessingService _webhookProcessingService;

    public WebhookController(IUnitOfWork unitOfWork,
        IWebhookStatService webhookStatService,
        IWebhookManagementService webhookManagementService,
        IWebhookProcessingService webhookProcessingService)
    {
        _unitOfWork = unitOfWork;
        _webhookStatService = webhookStatService;
        _webhookManagementService = webhookManagementService;
        _webhookProcessingService = webhookProcessingService;
    }

    [HttpGet]
    [RequireApiKey]
    public async Task<ActionResult<IEnumerable<Webhook>>> Get()
    {
        var webhooks = await _webhookManagementService.GetAllWebhooksAsync();
        return Ok(webhooks);
    }

    [HttpGet]
    [RequireApiKey]
    [Route("{id}")]
    public async Task<ActionResult<Webhook>> Get(int id)
    {
        var webhook = await _webhookManagementService.GetWebhookByIdAsync(id);
        return Ok(webhook);
    }


    [HttpPost]
    [RequireApiKey]
    public async Task<ActionResult<Webhook>> Post([FromBody] CreateWebhookDto createWebhookRequest)
    {
        var webhook = await _webhookManagementService.CreateWebhookAsync(createWebhookRequest);
        return Created($"/api/webhooks/{webhook.Id}", webhook);
    }

    [HttpPut]
    [RequireApiKey]
    [Route("{id}")]
    public async Task<ActionResult<Webhook>> Put(int id, [FromBody] UpdateWebhookDto updateWebhookRequest)
    {
        var webhook = await _webhookManagementService.UpdateWebhookAsync(id, updateWebhookRequest);
        return Ok(webhook);
    }

    [HttpDelete]
    [RequireApiKey]
    [Route("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _webhookManagementService.DeleteWebhookAsync(id);
        return NoContent();
    }


    [HttpPost]
    [AllowAnonymous]
    [Route("/api/trigger/{uuid}")]
    public async Task<ActionResult> Post(string uuid, [FromBody] JsonElement payload)
    {
        var result = await _webhookProcessingService.ProcessWebhookAsync(uuid, payload, Request);
        return StatusCode(result.StatusCode, result.Response ?? result.Error);
    }


    [HttpPost]
    [RequireApiKey]
    [Route("generate-key")]
    public async Task<ActionResult<SecretKeyResult>> GenerateKey(GenerateSecretKeyDto generateSecretKeyRequest)
    {
        var result = await _webhookManagementService.GenerateSecretKeyAsync(generateSecretKeyRequest);
        return Ok(result);
    }

    [HttpPost]
    [RequireApiKey]
    [Route("generate-key/new")]
    public ActionResult<SecretKeyResult> GenerateKeyForNew()
    {
        var result = _webhookManagementService.GenerateSecretKeyForNewWebhook();
        return Ok(result);
    }

    [HttpGet]
    [RequireApiKey]
    [Route("logs")]
    public async Task<ActionResult<List<WebhookLog>>> GetLogs(
        [FromQuery] int? webhookId,
        [FromQuery] int? statusCode,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? searchTerm,
        [FromQuery] int limit = 100)
    {
        var logs = await _unitOfWork.WebhookLogs.GetFilteredLogsAsync(
            webhookId, statusCode, dateFrom, dateTo, searchTerm, limit);
        return Ok(logs);
    }

    [HttpGet]
    [RequireApiKey]
    [Route("logs/export")]
    public async Task<ActionResult<IEnumerable<WebhookLogExportResponse>>> ExportLogs()
    {
        var logs = await _webhookManagementService.ExportWebhookLogsAsync();
        return Ok(logs);
    }

    [HttpGet]
    [RequireApiKey]
    [Route("stats/overview")]
    public async Task<ActionResult<OverviewStatsResponse>> GetOverviewStats()
    {
        var stats = await _webhookStatService.GetOverviewStatsAsync();
        return Ok(stats);
    }

    [HttpGet]
    [RequireApiKey]
    [Route("stats/webhook/{id}")]
    public async Task<ActionResult<WebhookStatsResponse>> GetWebhookStats(int id, [FromQuery] int days = 30)
    {
        var stats = await _webhookStatService.GetWebhookStatsAsync(id, days);
        return Ok(stats);
    }
}
