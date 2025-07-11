import { handleApiResponse } from "@/lib/error-handling";
import { TeleHookUser } from "@/types/user";
import { useQuery } from "@tanstack/react-query";

const fetchUser = async (id: string): Promise<TeleHookUser> => {
  const response = await fetch(`/api/users?id=${id}`);
  return handleApiResponse(response);
};


export const useUser = (id: string) => {
  return useQuery({
    queryKey: ["user", id],
    queryFn: () => fetchUser(id),
    enabled: !!id,
  });
};
