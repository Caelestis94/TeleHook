import z from "zod";

export const WebhookValidationSchema = z.object({
  name: z.string().min(1, "Name is required"),
  botId: z.number().min(1, "Bot is required"),
  topicId: z.string().optional().default(""),
  messageTemplate: z.string().min(1, "Message template is required"),
  parseMode: z
    .enum(["Markdown", "HTML", "MarkdownV2"], {
      errorMap: () => ({
        message: "Parse mode must be one of: Markdown, HTML, or MarkdownV2",
      }),
    })
    .default("MarkdownV2"),
  disableWebPagePreview: z.boolean().default(true),
  disableNotification: z.boolean().default(false),
  isDisabled: z.boolean().default(false),
  payloadSample: z.string().min(1, "Payload sample is required").default("{}"),
  isProtected: z.boolean().default(false),
  secretKey: z.string().optional(),
});
