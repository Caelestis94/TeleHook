import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  handleApiResponse,
  handleError,
  isValidationError,
} from "@/lib/error-handling";
import type { 
  Webhook, 
  CreateWebhookRequest, 
  UpdateWebhookRequest, 
  TestWebhookRequest,
  GenerateSecretKeyResponse
} from "@/types/webhook";

const createWebhook = async (data: CreateWebhookRequest): Promise<Webhook> => {
  const response = await fetch("/api/webhooks", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
  return handleApiResponse(response);
};

const updateWebhook = async ({
  id,
  data,
}: {
  id: number;
  data: UpdateWebhookRequest;
}): Promise<Webhook> => {
  const response = await fetch(`/api/webhooks/${id}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
  return handleApiResponse(response);
};

const deleteWebhook = async (id: number): Promise<void> => {
  const response = await fetch(`/api/webhooks/${id}`, {
    method: "DELETE",
  });
  return handleApiResponse(response);
};

const testWebhook = async ({
  uuid,
  payload,
  headers,
}: TestWebhookRequest): Promise<void> => {
  const response = await fetch(`/api/trigger/${uuid}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...headers,
    },
    body: JSON.stringify(payload),
  });
  return handleApiResponse(response);
};

const generateSecretKey = async (): Promise<GenerateSecretKeyResponse> => {
  const response = await fetch("/api/webhooks/generate-key/new", {
    method: "POST",
  });
  return handleApiResponse(response);
};

export const useCreateWebhook = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createWebhook,
    onSuccess: () => {
      // Invalidate and refetch webhooks list
      queryClient.invalidateQueries({ queryKey: ["webhooks"] });
      toast.success(`Webhook created successfully`);
    },
    onError: (error) => {
      if (!isValidationError(error)) {
        handleError(error, "Failed to create webhook");
      }
    },
  });
};

export const useUpdateWebhook = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateWebhook,
    onSuccess: (updatedWebhook) => {
      // Invalidate and refetch webhooks list
      queryClient.invalidateQueries({ queryKey: ["webhooks"] });
      // Invalidate specific webhook
      queryClient.invalidateQueries({
        queryKey: ["webhooks", updatedWebhook.id],
      });
      toast.success(`Webhook updated successfully`);
    },
    onError: (error) => {
      if (!isValidationError(error)) {
        handleError(error, "Failed to update webhook");
      }
    },
  });
};

export const useDeleteWebhook = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteWebhook,
    onSuccess: (_, deletedWebhookId) => {
      // Invalidate and refetch webhooks list
      queryClient.invalidateQueries({ queryKey: ["webhooks"] });
      // Remove the specific webhook from cache
      queryClient.removeQueries({ queryKey: ["webhooks", deletedWebhookId] });
      toast.success("Webhook deleted successfully");
    },
    onError: (error) => {
      handleError(error, "Failed to delete webhook");
    },
  });
};

export const useTestWebhook = () => {
  return useMutation({
    mutationFn: testWebhook,
    onSuccess: () => {
      toast.success("Test message sent using template sample payload");
    },
    onError: (error) => {
      handleError(error, "Failed to send test message");
    },
  });
};

export const useGenerateSecretKey = () => {
  return useMutation({
    mutationFn: generateSecretKey,
    onError: (error) => {
      handleError(error, "Failed to generate secret key");
    },
  });
};
