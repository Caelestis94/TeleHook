using FluentValidation;
using FluentValidation.Results;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services;

public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ValidationResult> ValidateAsync<T>(T request)
    {
        var validator = _serviceProvider.GetRequiredService<IValidator<T>>();
        return await validator.ValidateAsync(request);
    }
}