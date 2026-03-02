// Custom Formula Types — mirrors backend DTOs and Entities

// ─── Enums ────────────────────────────────────────────────────────────────────

export enum FormulaValidationStatus {
  Draft = 'draft',
  Validated = 'validated',
  Invalid = 'invalid',
}

export enum FormulaVariableType {
  Currency = 'currency',
  Percentage = 'percentage',
  Number = 'number',
  Integer = 'integer',
  Multiplier = 'multiplier',
  Boolean = 'boolean',
}

// ─── Labels ───────────────────────────────────────────────────────────────────

export const formulaValidationStatusLabels: Record<string, string> = {
  draft: 'Rascunho',
  validated: 'Validada',
  invalid: 'Inválida',
};

export const formulaValidationStatusColors: Record<string, string> = {
  draft: 'bg-gray-100 text-gray-700',
  validated: 'bg-green-100 text-green-700',
  invalid: 'bg-red-100 text-red-700',
};

export const formulaVariableTypeLabels: Record<string, string> = {
  currency: 'Moeda',
  percentage: 'Percentual',
  number: 'Número',
  integer: 'Inteiro',
  multiplier: 'Multiplicador',
  boolean: 'Booleano',
};

// ─── Interfaces ───────────────────────────────────────────────────────────────

export interface FormulaVariableDefinition {
  name: string;
  label: string;
  type: string;
  unit?: string;
  description?: string;
  isRequired: boolean;
  defaultValue?: number;
  minValue?: number;
  maxValue?: number;
  displayOrder: number;
}

export interface FormulaVersion {
  id: string;
  formulaId: string;
  versionNumber: number;
  expression: string;
  variables: FormulaVariableDefinition[];
  resultUnit: string;
  resultLabel: string | null;
  testInputs: string | null;
  testResult: number | null;
  validationStatus: string;
  validationErrors: string | null;
  createdAt: string;
}

export interface CustomFormula {
  id: string;
  clientId: string;
  companyId: string;
  name: string;
  description: string | null;
  sectorTag: string | null;
  currentVersionId: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  currentVersion: FormulaVersion | null;
}

export interface CustomFormulaListResponse {
  items: CustomFormula[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ─── Filters ──────────────────────────────────────────────────────────────────

export interface CustomFormulaFilters {
  companyId?: string;
  page?: number;
  pageSize?: number;
  isActive?: boolean;
  sectorTag?: string;
  search?: string;
}

// ─── Requests ─────────────────────────────────────────────────────────────────

export interface CreateCustomFormulaRequest {
  companyId: string;
  name: string;
  description?: string;
  sectorTag?: string;
  // Initial version
  expression: string;
  variables: FormulaVariableDefinition[];
  resultUnit: string;
  resultLabel?: string;
}

export interface UpdateFormulaMetadataRequest {
  name: string;
  description?: string;
  sectorTag?: string;
}

export interface PublishNewFormulaVersionRequest {
  expression: string;
  variables: FormulaVariableDefinition[];
  resultUnit: string;
  resultLabel?: string;
}

export interface TestFormulaRequest {
  expression: string;
  inputs: Record<string, number>;
}

export interface TestFormulaResponse {
  isValid: boolean;
  result: number | null;
  errors: string[];
  normalizedExpression: string | null;
}
