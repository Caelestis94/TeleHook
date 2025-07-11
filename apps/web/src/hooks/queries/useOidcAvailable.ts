import { useQuery } from '@tanstack/react-query';
import { handleApiResponse } from '@/lib/error-handling';

interface OidcAvailableResponse {
  isOidcConfigured: boolean;
}

const fetchOidcAvailable = async (): Promise<OidcAvailableResponse> => {
  const response = await fetch('/api/auth/oidc-available');
  return handleApiResponse(response);
};

export const useOidcAvailable = () => {
  return useQuery({
    queryKey: ['oidc-available'],
    queryFn: fetchOidcAvailable,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};