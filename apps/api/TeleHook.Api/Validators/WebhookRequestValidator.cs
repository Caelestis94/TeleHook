using FluentValidation;
using TeleHook.Api.DTO;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Validators;

public class WebhookCreateRequestValidator : AbstractValidator<CreateWebhookDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public WebhookCreateRequestValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must be 200 characters or less")
            .MustAsync(BeUniqueNameAsync)
            .WithMessage("A webhook with this name already exists");

        RuleFor(x => x.BotId)
            .GreaterThan(0)
            .WithMessage("BotId must be greater than 0")
            .MustAsync(BotExistsAsync)
            .WithMessage("The specified Bot does not exist");

        RuleFor(x => x.PayloadSample)
            .NotEmpty()
            .WithMessage("PayloadSample is required");
        
        RuleFor(x => x.MessageTemplate)
            .NotEmpty()
            .WithMessage("MessageTemplate is required")
            .MaximumLength(4000)
            .WithMessage("MessageTemplate must be 4000 characters or less")
            .Must(ValidatorHelpers.BeValidScribanTemplate)
            .WithMessage("Template syntax is invalid");

        RuleFor(x => x.ParseMode)
            .NotEmpty()
            .WithMessage("ParseMode is required")
            .Must((request, parseMode) => ValidatorHelpers.BeValidParseMode(parseMode))
            .WithMessage("ParseMode must be one of: Markdown, MarkdownV2, HTML");

        RuleFor(x => x.TopicId)
            .Must((request, topicId) => ValidatorHelpers.BeValidTopicId(topicId))
            .WithMessage("TopicId must be numeric")
            .When(x => !string.IsNullOrEmpty(x.TopicId));
    }

    private async Task<bool> BeUniqueNameAsync(CreateWebhookDto createWebhookRequest, string name, CancellationToken cancellationToken)
    {
        return await ValidatorHelpers.CheckUniqueNameAsync(_unitOfWork.Webhooks, name);
    }

    private async Task<bool> BotExistsAsync(CreateWebhookDto createWebhookRequest, int botId, CancellationToken cancellationToken)
    {
        return await ValidatorHelpers.CheckEntityExistsAsync(_unitOfWork.Bots, botId);
    }
}

public class WebhookUpdateRequestValidator : AbstractValidator<UpdateWebhookDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public WebhookUpdateRequestValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required")
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0")
            .MustAsync(BeAnExistingWebhookAsync)
            .WithMessage("Webhook with this Id does not exist");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must be 200 characters or less")
            .MustAsync(BeUniqueNameForUpdateAsync)
            .WithMessage("A webhook with this name already exists");

        RuleFor(x => x.BotId)
            .GreaterThan(0)
            .WithMessage("BotId must be greater than 0")
            .MustAsync(BotExistsAsync)
            .WithMessage("The specified Bot does not exist");

        RuleFor(x => x.PayloadSample)
            .NotEmpty()
            .WithMessage("PayloadSample is required");

        RuleFor(x => x.MessageTemplate)
            .NotEmpty()
            .WithMessage("MessageTemplate is required")
            .MaximumLength(4000)
            .WithMessage("MessageTemplate must be 4000 characters or less")
            .Must(ValidatorHelpers.BeValidScribanTemplate)
            .WithMessage("Template syntax is invalid");

        RuleFor(x => x.ParseMode)
            .NotEmpty()
            .WithMessage("ParseMode is required")
            .Must((request, parseMode) => ValidatorHelpers.BeValidParseMode(parseMode))
            .WithMessage("ParseMode must be one of: Markdown, MarkdownV2, HTML");

        RuleFor(x => x.TopicId)
            .Must((request, topicId) => ValidatorHelpers.BeValidTopicId(topicId))
            .WithMessage("TopicId must be numeric")
            .When(x => !string.IsNullOrEmpty(x.TopicId));
    }

    private async Task<bool> BeAnExistingWebhookAsync(int id, CancellationToken cancellationToken)
    {
        return await ValidatorHelpers.CheckEntityExistsAsync(_unitOfWork.Webhooks, id);
    }
    
    private async Task<bool> BeUniqueNameForUpdateAsync(UpdateWebhookDto updateWebhookRequest, string name, CancellationToken cancellationToken)
    {
        return await ValidatorHelpers.CheckUniqueNameForUpdateAsync(_unitOfWork.Webhooks, name, updateWebhookRequest.Id);
    }

    private async Task<bool> BotExistsAsync(UpdateWebhookDto updateWebhookRequest, int botId, CancellationToken cancellationToken)
    {
        return await ValidatorHelpers.CheckEntityExistsAsync(_unitOfWork.Bots, botId);
    }

}