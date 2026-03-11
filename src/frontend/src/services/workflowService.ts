import { api } from './api';
import type { Workflow, CreateWorkflowRequest } from '@/types/phase6';
import type { PagedResult } from '@/types';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

export interface WorkflowFilters {
  status?: string;
  workflowType?: string;
  priority?: string;
  page?: number;
  pageSize?: number;
}

export const workflowService = {
  async getAll(params?: WorkflowFilters): Promise<PagedResult<Workflow>> {
    const response = await api.get<ApiResponse<PagedResult<Workflow>>>('/workflows', { params });
    return response.data.data;
  },

  async getPending(): Promise<Workflow[]> {
    const response = await api.get<ApiResponse<Workflow[]>>('/workflows/pending');
    return response.data.data;
  },

  async getById(id: string): Promise<Workflow> {
    const response = await api.get<ApiResponse<Workflow>>(`/workflows/${id}`);
    return response.data.data;
  },

  async create(data: CreateWorkflowRequest): Promise<Workflow> {
    const response = await api.post<ApiResponse<Workflow>>('/workflows', data);
    return response.data.data;
  },

  async approveStep(workflowId: string, stepId: string, comments?: string): Promise<Workflow> {
    const response = await api.post<ApiResponse<Workflow>>(
      `/workflows/${workflowId}/steps/${stepId}/approve`,
      { comments }
    );
    return response.data.data;
  },

  async rejectStep(workflowId: string, stepId: string, comments: string): Promise<Workflow> {
    const response = await api.post<ApiResponse<Workflow>>(
      `/workflows/${workflowId}/steps/${stepId}/reject`,
      { comments }
    );
    return response.data.data;
  },

  async cancel(id: string, reason: string): Promise<Workflow> {
    const response = await api.post<ApiResponse<Workflow>>(`/workflows/${id}/cancel`, { reason });
    return response.data.data;
  },
};
