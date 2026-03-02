// Financial Types — mirrors backend DTOs and Entities

// ─── Enums ────────────────────────────────────────────────────────────────────

export enum FinancialPeriodStatus {
  Draft = 'draft',
  Submitted = 'submitted',
  Approved = 'approved',
  Locked = 'locked',
}

// ─── Labels ───────────────────────────────────────────────────────────────────

export const financialPeriodStatusLabels: Record<string, string> = {
  draft: 'Rascunho',
  submitted: 'Submetido',
  approved: 'Aprovado',
  locked: 'Bloqueado',
};

export const financialPeriodStatusColors: Record<string, string> = {
  draft: 'bg-gray-100 text-gray-700',
  submitted: 'bg-yellow-100 text-yellow-700',
  approved: 'bg-green-100 text-green-700',
  locked: 'bg-blue-100 text-blue-700',
};

export const monthNames: string[] = [
  'Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun',
  'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez',
];

export const runwayStatusColors: Record<string, string> = {
  green: 'text-green-600',
  yellow: 'text-yellow-600',
  red: 'text-red-600',
};

// ─── Interfaces ───────────────────────────────────────────────────────────────

export interface FinancialMetric {
  id: string;
  periodId: string;
  // Revenue
  grossRevenue: number | null;
  netRevenue: number | null;
  mrr: number | null;
  arr: number | null;
  // Cash & Burn
  cashBalance: number | null;
  burnRate: number | null;
  runwayMonths: number | null;
  runwayStatus: string | null;
  // Unit Economics
  customerCount: number | null;
  churnRate: number | null;
  cac: number | null;
  ltv: number | null;
  ltvToCacRatio: number | null;
  nps: number | null;
  // Profitability
  ebitda: number | null;
  ebitdaMargin: number | null;
  netIncome: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface FinancialPeriod {
  id: string;
  clientId: string;
  companyId: string;
  year: number;
  month: number;
  periodLabel: string;
  status: string;
  notes: string | null;
  submittedAt: string | null;
  approvedAt: string | null;
  lockedAt: string | null;
  createdAt: string;
  updatedAt: string;
  metrics: FinancialMetric | null;
}

export interface FinancialPeriodListResponse {
  items: FinancialPeriod[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface FinancialTrend {
  mrrGrowthPercent: number | null;
  arrCurrentMonth: number | null;
  avgBurnRate3Months: number | null;
  runwayMonths: number | null;
  runwayStatus: string | null;
  avgChurnRate3Months: number | null;
}

export interface FinancialDashboard {
  companyId: string;
  year: number;
  periods: FinancialPeriod[];
  trend: FinancialTrend | null;
}

// ─── Filters ──────────────────────────────────────────────────────────────────

export interface FinancialFilters {
  companyId?: string;
  page?: number;
  pageSize?: number;
  year?: number;
  status?: string;
}

// ─── Requests ─────────────────────────────────────────────────────────────────

export interface CreateFinancialPeriodRequest {
  companyId: string;
  year: number;
  month: number;
  notes?: string;
}

export interface UpdateFinancialPeriodRequest {
  notes?: string;
}

export interface UpsertRevenueRequest {
  grossRevenue?: number;
  netRevenue?: number;
  mrr?: number;
}

export interface UpsertCashBurnRequest {
  cashBalance?: number;
  burnRate?: number;
}

export interface UpsertUnitEconomicsRequest {
  customerCount?: number;
  churnRate?: number;
  cac?: number;
  ltv?: number;
  nps?: number;
}

export interface UpsertProfitabilityRequest {
  ebitda?: number;
  netIncome?: number;
}
