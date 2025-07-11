"use client";

import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Bot as BotIcon } from "lucide-react";
import type { Bot } from "@/types/bot";
import { useState } from "react";
import { botColumns, BotCard } from "@/app/dashboard/bots";
import { DataTable } from "@/components/data-table";
import { Separator } from "@/components/ui/separator";

interface BotsTableProps {
  bots: Bot[];
  onEdit: (bot: Bot) => void;
  onDelete: (bot: Bot) => void;
  onTest: (bot: Bot) => void;
  onCopy: (text: string) => void;
  testingBotId: number | null;
  isLoading?: boolean;
}

export function BotsTable({
  bots,
  onEdit,
  onDelete,
  onTest,
  onCopy,
  testingBotId,
  isLoading = false,
}: BotsTableProps) {
  const [expandedCard, setExpandedCard] = useState<number | null>(null);

  const columns = botColumns({
    onEdit,
    onDelete,
    onTest,
    onCopy,
    testingBotId,
  });

  const EmptyState = () => (
    <Card>
      <CardContent className="flex flex-col items-center justify-center py-12 text-center">
        <BotIcon className="w-12 h-12 text-muted-foreground mb-4" />
        <h3 className="text-lg font-semibold mb-2">No bots configured</h3>
        <p className="text-muted-foreground text-sm">
          Add your first Telegram bot to get started
        </p>
      </CardContent>
    </Card>
  );

  return (
    <div className="space-y-4">
      <Separator className="my-4" />
      {/* Mobile Card Layout */}
      <div className="block lg:hidden">
        {isLoading ? (
          <div className="space-y-4">
            {Array.from({ length: 3 }).map((_, index) => (
              <Card key={index}>
                <CardContent className="p-4">
                  <div className="space-y-3">
                    <div className="flex items-center justify-between">
                      <Skeleton className="h-5 w-40" />
                      <Skeleton className="h-5 w-16" />
                    </div>
                    <Skeleton className="h-4 w-full" />
                    <Skeleton className="h-4 w-32" />
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
        ) : bots.length === 0 ? (
          <EmptyState />
        ) : (
          <div className="space-y-4">
            {bots.map((bot) => (
              <BotCard
                key={bot.id}
                bot={bot}
                onEdit={onEdit}
                onDelete={onDelete}
                onTest={onTest}
                onCopy={onCopy}
                testingBotId={testingBotId}
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
          data={bots}
          isLoading={isLoading}
          searchKey="name"
          searchPlaceholder="Search bots by name..."
          defaultPageSize={10}
          pageSizeOptions={[5, 10, 25, 50]}
        />
      </div>
    </div>
  );
}
