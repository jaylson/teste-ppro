import { api } from './api';
import type { InvestorSummary, Communication } from '@/types/phase6';
import type { Document, PagedResult } from '@/types';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

export interface PortalCommunicationFilters {
  page?: number;
  pageSize?: number;
}

export const investorPortalService = {
  async getSummary(): Promise<InvestorSummary> {
    const response = await api.get<ApiResponse<InvestorSummary>>('/portal/summary');
    return response.data.data;
  },

  async getCommunications(params?: PortalCommunicationFilters): Promise<PagedResult<Communication>> {
    const response = await api.get<ApiResponse<PagedResult<Communication>>>('/portal/communications', { params });
    return response.data.data;
  },

  async getDocuments(): Promise<Document[]> {
    const response = await api.get<ApiResponse<Document[]>>('/portal/documents');
    return response.data.data;
  },
};
