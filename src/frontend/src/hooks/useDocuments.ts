import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { documentService } from '@/services/documentService';
import type {
  DocumentFilters,
  CreateDocumentRequest,
  UpdateDocumentMetadataRequest,
} from '@/types';
import toast from 'react-hot-toast';

const QUERY_KEY = ['documents'];

export function useDocuments(filters?: DocumentFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, filters],
    queryFn: () => documentService.list(filters),
    staleTime: 30000,
    enabled: !!filters?.companyId,
  });
}

export function useDocumentsByEntity(
  companyId: string,
  entityType: string,
  entityId: string
) {
  return useQuery({
    queryKey: [...QUERY_KEY, 'by-entity', companyId, entityType, entityId],
    queryFn: () => documentService.getByEntity(companyId, entityType, entityId),
    staleTime: 30000,
    enabled: !!companyId && !!entityType && !!entityId,
  });
}

export function useDocument(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => documentService.getById(id),
    enabled: !!id,
  });
}

export function useCreateDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateDocumentRequest) => documentService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Documento registrado com sucesso');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao registrar documento';
      toast.error(message);
    },
  });
}

export function useUpdateDocumentMetadata() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateDocumentMetadataRequest }) =>
      documentService.updateMetadata(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Documento atualizado');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao atualizar documento';
      toast.error(message);
    },
  });
}

export function useVerifyDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => documentService.verify(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Documento verificado');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao verificar documento';
      toast.error(message);
    },
  });
}

export function useDeleteDocument() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => documentService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Documento excluído');
    },
    onError: (error: any) => {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Erro ao excluir documento';
      toast.error(message);
    },
  });
}
