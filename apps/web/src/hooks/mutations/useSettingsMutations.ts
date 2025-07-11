import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  handleApiResponse,
  handleError,
  isValidationError,
} from "@/lib/error-handling";
import type {
  SettingsUpdatedResponse,
  NotificationTestResult,
  AppSettingsFormData,
} from "@/types/settings";


const updateSettings = async (
  data: AppSettingsFormData
): Promise<SettingsUpdatedResponse> => {
  const response = await fetch("/api/settings", {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
  return handleApiResponse(response);
};

const checkHealth = async (): Promise<boolean> => {
  try {
    const response = await fetch("/api/health", {
      method: "GET",
    });
    return response.ok;
  } catch {
    return false;
  }
};

const testNotification = async (): Promise<NotificationTestResult> => {
  const response = await fetch("/api/settings/notification/test");
  return handleApiResponse(response);
};

export const useUpdateSettings = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateSettings,
    onSuccess: (response) => {
      queryClient.invalidateQueries({ queryKey: ["settings"] });
      toast.success("Settings updated successfully");
      return response;
    },
    onError: (error) => {
      if (!isValidationError(error)) {
        handleError(error, "Failed to update settings");
      }
    },
  });
};

export const useHealthCheck = () => {
  return useMutation({
    mutationFn: checkHealth,
    onError: () => {},
  });
};

export const useTestNotification = () => {
  return useMutation({
    mutationFn: testNotification,
    onSuccess: (result) => {
      if (result.isSuccess) {
        toast.success("Test notification sent successfully");
      } else {
        toast.error("Test notification failed, check your settings.");
      }
    },
    onError: (error) => {
      handleError(error, "Failed to send test notification");
    },
  });
};
