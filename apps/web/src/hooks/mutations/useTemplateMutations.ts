import { useMutation } from "@tanstack/react-query";
import { handleApiResponse, handleError } from "@/lib/error-handling";

interface RenderTemplateRequest {
  template: string;
  sampleData: Record<string, unknown>;
}

interface RenderTemplateResponse {
  success: boolean;
  rendered: string;
  errors?: string[];
}

const renderTemplate = async (data: RenderTemplateRequest): Promise<string> => {
  const response = await fetch("/api/templates/render", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  
  const result: RenderTemplateResponse = await handleApiResponse(response);
  
  if (!result.success) {
    const errorMessage = result.errors?.join(", ") || "Failed to render template";
    throw new Error(errorMessage);
  }
  
  return result.rendered;
};

export const useRenderTemplate = () => {
  return useMutation({
    mutationFn: renderTemplate,
    onError: (error) => {
      handleError(error, "Failed to render template");
    },
  });
};