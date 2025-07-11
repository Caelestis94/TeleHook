using FluentValidation;
using TeleHook.Api.DTO;

namespace TeleHook.Api.Validators;

public class RenderTemplateRequestValidator : AbstractValidator<RenderTemplateDto>
{
    public RenderTemplateRequestValidator()
    {
        RuleFor(x => x.Template)
            .NotEmpty()
            .WithMessage("Template is required")
            .MaximumLength(4000)
            .WithMessage("Template must be 4000 characters or less")
            .Must(ValidatorHelpers.BeValidScribanTemplate)
            .WithMessage("Template syntax is invalid");

        RuleFor(x => x.SampleData)
            .NotNull()
            .WithMessage("SampleData is required");
    }
}
