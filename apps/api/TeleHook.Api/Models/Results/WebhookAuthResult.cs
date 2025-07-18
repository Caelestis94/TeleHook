namespace TeleHook.Api.Models.Results;

public class WebhookAuthResult : BaseResult
{
    public int StatusCode { get; set; }

    private WebhookAuthResult(bool isSuccess, int statusCode = 200, string? error = null) : base(isSuccess, error)
    {
        StatusCode = statusCode;
    }

    public static WebhookAuthResult Success() => new(true);
    public static WebhookAuthResult Failure(string error, int statusCode) => new(false, statusCode, error);
}
