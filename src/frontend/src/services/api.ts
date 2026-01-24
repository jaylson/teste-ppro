import axios, {
  AxiosError,
  InternalAxiosRequestConfig,
} from 'axios';
import { useAuthStore } from '@/stores/authStore';
import { useClientStore } from '@/stores/clientStore';

const API_URL = import.meta.env.VITE_API_URL || '/api';
// Removed unused import_meta_env declaration

export const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Flag para evitar múltiplos refreshes simultâneos
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: Error) => void;
}> = [];

const processQueue = (error: Error | null, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token!);
    }
  });
  failedQueue = [];
};

// Interceptor de request - adiciona token
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const { accessToken } = useAuthStore.getState();
    const { selectedCompanyId } = useClientStore.getState();
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    if (selectedCompanyId) {
      config.headers['X-Company-Id'] = selectedCompanyId;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor de response - refresh automático
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean;
    };

    // Se não for 401 ou já tentou retry, rejeita
    if (error.response?.status !== 401 || originalRequest._retry) {
      return Promise.reject(error);
    }

    // Se já está fazendo refresh, aguarda na fila
    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      })
        .then((token) => {
          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${token}`;
          }
          return api(originalRequest);
        })
        .catch((err) => Promise.reject(err));
    }

    originalRequest._retry = true;
    isRefreshing = true;

    const { refreshToken, setAuth, logout } = useAuthStore.getState();

    if (!refreshToken) {
      logout();
      return Promise.reject(error);
    }

    try {
      const response = await axios.post(`${API_URL}/auth/refresh`, {
        refreshToken,
      });

      const {
        accessToken: newAccessToken,
        refreshToken: newRefreshToken,
        user,
      } = response.data.data;

      setAuth(user, newAccessToken, newRefreshToken);

      processQueue(null, newAccessToken);

      if (originalRequest.headers) {
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
      }
      return api(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError as Error, null);
      logout();
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
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
  // Listar todos os clientes (billing clients)
  getAll: async (): Promise<ClientListItem[]> => {
    const response = await api.get<ClientListItem[]>('/billing/clients');
    return response.data;
  },

  // Buscar cliente por ID
  getById: async (id: string): Promise<Client> => {
    const response = await api.get<Client>(`/billing/clients/${id}`);
    return response.data;
  },

  // Criar novo cliente
  create: async (client: Omit<Client, 'id' | 'createdAt' | 'subscriptionsCount'>): Promise<Client> => {
    const response = await api.post<Client>('/billing/clients', client);
    return response.data;
  },

  // Atualizar cliente
  update: async (id: string, client: Omit<Client, 'id' | 'createdAt' | 'subscriptionsCount'>): Promise<Client> => {
    const response = await api.put<Client>(`/billing/clients/${id}`, client);
    return response.data;
  },

  // Deletar cliente
  delete: async (id: string): Promise<void> => {
    await api.delete(`/billing/clients/${id}`);
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
  dueDay?: number;
  paymentMethod?: string;
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
  dueDay?: number;
  paymentMethod?: string;
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
  create: async (subscription: { clientId: string; planId: string; startDate?: string; autoRenew?: boolean; dueDay?: number; paymentMethod?: string }): Promise<Subscription> => {
    const response = await api.post<Subscription>('/subscriptions', subscription);
    return response.data;
  },

  // Atualizar assinatura
  update: async (id: string, subscription: { planId: string; autoRenew: boolean; companiesCount: number; usersCount: number; startDate: string; endDate?: string; dueDay: number; paymentMethod: string }): Promise<Subscription> => {
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

export interface Invoice {
  id: string;
  subscriptionId: string;
  clientId: string;
  clientName: string;
  clientEmail: string;
  clientDocument: string;
  planId: string;
  planName: string;
  amount: number;
  dueDate: string;
  issueDate: string;
  paymentDate?: string;
  status: 'pending' | 'paid' | 'overdue' | 'cancelled';
  invoiceNumber: string;
  referenceMonth: number;
  referenceYear: number;
  description: string;
  notes?: string;
  pdfUrl?: string;
  createdAt: string;
}

export interface InvoiceFilters {
  clientId?: string;
  status?: string;
  startDate?: string;
  endDate?: string;
  planId?: string;
}

export const invoicesApi = {
  // Listar todas as faturas com filtros
  getAll: async (filters?: InvoiceFilters): Promise<Invoice[]> => {
    const params = new URLSearchParams();
    if (filters?.clientId) params.append('clientId', filters.clientId);
    if (filters?.status) params.append('status', filters.status);
    if (filters?.startDate) params.append('startDate', filters.startDate);
    if (filters?.endDate) params.append('endDate', filters.endDate);
    if (filters?.planId) params.append('planId', filters.planId);
    
    const response = await api.get<Invoice[]>(`/invoices?${params.toString()}`);
    return response.data;
  },

  // Buscar fatura por ID
  getById: async (id: string): Promise<Invoice> => {
    const response = await api.get<Invoice>(`/invoices/${id}`);
    return response.data;
  },

  // Buscar faturas por cliente
  getByClientId: async (clientId: string): Promise<Invoice[]> => {
    const response = await api.get<Invoice[]>(`/invoices/client/${clientId}`);
    return response.data;
  },

  // Buscar faturas por assinatura
  getBySubscriptionId: async (subscriptionId: string): Promise<Invoice[]> => {
    const response = await api.get<Invoice[]>(`/invoices/subscription/${subscriptionId}`);
    return response.data;
  },

  // Criar fatura manual
  create: async (invoice: { subscriptionId: string; dueDate: string; notes?: string }): Promise<Invoice> => {
    const response = await api.post<Invoice>('/invoices', invoice);
    return response.data;
  },

  // Marcar fatura como paga
  markAsPaid: async (id: string, paymentDate?: Date): Promise<Invoice> => {
    const response = await api.post<Invoice>(`/invoices/${id}/pay`, paymentDate ? { paymentDate: paymentDate.toISOString() } : {});
    return response.data;
  },

  // Cancelar fatura
  cancel: async (id: string): Promise<void> => {
    await api.post(`/invoices/${id}/cancel`);
  },

  // Gerar faturas do mês
  generateMonthly: async (month?: number, year?: number): Promise<{ message: string; invoicesGenerated: number }> => {
    const params = new URLSearchParams();
    if (month) params.append('month', month.toString());
    if (year) params.append('year', year.toString());
    
    const url = params.toString() ? `/invoices/generate-monthly?${params.toString()}` : '/invoices/generate-monthly';
    const response = await api.post<{ message: string; invoicesGenerated: number }>(url);
    return response.data;
  },

  // Download PDF da fatura
  downloadPdf: async (id: string): Promise<Blob> => {
    const response = await api.get(`/invoices/${id}/pdf`, {
      responseType: 'blob',
    });
    return response.data;
  },

  // Obter URL do PDF (para ferramentas externas)
  getPdfUrl: (id: string): string => {
    return `${API_URL}/invoices/${id}/pdf`;
  },

  // Obter dados de MRR
  getMrrData: async (months: number = 12): Promise<{
    monthlyData: Array<{
      year: number;
      month: number;
      monthName: string;
      revenue: number;
      invoiceCount: number;
    }>;
    currentMrr: number;
    averageMrr: number;
    growthRate: number;
  }> => {
    const response = await api.get(`/invoices/mrr?months=${months}`);
    return response.data;
  },
};

export default api;

