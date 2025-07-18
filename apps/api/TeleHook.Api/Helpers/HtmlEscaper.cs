using System.Text;
using System.Text.RegularExpressions;

namespace TeleHook.Api.Helpers;

/// <summary>
/// A class to escape special characters in a string formatted with HTML for Telegram.
/// </summary>
public class HtmlEscaper
{
    // Patterns for HTML entities based on Telegram Bot API documentation
    private static readonly Dictionary<string, Regex> Patterns = new()
    {
        // Bold formatting
        { "bold", new Regex(@"<b>(.*?)</b>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        { "strong", new Regex(@"<strong>(.*?)</strong>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        
        // Italic formatting
        { "italic", new Regex(@"<i>(.*?)</i>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        { "emphasis", new Regex(@"<em>(.*?)</em>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        
        // Underline formatting
        { "underline", new Regex(@"<u>(.*?)</u>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        { "insert", new Regex(@"<ins>(.*?)</ins>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        
        // Strikethrough formatting
        { "strikethrough_s", new Regex(@"<s>(.*?)</s>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        { "strikethrough_strike", new Regex(@"<strike>(.*?)</strike>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        { "strikethrough_del", new Regex(@"<del>(.*?)</del>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        
        // Spoiler formatting
        { "spoiler_span", new Regex(@"<span\s+class=[""']tg-spoiler[""']>(.*?)</span>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        { "spoiler_tg", new Regex(@"<tg-spoiler>(.*?)</tg-spoiler>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        
        // Links
        { "link", new Regex(@"<a\s+href=[""'](.*?)[""']>(.*?)</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        
        // Emoji
        { "emoji", new Regex(@"<tg-emoji\s+emoji-id=[""'](\d+)[""']></tg-emoji>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        
        // Code formatting
        { "pre_code", new Regex(@"<pre><code(?:\s+class=[""']language-(\w+)[""'])?>(.*?)</code></pre>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline) },
        { "pre_simple", new Regex(@"<pre>(.*?)</pre>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline) },
        { "code", new Regex(@"<code>(.*?)</code>", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        
        // Block quotes
        { "blockquote_simple", new Regex(@"<blockquote>(.*?)</blockquote>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline) },
        { "blockquote_expandable", new Regex(@"<blockquote\s+expandable>(.*?)</blockquote>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline) }
    };

    /// <summary>
    /// Escapes special characters in the provided text according to HTML rules.
    /// </summary>
    /// <param name="text">The input string containing HTML formatting.</param>
    /// <returns>The escaped string suitable for Telegram API.</returns>
    public string Escape(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return EscapeOutsideEntities(text);
    }

    /// <summary>
    /// Escapes special characters outside of HTML entities.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <returns>The text with special characters escaped outside entities.</returns>
    private string EscapeOutsideEntities(string text)
    {
        var result = new StringBuilder();
        var currentIndex = 0;

        while (currentIndex < text.Length)
        {
            var nearestMatch = FindNearestMatch(text, currentIndex);

            if (nearestMatch == null)
            {
                // No more matches, escape the rest of the text
                var remainingText = text.Substring(currentIndex);
                result.Append(EscapeSpecialChars(remainingText));
                break;
            }

            // Add text before the match (escaped)
            if (nearestMatch.Value.Index > currentIndex)
            {
                var textBefore = text.Substring(currentIndex, nearestMatch.Value.Index - currentIndex);
                result.Append(EscapeSpecialChars(textBefore));
            }

            // Process the HTML match
            result.Append(ProcessHtmlMatch(nearestMatch.Value.Match, nearestMatch.Value.PatternName));
            currentIndex = nearestMatch.Value.Index + nearestMatch.Value.Match.Length;
        }

        return result.ToString();
    }

    /// <summary>
    /// Finds the nearest HTML pattern match starting from the given index.
    /// </summary>
    /// <param name="text">The text to search in.</param>
    /// <param name="startIndex">The index to start searching from.</param>
    /// <returns>The nearest match information or null if no match found.</returns>
    private (Match Match, string PatternName, int Index)? FindNearestMatch(string text, int startIndex)
    {
        Match nearestMatch = null;
        string nearestPatternName = null;
        int nearestIndex = int.MaxValue;

        foreach (var kvp in Patterns)
        {
            var match = kvp.Value.Match(text, startIndex);
            if (match.Success && match.Index < nearestIndex)
            {
                nearestMatch = match;
                nearestPatternName = kvp.Key;
                nearestIndex = match.Index;
            }
        }

        return nearestMatch != null ? (nearestMatch, nearestPatternName, nearestIndex) : null;
    }

    /// <summary>
    /// Processes an HTML match to escape content while preserving tags.
    /// </summary>
    /// <param name="match">The regex match object.</param>
    /// <param name="patternName">The name of the pattern that matched.</param>
    /// <returns>The processed HTML string.</returns>
    private string ProcessHtmlMatch(Match match, string patternName)
    {
        return patternName switch
        {
            "link" => ProcessLinkMatch(match),
            "emoji" => ProcessEmojiMatch(match),
            "pre_code" => ProcessPreCodeMatch(match),
            "spoiler_span" => ProcessSpoilerSpanMatch(match),
            "blockquote_expandable" => ProcessBlockquoteExpandableMatch(match),
            _ => ProcessStandardMatch(match, patternName)
        };
    }

    private string ProcessLinkMatch(Match match)
    {
        if (match.Groups.Count >= 3)
        {
            var href = match.Groups[1].Value;
            var linkText = match.Groups[2].Value;
            var escapedLinkText = EscapeSpecialChars(linkText);
            return $"<a href=\"{href}\">{escapedLinkText}</a>";
        }
        return match.Value;
    }

    private string ProcessEmojiMatch(Match match)
    {
        if (match.Groups.Count >= 2)
        {
            var emojiId = match.Groups[1].Value;
            return $"<tg-emoji emoji-id=\"{emojiId}\"></tg-emoji>";
        }
        return match.Value;
    }

    private string ProcessPreCodeMatch(Match match)
    {
        if (match.Groups.Count >= 3)
        {
            var language = match.Groups[1].Value;
            var code = match.Groups[2].Value;
            var escapedCode = EscapeSpecialChars(code);

            if (string.IsNullOrEmpty(language))
            {
                return $"<pre><code>{escapedCode}</code></pre>";
            }
            else
            {
                return $"<pre><code class=\"language-{language}\">{escapedCode}</code></pre>";
            }
        }
        return match.Value;
    }

    private string ProcessSpoilerSpanMatch(Match match)
    {
        if (match.Groups.Count >= 2)
        {
            var content = match.Groups[1].Value;
            var escapedContent = EscapeSpecialChars(content);
            return $"<span class=\"tg-spoiler\">{escapedContent}</span>";
        }
        return match.Value;
    }

    private string ProcessBlockquoteExpandableMatch(Match match)
    {
        if (match.Groups.Count >= 2)
        {
            var content = match.Groups[1].Value;
            var escapedContent = EscapeSpecialChars(content);
            return $"<blockquote expandable>{escapedContent}</blockquote>";
        }
        return match.Value;
    }

    private string ProcessStandardMatch(Match match, string patternName)
    {
        if (match.Groups.Count >= 2)
        {
            var content = match.Groups[1].Value;
            var escapedContent = EscapeSpecialChars(content);
            var tagName = GetTagNameFromPattern(patternName);
            return $"<{tagName}>{escapedContent}</{tagName}>";
        }
        return match.Value;
    }

    /// <summary>
    /// Gets the HTML tag name from the pattern name.
    /// </summary>
    /// <param name="patternName">The pattern name.</param>
    /// <returns>The corresponding HTML tag name.</returns>
    private string GetTagNameFromPattern(string patternName)
    {
        return patternName switch
        {
            "bold" => "b",
            "strong" => "strong",
            "italic" => "i",
            "emphasis" => "em",
            "underline" => "u",
            "insert" => "ins",
            "strikethrough_s" => "s",
            "strikethrough_strike" => "strike",
            "strikethrough_del" => "del",
            "spoiler_tg" => "tg-spoiler",
            "code" => "code",
            "pre_simple" => "pre",
            "blockquote_simple" => "blockquote",
            _ => patternName
        };
    }

    /// <summary>
    /// Escapes HTML special characters.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <returns>The escaped string.</returns>
    private string EscapeSpecialChars(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\\n", "\n");
    }

}
