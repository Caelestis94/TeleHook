namespace TeleHook.Api.Models.Results;

public class WebhookProcessingResult : BaseResult
{
    private WebhookProcessingResult(int statusCode, bool isSuccess, string? error = null, object? response = null) :
        base(isSuccess, error)
    {
        IsSuccess = isSuccess;
        Response = response;
        Error = error;
        StatusCode = statusCode;
    }

    public object? Response { get; init; }
    public int StatusCode { get; init; }

    public static WebhookProcessingResult Success(object response) => new(200,
        true, response: response);

    public static WebhookProcessingResult Failure(string error, int statusCode = 400) => new(statusCode,
        false,
        error);
}
