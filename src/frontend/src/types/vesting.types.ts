// Vesting Types — espelha os DTOs e Enums do backend

// ─── Enums ────────────────────────────────────────────────────────────────────

export enum VestingType {
  TimeBasedLinear = 'TimeBasedLinear',
  TimeBasedCliff = 'TimeBasedCliff',
  MilestoneBasedOnly = 'MilestoneBasedOnly',
  HybridTimeMilestone = 'HybridTimeMilestone',
}

export enum VestingPlanStatus {
  Draft = 'Draft',
  Active = 'Active',
  Inactive = 'Inactive',
  Archived = 'Archived',
}

export enum VestingGrantDetailStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Active = 'Active',
  Exercised = 'Exercised',
  Expired = 'Expired',
  Cancelled = 'Cancelled',
}

export enum MilestoneStatus {
  Pending = 'Pending',
  Achieved = 'Achieved',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
}

export enum MilestoneType {
  Revenue = 'Revenue',
  UserGrowth = 'UserGrowth',
  ProductLaunch = 'ProductLaunch',
  Custom = 'Custom',
}

export enum VestingTransactionType {
  Exercise = 'Exercise',
  Acceleration = 'Acceleration',
  Forfeiture = 'Forfeiture',
}

// ─── Interfaces ───────────────────────────────────────────────────────────────

export interface VestingPlan {
  id: string;
  clientId: string;
  companyId: string;
  name: string;
  description?: string;
  vestingType: VestingType;
  cliffMonths: number;
  vestingMonths: number;
  totalEquityPercentage: number;
  status: VestingPlanStatus;
  activatedAt?: string;
  activatedBy?: string;
  activeGrantsCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface VestingPlanListResponse {
  items: VestingPlan[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateVestingPlanRequest {
  companyId: string;
  name: string;
  description?: string;
  vestingType: VestingType;
  cliffMonths: number;
  vestingMonths: number;
  totalEquityPercentage: number;
}

export interface UpdateVestingPlanRequest {
  name: string;
  description?: string;
  cliffMonths: number;
  vestingMonths: number;
  totalEquityPercentage: number;
}

// ─── Vesting Grant ────────────────────────────────────────────────────────────

export interface VestingGrant {
  id: string;
  clientId: string;
  vestingPlanId: string;
  vestingPlanName: string;
  shareholderId: string;
  shareholderName: string;
  companyId: string;
  grantDate: string;
  totalShares: number;
  sharePrice: number;
  equityPercentage: number;
  vestingStartDate: string;
  vestingEndDate: string;
  cliffDate?: string;
  status: VestingGrantDetailStatus;
  vestedShares: number;
  exercisedShares: number;
  availableToExercise: number;
  vestedPercentage: number;
  approvedAt?: string;
  notes?: string;
  createdAt: string;
  updatedAt: string;
}

export interface VestingGrantListResponse {
  items: VestingGrant[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateVestingGrantRequest {
  vestingPlanId: string;
  shareholderId: string;
  grantDate: string;
  totalShares: number;
  sharePrice: number;
  equityPercentage: number;
  vestingStartDate: string;
  notes?: string;
}

export interface ExerciseSharesRequest {
  sharesToExercise: number;
  sharePriceAtExercise: number;
  exerciseDate: string;
  transactionType?: VestingTransactionType;
  notes?: string;
}

// ─── Milestones ───────────────────────────────────────────────────────────────

export interface VestingMilestone {
  id: string;
  clientId: string;
  vestingPlanId: string;
  vestingPlanName: string;
  companyId: string;
  name: string;
  description?: string;
  milestoneType: MilestoneType;
  targetValue?: number;
  targetUnit?: string;
  accelerationPercentage: number;
  isRequiredForFullVesting: boolean;
  status: MilestoneStatus;
  targetDate?: string;
  achievedDate?: string;
  achievedValue?: number;
  createdAt: string;
  updatedAt: string;
}

export interface VestingMilestoneListResponse {
  items: VestingMilestone[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateVestingMilestoneRequest {
  vestingPlanId: string;
  name: string;
  description?: string;
  milestoneType: MilestoneType;
  targetValue?: number;
  targetUnit?: string;
  accelerationPercentage: number;
  isRequiredForFullVesting: boolean;
  targetDate?: string;
}

export interface AchieveMilestoneRequest {
  achievedDate: string;
  achievedValue?: number;
}

// ─── Calculation / Projection ─────────────────────────────────────────────────

export interface VestingCalculationResult {
  grantId: string;
  asOfDate: string;
  totalShares: number;
  vestedShares: number;
  exercisedShares: number;
  availableToExercise: number;
  unvestedShares: number;
  vestedPercentage: number;
  isCliffMet: boolean;
  isFullyVested: boolean;
}

export interface VestingProjectionPoint {
  date: string;
  vestedShares: number;
  vestedPercentage: number;
}

export interface VestingProjectionResponse {
  grantId: string;
  projectionEndDate: string;
  points: VestingProjectionPoint[];
}

// ─── Transactions ─────────────────────────────────────────────────────────────

export interface VestingTransaction {
  id: string;
  vestingGrantId: string;
  shareholderId: string;
  shareholderName: string;
  transactionDate: string;
  sharesExercised: number;
  sharePriceAtExercise: number;
  strikePrice: number;
  totalExerciseValue: number;
  gainAmount: number;
  transactionType: VestingTransactionType;
  notes?: string;
  createdAt: string;
}

export interface VestingTransactionListResponse {
  items: VestingTransaction[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// ─── Filtros ──────────────────────────────────────────────────────────────────

export interface VestingPlanFilters {
  companyId?: string;
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
}

export interface VestingGrantFilters {
  companyId?: string;
  vestingPlanId?: string;
  shareholderId?: string;
  status?: string;
  page?: number;
  pageSize?: number;
}

export interface VestingMilestoneFilters {
  vestingPlanId?: string;
  companyId?: string;
  page?: number;
  pageSize?: number;
}

// ─── Labels helpers ───────────────────────────────────────────────────────────

export const vestingTypeLabels: Record<VestingType, string> = {
  [VestingType.TimeBasedLinear]: 'Tempo — Linear',
  [VestingType.TimeBasedCliff]: 'Tempo — Cliff',
  [VestingType.MilestoneBasedOnly]: 'Apenas Metas',
  [VestingType.HybridTimeMilestone]: 'Híbrido (Tempo + Metas)',
};

export const vestingPlanStatusLabels: Record<VestingPlanStatus, string> = {
  [VestingPlanStatus.Draft]: 'Rascunho',
  [VestingPlanStatus.Active]: 'Ativo',
  [VestingPlanStatus.Inactive]: 'Inativo',
  [VestingPlanStatus.Archived]: 'Arquivado',
};

export const vestingGrantStatusLabels: Record<VestingGrantDetailStatus, string> = {
  [VestingGrantDetailStatus.Pending]: 'Pendente',
  [VestingGrantDetailStatus.Approved]: 'Aprovado',
  [VestingGrantDetailStatus.Active]: 'Ativo',
  [VestingGrantDetailStatus.Exercised]: 'Exercido',
  [VestingGrantDetailStatus.Expired]: 'Expirado',
  [VestingGrantDetailStatus.Cancelled]: 'Cancelado',
};

export const milestoneStatusLabels: Record<MilestoneStatus, string> = {
  [MilestoneStatus.Pending]: 'Pendente',
  [MilestoneStatus.Achieved]: 'Atingido',
  [MilestoneStatus.Failed]: 'Falhou',
  [MilestoneStatus.Cancelled]: 'Cancelado',
};

export const milestoneTypeLabels: Record<MilestoneType, string> = {
  [MilestoneType.Revenue]: 'Receita',
  [MilestoneType.UserGrowth]: 'Crescimento de Usuários',
  [MilestoneType.ProductLaunch]: 'Lançamento de Produto',
  [MilestoneType.Custom]: 'Personalizado',
};

export const vestingTransactionTypeLabels: Record<VestingTransactionType, string> = {
  [VestingTransactionType.Exercise]: 'Exercício',
  [VestingTransactionType.Acceleration]: 'Aceleração',
  [VestingTransactionType.Forfeiture]: 'Perda',
};
