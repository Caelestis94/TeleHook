import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import {
  Edit,
  Trash2,
  Copy,
  TestTube,
  ChevronRight,
  Globe,
  WebhookIcon,
} from "lucide-react";
import { useRouter } from "next/navigation";
import type { Webhook } from "@/types/webhook";
import { useState } from "react";
import { useMediaQuery } from "@/hooks/useMediaQuery";

interface WebhookCardProps {
  webhook: Webhook;
  onDelete: (webhook: Webhook) => void;
  onTest: (webhook: Webhook) => void;
  onCopyUrl: (uuid: string) => void;
  expandedCard?: number | null;
  onToggleExpand?: (id: number | null) => void;
}

export function WebhookCard({
  webhook,
  onDelete,
  onTest,
  onCopyUrl,
  expandedCard,
  onToggleExpand,
}: WebhookCardProps) {
  const router = useRouter();
  const [localExpanded, setLocalExpanded] = useState<boolean>(false);
  const isMobile = useMediaQuery("(max-width: 768px)");

  // Use external state if provided, otherwise use local state
  const isExpanded =
    expandedCard !== undefined ? expandedCard === webhook.id : localExpanded;
  const toggleExpanded = onToggleExpand
    ? () => onToggleExpand(isExpanded ? null : webhook.id)
    : () => setLocalExpanded(!localExpanded);

  return (
    <Card className="transition-all duration-200 hover:shadow-md">
      <CardHeader className="pb-3 sm:pb-3">
        <div className="flex items-start justify-between">
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-2 flex-wrap">
              <WebhookIcon className="w-4 h-4 sm:w-5 sm:h-5 flex-shrink-0 text-green-600 dark:text-green-400" />
              <span className="font-semibold text-base sm:text-lg truncate">
                {webhook.name}
              </span>
              <Badge
                className={`${
                  webhook.isDisabled ? "badge-secondary" : "badge-success"
                } text-xs sm:text-sm`}
              >
                {webhook.isDisabled ? "Disabled" : "Enabled"}
              </Badge>
              <Badge
                className={`${
                  webhook.isProtected ? "badge-success" : "badge-error"
                } text-xs sm:text-sm`}
              >
                {webhook.isProtected ? "Protected" : "Unprotected"}
              </Badge>
            </div>
            {!isExpanded && (
              <div className="flex items-center gap-2 text-sm sm:text-base text-muted-foreground">
                <Globe className="w-3 h-3 sm:w-4 sm:h-4 flex-shrink-0" />
                <code className="text-xs sm:text-sm bg-muted px-2 py-1 rounded min-w-0 break-all overflow-hidden">
                  /api/trigger/{webhook.uuid.substring(0, 8)}...
                </code>
                <Button
                  variant="ghost"
                  size={isMobile ? "default" : "sm"}
                  className="h-8 w-8 sm:h-6 sm:w-6 p-0 flex-shrink-0"
                  onClick={() => onCopyUrl(webhook.uuid)}
                >
                  <Copy className="w-4 h-4 sm:w-3 sm:h-3" />
                </Button>
              </div>
            )}
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
          <div className="space-y-3 sm:grid sm:grid-cols-2 sm:gap-3 sm:space-y-0">
            <div className="space-y-1">
              <span className="text-sm sm:text-base text-muted-foreground font-medium">
                Chat ID:
              </span>
              <div className="flex items-center gap-2">
                <code className="text-xs sm:text-sm bg-muted px-2 py-1 sm:px-3 sm:py-2 rounded font-mono min-w-0 break-all overflow-hidden">
                  {webhook.bot.chatId.substring(4)}
                </code>
                <Button
                  variant="ghost"
                  size={isMobile ? "default" : "sm"}
                  className="h-8 w-8 sm:h-6 sm:w-6 p-0 flex-shrink-0"
                  onClick={() =>
                    navigator.clipboard.writeText(webhook.bot.chatId)
                  }
                >
                  <Copy className="w-4 h-4 sm:w-3 sm:h-3" />
                </Button>
              </div>
            </div>

            {webhook.topicId && (
              <div className="space-y-1">
                <span className="text-sm sm:text-base text-muted-foreground font-medium">
                  Topic ID:
                </span>
                <div className="flex items-center gap-2">
                  <code className="text-xs sm:text-sm bg-muted px-2 py-1 sm:px-3 sm:py-2 rounded font-mono min-w-0 break-all overflow-hidden">
                    {webhook.topicId}
                  </code>
                  <Button
                    variant="ghost"
                    size={isMobile ? "default" : "sm"}
                    className="h-8 w-8 sm:h-6 sm:w-6 p-0 flex-shrink-0"
                    onClick={() =>
                      navigator.clipboard.writeText(webhook.topicId.toString())
                    }
                  >
                    <Copy className="w-4 h-4 sm:w-3 sm:h-3" />
                  </Button>
                </div>
              </div>
            )}

            <div className="space-y-1 sm:col-span-2">
              <span className="text-sm sm:text-base text-muted-foreground font-medium">
                Full URL:
              </span>
              <div className="flex items-center gap-2">
                <code className="text-xs sm:text-sm bg-muted px-2 py-1 sm:px-3 sm:py-2 rounded font-mono min-w-0 break-all overflow-hidden">
                  /api/trigger/{webhook.uuid}
                </code>
                <Button
                  variant="ghost"
                  size={isMobile ? "default" : "sm"}
                  className="h-8 w-8 sm:h-6 sm:w-6 p-0 flex-shrink-0"
                  onClick={() => onCopyUrl(webhook.uuid)}
                >
                  <Copy className="w-4 h-4 sm:w-3 sm:h-3" />
                </Button>
              </div>
            </div>
          </div>

          <div className="flex flex-col sm:flex-row gap-3 sm:gap-2 pt-3 sm:pt-2 border-t">
            <Button
              disabled={webhook.isDisabled}
              variant="outline"
              size={isMobile ? "default" : "sm"}
              className="flex-1 min-w-0 h-10 sm:h-8"
              onClick={() => onTest(webhook)}
            >
              <TestTube className="w-4 h-4 mr-2" />
              <span className="text-sm sm:text-xs">Test</span>
            </Button>
            <Button
              variant="outline"
              size={isMobile ? "default" : "sm"}
              className="flex-1 min-w-0 h-10 sm:h-8"
              onClick={() =>
                router.push(`/dashboard/webhooks/config/edit/${webhook.id}`)
              }
            >
              <Edit className="w-4 h-4 mr-2" />
              <span className="text-sm sm:text-xs">Edit</span>
            </Button>
            <Button
              variant="outline"
              size={isMobile ? "default" : "sm"}
              className="text-destructive hover:bg-destructive/10 hover:text-destructive h-10 sm:h-8 sm:px-3"
              onClick={() => onDelete(webhook)}
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
