import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import {
  Edit,
  Trash2,
  TestTube,
  Copy,
  BadgeCheckIcon,
  BadgeAlert,
  Bot as BotIcon,
  ChevronRight,
  Calendar,
} from "lucide-react";
import { convertUTCToLocalDate } from "@/lib/utils";
import type { Bot } from "@/types/bot";
import { useState } from "react";
import { useMediaQuery } from "@/hooks/useMediaQuery";

interface BotCardProps {
  bot: Bot;
  onEdit: (bot: Bot) => void;
  onDelete: (bot: Bot) => void;
  onTest: (bot: Bot) => void;
  onCopy: (text: string) => void;
  testingBotId: number | null;
  expandedCard?: number | null;
  onToggleExpand?: (id: number | null) => void;
}

export function BotCard({
  bot,
  onEdit,
  onDelete,
  onTest,
  onCopy,
  testingBotId,
  expandedCard,
  onToggleExpand,
}: BotCardProps) {
  const [localExpanded, setLocalExpanded] = useState<boolean>(false);
  const isMobile = useMediaQuery("(max-width: 768px)");

  // Use external state if provided, otherwise use local state
  const isExpanded =
    expandedCard !== undefined ? expandedCard === bot.id : localExpanded;
  const toggleExpanded = onToggleExpand
    ? () => onToggleExpand(isExpanded ? null : bot.id)
    : () => setLocalExpanded(!localExpanded);

  return (
    <Card className="transition-all duration-200 hover:shadow-md">
      <CardHeader className="pb-3 sm:pb-3">
        <div className="flex items-start justify-between">
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-2">
              <BotIcon className="w-4 h-4 sm:w-5 sm:h-5 flex-shrink-0 text-blue-500" />
              <h3 className="font-semibold text-base sm:text-lg truncate">
                {bot.name}
              </h3>
              <Badge
                variant={bot.hasPassedTest ? "secondary" : "destructive"}
                className={`${
                  bot.hasPassedTest ? "badge-success" : "badge-error"
                } text-xs sm:text-sm`}
              >
                {bot.hasPassedTest ? "Tested" : "Untested"}
                {bot.hasPassedTest ? (
                  <BadgeCheckIcon className="inline w-3 h-3 sm:w-4 sm:h-4 ml-1" />
                ) : (
                  <BadgeAlert className="inline w-3 h-3 sm:w-4 sm:h-4 ml-1" />
                )}
              </Badge>
            </div>
            <div className="text-sm sm:text-base text-muted-foreground">
              <div className="flex items-center gap-2">
                <Calendar className="w-3 h-3 sm:w-4 sm:h-4" />
                <span>Created {convertUTCToLocalDate(bot.createdAt)}</span>
              </div>
            </div>
          </div>
          <Button
            variant="ghost"
            size={isMobile ? "default" : "sm"}
            className="h-8 w-8 sm:h-10 sm:w-10 p-0 flex-shrink-0"
            onClick={toggleExpanded}
          >
            <ChevronRight
              className={`w-4 h-4 sm:w-5 sm:h-5 transition-transform duration-200 ${
                isExpanded ? "rotate-90" : ""
              }`}
            />
          </Button>
        </div>
      </CardHeader>

      {isExpanded && (
        <CardContent className="pt-0 space-y-4 sm:space-y-3 animate-in slide-in-from-top-2 duration-200">
          <div className="space-y-4 sm:space-y-3">
            <div>
              <span className="text-sm sm:text-base text-muted-foreground font-medium">
                Bot Token:
              </span>
              <div className="flex items-center gap-2 sm:gap-3 mt-2 sm:mt-1">
                <code className="select-all text-xs sm:text-sm bg-muted px-2 py-1 sm:px-3 sm:py-2 rounded font-mono min-w-0 break-all overflow-hidden">
                  {bot.botToken}
                </code>
                <Button
                  variant="ghost"
                  size={isMobile ? "default" : "sm"}
                  className="h-8 w-8 sm:h-6 sm:w-6 p-0 flex-shrink-0"
                  onClick={() => onCopy(bot.botToken)}
                >
                  <Copy className="w-4 h-4 sm:w-3 sm:h-3" />
                </Button>
              </div>
            </div>

            <div>
              <span className="text-sm sm:text-base text-muted-foreground font-medium">
                Chat ID:
              </span>
              <div className="flex items-center gap-2 sm:gap-3 mt-2 sm:mt-1">
                <code className="select-all text-xs sm:text-sm bg-muted px-2 py-1 sm:px-3 sm:py-2 rounded font-mono min-w-0 break-all overflow-hidden">
                  {bot.chatId.substring(4)}
                </code>
                <Button
                  variant="ghost"
                  size={isMobile ? "default" : "sm"}
                  className="h-8 w-8 sm:h-6 sm:w-6 p-0 flex-shrink-0"
                  onClick={() => onCopy(bot.chatId)}
                >
                  <Copy className="w-4 h-4 sm:w-3 sm:h-3" />
                </Button>
              </div>
            </div>
          </div>

          <div className="flex flex-col sm:flex-row gap-3 sm:gap-2 pt-3 sm:pt-2 border-t">
            <Button
              variant="outline"
              size={isMobile ? "default" : "sm"}
              className="flex-1 min-w-0 h-10 sm:h-8"
              onClick={() => onTest(bot)}
              disabled={testingBotId === bot.id}
            >
              <TestTube className="w-4 h-4 mr-2" />
              <span className="text-sm sm:text-xs">
                {testingBotId === bot.id ? "Testing..." : "Test"}
              </span>
            </Button>
            <Button
              variant="outline"
              size={isMobile ? "default" : "sm"}
              className="flex-1 min-w-0 h-10 sm:h-8"
              onClick={() => onEdit(bot)}
            >
              <Edit className="w-4 h-4 mr-2" />
              <span className="text-sm sm:text-xs">Edit</span>
            </Button>
            <Button
              variant="outline"
              size={isMobile ? "default" : "sm"}
              className="text-destructive hover:bg-destructive/10 hover:text-destructive h-10 sm:h-8 sm:px-3"
              onClick={() => onDelete(bot)}
            >
              <Trash2 className="w-4 h-4 mr-2 sm:mr-0" />
              <span className="text-sm sm:text-xs sm:hidden">Delete</span>
            </Button>
          </div>
        </CardContent>
      )}
    </Card>
  );
}
