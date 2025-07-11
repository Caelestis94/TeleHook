import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { WebhookLog } from "@/types/log";
import {
  Eye,
  CheckCircle,
  XCircle,
  AlertCircle,
  Server,
  Smartphone,
  Clock,
  Activity,
  ChevronRight,
} from "lucide-react";
import { useState } from "react";

interface LogCardProps {
  log: WebhookLog;
  onViewDetails: (log: WebhookLog) => void;
  expandedCard?: number | null;
  onToggleExpand?: (id: number | null) => void;
}

export function LogCard({
  log,
  onViewDetails,
  expandedCard,
  onToggleExpand,
}: LogCardProps) {
  const [localExpanded, setLocalExpanded] = useState<boolean>(false);

  // Use external state if provided, otherwise use local state
  const isExpanded =
    expandedCard !== undefined ? expandedCard === log.id : localExpanded;
  const toggleExpanded = onToggleExpand
    ? () => onToggleExpand(isExpanded ? null : log.id)
    : () => setLocalExpanded(!localExpanded);

  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
    return date.toLocaleString();
  };

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

  const source = getRequestSource(log.requestHeaders);

  return (
    <Card className="transition-all duration-200 hover:shadow-md">
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-2">
              <Activity className="w-4 h-4 flex-shrink-0 text-red-600 dark:text-red-400" />
              <h3 className="font-semibold text-base truncate">
                {log.webhook.name}
              </h3>
              {getStatusBadge(log.responseStatusCode)}
            </div>
            <div className="flex items-center gap-3 text-sm text-muted-foreground">
              <div className="flex items-center gap-1">
                <Clock className="w-3 h-3" />
                <span className="font-mono text-xs">
                  {formatTimestamp(log.createdAt)}
                </span>
              </div>
              <div className="flex items-center gap-1">
                {source.icon}
                <span className="text-xs">{source.label}</span>
              </div>
            </div>
          </div>
          <Button
            variant="ghost"
            size="sm"
            className="h-8 w-8 p-0 flex-shrink-0"
            onClick={toggleExpanded}
          >
            <ChevronRight
              className={`w-4 h-4 transition-transform duration-200 ${
                isExpanded ? "rotate-90" : ""
              }`}
            />
          </Button>
        </div>
      </CardHeader>

      {isExpanded && (
        <CardContent className="pt-0 space-y-3 animate-in slide-in-from-top-2 duration-200">
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">
                  Processing:
                </span>
                {getProcessingTimeBadge(log.processingTimeMs)}
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">
                  Validation:
                </span>
                {log.payloadValidated ? (
                  <CheckCircle className="w-4 h-4 text-green-500" />
                ) : (
                  <XCircle className="w-4 h-4 text-red-500" />
                )}
              </div>
            </div>
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Method:</span>
                <Badge variant="outline" className="text-xs">
                  {log.httpMethod}
                </Badge>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Telegram:</span>
                {log.telegramSent ? (
                  <CheckCircle className="w-4 h-4 text-green-500" />
                ) : (
                  <XCircle className="w-4 h-4 text-red-500" />
                )}
              </div>
            </div>
          </div>

          <div className="pt-2 border-t">
            <Button
              variant="outline"
              size="sm"
              className="w-full"
              onClick={() => onViewDetails(log)}
            >
              <Eye className="w-4 h-4 mr-2" />
              View Details
            </Button>
          </div>
        </CardContent>
      )}
    </Card>
  );
}
