import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { vestingMilestoneService } from '@/services/vestingService';
import type {
  VestingMilestoneFilters,
  CreateVestingMilestoneRequest,
  AchieveMilestoneRequest,
} from '@/types';
import toast from 'react-hot-toast';

const QUERY_KEY = ['vesting-milestones'];

export function useVestingMilestones(filters?: VestingMilestoneFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, filters],
    queryFn: () => vestingMilestoneService.getMilestones(filters),
    staleTime: 30000,
  });
}

export function useVestingMilestonesByPlan(vestingPlanId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-plan', vestingPlanId],
    queryFn: () => vestingMilestoneService.getMilestonesByPlan(vestingPlanId),
    enabled: !!vestingPlanId,
    staleTime: 30000,
  });
}

export function useVestingMilestone(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => vestingMilestoneService.getMilestoneById(id),
    enabled: !!id,
  });
}

export function useCreateVestingMilestone() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateVestingMilestoneRequest) =>
      vestingMilestoneService.createMilestone(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Meta de vesting criada com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao criar meta');
    },
  });
}

export function useAchieveVestingMilestone() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AchieveMilestoneRequest }) =>
      vestingMilestoneService.achieveMilestone(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Meta marcada como atingida');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao registrar meta');
    },
  });
}

export function useFailVestingMilestone() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => vestingMilestoneService.failMilestone(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Meta marcada como falhou');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao atualizar meta');
    },
  });
}

export function useDeleteVestingMilestone() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => vestingMilestoneService.deleteMilestone(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Meta removida');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao remover meta');
    },
  });
}
