import { useQuery } from "@tanstack/react-query";
import { handleApiResponse } from "@/lib/error-handling";
import { Webhook } from "@/types/webhook";

const fetchWebhooks = async (): Promise<Webhook[]> => {
  const response = await fetch("/api/webhooks");
  return handleApiResponse(response);
};

const fetchWebhook = async (id: number): Promise<Webhook> => {
  const response = await fetch(`/api/webhooks/${id}`);
  return handleApiResponse(response);
};

export const useWebhooks = () => {
  return useQuery({
    queryKey: ["webhooks"],
    queryFn: fetchWebhooks,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

export const useWebhook = (id: number) => {
  return useQuery({
    queryKey: ["webhooks", id],
    queryFn: () => fetchWebhook(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};
