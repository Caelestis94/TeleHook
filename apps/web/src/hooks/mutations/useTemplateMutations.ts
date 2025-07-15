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

const renderTemplate = async (data: RenderTemplateRequest): Promise<RenderTemplateResponse> => {
  const response = await fetch("/api/templates/render", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  
  // Let handleApiResponse throw for actual HTTP errors (4xx, 5xx)
  const result: RenderTemplateResponse = await handleApiResponse(response);
  
  // Return the result as-is, let the component handle success:false
  return result;
};

export const useRenderTemplate = () => {
  return useMutation({
    mutationFn: renderTemplate,
    onError: (error) => {
      handleError(error, "Failed to render template");
    },
  });
};