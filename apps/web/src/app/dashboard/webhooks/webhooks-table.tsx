"use client";

import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Globe } from "lucide-react";
import { useRouter } from "next/navigation";
import type { Webhook } from "@/types/webhook";
import { useState } from "react";
import { WebhookCard, WebhookColumns } from "@/app/dashboard/webhooks";
import { DataTable } from "@/components/data-table";
import { Separator } from "@/components/ui/separator";

interface WebhooksTableProps {
  webhooks: Webhook[];
  onDelete: (endpoint: Webhook) => void;
  onTest: (endpoint: Webhook) => void;
  onCopyUrl: (uuid: string) => void;
  isLoading?: boolean;
}

export function WebhooksTable({
  webhooks,
  onDelete,
  onTest,
  onCopyUrl,
  isLoading = false,
}: WebhooksTableProps) {
  const router = useRouter();
  const [expandedCard, setExpandedCard] = useState<number | null>(null);

  const handleEdit = (webhook: Webhook) => {
    router.push(`/dashboard/webhooks/config/edit/${webhook.id}`);
  };

  const columns = WebhookColumns({
    onDelete,
    onTest,
    onCopyUrl,
    onEdit: handleEdit,
  });

  // Empty state component
  const EmptyState = () => (
    <Card>
      <CardContent className="flex flex-col items-center justify-center py-12 text-center">
        <Globe className="w-12 h-12 text-muted-foreground mb-4" />
        <h3 className="text-lg font-semibold mb-2">No webhooks configured</h3>
        <p className="text-muted-foreground text-sm">
          Create your first webhook to get started
        </p>
      </CardContent>
    </Card>
  );

  return (
    <div className="space-y-4">
      {/* Mobile Card Layout */}
      <Separator className="my-4" />

      <div className="block lg:hidden">
        {isLoading ? (
          <div className="space-y-4">
            {Array.from({ length: 3 }).map((_, index) => (
              <Card key={index}>
                <CardContent className="p-4">
                  <div className="space-y-3">
                    <div className="flex items-center justify-between">
                      <Skeleton className="h-5 w-32" />
                      <Skeleton className="h-5 w-16" />
                    </div>
                    <Skeleton className="h-4 w-full" />
                    <div className="flex items-center gap-2">
                      <Skeleton className="h-5 w-16" />
                      <Skeleton className="h-5 w-20" />
                    </div>
                    <div className="flex items-center justify-end gap-2">
                      <Skeleton className="h-8 w-16" />
                      <Skeleton className="h-8 w-8" />
                      <Skeleton className="h-8 w-8" />
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        ) : webhooks.length === 0 ? (
          <EmptyState />
        ) : (
          <div className="space-y-4">
            {webhooks.map((webhook) => (
              <WebhookCard
                key={webhook.id}
                webhook={webhook}
                onDelete={onDelete}
                onTest={onTest}
                onCopyUrl={onCopyUrl}
                expandedCard={expandedCard}
                onToggleExpand={setExpandedCard}
              />
            ))}
          </div>
        )}
      </div>

      {/* Desktop Table Layout */}
      <div className="hidden lg:block">
        <DataTable
          columns={columns}
          data={webhooks}
          isLoading={isLoading}
          searchKey="name"
          searchPlaceholder="Search webhooks by name..."
          defaultPageSize={10}
          pageSizeOptions={[5, 10, 25, 50]}
        />
      </div>
    </div>
  );
}
