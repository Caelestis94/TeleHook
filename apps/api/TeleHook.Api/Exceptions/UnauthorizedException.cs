namespace TeleHook.Api.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Authentication is required") : base(message)
    {
    }
}
