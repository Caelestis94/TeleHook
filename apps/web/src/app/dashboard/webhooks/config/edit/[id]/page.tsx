"use client";

import { useEffect } from "react";
import { useRouter, useParams } from "next/navigation";
import { toast } from "sonner";
import { useWebhook } from "@/hooks/queries";
import { useUpdateWebhook } from "@/hooks/mutations";
import { PageHeader } from "@/components/layout";
import { WebhookIcon } from "lucide-react";
import { WebhookForm } from "@/app/dashboard/webhooks/config/";
import type { WebhookFormData } from "@/types/webhook";

export default function EditWebhookPage() {
  const router = useRouter();
  const params = useParams();
  const webhookId = parseInt(params.id as string);

  const { data: currentWebhook, error } = useWebhook(webhookId);
  const updateWebhookMutation = useUpdateWebhook();

  useEffect(() => {
    if (error) {
      toast.error("Webhook not found");
      router.push("/dashboard/webhooks");
    }
  }, [error, router]);

  const handleSubmit = (data: WebhookFormData) => {
    if (!currentWebhook) {
      toast.error("Webhook not found");
      return;
    }

    const payload = {
      id: currentWebhook.id,
      name: data.name,
      botId: data.botId,
      topicId: data.topicId || undefined,
      payloadSample: data.payloadSample,
      messageTemplate: data.messageTemplate,
      parseMode: data.parseMode,
      disableWebPagePreview: data.disableWebPagePreview,
      disableNotification: data.disableNotification,
      isDisabled: data.isDisabled,
      isProtected: data.isProtected,
      secretKey: data.isProtected ? data.secretKey : undefined,
    };

    updateWebhookMutation.mutate(
      { id: currentWebhook.id, data: payload },
      {
        onSuccess: () => {
          router.push("/dashboard/webhooks");
        },
        onError: (error: unknown) => {
          window.setWebhookFormErrors?.(error);
        },
      }
    );
  };

  // Prepare initial data for the form
  const initialData = currentWebhook
    ? {
        name: currentWebhook.name,
        botId: currentWebhook.botId,
        topicId: currentWebhook.topicId || "",
        payloadSample: currentWebhook.payloadSample || "{}",
        messageTemplate: currentWebhook.messageTemplate,
        parseMode: currentWebhook.parseMode,
        disableWebPagePreview: currentWebhook.disableWebPagePreview,
        disableNotification: currentWebhook.disableNotification,
        isDisabled: currentWebhook.isDisabled,
        isProtected: currentWebhook.isProtected,
        secretKey: currentWebhook.secretKey || "",
      }
    : undefined;

  return (
    <div className="container mx-auto">
      <PageHeader
        title="Edit Webhook"
        description="Modify the webhook endpoint configuration"
        icon={WebhookIcon}
        iconBgColor="bg-green-100 dark:bg-green-900/20"
        iconColor="text-green-600 dark:text-green-400"
        breadcrumbItems={[
          { label: "Dashboard", href: "/dashboard" },
          { label: "Webhooks", href: "/dashboard/webhooks" },
          { label: "Edit Webhook" },
        ]}
      />
      <div className="mx-auto py-6">
        <WebhookForm
          isEdit={true}
          initialData={initialData}
          isLoading={updateWebhookMutation.isPending}
          onSubmit={handleSubmit}
        />
      </div>
    </div>
  );
}
