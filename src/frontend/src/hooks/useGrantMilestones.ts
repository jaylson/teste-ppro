import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { grantMilestoneService } from '@/services/vestingService';
import type {
  GrantMilestoneFilters,
  CreateGrantMilestoneRequest,
  AchieveGrantMilestoneRequest,
  RecordMilestoneProgressRequest,
} from '@/types';
import toast from 'react-hot-toast';

const QUERY_KEY = ['grant-milestones'];

export function useGrantMilestones(filters?: GrantMilestoneFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, filters],
    queryFn: () => grantMilestoneService.getMilestones(filters),
    staleTime: 30000,
  });
}

export function useGrantMilestonesByGrant(grantId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-grant', grantId],
    queryFn: () => grantMilestoneService.getMilestonesByGrant(grantId),
    enabled: !!grantId,
    staleTime: 30000,
  });
}

export function useGrantMilestone(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => grantMilestoneService.getMilestoneById(id),
    enabled: !!id,
  });
}

export function useMilestoneProgressDashboard(grantId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'dashboard', grantId],
    queryFn: () => grantMilestoneService.getDashboard(grantId),
    enabled: !!grantId,
    staleTime: 30000,
  });
}

export function useMilestoneProgressHistory(milestoneId: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, milestoneId, 'progress'],
    queryFn: () => grantMilestoneService.getProgressHistory(milestoneId),
    enabled: !!milestoneId,
    staleTime: 60000,
  });
}

export function useMilestoneProgressTimeline(
  milestoneId: string,
  from?: string,
  to?: string
) {
  return useQuery({
    queryKey: [...QUERY_KEY, milestoneId, 'timeline', from, to],
    queryFn: () => grantMilestoneService.getProgressTimeline(milestoneId, from, to),
    enabled: !!milestoneId,
    staleTime: 60000,
  });
}

export function useCreateGrantMilestone(companyId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateGrantMilestoneRequest) =>
      grantMilestoneService.createMilestone(companyId, data),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['grant-milestones', 'by-grant', vars.vestingGrantId] });
      toast.success('Milestone criado com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao criar milestone');
    },
  });
}

export function useRecordMilestoneProgress() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: RecordMilestoneProgressRequest }) =>
      grantMilestoneService.recordProgress(id, data),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['grant-milestones', result.id, 'progress'] });
      toast.success('Progresso registrado com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao registrar progresso');
    },
  });
}

export function useAchieveGrantMilestone() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AchieveGrantMilestoneRequest }) =>
      grantMilestoneService.achieveMilestone(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Milestone marcado como atingido');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao atualizar milestone');
    },
  });
}

export function useVerifyGrantMilestone() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => grantMilestoneService.verifyMilestone(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Milestone verificado');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao verificar milestone');
    },
  });
}

export function useFailGrantMilestone() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => grantMilestoneService.failMilestone(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Milestone marcado como não atingido');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao atualizar milestone');
    },
  });
}

export function useCancelGrantMilestone() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => grantMilestoneService.cancelMilestone(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Milestone cancelado');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao cancelar milestone');
    },
  });
}

export function useDeleteGrantMilestone() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => grantMilestoneService.deleteMilestone(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Milestone removido');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao remover milestone');
    },
  });
}
