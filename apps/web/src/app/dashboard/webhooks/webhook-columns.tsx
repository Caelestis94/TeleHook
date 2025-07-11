"use client";

import { ColumnDef } from "@tanstack/react-table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Edit, Trash2, Copy, TestTube, ArrowUpDown } from "lucide-react";
import { Webhook } from "@/types/webhook";

interface WebhooksColumnsProps {
  onDelete: (webhook: Webhook) => void;
  onTest: (webhook: Webhook) => void;
  onCopyUrl: (uuid: string) => void;
  onEdit: (webhook: Webhook) => void;
}

export const WebhookColumns = ({
  onDelete,
  onTest,
  onCopyUrl,
  onEdit,
}: WebhooksColumnsProps): ColumnDef<Webhook>[] => [
  {
    accessorKey: "name",
    meta: {
      displayName: "Name",
    },
    header: ({ column }) => {
      return (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          className="h-auto p-0 font-semibold"
        >
          Name
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      );
    },
    cell: ({ row }) => (
      <div className="font-medium">{row.getValue("name")}</div>
    ),
  },
  {
    accessorKey: "uuid",
    header: "Webhook URL",
    meta: {
      displayName: "Webhook URL",
    },
    cell: ({ row }) => {
      const uuid = row.getValue("uuid") as string;
      return (
        <div className="flex items-center gap-2">
          <code className="text-xs bg-muted px-2 py-1 rounded">
            /api/trigger/{uuid.substring(0, 15)}...
          </code>
          <Button variant="ghost" size="sm" onClick={() => onCopyUrl(uuid)}>
            <Copy className="w-3 h-3" />
          </Button>
        </div>
      );
    },
  },
  {
    id: "chatId",
    header: "Chat ID",
    meta: {
      displayName: "Chat ID",
    },
    accessorFn: (row) => row.bot.chatId,
    cell: ({ row }) => (
      <div className="font-mono text-sm">
        {row.original.bot.chatId.substring(4)}
      </div>
    ),
  },
  {
    accessorKey: "topicId",
    header: "Topic ID",
    meta: {
      displayName: "Topic ID",
    },
    cell: ({ row }) => {
      const topicId = row.getValue("topicId") as string;
      return <div>{topicId || "-"}</div>;
    },
  },
  {
    accessorKey: "isDisabled",
    header: "Status",
    meta: {
      displayName: "Status",
    },
    cell: ({ row }) => {
      const isDisabled = row.getValue("isDisabled") as boolean;
      return (
        <Badge className={isDisabled ? "badge-secondary" : "badge-success"}>
          {isDisabled ? "Disabled" : "Enabled"}
        </Badge>
      );
    },
  },
  {
    accessorKey: "isProtected",
    header: "Protected",
    meta: {
      displayName: "Protected",
    },
    cell: ({ row }) => {
      const isProtected = row.getValue("isProtected") as boolean;
      return (
        <Badge className={isProtected ? "badge-success" : "badge-error"}>
          {isProtected ? "Protected" : "Unprotected"}
        </Badge>
      );
    },
  },
  {
    id: "actions",
    header: () => <div className="text-right">Actions</div>,
    meta: {
      displayName: "Actions",
    },
    cell: ({ row }) => {
      const webhook = row.original;
      return (
        <div className="flex items-center justify-end gap-2">
          <Button
            disabled={webhook.isDisabled}
            variant="ghost"
            size="sm"
            onClick={() => onTest(webhook)}
          >
            <TestTube className="w-4 h-4" />
            Test
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => onEdit(webhook)}
            title="Edit Webhook"
          >
            <Edit className="w-4 h-4" />
          </Button>
          <Button
            variant="ghost"
            size="sm"
            className="text-destructive hover:bg-destructive/10 hover:text-foreground dark:hover:bg-destructive/20"
            onClick={() => onDelete(webhook)}
          >
            <Trash2 className="w-4 h-4" />
          </Button>
        </div>
      );
    },
  },
];
