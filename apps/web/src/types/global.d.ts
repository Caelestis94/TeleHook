import { ApiError } from "./api-error";

declare global {
  interface Window {
    setWebhookFormErrors?: (error: ApiError | unknown) => void;
  }
}
