import z from "zod";

export const AppSettingsValidationSchema = z
  .object({
    logLevel: z.enum(
      ["Trace", "Debug", "Information", "Warning", "Error", "Critical", "None"],
      {
        errorMap: () => ({
          message:
            "Log level must be one of: Trace, Debug, Information, Warning, Error, Critical, None",
        }),
      }
    ),
    enableWebhookLogging: z.boolean(),
    webhookLogRetentionDays: z
      .number()
      .min(0, "Webhook log retention days must be 0 or greater")
      .max(365, "Webhook log retention days cannot exceed 365"),
    statsDaysInterval: z
      .number()
      .min(1, "Stats days interval must be at least 1")
      .max(365, "Stats days interval cannot exceed 365"),
    enableFailureNotifications: z.boolean(),
    notificationBotToken: z.string().optional(),
    notificationChatId: z.string().optional(),
    notificationTopicId: z.string().optional(),
  })
  .refine(
    (data) => {
      // If notifications are enabled, bot token and chat ID are required
      if (data.enableFailureNotifications) {
        return data.notificationBotToken && data.notificationChatId;
      }
      return true;
    },
    {
      message:
        "Bot token and chat ID are required when failure notifications are enabled",
      path: ["notificationBotToken"],
    }
  );
