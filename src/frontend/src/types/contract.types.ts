// Interface para metadados do contrato (usada no builder)
export interface ContractMetadata {
  title: string;
  description?: string;
  contractDate?: string;
  expirationDate?: string;
}
// F3-FE-TYPES: Contract-related types
// File: src/frontend/src/types/contract.types.ts
// Author: GitHub Copilot
// Date: 13/02/2026

/**
 * ============================================
 * ENUMS (mirroring backend)
 * ============================================
 */

export enum ContractTemplateType {
  StockOption = 'StockOption',
  ShareholdersAgreement = 'ShareholdersAgreement',
  NDA = 'NDA',
  Investment = 'Investment',
  Employment = 'Employment',
  ServiceAgreement = 'ServiceAgreement',
  Partnership = 'Partnership',
  Confidentiality = 'Confidentiality',
  Other = 'Other'
}

export enum ClauseType {
  Governance = 'Governance',
  RightsObligations = 'RightsObligations',
  Compliance = 'Compliance',
  Financial = 'Financial',
  Termination = 'Termination',
  Confidentiality = 'Confidentiality',
  DisputeResolution = 'DisputeResolution',
  Amendments = 'Amendments',
  General = 'General'
}

export enum ContractStatus {
  Draft = 'Draft',
  PendingReview = 'PendingReview',
  Approved = 'Approved',
  SentForSignature = 'SentForSignature',
  PartiallySigned = 'PartiallySigned',
  Signed = 'Signed',
  Executed = 'Executed',
  Expired = 'Expired',
  Cancelled = 'Cancelled'
}

export enum SignatureStatus {
  Pending = 'Pending',
  WaitingSignature = 'WaitingSignature',
  Signed = 'Signed',
  Rejected = 'Rejected',
  Expired = 'Expired'
}

/**
 * ============================================
 * DTO AND ENTITY TYPES
 * ============================================
 */

/**
 * Reusable contract template
 */
export interface ContractTemplate {
  id: string;
  clientId: string;
  companyId?: string;
  name: string;
  description: string;
  code: string;
  templateType: ContractTemplateType;
  content: string; // HTML/Text with {{variables}}
  defaultStatus: ContractStatus;
  tags: string[];
  version: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
  isDeleted: boolean;
}

export interface CreateContractTemplateRequest {
  name: string;
  code: string;
  templateType: ContractTemplateType;
  content: string;
  description?: string;
  companyId?: string;
  defaultStatus?: ContractStatus;
  tags?: string[];
}

export interface UpdateContractTemplateRequest {
  name?: string;
  description?: string;
  content?: string;
  tags?: string[];
  isActive?: boolean;
}

/**
 * Standardized clause from library
 */
export interface Clause {
  id: string;
  clientId: string;
  name: string;
  description: string;
  code: string;
  content: string; // Text with {{variables}}
  clauseType: ClauseType;
  isMandatory: boolean;
  tags: string[];
  displayOrder: number;
  version: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
  isDeleted: boolean;
}

export interface CreateClauseRequest {
  name: string;
  code: string;
  content: string;
  clauseType: ClauseType;
  isMandatory?: boolean;
  description?: string;
  tags?: string[];
  displayOrder?: number;
}

export interface UpdateClauseRequest {
  name?: string;
  description?: string;
  content?: string;
  tags?: string[];
  isMandatory?: boolean;
  displayOrder?: number;
  isActive?: boolean;
}

/**
 * Main contract document
 */
export interface Contract {
  id: string;
  clientId: string;
  companyId: string;
  title: string;
  description: string;
  contractType: ContractTemplateType;
  templateId?: string;
  documentPath?: string;
  documentSize?: number;
  documentHash?: string;
  status: ContractStatus;
  contractDate?: string;
  expirationDate?: string;
  externalReference?: string;
  notes?: string;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
  isDeleted: boolean;
  // Navigation
  parties?: ContractParty[];
  clauses?: ContractClause[];
}

export interface CreateContractRequest {
  title: string;
  contractType: ContractTemplateType;
  templateId?: string;
  description?: string;
  contractDate?: string;
  expirationDate?: string;
}

export interface UpdateContractRequest {
  title?: string;
  description?: string;
  contractDate?: string;
  expirationDate?: string;
  notes?: string;
  status?: ContractStatus;
}

/**
 * Party to a contract (signer, recipient, etc)
 */
export interface ContractParty {
  id: string;
  contractId: string;
  partyType: string; // "signer", "recipient", "witness", etc
  partyName: string;
  partyEmail: string;
  userId?: string;
  shareholderId?: string;
  signatureStatus: SignatureStatus;
  signatureDate?: string;
  signatureToken?: string;
  externalId?: string;
  sequenceOrder: number;
  createdAt: string;
  updatedAt: string;
  isDeleted: boolean;
}

export interface CreateContractPartyRequest {
  partyName: string;
  partyEmail: string;
  partyType?: string;
  userId?: string;
  shareholderId?: string;
  sequenceOrder?: number;
}

export interface UpdateContractPartyRequest {
  partyName?: string;
  partyEmail?: string;
  partyType?: string;
}

/**
 * Clause instance in a contract with customizations
 */
export interface ContractClause {
  id: string;
  contractId: string;
  clauseId: string;
  customContent?: string;
  displayOrder: number;
  isMandatory: boolean;
  clauseVariables: Record<string, string>;
  notes?: string;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
  isDeleted: boolean;
  // Navigation
  baseClause?: Clause;
}

export interface CreateContractClauseRequest {
  clauseId: string;
  displayOrder?: number;
  isMandatory?: boolean;
  variables?: Record<string, string>;
  customContent?: string;
  notes?: string;
}

export interface UpdateContractClauseRequest {
  customContent?: string;
  displayOrder?: number;
  variables?: Record<string, string>;
  notes?: string;
}

/**
 * ============================================
 * LIST RESPONSES WITH PAGINATION
 * ============================================
 */

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ContractTemplateListResponse extends PaginatedResponse<ContractTemplate> {}

export interface ClauseListResponse extends PaginatedResponse<Clause> {}

export interface ContractListResponse extends PaginatedResponse<Contract> {
  totalValue?: number;
  totalContracts?: number;
}

/**
 * ============================================
 * FILTER TYPES
 * ============================================
 */

export interface ContractTemplateFilters {
  search?: string;
  templateType?: ContractTemplateType;
  isActive?: boolean;
  tags?: string[];
  page?: number;
  pageSize?: number;
}

export interface ClauseFilters {
  search?: string;
  clauseType?: ClauseType;
  isMandatory?: boolean;
  isActive?: boolean;
  tags?: string[];
  page?: number;
  pageSize?: number;
}

export interface ContractFilters {
  search?: string;
  status?: ContractStatus;
  contractType?: ContractTemplateType;
  companyId?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

/**
 * ============================================
 * BUILDER TYPES (5-Step Wizard)
 * ============================================
 */

/**
 * Builder state for multi-step wizard
 */
export interface ContractBuilderState {
  // Step 1: Template selection
  templateId?: string;
  selectedTemplate?: ContractTemplate;

  // Step 2: Parties
  parties: CreateContractPartyRequest[];

  // Step 3: Clauses
  selectedClauseIds: string[];
  customClauses: Record<string, UpdateContractClauseRequest>;

  // Step 4: Data
  contractTitle: string;
  contractDescription: string;
  contractDate?: string;
  expirationDate?: string;
  templateVariables: Record<string, string>;

  // Step 5: Review
  generatedDocument?: string; // Base64 or URL
  documentHash?: string;

  // Metadata
  currentStep: number;
  isValid: boolean;
}

export interface StepValidationResult {
  isValid: boolean;
  errors: string[];
}

/**
 * ============================================
 * API REQUEST/RESPONSE HELPERS
 * ============================================
 */

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

/**
 * Display mapping helpers
 */
export const ContractStatusColors: Record<ContractStatus, string> = {
  [ContractStatus.Draft]: 'bg-gray-100 text-gray-800',
  [ContractStatus.PendingReview]: 'bg-amber-100 text-amber-800',
  [ContractStatus.Approved]: 'bg-blue-100 text-blue-800',
  [ContractStatus.SentForSignature]: 'bg-purple-100 text-purple-800',
  [ContractStatus.PartiallySigned]: 'bg-orange-100 text-orange-800',
  [ContractStatus.Signed]: 'bg-green-100 text-green-800',
  [ContractStatus.Executed]: 'bg-green-200 text-green-900',
  [ContractStatus.Expired]: 'bg-red-100 text-red-800',
  [ContractStatus.Cancelled]: 'bg-gray-200 text-gray-900'
};

export const SignatureStatusColors: Record<SignatureStatus, string> = {
  [SignatureStatus.Pending]: 'bg-gray-100 text-gray-800',
  [SignatureStatus.WaitingSignature]: 'bg-yellow-100 text-yellow-800',
  [SignatureStatus.Signed]: 'bg-green-100 text-green-800',
  [SignatureStatus.Rejected]: 'bg-red-100 text-red-800',
  [SignatureStatus.Expired]: 'bg-red-100 text-red-800'
};

export const TemplateTypeDisplayNames: Record<ContractTemplateType, string> = {
  [ContractTemplateType.StockOption]: 'Stock Option Agreement',
  [ContractTemplateType.ShareholdersAgreement]: 'Shareholders Agreement',
  [ContractTemplateType.NDA]: 'Non-Disclosure Agreement',
  [ContractTemplateType.Investment]: 'Investment Agreement',
  [ContractTemplateType.Employment]: 'Employment Contract',
  [ContractTemplateType.ServiceAgreement]: 'Service Agreement',
  [ContractTemplateType.Partnership]: 'Partnership Agreement',
  [ContractTemplateType.Confidentiality]: 'Confidentiality Agreement',
  [ContractTemplateType.Other]: 'Other'
};

export const ClauseTypeDisplayNames: Record<ClauseType, string> = {
  [ClauseType.Governance]: 'Governance',
  [ClauseType.RightsObligations]: 'Rights & Obligations',
  [ClauseType.Compliance]: 'Compliance',
  [ClauseType.Financial]: 'Financial',
  [ClauseType.Termination]: 'Termination',
  [ClauseType.Confidentiality]: 'Confidentiality',
  [ClauseType.DisputeResolution]: 'Dispute Resolution',
  [ClauseType.Amendments]: 'Amendments',
  [ClauseType.General]: 'General'
};

// ============================================================
// Version History types (Fase 4)
// ============================================================

export interface ContractVersion {
  id: string;
  contractId: string;
  versionNumber: number;
  fileType: 'pdf' | 'docx';
  source: 'builder' | 'upload';
  fileSize?: number;
  fileHash?: string;
  notes?: string;
  createdAt: string;
  createdBy?: string;
}

export interface UploadContractRequest {
  companyId: string;
  title: string;
  contractType: ContractTemplateType;
  description?: string;
  notes?: string;
}

export interface UploadVersionRequest {
  notes?: string;
}

