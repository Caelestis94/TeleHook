using FluentValidation;
using TeleHook.Api.DTO;

namespace TeleHook.Api.Validators;

public class EmailPasswordSignInRequestValidator : AbstractValidator<EmailPasswordSignInDto>
{
    public EmailPasswordSignInRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");
    }
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MaximumLength(50)
            .WithMessage("Username must be 50 characters or less");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long");

        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .WithMessage("First name must be 50 characters or less");

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .WithMessage("Last name must be 50 characters or less");
    }

}
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MaximumLength(50)
            .WithMessage("Username must be 50 characters or less");

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .WithMessage("First name must be 50 characters or less");

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .WithMessage("Last name must be 50 characters or less");
        
    }
}
