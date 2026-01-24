export enum DocumentType {
  Cpf = 0,
  Cnpj = 1,
}

export enum ClientStatus {
  Active = 0,
  Inactive = 1,
  Suspended = 2,
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
  cnpj: string;
  status: number;
  createdAt: string;
}
