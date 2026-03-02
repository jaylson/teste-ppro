import { api } from './api';
import type {
  CustomFormula,
  CustomFormulaListResponse,
  CustomFormulaFilters,
  CreateCustomFormulaRequest,
  UpdateFormulaMetadataRequest,
  PublishNewFormulaVersionRequest,
  FormulaVersion,
  TestFormulaRequest,
  TestFormulaResponse,
} from '@/types';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

export const customFormulaService = {
  async list(filters?: CustomFormulaFilters): Promise<CustomFormulaListResponse> {
    const params: Record<string, string | number | boolean | undefined> = {};
    if (filters?.companyId) params.companyId = filters.companyId;
    if (filters?.page) params.page = filters.page;
    if (filters?.pageSize) params.pageSize = filters.pageSize;
    if (filters?.isActive !== undefined) params.isActive = filters.isActive;
    if (filters?.sectorTag) params.sectorTag = filters.sectorTag;
    const response = await api.get<ApiResponse<CustomFormulaListResponse>>('/custom-formulas', {
      params,
    });
    return response.data.data;
  },

  async getById(id: string): Promise<CustomFormula> {
    const response = await api.get<ApiResponse<CustomFormula>>(`/custom-formulas/${id}`);
    return response.data.data;
  },

  async getVersions(id: string): Promise<FormulaVersion[]> {
    const response = await api.get<ApiResponse<FormulaVersion[]>>(`/custom-formulas/${id}/versions`);
    return response.data.data;
  },

  async create(data: CreateCustomFormulaRequest): Promise<CustomFormula> {
    const response = await api.post<ApiResponse<CustomFormula>>('/custom-formulas', data);
    return response.data.data;
  },

  async updateMetadata(id: string, data: UpdateFormulaMetadataRequest): Promise<CustomFormula> {
    const response = await api.put<ApiResponse<CustomFormula>>(
      `/custom-formulas/${id}/metadata`,
      data
    );
    return response.data.data;
  },

  async publishVersion(
    id: string,
    data: PublishNewFormulaVersionRequest
  ): Promise<CustomFormula> {
    const response = await api.post<ApiResponse<CustomFormula>>(
      `/custom-formulas/${id}/versions`,
      data
    );
    return response.data.data;
  },

  async activate(id: string): Promise<CustomFormula> {
    const response = await api.post<ApiResponse<CustomFormula>>(`/custom-formulas/${id}/activate`);
    return response.data.data;
  },

  async deactivate(id: string): Promise<CustomFormula> {
    const response = await api.post<ApiResponse<CustomFormula>>(
      `/custom-formulas/${id}/deactivate`
    );
    return response.data.data;
  },

  async test(data: TestFormulaRequest): Promise<TestFormulaResponse> {
    const response = await api.post<ApiResponse<TestFormulaResponse>>(
      '/custom-formulas/test',
      data
    );
    return response.data.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/custom-formulas/${id}`);
  },
};
