namespace TeleHook.Api.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string resourceType, string conflictField, object value) 
        : base($"{resourceType} with {conflictField} '{value}' already exists")
    {
        
    }
    
    public ConflictException(string message) : base(message)
    {
        
    }
}