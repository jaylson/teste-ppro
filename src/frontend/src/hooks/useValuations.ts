import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { valuationService } from '@/services/valuationService';
import type {
  ValuationFilters,
  CreateValuationRequest,
  UpdateValuationRequest,
  RejectValuationRequest,
  AddValuationMethodRequest,
  CalculateMethodRequest,
} from '@/types';
import toast from 'react-hot-toast';

const QUERY_KEY = ['valuations'];

export function useValuations(filters?: ValuationFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, filters],
    queryFn: () => valuationService.list(filters),
    staleTime: 30000,
    enabled: filters !== undefined,
  });
}

export function useValuation(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => valuationService.getById(id),
    enabled: !!id,
  });
}

export function useCreateValuation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateValuationRequest) => valuationService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Valuation criado com sucesso');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao criar valuation';
      toast.error(message);
    },
  });
}

export function useUpdateValuation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateValuationRequest }) =>
      valuationService.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Valuation atualizado com sucesso');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao atualizar valuation';
      toast.error(message);
    },
  });
}

export function useSubmitValuation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => valuationService.submit(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Valuation submetido para aprovação');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao submeter valuation';
      toast.error(message);
    },
  });
}

export function useApproveValuation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => valuationService.approve(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Valuation aprovado com sucesso');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao aprovar valuation';
      toast.error(message);
    },
  });
}

export function useRejectValuation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: RejectValuationRequest }) =>
      valuationService.reject(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Valuation rejeitado');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao rejeitar valuation';
      toast.error(message);
    },
  });
}

export function useReturnValuationToDraft() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => valuationService.returnToDraft(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Valuation retornado para rascunho');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao retornar valuation';
      toast.error(message);
    },
  });
}

export function useDeleteValuation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => valuationService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Valuation excluído');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao excluir valuation';
      toast.error(message);
    },
  });
}

// ─── Method hooks ─────────────────────────────────────────────────────────────

export function useAddValuationMethod() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ valuationId, data }: { valuationId: string; data: AddValuationMethodRequest }) =>
      valuationService.addMethod(valuationId, data),
    onSuccess: (_, { valuationId }) => {
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, valuationId] });
      toast.success('Metodologia adicionada');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao adicionar metodologia';
      toast.error(message);
    },
  });
}

export function useCalculateMethod() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      valuationId,
      methodId,
      data,
    }: {
      valuationId: string;
      methodId: string;
      data: CalculateMethodRequest;
    }) => valuationService.calculateMethod(valuationId, methodId, data),
    onSuccess: (_, { valuationId }) => {
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, valuationId] });
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao calcular metodologia';
      toast.error(message);
    },
  });
}

export function useSelectValuationMethod() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ valuationId, methodId }: { valuationId: string; methodId: string }) =>
      valuationService.selectMethod(valuationId, methodId),
    onSuccess: (_, { valuationId }) => {
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, valuationId] });
      toast.success('Metodologia principal selecionada');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao selecionar metodologia';
      toast.error(message);
    },
  });
}
