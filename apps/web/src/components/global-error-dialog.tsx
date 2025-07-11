// src/components/global-error-dialog.tsx
"use client";
import { ConfirmationDialog } from "@/components/confirmation-dialog";
import { useError } from "@/contexts/error-context";

export function GlobalErrorDialog() {
  const { isApiKeyErrorVisible, hideApiKeyError } = useError();

  return (
    <ConfirmationDialog
      open={isApiKeyErrorVisible}
      onOpenChange={hideApiKeyError}
      title="API Configuration Error"
      isCancelAvailable={false}
      confirmText="Understood"
      variant="default"
      onConfirm={hideApiKeyError}
    >
      <span className="space-y-3 block">
        <span className="text-sm text-muted-foreground block">
          There&apos;s an issue with the API key configuration. The backend is
          returning a 401 Unauthorized error.
        </span>
        <span className="badge-error rounded-lg p-3 block">
          <span className="block">
            Please check your <code>.env</code> file and ensure the{" "}
            <code>API_KEY</code> variable is set correctly.
          </span>
        </span>
      </span>
    </ConfirmationDialog>
  );
}
