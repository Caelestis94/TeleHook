namespace TeleHook.Api.Models.Results;

public class MessageFormattingResult : BaseResult
{
    public string? MessageText { get; init; }

    private MessageFormattingResult(bool isSuccess, string? messageText = null, string? error = null) : base(isSuccess, error)
    {
        MessageText = messageText;
    }
    
    
    public static MessageFormattingResult Success(string messageText) => new(true,messageText: messageText);

    public static MessageFormattingResult Failure(string error) => new(false, error: error);

}
