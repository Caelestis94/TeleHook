namespace TeleHook.Api.Models.Results;

public class BotTestResult : BaseResult
{
    private  BotTestResult(bool isSuccess, string? error = null) : base(isSuccess, error) {}
    public static BotTestResult Success() => new(true);
    public static BotTestResult Failure(string error) => new(false, error);
}
