"use client";

import { useState } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Activity } from "lucide-react";
import { LogCard, LogColumns } from "@/app/dashboard/logs/";
import { DataTable } from "@/components/data-table";
import { WebhookLog } from "@/types/log";
import { Separator } from "@/components/ui/separator";

interface LogsTableProps {
  logs: WebhookLog[];
  onViewDetails: (log: WebhookLog) => void;
  isLoading?: boolean;
}

export function LogsTable({
  logs,
  onViewDetails,
  isLoading = false,
}: LogsTableProps) {
  const [expandedCard, setExpandedCard] = useState<number | null>(null);

  const columns = LogColumns(onViewDetails);

  // Empty state component
  const EmptyState = () => (
    <Card>
      <CardContent className="flex flex-col items-center justify-center py-12 text-center">
        <Activity className="w-12 h-12 text-muted-foreground mb-4" />
        <h3 className="text-lg font-semibold mb-2">
          {logs.length === 0
            ? "No webhook logs found"
            : "No logs match the current filters"}
        </h3>
        <p className="text-muted-foreground text-sm">
          {logs.length === 0
            ? "Webhook requests will appear here once you start receiving them"
            : "Try adjusting your filters to see more results"}
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
                      <Skeleton className="h-4 w-32" />
                      <Skeleton className="h-5 w-16" />
                    </div>
                    <div className="flex items-center gap-2">
                      <Skeleton className="h-4 w-4" />
                      <Skeleton className="h-4 w-24" />
                    </div>
                    <div className="flex items-center gap-2">
                      <Skeleton className="h-4 w-16" />
                      <Skeleton className="h-4 w-20" />
                    </div>
                    <div className="flex items-center justify-end">
                      <Skeleton className="h-8 w-8" />
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        ) : logs.length === 0 ? (
          <EmptyState />
        ) : (
          <div className="space-y-4">
            {logs.map((log) => (
              <LogCard
                key={log.id}
                log={log}
                onViewDetails={onViewDetails}
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
          data={logs}
          isLoading={isLoading}
          defaultPageSize={25}
          pageSizeOptions={[10, 25, 50, 100]}
        />
      </div>
    </div>
  );
}
