import { ConfirmationDialog } from "@/components/confirmation-dialog";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { WebhookFormData } from "@/types/webhook";
import { Upload, FileText, Loader2, Copy, Network, X } from "lucide-react";
import { useEffect, useState, useCallback } from "react";
import { useFormContext } from "react-hook-form";
import { useSession } from "next-auth/react";
import {
  useStartCapture,
  useCaptureStatus,
  useCancelCapture,
} from "@/hooks/queries/usePayloadCapture";
import { Separator } from "@/components/ui/separator";
import { toast } from "sonner";
import { formatTimeRemaining, parseJsonSafely } from "@/lib/utils";
import { JsonTreeViewer } from "@/components/json-tree-viewer";

export function WebhookPayloadSampleForm({}) {
  const [sampleInput, setSampleInput] = useState("");
  const [showClipboardPasteFailedDialog, setShowClipboardPasteFailedDialog] =
    useState(false);
  const [activeSessionId, setActiveSessionId] = useState<string | null>(null);
  const [showRetryDialog, setShowRetryDialog] = useState(false);
  const [isCancelling, setIsCancelling] = useState(false);
  const [timeRemaining, setTimeRemaining] = useState<number>(0);

  const form = useFormContext<WebhookFormData>();
  const { data: session } = useSession();

  // TanStack Query hooks
  const startCaptureMutation = useStartCapture();
  const cancelCaptureMutation = useCancelCapture();
  const { data: captureSession, error: captureError } = useCaptureStatus(
    activeSessionId,
    !!activeSessionId
  );

  const isValidJson = (str: string) => {
    try {
      JSON.parse(str);
      return true;
    } catch {
      return false;
    }
  };

  // Stable reference to form setValue
  const setPayloadSample = useCallback(
    (value: string) => {
      form.setValue("payloadSample", value);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [form.setValue]
  );

  useEffect(() => {
    if (sampleInput && isValidJson(sampleInput)) {
      setPayloadSample(sampleInput);
    }
  }, [sampleInput, setPayloadSample]);

  useEffect(() => {
    if (captureSession) {
      if (captureSession.status === "Completed" && captureSession.payload) {
        const payloadString = JSON.stringify(captureSession.payload, null, 2);
        setSampleInput(payloadString);
        setActiveSessionId(null);
        toast.success("Payload captured successfully!");
      } else if (captureSession.status === "Cancelled") {
        setActiveSessionId(null);
      } else if (new Date(captureSession.expiresAt) < new Date()) {
        setActiveSessionId(null);
        setShowRetryDialog(true);
      }
    }
  }, [captureSession]);

  useEffect(() => {
    if (captureError) {
      setActiveSessionId(null);
      if (
        !captureError.message?.includes("404") &&
        !captureError.message?.includes("not found")
      ) {
        toast.error("Failed to get capture status");
      }
    }
  }, [captureError]);

  useEffect(() => {
    if (!captureSession || captureSession.status !== "Waiting") {
      setTimeRemaining(0);
      return;
    }

    const updateTimer = () => {
      const now = new Date().getTime();
      const expiry = new Date(captureSession.expiresAt).getTime();
      const remaining = Math.max(0, expiry - now);
      setTimeRemaining(remaining);
    };

    updateTimer();

    const interval = setInterval(updateTimer, 1000);

    return () => clearInterval(interval);
  }, [captureSession]);

  const handleStartCapture = () => {
    if (!session?.user?.id) {
      toast.error("Please sign in to start payload capture");
      return;
    }

    startCaptureMutation.mutate(
      { userId: parseInt(session.user.id) },
      {
        onSuccess: (captureSession) => {
          setActiveSessionId(captureSession.sessionId);
        },
      }
    );
  };

  const handleCancelCapture = () => {
    if (activeSessionId && !isCancelling && !cancelCaptureMutation.isPending) {
      setIsCancelling(true);
      cancelCaptureMutation.mutate(activeSessionId, {
        onSuccess: () => {
          setActiveSessionId(null);
          setIsCancelling(false);
        },
        onError: () => {
          setActiveSessionId(null);
          setIsCancelling(false);
        },
      });
    }
  };

  const copyToClipboard = (text: string) => {
    if (typeof navigator !== "undefined" && navigator.clipboard) {
      navigator.clipboard.writeText(text);
      toast.success("Copied to clipboard!");
    } else {
      toast.error("Clipboard access not available in this environment.");
    }
  };

  const getFullCaptureUrl = () => {
    if (captureSession && typeof window !== "undefined") {
      return `${window.location.origin}${captureSession.captureUrl}`;
    }
    return "";
  };

  const handleFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      const content = e.target?.result as string;
      setSampleInput(content);
    };
    reader.readAsText(file);
  };

  const isCapturing = !!activeSessionId && captureSession?.status === "Waiting";

  return (
    <div className="space-y-6">
      {/* Payload Capture Section */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base sm:text-lg">
            <Network className="h-4 w-4 sm:h-5 sm:w-5 text-green-600 dark:text-green-400" />
            {isCapturing ? "Capturing Payload" : "Payload Capture"}
          </CardTitle>
          <CardDescription className="text-sm">
            {isCapturing
              ? "Waiting for webhook payload. Send a test webhook to the URL below."
              : "Capture a real webhook payload automatically by triggering your service."}
          </CardDescription>
          <Separator />
        </CardHeader>
        <CardContent className="space-y-4">
          {!isCapturing ? (
            <div className="text-center space-y-4">
              <div className="text-xs sm:text-sm text-muted-foreground space-y-2">
                <p>
                  <strong>Instructions:</strong> Click &quot;Capture
                  Payload&quot; to get a temporary URL. Configure your service
                  to send a webhook to that URL, and the payload will be
                  captured automatically.
                </p>
                <p>The capture session will expire after 5 minutes.</p>
              </div>

              <Button
                type="button"
                onClick={handleStartCapture}
                disabled={startCaptureMutation.isPending || !session?.user?.id}
                className="w-full max-w-sm"
                size="lg"
              >
                {startCaptureMutation.isPending ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Starting...
                  </>
                ) : (
                  <>
                    <Network className="h-4 w-4 mr-2" />
                    Capture Payload
                  </>
                )}
              </Button>
            </div>
          ) : (
            <div className="space-y-4">
              <div className="text-center">
                <div className="flex flex-col items-center justify-center gap-2 mb-2">
                  <Loader2 className="h-24 w-24 sm:h-36 sm:w-36 animate-spin text-blue-500" />
                  <span className="text-xs sm:text-sm font-medium">
                    Waiting for webhook...
                  </span>
                </div>

                {timeRemaining > 0 && (
                  <div className="mb-4">
                    <span className="text-xs text-muted-foreground">
                      Time remaining:{" "}
                      <span className="font-mono font-bold">
                        {formatTimeRemaining(timeRemaining)}
                      </span>
                    </span>
                  </div>
                )}

                {/* Mobile-friendly URL display */}
                <div className="space-y-3">
                  <div className="p-3 rounded-lg">
                    <Label className="text-xs font-medium text-muted-foreground mb-1 block">
                      Webhook URL:
                    </Label>
                    <div className="break-all text-xs sm:text-sm font-mono bg-muted p-2 rounded border select-all">
                      {getFullCaptureUrl()}
                    </div>
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => copyToClipboard(getFullCaptureUrl())}
                      className="w-full mt-2"
                    >
                      <Copy className="h-3 w-3 mr-1" />
                      Copy URL
                    </Button>
                  </div>
                  <p className="text-xs text-muted-foreground px-2">
                    Configure your service to send a POST request to this URL
                  </p>
                </div>
              </div>

              <div className="text-center">
                <Button
                  variant="destructive"
                  size="sm"
                  type="button"
                  onClick={handleCancelCapture}
                  disabled={isCancelling || cancelCaptureMutation.isPending}
                  className="w-full max-w-xs"
                >
                  {isCancelling || cancelCaptureMutation.isPending ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Cancelling...
                    </>
                  ) : (
                    <>
                      <X className="h-4 w-4 mr-2" />
                      Cancel Capture
                    </>
                  )}
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Fallback Methods */}
      {!isCapturing && (
        <>
          <div className="text-center max-w-40 justify-center mx-auto flex items-center text-sm text-muted-foreground">
            <Separator className="border-1" />
            <span className="block mx-2 text-xs sm:text-sm">or</span>
            <Separator className="border-1" />
          </div>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-base sm:text-lg">
                <Upload className="h-4 w-4 sm:h-5 sm:w-5 text-green-600 dark:text-green-400" />
                Manual Payload Entry
              </CardTitle>
              <CardDescription className="text-sm">
                Upload or paste a sample JSON payload manually if you already
                have one.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Responsive grid that stacks on mobile */}
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 h-[400px] sm:h-[500px] lg:h-96">
                <div className="flex flex-col">
                  <span className="text-xs sm:text-sm text-muted-foreground mb-2 block">
                    Paste your JSON here
                  </span>
                  <Textarea
                    id="sample-input"
                    value={sampleInput}
                    onChange={(e) => setSampleInput(e.target.value)}
                    placeholder='{"message": "Hello World", "user": {"name": "John", "id": 123}}'
                    className="flex-1 font-mono text-xs sm:text-sm resize-none"
                  />
                </div>
                <div className="flex flex-col">
                  <span className="text-xs sm:text-sm text-muted-foreground mb-2 block">
                    JSON Preview
                  </span>
                  <div className="flex-1 overflow-y-auto bg-muted p-2 rounded-lg text-xs sm:text-sm">
                    {(() => {
                      const parsedData = parseJsonSafely(sampleInput);
                      if (parsedData !== null) {
                        return <JsonTreeViewer data={parsedData} />;
                      } else if (sampleInput.trim()) {
                        return (
                          <div className="text-red-500 text-xs">
                            Invalid JSON
                          </div>
                        );
                      } else {
                        return (
                          <div className="text-muted-foreground text-xs">
                            Enter JSON to see preview
                          </div>
                        );
                      }
                    })()}
                  </div>
                </div>
              </div>

              {/* Mobile-friendly button layout */}
              <div className="flex flex-col sm:flex-row gap-2">
                <Button
                  type="button"
                  variant="outline"
                  onClick={async () => {
                    try {
                      const text = await navigator.clipboard.readText();
                      setSampleInput(text);
                    } catch {
                      setShowClipboardPasteFailedDialog(true);
                    }
                  }}
                  className="flex-1 text-xs sm:text-sm"
                >
                  <FileText className="h-3 w-3 sm:h-4 sm:w-4 mr-2" />
                  Paste from Clipboard
                </Button>

                <Button
                  variant="outline"
                  type="button"
                  onClick={() =>
                    document.getElementById("file-upload")?.click()
                  }
                  className="flex-1 text-xs sm:text-sm"
                >
                  <Upload className="h-3 w-3 sm:h-4 sm:w-4 mr-2" />
                  Upload File
                </Button>
                <input
                  id="file-upload"
                  type="file"
                  accept=".json,.txt"
                  onChange={handleFileUpload}
                  className="hidden"
                />
              </div>
            </CardContent>
          </Card>
        </>
      )}

      {/* Dialogs */}
      <ConfirmationDialog
        open={showClipboardPasteFailedDialog}
        onOpenChange={() => setShowClipboardPasteFailedDialog(false)}
        title="Clipboard Paste Failed"
        isCancelAvailable={false}
        confirmText="OK"
        onConfirm={() => setShowClipboardPasteFailedDialog(false)}
      >
        <span className="text-sm">
          Failed to read from clipboard. Please paste your JSON manually.
        </span>
      </ConfirmationDialog>

      <ConfirmationDialog
        open={showRetryDialog}
        onOpenChange={() => setShowRetryDialog(false)}
        title="Capture Session Expired"
        isCancelAvailable={true}
        confirmText="Start New Session"
        cancelText="Cancel"
        onConfirm={() => {
          setShowRetryDialog(false);
          handleStartCapture();
        }}
      >
        {" "}
        <span className="text-sm">
          The capture session has expired. Would you like to start a new one?
        </span>
      </ConfirmationDialog>
    </div>
  );
}
