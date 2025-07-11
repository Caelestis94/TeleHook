"use client";

import { useEffect, useState } from "react";
import { Activity, Download } from "lucide-react";
import { LogDetailsDialog, LogsFilters, LogsStatsCards, LogsTable } from ".";
import { useLogs, useLogsExport, useWebhooks } from "@/hooks/queries";
import type { WebhookLog, LogFilters } from "@/types/log";
import { PageHeader } from "@/components/layout";
import { handleError } from "@/lib/error-handling";

export function LogsPageClient() {
  // Filter state
  const [filters, setFilters] = useState<LogFilters>({
    webhookId: "",
    statusCode: "",
    dateFrom: "",
    dateTo: "",
    searchTerm: "",
  });

  // Dialog state
  const [selectedLog, setSelectedLog] = useState<WebhookLog | null>(null);
  const [showDetailsDialog, setShowDetailsDialog] = useState(false);

  // TanStack Query hooks
  const {
    data: logs = [],
    isLoading: logsLoading,
    error: logsError,
    isError: logsIsError,
    refetch: refetchLogs,
  } = useLogs(filters);
  const {
    data: webhooks = [],
    isLoading: webhooksLoading,
    isError: webhooksIsError,
    error: webhooksError,
  } = useWebhooks();
  const {
    isLoading: exportLoading,
    refetch: exportLogs,
    isError: exportIsError,
    error: exportError,
  } = useLogsExport();

  const isLoading = logsLoading || webhooksLoading || exportLoading;
  const isError = logsIsError || webhooksIsError || exportIsError;
  const error = logsError || webhooksError || exportError;

  useEffect(() => {
    if (isError && error) {
      handleError(error);
    }
  }, [isError, error]);

  // Simplified refresh - just refetch the queries
  const handleRefresh = () => {
    refetchLogs();
  };

  const openLogDetails = (log: WebhookLog) => {
    setSelectedLog(log);
    setShowDetailsDialog(true);
  };

  const clearFilters = () => {
    setFilters({
      webhookId: "",
      statusCode: "",
      dateFrom: "",
      dateTo: "",
      searchTerm: "",
    });
  };

  const handleExport = async () => {
    try {
      const result = await exportLogs();
      if (result.data) {
        const jsonString = JSON.stringify(result.data, null, 2);
        const blob = new Blob([jsonString], { type: "application/json" });
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = `webhook-logs-${
          new Date().toISOString().split("T")[0]
        }.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
      }
    } catch (error) {
      console.error("Export failed:", error);
    }
  };

  return (
    <>
      <div className="space-y-6">
        {/* Header */}
        <PageHeader
          title="Webhook Logs"
          description="Monitor webhook requests and debug delivery issues"
          icon={Activity}
          iconColor="text-red-600 dark:text-red-400"
          iconBgColor="bg-red-100 dark:bg-red-900/20"
          refreshAction={{
            onClick: handleRefresh,
            disabled: isLoading,
          }}
          actions={[
            {
              label: "Export",
              icon: Download,
              onClick: handleExport,
              disabled: exportLoading,
              variant: "outline",
            },
          ]}
          breadcrumbItems={[
            { label: "Dashboard", href: "/dashboard" },
            { label: "Logs" },
          ]}
        />
        {/* Filters */}
        <LogsFilters
          filters={filters}
          onFiltersChange={setFilters}
          webhooks={webhooks}
          onClearFilters={clearFilters}
        />

        {/* Stats Summary */}
        <LogsStatsCards logs={logs} />

        {/* Logs table */}
        <LogsTable
          isLoading={isLoading}
          logs={logs}
          onViewDetails={openLogDetails}
        />

        {/* Log Details Dialog */}
        <LogDetailsDialog
          log={selectedLog}
          open={showDetailsDialog}
          onOpenChange={setShowDetailsDialog}
        />
      </div>
    </>
  );
}
