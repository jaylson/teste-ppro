import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { workflowService, type WorkflowFilters } from '@/services/workflowService';
import type { CreateWorkflowRequest } from '@/types/phase6';
import toast from 'react-hot-toast';

const QUERY_KEY = ['workflows'];

export function useWorkflows(params?: WorkflowFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, params],
    queryFn: () => workflowService.getAll(params),
    staleTime: 30000,
  });
}

export function usePendingWorkflows(companyId?: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'pending', companyId],
    queryFn: () => workflowService.getPending(companyId),
    staleTime: 30000,
  });
}

export function useWorkflow(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => workflowService.getById(id),
    enabled: !!id,
  });
}

export function useCreateWorkflow() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateWorkflowRequest) => workflowService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Workflow criado com sucesso');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao criar workflow';
      toast.error(message);
    },
  });
}

export function useApproveStep() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      workflowId,
      stepId,
      comments,
    }: {
      workflowId: string;
      stepId: string;
      comments?: string;
    }) => workflowService.approveStep(workflowId, stepId, comments),
    onSuccess: (_, { workflowId }) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, workflowId] });
      toast.success('Etapa aprovada');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao aprovar etapa';
      toast.error(message);
    },
  });
}

export function useRejectStep() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      workflowId,
      stepId,
      comments,
    }: {
      workflowId: string;
      stepId: string;
      comments: string;
    }) => workflowService.rejectStep(workflowId, stepId, comments),
    onSuccess: (_, { workflowId }) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, workflowId] });
      toast.success('Etapa rejeitada');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao rejeitar etapa';
      toast.error(message);
    },
  });
}

export function useCancelWorkflow() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      workflowService.cancel(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Workflow cancelado');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao cancelar workflow';
      toast.error(message);
    },
  });
}
