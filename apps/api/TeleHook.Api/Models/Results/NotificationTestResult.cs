namespace TeleHook.Api.Models.Results;

public class NotificationTestResult : BaseResult
{
    private NotificationTestResult(bool isSuccess, string? error = null) : base(isSuccess, error){}
    public static NotificationTestResult Success() => new(true);
    public static NotificationTestResult Failure(string error) => new(false, error);
}