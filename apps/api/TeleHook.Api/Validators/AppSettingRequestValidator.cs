using FluentValidation;
using TeleHook.Api.DTO;

namespace TeleHook.Api.Validators;

public class AppSettingRequestValidator : AbstractValidator<AppSettingDto>
{
    public AppSettingRequestValidator()
    {
        RuleFor(x => x.LogLevel)
            .NotEmpty()
            .WithMessage("LogLevel is required")
            .Must(BeValidLogLevel)
            .WithMessage("LogLevel must be one of: Trace, Debug, Information, Warning, Error, Critical, None");

        RuleFor(x => x.LogRetentionDays)
            .GreaterThanOrEqualTo(1)
            .WithMessage("LogRetentionDays must be at least 1 day")
            .LessThanOrEqualTo(365)
            .WithMessage("LogRetentionDays cannot exceed 365 days");

        RuleFor(x => x.WebhookLogRetentionDays)
            .GreaterThanOrEqualTo(0)
            .WithMessage("WebhookLogRetentionDays must be 0 or greater (0 = keep forever)")
            .LessThanOrEqualTo(365)
            .WithMessage("WebhookLogRetentionDays cannot exceed 365 days");

        RuleFor(x => x.StatsDaysInterval)
            .GreaterThanOrEqualTo(1)
            .WithMessage("StatsDaysInterval must be at least 1 day")
            .LessThanOrEqualTo(365)
            .WithMessage("StatsDaysInterval cannot exceed 365 days");
        RuleFor(x => x.NotificationBotToken)
            .NotEmpty()
            .WithMessage("Notification bot token is required when failure notifications are enabled")
            .When(x => x.EnableFailureNotifications);

        RuleFor(x => x.NotificationChatId)
            .NotEmpty()
            .WithMessage("Notification chat ID is required when failure notifications are enabled")
            .When(x => x.EnableFailureNotifications);

        RuleFor(x => x.NotificationBotToken)
            .Must(ValidatorHelpers.BeValidBotToken)
            .WithMessage("Notification bot token must be a valid Telegram bot token format")
            .When(x => x.EnableFailureNotifications);

        RuleFor(x => x.NotificationChatId)
            .Must(ValidatorHelpers.BeValidChatId)
            .WithMessage("Notification chat ID must be a valid chat ID format")
            .When(x => x.EnableFailureNotifications);

        RuleFor(x => x.NotificationTopicId)
            .Must(ValidatorHelpers.BeValidTopicId)
            .WithMessage("Notification topic ID must be a valid topic ID format")
            .When(x => x.EnableFailureNotifications && !string.IsNullOrEmpty(x.NotificationTopicId));
    }

    private static bool BeValidLogLevel(string logLevel)
    {
        var validLogLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical", "None" };
        return validLogLevels.Contains(logLevel);
    }
}