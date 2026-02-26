// Simulation Types for Round Simulator

// ============================================================================
// ENUMS
// ============================================================================

export enum RoundType {
  Equity = 1,
  ConvertibleNote = 2,
  SAFE = 3,
}

export enum AcquisitionType {
  Primary = 1,
  Secondary = 2,
}

export const acquisitionTypeLabels: Record<AcquisitionType, string> = {
  [AcquisitionType.Primary]: 'Emissão Primária (novas ações)',
  [AcquisitionType.Secondary]: 'Aquisição Secundária (sem novas ações)',
};

// ============================================================================
// REQUEST TYPES
// ============================================================================

export interface NewInvestorRequest {
  name: string;
  investmentAmount: number;
  email?: string;
  document?: string;
}

export interface RoundSimulationRequest {
  companyId: string;
  preMoneyValuation: number;
  investmentAmount: number;
  roundName: string;
  roundType: RoundType;
  newShareClassId?: string;
  newShareClassName?: string;
  newInvestors: NewInvestorRequest[];
  includeOptionPool: boolean;
  optionPoolPercentage: number;
  optionPoolPreMoney: boolean;
  acquisitionType?: AcquisitionType;
  includeVesting?: boolean;
}

export interface ScenarioCompareRequest {
  scenarios: RoundSimulationRequest[];
}

// ============================================================================
// RESPONSE TYPES
// ============================================================================

export interface SimulatedShareholderEntry {
  shareholderId?: string;
  shareholderName: string;
  shareholderType: string;
  shares: number;
  ownership: number;
  value: number;
  dilutionPercentage: number;
  isNewInvestor: boolean;
}

export interface SimulatedNewInvestor {
  name: string;
  investmentAmount: number;
  sharesReceived: number;
  ownershipPercentage: number;
  valueAtRound: number;
}

export interface OptionPoolInfo {
  percentage: number;
  shares: number;
  isPreMoney: boolean;
  value: number;
}

export interface RoundSimulationResponse {
  roundName: string;
  preMoneyValuation: number;
  investmentAmount: number;
  postMoneyValuation: number;
  pricePerShare: number;
  sharesBefore: number;
  newSharesIssued: number;
  sharesAfter: number;
  totalDilution: number;
  capTableBefore: SimulatedShareholderEntry[];
  capTableAfter: SimulatedShareholderEntry[];
  newInvestors: SimulatedNewInvestor[];
  optionPool?: OptionPoolInfo;
  acquisitionType?: AcquisitionType;
  vestingEntries: VestingSimulationEntry[];
  fullyDilutedCapTable: SimulatedShareholderEntry[];
  fullyDilutedShares: number;
  simulatedAt: string;
}

export interface VestingSimulationEntry {
  grantId: string;
  shareholderName: string;
  planName: string;
  totalShares: number;
  vestedShares: number;
  unvestedShares: number;
  exercisedShares: number;
  remainingShares: number;
  vestedPercentage: number;
  fullyDilutedOwnership: number;
  vestingEndDate: string;
  status: string;
}

export interface DilutionResponse {
  investmentAmount: number;
  preMoneyValuation: number;
  postMoneyValuation: number;
  dilutionPercentage: number;
  investorOwnership: number;
}

export interface ScenarioCompareResponse {
  scenarios: RoundSimulationResponse[];
}

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

export const getRoundTypeLabel = (type: RoundType): string => {
  const labels: Record<RoundType, string> = {
    [RoundType.Equity]: 'Equity',
    [RoundType.ConvertibleNote]: 'Nota Conversível',
    [RoundType.SAFE]: 'SAFE',
  };
  return labels[type] || 'Desconhecido';
};

export const getRoundTypeColor = (type: RoundType): string => {
  const colors: Record<RoundType, string> = {
    [RoundType.Equity]: 'green',
    [RoundType.ConvertibleNote]: 'blue',
    [RoundType.SAFE]: 'purple',
  };
  return colors[type] || 'gray';
};

export const formatCurrency = (value: number): string => {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(value);
};

export const formatPercentage = (value: number, decimals = 2): string => {
  return `${value.toFixed(decimals)}%`;
};

export const formatShares = (value: number): string => {
  return new Intl.NumberFormat('pt-BR').format(value);
};

// Common round name suggestions
export const roundNameSuggestions = [
  'Pre-Seed',
  'Seed',
  'Series A',
  'Series B',
  'Series C',
  'Series D',
  'Bridge',
  'Extension',
];
