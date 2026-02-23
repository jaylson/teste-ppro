// F3-BLD-FE-001: Contract Builder Step 1 - Select Template or Upload DOCX
// File: src/frontend/src/components/contracts/builder/Step1SelectType.tsx
// Author: GitHub Copilot
// Date: 14/02/2026

import React, { useState, useEffect, useRef } from 'react';
import {
  FileText, Search, CheckCircle, Loader2, Upload, AlertCircle, X
} from 'lucide-react';
import { Card, Button } from '@/components/ui';
import { ContractTemplateType } from '@/types/contract.types';
import type { ContractTemplate } from '@/types/contract.types';
import { contractTemplateService, contractVersionService } from '@/services/contractService';
import { CONTRACT_TEMPLATE_TYPE_CONFIG } from '@/constants/contractConstants';

type StepMode = 'builder' | 'upload';

const CONTRACT_TYPE_OPTIONS: { value: ContractTemplateType; label: string }[] = [
  { value: ContractTemplateType.Investment, label: 'Investimento' },
  { value: ContractTemplateType.ShareholdersAgreement, label: 'Acordo de Acionistas' },
  { value: ContractTemplateType.Employment, label: 'Trabalho / Emprego' },
  { value: ContractTemplateType.ServiceAgreement, label: 'Prestação de Serviços' },
  { value: ContractTemplateType.Partnership, label: 'Parceria' },
  { value: ContractTemplateType.NDA, label: 'NDA / Confidencialidade' },
  { value: ContractTemplateType.Confidentiality, label: 'Confidencialidade' },
  { value: ContractTemplateType.StockOption, label: 'Stock Option' },
  { value: ContractTemplateType.Other, label: 'Outro' },
];

interface Step1SelectTypeProps {
  onSelect: (templateId: string | null) => void;
  onSelectTemplate?: (template: ContractTemplate | null) => void;
  selectedTemplateId?: string | null;
  companyId?: string | null;
  onUploadSuccess?: (contractId: string) => void;
}

export const Step1SelectType: React.FC<Step1SelectTypeProps> = ({
  onSelect,
  onSelectTemplate,
  selectedTemplateId,
  companyId,
  onUploadSuccess,
}) => {
  // ---- Mode toggle ----
  const [mode, setMode] = useState<StepMode>('builder');

  // ---- Builder state ----
  const [templates, setTemplates] = useState<ContractTemplate[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedType, setSelectedType] = useState<ContractTemplateType | 'all'>('all');
  const [selectedTemplate, setSelectedTemplate] = useState<string | null>(
    selectedTemplateId || null
  );

  // ---- Upload state ----
  const [uploadTitle, setUploadTitle] = useState('');
  const [uploadContractType, setUploadContractType] = useState<ContractTemplateType>(ContractTemplateType.Other);
  const [uploadDescription, setUploadDescription] = useState('');
  const [uploadFile, setUploadFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [isDragOver, setIsDragOver] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

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
        type: selectedType !== 'all' ? selectedType : undefined,
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
    const template = templates.find((t) => t.id === templateId) ?? null;
    onSelectTemplate?.(template);
  };

  const handleSkipTemplate = () => {
    setSelectedTemplate(null);
    onSelect(null);
    onSelectTemplate?.(null);
  };

  // ---- Upload handlers ----
  const validateDocxFile = (file: File): string | null => {
    const validTypes = [
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    ];
    const validExtension = file.name.toLowerCase().endsWith('.docx');
    if (!validTypes.includes(file.type) && !validExtension) {
      return 'Somente arquivos .docx são aceitos.';
    }
    if (file.size > 10 * 1024 * 1024) {
      return 'Arquivo muito grande. Tamanho máximo: 10 MB.';
    }
    return null;
  };

  const handleFileSelect = (file: File) => {
    const err = validateDocxFile(file);
    if (err) {
      setUploadError(err);
      setUploadFile(null);
      return;
    }
    setUploadFile(file);
    setUploadError(null);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
    const file = e.dataTransfer.files[0];
    if (file) handleFileSelect(file);
  };

  const handleUploadSubmit = async () => {
    if (!uploadTitle.trim() || !uploadFile || !companyId) return;
    setIsUploading(true);
    setUploadError(null);
    try {
      const requestData = {
        companyId,
        title: uploadTitle.trim(),
        contractType: uploadContractType,
        description: uploadDescription.trim() || undefined,
      };
      const contract = await contractVersionService.createFromUpload(requestData, uploadFile);
      onUploadSuccess?.(contract.id);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setUploadError(msg ?? 'Erro ao fazer upload. Tente novamente.');
    } finally {
      setIsUploading(false);
    }
  };

  const isUploadValid =
    uploadTitle.trim().length >= 3 && uploadFile !== null && !!companyId;

  return (
    <div className="space-y-6">
      {/* ---- Mode Toggle ---- */}
      <div className="flex rounded-lg border border-gray-200 overflow-hidden mb-2">
        <button
          type="button"
          onClick={() => setMode('builder')}
          className={`flex-1 py-3 px-4 text-sm font-medium transition-colors flex items-center justify-center gap-2 ${
            mode === 'builder'
              ? 'bg-cyan-600 text-white'
              : 'bg-white text-gray-600 hover:bg-gray-50'
          }`}
        >
          <FileText className="w-4 h-4" />
          Usar Builder
        </button>
        <button
          type="button"
          onClick={() => setMode('upload')}
          className={`flex-1 py-3 px-4 text-sm font-medium transition-colors flex items-center justify-center gap-2 ${
            mode === 'upload'
              ? 'bg-cyan-600 text-white'
              : 'bg-white text-gray-600 hover:bg-gray-50'
          }`}
        >
          <Upload className="w-4 h-4" />
          Upload de DOCX
        </button>
      </div>

      {/* ============================= */}
      {/* MODE: UPLOAD */}
      {/* ============================= */}
      {mode === 'upload' && (
        <div className="space-y-5">
          <div className="text-center mb-6">
            <Upload className="w-14 h-14 text-cyan-600 mx-auto mb-3" />
            <h3 className="text-lg font-medium text-gray-900 mb-1">
              Upload de contrato DOCX
            </h3>
            <p className="text-sm text-gray-500">
              Faça upload de um arquivo Word (.docx) para criar o contrato direto.
            </p>
          </div>

          {/* Title */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Título do contrato <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              value={uploadTitle}
              onChange={(e) => setUploadTitle(e.target.value)}
              placeholder="Ex: Contrato de investimento — Série A"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600 text-sm"
            />
            {uploadTitle.trim().length > 0 && uploadTitle.trim().length < 3 && (
              <p className="text-xs text-red-500 mt-1">Mínimo de 3 caracteres.</p>
            )}
          </div>

          {/* Contract Type */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tipo de contrato <span className="text-red-500">*</span>
            </label>
            <select
              value={uploadContractType}
              onChange={(e) => setUploadContractType(e.target.value as ContractTemplateType)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600 text-sm"
            >
              {CONTRACT_TYPE_OPTIONS.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Descrição (opcional)
            </label>
            <textarea
              value={uploadDescription}
              onChange={(e) => setUploadDescription(e.target.value)}
              rows={3}
              placeholder="Breve descrição do contrato..."
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600 text-sm resize-none"
            />
          </div>

          {/* Dropzone */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Arquivo DOCX <span className="text-red-500">*</span>
            </label>
            <div
              onDragOver={(e) => { e.preventDefault(); setIsDragOver(true); }}
              onDragLeave={() => setIsDragOver(false)}
              onDrop={handleDrop}
              onClick={() => fileInputRef.current?.click()}
              className={`relative border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors ${
                isDragOver
                  ? 'border-cyan-500 bg-cyan-50'
                  : uploadFile
                  ? 'border-green-400 bg-green-50'
                  : 'border-gray-300 hover:border-cyan-400 hover:bg-gray-50'
              }`}
            >
              <input
                ref={fileInputRef}
                type="file"
                accept=".docx,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                className="hidden"
                onChange={(e) => {
                  const file = e.target.files?.[0];
                  if (file) handleFileSelect(file);
                }}
              />
              {uploadFile ? (
                <div className="flex items-center justify-center gap-3">
                  <CheckCircle className="w-8 h-8 text-green-600" />
                  <div className="text-left">
                    <p className="text-sm font-medium text-gray-900">{uploadFile.name}</p>
                    <p className="text-xs text-gray-500">
                      {(uploadFile.size / 1024).toFixed(1)} KB
                    </p>
                  </div>
                  <button
                    type="button"
                    onClick={(e) => { e.stopPropagation(); setUploadFile(null); }}
                    className="ml-auto text-gray-400 hover:text-red-500"
                  >
                    <X className="w-5 h-5" />
                  </button>
                </div>
              ) : (
                <>
                  <Upload className="w-10 h-10 text-gray-400 mx-auto mb-3" />
                  <p className="text-sm font-medium text-gray-700 mb-1">
                    Arraste o arquivo ou clique para selecionar
                  </p>
                  <p className="text-xs text-gray-500">Somente .docx • Máx. 10 MB</p>
                </>
              )}
            </div>
          </div>

          {/* Upload Error */}
          {uploadError && (
            <div className="flex items-start gap-2 p-3 bg-red-50 border border-red-200 rounded-lg">
              <AlertCircle className="w-4 h-4 text-red-600 mt-0.5 shrink-0" />
              <p className="text-sm text-red-700">{uploadError}</p>
            </div>
          )}

          {/* Submit */}
          <Button
            variant="primary"
            onClick={handleUploadSubmit}
            disabled={!isUploadValid || isUploading}
            className="w-full"
          >
            {isUploading ? (
              <span className="flex items-center justify-center gap-2">
                <Loader2 className="w-4 h-4 animate-spin" />
                Enviando...
              </span>
            ) : (
              'Criar contrato com este arquivo'
            )}
          </Button>
        </div>
      )}

      {/* ============================= */}
      {/* MODE: BUILDER */}
      {/* ============================= */}
      {mode === 'builder' && (
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
            <p className="text-xs text-gray-500 mt-2">
              Voce pode continuar sem template.
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
                      <span className="inline-flex items-center text-xs text-cyan-700 bg-cyan-100 px-2 py-0.5 rounded-full mt-2">
                        Nao exige template
                      </span>
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
                  const typeConfig = CONTRACT_TEMPLATE_TYPE_CONFIG[template.templateType] ?? {
                    label: 'Outro',
                    description: 'Tipo não reconhecido',
                    icon: 'FileText',
                    bgColor: 'bg-slate-100',
                    textColor: 'text-slate-800',
                  };

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
      )}
    </div>
  );
};

export default Step1SelectType;
