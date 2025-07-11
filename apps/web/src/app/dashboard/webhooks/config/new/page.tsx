"use client";

import { useRouter } from "next/navigation";
import { useCreateWebhook } from "@/hooks/mutations";
import { WebhookIcon } from "lucide-react";
import { PageHeader } from "@/components/layout";
import { WebhookForm } from "@/app/dashboard/webhooks/config/";
import type { WebhookFormData } from "@/types/webhook";

export default function NewWebhookPage() {
  const router = useRouter();
  const createWebhookMutation = useCreateWebhook();

  const handleSubmit = (data: WebhookFormData) => {
    const payload = {
      name: data.name,
      botId: data.botId,
      topicId: data.topicId || undefined,
      messageTemplate: data.messageTemplate,
      parseMode: data.parseMode,
      disableWebPagePreview: data.disableWebPagePreview,
      disableNotification: data.disableNotification,
      isDisabled: data.isDisabled,
      isProtected: data.isProtected,
      secretKey: data.isProtected ? data.secretKey : undefined,
      payloadSample: data.payloadSample,
    };

    createWebhookMutation.mutate(payload, {
      onSuccess: () => {
        router.push("/dashboard/webhooks");
      },
      onError: (error: unknown) => {
        window.setWebhookFormErrors?.(error);
      },
    });
  };

  return (
    <div className="container mx-auto">
      <PageHeader
        title="Create Webhook"
        description="Create a new webhook endpoint configuration"
        icon={WebhookIcon}
        iconBgColor="bg-green-100 dark:bg-green-900/20"
        iconColor="text-green-600 dark:text-green-400"
        breadcrumbItems={[
          { label: "Dashboard", href: "/dashboard" },
          { label: "Webhooks", href: "/dashboard/webhooks" },
          { label: "Create Webhook" },
        ]}
      />
      <div className="mx-auto py-6">
        <WebhookForm
          isEdit={false}
          isLoading={createWebhookMutation.isPending}
          onSubmit={handleSubmit}
        />
      </div>
    </div>
  );
}
