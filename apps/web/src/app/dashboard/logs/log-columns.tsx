"use client";

import { ColumnDef } from "@tanstack/react-table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Eye,
  CheckCircle,
  XCircle,
  AlertCircle,
  Server,
  Smartphone,
  ArrowUpDown,
} from "lucide-react";
import { WebhookLog } from "@/types/log";
import { formatMillisecondsToDateTime } from "@/lib/utils";

const getStatusBadge = (statusCode: number) => {
  if (statusCode >= 200 && statusCode < 300) {
    return <Badge className="badge-success">Success</Badge>;
  } else if (statusCode >= 400 && statusCode < 500) {
    return <Badge className="badge-warning">Client Error</Badge>;
  } else if (statusCode >= 500) {
    return <Badge className="badge-error">Server Error</Badge>;
  }
  return <Badge variant="outline">{statusCode}</Badge>;
};

const getProcessingTimeBadge = (timeMs: number) => {
  let className = "";

  if (timeMs > 2000) {
    className = "badge-error";
  } else if (timeMs > 1200) {
    className = "badge-warning";
  } else {
    className = "badge-success";
  }

  return <Badge className={className}>{timeMs}ms</Badge>;
};

const getRequestSource = (headers: string) => {
  try {
    const parsedHeaders = JSON.parse(headers);
    const userAgent =
      parsedHeaders["User-Agent"] || parsedHeaders["user-agent"] || "";
    const host = parsedHeaders["Host"] || parsedHeaders["host"] || "";

    // Check if it's from our frontend (internal test)
    if (
      userAgent.includes("Mozilla") ||
      userAgent.includes("Firefox") ||
      userAgent.includes("Chrome")
    ) {
      if (host.includes("localhost") || host.includes("127.0.0.1")) {
        return {
          type: "internal",
          icon: <Smartphone className="w-3 h-3" />,
          label: "Internal Test",
        };
      } else {
        return {
          type: "external",
          icon: <Server className="w-3 h-3" />,
          label: "External",
        };
      }
    } else if (userAgent) {
      // Any non-browser user agent is external service
      return {
        type: "external",
        icon: <Server className="w-3 h-3" />,
        label: "External Service",
      };
    }
    return {
      type: "unknown",
      icon: <AlertCircle className="w-3 h-3" />,
      label: "Unknown",
    };
  } catch {
    return {
      type: "unknown",
      icon: <AlertCircle className="w-3 h-3" />,
      label: "Unknown",
    };
  }
};

export const LogColumns = (
  onViewDetails: (log: WebhookLog) => void
): ColumnDef<WebhookLog>[] => [
  {
    id: "webhook",
    header: "Webhook",
    meta: {
      displayName: "Webhook",
    },
    accessorFn: (row) => row.webhook.name,
    cell: ({ row }) => (
      <div className="font-medium">{row.original.webhook.name}</div>
    ),
  },
  {
    accessorKey: "createdAt",
    meta: {
      displayName: "Timestamp",
    },
    header: ({ column }) => {
      return (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          className="h-auto p-0 font-semibold"
        >
          Timestamp
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      );
    },
    cell: ({ row }) => (
      <div className="font-mono text-sm">
        {formatMillisecondsToDateTime(row.getValue("createdAt"))}
      </div>
    ),
  },

  {
    accessorKey: "requestHeaders",
    header: "Source",
    meta: {
      displayName: "Request Source",
    },
    cell: ({ row }) => {
      const source = getRequestSource(row.getValue("requestHeaders"));
      return (
        <div className="flex items-center gap-2">
          {source.icon}
          <span className="text-sm">{source.label}</span>
        </div>
      );
    },
  },
  {
    accessorKey: "responseStatusCode",
    meta: {
      displayName: "Status Code",
    },
    header: ({ column }) => {
      return (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          className="h-auto p-0 font-semibold"
        >
          Status
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      );
    },
    cell: ({ row }) => getStatusBadge(row.getValue("responseStatusCode")),
  },
  {
    accessorKey: "processingTimeMs",
    meta: {
      displayName: "Processing Time",
    },
    header: ({ column }) => {
      return (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          className="h-auto p-0 font-semibold"
        >
          Processing
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      );
    },
    cell: ({ row }) => getProcessingTimeBadge(row.getValue("processingTimeMs")),
  },
  {
    accessorKey: "telegramSent",
    header: "Telegram",
    meta: {
      displayName: "Telegram Sent",
    },
    cell: ({ row }) =>
      row.getValue("telegramSent") ? (
        <CheckCircle className="w-4 h-4 text-green-500" />
      ) : (
        <XCircle className="w-4 h-4 text-red-500" />
      ),
  },
  {
    id: "actions",
    header: () => <div className="text-right">Actions</div>,
    meta: {
      displayName: "Actions",
    },
    cell: ({ row }) => (
      <div className="text-right">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => onViewDetails(row.original)}
        >
          <Eye className="w-4 h-4" />
        </Button>
      </div>
    ),
  },
];
