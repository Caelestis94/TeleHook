using Microsoft.AspNetCore.Mvc;
using TeleHook.Api.DTO;
using TeleHook.Api.Middleware.Attributes;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Controllers;

[Route("api/bots")]
[ApiController]
public class BotController : ControllerBase
{
    private readonly IBotManagementService _botManagementService;

    public BotController(IBotManagementService botManagementService)
    {
        _botManagementService = botManagementService;
    }

    [HttpGet]
    [RequireApiKey]
    public async Task<ActionResult<IEnumerable<Bot>>> Get()
    {
        var bots = await _botManagementService.GetAllBotsAsync();
        return Ok(bots);
    }

    [HttpGet("{id}")]
    [RequireApiKey]
    public async Task<ActionResult<Bot>> Get(int id)
    {
        var bot = await _botManagementService.GetBotByIdAsync(id);

        return Ok(bot);
    }

    [HttpGet("{id}/webhooks")]
    [RequireApiKey]
    public async Task<ActionResult<IEnumerable<Webhook>>> GetWebhooksByBotId(int id)
    {
        var webhooks = await _botManagementService.GetBotWebhooksAsync(id);

        return Ok(webhooks);
    }

    [HttpPost]
    [RequireApiKey]
    public async Task<ActionResult<Bot>> Post([FromBody] CreateBotDto createBotRequest)
    {
        var bot = await _botManagementService.CreateBotAsync(createBotRequest);
        return Created($"/api/bots/{bot.Id}", bot);
    }

    [HttpPut]
    [RequireApiKey]
    [Route("{id}")]
    public async Task<ActionResult<Bot>> Put(int id, [FromBody] UpdateBotDto updateBotRequest)
    {
        var bot = await _botManagementService.UpdateBotAsync(id, updateBotRequest);

        return Ok(bot);
    }


    [HttpGet]
    [RequireApiKey]
    [Route("{id}/test")]
    public async Task<ActionResult<BotTestResult>> TestBot(int id)
    {
        var result = await _botManagementService.TestBotConnectionAsync(id);
        return Ok(result);
    }

    [HttpDelete]
    [RequireApiKey]
    [Route("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _botManagementService.DeleteBotAsync(id);
        return NoContent();
    }
}