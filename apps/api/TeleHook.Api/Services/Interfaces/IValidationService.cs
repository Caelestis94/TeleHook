using FluentValidation.Results;

namespace TeleHook.Api.Services.Interfaces;

public interface IValidationService
{
    Task<ValidationResult> ValidateAsync<T>(T request);
}