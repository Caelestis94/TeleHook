import z from "zod";
import { BotValidationSchema } from "@/validation/bot-schema";

/**
 * Telegram Bot Configuration
 * Represents a Telegram bot that can receive webhook notifications
 */
export type Bot = {
  id: number;
  name: string;
  botToken: string;
  chatId: string;
  hasPassedTest: boolean;
  createdAt: string;
};

/**
 * Response from bot test operation
 */
export type BotTestResult = {
  isSuccess: boolean;
  error?: string;
};

/**
 * Form data for creating/editing bots (from validation schema)
 */
export type BotFormData = z.infer<typeof BotValidationSchema>;
