import { z } from "zod";
import { AppSettingsValidationSchema } from "@/validation/setting-schema";

/**
 * Log level enum matching backend values
 */
export type LogLevel =
  | "Trace"
  | "Debug"
  | "Information"
  | "Warning"
  | "Error"
  | "Critical"
  | "None";

/**
 * Settings entity from the backend
 */
export type AppSetting = {
  id: number;
  logLevel: LogLevel;
  logPath: string;
  logRetentionDays: number;
  enableWebhookLogging: boolean;
  webhookLogRetentionDays: number;
  statsDaysInterval: number;
  enableFailureNotifications: boolean;
  notificationBotToken?: string;
  notificationChatId?: string;
  notificationTopicId?: string;
  additionalSettings?: string;
  createdAt: string;
  updatedAt: string;
};

/**
 * Request data for updating app settings
 */
export type UpdateAppSettingsRequest = {
  logLevel: LogLevel;
  logPath: string;
  logRetentionDays: number;
  enableWebhookLogging: boolean;
  webhookLogRetentionDays: number;
  statsDaysInterval: number;
  enableFailureNotifications: boolean;
  notificationBotToken?: string;
  notificationChatId?: string;
  notificationTopicId?: string;
};

/**
 * Response type for settings update
 */
export type SettingsUpdatedResponse = {
  isRestartRequired: boolean;
  settings: AppSetting;
};

/**
 * Response type for notification test
 */
export type NotificationTestResult = {
  error: string;
  isSuccess: boolean;
};

/**
 * Form data for app settings (from validation schema)
 */
export type AppSettingsFormData = z.infer<typeof AppSettingsValidationSchema>;
