import { useQuery } from "@tanstack/react-query";
import { handleApiResponse } from "@/lib/error-handling";
import type { Bot } from "@/types/bot";
import type { Webhook } from "@/types/webhook";

const fetchBots = async (): Promise<Bot[]> => {
  const response = await fetch("/api/bots");
  return handleApiResponse(response);
};

const fetchBot = async (id: number): Promise<Bot> => {
  const response = await fetch(`/api/bots/${id}`);
  return handleApiResponse(response);
};

const fetchBotWebhooks = async (botId: number): Promise<Webhook[]> => {
  const response = await fetch(`/api/bots/${botId}/webhooks`);
  return handleApiResponse(response);
};

export const useBots = () => {
  return useQuery({
    queryKey: ["bots"],
    queryFn: fetchBots,
  });
};

export const useBot = (id: number) => {
  return useQuery({
    queryKey: ["bots", id],
    queryFn: () => fetchBot(id),
    enabled: !!id,
  });
};

export const useBotWebhooks = (botId: number) => {
  return useQuery({
    queryKey: ["bots", botId, "webhooks"],
    queryFn: () => fetchBotWebhooks(botId),
    enabled: !!botId,
  });
};
