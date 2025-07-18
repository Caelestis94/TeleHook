using System.Text;
using System.Text.RegularExpressions;

namespace TeleHook.Api.Helpers
{
    /// <summary>
    /// A class to escape special characters in a string formatted with Telegram's MarkdownV2 syntax.
    /// </summary>
    public class MarkdownV2Escaper
    {
        // Patterns for different MarkdownV2 entities
        private static readonly Dictionary<string, Regex> Patterns = new()
        {
            { "bold", new Regex(@"\*(.*?)\*", RegexOptions.Compiled) },
            { "italic", new Regex(@"_(.*?)_", RegexOptions.Compiled) },
            { "underline", new Regex(@"__(.*?)__", RegexOptions.Compiled) },
            { "strikethrough", new Regex(@"~(.*?)~", RegexOptions.Compiled) },
            { "spoiler", new Regex(@"\|\|(.*?)\|\|", RegexOptions.Compiled) },
            { "inline_code", new Regex(@"`(.*?)`", RegexOptions.Compiled) },
            { "pre_code", new Regex(@"```(.*?)```", RegexOptions.Compiled | RegexOptions.Singleline) },
            { "link", new Regex(@"\[(.*?)\]\((.*?)\)", RegexOptions.Compiled) }
        };

        private static readonly Regex SpecialCharsRegex = new(@"([*_`\[\]()~>#+\-=|{}.!])", RegexOptions.Compiled);

        /// <summary>
        /// Escapes special characters in the provided text according to MarkdownV2 rules.
        /// </summary>
        /// <param name="text">The input string containing MarkdownV2 formatting.</param>
        /// <returns>The escaped string suitable for Telegram API.</returns>
        public string Escape(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // First, escape all special characters outside of Markdown entities
            var escapedText = EscapeOutsideEntities(text);

            // Now, handle escaping within each Markdown entity
            foreach (var (entity, pattern) in Patterns)
            {
                escapedText = pattern.Replace(escapedText, match => EscapeWithinEntity(match, entity));
            }

            return escapedText;
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
        /// Escapes special characters within a specific Markdown entity.
        /// </summary>
        /// <param name="match">The regex match object.</param>
        /// <param name="entity">The type of Markdown entity.</param>
        /// <returns>The escaped entity string.</returns>
        private string EscapeWithinEntity(Match match, string entity)
        {
            switch (entity)
            {
                case "inline_code":
                case "pre_code":
                    // Inside code entities, escape ` and \
                    var codeContent = match.Groups[1].Value;
                    var escapedCodeContent = Regex.Replace(codeContent, @"([`\\])", @"\$1");
                    return WrapEntity(entity, escapedCodeContent);

                case "link":
                    // Inside URLs, escape ) and \
                    if (match.Groups.Count >= 3)
                    {
                        var displayText = match.Groups[1].Value;
                        var url = match.Groups[2].Value;
                        var escapedUrl = Regex.Replace(url, @"([)\\])", @"\$1");
                        var escapedDisplayText = EscapeSpecialChars(displayText);
                        return $"[{escapedDisplayText}]({escapedUrl})";
                    }
                    else
                    {
                        var content = match.Groups[1].Value;
                        var escapedContent = EscapeSpecialChars(content);
                        return WrapEntity(entity, escapedContent);
                    }

                default:
                    // For other entities, escape all special characters
                    var defaultContent = match.Groups[1].Value;
                    var escapedDefaultContent = EscapeSpecialChars(defaultContent);
                    return WrapEntity(entity, escapedDefaultContent);
            }
        }

        /// <summary>
        /// Wraps the content with the appropriate Markdown syntax based on the entity type.
        /// </summary>
        /// <param name="entity">The type of Markdown entity.</param>
        /// <param name="content">The content to wrap.</param>
        /// <returns>The wrapped content.</returns>
        private string WrapEntity(string entity, string content)
        {
            return entity switch
            {
                "bold" => $"*{content}*",
                "italic" => $"_{content}_",
                "underline" => $"__{content}__",
                "strikethrough" => $"~{content}~",
                "spoiler" => $"||{content}||",
                "inline_code" => $"`{content}`",
                "pre_code" => $"```{content}```",
                "link" => content, // Already handled in EscapeWithinEntity
                _ => content
            };
        }

        /// <summary>
        /// Escapes all special characters in the text with a preceding backslash.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The escaped string.</returns>
        private string EscapeSpecialChars(string text)
        {
            return SpecialCharsRegex.Replace(text, @"\$1");
        }
    }
}
