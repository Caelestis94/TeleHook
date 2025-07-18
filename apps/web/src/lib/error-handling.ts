import { ApiError } from "@/types/api-error";
import { toast } from "sonner";

export class AppError extends Error {
  status: number;
  code?: string;
  details?: string[];

  constructor(
    message: string,
    status = 500,
    code?: string,
    details?: string[]
  ) {
    super(message);
    this.name = "AppError";
    this.status = status;
    this.code = code;
    this.details = details;
  }
}

let apiKeyErrorCallback: (() => void) | null = null;

export function setApiKeyErrorCallback(callback: () => void) {
  apiKeyErrorCallback = callback;
}

/**
 * Handle API response errors and convert them to AppError
 */
export async function handleApiResponse<T = unknown>(
  response: Response
): Promise<T> {
  if (!response.ok) {
    let errorData: ApiError;
    try {
      errorData = await response.json();
    } catch {
      // If response is not JSON, use status text
      throw new AppError(
        response.statusText || "An error occurred",
        response.status
      );
    }
    const message = errorData.message || `HTTP ${response.status}`;

    throw new AppError(
      message,
      response.status,
      errorData.traceId,
      errorData.details
    );
  }

  try {
    return await response.json();
  } catch {
    // If response is not JSON but was successful, return null
    return null as unknown as T;
  }
}

/**
 * Handle errors with user-friendly toast messages
 */
export function handleError(
  error: ApiError | Error | unknown,
  defaultMessage = "An unexpected error occurred"
): void {
  if (error instanceof AppError) {
    switch (error.status) {
      case 400:
        toast.error(`Bad Request: ${error.message}`);
        break;
      case 401:
        const lowerMessage = error.message.toLowerCase();
        if (
          lowerMessage.includes("invalid api key") ||
          lowerMessage.includes("api key") ||
          lowerMessage.includes("unauthorized")
        ) {
          if (apiKeyErrorCallback) {
            apiKeyErrorCallback();
            return;
          }
        }
        toast.error("Authentication required. Please log in.");
        break;
      case 403:
        toast.error("You don't have permission to perform this action.");
        break;
      case 404:
        toast.error("The requested resource was not found.");
        break;
      case 409:
        toast.error(`Conflict: ${error.message}`);
        break;
      case 422:
        toast.error(`Validation Error: ${error.message}`);
        break;
      case 429:
        toast.error("Too many requests. Please try again later.");
        break;
      case 500:
        toast.error("Server error. Please try again later.");
        break;
      default:
        toast.error(error.message || defaultMessage);
    }
  } else if (error instanceof TypeError && error.message.includes("fetch")) {
    toast.error("Network error. Please check your connection.");
  } else {
    toast.error(defaultMessage);
  }
}

/**
 * Wrapper for async operations with error handling
 */
export async function withErrorHandling<T>(
  operation: () => Promise<T>,
  errorMessage?: string
): Promise<T | null> {
  try {
    return await operation();
  } catch (error) {
    handleError(error, errorMessage);
    return null;
  }
}

/**
 * Get user-friendly error message based on error type
 */
export function getErrorMessage(error: ApiError | Error | unknown): string {
  if (error instanceof AppError) {
    return error.message;
  }

  if (error instanceof Error) {
    // Handle common error patterns
    if (error.message.includes("fetch")) {
      return "Network connection error";
    }
    if (error.message.includes("timeout")) {
      return "Request timeout - please try again";
    }
    return error.message;
  }

  return "An unexpected error occurred";
}

/**
 * Check if error is a network/connection error
 */
export function isNetworkError(error: ApiError | Error | unknown): boolean {
  return error instanceof TypeError && error.message.includes("fetch");
}

/**
 * Check if error is a validation error (400 or 422)
 */
export function isValidationError(error: ApiError | Error | unknown): boolean {
  return (
    error instanceof AppError && (error.status === 400 || error.status === 422)
  );
}
