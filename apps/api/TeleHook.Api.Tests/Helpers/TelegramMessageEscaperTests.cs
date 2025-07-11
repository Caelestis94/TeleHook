using TeleHook.Api.Helpers;
using TeleHook.Api.Services;
using TeleHook.Api.Services.Utilities;
using Xunit;

namespace TeleHook.Api.Tests.Helpers;

public class TelegramMessageEscaperTests
{
    private readonly TelegramMessageEscaper _escaper;
    private readonly MarkdownV2Escaper _markdownV2Escaper;
    private readonly MarkdownEscaper _markdownEscaper;
    private readonly HtmlEscaper _htmlEscaper;

    public TelegramMessageEscaperTests()
    {
        _markdownEscaper = new MarkdownEscaper();
        _htmlEscaper = new HtmlEscaper();
        _markdownV2Escaper = new MarkdownV2Escaper();
    }

    [Theory]
    [InlineData("Simple text", "Simple text")]
    [InlineData("Text with *bold*", "Text with *bold*")]
    [InlineData("Text with _italic_", "Text with _italic_")]
    [InlineData("Text with `code`", "Text with `code`")]
    [InlineData("Text with [link](url)", "Text with [link](url)")]
    [InlineData("Text with ~strikethrough~", "Text with ~strikethrough~")]
    [InlineData("Text with ||spoiler||", "Text with ||spoiler||")]
    [InlineData("Text with #hashtag", "Text with \\#hashtag")]
    [InlineData("Text with +plus and -minus", "Text with \\+plus and \\-minus")]
    [InlineData("Text with = equals", "Text with \\= equals")]
    [InlineData("Text with {braces}", "Text with \\{braces\\}")]
    [InlineData("Text with (parentheses)", "Text with \\(parentheses\\)")]
    [InlineData("Text with . and !", "Text with \\. and \\!")]
    public void EscapeMarkdownV2_ShouldEscapeSpecialCharacters(string input, string expected)
    {
        // Act
        var result = _markdownV2Escaper.Escape(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Simple text", "Simple text")]
    [InlineData("Text with <tag>", "Text with &lt;tag&gt;")]
    [InlineData("Text with & ampersand", "Text with &amp; ampersand")]
    [InlineData("Text with \"quotes\"", "Text with \"quotes\"")] // HTML escaper doesn't escape quotes
    [InlineData("Text with 'apostrophe'", "Text with 'apostrophe'")] // HTML escaper doesn't escape apostrophes  
    [InlineData("<script>alert('xss')</script>", "&lt;script&gt;alert('xss')&lt;/script&gt;")]
    public void EscapeHtml_ShouldEscapeHtmlCharacters(string input, string expected)
    {
        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EscapeMarkdownV2_WithComplexMessage_ShouldEscapeAllSpecialCharacters()
    {
        // Arrange
        var input = "Push to repo: [main] *Important* fix for (critical) bug! See: https://example.com/issue#123";

        // Act
        var result = _markdownV2Escaper.Escape(input);

        // Assert
        Assert.Contains("\\[main\\]", result);
        Assert.Contains("*Important*", result);
        Assert.Contains("\\(critical\\)", result);
        Assert.Contains("\\!", result);
        Assert.Contains("\\#", result);
    }

    [Fact]
    public void EscapeHtml_WithComplexMessage_ShouldEscapeAllHtmlCharacters()
    {
        // Arrange
        var input = "<b>Alert:</b> User 'admin' & 'guest' accessed \"private\" data";

        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Contains("&amp;", result);
        // Note: This implementation doesn't escape quotes and apostrophes
        Assert.Contains("'admin'", result);
        Assert.Contains("'guest'", result);
        Assert.Contains("\"private\"", result);
    }
    
    [Fact]
    public void EscapeHtml_WithPreCodeBlock_ShouldEscapeHtmlCharacters()
    {
        // Arrange
        var input = """
                    "<pre><code>var < x = 10;</code></pre>
                    """;

        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal("\"<pre><code>var &lt; x = 10;</code></pre>", result);
    }
    [Fact]
    public void EscapeHtml_WithSpoilerSpan_ShouldEscapeHtmlCharacters()
    {
        // Arrange
        var input = "<span class=\"tg-spoiler\">Spoiler < > text</span>";

        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal("<span class=\"tg-spoiler\">Spoiler &lt; &gt; text</span>", result);
    }

    [Fact]
    public void EscapeMarkdownV2_WithEmptyString_ShouldReturnEmpty()
    {
        // Act
        var result = _markdownV2Escaper.Escape("");

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void EscapeHtml_WithEmptyString_ShouldReturnEmpty()
    {
        // Act
        var result = _htmlEscaper.Escape("");

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void EscapeMarkdownV2_WithNull_ShouldReturnNull()
    {
        // Act
        var result = _markdownV2Escaper.Escape(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void EscapeHtml_WithNull_ShouldReturnNull()
    {
        // Act
        var result = _htmlEscaper.Escape(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void EscapeMarkdownV2_WithUnicodeCharacters_ShouldPreserveUnicode()
    {
        // Arrange
        var input = "Unicode: 🚀 emoji and ñoño characters";

        // Act
        var result = _markdownV2Escaper.Escape(input);

        // Assert
        Assert.Contains("🚀", result);
        Assert.Contains("ñoño", result);
    }

    [Fact]
    public void EscapeHtml_WithUnicodeCharacters_ShouldPreserveUnicode()
    {
        // Arrange
        var input = "Unicode: 🚀 emoji and ñoño characters";

        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Contains("🚀", result);
        Assert.Contains("ñoño", result);
    }

    #region HTML Escaper Special Cases Tests

    [Theory]
    [InlineData("<b>Bold & special < > chars</b>", "<b>Bold &amp; special &lt; &gt; chars</b>")]
    [InlineData("<i>Italic with <script></i>", "<i>Italic with &lt;script&gt;</i>")]
    [InlineData("<strong>Strong & bold</strong>", "<strong>Strong &amp; bold</strong>")]
    [InlineData("<em>Emphasis with < > &</em>", "<em>Emphasis with &lt; &gt; &amp;</em>")]
    [InlineData("<u>Underline & special</u>", "<u>Underline &amp; special</u>")]
    [InlineData("<ins>Insert with < ></ins>", "<ins>Insert with &lt; &gt;</ins>")]
    [InlineData("<s>Strike & < ></s>", "<s>Strike &amp; &lt; &gt;</s>")]
    [InlineData("<strike>Strike with &</strike>", "<strike>Strike with &amp;</strike>")]
    [InlineData("<del>Delete & < ></del>", "<del>Delete &amp; &lt; &gt;</del>")]
    public void EscapeHtml_WithFormattingTags_ShouldEscapeContentButPreserveTags(string input, string expected)
    {
        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("<a href=\"https://example.com\">Link & text < ></a>", "<a href=\"https://example.com\">Link &amp; text &lt; &gt;</a>")]
    [InlineData("<a href='https://test.com'>Single quotes & < ></a>", "<a href=\"https://test.com\">Single quotes &amp; &lt; &gt;</a>")]
    public void EscapeHtml_WithLinks_ShouldEscapeTextButPreserveHref(string input, string expected)
    {
        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("<tg-emoji emoji-id=\"123\"></tg-emoji>", "<tg-emoji emoji-id=\"123\"></tg-emoji>")]
    [InlineData("<tg-emoji emoji-id='456'></tg-emoji>", "<tg-emoji emoji-id=\"456\"></tg-emoji>")]
    public void EscapeHtml_WithTelegramEmoji_ShouldPreserveEmoji(string input, string expected)
    {
        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("<pre><code>var x = 10 & 20; if (x < 30) {}</code></pre>", "<pre><code>var x = 10 &amp; 20; if (x &lt; 30) {}</code></pre>")]
    [InlineData("<pre><code class=\"language-javascript\">console.log(\"Hello & < >\")</code></pre>", "<pre><code class=\"language-javascript\">console.log(\"Hello &amp; &lt; &gt;\")</code></pre>")]
    [InlineData("<pre>Simple pre with & < ></pre>", "<pre>Simple pre with &amp; &lt; &gt;</pre>")]
    [InlineData("<code>Inline code & < ></code>", "<code>Inline code &amp; &lt; &gt;</code>")]
    public void EscapeHtml_WithCodeBlocks_ShouldEscapeCodeContent(string input, string expected)
    {
        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("<tg-spoiler>Hidden & secret < ></tg-spoiler>", "<tg-spoiler>Hidden &amp; secret &lt; &gt;</tg-spoiler>")]
    [InlineData("<span class=\"tg-spoiler\">Spoiler & < > text</span>", "<span class=\"tg-spoiler\">Spoiler &amp; &lt; &gt; text</span>")]
    [InlineData("<span class='tg-spoiler'>Single quote spoiler & < ></span>", "<span class=\"tg-spoiler\">Single quote spoiler &amp; &lt; &gt;</span>")]
    public void EscapeHtml_WithSpoilers_ShouldEscapeSpoilerContent(string input, string expected)
    {
        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("<blockquote>Quote with & < ></blockquote>", "<blockquote>Quote with &amp; &lt; &gt;</blockquote>")]
    [InlineData("<blockquote expandable>Expandable & < > quote</blockquote>", "<blockquote expandable>Expandable &amp; &lt; &gt; quote</blockquote>")]
    public void EscapeHtml_WithBlockquotes_ShouldEscapeQuoteContent(string input, string expected)
    {
        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EscapeHtml_WithNestedTags_ShouldEscapeInnerTagsAsText()
    {
        // Arrange
        var input = "<b>Bold with <i>nested & < > italic</i></b>";

        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        // The escaper processes the outer <b> tag first, so inner <i> tags get escaped as text
        Assert.Equal("<b>Bold with &lt;i&gt;nested &amp; &lt; &gt; italic&lt;/i&gt;</b>", result);
    }

    [Fact]
    public void EscapeHtml_WithMixedContentAndTags_ShouldEscapeCorrectly()
    {
        // Arrange
        var input = "Plain text & < > with <b>bold & special</b> and more & text";

        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal("Plain text &amp; &lt; &gt; with <b>bold &amp; special</b> and more &amp; text", result);
    }

    [Theory]
    [InlineData("<B>UPPERCASE & < ></B>", "<b>UPPERCASE &amp; &lt; &gt;</b>")]
    [InlineData("<I>ITALIC & < ></I>", "<i>ITALIC &amp; &lt; &gt;</i>")]
    [InlineData("<CODE>CODE & < ></CODE>", "<code>CODE &amp; &lt; &gt;</code>")]
    public void EscapeHtml_WithUppercaseTags_ShouldNormalizeToLowercase(string input, string expected)
    {
        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EscapeHtml_WithMultilineContent_ShouldHandleNewlines()
    {
        // Arrange
        var input = "<pre><code>line1 & < >\nline2 & < >\nline3</code></pre>";

        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal("<pre><code>line1 &amp; &lt; &gt;\nline2 &amp; &lt; &gt;\nline3</code></pre>", result);
    }

    [Fact]
    public void EscapeHtml_WithComplexNestedStructure_ShouldEscapeCorrectly()
    {
        // Arrange
        var input = """
                    <blockquote>
                        <b>Alert & Warning</b>
                        <pre><code>if (x < 10 & y > 5) { alert("test"); }</code></pre>
                        <a href="https://example.com">Link & text < ></a>
                    </blockquote>
                    """;

        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Contains("Alert &amp; Warning", result);
        Assert.Contains("x &lt; 10 &amp; y &gt; 5", result);
        Assert.Contains("Link &amp; text &lt; &gt;", result);
        Assert.Contains("<blockquote>", result);
        Assert.Contains("</blockquote>", result);
    }

    [Theory]
    [InlineData("Text with \\n literal", "Text with \n literal")]
    [InlineData("Multiple\\nlines\\nhere", "Multiple\nlines\nhere")]
    public void EscapeHtml_WithLiteralNewlines_ShouldConvertToActualNewlines(string input, string expected)
    {
        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EscapeHtml_WithMalformedTags_ShouldEscapeAsPlainText()
    {
        // Arrange
        var input = "<b>Unclosed bold & < > tag";

        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal("&lt;b&gt;Unclosed bold &amp; &lt; &gt; tag", result);
    }

    [Fact]
    public void EscapeHtml_WithInvalidTagStructure_ShouldEscapeAsPlainText()
    {
        // Arrange
        var input = "<invalid>Invalid tag & < ></invalid>";

        // Act
        var result = _htmlEscaper.Escape(input);

        // Assert
        Assert.Equal("&lt;invalid&gt;Invalid tag &amp; &lt; &gt;&lt;/invalid&gt;", result);
    }

    #endregion
}
