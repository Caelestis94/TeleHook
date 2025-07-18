using TeleHook.Api.Models;

namespace TeleHook.Api.DTO;

public class SettingsUpdatedResponse
{
    public required AppSetting Setting { get; set; }
    public bool IsRestartRequired { get; set; }
}
