namespace TeleHook.Api.Models.Results;

public abstract class BaseResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    protected BaseResult(bool isSuccess, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
}