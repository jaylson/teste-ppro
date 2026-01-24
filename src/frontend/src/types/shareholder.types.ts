// Shareholder Types

// Re-export DocumentType from client.types
export { DocumentType } from './client.types';

export enum ShareholderType {
  Founder = 0,
  Investor = 1,
  Employee = 2,
  Advisor = 3,
  Other = 4,
}

export enum ShareholderStatus {
  Active = 0,
  Inactive = 1,
  Pending = 2,
}

// Import DocumentType for local use
import { DocumentType } from './client.types';

export interface Shareholder {
  id: string;
  clientId: string;
  companyId: string;
  companyName: string;
  name: string;
  document: string;
  documentFormatted: string;
  documentType: DocumentType;
  email?: string;
  phone?: string;
  type: ShareholderType;
  status: ShareholderStatus;
  notes?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ShareholderListResponse {
  items: Shareholder[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateShareholderRequest {
  companyId: string;
  name: string;
  document: string;
  documentType: DocumentType;
  type: ShareholderType;
  email?: string;
  phone?: string;
  notes?: string;
}

export interface UpdateShareholderRequest {
  companyId?: string;
  name: string;
  document?: string;
  documentType?: DocumentType;
  type: ShareholderType;
  status: ShareholderStatus;
  email?: string;
  phone?: string;
  notes?: string;
}

export interface ShareholderFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  type?: ShareholderType;
  status?: ShareholderStatus;
  companyId?: string;
}

// Helper functions for labels
export const shareholderTypeLabels: Record<ShareholderType, string> = {
  [ShareholderType.Founder]: 'Fundador',
  [ShareholderType.Investor]: 'Investidor',
  [ShareholderType.Employee]: 'Funcionário',
  [ShareholderType.Advisor]: 'Advisor',
  [ShareholderType.Other]: 'Outro',
};

export const shareholderStatusLabels: Record<ShareholderStatus, string> = {
  [ShareholderStatus.Active]: 'Ativo',
  [ShareholderStatus.Inactive]: 'Inativo',
  [ShareholderStatus.Pending]: 'Pendente',
};

export const documentTypeLabels: Record<DocumentType, string> = {
  [DocumentType.Cpf]: 'CPF',
  [DocumentType.Cnpj]: 'CNPJ',
};
