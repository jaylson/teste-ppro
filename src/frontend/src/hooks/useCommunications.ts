import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { communicationService, type CommunicationFilters } from '@/services/communicationService';
import type { CreateCommunicationRequest } from '@/types/phase6';
import toast from 'react-hot-toast';

const QUERY_KEY = ['communications'];

export function useCommunications(filters?: CommunicationFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, filters],
    queryFn: () => communicationService.getAll(filters),
    staleTime: 30000,
  });
}

export function useCommunication(id: string) {
  return useQuery({
    queryKey: [...QUERY_KEY, id],
    queryFn: () => communicationService.getById(id),
    enabled: !!id,
  });
}

export function useCreateCommunication() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCommunicationRequest) => communicationService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Comunicação criada com sucesso');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao criar comunicação';
      toast.error(message);
    },
  });
}

export function useUpdateCommunication() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<CreateCommunicationRequest> }) =>
      communicationService.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Comunicação atualizada');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao atualizar comunicação';
      toast.error(message);
    },
  });
}

export function usePublishCommunication() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => communicationService.publish(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, id] });
      toast.success('Comunicação publicada');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao publicar comunicação';
      toast.error(message);
    },
  });
}

export function useDeleteCommunication() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => communicationService.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Comunicação excluída');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao excluir comunicação';
      toast.error(message);
    },
  });
}
