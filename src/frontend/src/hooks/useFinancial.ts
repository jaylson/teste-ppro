import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { financialService } from '@/services/financialService';
import type {
  FinancialFilters,
  CreateFinancialPeriodRequest,
  UpdateFinancialPeriodRequest,
  UpsertRevenueRequest,
  UpsertCashBurnRequest,
  UpsertUnitEconomicsRequest,
  UpsertProfitabilityRequest,
} from '@/types';
import toast from 'react-hot-toast';

const QUERY_KEY = ['financial'];

export function useFinancialPeriods(filters?: FinancialFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, filters],
    queryFn: () => financialService.list(filters),
    staleTime: 30000,
    enabled: !!filters?.companyId,
  });
}

export function useFinancialPeriod(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => financialService.getById(id),
    enabled: !!id,
  });
}

export function useFinancialDashboard(companyId: string, year: number) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'dashboard', companyId, year],
    queryFn: () => financialService.getDashboard(companyId, year),
    staleTime: 60000,
    enabled: !!companyId && !!year,
  });
}

export function useCreateFinancialPeriod() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateFinancialPeriodRequest) => financialService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Período financeiro criado com sucesso');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao criar período financeiro';
      toast.error(message);
    },
  });
}

export function useUpdateFinancialPeriod() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateFinancialPeriodRequest }) =>
      financialService.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao atualizar período financeiro';
      toast.error(message);
    },
  });
}

export function useSubmitFinancialPeriod() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => financialService.submit(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Período submetido para aprovação');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao submeter período';
      toast.error(message);
    },
  });
}

export function useApproveFinancialPeriod() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => financialService.approve(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Período aprovado com sucesso');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao aprovar período';
      toast.error(message);
    },
  });
}

export function useLockFinancialPeriod() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => financialService.lock(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Período bloqueado');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao bloquear período';
      toast.error(message);
    },
  });
}

export function useReturnFinancialPeriodToSubmitted() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => financialService.returnToSubmitted(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Período retornado para revisão');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao retornar período';
      toast.error(message);
    },
  });
}

// ─── Metrics upsert hooks ─────────────────────────────────────────────────────

function makeMetricMutation<T>(
  mutationFn: (args: { id: string; data: T }) => Promise<unknown>,
  successMessage: string
) {
  return function useMetricMutation() {
    const queryClient = useQueryClient();
    return useMutation({
      mutationFn: ({ id, data }: { id: string; data: T }) => mutationFn({ id, data }),
      onSuccess: (_, { id }) => {
        queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
        queryClient.invalidateQueries({ queryKey: QUERY_KEY });
        toast.success(successMessage);
      },
      onError: (error: any) => {
        const message =
          error.response?.data?.message ||
          error.response?.data?.errors?.[0] ||
          'Erro ao salvar métricas';
        toast.error(message);
      },
    });
  };
}

export const useUpsertRevenue = makeMetricMutation<UpsertRevenueRequest>(
  ({ id, data }) => financialService.upsertRevenue(id, data),
  'Receita atualizada'
);

export const useUpsertCashBurn = makeMetricMutation<UpsertCashBurnRequest>(
  ({ id, data }) => financialService.upsertCashBurn(id, data),
  'Caixa/Burn atualizado'
);

export const useUpsertUnitEconomics = makeMetricMutation<UpsertUnitEconomicsRequest>(
  ({ id, data }) => financialService.upsertUnitEconomics(id, data),
  'Unit economics atualizados'
);

export const useUpsertProfitability = makeMetricMutation<UpsertProfitabilityRequest>(
  ({ id, data }) => financialService.upsertProfitability(id, data),
  'Lucratividade atualizada'
);
