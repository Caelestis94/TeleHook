namespace TeleHook.Api.Models.Results;

public enum SessionOperationResult 
{
    Success,
    SessionNotFound,
    SessionExpired,
    SessionAlreadyCompleted,
    SessionCancelled
}
