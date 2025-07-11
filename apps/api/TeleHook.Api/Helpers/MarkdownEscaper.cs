using System.Text;
using System.Text.RegularExpressions;

namespace TeleHook.Api.Helpers;

/// <summary>
/// A class to escape special characters in a string formatted with Telegram's legacy Markdown syntax.
/// </summary>
public class MarkdownEscaper
{
    // Patterns for legacy Markdown entities
    private static readonly Dictionary<string, Regex> Patterns = new()
    {
        { "bold", new Regex(@"\*\*(.*?)\*\*", RegexOptions.Compiled) },
        { "italic_asterisk", new Regex(@"\*(.*?)\*", RegexOptions.Compiled) },
        { "italic_underscore", new Regex(@"_(.*?)_", RegexOptions.Compiled) },
        { "inline_code", new Regex(@"`(.*?)`", RegexOptions.Compiled) },
        { "pre_code", new Regex(@"```(.*?)```", RegexOptions.Compiled | RegexOptions.Singleline) },
        { "link", new Regex(@"\[(.*?)\]\((.*?)\)", RegexOptions.Compiled) }
    };

    private static readonly Regex SpecialCharsRegex = new(@"([_*`\[\]])", RegexOptions.Compiled);

    /// <summary>
    /// Escapes special characters in the provided text according to legacy Markdown rules.
    /// </summary>
    /// <param name="text">The input string containing Markdown formatting.</param>
    /// <returns>The escaped string suitable for Telegram API.</returns>
    public string Escape(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return EscapeOutsideEntities(text);
    }

    /// <summary>
    /// Escapes special characters outside of Markdown entities.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <returns>The text with special characters escaped outside entities.</returns>
    private string EscapeOutsideEntities(string text)
    {
        var combinedPattern = string.Join("|", Patterns.Values.Select(p => p.ToString()));
        var regex = new Regex(combinedPattern, RegexOptions.Singleline);

        var result = new StringBuilder();
        var lastIndex = 0;

        foreach (Match match in regex.Matches(text))
        {
            // Add the text before this match (needs escaping)
            if (match.Index > lastIndex)
            {
                var textBefore = text.Substring(lastIndex, match.Index - lastIndex);
                result.Append(EscapeSpecialChars(textBefore));
            }

            // Add the markdown match (no escaping)
            result.Append(match.Value);
            lastIndex = match.Index + match.Length;
        }

        // Add any remaining text after the last match
        if (lastIndex < text.Length)
        {
            var remainingText = text.Substring(lastIndex);
            result.Append(EscapeSpecialChars(remainingText));
        }

        return result.ToString();
    }

    /// <summary>
    /// Escapes special characters that conflict with Markdown syntax.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <returns>The escaped string.</returns>
    private string EscapeSpecialChars(string text)
    {
        return SpecialCharsRegex.Replace(text, @"\$1");
    }
}