import { api } from './api';
import type {
  LoginRequest,
  AuthResponse,
  RefreshTokenRequest,
  ChangePasswordRequest,
  UserInfo,
} from '@/types';

const AUTH_ENDPOINTS = {
  LOGIN: '/auth/login',
  LOGOUT: '/auth/logout',
  REFRESH: '/auth/refresh',
  ME: '/auth/me',
  CHANGE_PASSWORD: '/auth/change-password',
};

export const authService = {
  /**
   * Realiza login no sistema
   */
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<{ data: AuthResponse }>(
      AUTH_ENDPOINTS.LOGIN,
      data
    );
    return response.data.data;
  },

  /**
   * Realiza logout
   */
  logout: async (): Promise<void> => {
    await api.post(AUTH_ENDPOINTS.LOGOUT);
  },

  /**
   * Renova o token de acesso
   */
  refreshToken: async (refreshToken: string): Promise<AuthResponse> => {
    const response = await api.post<{ data: AuthResponse }>(
      AUTH_ENDPOINTS.REFRESH,
      {
        refreshToken,
      } as RefreshTokenRequest
    );
    return response.data.data;
  },

  /**
   * Obtém dados do usuário autenticado
   */
  getMe: async (): Promise<UserInfo> => {
    const response = await api.get<{ data: UserInfo }>(AUTH_ENDPOINTS.ME);
    return response.data.data;
  },

  /**
   * Altera a senha do usuário
   */
  changePassword: async (data: ChangePasswordRequest): Promise<void> => {
    await api.post(AUTH_ENDPOINTS.CHANGE_PASSWORD, data);
  },
};
