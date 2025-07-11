import { z } from "zod";
import { WebhookValidationSchema } from "@/validation/webhook-schema";
import type { Bot } from "@/types/bot";

/**
 * Webhook Configuration
 * Represents a webhook endpoint that forwards notifications to Telegram
 */
export type Webhook = {
  id: number;
  name: string;
  uuid: string;
  botId: number;
  topicId?: string;
  messageTemplate: string;
  parseMode: "Markdown" | "HTML" | "MarkdownV2";
  disableWebPagePreview: boolean;
  disableNotification: boolean;
  isDisabled: boolean;
  isProtected: boolean;
  createdAt: string;
  payloadSample: string;
  bot: Bot;
  secretKey?: string;
};

/**
 * Form data for creating/editing webhooks (from validation schema)
 */
export type WebhookFormData = z.infer<typeof WebhookValidationSchema>;

/**
 * Data required to create a new webhook
 */
export type CreateWebhookRequest = {
  name: string;
  botId: number;
  messageTemplate: string;
  isProtected: boolean;
  topicId?: string;
  parseMode: "Markdown" | "HTML" | "MarkdownV2";
  disableWebPagePreview: boolean;
  disableNotification: boolean;
  isDisabled: boolean;
  secretKey?: string;
  payloadSample: string;
};

/**
 * Data required to update an existing webhook
 */
export type UpdateWebhookRequest = {
  id: number;
  name: string;
  botId: number;
  parseMode: "Markdown" | "HTML" | "MarkdownV2";
  disableWebPagePreview: boolean;
  disableNotification: boolean;
  isDisabled: boolean;
  secretKey?: string;
  payloadSample: string;
  messageTemplate: string;
  isProtected: boolean;
  topicId?: string;
};

/**
 * Data required to test a webhook
 */
export type TestWebhookRequest = {
  uuid: string;
  payload: unknown;
  headers: Record<string, string>;
};

/**
 * Response from generating a new secret key
 */
export type GenerateSecretKeyResponse = {
  secretKey: string;
};
