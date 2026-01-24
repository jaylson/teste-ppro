export enum DocumentType {
  Cpf = 1,
  Cnpj = 2,
}

export enum ClientStatus {
  Active = 1,
  Inactive = 2,
  Suspended = 3,
}

export interface Client {
  id: string;
  name: string;
  document: string;
  documentType: DocumentType;
  email: string;
  phone?: string;
  status: ClientStatus;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateClientRequest {
  name: string;
  document: string;
  documentType: DocumentType;
  email: string;
  phone?: string;
}

export interface UpdateClientRequest {
  name?: string;
  email?: string;
  phone?: string;
}

export interface ClientListResponse {
  items: Client[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ClientCompany {
  id: string;
  name: string;
  cnpj?: string;
  cnpjFormatted?: string;
  valuation?: number;
  status: string | number;
  createdAt?: string;
}
