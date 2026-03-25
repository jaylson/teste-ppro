import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationService, type NotificationFilters } from '@/services/notificationService';
import type { Notification, NotificationPreference } from '@/types/phase6';
import type { PagedResult } from '@/types';
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
    refetchInterval: 15000,
    staleTime: 10000,
  });
}

export function useMarkAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => notificationService.markAsRead(id),
    onMutate: async (id: string) => {
      await queryClient.cancelQueries({ queryKey: QUERY_KEY });

      // Optimistic update: mark as read in all cached queries
      queryClient.setQueriesData<PagedResult<Notification>>(
        { queryKey: QUERY_KEY, exact: false },
        (old) => {
          if (!old || !('items' in old)) return old;
          return { ...old, items: old.items.map((n) => n.id === id ? { ...n, isRead: true } : n) };
        }
      );
      queryClient.setQueriesData<Notification[]>(
        { queryKey: [...QUERY_KEY, 'recent'], exact: true },
        (old) => old?.map((n) => n.id === id ? { ...n, isRead: true } : n)
      );
      queryClient.setQueriesData<number>(
        { queryKey: [...QUERY_KEY, 'unread-count'], exact: true },
        (old) => (old != null && old > 0 ? old - 1 : 0)
      );
    },
    onError: (error: any) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      const message = error.response?.data?.message || 'Erro ao marcar como lida';
      toast.error(message);
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
    },
  });
}

export function useMarkAllAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => notificationService.markAllAsRead(),
    onMutate: async () => {
      await queryClient.cancelQueries({ queryKey: QUERY_KEY });

      queryClient.setQueriesData<PagedResult<Notification>>(
        { queryKey: QUERY_KEY, exact: false },
        (old) => {
          if (!old || !('items' in old)) return old;
          return { ...old, items: old.items.map((n) => ({ ...n, isRead: true })) };
        }
      );
      queryClient.setQueriesData<Notification[]>(
        { queryKey: [...QUERY_KEY, 'recent'], exact: true },
        (old) => old?.map((n) => ({ ...n, isRead: true }))
      );
      queryClient.setQueriesData<number>(
        { queryKey: [...QUERY_KEY, 'unread-count'], exact: true },
        () => 0
      );
    },
    onError: (error: any) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
      const message = error.response?.data?.message || 'Erro ao marcar notificações';
      toast.error(message);
    },
    onSuccess: () => {
      toast.success('Todas as notificações marcadas como lidas');
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
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
