import { api } from './api';

// Tipo para resposta wrapper da API
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

export interface ShareClass {
  id: string;
  clientId: string;
  companyId: string;
  companyName: string;
  code: string;
  name: string;
  description?: string;
  hasVotingRights: boolean;
  votingWeight: number;
  liquidationPreference: number;
  participationRights: boolean;
  conversionRatio: number;
  antidilutionProtection: boolean;
  status: 'Active' | 'Inactive';
  createdAt: string;
  updatedAt: string;
}

export interface ShareClassSummary {
  id: string;
  code: string;
  name: string;
  hasVotingRights: boolean;
  status: 'Active' | 'Inactive';
}

export interface ShareClassListResponse {
  items: ShareClass[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export const shareClassService = {
  /**
   * Listar classes de ações com paginação e filtros
   */
  async getShareClasses(companyId?: string, page = 1, pageSize = 100): Promise<ShareClassListResponse> {
    const params: Record<string, string | number | undefined> = {
      page,
      pageSize,
    };
    if (companyId) params.companyId = companyId;
    
    const response = await api.get<ApiResponse<ShareClassListResponse>>('/share-classes', { params });
    return response.data.data;
  },

  /**
   * Listar classes de ações de uma empresa (resumido)
   */
  async getByCompany(companyId: string): Promise<ShareClassSummary[]> {
    const response = await api.get<ApiResponse<ShareClassSummary[]>>(`/share-classes/by-company/${companyId}`);
    return response.data.data;
  },

  /**
   * Obter classe de ações por ID
   */
  async getById(id: string): Promise<ShareClass> {
    const response = await api.get<ApiResponse<ShareClass>>(`/share-classes/${id}`);
    return response.data.data;
  },
};
