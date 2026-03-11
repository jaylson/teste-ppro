import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationService, type NotificationFilters } from '@/services/notificationService';
import type { NotificationPreference } from '@/types/phase6';
import toast from 'react-hot-toast';

const QUERY_KEY = ['notifications'];

export function useNotifications(params?: NotificationFilters) {
  return useQuery({
    queryKey: [...QUERY_KEY, params],
    queryFn: () => notificationService.getAll(params),
    staleTime: 15000,
  });
}

export function useRecentNotifications() {
  return useQuery({
    queryKey: [...QUERY_KEY, 'recent'],
    queryFn: () => notificationService.getRecent(),
    staleTime: 15000,
  });
}

export function useUnreadNotificationsCount() {
  return useQuery({
    queryKey: [...QUERY_KEY, 'unread-count'],
    queryFn: () => notificationService.getUnreadCount(),
    refetchInterval: 30000,
    staleTime: 10000,
  });
}

export function useMarkAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => notificationService.markAsRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao marcar como lida';
      toast.error(message);
    },
  });
}

export function useMarkAllAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => notificationService.markAllAsRead(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      toast.success('Todas as notificações marcadas como lidas');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao marcar notificações';
      toast.error(message);
    },
  });
}

export function useNotificationPreferences() {
  return useQuery({
    queryKey: [...QUERY_KEY, 'preferences'],
    queryFn: () => notificationService.getPreferences(),
    staleTime: 60000,
  });
}

export function useUpdateNotificationPreference() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      notificationType,
      channel,
    }: {
      notificationType: string;
      channel: NotificationPreference['channel'];
    }) => notificationService.updatePreference(notificationType, channel),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [...QUERY_KEY, 'preferences'] });
      toast.success('Preferência atualizada');
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erro ao atualizar preferência';
      toast.error(message);
    },
  });
}
