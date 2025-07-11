// components/logs/LogDetailsDialog.tsx
import { Badge } from "@/components/ui/badge";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { CheckCircle, XCircle } from "lucide-react";
import { JsonTreeViewer } from "@/components/json-tree-viewer";
import { WebhookLog } from "@/types/log";
import { convertUTCToLocalDateTime, parseJsonSafely } from "@/lib/utils";
import TelegramMarkdownPreview from "@/components/telegram-markdown-preview";
import { useMediaQuery } from "@/hooks/useMediaQuery";
import { Button } from "@/components/ui/button";

interface LogDetailsDialogProps {
  log: WebhookLog | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function LogDetailsDialog({
  log,
  open,
  onOpenChange,
}: LogDetailsDialogProps) {
  const isMobile = useMediaQuery("(max-width: 768px)");

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

    if (timeMs > 1000) {
      className = "badge-error";
    } else if (timeMs > 500) {
      className = "badge-warning";
    } else {
      className = "badge-success";
    }

    return <Badge className={className}>{timeMs}ms</Badge>;
  };

  if (!log) {
    return null;
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="lg:!max-w-[60vw] h-screen w-screen lg:!h-[80vh] !max-w-full !max-h-full flex flex-col">
        <DialogHeader className="flex-shrink-0">
          <DialogTitle>Request Details</DialogTitle>
          <DialogDescription>
            Complete webhook request information
          </DialogDescription>
        </DialogHeader>

        <div className="flex-1 min-h-0 overflow-y-auto">
          <div className="space-y-6">
            {/* Main Content - Two Column Layout */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              {/* Left Column - Request Information */}
              <div className="space-y-6">
                {/* Request Details Section */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      Request Details
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    <div>
                      <Label className="font-medium mb-2 text-muted-foreground">
                        Request ID
                      </Label>
                      <p className="font-mono text-xs break-all bg-muted p-2 rounded">
                        {log.requestId}
                      </p>
                    </div>
                    <div>
                      <Label className="font-medium mb-2 text-muted-foreground">
                        Endpoint
                      </Label>
                      <p className="font-medium bg-muted px-2 py-1 rounded text-sm">
                        {log.webhook.name}
                      </p>
                    </div>
                    <div>
                      <Label className="font-medium mb-2 text-muted-foreground">
                        Timestamp
                      </Label>
                      <p className="bg-muted px-2 py-1 rounded text-sm">
                        {convertUTCToLocalDateTime(log.createdAt)}
                      </p>
                    </div>
                    <div>
                      <Label className="font-medium mb-2 text-muted-foreground">
                        Method
                      </Label>
                      <p className="font-mono bg-muted px-2 py-1 rounded text-sm inline-block">
                        {log.httpMethod}
                      </p>
                    </div>
                    <div>
                      <Label className="font-medium mb-2 text-muted-foreground">
                        URL
                      </Label>
                      <p className="font-mono text-sm break-all bg-muted p-2 rounded">
                        {log.requestUrl}
                      </p>
                    </div>
                  </CardContent>
                </Card>
                {/* Request Headers */}
                <Card>
                  <CardHeader>
                    <CardTitle>Headers</CardTitle>
                  </CardHeader>
                  <CardContent>
                    {(() => {
                      const parsedData = parseJsonSafely(
                        log.requestHeaders || "{}"
                      );
                      if (parsedData !== null) {
                        return (
                          <JsonTreeViewer
                            clipboard={true}
                            data={parsedData}
                            className="p-3 bg-muted rounded text-xs overflow-x-auto whitespace-pre-wrap max-h-48 overflow-y-auto"
                          />
                        );
                      } else if (log.requestHeaders.trim()) {
                        return (
                          <div className="p-3 bg-muted font-semibold rounded text-xs mt-3 overflow-x-auto whitespace-pre-wrap max-h-48 overflow-y-auto">
                            JSON could not be parsed
                          </div>
                        );
                      }
                    })()}
                  </CardContent>
                </Card>

                {/* Request Body */}
                <Card>
                  <CardHeader>
                    <CardTitle>Request Body</CardTitle>
                  </CardHeader>
                  <CardContent>
                    {(() => {
                      const parsedData = parseJsonSafely(
                        log.requestBody || "{}"
                      );
                      if (parsedData !== null) {
                        return (
                          <JsonTreeViewer
                            clipboard={true}
                            data={parsedData}
                            className="p-3 bg-muted rounded text-xs overflow-x-auto whitespace-pre-wrap max-h-48 overflow-y-auto"
                          />
                        );
                      } else if (log.requestBody.trim()) {
                        return (
                          <div className="p-3 bg-muted font-semibold rounded text-xs mt-3 overflow-x-auto whitespace-pre-wrap max-h-48 overflow-y-auto">
                            JSON could not be parsed
                          </div>
                        );
                      }
                    })()}
                  </CardContent>
                </Card>
              </div>

              {/* Right Column - Processing & Response */}
              <div className="space-y-6">
                {/* Processing Results Section */}
                <Card>
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <CardTitle className="flex gap-2">
                        Processing Results
                      </CardTitle>
                      <div className="flex flex-col items-end gap-2">
                        {getProcessingTimeBadge(log.processingTimeMs)}
                        <span className="text-xs text-muted-foreground">
                          Processing Time
                        </span>
                      </div>
                    </div>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    {log.validationErrors && (
                      <div>
                        <Label className="font-medium text-red-600">
                          Validation Errors
                        </Label>
                        {(() => {
                          const parsedData = parseJsonSafely(
                            log.validationErrors || "{}"
                          );
                          if (parsedData !== null) {
                            return (
                              <JsonTreeViewer
                                clipboard={true}
                                data={parsedData}
                                className="p-3 bg-muted rounded text-xs overflow-x-auto whitespace-pre-wrap max-h-32 overflow-y-auto"
                              />
                            );
                          } else if (log.validationErrors.trim()) {
                            return (
                              <div className="p-3 bg-muted font-semibold rounded text-xs mt-3 overflow-x-auto whitespace-pre-wrap max-h-48 overflow-y-auto">
                                JSON could not be parsed
                              </div>
                            );
                          }
                        })()}
                      </div>
                    )}

                    <div className="flex items-center justify-between p-2 bg-muted/50 rounded">
                      <span className="font-medium">Telegram Sent</span>
                      {log.telegramSent ? (
                        <CheckCircle className="w-5 h-5 text-green-600" />
                      ) : (
                        <XCircle className="w-5 h-5 text-red-600" />
                      )}
                    </div>

                    {log.messageFormatted && (
                      <div>
                        <Label className="font-medium mb-2 text-muted-foreground">
                          Formatted Message
                        </Label>
                        <div className="p-4 bg-muted rounded-lg">
                          <pre className="text-sm whitespace-pre-wrap font-mono">
                            <TelegramMarkdownPreview
                              markdown={log.messageFormatted}
                            />
                          </pre>
                        </div>
                      </div>
                    )}

                    {log.telegramResponse && (
                      <div>
                        <Label className="font-medium text-muted-foreground">
                          Telegram Response
                        </Label>
                        {(() => {
                          const parsedData = parseJsonSafely(
                            log.telegramResponse || "{}"
                          );
                          if (parsedData !== null) {
                            return (
                              <JsonTreeViewer
                                clipboard={true}
                                data={parsedData}
                                className="p-3 bg-muted rounded text-xs overflow-x-auto whitespace-pre-wrap max-h-64 overflow-y-auto"
                              />
                            );
                          } else if (log.telegramResponse.trim()) {
                            return (
                              <div className="p-3 bg-muted font-semibold rounded text-xs mt-3 overflow-x-auto whitespace-pre-wrap max-h-48 overflow-y-auto">
                                JSON could not be parsed
                              </div>
                            );
                          }
                        })()}
                      </div>
                    )}
                  </CardContent>
                </Card>

                {/* Response Section */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      Response
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    <div className="flex items-center justify-between">
                      <Label className="font-medium text-muted-foreground">
                        Status Code
                      </Label>
                      {getStatusBadge(log.responseStatusCode)}
                    </div>
                    <div>
                      <Label className="font-medium text-muted-foreground mb-2">
                        Response Body
                      </Label>
                      {(() => {
                        const parsedData = parseJsonSafely(
                          log.responseBody || "{}"
                        );
                        if (parsedData !== null) {
                          return (
                            <JsonTreeViewer
                              clipboard={true}
                              data={parsedData}
                              className="p-3 bg-muted rounded text-xs overflow-x-auto whitespace-pre-wrap max-h-48 overflow-y-auto"
                            />
                          );
                        } else if (log.responseBody.trim()) {
                          return (
                            <div className="p-3 bg-muted font-semibold rounded text-xs mt-3 overflow-x-auto whitespace-pre-wrap max-h-48 overflow-y-auto">
                              JSON could not be parsed
                            </div>
                          );
                        }
                      })()}
                    </div>
                  </CardContent>
                </Card>
              </div>
            </div>
          </div>
        </div>
        {isMobile && (
          <DialogFooter>
            <Button className="w-full" onClick={() => onOpenChange(false)}>
              Close
            </Button>
          </DialogFooter>
        )}
      </DialogContent>
    </Dialog>
  );
}
