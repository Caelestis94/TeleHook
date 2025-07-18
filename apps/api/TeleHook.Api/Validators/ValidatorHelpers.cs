using System.Text.RegularExpressions;
using Scriban;
using TeleHook.Api.Repositories.Interfaces;

namespace TeleHook.Api.Validators;

public static class ValidatorHelpers
{
    public static bool BeValidParseMode(string parseMode)
    {
        var validParseModes = new[] { "Markdown", "MarkdownV2", "HTML" };
        return validParseModes.Contains(parseMode);
    }

    public static bool BeValidScribanTemplate(string template)
    {
        try
        {
            var parsed = Template.Parse(template);
            return !parsed.HasErrors;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool BeValidBotToken(string botToken)
    {
        return Regex.IsMatch(botToken, @"^\d+:[A-Za-z0-9_-]+$");
    }

    public static bool BeValidChatId(string chatId)
    {
        return Regex.IsMatch(chatId, @"^-?\d+$");
    }

    public static bool BeValidTopicId(string? topicId)
    {
        return string.IsNullOrEmpty(topicId) || Regex.IsMatch(topicId, @"^\d+$");
    }

    public static async Task<bool> CheckUniqueNameAsync<T>(IRepository<T> repository, string name) where T : class
    {
        if (repository is IBotRepository botRepo)
            return !await botRepo.ExistsByNameAsync(name);
        if (repository is IWebhookRepository webhookRepo)
            return !await webhookRepo.ExistsByNameAsync(name);

        return true;
    }

    public static async Task<bool> CheckUniqueNameForUpdateAsync<T>(IRepository<T> repository, string name, int excludeId) where T : class
    {
        if (repository is IBotRepository botRepo)
            return !await botRepo.ExistsByNameExcludingIdAsync(name, excludeId);
        if (repository is IWebhookRepository webhookRepo)
            return !await webhookRepo.ExistsByNameExcludingIdAsync(name, excludeId);

        return true;
    }

    public static async Task<bool> CheckEntityExistsAsync<T>(IRepository<T> repository, int id) where T : class
    {
        return await repository.ExistsAsync(id);
    }
}
