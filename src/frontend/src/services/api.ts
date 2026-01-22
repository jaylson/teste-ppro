import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || '/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para adicionar token JWT
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor para tratar erros
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export interface Client {
  id?: string;
  name: string;
  email: string;
  document: string;
  type: 'individual' | 'company';
  status: 'active' | 'suspended' | 'cancelled';
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  createdAt?: string;
  subscriptionsCount?: number;
}

export interface ClientListItem {
  id: string;
  name: string;
  email: string;
  document: string;
  type: 'individual' | 'company';
  status: 'active' | 'suspended' | 'cancelled';
  createdAt: string;
  subscriptionsCount: number;
}

export const clientsApi = {
  // Listar todos os clientes
  getAll: async (): Promise<ClientListItem[]> => {
    const response = await api.get<ClientListItem[]>('/clients');
    return response.data;
  },

  // Buscar cliente por ID
  getById: async (id: string): Promise<Client> => {
    const response = await api.get<Client>(`/clients/${id}`);
    return response.data;
  },

  // Criar novo cliente
  create: async (client: Omit<Client, 'id' | 'createdAt' | 'subscriptionsCount'>): Promise<Client> => {
    const response = await api.post<Client>('/clients', client);
    return response.data;
  },

  // Atualizar cliente
  update: async (id: string, client: Omit<Client, 'id' | 'createdAt' | 'subscriptionsCount'>): Promise<Client> => {
    const response = await api.put<Client>(`/clients/${id}`, client);
    return response.data;
  },

  // Deletar cliente
  delete: async (id: string): Promise<void> => {
    await api.delete(`/clients/${id}`);
  },
};

export interface Subscription {
  id?: string;
  clientId: string;
  clientName?: string;
  clientEmail?: string;
  planId: string;
  planName?: string;
  planPrice?: number;
  billingCycle?: 'monthly' | 'yearly';
  status: 'pending' | 'active' | 'suspended' | 'cancelled';
  startDate: string;
  endDate?: string;
  autoRenew: boolean;
  companiesCount: number;
  usersCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface SubscriptionListItem {
  id: string;
  clientId: string;
  clientName: string;
  planId: string;
  planName: string;
  planPrice: number;
  billingCycle: 'monthly' | 'yearly';
  status: 'pending' | 'active' | 'suspended' | 'cancelled';
  startDate: string;
  endDate?: string;
  autoRenew: boolean;
  companiesCount: number;
  usersCount: number;
  createdAt: string;
}

export const subscriptionsApi = {
  // Listar todas as assinaturas
  getAll: async (): Promise<SubscriptionListItem[]> => {
    const response = await api.get<SubscriptionListItem[]>('/subscriptions');
    return response.data;
  },

  // Buscar assinatura por ID
  getById: async (id: string): Promise<Subscription> => {
    const response = await api.get<Subscription>(`/subscriptions/${id}`);
    return response.data;
  },

  // Buscar assinaturas por cliente
  getByClientId: async (clientId: string): Promise<SubscriptionListItem[]> => {
    const response = await api.get<SubscriptionListItem[]>(`/subscriptions/client/${clientId}`);
    return response.data;
  },

  // Criar nova assinatura
  create: async (subscription: { clientId: string; planId: string; startDate?: string; autoRenew?: boolean }): Promise<Subscription> => {
    const response = await api.post<Subscription>('/subscriptions', subscription);
    return response.data;
  },

  // Atualizar assinatura
  update: async (id: string, subscription: { planId: string; autoRenew: boolean; companiesCount: number; usersCount: number; startDate: string; endDate?: string }): Promise<Subscription> => {
    const response = await api.put<Subscription>(`/subscriptions/${id}`, subscription);
    return response.data;
  },

  // Ativar assinatura
  activate: async (id: string): Promise<void> => {
    await api.post(`/subscriptions/${id}/activate`);
  },

  // Suspender assinatura
  suspend: async (id: string): Promise<void> => {
    await api.post(`/subscriptions/${id}/suspend`);
  },

  // Cancelar assinatura
  cancel: async (id: string): Promise<void> => {
    await api.post(`/subscriptions/${id}/cancel`);
  },

  // Deletar assinatura
  delete: async (id: string): Promise<void> => {
    await api.delete(`/subscriptions/${id}`);
  },
};

export interface Plan {
  id?: string;
  name: string;
  description: string;
  price: number;
  billingCycle: 'monthly' | 'yearly';
  maxCompanies: number;
  maxUsers: number;
  features?: string[];
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export const plansApi = {
  // Listar todos os planos
  getAll: async (): Promise<Plan[]> => {
    const response = await api.get<Plan[]>('/plans');
    return response.data;
  },

  // Listar apenas planos ativos
  getActive: async (): Promise<Plan[]> => {
    const response = await api.get<Plan[]>('/plans/active');
    return response.data;
  },

  // Buscar plano por ID
  getById: async (id: string): Promise<Plan> => {
    const response = await api.get<Plan>(`/plans/${id}`);
    return response.data;
  },

  // Criar novo plano
  create: async (plan: Omit<Plan, 'id' | 'createdAt' | 'updatedAt'>): Promise<Plan> => {
    const response = await api.post<Plan>('/plans', plan);
    return response.data;
  },

  // Atualizar plano
  update: async (id: string, plan: Omit<Plan, 'id' | 'createdAt' | 'updatedAt'>): Promise<Plan> => {
    const response = await api.put<Plan>(`/plans/${id}`, plan);
    return response.data;
  },

  // Ativar/Desativar plano
  toggleStatus: async (id: string): Promise<{ message: string; isActive: boolean }> => {
    const response = await api.patch<{ message: string; isActive: boolean }>(`/plans/${id}/toggle-status`);
    return response.data;
  },

  // Deletar plano
  delete: async (id: string): Promise<void> => {
    await api.delete(`/plans/${id}`);
  },
};

export default api;
