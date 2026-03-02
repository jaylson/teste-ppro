// Valuation Types — mirrors backend DTOs and Entities

// ─── Enums ────────────────────────────────────────────────────────────────────

export enum ValuationStatus {
  Draft = 'draft',
  PendingApproval = 'pending_approval',
  Approved = 'approved',
  Rejected = 'rejected',
}

export enum ValuationEventType {
  Seed = 'seed',
  SeriesA = 'series_a',
  SeriesB = 'series_b',
  SeriesC = 'series_c',
  SeriesD = 'series_d',
  Internal409A = 'internal_409a',
  Acquisition = 'acquisition',
  Ipo = 'ipo',
  SecondaryTransaction = 'secondary_transaction',
  Other = 'other',
}

export enum ValuationMethodType {
  ArrMultiple = 'arr_multiple',
  Dcf = 'dcf',
  Comparables = 'comparables',
  EbitdaMultiple = 'ebitda_multiple',
  MrrMultiple = 'mrr_multiple',
  AssetBased = 'asset_based',
  Berkus = 'berkus',
  Custom = 'custom',
}

// ─── Labels ───────────────────────────────────────────────────────────────────

export const valuationStatusLabels: Record<string, string> = {
  draft: 'Rascunho',
  pending_approval: 'Aguardando Aprovação',
  approved: 'Aprovado',
  rejected: 'Rejeitado',
};

export const valuationStatusColors: Record<string, string> = {
  draft: 'bg-gray-100 text-gray-700',
  pending_approval: 'bg-yellow-100 text-yellow-700',
  approved: 'bg-green-100 text-green-700',
  rejected: 'bg-red-100 text-red-700',
};

export const valuationMethodLabels: Record<string, string> = {
  arr_multiple: 'ARR Multiple',
  dcf: 'Fluxo de Caixa Descontado',
  comparables: 'Comparáveis de Mercado',
  ebitda_multiple: 'EBITDA Multiple',
  mrr_multiple: 'MRR Multiple',
  asset_based: 'Baseado em Ativos',
  berkus: 'Método Berkus',
  custom: 'Fórmula Customizada',
};

export const valuationEventTypeLabels: Record<string, string> = {
  seed: 'Seed',
  series_a: 'Series A',
  series_b: 'Series B',
  series_c: 'Series C',
  series_d: 'Series D',
  internal_409a: 'Internal 409A',
  acquisition: 'Aquisição',
  ipo: 'IPO',
  secondary_transaction: 'Transação Secundária',
  other: 'Outro',
};

// ─── Interfaces ───────────────────────────────────────────────────────────────

export interface ValuationMethod {
  id: string;
  valuationId: string;
  methodType: string;
  isSelected: boolean;
  calculatedValue: number | null;
  inputs: string | null;
  dataSource: string | null;
  notes: string | null;
  formulaVersionId: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface Valuation {
  id: string;
  clientId: string;
  companyId: string;
  valuationDate: string;
  eventType: string;
  eventName: string | null;
  valuationAmount: number | null;
  totalShares: number;
  pricePerShare: number | null;
  status: string;
  notes: string | null;
  submittedAt: string | null;
  approvedAt: string | null;
  rejectedAt: string | null;
  rejectionReason: string | null;
  createdAt: string;
  updatedAt: string;
  methods: ValuationMethod[];
}

export interface ValuationListResponse {
  items: Valuation[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ─── Filters ──────────────────────────────────────────────────────────────────

export interface ValuationFilters {
  companyId?: string;
  page?: number;
  pageSize?: number;
  status?: string;
  eventType?: string;
}

// ─── Requests ─────────────────────────────────────────────────────────────────

export interface CreateValuationRequest {
  companyId: string;
  valuationDate: string;
  eventType: string;
  eventName?: string;
  totalShares: number;
  notes?: string;
}

export interface UpdateValuationRequest {
  valuationDate: string;
  eventType: string;
  eventName?: string;
  totalShares: number;
  notes?: string;
}

export interface RejectValuationRequest {
  reason: string;
}

export interface AddValuationMethodRequest {
  methodType: string;
  inputsJson?: string;
  dataSource?: string;
  notes?: string;
  formulaVersionId?: string;
}

export interface CalculateMethodRequest {
  methodType: string;
  inputs: Record<string, number>;
  formulaVersionId?: string;
}

export interface CalculateMethodResponse {
  methodType: string;
  calculatedValue: number;
  breakdown: Record<string, unknown>;
  formulaExpression: string | null;
}
