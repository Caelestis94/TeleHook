"use client";

import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Form } from "@/components/ui/form";
import type { WebhookFormData } from "@/types/webhook";
import { WebhookValidationSchema } from "@/validation/webhook-schema";
import { mapApiErrorsToFields } from "@/validation/utils";
import {
  WebhookReviewForm,
  WebhookSettingsForm,
  WebhookTemplateEditorForm,
  WebhookPayloadSampleForm,
} from "@/app/dashboard/webhooks/config/";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useMediaQuery } from "@/hooks/useMediaQuery";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ApiError } from "@/types/api-error";

interface WebhookFormProps {
  isEdit?: boolean;
  initialData?: Partial<WebhookFormData>;
  isLoading?: boolean;
  onSubmit: (data: WebhookFormData) => void;
}

const defaultFormData: WebhookFormData = {
  name: "",
  botId: 0,
  topicId: "",
  messageTemplate: "",
  parseMode: "MarkdownV2",
  disableWebPagePreview: false,
  disableNotification: false,
  isDisabled: false,
  isProtected: false,
  secretKey: "",
  payloadSample: "{}",
};

export function WebhookForm({
  isEdit = false,
  initialData,
  isLoading = false,
  onSubmit,
}: WebhookFormProps) {
  const [activeTab, setActiveTab] = useState("settings");
  const isMobile = useMediaQuery("(max-width: 768px)");

  // Use initial data or defaults
  const formData = initialData
    ? { ...defaultFormData, ...initialData }
    : defaultFormData;

  // React Hook Form setup
  const form = useForm<WebhookFormData>({
    resolver: zodResolver(WebhookValidationSchema),
    defaultValues: formData,
    mode: "onChange",
  });

  // Update form when initial data changes
  useEffect(() => {
    if (initialData) {
      form.reset({ ...defaultFormData, ...initialData });
    }
  }, [initialData, form]);

  const formErrors = form.formState.errors;
  const tabErrors = {
    settings: !!(formErrors.name || formErrors.botId || formErrors.secretKey),
    template: !!(formErrors.messageTemplate || formErrors.parseMode),
    review: false,
  };

  const tabOptions = [
    {
      value: "settings",
      label: "Basic Settings",
      hasError: tabErrors.settings,
    },
    {
      value: "payload",
      label: "Payload Sample",
      hasError: !!formErrors.payloadSample,
    },
    {
      value: "template",
      label: "Message Template",
      hasError: tabErrors.template,
    },
    { value: "review", label: "Final Review", hasError: false },
  ];

  // Handle form submission with error mapping
  const handleSubmit = (data: WebhookFormData) => {
    onSubmit(data);
  };

  // Method to set server errors from outside
  const setServerErrors = (error: ApiError) => {
    if (error && error.details) {
      const fieldErrors = mapApiErrorsToFields(error.details, {
        name: "name",
        botId: ["botid", "bot"],
        topicId: ["topicid", "topic"],
        payloadSample: ["payloadsample", "sample"],
        messageTemplate: ["messagetemplate", "template"],
        parseMode: ["parsemode", "mode"],
        secretKey: ["secretkey", "key"],
      });

      Object.entries(fieldErrors).forEach(([field, messages]) => {
        if (messages && messages.length > 0) {
          form.setError(field as keyof WebhookFormData, {
            type: "server",
            message: messages[0],
          });
        }
      });

      toast.error("Please correct the highlighted fields");
    }
  };

  // Expose setServerErrors method
  useEffect(() => {
    window.setWebhookFormErrors = setServerErrors;
    return () => {
      delete window.setWebhookFormErrors;
    };
  });

  const getCurrentTabIndex = () => {
    return tabOptions.findIndex((tab) => tab.value === activeTab);
  };

  const canGoNext = () => {
    return getCurrentTabIndex() < tabOptions.length - 1;
  };

  const canGoPrevious = () => {
    return getCurrentTabIndex() > 0;
  };

  const goToNextTab = () => {
    const currentIndex = getCurrentTabIndex();
    if (canGoNext()) {
      setActiveTab(tabOptions[currentIndex + 1].value);
    }
  };

  const goToPreviousTab = () => {
    const currentIndex = getCurrentTabIndex();
    if (canGoPrevious()) {
      setActiveTab(tabOptions[currentIndex - 1].value);
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-6">
        <Tabs
          value={activeTab}
          onValueChange={setActiveTab}
          className="space-y-6"
        >
          {/* Mobile: Dropdown */}
          {isMobile ? (
            <Select value={activeTab} onValueChange={setActiveTab}>
              <SelectTrigger className="w-full">
                <SelectValue>
                  {tabOptions.find((tab) => tab.value === activeTab)?.label}
                  {tabOptions.find((tab) => tab.value === activeTab)
                    ?.hasError && (
                    <span className="ml-2 text-destructive">●</span>
                  )}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {tabOptions.map((tab) => (
                  <SelectItem key={tab.value} value={tab.value}>
                    <div className="flex items-center gap-2">
                      {tab.label}
                      {tab.hasError && (
                        <span className="text-destructive">●</span>
                      )}
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          ) : (
            /* Desktop: Regular tabs */
            <TabsList className="grid w-full grid-cols-4">
              <TabsTrigger
                value="settings"
                className={
                  tabErrors.settings
                    ? "border-destructive data-[state=active]:border-destructive"
                    : ""
                }
              >
                Basic Settings
                {tabErrors.settings && (
                  <span className="ml-1 text-destructive">●</span>
                )}
              </TabsTrigger>

              <TabsTrigger
                value="payload"
                className={
                  formErrors.payloadSample
                    ? "border-destructive data-[state=active]:border-destructive"
                    : ""
                }
              >
                Payload Sample
                {formErrors.payloadSample && (
                  <span className="ml-1 text-destructive">●</span>
                )}
              </TabsTrigger>

              <TabsTrigger
                value="template"
                className={
                  tabErrors.template
                    ? "border-destructive data-[state=active]:border-destructive"
                    : ""
                }
              >
                Message Template
                {tabErrors.template && (
                  <span className="ml-1 text-destructive">●</span>
                )}
              </TabsTrigger>

              <TabsTrigger value="review">Final Review</TabsTrigger>
            </TabsList>
          )}

          {/* Tab contents remain the same */}
          <TabsContent value="settings" className="space-y-6">
            <WebhookSettingsForm isEdit={isEdit} />
          </TabsContent>

          <TabsContent value="payload" className="space-y-6">
            <WebhookPayloadSampleForm />
          </TabsContent>

          <TabsContent value="template" className="space-y-6">
            <WebhookTemplateEditorForm />
          </TabsContent>

          <TabsContent value="review" className="space-y-6">
            <WebhookReviewForm isLoading={isLoading} isEdit={isEdit} />
          </TabsContent>
          {/* Mobile Navigation Buttons */}
          <div className="flex justify-between items-center pt-4 border-t">
            <Button
              type="button"
              variant="outline"
              onClick={goToPreviousTab}
              disabled={!canGoPrevious()}
              className="flex items-center gap-2"
            >
              <ChevronLeft className="h-4 w-4" />
              Previous
            </Button>

            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <span>
                {getCurrentTabIndex() + 1} of {tabOptions.length}
              </span>
            </div>

            <Button
              type="button"
              variant="outline"
              onClick={goToNextTab}
              disabled={!canGoNext()}
              className="flex items-center gap-2"
            >
              Next
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </Tabs>
      </form>
    </Form>
  );
}
