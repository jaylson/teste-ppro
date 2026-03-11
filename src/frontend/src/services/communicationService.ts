import { api } from './api';
import type { Communication, CreateCommunicationRequest } from '@/types/phase6';
import type { PagedResult } from '@/types';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

export interface CommunicationFilters {
  search?: string;
  commType?: string;
  status?: 'draft' | 'published';
  page?: number;
  pageSize?: number;
}

export const communicationService = {
  async getAll(params?: CommunicationFilters): Promise<PagedResult<Communication>> {
    const response = await api.get<ApiResponse<PagedResult<Communication>>>('/communications', { params });
    return response.data.data;
  },

  async getById(id: string): Promise<Communication> {
    const response = await api.get<ApiResponse<Communication>>(`/communications/${id}`);
    return response.data.data;
  },

  async create(data: CreateCommunicationRequest): Promise<Communication> {
    const response = await api.post<ApiResponse<Communication>>('/communications', data);
    return response.data.data;
  },

  async update(id: string, data: Partial<CreateCommunicationRequest>): Promise<Communication> {
    const response = await api.put<ApiResponse<Communication>>(`/communications/${id}`, data);
    return response.data.data;
  },

  async publish(id: string): Promise<Communication> {
    const response = await api.post<ApiResponse<Communication>>(`/communications/${id}/publish`);
    return response.data.data;
  },

  async trackView(id: string, duration?: number): Promise<void> {
    await api.post(`/communications/${id}/view`, { duration });
  },

  async remove(id: string): Promise<void> {
    await api.delete(`/communications/${id}`);
  },

  async getForPortal(limit?: number): Promise<Communication[]> {
    const response = await api.get<ApiResponse<Communication[]>>('/portal/communications', {
      params: limit ? { limit } : undefined,
    });
    return response.data.data;
  },
};
