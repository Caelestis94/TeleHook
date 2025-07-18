namespace TeleHook.Api.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "Access to this resource is forbidden") : base(message)
    {
    }
}
