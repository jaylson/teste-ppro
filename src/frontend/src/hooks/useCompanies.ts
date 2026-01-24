import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { companyService } from '@/services/companyService';
import type {
  CreateCompanyRequest,
  UpdateCompanyRequest,
  UpdateShareInfoRequest,
  PaginationParams,
} from '@/types';
import toast from 'react-hot-toast';

const QUERY_KEY = ['companies'];

export function useCompanies(params?: PaginationParams) {
  return useQuery({
    queryKey: [...QUERY_KEY, params],
    queryFn: () => companyService.getCompanies(params),
  });
}

export function useCompany(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => companyService.getCompanyById(id),
    enabled: !!id,
  });
}

export function useCreateCompany() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCompanyRequest) => companyService.createCompany(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Empresa criada com sucesso');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao criar empresa';
      toast.error(message);
    },
  });
}

export function useUpdateCompany() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCompanyRequest }) =>
      companyService.updateCompany(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Empresa atualizada com sucesso');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao atualizar empresa';
      toast.error(message);
    },
  });
}

export function useUpdateShareInfo() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateShareInfoRequest }) =>
      companyService.updateShareInfo(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Informações de ações atualizadas');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao atualizar ações';
      toast.error(message);
    },
  });
}

export function useDeleteCompany() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => companyService.deleteCompany(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Empresa removida com sucesso');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao remover empresa';
      toast.error(message);
    },
  });
}
