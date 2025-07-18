"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  FormDescription,
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { ConfirmationDialog } from "@/components/confirmation-dialog";
import { AppSettingsValidationSchema } from "@/validation/setting-schema";

import { mapApiErrorsToFields } from "@/validation/utils";
import { AppError } from "@/lib/error-handling";
import { toast } from "sonner";
import {
  Settings,
  RotateCcw,
  CheckCircle,
  XCircle,
  Bell,
  BellOff,
} from "lucide-react";
import type { AppSettingsFormData, AppSetting } from "@/types/settings";
import { useBots } from "@/hooks/queries";
import {
  useUpdateSettings,
  useHealthCheck,
  useTestNotification,
} from "@/hooks/mutations";

interface AppSettingsProps {
  currentSettings?: AppSetting;
}

export function AppSettings({ currentSettings }: AppSettingsProps) {
  const updateSettingsMutation = useUpdateSettings();
  const healthCheckMutation = useHealthCheck();
  const testNotificationMutation = useTestNotification();
  const { data: bots = [] } = useBots();
  const [showRestartDialog, setShowRestartDialog] = useState(false);
  const [isMonitoringHealth, setIsMonitoringHealth] = useState(false);
  const [healthStatus, setHealthStatus] = useState<
    "checking" | "online" | "offline" | null
  >(null);
  const [pendingData, setPendingData] = useState<AppSettingsFormData | null>(
    null
  );

  const form = useForm<AppSettingsFormData>({
    resolver: zodResolver(AppSettingsValidationSchema),
    defaultValues: {
      logLevel: currentSettings?.logLevel || "Warning",
      enableWebhookLogging: currentSettings?.enableWebhookLogging ?? true,
      webhookLogRetentionDays: currentSettings?.webhookLogRetentionDays || 0,
      statsDaysInterval: currentSettings?.statsDaysInterval || 30,
      enableFailureNotifications:
        currentSettings?.enableFailureNotifications ?? false,
      notificationBotToken: currentSettings?.notificationBotToken || "",
      notificationChatId: currentSettings?.notificationChatId || "",
      notificationTopicId: currentSettings?.notificationTopicId || "",
    },
    mode: "onChange",
  });

  // Update form when settings change
  useEffect(() => {
    if (currentSettings) {
      form.reset({
        logLevel: currentSettings.logLevel,
        enableWebhookLogging: currentSettings.enableWebhookLogging,
        webhookLogRetentionDays: currentSettings.webhookLogRetentionDays,
        statsDaysInterval: currentSettings.statsDaysInterval,
        enableFailureNotifications: currentSettings.enableFailureNotifications,
        notificationBotToken: currentSettings.notificationBotToken || "",
        notificationChatId: currentSettings.notificationChatId || "",
        notificationTopicId: currentSettings.notificationTopicId || "",
      });
    }
  }, [currentSettings, form]);

  const checkIfRestartRequired = (data: AppSettingsFormData) => {
    return data.logLevel !== currentSettings?.logLevel;
  };

  const handleBotSelection = async (botId: string) => {
    if (botId === "manual") return;

    const selectedBot = bots.find((bot) => bot.id.toString() === botId);
    if (selectedBot) {
      form.setValue("notificationBotToken", selectedBot.botToken);
      form.setValue("notificationChatId", selectedBot.chatId);

      // Trigger validation to update form state after programmatic changes
      await form.trigger(["notificationBotToken", "notificationChatId"]);
    }
  };

  const handleSubmit = async (data: AppSettingsFormData) => {
    const restartRequired = checkIfRestartRequired(data);

    if (restartRequired) {
      setPendingData(data);
      setShowRestartDialog(true);
      return;
    }

    await saveSettings(data);
  };

  const handleTestNotification = async () => {
    const formData = form.getValues();

    // First validate the form
    const isValid = await form.trigger();
    if (!isValid) {
      toast.error("Please correct the form errors before testing");
      return;
    }

    try {
      // Save settings first to ensure test uses current values
      await saveSettings(formData);
      // Then test with the saved settings
      await testNotificationMutation.mutateAsync();
    } catch (error) {
      // If save fails, don't proceed with test
      console.error("Failed to save settings before test:", error);
    }
  };

  const saveSettings = async (data: AppSettingsFormData) => {
    try {
      const response = await updateSettingsMutation.mutateAsync(data);

      if (response.isRestartRequired) {
        startHealthMonitoring();
      }
    } catch (error) {
      if (error instanceof AppError && error.details) {
        const fieldErrors = mapApiErrorsToFields(error.details, {
          logLevel: ["loglevel", "level"],
          enableWebhookLogging: ["enablewebhooklogging", "logging"],
          webhookLogRetentionDays: ["webhooklogretentiondays", "retention"],
          statsDaysInterval: ["statsdaysinterval", "interval"],
          enableFailureNotifications: [
            "enablefailurenotifications",
            "notifications",
          ],
          notificationBotToken: ["notificationbottoken", "bottoken"],
          notificationChatId: ["notificationchatid", "chatid"],
          notificationTopicId: ["notificationtopicid", "topicid"],
        });

        Object.entries(fieldErrors).forEach(([field, messages]) => {
          if (messages && messages.length > 0) {
            form.setError(field as keyof AppSettingsFormData, {
              type: "server",
              message: messages[0],
            });
          }
        });

        toast.error("Please correct the highlighted fields");
      }
    }
  };

  const startHealthMonitoring = () => {
    setIsMonitoringHealth(true);
    setHealthStatus("checking");

    const startTime = Date.now();
    const maxWaitTime = 30000; // 30 seconds

    const checkHealth = async () => {
      try {
        const isHealthy = await healthCheckMutation.mutateAsync();

        if (isHealthy) {
          setHealthStatus("online");
          setIsMonitoringHealth(false);
          toast.success("Application restarted successfully");
          return;
        }
      } catch {
        // Health check failed, continue monitoring
      }

      const elapsed = Date.now() - startTime;
      if (elapsed >= maxWaitTime) {
        setHealthStatus("offline");
        setIsMonitoringHealth(false);
        toast.error(
          "Application is taking longer than expected to restart. Please check manually."
        );
        return;
      }

      // Check again in 2 seconds
      setTimeout(checkHealth, 2000);
    };

    // Start checking after a 3-second delay to allow for restart
    setTimeout(checkHealth, 3000);
  };

  const handleConfirmRestart = async () => {
    setShowRestartDialog(false);
    if (pendingData) {
      await saveSettings(pendingData);
      setPendingData(null);
    }
  };

  const logLevels = [
    { value: "Trace", label: "Trace" },
    { value: "Debug", label: "Debug" },
    { value: "Information", label: "Information" },
    { value: "Warning", label: "Warning" },
    { value: "Error", label: "Error" },
    { value: "Critical", label: "Critical" },
  ];

  return (
    <>
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Settings className="h-5 w-5 text-yellow-600 dark:text-yellow-400" />
            Application Settings
          </CardTitle>
          <CardDescription>
            Configure application behavior and logging settings
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {isMonitoringHealth && (
            <div className="flex items-center gap-3 p-4 bg-blue-50 dark:bg-blue-950/20 border border-blue-200 dark:border-blue-800 rounded-lg">
              {healthStatus === "checking" && (
                <>
                  <RotateCcw className="h-4 w-4 text-blue-600 dark:text-blue-400 animate-spin" />
                  <span className="text-sm text-blue-700 dark:text-blue-300">
                    Application is restarting... Monitoring health status
                  </span>
                </>
              )}
              {healthStatus === "online" && (
                <>
                  <CheckCircle className="h-4 w-4 text-green-600 dark:text-green-400" />
                  <span className="text-sm text-green-700 dark:text-green-300">
                    Application is back online
                  </span>
                </>
              )}
              {healthStatus === "offline" && (
                <>
                  <XCircle className="h-4 w-4 text-red-600 dark:text-red-400" />
                  <span className="text-sm text-red-700 dark:text-red-300">
                    Application may still be restarting or unresponsive
                  </span>
                </>
              )}
            </div>
          )}

          <Form {...form}>
            <form
              onSubmit={form.handleSubmit(handleSubmit)}
              className="space-y-6"
            >
              <div className="space-y-4">
                <FormField
                  control={form.control}
                  name="logLevel"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="flex items-center gap-2">
                        Log Level
                      </FormLabel>
                      <Select
                        onValueChange={field.onChange}
                        value={field.value}
                      >
                        <FormControl className="min-w-[200px]">
                          <SelectTrigger>
                            <SelectValue placeholder="Select log level" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {logLevels.map((level) => (
                            <SelectItem key={level.value} value={level.value}>
                              {level.label}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormDescription>
                        Changing the log level requires an application restart
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="enableWebhookLogging"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                      <div className="space-y-0.5">
                        <FormLabel className="text-base">
                          Enable Webhook Logging
                        </FormLabel>
                        <FormDescription>
                          Log all incoming webhook requests for debugging and
                          monitoring
                        </FormDescription>
                      </div>
                      <FormControl>
                        <Switch
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="webhookLogRetentionDays"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Webhook Log Retention (Days)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          placeholder="0"
                          {...field}
                          onChange={(e) =>
                            field.onChange(parseInt(e.target.value) || 0)
                          }
                        />
                      </FormControl>
                      <FormDescription>
                        How many days to keep webhook logs. Set to 0 to keep
                        logs indefinitely.
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="statsDaysInterval"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Statistics Interval (Days)</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          placeholder="30"
                          {...field}
                          onChange={(e) =>
                            field.onChange(parseInt(e.target.value) || 30)
                          }
                        />
                      </FormControl>
                      <FormDescription>
                        Default number of days to show in statistics views
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {/* Notification Settings Section */}
              <div className="space-y-4 pt-6 border-t">
                <div className="space-y-2">
                  <h3 className="text-lg font-medium flex items-center gap-2">
                    <Bell className="h-5 w-5 text-yellow-600 dark:text-yellow-400" />
                    Failure Notifications
                  </h3>
                  <p className="text-sm text-muted-foreground">
                    Get notified when webhook deliveries fail
                  </p>
                </div>

                <FormField
                  control={form.control}
                  name="enableFailureNotifications"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                      <div className="space-y-0.5">
                        <FormLabel className="text-base">
                          Enable Failure Notifications
                        </FormLabel>
                        <FormDescription>
                          Send Telegram notifications when webhook deliveries
                          fail
                        </FormDescription>
                      </div>
                      <FormControl>
                        <Switch
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>
                    </FormItem>
                  )}
                />

                {form.watch("enableFailureNotifications") && (
                  <div className="space-y-4">
                    {bots.length > 0 && (
                      <div className="space-y-2">
                        <FormLabel>Quick Fill from Bot</FormLabel>
                        <Select onValueChange={handleBotSelection}>
                          <SelectTrigger>
                            <SelectValue placeholder="Select a bot to auto-fill settings" />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="manual">
                              Enter manually
                            </SelectItem>
                            {bots.map((bot) => (
                              <SelectItem
                                key={bot.id}
                                value={bot.id.toString()}
                              >
                                {bot.name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormDescription>
                          Select a bot to automatically fill in the token and
                          chat ID
                        </FormDescription>
                      </div>
                    )}

                    <FormField
                      control={form.control}
                      name="notificationBotToken"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Bot Token</FormLabel>
                          <FormControl>
                            <Input
                              type="password"
                              placeholder="Enter bot token"
                              {...field}
                            />
                          </FormControl>
                          <FormDescription>
                            Telegram bot token for sending notifications
                          </FormDescription>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="notificationChatId"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Chat ID</FormLabel>
                          <FormControl>
                            <Input placeholder="Enter chat ID" {...field} />
                          </FormControl>
                          <FormDescription>
                            Chat ID where notifications will be sent
                          </FormDescription>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="notificationTopicId"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Topic ID (Optional)</FormLabel>
                          <FormControl>
                            <Input placeholder="Enter topic ID" {...field} />
                          </FormControl>
                          <FormDescription>
                            Topic ID for forum/group chats (optional)
                          </FormDescription>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <div className="flex items-center gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        type="button"
                        onClick={handleTestNotification}
                        disabled={
                          testNotificationMutation.isPending ||
                          updateSettingsMutation.isPending ||
                          !form.formState.isValid ||
                          isMonitoringHealth
                        }
                      >
                        {testNotificationMutation.isPending ||
                        updateSettingsMutation.isPending ? (
                          "Saving & Testing..."
                        ) : (
                          <>
                            <BellOff className="h-4 w-4 mr-2" />
                            Save & Test
                          </>
                        )}
                      </Button>
                    </div>
                  </div>
                )}
              </div>

              <div className="md:flex md:justify-end pt-4 grid grid-cols-1">
                <Button
                  type="submit"
                  disabled={
                    updateSettingsMutation.isPending ||
                    !form.formState.isValid ||
                    isMonitoringHealth
                  }
                >
                  {updateSettingsMutation.isPending
                    ? "Saving..."
                    : "Save Settings"}
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>

      <ConfirmationDialog
        open={showRestartDialog}
        onOpenChange={setShowRestartDialog}
        onConfirm={handleConfirmRestart}
        title="Application Restart Required"
        confirmText="Save & Restart"
        variant="default"
      >
        <div className="space-y-3">
          <p>
            Changing the log level requires restarting the application. This
            will temporarily interrupt service for a few seconds.
          </p>
          <div className="p-3 bg-orange-50 dark:bg-orange-950/20 border border-orange-200 dark:border-orange-800 rounded-lg">
            <p className="text-sm text-orange-700 dark:text-orange-300">
              The application will automatically restart in a Docker
              environment. Health monitoring will track when the service is back
              online.
            </p>
          </div>
          <p className="text-sm text-muted-foreground">
            Do you want to continue with the restart?
          </p>
        </div>
      </ConfirmationDialog>
    </>
  );
}
