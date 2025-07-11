import { useQuery } from '@tanstack/react-query';
import { handleApiResponse } from '@/lib/error-handling';
import type { WebhookLog, WebhookLogExport, LogFilters } from "@/types/log";

const fetchLogs = async (filters?: LogFilters): Promise<WebhookLog[]> => {
  // Build query params if filters are provided
  const params = new URLSearchParams();
  if (filters?.webhookId) params.append('webhookId', filters.webhookId);
  if (filters?.statusCode) params.append('statusCode', filters.statusCode);
  if (filters?.dateFrom) params.append('dateFrom', filters.dateFrom);
  if (filters?.dateTo) params.append('dateTo', filters.dateTo);
  if (filters?.searchTerm) params.append('searchTerm', filters.searchTerm);
  
  const url = `/api/webhooks/logs${params.toString() ? `?${params.toString()}` : ''}`;
  const response = await fetch(url);
  return handleApiResponse(response);
};

const fetchLogsExport = async (): Promise<WebhookLogExport[]> => {
  const response = await fetch('/api/webhooks/logs/export');
  return handleApiResponse(response);
};

export const useLogs = (filters?: LogFilters) => {
  return useQuery({
    queryKey: ['logs', filters],
    queryFn: () => fetchLogs(filters),
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
};

export const useLogsExport = () => {
  return useQuery({
    queryKey: ['logs-export'],
    queryFn: fetchLogsExport,
    enabled: false,
    staleTime: 0,
  });
};