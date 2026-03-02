import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { customFormulaService } from '@/services/customFormulaService';
import type {
  CustomFormulaFilters,
  CreateCustomFormulaRequest,
  UpdateFormulaMetadataRequest,
  PublishNewFormulaVersionRequest,
  TestFormulaRequest,
} from '@/types';
import toast from 'react-hot-toast';

const QUERY_KEY = ['custom-formulas'];

export function useCustomFormulas(filters?: CustomFormulaFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, filters],
    queryFn: () => customFormulaService.list(filters),
    staleTime: 30000,
    enabled: !!filters?.companyId,
  });
}

export function useCustomFormula(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => customFormulaService.getById(id),
    enabled: !!id,
  });
}

export function useFormulaVersions(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id, 'versions'],
    queryFn: () => customFormulaService.getVersions(id),
    enabled: !!id,
  });
}

export function useCreateCustomFormula() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCustomFormulaRequest) => customFormulaService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Fórmula criada com sucesso');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao criar fórmula';
      toast.error(message);
    },
  });
}

export function useUpdateFormulaMetadata() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateFormulaMetadataRequest }) =>
      customFormulaService.updateMetadata(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Fórmula atualizada');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao atualizar fórmula';
      toast.error(message);
    },
  });
}

export function usePublishFormulaVersion() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: PublishNewFormulaVersionRequest }) =>
      customFormulaService.publishVersion(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id, 'versions'] });
      toast.success('Nova versão publicada com sucesso');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao publicar versão';
      toast.error(message);
    },
  });
}

export function useActivateFormula() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => customFormulaService.activate(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Fórmula ativada');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao ativar fórmula');
    },
  });
}

export function useDeactivateFormula() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => customFormulaService.deactivate(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Fórmula desativada');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao desativar fórmula');
    },
  });
}

export function useTestFormula() {
  return useMutation({
    mutationFn: (data: TestFormulaRequest) => customFormulaService.test(data),
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao testar fórmula');
    },
  });
}

export function useDeleteCustomFormula() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => customFormulaService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Fórmula excluída');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao excluir fórmula');
    },
  });
}
