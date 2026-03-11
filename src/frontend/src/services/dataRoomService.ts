import { api } from './api';
import type { DataRoom, DataRoomFolder } from '@/types/phase6';
import type { Document } from '@/types';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

export interface CreateFolderRequest {
  name: string;
  description?: string;
  parentId?: string;
  visibility?: 'internal' | 'investors' | 'public';
}

export const dataRoomService = {
  async getDataRoom(): Promise<DataRoom> {
    const response = await api.get<ApiResponse<DataRoom>>('/dataroom');
    return response.data.data;
  },

  async getFolders(parentId?: string): Promise<DataRoomFolder[]> {
    const response = await api.get<ApiResponse<DataRoomFolder[]>>('/dataroom/folders', {
      params: parentId ? { parentId } : undefined,
    });
    return response.data.data;
  },

  async createFolder(data: CreateFolderRequest): Promise<DataRoomFolder> {
    const response = await api.post<ApiResponse<DataRoomFolder>>('/dataroom/folders', data);
    return response.data.data;
  },

  async deleteFolder(id: string): Promise<void> {
    await api.delete(`/dataroom/folders/${id}`);
  },

  async getDocumentsInFolder(folderId: string): Promise<Document[]> {
    const response = await api.get<ApiResponse<Document[]>>(`/dataroom/folders/${folderId}/documents`);
    return response.data.data;
  },

  async addDocumentToFolder(folderId: string, documentId: string): Promise<void> {
    await api.post(`/dataroom/folders/${folderId}/documents`, { documentId });
  },

  async removeDocumentFromFolder(folderId: string, documentId: string): Promise<void> {
    await api.delete(`/dataroom/folders/${folderId}/documents/${documentId}`);
  },
};
