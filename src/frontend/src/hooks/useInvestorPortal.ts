import { useQuery } from '@tanstack/react-query';
import { investorPortalService, type PortalCommunicationFilters } from '@/services/investorPortalService';

const QUERY_KEY = ['investor-portal'];

export function useInvestorSummary() {
  return useQuery({
    queryKey: [...QUERY_KEY, 'summary'],
    queryFn: () => investorPortalService.getSummary(),
    staleTime: 60000,
  });
}

export function usePortalCommunications(params?: PortalCommunicationFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'communications', params],
    queryFn: () => investorPortalService.getCommunications(params),
    staleTime: 30000,
  });
}

export function usePortalDocuments() {
  return useQuery({
    queryKey: [...QUERY_KEY, 'documents'],
    queryFn: () => investorPortalService.getDocuments(),
    staleTime: 60000,
  });
}
