"use client";

import { ColumnDef } from "@tanstack/react-table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Edit,
  Trash2,
  TestTube,
  Copy,
  BadgeCheckIcon,
  BadgeAlert,
  ArrowUpDown,
} from "lucide-react";
import { convertUTCToLocalDate } from "@/lib/utils";
import { Bot } from "@/types/bot";

interface BotColumnsProps {
  onEdit: (bot: Bot) => void;
  onDelete: (bot: Bot) => void;
  onTest: (bot: Bot) => void;
  onCopy: (text: string) => void;
  testingBotId: number | null;
}

export const botColumns = ({
  onEdit,
  onDelete,
  onTest,
  onCopy,
  testingBotId,
}: BotColumnsProps): ColumnDef<Bot>[] => [
  {
    accessorKey: "name",
    meta: {
      displayName: "Instance Name",
    },
    header: ({ column }) => {
      return (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          className="h-auto p-0 font-semibold"
        >
          Instance Name
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      );
    },
    cell: ({ row }) => (
      <div className="font-medium">{row.getValue("name")}</div>
    ),
  },
  {
    accessorKey: "botToken",
    header: "Bot Token",
    meta: {
      displayName: "Bot Token",
    },
    cell: ({ row }) => {
      const token = row.getValue("botToken") as string;
      return (
        <div className="flex items-center gap-2">
          <span className="font-mono text-sm">{token.substring(0, 20)}...</span>
          <Button variant="ghost" size="sm" onClick={() => onCopy(token)}>
            <Copy className="w-3 h-3" />
          </Button>
        </div>
      );
    },
  },
  {
    accessorKey: "chatId",
    meta: {
      displayName: "Chat ID",
    },
    header: "Chat ID",
    cell: ({ row }) => {
      const chatId = row.getValue("chatId") as string;
      return (
        <div className="flex items-center gap-2">
          <span className="font-mono text-sm">{chatId.substring(4)}</span>
          <Button variant="ghost" size="sm" onClick={() => onCopy(chatId)}>
            <Copy className="w-3 h-3" />
          </Button>
        </div>
      );
    },
  },
  {
    accessorKey: "createdAt",
    meta: {
      displayName: "Created At",
    },
    header: ({ column }) => {
      return (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          className="h-auto p-0 font-semibold"
        >
          Created
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      );
    },
    cell: ({ row }) => (
      <div>{convertUTCToLocalDate(row.getValue("createdAt"))}</div>
    ),
  },
  {
    accessorKey: "hasPassedTest",
    header: "Status",
    meta: {
      displayName: "Test Status",
    },
    cell: ({ row }) => {
      const hasPassedTest = row.getValue("hasPassedTest") as boolean;
      return (
        <Badge
          variant={hasPassedTest ? "secondary" : "destructive"}
          className={hasPassedTest ? "badge-success" : "badge-error"}
        >
          {hasPassedTest ? "Tested" : "Untested"}
          {hasPassedTest ? (
            <BadgeCheckIcon className="inline w-4 h-4 ml-1" />
          ) : (
            <BadgeAlert className="inline w-4 h-4 ml-1" />
          )}
        </Badge>
      );
    },
  },
  {
    id: "actions",
    meta: {
      displayName: "Actions",
    },
    header: () => <div className="text-right">Actions</div>,
    cell: ({ row }) => {
      const bot = row.original;
      return (
        <div className="flex items-center justify-end gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => onTest(bot)}
            disabled={testingBotId === bot.id}
          >
            <TestTube className="w-4 h-4" />
            {testingBotId === bot.id ? "Testing..." : "Test"}
          </Button>
          <Button variant="ghost" size="sm" onClick={() => onEdit(bot)}>
            <Edit className="w-4 h-4" />
          </Button>
          <Button
            className="text-destructive hover:bg-destructive/10 hover:text-foreground dark:hover:bg-destructive/20"
            variant="ghost"
            size="sm"
            onClick={() => onDelete(bot)}
          >
            <Trash2 className="w-4 h-4" />
          </Button>
        </div>
      );
    },
  },
];
