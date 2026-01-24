import { api } from './api';
import {
  Client,
  CreateClientRequest,
  UpdateClientRequest,
  ClientListResponse,
  ClientCompany,
} from '@/types';

// Tipo para resposta wrapper da API
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

export const clientService = {
  /**
   * Listar todos os clientes com paginação e busca
   */
  async getClients(params?: {
    page?: number;
    pageSize?: number;
    search?: string;
  }): Promise<ClientListResponse> {
    const response = await api.get<ApiResponse<ClientListResponse>>('/clients', { params });
    return response.data.data;
  },

  /**
   * Obter detalhes de um cliente específico
   */
  async getClientById(id: string): Promise<Client> {
    const response = await api.get<ApiResponse<Client>>(`/clients/${id}`);
    return response.data.data;
  },

  /**
   * Criar um novo cliente
   */
  async createClient(data: CreateClientRequest): Promise<Client> {
    const response = await api.post<ApiResponse<Client>>('/clients', data);
    return response.data.data;
  },

  /**
   * Atualizar um cliente existente
   */
  async updateClient(id: string, data: UpdateClientRequest): Promise<Client> {
    const response = await api.put<ApiResponse<Client>>(`/clients/${id}`, data);
    return response.data.data;
  },

  /**
   * Excluir um cliente (soft delete)
   */
  async deleteClient(id: string): Promise<void> {
    await api.delete(`/clients/${id}`);
  },

  /**
   * Ativar um cliente
   */
  async activateClient(id: string): Promise<Client> {
    const response = await api.patch<ApiResponse<Client>>(`/clients/${id}/activate`);
    return response.data.data;
  },

  /**
   * Desativar um cliente
   */
  async deactivateClient(id: string): Promise<Client> {
    const response = await api.patch<ApiResponse<Client>>(`/clients/${id}/deactivate`);
    return response.data.data;
  },

  /**
   * Suspender um cliente
   */
  async suspendClient(id: string): Promise<Client> {
    const response = await api.patch<ApiResponse<Client>>(`/clients/${id}/suspend`);
    return response.data.data;
  },

  /**
   * Listar empresas de um cliente
   */
  async getClientCompanies(id: string): Promise<ClientCompany[]> {
    const response = await api.get<ApiResponse<ClientCompany[]>>(`/clients/${id}/companies`);
    return response.data.data || [];
  },

  /**
   * Verificar se um documento já existe
   */
  async checkDocumentExists(document: string): Promise<boolean> {
    const response = await api.get<ApiResponse<boolean>>(`/clients/document/${document}/exists`);
    return response.data.data;
  },

  /**
   * Buscar cliente por documento
   */
  async getClientByDocument(document: string): Promise<Client | null> {
    try {
      const response = await api.get<ApiResponse<Client>>(`/clients/document/${document}`);
      return response.data.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },
};
