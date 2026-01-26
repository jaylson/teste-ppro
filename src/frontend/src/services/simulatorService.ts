import { api } from './api';
import type {
  RoundSimulationRequest,
  RoundSimulationResponse,
  DilutionResponse,
  ScenarioCompareRequest,
  ScenarioCompareResponse,
} from '@/types';

// Tipo para resposta wrapper da API
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

export const simulatorService = {
  /**
   * Simula uma rodada de investimento
   * Calcula o impacto no cap table, incluindo diluição e novas participações
   */
  async simulateRound(request: RoundSimulationRequest): Promise<RoundSimulationResponse> {
    const response = await api.post<ApiResponse<RoundSimulationResponse>>('/simulator/round', request);
    return response.data.data;
  },

  /**
   * Calcula a diluição de uma rodada de forma rápida
   * Útil para cálculos simples sem detalhes do cap table
   */
  async calculateDilution(
    companyId: string,
    investmentAmount: number,
    preMoneyValuation: number
  ): Promise<DilutionResponse> {
    const response = await api.get<ApiResponse<DilutionResponse>>('/simulator/dilution', {
      params: {
        companyId,
        investmentAmount,
        preMoneyValuation,
      },
    });
    return response.data.data;
  },

  /**
   * Compara múltiplos cenários de rodadas
   * Permite simular até 5 cenários diferentes de uma vez
   */
  async compareScenarios(request: ScenarioCompareRequest): Promise<ScenarioCompareResponse> {
    const response = await api.post<ApiResponse<ScenarioCompareResponse>>('/simulator/scenarios', request);
    return response.data.data;
  },
};
