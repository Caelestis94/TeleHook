/**
 * Standard API Error Response
 */
export type ApiError = {
  statusCode: number;
  message: string;
  details: string[];
  traceId: string;
  timestamp: string;
};
