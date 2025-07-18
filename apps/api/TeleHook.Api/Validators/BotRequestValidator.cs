using FluentValidation;
using TeleHook.Api.DTO;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Validators;

public class BotCreateRequestValidator : AbstractValidator<CreateBotDto>
{
    private readonly IBotRepository _botRepository;

    public BotCreateRequestValidator(IBotRepository botRepository)
    {
        _botRepository = botRepository;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must be 100 characters or less")
            .MustAsync(BeUniqueName)
            .WithMessage("A Bot with this name already exists");

        RuleFor(x => x.BotToken)
            .NotEmpty()
            .WithMessage("BotToken is required")
            .Must((request, token) => ValidatorHelpers.BeValidBotToken(token))
            .WithMessage("BotToken format is invalid. Expected format: 123456789:ABCdefGHI...");

        RuleFor(x => x.ChatId)
            .NotEmpty()
            .WithMessage("ChatId is required")
            .Must((request, chatId) => ValidatorHelpers.BeValidChatId(chatId))
            .WithMessage("ChatId format is invalid. Must be a numeric value");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        return await ValidatorHelpers.CheckUniqueNameAsync(_botRepository, name);
    }
}

public class BotUpdateRequestValidator : AbstractValidator<UpdateBotDto>
{
    private readonly IBotRepository _botRepository;

    public BotUpdateRequestValidator(IBotRepository botRepository)
    {
        _botRepository = botRepository;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required")
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0")
            .MustAsync(BeAnExistingConfig)
            .WithMessage("Bot with this Id does not exist");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must be 100 characters or less")
            .MustAsync(BeUniqueNameForUpdate)
            .WithMessage("A Bot with this name already exists");

        RuleFor(x => x.BotToken)
            .NotEmpty()
            .WithMessage("BotToken is required")
            .Must((request, token) => ValidatorHelpers.BeValidBotToken(token))
            .WithMessage("BotToken format is invalid. Expected format: 123456789:ABCdefGHI...");

        RuleFor(x => x.ChatId)
            .NotEmpty()
            .WithMessage("ChatId is required")
            .Must((request, chatId) => ValidatorHelpers.BeValidChatId(chatId))
            .WithMessage("ChatId format is invalid. Must be a numeric value");
    }

    private async Task<bool> BeAnExistingConfig(int id, CancellationToken cancellationToken)
    {
        return await ValidatorHelpers.CheckEntityExistsAsync(_botRepository, id);
    }

    private async Task<bool> BeUniqueNameForUpdate(UpdateBotDto updateBotRequest, string name, CancellationToken cancellationToken)
    {
        return await ValidatorHelpers.CheckUniqueNameForUpdateAsync(_botRepository, name, updateBotRequest.Id);
    }
}
