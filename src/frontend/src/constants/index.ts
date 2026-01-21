// =====================================================
// CONSTANTES DO SISTEMA - Partnership Manager
// =====================================================

export const APP_NAME = 'Partnership Manager';
export const APP_VERSION = '1.0.0';
export const API_URL = import.meta.env.VITE_API_URL || '/api';

// =====================================================
// MENSAGENS DE ERRO
// =====================================================
export const ErrorMessages = {
  // Validação Geral
  REQUIRED: 'Este campo é obrigatório.',
  INVALID_EMAIL: 'Email inválido.',
  INVALID_URL: 'URL inválida.',
  MAX_LENGTH: (field: string, max: number) => `${field} deve ter no máximo ${max} caracteres.`,
  MIN_LENGTH: (field: string, min: number) => `${field} deve ter no mínimo ${min} caracteres.`,

  // Autenticação
  INVALID_CREDENTIALS: 'Email ou senha inválidos.',
  USER_NOT_FOUND: 'Usuário não encontrado.',
  USER_INACTIVE: 'Usuário inativo. Entre em contato com o administrador.',
  USER_BLOCKED: (minutes: number) => `Usuário bloqueado. Tente novamente em ${minutes} minutos.`,
  INVALID_TOKEN: 'Token inválido ou expirado.',
  SESSION_EXPIRED: 'Sessão expirada. Faça login novamente.',
  UNAUTHORIZED: 'Acesso não autorizado.',
  FORBIDDEN: 'Você não tem permissão para realizar esta ação.',

  // Senha
  PASSWORD_MIN_LENGTH: 'A senha deve ter no mínimo 8 caracteres.',
  PASSWORD_UPPERCASE: 'A senha deve conter pelo menos uma letra maiúscula.',
  PASSWORD_LOWERCASE: 'A senha deve conter pelo menos uma letra minúscula.',
  PASSWORD_NUMBER: 'A senha deve conter pelo menos um número.',
  PASSWORD_SPECIAL: 'A senha deve conter pelo menos um caractere especial.',
  PASSWORD_MISMATCH: 'As senhas não conferem.',

  // Empresa
  CNPJ_REQUIRED: 'O CNPJ é obrigatório.',
  CNPJ_INVALID: 'O CNPJ informado é inválido.',
  CNPJ_EXISTS: 'Já existe uma empresa cadastrada com este CNPJ.',

  // Sistema
  INTERNAL_ERROR: 'Ocorreu um erro interno. Tente novamente mais tarde.',
  NETWORK_ERROR: 'Erro de conexão. Verifique sua internet.',
  NOT_FOUND: (entity: string) => `${entity} não encontrado(a).`,
} as const;

// =====================================================
// MENSAGENS DE SUCESSO
// =====================================================
export const SuccessMessages = {
  // Autenticação
  LOGIN_SUCCESS: 'Login realizado com sucesso!',
  LOGOUT_SUCCESS: 'Logout realizado com sucesso.',
  PASSWORD_CHANGED: 'Senha alterada com sucesso.',
  PASSWORD_RESET_SENT: 'Email de recuperação enviado com sucesso.',

  // CRUD
  CREATED: (entity: string) => `${entity} criado(a) com sucesso.`,
  UPDATED: (entity: string) => `${entity} atualizado(a) com sucesso.`,
  DELETED: (entity: string) => `${entity} excluído(a) com sucesso.`,
  SAVED: 'Dados salvos com sucesso.',

  // Empresa
  COMPANY_CREATED: 'Empresa criada com sucesso.',
  COMPANY_UPDATED: 'Empresa atualizada com sucesso.',

  // Usuário
  USER_CREATED: 'Usuário criado com sucesso.',
  USER_UPDATED: 'Usuário atualizado com sucesso.',
  USER_ACTIVATED: 'Usuário ativado com sucesso.',
  USER_DEACTIVATED: 'Usuário desativado com sucesso.',
} as const;

// =====================================================
// CONFIGURAÇÕES
// =====================================================
export const Config = {
  // Paginação
  DEFAULT_PAGE_SIZE: 10,
  MAX_PAGE_SIZE: 100,

  // Cache
  CACHE_EXPIRATION_MINUTES: 30,

  // Autenticação
  TOKEN_EXPIRATION_HOURS: 24,
  MAX_LOGIN_ATTEMPTS: 5,
  LOCKOUT_MINUTES: 15,

  // Validação
  MIN_PASSWORD_LENGTH: 8,
  MAX_PASSWORD_LENGTH: 100,
  MAX_NAME_LENGTH: 200,
  MAX_EMAIL_LENGTH: 255,

  // Formatos
  DATE_FORMAT: 'dd/MM/yyyy',
  DATETIME_FORMAT: 'dd/MM/yyyy HH:mm',
  CURRENCY_LOCALE: 'pt-BR',
  CURRENCY_CODE: 'BRL',
} as const;

// =====================================================
// ROTAS
// =====================================================
export const Routes = {
  // Públicas
  LOGIN: '/login',
  FORGOT_PASSWORD: '/forgot-password',
  RESET_PASSWORD: '/reset-password',

  // Privadas
  DASHBOARD: '/dashboard',
  CAP_TABLE: '/cap-table',
  PARTNERS: '/partners',
  APPROVALS: '/approvals',
  CONTRACTS: '/contracts',
  VESTING: '/vesting',
  VALUATION: '/valuation',
  FINANCIAL: '/financial',
  INVESTOR_PORTAL: '/investor',
  SETTINGS: '/settings',
} as const;

// =====================================================
// ROLES E PERMISSÕES
// =====================================================
export const Roles = {
  SUPER_ADMIN: 'SuperAdmin',
  ADMIN: 'Admin',
  FOUNDER: 'Founder',
  BOARD_MEMBER: 'BoardMember',
  LEGAL: 'Legal',
  FINANCE: 'Finance',
  HR: 'HR',
  EMPLOYEE: 'Employee',
  INVESTOR: 'Investor',
  VIEWER: 'Viewer',
} as const;

export const RoleLabels: Record<string, string> = {
  SuperAdmin: 'Super Administrador',
  Admin: 'Administrador',
  Founder: 'Fundador',
  BoardMember: 'Conselheiro',
  Legal: 'Jurídico',
  Finance: 'Financeiro',
  HR: 'RH',
  Employee: 'Funcionário',
  Investor: 'Investidor',
  Viewer: 'Visualizador',
};

// =====================================================
// STATUS
// =====================================================
export const ShareholderTypes = {
  FOUNDER: 'Fundador',
  INVESTOR: 'Investidor',
  EMPLOYEE: 'Funcionário',
  ADVISOR: 'Advisor',
  ESOP: 'ESOP',
} as const;

export const ShareholderStatus = {
  ACTIVE: 'Ativo',
  INACTIVE: 'Inativo',
  PENDING: 'Pendente',
  VESTING: 'Vesting',
  EXITED: 'Saiu',
} as const;

// =====================================================
// CORES DO DESIGN SYSTEM
// =====================================================
export const Colors = {
  primary: '#111827',
  secondary: '#333333',
  background: '#F3F4F6',
  accent: '#0891B2',
  success: '#059669',
  warning: '#D97706',
  error: '#DC2626',
  info: '#2563EB',
  purple: '#7C3AED',
} as const;
