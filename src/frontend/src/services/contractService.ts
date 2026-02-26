import { api } from './api';
import type {
  Contract,
  ContractTemplate,
  Clause,
  ContractListResponse,
  ContractTemplateListResponse,
  ClauseListResponse,
  CreateContractRequest,
  UpdateContractRequest,
  CreateContractTemplateRequest,
  UpdateContractTemplateRequest,
  CreateClauseRequest,
  UpdateClauseRequest,
  CreateContractPartyRequest,
  UpdateContractPartyRequest,
  CreateContractClauseRequest,
  UpdateContractClauseRequest,
  ContractFilters,
  ContractTemplateFilters,
  ClauseFilters,
  ContractParty,
  ContractClause,
} from '@/types/contract.types';

// Tipo para resposta wrapper da API
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

/**
 * ====================================
 * CONTRACT SERVICE
 * ====================================
 */
export const contractService = {
  /**
   * Listar contratos com paginação e filtros
   */
  async getContracts(filters?: ContractFilters): Promise<ContractListResponse> {
    const params: Record<string, string | number | undefined> = {};
    
    if (filters?.page) params.pageNumber = filters.page;
    if (filters?.pageSize) params.pageSize = filters.pageSize;
    if (filters?.search) params.search = filters.search;
    if (filters?.status) params.status = filters.status;
    if (filters?.contractType) params.contractType = filters.contractType;
    if (filters?.companyId) params.companyId = filters.companyId;
    if (filters?.fromDate) params.fromDate = filters.fromDate;
    if (filters?.toDate) params.toDate = filters.toDate;
    
    const response = await api.get<ApiResponse<ContractListResponse>>('/contracts', { params });
    return response.data.data;
  },

  /**
   * Obter um contrato específico pelo ID
   */
  async getContractById(id: string): Promise<Contract> {
    const response = await api.get<ApiResponse<Contract>>(`/contracts/${id}`);
    return response.data.data;
  },

  /**
   * Criar um novo contrato
   */
  async createContract(data: CreateContractRequest): Promise<Contract> {
    const response = await api.post<ApiResponse<Contract>>('/contracts', data);
    return response.data.data;
  },

  /**
   * Atualizar um contrato existente
   */
  async updateContract(id: string, data: UpdateContractRequest): Promise<Contract> {
    const response = await api.put<ApiResponse<Contract>>(`/contracts/${id}`, data);
    return response.data.data;
  },

  /**
   * Excluir um contrato (soft delete)
   */
  async deleteContract(id: string): Promise<void> {
    await api.delete(`/contracts/${id}`);
  },

  /**
   * Fazer download do PDF do contrato
   */
  async downloadContractPdf(id: string): Promise<Blob> {
    const response = await api.get(`/contracts/${id}/download`, {
      responseType: 'blob',
    });
    return response.data;
  },

  /**
   * Gerar nova versão do contrato (regenerar PDF)
   */
  async regenerateContract(id: string): Promise<Contract> {
    const response = await api.post<ApiResponse<Contract>>(`/contracts/${id}/regenerate`);
    return response.data.data;
  },

  // ---- Party Management ----

  /**
   * Adicionar uma parte ao contrato
   */
  async addParty(contractId: string, data: CreateContractPartyRequest): Promise<ContractParty> {
    const response = await api.post<ApiResponse<ContractParty>>(
      `/contracts/${contractId}/parties`,
      data
    );
    return response.data.data;
  },

  /**
   * Atualizar uma parte do contrato
   */
  async updateParty(partyId: string, data: UpdateContractPartyRequest): Promise<ContractParty> {
    const response = await api.put<ApiResponse<ContractParty>>(
      `/contracts/parties/${partyId}`,
      data
    );
    return response.data.data;
  },

  /**
   * Remover uma parte do contrato
   */
  async removeParty(partyId: string): Promise<void> {
    await api.delete(`/contracts/parties/${partyId}`);
  },

  // ---- Clause Management ----

  /**
   * Adicionar uma cláusula ao contrato
   */
  async addClause(contractId: string, data: CreateContractClauseRequest): Promise<ContractClause> {
    const response = await api.post<ApiResponse<ContractClause>>(
      `/contracts/${contractId}/clauses`,
      data
    );
    return response.data.data;
  },

  /**
   * Atualizar uma cláusula do contrato
   */
  async updateClause(clauseId: string, data: UpdateContractClauseRequest): Promise<ContractClause> {
    const response = await api.put<ApiResponse<ContractClause>>(
      `/contracts/clauses/${clauseId}`,
      data
    );
    return response.data.data;
  },

  /**
   * Remover uma cláusula do contrato
   */
  async removeClause(clauseId: string): Promise<void> {
    await api.delete(`/contracts/clauses/${clauseId}`);
  },

  /**
   * Reordenar cláusulas do contrato
   */
  async reorderClauses(
    contractId: string,
    clauseOrders: Array<{ clauseId: string; displayOrder: number }>
  ): Promise<void> {
    await api.put(`/contracts/${contractId}/clauses/order`, { clauses: clauseOrders });
  },
};

/**
 * ====================================
 * CONTRACT TEMPLATE SERVICE
 * ====================================
 */
export const contractTemplateService = {
  /**
   * Listar templates com paginação e filtros
   */
  async getTemplates(filters?: ContractTemplateFilters): Promise<ContractTemplateListResponse> {
    const params: Record<string, string | number | boolean | undefined> = {};
    
    if (filters?.page) params.pageNumber = filters.page;
    if (filters?.pageSize) params.pageSize = filters.pageSize;
    if (filters?.search) params.search = filters.search;
    if (filters?.templateType) params.templateType = filters.templateType;
    if (filters?.isActive !== undefined) params.isActive = filters.isActive;
    if (filters?.tags && filters.tags.length > 0) params.tags = filters.tags.join(',');
    
    const response = await api.get<ApiResponse<ContractTemplateListResponse>>(
      '/contract-templates',
      { params }
    );
    return response.data.data;
  },

  /**
   * Obter um template específico pelo ID
   */
  async getTemplateById(id: string): Promise<ContractTemplate> {
    const response = await api.get<ApiResponse<ContractTemplate>>(`/contract-templates/${id}`);
    return response.data.data;
  },

  /**
   * Criar um novo template
   */
  async createTemplate(data: CreateContractTemplateRequest): Promise<ContractTemplate> {
    const response = await api.post<ApiResponse<ContractTemplate>>('/contract-templates', data);
    return response.data.data;
  },

  /**
   * Atualizar um template existente
   */
  async updateTemplate(id: string, data: UpdateContractTemplateRequest): Promise<ContractTemplate> {
    const response = await api.put<ApiResponse<ContractTemplate>>(
      `/contract-templates/${id}`,
      data
    );
    return response.data.data;
  },

  /**
   * Excluir um template (soft delete)
   */
  async deleteTemplate(id: string): Promise<void> {
    await api.delete(`/contract-templates/${id}`);
  },

  /**
   * Ativar um template
   */
  async activateTemplate(id: string): Promise<ContractTemplate> {
    const response = await api.patch<ApiResponse<ContractTemplate>>(
      `/contract-templates/${id}/activate`
    );
    return response.data.data;
  },

  /**
   * Desativar um template
   */
  async deactivateTemplate(id: string): Promise<ContractTemplate> {
    const response = await api.patch<ApiResponse<ContractTemplate>>(
      `/contract-templates/${id}/deactivate`
    );
    return response.data.data;
  },

  /**
   * Clonar um template existente
   */
  async cloneTemplate(id: string, newName: string): Promise<ContractTemplate> {
    const response = await api.post<ApiResponse<ContractTemplate>>(
      `/contract-templates/${id}/clone`,
      { newName }
    );
    return response.data.data;
  },
};

/**
 * ====================================
 * CLAUSE SERVICE
 * ====================================
 */
export const clauseService = {
  /**
   * Listar cláusulas com paginação e filtros
   */
  async getClauses(filters?: ClauseFilters): Promise<ClauseListResponse> {
    const params: Record<string, string | number | boolean | undefined> = {};
    
    if (filters?.page) params.pageNumber = filters.page;
    if (filters?.pageSize) params.pageSize = filters.pageSize;
    if (filters?.search) params.search = filters.search;
    if (filters?.clauseType) params.clauseType = filters.clauseType;
    if (filters?.isMandatory !== undefined) params.isMandatory = filters.isMandatory;
    if (filters?.isActive !== undefined) params.isActive = filters.isActive;
    if (filters?.tags && filters.tags.length > 0) params.tags = filters.tags.join(',');
    
    const response = await api.get<ApiResponse<ClauseListResponse>>('/clauses', { params });
    return response.data.data;
  },

  /**
   * Obter uma cláusula específica pelo ID
   */
  async getClauseById(id: string): Promise<Clause> {
    const response = await api.get<ApiResponse<Clause>>(`/clauses/${id}`);
    return response.data.data;
  },

  /**
   * Criar uma nova cláusula
   */
  async createClause(data: CreateClauseRequest): Promise<Clause> {
    const response = await api.post<ApiResponse<Clause>>('/clauses', data);
    return response.data.data;
  },

  /**
   * Atualizar uma cláusula existente
   */
  async updateClause(id: string, data: UpdateClauseRequest): Promise<Clause> {
    const response = await api.put<ApiResponse<Clause>>(`/clauses/${id}`, data);
    return response.data.data;
  },

  /**
   * Excluir uma cláusula (soft delete)
   */
  async deleteClause(id: string): Promise<void> {
    await api.delete(`/clauses/${id}`);
  },

  /**
   * Ativar uma cláusula
   */
  async activateClause(id: string): Promise<Clause> {
    const response = await api.patch<ApiResponse<Clause>>(`/clauses/${id}/activate`);
    return response.data.data;
  },

  /**
   * Desativar uma cláusula
   */
  async deactivateClause(id: string): Promise<Clause> {
    const response = await api.patch<ApiResponse<Clause>>(`/clauses/${id}/deactivate`);
    return response.data.data;
  },
};

/**
 * ====================================
 * CONTRACT BUILDER SERVICE (5-Step Wizard)
 * ====================================
 */

export interface BuilderSessionResponse {
  sessionId: string;
  clientId: string;
  companyId: string;
  templateId?: string;
  currentStep: number;
  parties: CreateContractPartyRequest[];
  selectedClauseIds: string[];
  variables: Record<string, string>;
  expiresAt: string;
  createdAt: string;
  lastAccessedAt: string;
}

export interface StartBuilderRequest {
  templateId?: string;
}

export interface AddPartiesRequest {
  sessionId: string;
  parties: CreateContractPartyRequest[];
}

export interface SelectClausesRequest {
  sessionId: string;
  clauseIds: string[];
}

export interface FillDataRequest {
  sessionId: string;
  title: string;
  description?: string;
  contractDate?: string;
  expirationDate?: string;
  variables: Record<string, string>;
}

export interface PreviewContractRequest {
  sessionId: string;
}

export interface GenerateContractRequest {
  sessionId: string;
  format?: 'html' | 'pdf';
}

export interface PreviewContractResponse {
  htmlContent: string;
  extractedVariables: string[];
  missingVariables: string[];
}

export const contractBuilderService = {
  /**
   * Step 1: Iniciar sessão do builder
   */
  async startBuilder(data?: StartBuilderRequest): Promise<BuilderSessionResponse> {
    const response = await api.post<ApiResponse<BuilderSessionResponse>>(
      '/contractbuilder/start',
      data || {}
    );
    return response.data.data;
  },

  /**
   * Step 2: Adicionar partes
   */
  async addParties(data: AddPartiesRequest): Promise<BuilderSessionResponse> {
    const response = await api.post<ApiResponse<BuilderSessionResponse>>(
      '/contractbuilder/parties',
      data
    );
    return response.data.data;
  },

  /**
   * Step 3: Selecionar cláusulas
   */
  async selectClauses(data: SelectClausesRequest): Promise<BuilderSessionResponse> {
    const response = await api.post<ApiResponse<BuilderSessionResponse>>(
      '/contractbuilder/clauses',
      data
    );
    return response.data.data;
  },

  /**
   * Step 4: Preencher dados e variáveis
   */
  async fillData(data: FillDataRequest): Promise<BuilderSessionResponse> {
    const response = await api.post<ApiResponse<BuilderSessionResponse>>(
      '/contractbuilder/data',
      data
    );
    return response.data.data;
  },

  /**
   * Step 5: Preview do contrato
   */
  async previewContract(data: PreviewContractRequest): Promise<PreviewContractResponse> {
    const response = await api.post<ApiResponse<PreviewContractResponse>>(
      '/contractbuilder/preview',
      data
    );
    return response.data.data;
  },

  /**
   * Step 6: Gerar contrato final
   */
  async generateContract(data: GenerateContractRequest): Promise<Contract> {
    const response = await api.post<ApiResponse<Contract>>(
      '/contractbuilder/generate',
      data
    );
    return response.data.data;
  },

  /**
   * Obter estado da sessão
   */
  async getSession(sessionId: string): Promise<BuilderSessionResponse> {
    const response = await api.get<ApiResponse<BuilderSessionResponse>>(
      `/contractbuilder/${sessionId}`
    );
    return response.data.data;
  },

  /**
   * Cancelar/excluir sessão
   */
  async cancelSession(sessionId: string): Promise<void> {
    await api.delete(`/contractbuilder/${sessionId}`);
  },
};

/**
 * ====================================
 * CONTRACT VERSION SERVICE
 * ====================================
 */
export const contractVersionService = {
  /**
   * Create a new contract from an uploaded DOCX file
   */
  async createFromUpload(
    requestData: { companyId: string; title: string; contractType: string; description?: string; notes?: string },
    file: File
  ): Promise<import('@/types/contract.types').Contract> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('companyId', requestData.companyId);
    formData.append('title', requestData.title);
    formData.append('contractType', requestData.contractType);
    if (requestData.description) formData.append('description', requestData.description);
    if (requestData.notes) formData.append('notes', requestData.notes);

    const response = await api.post<ApiResponse<import('@/types/contract.types').Contract>>(
      '/contracts/upload',
      formData,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return response.data.data;
  },

  /**
   * Upload a new version of a contract
   */
  async uploadVersion(contractId: string, file: File, notes?: string): Promise<Contract> {
    const formData = new FormData();
    formData.append('file', file);
    if (notes) {
      formData.append('notes', notes);
    }

    const response = await api.post<ApiResponse<Contract>>(
      `/contracts/${contractId}/versions`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data.data;
  },

  /**
   * Get version history for a contract
   */
  async getVersions(contractId: string): Promise<import('@/types/contract.types').ContractVersion[]> {
    const response = await api.get<ApiResponse<import('@/types/contract.types').ContractVersion[]>>(
      `/contracts/${contractId}/versions`
    );
    return response.data.data;
  },

  /**
   * Get a specific version of a contract
   */
  async getVersion(contractId: string, versionId: string): Promise<any> {
    const response = await api.get<ApiResponse<any>>(
      `/contracts/${contractId}/versions/${versionId}`
    );
    return response.data.data;
  },

  /**
   * Download a specific version
   */
  async downloadVersion(contractId: string, versionId: string): Promise<Blob> {
    const response = await api.get(
      `/contracts/${contractId}/versions/${versionId}/download`,
      {
        responseType: 'blob',
      }
    );
    return response.data;
  },
};
