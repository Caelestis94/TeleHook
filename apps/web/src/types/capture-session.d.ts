/**
 * Payload capture session status
 */
export type CaptureSessionStatus = "Waiting" | "Completed" | "Cancelled";

/**
 * Capture session entity (from backend)
 */
export type CaptureSession = {
  sessionId: string;
  captureUrl: string;
  status: CaptureSessionStatus;
  createdAt: string;
  expiresAt: string;
  payload?: unknown;
};

/**
 * Request data for starting a capture session
 */
export type StartCaptureSessionRequest = {
  userId: number;
};
