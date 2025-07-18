import { useQuery } from "@tanstack/react-query";
import { handleApiResponse } from "@/lib/error-handling";
import { WebhookStatsOverview } from "@/types/stats";

const fetchOverviewStats = async (): Promise<WebhookStatsOverview> => {
  const response = await fetch("/api/webhooks/stats/overview");
  return handleApiResponse(response);
};

export const useOverviewStats = () => {
  return useQuery({
    queryKey: ["overview-stats"],
    queryFn: fetchOverviewStats,
    refetchInterval: 60000, // 60 seconds
    retry: 2,
    staleTime: 30000, // 30 seconds
  });
};
