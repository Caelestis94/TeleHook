"use client"; // if you're using Next.js App Router

import telegramMdToHtml from "@/lib/markdown-parser";
import { useMemo } from "react";
import DOMPurify from "dompurify";

interface Props {
  markdown: string;
}

export default function TelegramMarkdownPreview({ markdown }: Props) {
  const html = useMemo(() => {
    const dirty = telegramMdToHtml(markdown); // markdown-v2 -> HTML
    return DOMPurify.sanitize(dirty);
  }, [markdown]);

  return (
    <div
      className="prose prose-sm max-w-none font-sans telegram-message"
      dangerouslySetInnerHTML={{ __html: html }}
    />
  );
}
