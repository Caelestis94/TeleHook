import { ApiError } from "@/types/api-error";
import { TeleHookUser } from "@/types/user";
import { useMutation } from "@tanstack/react-query";

interface SetupData {
  email: string;
  username: string;
  password: string;
  firstName: string;
  lastName: string;
}

const setupAdmin = async (data: SetupData): Promise<TeleHookUser> => {
  const response = await fetch("/api/setup", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    throw {
      status: response.status,
      statusText: response.statusText,
      details: errorData.details || [],
      message: errorData.message || "Setup failed",
    };
  }

  return response.json();
};

export const useSetupAdmin = () => {
  return useMutation({
    mutationFn: setupAdmin,
    onError: (_: Error | ApiError) => {},
  });
};
