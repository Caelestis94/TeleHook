import { useQuery } from "@tanstack/react-query";
import { handleApiResponse } from "@/lib/error-handling";

const fetchSetupStatus = async (): Promise<boolean> => {
  const response = await fetch("/api/users/setup-required");
  return handleApiResponse(response);
};

export const useSetupStatus = () => {
  return useQuery({
    queryKey: ["setup-status"],
    queryFn: fetchSetupStatus,
    retry: 1,
    staleTime: 0,
  });
};
