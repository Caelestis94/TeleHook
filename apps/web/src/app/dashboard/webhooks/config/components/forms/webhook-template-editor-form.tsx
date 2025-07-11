"use client";

import { useRef } from "react";
import { useFormContext } from "react-hook-form";
import { useMediaQuery } from "@/hooks/useMediaQuery";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  FormField,
  FormItem,
  FormControl,
  FormMessage,
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Button } from "@/components/ui/button";
import { JsonTreeViewer } from "@/components/json-tree-viewer";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import {
  HelpCircle,
  Wand2,
  Braces,
  Monitor,
  Code2,
  MessageSquare,
} from "lucide-react";
import type { WebhookFormData } from "@/types/webhook";
import {
  WebhookScribanEditor,
  WebhookHelpDialog,
  WebhookTemplatePreview,
  type WebhookScribanEditorRef,
} from "@/app/dashboard/webhooks/config/";

import {
  formatScribanTemplate,
  getAvailableVariables,
  getSampleData,
} from "@/lib/webhook-template-utils";

export function WebhookTemplateEditorForm() {
  const form = useFormContext<WebhookFormData>();
  const editorRef = useRef<WebhookScribanEditorRef>(null);
  const isMobile = useMediaQuery("(max-width: 1024px)");

  const messageTemplate = form.watch("messageTemplate");
  const parseMode = form.watch("parseMode");
  const payloadSample = form.watch("payloadSample");

  const handleTemplateChange = (newTemplate: string) => {
    form.setValue("messageTemplate", newTemplate);
  };

  const handleFormatTemplate = () => {
    const formatted = formatScribanTemplate(messageTemplate);
    form.setValue("messageTemplate", formatted);
  };

  const variables = getAvailableVariables(payloadSample);
  const sampleData = getSampleData(payloadSample);
  const hasValidPayload = payloadSample && payloadSample !== "{}";

  return (
    <div className="space-y-6">
      {/* Template Editor */}
      <div
        className={`grid gap-6 ${
          isMobile ? "grid-cols-1" : "grid-cols-1 lg:grid-cols-3"
        }`}
      >
        {/* Editor Panel */}
        <Card className={isMobile ? "" : "lg:col-span-2"}>
          <CardHeader>
            <div className="flex flex-col gap-2">
              <div>
                <CardTitle className="flex items-center gap-2 text-base sm:text-lg">
                  <Code2 className="h-4 w-4 sm:h-5 sm:w-5 text-green-600 dark:text-green-400" />
                  Message Template
                </CardTitle>
                <CardDescription className="text-sm">
                  Write your Scriban template with syntax highlighting and
                  autocomplete
                </CardDescription>
              </div>
              <div className="flex flex-col sm:flex-row gap-2">
                <TooltipProvider>
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={handleFormatTemplate}
                        disabled={!hasValidPayload}
                        className="w-full sm:w-auto"
                      >
                        <Wand2 className="h-3 w-3 sm:h-4 sm:w-4 mr-2" />
                        Format
                      </Button>
                    </TooltipTrigger>
                    <TooltipContent>
                      <p className="text-xs sm:text-sm">
                        {hasValidPayload
                          ? "Format and properly indent your Scriban template"
                          : "Configure a payload sample first"}
                      </p>
                    </TooltipContent>
                  </Tooltip>
                </TooltipProvider>
                <TooltipProvider>
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <span className="w-full sm:w-auto">
                        <WebhookHelpDialog>
                          <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            className="w-full sm:w-auto bg-blue-50 hover:bg-blue-100 border-blue-200 dark:bg-blue-900/20 dark:hover:bg-blue-900/30 dark:border-blue-800"
                          >
                            <HelpCircle className="h-3 w-3 sm:h-4 sm:w-4 mr-2" />
                            Syntax Help
                          </Button>
                        </WebhookHelpDialog>
                      </span>
                    </TooltipTrigger>
                    <TooltipContent>
                      <p className="text-xs sm:text-sm">
                        Show Scriban syntax help and usage examples
                      </p>
                    </TooltipContent>
                  </Tooltip>
                </TooltipProvider>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <FormField
              control={form.control}
              name="messageTemplate"
              render={({ field }) => (
                <FormItem>
                  <FormControl>
                    <div className="w-full min-w-0 overflow-hidden">
                      {hasValidPayload ? (
                        <WebhookScribanEditor
                          ref={editorRef}
                          value={field.value}
                          onChange={handleTemplateChange}
                          variables={variables}
                          sampleData={sampleData}
                          height={isMobile ? 300 : 400}
                        />
                      ) : (
                        <div
                          className={`border rounded-lg bg-muted/20 flex items-center justify-center ${
                            isMobile ? "h-[300px]" : "h-[400px]"
                          }`}
                        >
                          <div className="text-center flex flex-col items-center gap-2 max-w-md mx-auto p-4 sm:p-6">
                            <Code2 className="h-8 w-8 sm:h-12 sm:w-12 text-muted-foreground/50" />
                            <div className="text-xs sm:text-sm text-muted-foreground">
                              <p className="font-medium mb-1">
                                Template Editor Disabled
                              </p>
                              <p>
                                Please configure a payload sample in the
                                previous step to enable the template editor and
                                see available variables.
                              </p>
                            </div>
                          </div>
                        </div>
                      )}
                    </div>
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </CardContent>
        </Card>

        {/* Variables Panel */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base sm:text-lg">
              <Braces className="h-4 w-4 sm:h-5 sm:w-5 text-green-600 dark:text-green-400" />
              Webhook Payload Structure
            </CardTitle>
            <CardDescription className="text-sm">
              Browse the JSON structure to understand available variables for
              your template
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {payloadSample !== "{}" ? (
              <div
                className={`overflow-auto border rounded-lg p-2 bg-muted/20 ${
                  isMobile ? "h-64" : "h-96"
                }`}
              >
                <JsonTreeViewer
                  data={sampleData}
                  clipboard={true}
                  className="text-xs sm:text-sm"
                />
              </div>
            ) : (
              <div
                className={`text-center py-8 text-muted-foreground text-xs sm:text-sm border rounded-lg bg-muted/20 flex items-center justify-center ${
                  isMobile ? "h-64" : "h-96"
                }`}
              >
                <div className="flex flex-col items-center gap-2 px-4">
                  <Braces className="h-6 w-6 sm:h-8 sm:w-8 text-muted-foreground/50" />
                  <span className="text-center">
                    Please capture/paste/upload a JSON Payload to see the
                    available variables for the template
                  </span>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Preview Panel */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base sm:text-lg">
            <Monitor className="h-4 w-4 sm:h-5 sm:w-5 text-green-600 dark:text-green-400" />
            Template Preview
          </CardTitle>
          <CardDescription className="text-sm">
            See how your template will look when rendered with sample data
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {hasValidPayload && sampleData && messageTemplate ? (
              <WebhookTemplatePreview
                template={messageTemplate}
                parseMode={form.watch("parseMode")}
                sampleData={sampleData}
              />
            ) : (
              <div
                className={`text-center py-8 text-muted-foreground border rounded-lg bg-muted/20 flex items-center justify-center ${
                  isMobile ? "h-48" : "h-64"
                }`}
              >
                <div className="text-center flex flex-col items-center gap-2 px-4">
                  <Monitor className="h-6 w-6 sm:h-8 sm:w-8 text-muted-foreground/50" />
                  <div className="text-xs sm:text-sm">
                    {!hasValidPayload
                      ? "Configure a payload sample and write a template to see the preview"
                      : "Write a template to see the preview"}
                  </div>
                  <div className="text-xs mt-1">
                    The preview will update when you click &quot;Render
                    Template&quot;
                  </div>
                </div>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Message Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base sm:text-lg">
            <MessageSquare className="h-4 w-4 sm:h-5 sm:w-5 text-green-600 dark:text-green-400" />
            Message Settings
          </CardTitle>
          <CardDescription className="text-sm">
            Configure how messages are parsed and delivered
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Parse Mode */}
          <FormField
            control={form.control}
            name="parseMode"
            render={({ field }) => (
              <FormItem>
                <label className="text-sm font-medium">Parse Mode</label>
                <Select
                  onValueChange={field.onChange}
                  defaultValue={field.value}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select parse mode" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value="MarkdownV2">
                      MarkdownV2 (Recommended)
                    </SelectItem>
                    <SelectItem value="Markdown">Markdown (Legacy)</SelectItem>
                    <SelectItem value="HTML">HTML</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
                <div className="text-xs sm:text-sm text-muted-foreground">
                  {parseMode === "MarkdownV2" && (
                    <p>
                      Modern Markdown with support for bold, italic, code, and
                      links. Recommended for new webhooks.
                    </p>
                  )}
                  {parseMode === "Markdown" && (
                    <p>
                      Legacy Markdown support. Use MarkdownV2 for better
                      formatting options.
                    </p>
                  )}
                  {parseMode === "HTML" && (
                    <p>
                      HTML formatting with tags like &lt;b&gt;, &lt;i&gt;,
                      &lt;code&gt;, and &lt;a&gt;.
                    </p>
                  )}
                </div>
              </FormItem>
            )}
          />

          {/* Disable Web Page Preview */}
          <FormField
            control={form.control}
            name="disableWebPagePreview"
            render={({ field }) => (
              <FormItem className="flex flex-col sm:flex-row sm:items-center sm:justify-between rounded-lg border p-3 sm:p-4 gap-3 sm:gap-0">
                <div className="space-y-0.5">
                  <label className="text-sm sm:text-base font-medium">
                    Disable Web Page Preview
                  </label>
                  <FormMessage />
                  <p className="text-xs sm:text-sm text-muted-foreground">
                    Prevent Telegram from showing link previews in messages
                  </p>
                </div>
                <FormControl>
                  <Switch
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    className="self-start sm:self-auto"
                  />
                </FormControl>
              </FormItem>
            )}
          />

          {/* Disable Notification */}
          <FormField
            control={form.control}
            name="disableNotification"
            render={({ field }) => (
              <FormItem className="flex flex-col sm:flex-row sm:items-center sm:justify-between rounded-lg border p-3 sm:p-4 gap-3 sm:gap-0">
                <div className="space-y-0.5">
                  <label className="text-sm sm:text-base font-medium">
                    Silent Delivery
                  </label>
                  <FormMessage />
                  <p className="text-xs sm:text-sm text-muted-foreground">
                    Send messages silently without notification sound
                  </p>
                </div>
                <FormControl>
                  <Switch
                    checked={field.value}
                    onCheckedChange={field.onChange}
                    className="self-start sm:self-auto"
                  />
                </FormControl>
              </FormItem>
            )}
          />
        </CardContent>
      </Card>
    </div>
  );
}
