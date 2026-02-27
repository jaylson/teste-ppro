import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { milestoneTemplateService } from '@/services/vestingService';
import type {
  MilestoneTemplateFilters,
  CreateMilestoneTemplateRequest,
  UpdateMilestoneTemplateRequest,
} from '@/types';
import toast from 'react-hot-toast';

const QUERY_KEY = ['milestone-templates'];

export function useMilestoneTemplates(filters?: MilestoneTemplateFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, filters],
    queryFn: () => milestoneTemplateService.getTemplates(filters),
    staleTime: 30000,
  });
}

export function useMilestoneTemplatesByCompany(companyId: string, activeOnly = true) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-company', companyId, activeOnly],
    queryFn: () => milestoneTemplateService.getAllByCompany(companyId, activeOnly),
    enabled: !!companyId,
    staleTime: 30000,
  });
}

export function useMilestoneTemplate(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => milestoneTemplateService.getTemplateById(id),
    enabled: !!id,
  });
}

export function useCreateMilestoneTemplate(companyId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateMilestoneTemplateRequest) =>
      milestoneTemplateService.createTemplate(companyId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Template de milestone criado com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao criar template');
    },
  });
}

export function useUpdateMilestoneTemplate(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateMilestoneTemplateRequest) =>
      milestoneTemplateService.updateTemplate(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Template atualizado com sucesso');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao atualizar template');
    },
  });
}

export function useActivateMilestoneTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => milestoneTemplateService.activateTemplate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Template ativado');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao ativar template');
    },
  });
}

export function useDeactivateMilestoneTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => milestoneTemplateService.deactivateTemplate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Template desativado');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao desativar template');
    },
  });
}

export function useDeleteMilestoneTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => milestoneTemplateService.deleteTemplate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Template removido');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao remover template');
    },
  });
}
