// F3-BLD-FE-004: Contract Builder Step 4 - Fill Data
// File: src/frontend/src/components/contracts/builder/Step4FillData.tsx
// Author: GitHub Copilot
// Date: 14/02/2026

import React from 'react';
import { FileText, Calendar, AlignLeft } from 'lucide-react';
import { Card } from '@/components/ui';

import type { ContractMetadata } from '@/types/contract.types';

interface Step4FillDataProps {
  metadata: ContractMetadata;
  variables: Record<string, string>;
  onUpdateMetadata: (metadata: ContractMetadata) => void;
  onUpdateVariables: (variables: Record<string, string>) => void;
  extractedVariables?: string[]; // Variables found in template/clauses
}

export const Step4FillData: React.FC<Step4FillDataProps> = ({
  metadata,
  variables,
  onUpdateMetadata,
  onUpdateVariables,
  extractedVariables = [],
}) => {
  const handleMetadataChange = (field: keyof ContractMetadata, value: string) => {
    onUpdateMetadata({
      ...metadata,
      [field]: value,
    });
  };

  const handleVariableChange = (variableName: string, value: string) => {
    onUpdateVariables({
      ...variables,
      [variableName]: value,
    });
  };

  // Common variable names that should be displayed with user-friendly labels
  const variableLabels: Record<string, string> = {
    CONTRACT_TITLE: 'Título do Contrato',
    CONTRACT_DATE: 'Data do Contrato',
    COMPANY_NAME: 'Nome da Empresa',
    COMPANY_ADDRESS: 'Endereço da Empresa',
    COMPANY_CNPJ: 'CNPJ da Empresa',
    PARTY_1_NAME: 'Nome da Parte 1',
    PARTY_2_NAME: 'Nome da Parte 2',
    VALUE: 'Valor',
    CURRENCY: 'Moeda',
    EFFECTIVE_DATE: 'Data de Vigência',
    TERMINATION_DATE: 'Data de Término',
    JURISDICTION: 'Jurisdição',
    // Add more as needed
  };

  const getVariableLabel = (variableName: string): string => {
    return variableLabels[variableName] || variableName.replace(/_/g, ' ').toLowerCase();
  };

  return (
    <div className="space-y-6">
      {/* Description */}
      <div className="text-center mb-8">
        <AlignLeft className="w-16 h-16 text-cyan-600 mx-auto mb-4" />
        <h3 className="text-lg font-medium text-gray-900 mb-2">
          Preencha os dados do contrato
        </h3>
        <p className="text-sm text-gray-500">
          Complete as informações básicas e preencha as variáveis do template selecionado.
        </p>
      </div>

      {/* Basic Metadata */}
      <Card className="p-6">
        <h4 className="text-md font-semibold text-gray-900 mb-4">
          <FileText className="w-5 h-5 inline mr-2" />
          Informações Básicas
        </h4>
        <div className="space-y-4">
          {/* Title */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Título do Contrato *
            </label>
            <input
              type="text"
              value={metadata.title}
              onChange={(e) => handleMetadataChange('title', e.target.value)}
              placeholder="Ex: Acordo de Parceria Estratégica - Empresa ABC"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
            />
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Descrição (opcional)
            </label>
            <textarea
              value={metadata.description || ''}
              onChange={(e) => handleMetadataChange('description', e.target.value)}
              placeholder="Breve descrição sobre o contrato"
              rows={3}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
            />
          </div>

          {/* Dates */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <Calendar className="w-4 h-4 inline mr-1" />
                Data do Contrato
              </label>
              <input
                type="date"
                value={metadata.contractDate || ''}
                onChange={(e) => handleMetadataChange('contractDate', e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <Calendar className="w-4 h-4 inline mr-1" />
                Data de Expiração (opcional)
              </label>
              <input
                type="date"
                value={metadata.expirationDate || ''}
                onChange={(e) => handleMetadataChange('expirationDate', e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
              />
            </div>
          </div>
        </div>
      </Card>

      {/* Template Variables */}
      {extractedVariables.length > 0 && (
        <Card className="p-6">
          <h4 className="text-md font-semibold text-gray-900 mb-4">
            <FileText className="w-5 h-5 inline mr-2" />
            Variáveis do Template ({extractedVariables.length})
          </h4>
          <p className="text-sm text-gray-600 mb-4">
            Preencha as variáveis abaixo. Elas serão substituídas no documento final.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {extractedVariables.map((varName) => (
              <div key={varName}>
                <label className="block text-sm font-medium text-gray-700 mb-2 capitalize">
                  {getVariableLabel(varName)}
                </label>
                <input
                  type="text"
                  value={variables[varName] || ''}
                  onChange={(e) => handleVariableChange(varName, e.target.value)}
                  placeholder={`Digite ${getVariableLabel(varName)}`}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
                />
                <p className="text-xs text-gray-500 mt-1">Variável: {'{{'}{varName}{'}}'}</p>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* No Variables Message */}
      {extractedVariables.length === 0 && (
        <Card className="p-6 bg-gray-50 border-2 border-dashed border-gray-300">
          <div className="text-center py-8">
            <FileText className="w-12 h-12 text-gray-300 mx-auto mb-3" />
            <h4 className="text-md font-medium text-gray-900 mb-2">
              Nenhuma variável detectada
            </h4>
            <p className="text-sm text-gray-600">
              O template selecionado não possui variáveis customizáveis ou você está criando
              um contrato do zero.
            </p>
          </div>
        </Card>
      )}

      {/* Common Variables Reference */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <h5 className="text-sm font-semibold text-blue-900 mb-2">
          💡 Variáveis Comuns
        </h5>
        <p className="text-xs text-blue-800 mb-2">
          As seguintes variáveis são comumente usadas em contratos:
        </p>
        <div className="grid grid-cols-2 gap-2 text-xs text-blue-700">
          <div>• {'{'}CONTRACT_TITLE{'}'} - Título do contrato</div>
          <div>• {'{'}CONTRACT_DATE{'}'} - Data do contrato</div>
          <div>• {'{'}COMPANY_NAME{'}'} - Nome da empresa</div>
          <div>• {'{'}PARTY_1_NAME{'}'} - Nome da primeira parte</div>
          <div>• {'{'}VALUE{'}'} - Valor monetário</div>
          <div>• {'{'}JURISDICTION{'}'} - Jurisdição legal</div>
        </div>
      </div>

      {/* Validation Info */}
      {metadata.title.trim() === '' && (
        <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
          <p className="text-sm text-amber-800">
            <strong>⚠️ Atenção:</strong> O título do contrato é obrigatório para prosseguir
            para o próximo passo.
          </p>
        </div>
      )}
    </div>
  );
};

export default Step4FillData;
