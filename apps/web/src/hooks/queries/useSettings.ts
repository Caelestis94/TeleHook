import { useQuery } from "@tanstack/react-query";
import { handleApiResponse } from "@/lib/error-handling";
import type { AppSetting } from "@/types/settings";

const fetchSettings = async (): Promise<AppSetting> => {
  const response = await fetch("/api/settings");
  return handleApiResponse(response);
};

export const useSettings = () => {
  return useQuery({
    queryKey: ["settings"],
    queryFn: fetchSettings,
  });
};
