"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { toast } from "sonner";
import TelegramMarkdownPreview from "@/components/telegram-markdown-preview";
import { useRenderTemplate } from "@/hooks/mutations";

interface TemplatePreviewProps {
  template: string;
  parseMode?: "MarkdownV2" | "Markdown" | "HTML";
  sampleData: Record<string, unknown>;
}

export function WebhookTemplatePreview({
  template,
  sampleData,
  parseMode = "MarkdownV2",
}: TemplatePreviewProps) {
  const [renderedOutput, setRenderedOutput] = useState<string>("");
  const [warnings, setWarnings] = useState<string[]>([]);
  
  const {
    mutate: renderTemplate,
    isPending: isLoading,
    error,
  } = useRenderTemplate();

  const handleRender = () => {
    setWarnings([]);
    
    renderTemplate(
      { template, sampleData },
      {
        onSuccess: (result) => {
          setRenderedOutput(result);
        },
        onError: () => {
          toast.error(
            "Failed to render template, please check your syntax and data."
          );
        },
      }
    );
  };

  return (
    <div className="space-y-4">
      <Button
        onClick={handleRender}
        type="button"
        disabled={isLoading || !template.trim()}
        className="w-full"
      >
        {isLoading ? "Rendering..." : "Render Template"}
      </Button>

      {error && (
        <Alert variant="destructive">
          <AlertDescription>
            {error instanceof Error ? error.message : "Failed to render template"}
          </AlertDescription>
        </Alert>
      )}

      {warnings.length > 0 && (
        <Alert>
          <AlertDescription>
            <strong>Warnings:</strong>
            <ul className="mt-1 ml-4 list-disc">
              {warnings.map((warning, index) => (
                <li key={index}>{warning}</li>
              ))}
            </ul>
          </AlertDescription>
        </Alert>
      )}

      <div className="border rounded-md p-4 bg-muted/50 min-h-64">
        {renderedOutput ? (
          <div className="text-sm">
            {parseMode === "MarkdownV2" ? (
              <TelegramMarkdownPreview markdown={renderedOutput} />
            ) : parseMode === "HTML" ? (
              <div
                className="prose prose-sm max-w-none"
                dangerouslySetInnerHTML={{ __html: renderedOutput }}
              />
            ) : (
              <pre className="whitespace-pre-wrap font-mono">
                {renderedOutput}
              </pre>
            )}
          </div>
        ) : (
          <p className="text-sm text-muted-foreground">
            Click &quot;Render Template&quot; to see the output
          </p>
        )}
      </div>
    </div>
  );
}
