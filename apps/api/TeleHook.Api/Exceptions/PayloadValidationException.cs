namespace TeleHook.Api.Exceptions;

public class PayloadValidationException : BaseException
{
    public IEnumerable<string> ValidationErrors { get; }
    
    public PayloadValidationException(IEnumerable<string> validationErrors) 
        : base("Payload validation failed")
    {
        ValidationErrors = validationErrors;
    }
}