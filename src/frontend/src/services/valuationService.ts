import { api } from './api';
import type {
  Valuation,
  ValuationListResponse,
  ValuationFilters,
  CreateValuationRequest,
  UpdateValuationRequest,
  RejectValuationRequest,
  AddValuationMethodRequest,
  ValuationMethod,
  CalculateMethodRequest,
  CalculateMethodResponse,
} from '@/types';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

// ─── Valuations ───────────────────────────────────────────────────────────────

export const valuationService = {
  async list(filters?: ValuationFilters): Promise<ValuationListResponse> {
    const params: Record<string, string | number | undefined> = {};
    if (filters?.companyId) params.companyId = filters.companyId;
    if (filters?.page) params.page = filters.page;
    if (filters?.pageSize) params.pageSize = filters.pageSize;
    if (filters?.status) params.status = filters.status;
    if (filters?.eventType) params.eventType = filters.eventType;
    const response = await api.get<ApiResponse<ValuationListResponse>>('/valuations', { params });
    return response.data.data;
  },

  async getById(id: string): Promise<Valuation> {
    const response = await api.get<ApiResponse<Valuation>>(`/valuations/${id}`);
    return response.data.data;
  },

  async create(data: CreateValuationRequest): Promise<Valuation> {
    const response = await api.post<ApiResponse<Valuation>>('/valuations', data);
    return response.data.data;
  },

  async update(id: string, data: UpdateValuationRequest): Promise<Valuation> {
    const response = await api.put<ApiResponse<Valuation>>(`/valuations/${id}`, data);
    return response.data.data;
  },

  async submit(id: string): Promise<Valuation> {
    const response = await api.post<ApiResponse<Valuation>>(`/valuations/${id}/submit`);
    return response.data.data;
  },

  async approve(id: string): Promise<Valuation> {
    const response = await api.post<ApiResponse<Valuation>>(`/valuations/${id}/approve`);
    return response.data.data;
  },

  async reject(id: string, data: RejectValuationRequest): Promise<Valuation> {
    const response = await api.post<ApiResponse<Valuation>>(`/valuations/${id}/reject`, data);
    return response.data.data;
  },

  async returnToDraft(id: string): Promise<Valuation> {
    const response = await api.post<ApiResponse<Valuation>>(`/valuations/${id}/return-to-draft`);
    return response.data.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/valuations/${id}`);
  },

  // ─── Methods ─────────────────────────────────────────────────────────────────

  async addMethod(valuationId: string, data: AddValuationMethodRequest): Promise<ValuationMethod> {
    const response = await api.post<ApiResponse<ValuationMethod>>(
      `/valuations/${valuationId}/methods`,
      data
    );
    return response.data.data;
  },

  async calculateMethod(
    valuationId: string,
    methodId: string,
    data: CalculateMethodRequest
  ): Promise<CalculateMethodResponse> {
    const response = await api.post<ApiResponse<CalculateMethodResponse>>(
      `/valuations/${valuationId}/methods/${methodId}/calculate`,
      data
    );
    return response.data.data;
  },

  async selectMethod(valuationId: string, methodId: string): Promise<Valuation> {
    const response = await api.post<ApiResponse<Valuation>>(
      `/valuations/${valuationId}/methods/${methodId}/select`
    );
    return response.data.data;
  },

  async deleteMethod(valuationId: string, methodId: string): Promise<void> {
    await api.delete(`/valuations/${valuationId}/methods/${methodId}`);
  },
};
