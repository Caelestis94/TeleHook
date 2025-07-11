"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";
import { HelpCircle, Bot as BotIcon } from "lucide-react";
import type { BotFormData, Bot } from "@/types/bot";
import { BotsTable, BotFormDialog } from "@/app/dashboard/bots";
import { ConfirmationDialog } from "@/components/confirmation-dialog";
import { useBots, useBotWebhooks } from "@/hooks/queries";
import {
  useCreateBot,
  useUpdateBot,
  useDeleteBot,
  useTestBot,
} from "@/hooks/mutations";
import { HelpCard, PageHeader } from "@/components/layout";
import { handleError } from "@/lib/error-handling";

export function BotsPageClient() {
  const createBotMutation = useCreateBot();
  const updateBotMutation = useUpdateBot();
  const deleteBotMutation = useDeleteBot();
  const testBotMutation = useTestBot();

  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [showEditDialog, setShowEditDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [selectedBot, setSelectedBot] = useState<Bot | null>(null);
  const [showWebhooksAssociatedDialog, setShowWebhooksAssociatedDialog] =
    useState(false);
  const testingBot = testBotMutation.isPending
    ? testBotMutation.variables
    : null;

  const {
    data: associatedWebhooks = [],
    refetch: refetchBotWebhooks,
    error: botWebhooksError,
    isError: isBotWebhooksError,
  } = useBotWebhooks(selectedBot?.id || 0);
  const {
    data: bots = [],
    error: botsError,
    isError: isBotsError,
    isLoading: loading,
    refetch: refetchBots,
  } = useBots();
  const [showClipboardPasteFailedDialog, setShowClipboardPasteFailedDialog] =
    useState(false);
  const [copiedText, setSetCopiedText] = useState("");

  const fetchBots = () => refetchBots();

  const isError = isBotsError || isBotWebhooksError;
  const error = botsError || botWebhooksError;

  useEffect(() => {
    if (isError && error) {
      handleError(error);
    }
  }, [isError, error]);

  const handleCreate = async (formData: BotFormData) => {
    await createBotMutation.mutateAsync(formData);
    setShowCreateDialog(false);
  };

  const handleUpdate = async (formData: BotFormData) => {
    if (!selectedBot) return;

    await updateBotMutation.mutateAsync({ id: selectedBot.id, data: formData });
    setShowEditDialog(false);
    setSelectedBot(null);
  };

  const handleDelete = async () => {
    if (!selectedBot) return;

    await refetchBotWebhooks();

    if (associatedWebhooks && associatedWebhooks.length > 0) {
      setShowWebhooksAssociatedDialog(true);
      setShowDeleteDialog(false);
      return;
    }

    await deleteBotMutation.mutateAsync(selectedBot.id);
    setShowDeleteDialog(false);
    setSelectedBot(null);
  };

  const handleBotConnectionTest = async (bot: Bot) => {
    await testBotMutation.mutateAsync(bot.id);
  };

  const copyToClipboard = (text: string) => {
    if (typeof navigator !== "undefined" && navigator.clipboard) {
      navigator.clipboard.writeText(text);
      toast.success("Copied to clipboard");
    } else {
      setShowClipboardPasteFailedDialog(true);
      setSetCopiedText(text);
      toast.error("Clipboard API not available");
    }
  };

  const openEditDialog = (bot: Bot) => {
    setSelectedBot(bot);
    setShowEditDialog(true);
  };

  const openDeleteDialog = (bot: Bot) => {
    setSelectedBot(bot);
    setShowDeleteDialog(true);
  };

  return (
    <>
      <div className="space-y-6">
        {/* Header */}
        <PageHeader
          title="Telegram Bots"
          description="Manage your Telegram bot configurations for webhook delivery"
          icon={BotIcon}
          refreshAction={{
            onClick: fetchBots,
            disabled: loading,
          }}
          createAction={{
            label: "Add Bot",
            onClick: () => setShowCreateDialog(true),
          }}
          breadcrumbItems={[
            { label: "Dashboard", href: "/dashboard" },
            { label: "Bots" },
          ]}
        />

        {/* Help text */}
        <HelpCard
          icon={HelpCircle}
          title="How to set up a Telegram bot"
          steps={[
            {
              title: "Create Bot with BotFather",
              description: `Message <code class="px-1 py-0.5 rounded bg-muted">@BotFather</code> on Telegram, send <code class="px-1 py-0.5 rounded bg-muted">/newbot</code>, and follow the instructions to get your bot token.`,
            },
            {
              title: "Get Chat ID",
              description: `In your private group, right-click any message and select <strong>Copy Message Link</strong>. From the URL (<code class="px-1 py-0.5 rounded bg-muted">t.me/c/12345...</code>), take the first set of numbers and prefix it with <code class="px-1 py-0.5 rounded bg-muted">-100</code> to get the full ID.`,
            },
          ]}
        />

        {/* Bots table */}
        <BotsTable
          bots={bots}
          isLoading={loading}
          onEdit={openEditDialog}
          onDelete={openDeleteDialog}
          onTest={handleBotConnectionTest}
          onCopy={copyToClipboard}
          testingBotId={testingBot}
        />

        {/* Unified Create/Edit Dialog */}
        <BotFormDialog
          key={selectedBot ? `edit-bot-${selectedBot.id}` : "create-bot"}
          open={showCreateDialog || showEditDialog}
          onOpenChange={(open) => {
            if (showCreateDialog) {
              setShowCreateDialog(open);
            } else {
              setShowEditDialog(open);
              if (!open) {
                setSelectedBot(null);
              }
            }
          }}
          title={selectedBot ? "Edit Telegram Bot" : "Add New Telegram Bot"}
          description={
            selectedBot
              ? "Update the instance name, bot token and chat ID"
              : "Configure a new Telegram bot for webhook delivery"
          }
          defaultValues={
            selectedBot
              ? {
                  name: selectedBot.name,
                  botToken: selectedBot.botToken,
                  chatId: selectedBot.chatId,
                }
              : undefined
          }
          onSubmit={selectedBot ? handleUpdate : handleCreate}
          submitLabel={selectedBot ? "Update Bot" : "Create Bot"}
          isSubmitting={
            createBotMutation.isPending || updateBotMutation.isPending
          }
        />

        {/* Delete Dialog */}
        <ConfirmationDialog
          open={showDeleteDialog}
          onOpenChange={setShowDeleteDialog}
          title="Delete Telegram Bot"
          confirmText="Delete Bot"
          variant="destructive"
          onConfirm={handleDelete}
        >
          <span className="space-y-3 block">
            <span className="text-sm text-muted-foreground block">
              You are about to permanently delete the bot:
            </span>
            <span className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-3 block">
              <span className="font-semibold text-red-800 dark:text-red-200">
                {selectedBot?.name}
              </span>
            </span>
            <span className="text-sm text-muted-foreground block">
              This action cannot be undone. All associated data will be
              permanently removed.
            </span>
          </span>
        </ConfirmationDialog>

        {/* Webhooks Associated Dialog */}
        <ConfirmationDialog
          open={showWebhooksAssociatedDialog}
          onOpenChange={setShowWebhooksAssociatedDialog}
          title="Cannot Delete Bot"
          confirmText="Understood"
          variant="default"
          isCancelAvailable={false}
          onConfirm={() => setShowWebhooksAssociatedDialog(false)}
        >
          <span className="space-y-4 block">
            <span className="flex items-start gap-3">
              <span className="flex-1 block">
                <span className="text-sm text-muted-foreground mb-3 block">
                  This bot cannot be deleted because it is currently being used
                  by the following webhooks:
                </span>
                <span className="bg-muted/50 rounded-lg p-3 border block">
                  <span className="space-y-2 block">
                    {associatedWebhooks.map((webhook, index) => (
                      <span
                        key={webhook.id || index}
                        className="flex items-center gap-2 text-sm"
                      >
                        <span className="w-2 h-2 rounded-full bg-blue-500 block" />
                        <span className="font-medium">{webhook.name}</span>
                      </span>
                    ))}
                  </span>
                </span>
                <span className="text-sm text-muted-foreground mt-3 block">
                  Please remove or reassign these webhooks to a different bot
                  before deleting this one.
                </span>
              </span>
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
            <span className="bg-muted/50 rounded-lg p-3 mt-2 select-all font-mono block">
              {copiedText}
            </span>
          </span>
        </ConfirmationDialog>
      </div>
    </>
  );
}
