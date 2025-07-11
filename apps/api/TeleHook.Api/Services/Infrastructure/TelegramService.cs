using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TeleHook.Api.Models;
using TeleHook.Api.Models.Results;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Services.Infrastructure;

public class TelegramService : ITelegramService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(HttpClient httpClient, ILogger<TelegramService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TelegramResult> SendMessageAsync(Webhook webhook, string messageText)
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["WebhookId"] = webhook.Id,
            ["WebhookName"] = webhook.Name,
            ["ChatId"] = webhook.Bot.ChatId,
            ["ParseMode"] = webhook.ParseMode,
            ["MessageLength"] = messageText.Length
        });

        _logger.LogDebug("Sending message to Telegram - webhook '{WebhookUUID}', chat '{ChatId}', length: {MessageLength}",
            webhook.Uuid, webhook.Bot.ChatId, messageText.Length);

        var result = await SendMessageAsync(
            webhook.Bot.BotToken,
            webhook.Bot.ChatId,
            messageText,
            webhook.ParseMode,
            webhook.DisableWebPagePreview,
            webhook.DisableNotification,
            webhook.TopicId);

        if (!result.IsSuccess)
        {
            _logger.LogError("Telegram API error for webhook '{WebhookUUID}': {ErrorMessage}", 
                webhook.Uuid, result.Error);
        }

        return result;
    }

    public async Task<TelegramResult> SendMessageAsync(string botToken, string chatId, string messageText, string? parseMode = null, bool? disableWebPagePreview = null, bool? disableNotification = null, string? topicId = null)
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ChatId"] = chatId,
            ["ParseMode"] = parseMode ?? "None",
            ["MessageLength"] = messageText.Length
        });

        _logger.LogDebug("Sending message to Telegram - chat '{ChatId}', length: {MessageLength}",
            chatId, messageText.Length);

        try
        {
            var telegramPayload = new
            {
                chat_id = chatId,
                text = messageText,
                parse_mode = parseMode,
                disable_web_page_preview = disableWebPagePreview,
                disable_notification = disableNotification,
                message_thread_id = string.IsNullOrEmpty(topicId) ? (object?)null : topicId
            };

            var jsonPayload = JsonSerializer.Serialize(telegramPayload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var url = $"https://api.telegram.org/bot{botToken}/sendMessage";

            _logger.LogDebug("HTTP POST to Telegram API for chat '{ChatId}'", chatId);

            var response = await _httpClient.PostAsync(url, content);

            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Message sent successfully to Telegram chat '{ChatId}'", chatId);
                return TelegramResult.Success(body);
            }


            _logger.LogError(
                "Telegram API error for chat '{ChatId}'. Status: {StatusCode}, Error: {Error}",
                chatId, response.StatusCode, body);
            
            return TelegramResult.Failure(
                $"Telegram API returned error: {body}", 
                (int)response.StatusCode, 
                body);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while sending Telegram message to chat '{ChatId}'", chatId);
            return TelegramResult.NetworkError("HTTP request failed while sending Telegram message");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout while sending Telegram message to chat '{ChatId}'", chatId);
            return TelegramResult.Timeout("Request timeout while sending Telegram message");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception while sending Telegram message to chat '{ChatId}'", chatId);
            return TelegramResult.NetworkError("Unexpected error while sending Telegram message");
        }
    }

    public async Task<TelegramResult> TestConnectionAsync(Bot bot)
    {
        var tokenSuffix = bot.BotToken.Length >= 4
            ? bot.BotToken.Substring(bot.BotToken.Length - 4)
            : "****";

        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ConfigId"] = bot.Id,
            ["ChatId"] = bot.ChatId,
            ["TokenSuffix"] = tokenSuffix
        });

        _logger.LogDebug("Testing Telegram bot connection for bot '{BotId}'", bot.Id);

        var url = $"https://api.telegram.org/bot{bot.BotToken}/getMe";
        
        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Telegram bot connection test failed for bot {BotId}. Status: {StatusCode}, Error: {Error}",
                    bot.Id, response.StatusCode, errorContent);
                
                return TelegramResult.Failure(
                    $"Telegram bot connection test failed: {errorContent}", 
                    (int)response.StatusCode, 
                    errorContent);
            }

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(body);

            if (!result.TryGetProperty("ok", out var okProp) || !okProp.GetBoolean())
            {
                _logger.LogError("Telegram API returned ok=false for config '{BotId}'", bot.Id);
                return TelegramResult.Failure("Telegram API returned ok=false", 400);
            }

            var botUsername = "unknown";
            if (result.TryGetProperty("result", out var resultProp) &&
                resultProp.TryGetProperty("username", out var usernameProp))
                botUsername = usernameProp.GetString() ?? "unknown";

            _logger.LogInformation(
                "Bot connection test successful for bot '{ConfigId}'. Bot: @{BotUsername}",
                bot.Id, botUsername);

            return TelegramResult.Success(body);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during Telegram bot connection test for bot '{BotId}'",
                bot.Id);
            return TelegramResult.NetworkError("HTTP request failed during Telegram bot connection test");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout during Telegram bot connection test for bot '{BotId}'",
                bot.Id);
            return TelegramResult.Timeout("Request timeout during Telegram bot connection test");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception during Telegram bot connection test for bot '{BotId}'",
                bot.Id);
            return TelegramResult.NetworkError("Unexpected error during Telegram bot connection test");
        }
    }
}