import { api } from './api';
import type {
  CapTable,
  CapTableSummaryByType,
  CapTableSummaryByClass,
} from '@/types';

// Tipo para resposta wrapper da API
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

export const capTableService = {
  /**
   * Obtém o Cap Table completo de uma empresa
   * Inclui lista de sócios, ações, percentuais e resumos
   */
  async getCapTable(companyId: string): Promise<CapTable> {
    const response = await api.get<ApiResponse<CapTable>>(`/cap-table/${companyId}`);
    return response.data.data;
  },

  /**
   * Obtém o resumo do Cap Table por tipo de sócio
   * (Founders, Investors, Employees, Advisors, etc.)
   */
  async getSummaryByType(companyId: string): Promise<CapTableSummaryByType[]> {
    const response = await api.get<ApiResponse<CapTableSummaryByType[]>>(`/cap-table/${companyId}/summary-by-type`);
    return response.data.data;
  },

  /**
   * Obtém o resumo do Cap Table por classe de ações
   * (Common, Preferred Series A, Preferred Series B, etc.)
   */
  async getSummaryByClass(companyId: string): Promise<CapTableSummaryByClass[]> {
    const response = await api.get<ApiResponse<CapTableSummaryByClass[]>>(`/cap-table/${companyId}/summary-by-class`);
    return response.data.data;
  },
};
