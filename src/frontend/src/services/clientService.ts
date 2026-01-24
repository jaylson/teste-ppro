import { api } from './api';
import {
  Client,
  CreateClientRequest,
  UpdateClientRequest,
  ClientListResponse,
  ClientCompany,
} from '@/types';

export const clientService = {
  /**
   * Listar todos os clientes com paginação e busca
   */
  async getClients(params?: {
    page?: number;
    pageSize?: number;
    search?: string;
  }): Promise<ClientListResponse> {
    const response = await api.get<ClientListResponse>('/clients', { params });
    return response.data;
  },

  /**
   * Obter detalhes de um cliente específico
   */
  async getClientById(id: string): Promise<Client> {
    const response = await api.get<Client>(`/clients/${id}`);
    return response.data;
  },

  /**
   * Criar um novo cliente
   */
  async createClient(data: CreateClientRequest): Promise<Client> {
    const response = await api.post<Client>('/clients', data);
    return response.data;
  },

  /**
   * Atualizar um cliente existente
   */
  async updateClient(id: string, data: UpdateClientRequest): Promise<Client> {
    const response = await api.put<Client>(`/clients/${id}`, data);
    return response.data;
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
    const response = await api.patch<Client>(`/clients/${id}/activate`);
    return response.data;
  },

  /**
   * Desativar um cliente
   */
  async deactivateClient(id: string): Promise<Client> {
    const response = await api.patch<Client>(`/clients/${id}/deactivate`);
    return response.data;
  },

  /**
   * Suspender um cliente
   */
  async suspendClient(id: string): Promise<Client> {
    const response = await api.patch<Client>(`/clients/${id}/suspend`);
    return response.data;
  },

  /**
   * Listar empresas de um cliente
   */
  async getClientCompanies(id: string): Promise<ClientCompany[]> {
    const response = await api.get<ClientCompany[]>(`/clients/${id}/companies`);
    return response.data;
  },

  /**
   * Verificar se um documento já existe
   */
  async checkDocumentExists(document: string): Promise<boolean> {
    const response = await api.get<boolean>(`/clients/document/${document}/exists`);
    return response.data;
  },

  /**
   * Buscar cliente por documento
   */
  async getClientByDocument(document: string): Promise<Client | null> {
    try {
      const response = await api.get<Client>(`/clients/document/${document}`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },
};
