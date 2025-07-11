import { QueryClient } from "@tanstack/react-query";
import { handleError } from "./error-handling";

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      gcTime: 10 * 60 * 1000, // 10 minutes
      retry: 0,
      refetchOnWindowFocus: false,
      refetchOnReconnect: true,
    },
    mutations: {
      retry: (failureCount, error) => {
        if (failureCount >= 1) return false;

        if (error instanceof Error && "status" in error) {
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          const status = (error as any).status;
          if (status >= 400 && status < 500) return false;
        }

        // Retry on network errors and server errors (5xx)
        return true;
      },
      throwOnError: true, // This ensures mutation errors are thrown
      onError: (error) => {
        handleError(error);
      },
    },
  },
});
