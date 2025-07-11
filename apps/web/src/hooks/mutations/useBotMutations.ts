import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  handleApiResponse,
  handleError,
  isValidationError,
} from "@/lib/error-handling";
import { toast } from "sonner";
import type { 
  Bot, 
  BotFormData, 
  BotTestResult
} from "@/types/bot";

const createBot = async (data: BotFormData): Promise<Bot> => {
  const response = await fetch("/api/bots", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleApiResponse(response);
};

const updateBot = async ({
  id,
  data,
}: {
  id: number;
  data: BotFormData;
}): Promise<Bot> => {
  const response = await fetch(`/api/bots/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ ...data, id }),
  });
  return handleApiResponse(response);
};

const deleteBot = async (id: number): Promise<void> => {
  const response = await fetch(`/api/bots/${id}`, {
    method: "DELETE",
  });
  return handleApiResponse(response);
};

const testBot = async (id: number): Promise<BotTestResult> => {
  const response = await fetch(`/api/bots/${id}/test`);
  return handleApiResponse(response);
};

export const useCreateBot = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createBot,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["bots"] });
      toast.success("Telegram bot created successfully");
    },
    onError: (error) => {
      if (!isValidationError(error)) {
        handleError(error, "Failed to create Telegram bot");
      }
    },
  });
};

export const useUpdateBot = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateBot,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["bots"] });
      toast.success("Telegram bot updated successfully");
    },
    onError: (error) => {
      if (!isValidationError(error)) {
        handleError(error, "Failed to update Telegram bot");
      }
    },
  });
};

export const useDeleteBot = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteBot,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["bots"] });
      toast.success("Telegram bot deleted successfully");
    },
    onError: (error) => {
      handleError(error, "Failed to delete Telegram bot");
    },
  });
};

export const useTestBot = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: testBot,
    onSuccess: (result, id) => {
      if (result.isSuccess) {
        // Update the specific bot's hasPassedTest status to true
        queryClient.setQueryData(["bots"], (oldBots: Bot[] | undefined) => {
          if (!oldBots) return oldBots;
          return oldBots.map((bot) =>
            bot.id === id ? { ...bot, hasPassedTest: true } : bot
          );
        });
        toast.success(`Telegram bot test passed successfully`);
      } else {
        // Update the specific bot's hasPassedTest status to false
        queryClient.setQueryData(["bots"], (oldBots: Bot[] | undefined) => {
          if (!oldBots) return oldBots;
          return oldBots.map((bot) =>
            bot.id === id ? { ...bot, hasPassedTest: false } : bot
          );
        });
        toast.error("Bot test failed, invalid token?");
      }
    },
    onError: (error, id) => {
      // Update the specific bot's hasPassedTest status to false
      queryClient.setQueryData(["bots"], (oldBots: Bot[] | undefined) => {
        if (!oldBots) return oldBots;
        return oldBots.map((bot) =>
          bot.id === id ? { ...bot, hasPassedTest: false } : bot
        );
      });
      handleError(error, "Failed to test bot connection");
    },
  });
};
