"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";
import { Webhook as WebhookIcon, Info } from "lucide-react";
import { useRouter } from "next/navigation";
import { PageHeader } from "@/components/layout";
import type { Webhook } from "@/types/webhook";
import { WebhooksTable } from "@/app/dashboard/webhooks";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ConfirmationDialog } from "@/components/confirmation-dialog";
import { useWebhooks, useBots } from "@/hooks/queries";
import { useDeleteWebhook, useTestWebhook } from "@/hooks/mutations";
import { handleError } from "@/lib/error-handling";

export function WebhooksPageClient() {
  const router = useRouter();
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [showTestDialog, setShowTestDialog] = useState(false);
  const [selectedWebhook, setSelectedWebhook] = useState<Webhook | null>(null);
  const [showClipboardPasteFailedDialog, setShowClipboardPasteFailedDialog] =
    useState(false);
  const [copiedText, setSetCopiedText] = useState("");

  // TanStack Query hooks
  const {
    data: webhooks = [],
    isLoading: webhooksLoading,
    isError,
    error,
    refetch: refetchWebhooks,
  } = useWebhooks();
  const { data: bots = [], isLoading: botsLoading } = useBots();
  const deleteWebhookAction = useDeleteWebhook();
  const testWebhookAction = useTestWebhook();

  useEffect(() => {
    if (isError && error) {
      handleError(error);
    }
  }, [isError, error]);

  const isLoading = webhooksLoading || botsLoading;

  // Delete endpoint
  const handleDelete = async () => {
    if (!selectedWebhook) return;

    await deleteWebhookAction.mutateAsync(selectedWebhook.id);
    setShowDeleteDialog(false);
    setSelectedWebhook(null);
  };

  // Test endpoint
  const handleTest = async () => {
    if (!selectedWebhook) return;

    const payload = selectedWebhook.payloadSample;
    if (!payload) {
      toast.error("No sample payload available for this template");
      return;
    }

    const samplePayload = JSON.parse(payload);

    const headers = {
      ...(selectedWebhook.isProtected
        ? { Authorization: "Bearer " + selectedWebhook.secretKey }
        : {}),
    };

    await testWebhookAction.mutateAsync({
      uuid: selectedWebhook.uuid,
      payload: samplePayload,
      headers,
    });

    setShowTestDialog(false);
  };

  const copyWebhookUrl = (uuid: string) => {
    if (typeof navigator !== "undefined" && navigator.clipboard) {
      const url = `${window.location.origin}/api/trigger/${uuid}`;

      navigator.clipboard.writeText(url);
      toast.success("Webhook URL copied to clipboard");
    } else {
      setSetCopiedText(`${window.location.origin}/api/trigger/${uuid}`);
      setShowClipboardPasteFailedDialog(true);
      toast.error("Clipboard API not available");
    }
  };

  // Simplified refresh - just refetch the queries
  const handleRefresh = () => {
    refetchWebhooks();
  };
  return (
    <>
      <div className="space-y-6">
        {/* Header */}

        <PageHeader
          title="Webhooks"
          description="Configure webhooks that forward notifications to Telegram"
          icon={WebhookIcon}
          iconBgColor="bg-green-100 dark:bg-green-900/20"
          iconColor="text-green-600 dark:text-green-400"
          refreshAction={{
            onClick: handleRefresh,
            disabled: isLoading,
          }}
          breadcrumbItems={[
            { label: "Dashboard", href: "/dashboard" },
            { label: "Webhooks" },
          ]}
          createAction={{
            onClick: () => router.push("/dashboard/webhooks/config/new"),
            disabled: isLoading || bots.length === 0,
            label: "Create Webhook",
          }}
        />
        {/* Info Card */}
        <Card className="border-l-4 border-l-green-500">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Info className="w-5 h-5 text-green-600 dark:text-green-400" />
              <span>Webhook Endpoints</span>
            </CardTitle>
            <hr />
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <h4 className="font-medium mb-2 text-sm flex items-center gap-2">
                  <span className="w-6 h-6 rounded-full bg-green-100 dark:bg-green-900/20 text-green-600 dark:text-green-400 text-xs flex items-center justify-center font-semibold">
                    1
                  </span>
                  Receive Webhooks
                </h4>
                <p className="text-sm text-muted-foreground">
                  Each webhook gets a unique URL endpoint that services can POST
                  to. Supports endpoint security with secret keys.
                </p>
              </div>
              <div>
                <h4 className="font-medium mb-2 text-sm flex items-center gap-2">
                  <span className="w-6 h-6 rounded-full bg-green-100 dark:bg-green-900/20 text-green-600 dark:text-green-400 text-xs flex items-center justify-center font-semibold">
                    2
                  </span>
                  Forward to Telegram
                </h4>
                <p className="text-sm text-muted-foreground">
                  Payloads are sent to your bot and chat. To send to a specific
                  topic, copy the link to any message in that topic; the second
                  number in the URL (
                  <code className="px-1 py-0.5 rounded bg-muted">
                    t.me/c/CHAT_ID/TOPIC_ID/...
                  </code>
                  ) is your Topic ID.
                </p>
              </div>
            </div>

            {/* Prerequisites warning */}
            {!isLoading && bots.length === 0 && (
              <div className="mt-4 p-3 rounded-lg border border-orange-200 dark:border-orange-800 bg-orange-50 dark:bg-orange-700/20">
                <h4 className="font-medium text-orange-900 dark:text-orange-300 mb-1 text-sm">
                  Setup Required:
                </h4>
                <ul className="text-sm text-orange-800 dark:text-orange-200 space-y-1">
                  {bots.length === 0 && (
                    <li>• Configure at least one Telegram bot first</li>
                  )}
                </ul>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Endpoints table */}
        <WebhooksTable
          isLoading={isLoading}
          webhooks={webhooks}
          onDelete={(endpoint) => {
            setSelectedWebhook(endpoint);
            setShowDeleteDialog(true);
          }}
          onTest={(endpoint) => {
            setSelectedWebhook(endpoint);
            setShowTestDialog(true);
          }}
          onCopyUrl={copyWebhookUrl}
        />

        <ConfirmationDialog
          open={showTestDialog}
          onOpenChange={setShowTestDialog}
          onConfirm={handleTest}
          title="Test Webhook"
          confirmText="Understood"
          variant="default"
          isLoading={testWebhookAction.isPending}
        >
          <span className="text-sm text-muted-foreground">
            This will send a test message using the sample payload used to
            configure the webhook.
          </span>
        </ConfirmationDialog>

        {/* Delete Dialog */}
        <ConfirmationDialog
          open={showDeleteDialog}
          onOpenChange={setShowDeleteDialog}
          title="Delete Webhook Endpoint"
          isCancelAvailable={false}
          isLoading={deleteWebhookAction.isPending}
          confirmText="Delete Endpoint"
          variant="destructive"
          onConfirm={handleDelete}
        >
          <span className="space-y-3 block">
            <span className="text-sm text-muted-foreground block">
              You are about to permanently delete the webhook endpoint:
            </span>
            <span className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-3 block">
              <span className="font-semibold text-red-800 dark:text-red-200">
                {selectedWebhook?.name}
              </span>
            </span>
            <span className="text-sm text-muted-foreground block">
              This action cannot be undone. All associated data (logs & stats)
              will be permanently removed.
            </span>
          </span>
        </ConfirmationDialog>

        {/* Clipboard Paste Failed Dialog */}
        <ConfirmationDialog
          open={showClipboardPasteFailedDialog}
          onOpenChange={() => setShowClipboardPasteFailedDialog(false)}
          title="Clipboard Paste Failed"
          isCancelAvailable={false}
          confirmText="OK"
          onConfirm={() => setShowClipboardPasteFailedDialog(false)}
        >
          <span className="text-sm text-muted-foreground">
            The Clipboard API is not available in this environment. Please copy
            the following value manually:
            <span className="bg-muted/50 rounded-lg p-3 mt-2 block font-mono select-all">
              {copiedText}
            </span>
          </span>
        </ConfirmationDialog>
      </div>
    </>
  );
}
