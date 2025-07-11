import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { handleApiResponse } from "@/lib/error-handling";
import {
  CaptureSession,
  StartCaptureSessionRequest,
} from "@/types/capture-session";
import { ApiError } from "@/types/api-error";


const startCapture = async (
  data: StartCaptureSessionRequest
): Promise<CaptureSession> => {
  const response = await fetch("/api/payloads/capture/start", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
  return handleApiResponse(response);
};

const getStatus = async (sessionId: string): Promise<CaptureSession> => {
  const response = await fetch(`/api/payloads/capture/status/${sessionId}`);
  return handleApiResponse(response);
};

const cancelCapture = async (
  sessionId: string
): Promise<CaptureSession | null> => {
  const response = await fetch(`/api/payloads/capture/cancel/${sessionId}`, {
    method: "GET",
  });
  return handleApiResponse(response);
};

export const useStartCapture = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: startCapture,
    onSuccess: (session) => {
      // Cache the session immediately
      queryClient.setQueryData(["payloadCapture", session.sessionId], session);
      toast.success("Payload capture session started");
    },
    onError: (error: ApiError | Error) => {
      toast.error(error.message || "Failed to start payload capture");
    },
  });
};

export const useCaptureStatus = (sessionId: string | null, enabled = true) => {
  return useQuery({
    queryKey: ["payloadCapture", sessionId],
    queryFn: () => getStatus(sessionId!),
    enabled: enabled && !!sessionId,
    refetchInterval: (query) => {
      const data = query.state.data as CaptureSession | undefined;

      if (!data) {
        return 2000;
      }
      if (data.status === "Completed" || data.status === "Cancelled") {
        return false;
      }

      if (new Date(data.expiresAt) < new Date()) {
        return false;
      }

      // Poll every 2 seconds
      return 2000;
    },
    staleTime: 0,
  });
};

export const useCancelCapture = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: cancelCapture,
    retry: false,
    onSuccess: (session, sessionId) => {
      // If API returns the session object, cache it
      if (session) {
        queryClient.setQueryData(
          ["payloadCapture", session.sessionId],
          session
        );
        queryClient.invalidateQueries({
          queryKey: ["payloadCapture", session.sessionId],
        });
      } else {
        // If API returns null, invalidate using the sessionId we passed
        queryClient.invalidateQueries({
          queryKey: ["payloadCapture", sessionId],
        });
      }
      toast.success("Payload capture cancelled");
    },
    onError: (error: ApiError | Error) => {
      toast.error(error.message || "Failed to cancel payload capture");
    },
  });
};
