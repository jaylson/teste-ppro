import { api } from './api';
import type { Notification, NotificationPreference } from '@/types/phase6';
import type { PagedResult } from '@/types';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

export interface NotificationFilters {
  notificationType?: string;
  isRead?: boolean;
  page?: number;
  pageSize?: number;
}

export const notificationService = {
  async getAll(params?: NotificationFilters): Promise<PagedResult<Notification>> {
    const response = await api.get<ApiResponse<PagedResult<Notification>>>('/notifications', { params });
    return response.data.data;
  },

  async getRecent(): Promise<Notification[]> {
    const response = await api.get<ApiResponse<Notification[]>>('/notifications/recent');
    return response.data.data;
  },

  async getUnreadCount(): Promise<number> {
    const response = await api.get<ApiResponse<{ count: number }>>('/notifications/unread-count');
    return response.data.data.count;
  },

  async markAsRead(id: string): Promise<void> {
    await api.post(`/notifications/${id}/read`);
  },

  async markAllAsRead(): Promise<void> {
    await api.post('/notifications/read-all');
  },

  async getPreferences(): Promise<NotificationPreference[]> {
    const response = await api.get<ApiResponse<NotificationPreference[]>>('/notifications/preferences');
    return response.data.data;
  },

  async updatePreference(
    notificationType: string,
    channel: NotificationPreference['channel']
  ): Promise<NotificationPreference> {
    const response = await api.put<ApiResponse<NotificationPreference>>(
      `/notifications/preferences/${notificationType}`,
      { channel }
    );
    return response.data.data;
  },
};
