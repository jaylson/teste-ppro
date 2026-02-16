// F3-BLD-FE-001: Contract Builder Step 1 - Select Template
// File: src/frontend/src/components/contracts/builder/Step1SelectType.tsx
// Author: GitHub Copilot
// Date: 14/02/2026

import React, { useState, useEffect } from 'react';
import { FileText, Search, Star, CheckCircle, Loader2 } from 'lucide-react';
import { Card, Button } from '@/components/ui';
import type { ContractTemplate, ContractTemplateType } from '@/types/contract.types';
import { contractTemplateService } from '@/services/contractService';
import { CONTRACT_TEMPLATE_TYPE_CONFIG } from '@/constants/contractConstants';

interface Step1SelectTypeProps {
  onSelect: (templateId: string | null) => void;
  selectedTemplateId?: string | null;
}

export const Step1SelectType: React.FC<Step1SelectTypeProps> = ({
  onSelect,
  selectedTemplateId,
}) => {
  const [templates, setTemplates] = useState<ContractTemplate[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedType, setSelectedType] = useState<ContractTemplateType | 'all'>('all');
  const [selectedTemplate, setSelectedTemplate] = useState<string | null>(
    selectedTemplateId || null
  );

  useEffect(() => {
    loadTemplates();
  }, [selectedType]);

  const loadTemplates = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const filters = {
        page: 1,
        pageSize: 100,
        isActive: true,
        templateType: selectedType !== 'all' ? selectedType : undefined,
      };

      const response = await contractTemplateService.getTemplates(filters);
      setTemplates(response.items);
    } catch (err) {
      setError('Erro ao carregar templates. Tente novamente.');
      console.error('Erro ao carregar templates:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const filteredTemplates = templates.filter((template) => {
    const matchesSearch =
      template.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      template.description?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      false;
    return matchesSearch;
  });

  const handleSelectTemplate = (templateId: string) => {
    setSelectedTemplate(templateId);
    onSelect(templateId);
  };

  const handleSkipTemplate = () => {
    setSelectedTemplate(null);
    onSelect(null);
  };

  return (
    <div className="space-y-6">
      {/* Description */}
      <div className="text-center mb-8">
        <FileText className="w-16 h-16 text-cyan-600 mx-auto mb-4" />
        <h3 className="text-lg font-medium text-gray-900 mb-2">
          Escolha um template ou crie do zero
        </h3>
        <p className="text-sm text-gray-500">
          Selecione um template pré-configurado para agilizar a criação do contrato
        </p>
      </div>

      {/* Filters */}
      <div className="flex gap-4 mb-6">
        <div className="flex-1 relative">
          <Search className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
          <input
            type="text"
            placeholder="Buscar templates..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
          />
        </div>
        <select
          value={selectedType}
          onChange={(e) => setSelectedType(e.target.value as ContractTemplateType | 'all')}
          className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
        >
          <option value="all">Todos os Tipos</option>
          {Object.entries(CONTRACT_TEMPLATE_TYPE_CONFIG).map(([key, config]) => (
            <option key={key} value={key}>
              {config.label}
            </option>
          ))}
        </select>
      </div>

      {/* Loading State */}
      {isLoading && (
        <div className="text-center py-12">
          <Loader2 className="w-12 h-12 text-cyan-600 mx-auto mb-4 animate-spin" />
          <p className="text-gray-500">Carregando templates...</p>
        </div>
      )}

      {/* Error State */}
      {!isLoading && error && (
        <div className="text-center py-12">
          <div className="text-red-600 mb-4">⚠️</div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">Erro ao carregar</h3>
          <p className="text-gray-500 mb-4">{error}</p>
          <Button variant="primary" onClick={loadTemplates}>
            Tentar Novamente
          </Button>
        </div>
      )}

      {/* Templates Grid */}
      {!isLoading && !error && (
        <>
          {/* Option: Create from Scratch */}
          <Card
            className={`p-6 cursor-pointer border-2 transition-all ${
              selectedTemplate === null
                ? 'border-cyan-600 bg-cyan-50'
                : 'border-gray-200 hover:border-cyan-400'
            }`}
            onClick={handleSkipTemplate}
          >
            <div className="flex items-start justify-between">
              <div className="flex items-start space-x-4">
                <div className="p-3 bg-gray-100 rounded-lg">
                  <FileText className="w-6 h-6 text-gray-600" />
                </div>
                <div>
                  <h4 className="text-lg font-semibold text-gray-900">
                    Criar do Zero
                  </h4>
                  <p className="text-sm text-gray-600 mt-1">
                    Crie um contrato personalizado sem usar um template
                  </p>
                </div>
              </div>
              {selectedTemplate === null && (
                <CheckCircle className="w-6 h-6 text-cyan-600" />
              )}
            </div>
          </Card>

          {/* Template Cards */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {filteredTemplates.map((template) => {
              const isSelected = selectedTemplate === template.id;
              const typeConfig = CONTRACT_TEMPLATE_TYPE_CONFIG[template.templateType];

              return (
                <Card
                  key={template.id}
                  className={`p-6 cursor-pointer border-2 transition-all ${
                    isSelected
                      ? 'border-cyan-600 bg-cyan-50'
                      : 'border-gray-200 hover:border-cyan-400'
                  }`}
                  onClick={() => handleSelectTemplate(template.id)}
                >
                  <div className="flex items-start justify-between mb-3">
                    <div className="flex items-start space-x-3">
                      <div className={`p-2 ${typeConfig.bgColor} rounded-lg`}>
                        <FileText className={`w-5 h-5 ${typeConfig.textColor}`} />
                      </div>
                      <div>
                        <h4 className="font-semibold text-gray-900">{template.name}</h4>
                        <p className="text-xs text-gray-500 mt-0.5">
                          {typeConfig.label}
                        </p>
                      </div>
                    </div>
                    {isSelected && <CheckCircle className="w-5 h-5 text-cyan-600" />}
                  </div>
                  {template.description && (
                    <p className="text-sm text-gray-600 mb-3 line-clamp-2">
                      {template.description}
                    </p>
                  )}
                  <div className="flex items-center justify-between text-xs text-gray-500">
                    <span>Versão {template.version}</span>
                    {template.tags && template.tags.length > 0 && (
                      <div className="flex gap-1">
                        {template.tags.slice(0, 2).map((tag, idx) => (
                          <span
                            key={idx}
                            className="px-2 py-0.5 bg-gray-100 rounded-full"
                          >
                            {tag}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                </Card>
              );
            })}
          </div>

          {/* Empty State */}
          {filteredTemplates.length === 0 && (
            <div className="text-center py-12">
              <FileText className="w-16 h-16 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                Nenhum template encontrado
              </h3>
              <p className="text-gray-500">
                {searchTerm
                  ? 'Tente ajustar sua busca ou crie um contrato do zero'
                  : 'Não há templates disponíveis no momento'}
              </p>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default Step1SelectType;
