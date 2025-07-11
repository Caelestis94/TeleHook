import { handleApiResponse } from "@/lib/error-handling";
import { ApiError } from "@/types/api-error";
import { TeleHookUser, UpdateUserRequest } from "@/types/user";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

const updateUser = async ({
  id,
  data,
}: {
  id: string;
  data: UpdateUserRequest;
}): Promise<TeleHookUser> => {
  const response = await fetch(`/api/users?id=${id}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });
  return handleApiResponse(response);
};

export const useUpdateUser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateUser,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["user"] });
      toast.success("Profile updated successfully");
    },
    onError: (error: Error | ApiError) => {
      toast.error(error.message || "Failed to update profile");
    },
  });
};
