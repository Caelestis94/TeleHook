/**
 * Webhook Request Log Entry
 * Represents a logged webhook request with processing details
 */
export type WebhookLog = {
  id: number;
  webhookId: number;
  requestId: string;
  httpMethod: string;
  requestUrl: string;
  requestHeaders: string;
  requestBody: string;
  responseStatusCode: number;
  responseBody: string;
  processingTimeMs: number;
  payloadValidated: boolean;
  validationErrors?: string;
  messageFormatted?: string;
  telegramSent: boolean;
  telegramResponse?: string;
  createdAt: string;
  webhook: {
    id: number;
    name: string;
    uuid: string;
  };
};

/**
 * Webhook Log Export Entry
 * Represents a logged webhook request for export with webhook name instead of full object
 */
export type WebhookLogExport = {
  id: number;
  webhookId: number;
  webhookName: string;
  requestId: string;
  httpMethod: string;
  requestUrl: string;
  requestHeaders?: string;
  requestBody?: string;
  responseStatusCode: number;
  responseBody?: string;
  processingTimeMs: number;
  payloadValidated: boolean;
  validationErrors?: string;
  messageFormatted?: string;
  telegramSent: boolean;
  telegramResponse?: string;
  createdAt: string;
};

/**
 * Log Filters Interface
 */
export type LogFilters = {
  webhookId?: string;
  statusCode?: string;
  dateFrom?: string;
  dateTo?: string;
  searchTerm?: string;
};
