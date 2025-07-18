using FluentValidation;
using TeleHook.Api.DTO;

namespace TeleHook.Api.Validators;

public class OidcSignInRequestValidator : AbstractValidator<OidcSignInDto>
{
    public OidcSignInRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required for OIDC signin")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");

        RuleFor(x => x.OidcId)
            .NotEmpty()
            .WithMessage("OIDC ID is required for OIDC signin");

        When(x => !string.IsNullOrEmpty(x.Username), () =>
        {
            RuleFor(x => x.Username)
                .MinimumLength(2)
                .WithMessage("Username must be at least 2 characters long");
        });

        When(x => !string.IsNullOrEmpty(x.FirstName), () =>
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(50)
                .WithMessage("First name cannot exceed 50 characters");
        });

        When(x => !string.IsNullOrEmpty(x.LastName), () =>
        {
            RuleFor(x => x.LastName)
                .MaximumLength(50)
                .WithMessage("Last name cannot exceed 50 characters");
        });
    }
}
