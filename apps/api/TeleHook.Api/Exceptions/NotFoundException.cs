namespace TeleHook.Api.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string resourceType, object id) : base($"{resourceType} with ID '{id}' was not found")
    {
    }
}