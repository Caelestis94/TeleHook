namespace TeleHook.Api.Exceptions;

public class TelegramApiException : BaseException
{
    public int? StatusCode { get; }
    public string? ErrorCode { get; }

    public TelegramApiException(string message) : base(message)
    {
    }

    public TelegramApiException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public TelegramApiException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public TelegramApiException(string message, int statusCode, string errorCode) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}