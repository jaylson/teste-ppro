import { api } from './api';
import type {
  LoginRequest,
  AuthResponse,
  RefreshTokenRequest,
  ChangePasswordRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ActivateAccountRequest,
  UserInfo,
} from '@/types';

const AUTH_ENDPOINTS = {
  LOGIN: '/auth/login',
  LOGOUT: '/auth/logout',
  REFRESH: '/auth/refresh',
  ME: '/auth/me',
  CHANGE_PASSWORD: '/auth/change-password',
  FORGOT_PASSWORD: '/auth/forgot-password',
  RESET_PASSWORD: '/auth/reset-password',
  ACTIVATE_ACCOUNT: '/auth/activate-account',
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

  /**
   * Solicita link de recuperação de senha por e-mail
   */
  forgotPassword: async (data: ForgotPasswordRequest): Promise<void> => {
    await api.post(AUTH_ENDPOINTS.FORGOT_PASSWORD, data);
  },

  /**
   * Redefine a senha usando o token recebido por e-mail
   */
  resetPassword: async (data: ResetPasswordRequest): Promise<void> => {
    await api.post(AUTH_ENDPOINTS.RESET_PASSWORD, data);
  },

  /**
   * Ativa a conta de um novo usuário definindo sua senha pela primeira vez
   */
  activateAccount: async (data: ActivateAccountRequest): Promise<void> => {
    await api.post(AUTH_ENDPOINTS.ACTIVATE_ACCOUNT, data);
  },
};
