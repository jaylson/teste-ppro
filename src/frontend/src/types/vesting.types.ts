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
  InProgress = 'InProgress',
  Achieved = 'Achieved',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
}

// ─── Enums — Grant Milestones & Acceleration ──────────────────────────────────

export enum MilestoneCategory {
  Financial = 'Financial',
  Operational = 'Operational',
  Product = 'Product',
  Market = 'Market',
  Strategic = 'Strategic',
}

export enum MetricType {
  Revenue = 'Revenue',
  Profit = 'Profit',
  Ebitda = 'Ebitda',
  UserCount = 'UserCount',
  Mrr = 'Mrr',
  Arr = 'Arr',
  CustomerCount = 'CustomerCount',
  Nps = 'Nps',
  MarketShare = 'MarketShare',
  ProductMilestone = 'ProductMilestone',
  Custom = 'Custom',
}

export enum TargetOperator {
  GreaterThan = 'GreaterThan',
  GreaterThanOrEqual = 'GreaterThanOrEqual',
  LessThan = 'LessThan',
  LessThanOrEqual = 'LessThanOrEqual',
  Equal = 'Equal',
}

export enum MeasurementFrequency {
  OneTime = 'OneTime',
  Monthly = 'Monthly',
  Quarterly = 'Quarterly',
  Annual = 'Annual',
}

export enum VestingAccelerationType {
  Percentage = 'Percentage',
  Months = 'Months',
  Shares = 'Shares',
}

export enum ProgressDataSource {
  Manual = 'Manual',
  Automatic = 'Automatic',
  Audited = 'Audited',
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

// ─── Milestone Templates ──────────────────────────────────────────────────────

export interface MilestoneTemplate {
  id: string;
  clientId: string;
  companyId: string;
  name: string;
  description?: string;
  category: MilestoneCategory;
  metricType: MetricType;
  targetOperator: TargetOperator;
  targetValue: number;
  targetUnit?: string;
  measurementFrequency: MeasurementFrequency;
  accelerationType: VestingAccelerationType;
  accelerationAmount: number;
  maxAccelerationCap?: number;
  effectiveCap: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface MilestoneTemplateListResponse {
  items: MilestoneTemplate[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateMilestoneTemplateRequest {
  name: string;
  description?: string;
  category: MilestoneCategory;
  metricType: MetricType;
  targetOperator: TargetOperator;
  targetValue: number;
  targetUnit?: string;
  measurementFrequency: MeasurementFrequency;
  accelerationType: VestingAccelerationType;
  accelerationAmount: number;
  maxAccelerationCap?: number;
}

export interface UpdateMilestoneTemplateRequest extends CreateMilestoneTemplateRequest {}

export interface MilestoneTemplateFilters {
  companyId?: string;
  category?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

// ─── Grant Milestones ─────────────────────────────────────────────────────────

export interface GrantMilestone {
  id: string;
  clientId: string;
  vestingGrantId: string;
  milestoneTemplateId?: string;
  name: string;
  description?: string;
  category: MilestoneCategory;
  metricType: MetricType;
  targetOperator: TargetOperator;
  targetValue: number;
  targetUnit?: string;
  measurementFrequency: MeasurementFrequency;
  accelerationType: VestingAccelerationType;
  accelerationAmount: number;
  maxAccelerationCap?: number;
  effectiveCap: number;
  status: MilestoneStatus;
  currentValue?: number;
  progressPercentage: number;
  targetDate?: string;
  achievedDate?: string;
  verifiedAt?: string;
  verifiedBy?: string;
  accelerationApplied: boolean;
  notes?: string;
  createdAt: string;
  updatedAt: string;
}

export interface GrantMilestoneListResponse {
  items: GrantMilestone[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateGrantMilestoneRequest {
  vestingGrantId: string;
  milestoneTemplateId?: string;
  name: string;
  description?: string;
  category: MilestoneCategory;
  metricType: MetricType;
  targetOperator: TargetOperator;
  targetValue: number;
  targetUnit?: string;
  measurementFrequency: MeasurementFrequency;
  accelerationType: VestingAccelerationType;
  accelerationAmount: number;
  maxAccelerationCap?: number;
  targetDate?: string;
  notes?: string;
}

export interface AchieveGrantMilestoneRequest {
  achievedDate: string;
  achievedValue?: number;
  notes?: string;
}

export interface VerifyGrantMilestoneRequest {
  notes?: string;
}

export interface GrantMilestoneFilters {
  companyId?: string;
  grantId?: string;
  status?: string;
  category?: string;
  page?: number;
  pageSize?: number;
}

// ─── Milestone Progress ───────────────────────────────────────────────────────

export interface MilestoneProgress {
  id: string;
  clientId: string;
  grantMilestoneId: string;
  recordedDate: string;
  recordedValue: number;
  progressPercentage: number;
  notes?: string;
  dataSource?: ProgressDataSource;
  recordedBy?: string;
  createdAt: string;
}

export interface RecordMilestoneProgressRequest {
  recordedDate: string;
  recordedValue: number;
  notes?: string;
  dataSource?: ProgressDataSource;
}

// ─── Vesting Accelerations ────────────────────────────────────────────────────

export interface VestingAcceleration {
  id: string;
  clientId: string;
  vestingGrantId: string;
  grantMilestoneId: string;
  originalVestingEndDate: string;
  newVestingEndDate: string;
  sharesAccelerated: number;
  accelerationType: VestingAccelerationType;
  accelerationAmount: number;
  appliedAt: string;
  appliedBy: string;
  monthsAccelerated: number;
}

export interface AccelerationPreview {
  milestoneId: string;
  milestoneName: string;
  vestingGrantId: string;
  accelerationType: VestingAccelerationType;
  accelerationAmount: number;
  effectiveCap: number;
  currentEndDate: string;
  projectedNewEndDate: string;
  sharesUnlocked: number;
  monthsAccelerated: number;
  cappedByMaximum: boolean;
}

// ─── Dashboard ────────────────────────────────────────────────────────────────

export interface MilestoneProgressDashboard {
  vestingGrantId: string;
  totalMilestones: number;
  pendingCount: number;
  inProgressCount: number;
  achievedCount: number;
  failedCount: number;
  cancelledCount: number;
  totalPossibleAcceleration: number;
  pendingAcceleration: number;
  appliedAcceleration: number;
  milestones: GrantMilestone[];
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
  [MilestoneStatus.InProgress]: 'Em Andamento',
  [MilestoneStatus.Achieved]: 'Atingido',
  [MilestoneStatus.Failed]: 'Falhou',
  [MilestoneStatus.Cancelled]: 'Cancelado',
};

export const milestoneCategoryLabels: Record<MilestoneCategory, string> = {
  [MilestoneCategory.Financial]: 'Financeiro',
  [MilestoneCategory.Operational]: 'Operacional',
  [MilestoneCategory.Product]: 'Produto',
  [MilestoneCategory.Market]: 'Mercado',
  [MilestoneCategory.Strategic]: 'Estratégico',
};

export const metricTypeLabels: Record<MetricType, string> = {
  [MetricType.Revenue]: 'Receita',
  [MetricType.Profit]: 'Lucro',
  [MetricType.Ebitda]: 'EBITDA',
  [MetricType.UserCount]: 'Nº de Usuários',
  [MetricType.Mrr]: 'MRR',
  [MetricType.Arr]: 'ARR',
  [MetricType.CustomerCount]: 'Nº de Clientes',
  [MetricType.Nps]: 'NPS',
  [MetricType.MarketShare]: 'Market Share',
  [MetricType.ProductMilestone]: 'Marco de Produto',
  [MetricType.Custom]: 'Personalizado',
};

export const targetOperatorLabels: Record<TargetOperator, string> = {
  [TargetOperator.GreaterThan]: '>',
  [TargetOperator.GreaterThanOrEqual]: '≥',
  [TargetOperator.LessThan]: '<',
  [TargetOperator.LessThanOrEqual]: '≤',
  [TargetOperator.Equal]: '=',
};

export const measurementFrequencyLabels: Record<MeasurementFrequency, string> = {
  [MeasurementFrequency.OneTime]: 'Único',
  [MeasurementFrequency.Monthly]: 'Mensal',
  [MeasurementFrequency.Quarterly]: 'Trimestral',
  [MeasurementFrequency.Annual]: 'Anual',
};

export const vestingAccelerationTypeLabels: Record<VestingAccelerationType, string> = {
  [VestingAccelerationType.Percentage]: 'Porcentagem do período',
  [VestingAccelerationType.Months]: 'Meses adiantados',
  [VestingAccelerationType.Shares]: 'Ações desbloqueadas',
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
