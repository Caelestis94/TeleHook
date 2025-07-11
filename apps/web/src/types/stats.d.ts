

/**
 * Indivual webhook statistics
 */
export type WebhookStats = {
  webhookId: number;
  totalRequests: number;
  successRate: number;
  avgProcessingTime: number;
  dailyStats: DailyStat[];
};
/**
 * Daily statistics for an individual webhook
 */
export type DailyStat = {
  date: Date;
  requests: number;
  successRate: number;
  avgProcessingTime: number;
};

/**
 * Overview of webhook statistics across all webhooks
 */
export type WebhookStatsOverview = {
  summary: Summary;
  topWebhooks: TopWebhook[];
  dailyTrend: DailyTrend[];
  period: string;
};

/**
 * Daily trend statistics for all webhooks
 */
export type DailyTrend = {
  date: Date;
  requests: number;
  successRate: number;
  avgProcessingTime: number;
};

/**
 * Summary statistics for webhooks
 */
export type Summary = {
  totalRequests: number;
  successRate: number;
  failedRequests: number;
  avgProcessingTime: number;
  todayRequests: number;
};

/**
 * Top webhooks based on request volume
 */
export type TopWebhook = {
  webhookId: number;
  webhookName: string;
  totalRequests: number;
  successRate: number;
};
