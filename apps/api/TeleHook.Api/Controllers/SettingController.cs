using Microsoft.AspNetCore.Mvc;
using TeleHook.Api.DTO;
using TeleHook.Api.Middleware.Attributes;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingController : ControllerBase
{
    private readonly ISettingManagementService _settingManagementService;

    public SettingController(ISettingManagementService settingManagementService)
    {
        _settingManagementService = settingManagementService;
    }

    [HttpGet]
    [RequireApiKey]
    public async Task<ActionResult<AppSetting>> Get()
    {
        var settings = await _settingManagementService.GetSettingsAsync();

        return Ok(settings);
    }

    [HttpPut]
    [RequireApiKey]
    public async Task<ActionResult<SettingsUpdatedResponse>> Put([FromBody] AppSettingDto updateSettingsRequest)
    {
        var updatedSettings = await _settingManagementService.UpdateSettingsAsync(updateSettingsRequest);

        return Ok(updatedSettings);
    }

    [HttpGet]
    [RequireApiKey]
    [Route("notification/test")]
    public async Task<ActionResult<NotificationTestResult>> TestNotification()
    {
        var result = await _settingManagementService.TestNotificationAsync();

        return Ok(result);
    }
}
