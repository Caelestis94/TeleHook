import z from "zod";

export const BotValidationSchema = z.object({
  name: z
    .string()
    .min(1, "Name is required")
    .max(100, "Name must be 100 characters or less"),
  botToken: z
    .string()
    .min(1, "Bot token is required")
    .regex(
      /^[0-9]{9,10}:[a-zA-Z0-9_-]{35}$/,
      "Bot token must be a valid Telegram bot token"
    ),
  chatId: z
    .string()
    .min(1, "Chat ID is required")
    .regex(/^-?\d+$/, "Chat ID must be a numeric value"),
});
