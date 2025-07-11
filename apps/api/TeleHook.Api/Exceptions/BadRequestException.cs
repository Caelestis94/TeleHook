namespace TeleHook.Api.Exceptions;

public class BadRequestException : BaseException
{
    public IEnumerable<string>? Details { get; }

    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, IEnumerable<string> details) : base(message)
    {
        Details = details;
    }
    
}