export default function telegramMdToHtml(text: string): string {
  if (!text) return "";

  return (
    text
      // Escape HTML first to prevent interference with markdown processing
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      // Code blocks (must be processed before inline code)
      .replace(/```([\s\S]*?)```/g, "<pre><code>$1</code></pre>")
      // Inline code
      .replace(/`([^`\r\n]+)`/g, "<code>$1</code>")
      // Spoiler text
      .replace(/\|\|([^|\r\n]+)\|\|/g, '<span class="spoiler">$1</span>')
      // Bold
      .replace(/\*([^*\r\n]+)\*/g, "<strong>$1</strong>")
      // Italic
      .replace(/(?<!\w)_([^_\r\n]+)_(?!\w)/g, "<em>$1</em>")
      // Underline
      .replace(/__([^_\r\n]+)__/g, "<u>$1</u>")
      // Strikethrough
      .replace(/~([^~\r\n]+)~/g, "<s>$1</s>")
      // Inline links (no need for URL validation since DOMPurify will handle it)
      .replace(
        /\[([^\]]+)\]\(([^)\s]+)\)/g,
        '<a href="$2" target="_blank" rel="noopener noreferrer">$1</a>'
      )
      // Blockquotes
      .replace(/^> (.+)$/gm, "<blockquote>$1</blockquote>")
      // Convert newlines to breaks
      .replace(/\r?\n/g, "<br>")
  );
}
