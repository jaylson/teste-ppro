import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { vestingPlanService } from '@/services/vestingService';
import type {
  VestingPlanFilters,
  CreateVestingPlanRequest,
  UpdateVestingPlanRequest,
} from '@/types';
import toast from 'react-hot-toast';

const QUERY_KEY = ['vesting-plans'];

export function useVestingPlans(filters?: VestingPlanFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, filters],
    queryFn: () => vestingPlanService.getPlans(filters),
    staleTime: 30000,
    enabled: !!filters?.companyId,
  });
}

export function useVestingPlansByCompany(companyId: string, status?: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-company', companyId, status],
    queryFn: () => vestingPlanService.getPlansByCompany(companyId, status),
    staleTime: 30000,
    enabled: !!companyId,
  });
}

export function useVestingPlan(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => vestingPlanService.getPlanById(id),
    enabled: !!id,
  });
}

export function useCreateVestingPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateVestingPlanRequest) => vestingPlanService.createPlan(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Plano de vesting criado com sucesso');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao criar plano de vesting';
      toast.error(message);
    },
  });
}

export function useUpdateVestingPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateVestingPlanRequest }) =>
      vestingPlanService.updatePlan(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, variables.id] });
      toast.success('Plano atualizado com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao atualizar plano');
    },
  });
}

export function useActivateVestingPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => vestingPlanService.activatePlan(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Plano ativado com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao ativar plano');
    },
  });
}

export function useDeactivateVestingPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => vestingPlanService.deactivatePlan(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Plano desativado');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao desativar plano');
    },
  });
}

export function useArchiveVestingPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => vestingPlanService.archivePlan(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Plano arquivado');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao arquivar plano');
    },
  });
}

export function useDeleteVestingPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => vestingPlanService.deletePlan(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Plano removido');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao remover plano');
    },
  });
}
