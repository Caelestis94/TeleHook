namespace TeleHook.Api.Models.Results;

public class TelegramResult : BaseResult
{
    private TelegramResult(int statusCode, bool isSuccess, string? error = null, string? errorDetails = null,string? responseMessage = null) : base(isSuccess, error)
    {
        StatusCode = statusCode;
        ErrorDetails = errorDetails;
        ResponseMessage = responseMessage;
    }

    public int StatusCode { get; init; }
    public string? ErrorDetails { get; init; }
    
    public string? ResponseMessage { get; set; }

    public static TelegramResult Success(string? responseMessage = null, int statusCode = 200) => new(statusCode,true, null, null, responseMessage);

    public static TelegramResult Failure(string error, int statusCode, string? errorDetails = null) => new(
        statusCode,
        false,
        error,
        errorDetails
    );

    public static TelegramResult NetworkError(string error) => new(
        502,
        false, error
    );
    
    public static TelegramResult Timeout(string error) => new(
        504,
        false,
        error
    );
}
