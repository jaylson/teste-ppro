// F3-FE-CONST: Contract constants and configurations
// File: src/frontend/src/constants/contractConstants.ts
// Author: GitHub Copilot
// Date: 13/02/2026

import {
  ContractStatus,
  SignatureStatus,
  ContractTemplateType,
  ClauseType
} from '@/types';

/**
 * ============================================
 * CONTRACT STATUS CONFIGURATION
 * ============================================
 */

export const CONTRACT_STATUS_CONFIG: Record<
  ContractStatus,
  {
    label: string;
    color: string;
    bgColor: string;
    textColor: string;
    icon: string; // lucide icon name
    description: string;
  }
> = {
  [ContractStatus.Draft]: {
    label: 'Rascunho',
    color: 'gray-600',
    bgColor: 'bg-gray-100',
    textColor: 'text-gray-800',
    icon: 'FileText',
    description: 'Contrato em edição'
  },
  [ContractStatus.PendingReview]: {
    label: 'Pendente Revisão',
    color: 'amber-600',
    bgColor: 'bg-amber-100',
    textColor: 'text-amber-800',
    icon: 'Clock',
    description: 'Aguardando revisão'
  },
  [ContractStatus.Approved]: {
    label: 'Aprovado',
    color: 'blue-600',
    bgColor: 'bg-blue-100',
    textColor: 'text-blue-800',
    icon: 'CheckCircle',
    description: 'Aprovado e pronto para assinatura'
  },
  [ContractStatus.SentForSignature]: {
    label: 'Enviado para Assinatura',
    color: 'purple-600',
    bgColor: 'bg-purple-100',
    textColor: 'text-purple-800',
    icon: 'Send',
    description: 'Enviado aos signatários'
  },
  [ContractStatus.PartiallySigned]: {
    label: 'Parcialmente Assinado',
    color: 'orange-600',
    bgColor: 'bg-orange-100',
    textColor: 'text-orange-800',
    icon: 'AlertCircle',
    description: 'Alguns signatários já assinaram'
  },
  [ContractStatus.Signed]: {
    label: 'Assinado',
    color: 'green-600',
    bgColor: 'bg-green-100',
    textColor: 'text-green-800',
    icon: 'CheckCircle',
    description: 'Todos os signatários assinaram'
  },
  [ContractStatus.Executed]: {
    label: 'Executado',
    color: 'emerald-700',
    bgColor: 'bg-emerald-100',
    textColor: 'text-emerald-900',
    icon: 'CheckCircle2',
    description: 'Contrato executado e finalizado'
  },
  [ContractStatus.Expired]: {
    label: 'Expirado',
    color: 'red-600',
    bgColor: 'bg-red-100',
    textColor: 'text-red-800',
    icon: 'AlertCircle',
    description: 'Contrato expirou'
  },
  [ContractStatus.Cancelled]: {
    label: 'Cancelado',
    color: 'slate-600',
    bgColor: 'bg-slate-100',
    textColor: 'text-slate-800',
    icon: 'XCircle',
    description: 'Contrato foi cancelado'
  }
};

/**
 * ============================================
 * SIGNATURE STATUS CONFIGURATION
 * ============================================
 */

export const SIGNATURE_STATUS_CONFIG: Record<
  SignatureStatus,
  {
    label: string;
    bgColor: string;
    textColor: string;
    icon: string;
  }
> = {
  [SignatureStatus.Pending]: {
    label: 'Pendente',
    bgColor: 'bg-gray-100',
    textColor: 'text-gray-800',
    icon: 'Clock'
  },
  [SignatureStatus.WaitingSignature]: {
    label: 'Aguardando Assinatura',
    bgColor: 'bg-yellow-100',
    textColor: 'text-yellow-800',
    icon: 'AlertCircle'
  },
  [SignatureStatus.Signed]: {
    label: 'Assinado',
    bgColor: 'bg-green-100',
    textColor: 'text-green-800',
    icon: 'CheckCircle'
  },
  [SignatureStatus.Rejected]: {
    label: 'Rejeitado',
    bgColor: 'bg-red-100',
    textColor: 'text-red-800',
    icon: 'XCircle'
  },
  [SignatureStatus.Expired]: {
    label: 'Expirado',
    bgColor: 'bg-red-100',
    textColor: 'text-red-800',
    icon: 'AlertCircle'
  }
};

/**
 * ============================================
 * TEMPLATE TYPE CONFIGURATION
 * ============================================
 */

export const CONTRACT_TEMPLATE_TYPE_CONFIG: Record<
  ContractTemplateType,
  {
    label: string;
    description: string;
    icon: string;
  }
> = {
  [ContractTemplateType.StockOption]: {
    label: 'Stock Option',
    description: 'Acordo de Opção de Ações',
    icon: 'TrendingUp'
  },
  [ContractTemplateType.ShareholdersAgreement]: {
    label: 'Acordo de Acionistas',
    description: 'Acordo entre acionistas',
    icon: 'Users'
  },
  [ContractTemplateType.NDA]: {
    label: 'Confidencialidade',
    description: 'Acordo de Não-Divulgação',
    icon: 'Lock'
  },
  [ContractTemplateType.Investment]: {
    label: 'Investimento',
    description: 'Acordo de Investimento',
    icon: 'DollarSign'
  },
  [ContractTemplateType.Employment]: {
    label: 'Emprego',
    description: 'Contrato de Trabalho',
    icon: 'Briefcase'
  },
  [ContractTemplateType.ServiceAgreement]: {
    label: 'Prestação de Serviço',
    description: 'Acordo de Serviços',
    icon: 'FileText'
  },
  [ContractTemplateType.Partnership]: {
    label: 'Parceria',
    description: 'Acordo de Parceria',
    icon: 'Handshake'
  },
  [ContractTemplateType.Confidentiality]: {
    label: 'Confidencialidade',
    description: 'Acordo de Confidencialidade',
    icon: 'EyeOff'
  },
  [ContractTemplateType.Other]: {
    label: 'Outro',
    description: 'Outro tipo de contrato',
    icon: 'FileText'
  }
};

/**
 * ============================================
 * CLAUSE TYPE CONFIGURATION
 * ============================================
 */

export const CLAUSE_TYPE_CONFIG: Record<
  ClauseType,
  {
    label: string;
    description: string;
    color: string;
  }
> = {
  [ClauseType.Governance]: {
    label: 'Governança',
    description: 'Cláusulas de governança corporativa',
    color: 'blue-600'
  },
  [ClauseType.RightsObligations]: {
    label: 'Direitos e Deveres',
    description: 'Direitos e obrigações das partes',
    color: 'purple-600'
  },
  [ClauseType.Compliance]: {
    label: 'Conformidade',
    description: 'Requisitos de conformidade legal',
    color: 'red-600'
  },
  [ClauseType.Financial]: {
    label: 'Financeira',
    description: 'Termos financeiros e pagamentos',
    color: 'green-600'
  },
  [ClauseType.Termination]: {
    label: 'Rescisão',
    description: 'Cláusulas de rescisão e encerramento',
    color: 'orange-600'
  },
  [ClauseType.Confidentiality]: {
    label: 'Confidencialidade',
    description: 'Proteção de informações confidenciais',
    color: 'indigo-600'
  },
  [ClauseType.DisputeResolution]: {
    label: 'Resolução de Disputas',
    description: 'Mecanismos de resolução de conflitos',
    color: 'amber-600'
  },
  [ClauseType.Amendments]: {
    label: 'Emendas',
    description: 'Procedimentos para alterações',
    color: 'cyan-600'
  },
  [ClauseType.General]: {
    label: 'Geral',
    description: 'Cláusulas gerais',
    color: 'gray-600'
  }
};

/**
 * ============================================
 * BUILDER CONFIGURATION
 * ============================================
 */

export const BUILDER_STEPS = [
  {
    id: 1,
    title: 'Selecionar Tipo',
    description: 'Escolha o tipo de contrato',
    icon: 'FileText'
  },
  {
    id: 2,
    title: 'Adicionar Partes',
    description: 'Defina os signatários',
    icon: 'Users'
  },
  {
    id: 3,
    title: 'Selecionar Cláusulas',
    description: 'Escolha as cláusulas a incluir',
    icon: 'List'
  },
  {
    id: 4,
    title: 'Preencher Dados',
    description: 'Complete os campos obrigatórios',
    icon: 'Edit'
  },
  {
    id: 5,
    title: 'Revisar',
    description: 'Visualize e gere o contrato',
    icon: 'Eye'
  }
];

/**
 * ============================================
 * TABLE CONFIGURATIONS
 * ============================================
 */

export const CONTRACTS_TABLE_COLUMNS = [
  { key: 'title', label: 'Título', width: '25%' },
  { key: 'contractType', label: 'Tipo', width: '15%' },
  { key: 'status', label: 'Status', width: '15%' },
  { key: 'parties', label: 'Partes', width: '15%' },
  { key: 'createdAt', label: 'Criado em', width: '15%' },
  { key: 'actions', label: 'Ações', width: '15%' }
];

export const TEMPLATES_TABLE_COLUMNS = [
  { key: 'name', label: 'Nome', width: '30%' },
  { key: 'templateType', label: 'Tipo', width: '20%' },
  { key: 'version', label: 'Versão', width: '10%' },
  { key: 'isActive', label: 'Status', width: '15%' },
  { key: 'createdAt', label: 'Criado em', width: '15%' },
  { key: 'actions', label: 'Ações', width: '10%' }
];

export const CLAUSES_TABLE_COLUMNS = [
  { key: 'name', label: 'Nome', width: '30%' },
  { key: 'clauseType', label: 'Tipo', width: '20%' },
  { key: 'isMandatory', label: 'Obrigatória', width: '15%' },
  { key: 'isActive', label: 'Ativa', width: '15%' },
  { key: 'createdAt', label: 'Criado em', width: '10%' },
  { key: 'actions', label: 'Ações', width: '10%' }
];

/**
 * ============================================
 * PAGINATION DEFAULTS
 * ============================================
 */

export const PAGINATION_DEFAULTS = {
  pageSize: 20,
  pageSizeOptions: [10, 20, 50, 100]
};

/**
 * ============================================
 * VALIDATION MESSAGES
 * ============================================
 */

export const CONTRACT_VALIDATION_MESSAGES = {
  titleRequired: 'Título do contrato é obrigatório',
  titleMinLength: 'Título deve ter pelo menos 3 caracteres',
  typeRequired: 'Tipo de contrato é obrigatório',
  templateContent: 'Conteúdo do template é obrigatório',
  clauseRequired: 'Pelo menos uma cláusula deve ser selecionada',
  partiesRequired: 'Pelo menos um signatário deve ser definido',
  partyEmailInvalid: 'Email do signatário inválido',
  dateInvalid: 'Datas inválidas'
};

/**
 * ============================================
 * API ENDPOINTS
 * ============================================
 */

export const CONTRACT_API_ENDPOINTS = {
  templates: '/api/contract-templates',
  clauses: '/api/clauses',
  contracts: '/api/contracts',
  parties: '/api/contract-parties',
  contractClauses: '/api/contract-clauses',
  builder: '/api/contract-builder',
  generate: '/api/contracts/generate'
};
