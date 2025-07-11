"use client";

import { useFormContext } from "react-hook-form";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import {
  CheckCircle,
  AlertCircle,
  Save,
  Globe,
  Shield,
  ShieldOff,
  NotepadTextDashed,
  VolumeOff,
  Loader2,
} from "lucide-react";
import { useBots } from "@/hooks/queries/useBots";
import { useMediaQuery } from "@/hooks/useMediaQuery";
import type { WebhookFormData } from "@/types/webhook";
import TelegramMarkdownPreview from "@/components/telegram-markdown-preview";
import { cn } from "@/lib/utils";

interface WebhookReviewFormProps {
  isLoading: boolean;
  isEdit?: boolean;
}

export function WebhookReviewForm({
  isLoading,
  isEdit = false,
}: WebhookReviewFormProps) {
  const form = useFormContext<WebhookFormData>();
  const { data: bots = [] } = useBots();
  const isMobile = useMediaQuery("(max-width: 768px)");

  const formData = form.getValues();
  const selectedBot = bots.find((bot) => bot.id === formData.botId);

  const hasValidationErrors =
    !formData.botId ||
    !formData.messageTemplate ||
    (formData.isProtected && !formData.secretKey);

  return (
    <div className="space-y-4 sm:space-y-6">
      {/* Validation Warnings */}
      {hasValidationErrors && (
        <Card className="border-orange-200 bg-orange-50 dark:border-orange-800 dark:bg-orange-950/50">
          <CardHeader className="pb-3">
            <CardTitle className="flex items-center gap-2 text-orange-800 dark:text-orange-200 text-base sm:text-lg">
              <AlertCircle className="w-4 h-4 sm:w-5 sm:h-5" />
              Configuration Incomplete
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ul className="text-xs sm:text-sm text-orange-700 dark:text-orange-300 space-y-1">
              {!formData.botId && <li>• Please select a Telegram bot</li>}
              {!formData.messageTemplate && (
                <li>• Please configure a message template</li>
              )}
              {formData.isProtected && !formData.secretKey && (
                <li>
                  • Secret key is required for protected webhooks. Please
                  generate one.
                </li>
              )}
            </ul>
          </CardContent>
        </Card>
      )}

      {/* Configuration Summary */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="flex items-center gap-2 text-base sm:text-lg">
            <CheckCircle className="h-4 w-4 sm:h-5 sm:w-5 text-green-600 dark:text-green-400" />
            Configuration Summary
          </CardTitle>
          <CardDescription className="text-xs sm:text-sm">
            Review your webhook configuration before{" "}
            {isEdit ? "updating" : "creating"}
          </CardDescription>
          <Separator />
        </CardHeader>
        <CardContent className="space-y-4 sm:space-y-6">
          {/* Basic Information */}
          <div className="space-y-2 sm:space-y-3">
            <h4 className="font-medium text-xs sm:text-sm">
              Basic Information
            </h4>
            <div className="space-y-2 sm:space-y-0 sm:grid sm:grid-cols-2 sm:gap-4">
              <div className="space-y-1">
                <span className="text-xs text-muted-foreground">Name:</span>
                <p className="text-xs sm:text-sm font-medium break-words">
                  {formData.name || "Not specified"}
                </p>
              </div>
              <div className="space-y-1">
                <span className="text-xs text-muted-foreground">Topic ID:</span>
                <p className="text-xs sm:text-sm font-medium">
                  {formData.topicId || "Main chat"}
                </p>
              </div>
            </div>
          </div>

          <Separator className="sm:hidden" />

          {/* Target Configuration */}
          <div className="space-y-2 sm:space-y-3">
            <h4 className="font-medium text-xs sm:text-sm">
              Target Configuration
            </h4>
            <div className="space-y-2">
              <div className="space-y-1">
                <span className="text-xs text-muted-foreground">
                  Telegram Bot:
                </span>
                <p className="text-xs sm:text-sm font-medium break-words">
                  {selectedBot?.name || "Not selected"}
                </p>
                {selectedBot && (
                  <span className="text-xs text-muted-foreground">
                    Chat: {selectedBot.chatId.substring(4)}
                  </span>
                )}
              </div>
            </div>
          </div>

          <Separator className="sm:hidden" />

          {/* Security & Status */}
          <div className="space-y-2 sm:space-y-3">
            <h4 className="font-medium text-xs sm:text-sm">
              Security & Status
            </h4>
            <div className="flex flex-wrap gap-2">
              <Badge
                className={cn(
                  "flex items-center gap-1 text-xs",
                  formData.isProtected
                    ? "bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400"
                    : "bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400"
                )}
                variant="outline"
              >
                {formData.isProtected ? (
                  <Shield className="w-2 h-2 sm:w-3 sm:h-3" />
                ) : (
                  <ShieldOff className="w-2 h-2 sm:w-3 sm:h-3" />
                )}
                {formData.isProtected ? "Protected" : "Unprotected"}
              </Badge>
              <Badge
                className={cn(
                  "flex items-center gap-1 text-xs",
                  formData.isDisabled
                    ? "bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400"
                    : "bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400"
                )}
                variant="outline"
              >
                <Globe className="w-2 h-2 sm:w-3 sm:h-3" />
                {formData.isDisabled ? "Disabled" : "Enabled"}
              </Badge>
            </div>
            {formData.isProtected && formData.secretKey && (
              <p className="text-xs text-muted-foreground">
                Secret key configured for Bearer token authentication
              </p>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Message Template Preview */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="flex items-center gap-2 text-base sm:text-lg">
            <NotepadTextDashed className="h-4 w-4 sm:h-5 sm:w-5 text-green-600 dark:text-green-400" />
            Message Template Preview
          </CardTitle>
          <CardDescription className="text-xs sm:text-sm">
            Preview of how messages will be formatted and sent to Telegram
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3 sm:space-y-4">
          {/* Parse Mode and Options */}
          <div className="space-y-2 sm:space-y-0 sm:flex sm:items-center sm:justify-between">
            <div className="flex items-center gap-2">
              <span className="text-xs text-muted-foreground">Parse Mode:</span>
              <Badge variant="outline" className="text-xs">
                {formData.parseMode}
              </Badge>
            </div>
            <div className="flex flex-wrap gap-1 sm:gap-2">
              {formData.disableWebPagePreview && (
                <Badge variant="secondary" className="text-xs">
                  <Globe className="w-2 h-2 sm:w-3 sm:h-3 mr-1" />
                  No Web Previews
                </Badge>
              )}
              {formData.disableNotification && (
                <Badge variant="secondary" className="text-xs">
                  <VolumeOff className="w-2 h-2 sm:w-3 sm:h-3 mr-1" />
                  Silent
                </Badge>
              )}
            </div>
          </div>

          {/* Template Preview */}
          {formData.messageTemplate ? (
            <div className="p-3 sm:p-4 bg-muted rounded-lg">
              <div className="text-xs sm:text-sm">
                {formData.parseMode === "MarkdownV2" ? (
                  <pre className="whitespace-pre-wrap font-mono break-words">
                    <TelegramMarkdownPreview
                      markdown={formData.messageTemplate}
                    />
                  </pre>
                ) : formData.parseMode === "HTML" ? (
                  <div
                    className="prose prose-sm max-w-none break-words"
                    dangerouslySetInnerHTML={{
                      __html: formData.messageTemplate,
                    }}
                  />
                ) : (
                  <pre className="whitespace-pre-wrap font-mono break-words">
                    {formData.messageTemplate}
                  </pre>
                )}
              </div>
            </div>
          ) : (
            <div className="p-3 sm:p-4 bg-muted rounded-lg text-center text-muted-foreground text-xs sm:text-sm">
              No message template specified
            </div>
          )}

          <p className="text-xs text-muted-foreground">
            Variables like {"{{ variable.name }}"} will be replaced with actual
            data from webhook payloads
          </p>
        </CardContent>
      </Card>

      {/* Submit Button */}
      <Card>
        <CardContent className="p-3 sm:p-6">
          <div className="space-y-3 sm:space-y-0 sm:flex sm:items-center sm:justify-between">
            <div className="space-y-1">
              <p className="text-sm sm:text-base font-medium">
                Ready to submit?
              </p>
              <p className="text-xs sm:text-sm text-muted-foreground">
                Your webhook will be{" "}
                {formData.name ? `saved as "${formData.name}"` : "created"}
              </p>
            </div>

            <Button
              type="submit"
              disabled={
                isLoading ||
                !formData.botId ||
                !formData.messageTemplate ||
                !formData.payloadSample ||
                (formData.isProtected && !formData.secretKey)
              }
              size={isMobile ? "default" : "lg"}
              className={cn(
                "w-full sm:w-auto sm:min-w-32",
                isLoading && "cursor-not-allowed"
              )}
            >
              {isLoading ? (
                <>
                  <Loader2 className="w-3 h-3 sm:w-4 sm:h-4 mr-2 animate-spin" />
                  Processing...
                </>
              ) : (
                <>
                  <Save className="w-3 h-3 sm:w-4 sm:h-4 mr-2" />
                  {isEdit ? "Update Webhook" : "Create Webhook"}
                </>
              )}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
