import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { simulatorService } from '@/services/simulatorService';
import type { RoundSimulationRequest, ScenarioCompareRequest } from '@/types';
import toast from 'react-hot-toast';

// ============================================================================
// QUERY KEYS
// ============================================================================

const SIMULATOR_KEY = ['simulator'];
const DILUTION_KEY = ['dilution'];

// ============================================================================
// SIMULATION HOOKS
// ============================================================================

/**
 * Hook para simular uma rodada de investimento
 */
export function useSimulateRound() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: RoundSimulationRequest) => simulatorService.simulateRound(request),
    onSuccess: () => {
      // Não invalidamos o cap table aqui pois é apenas simulação
      queryClient.invalidateQueries({ queryKey: SIMULATOR_KEY });
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Erro ao simular rodada');
    },
  });
}

/**
 * Hook para calcular diluição rapidamente
 */
export function useDilutionCalculation(
  companyId: string | undefined,
  investmentAmount: number | undefined,
  preMoneyValuation: number | undefined,
  enabled = true
) {
  return useQuery({
    queryKey: [...DILUTION_KEY, companyId, investmentAmount, preMoneyValuation],
    queryFn: () => simulatorService.calculateDilution(companyId!, investmentAmount!, preMoneyValuation!),
    enabled: enabled && !!companyId && !!investmentAmount && !!preMoneyValuation && investmentAmount > 0 && preMoneyValuation > 0,
    staleTime: 60000, // 1 minuto - cálculos de diluição são determinísticos
  });
}

/**
 * Hook para comparar múltiplos cenários
 */
export function useCompareScenarios() {
  return useMutation({
    mutationFn: (request: ScenarioCompareRequest) => simulatorService.compareScenarios(request),
    onError: (error: Error) => {
      toast.error(error.message || 'Erro ao comparar cenários');
    },
  });
}

// ============================================================================
// UTILITY HOOKS
// ============================================================================

/**
 * Hook que retorna se uma simulação está em andamento
 */
export function useIsSimulating() {
  const simulateRound = useSimulateRound();
  const compareScenarios = useCompareScenarios();

  return simulateRound.isPending || compareScenarios.isPending;
}
