namespace TeleHook.Api.Exceptions;

public class ValidationException : BaseException
{
    public IEnumerable<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors) : base("Validation failed")
    {
        Errors = errors;
    }
}