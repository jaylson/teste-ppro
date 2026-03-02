import { api } from './api';
import type {
  Document,
  DocumentListResponse,
  DocumentFilters,
  CreateDocumentRequest,
  UpdateDocumentMetadataRequest,
} from '@/types';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

export const documentService = {
  async list(filters?: DocumentFilters): Promise<DocumentListResponse> {
    const params: Record<string, string | number | undefined> = {};
    if (filters?.companyId) params.companyId = filters.companyId;
    if (filters?.page) params.page = filters.page;
    if (filters?.pageSize) params.pageSize = filters.pageSize;
    if (filters?.documentType) params.documentType = filters.documentType;
    if (filters?.visibility) params.visibility = filters.visibility;
    if (filters?.search) params.search = filters.search;
    const response = await api.get<ApiResponse<DocumentListResponse>>('/documents', { params });
    return response.data.data;
  },

  async getByEntity(
    companyId: string,
    entityType: string,
    entityId: string
  ): Promise<Document[]> {
    const response = await api.get<ApiResponse<Document[]>>('/documents/by-entity', {
      params: { companyId, entityType, entityId },
    });
    return response.data.data;
  },

  async getById(id: string): Promise<Document> {
    const response = await api.get<ApiResponse<Document>>(`/documents/${id}`);
    return response.data.data;
  },

  async create(data: CreateDocumentRequest): Promise<Document> {
    const response = await api.post<ApiResponse<Document>>('/documents', data);
    return response.data.data;
  },

  async updateMetadata(id: string, data: UpdateDocumentMetadataRequest): Promise<Document> {
    const response = await api.put<ApiResponse<Document>>(`/documents/${id}/metadata`, data);
    return response.data.data;
  },

  async verify(id: string): Promise<Document> {
    const response = await api.post<ApiResponse<Document>>(`/documents/${id}/verify`);
    return response.data.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/documents/${id}`);
  },
};
