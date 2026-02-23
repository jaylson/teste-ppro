// F3-BLD-FE-003: Contract Builder Step 3 - Select Clauses
// File: src/frontend/src/components/contracts/builder/Step3SelectClauses.tsx
// Author: GitHub Copilot
// Date: 14/02/2026

import React, { useState, useEffect } from 'react';
import { FileText, Search, CheckSquare, Square, Loader2, Tag } from 'lucide-react';
import { Card, Button } from '@/components/ui';
import type { Clause, ClauseType } from '@/types/contract.types';
import { clauseService } from '@/services/contractService';
import { CLAUSE_TYPE_CONFIG } from '@/constants/contractConstants';

interface Step3SelectClausesProps {
  selectedClauseIds: string[];
  onUpdate: (clauseIds: string[]) => void;
  onUpdateClauses?: (clauses: Clause[]) => void;
}

export const Step3SelectClauses: React.FC<Step3SelectClausesProps> = ({
  selectedClauseIds,
  onUpdate,
  onUpdateClauses,
}) => {
  const [clauses, setClauses] = useState<Clause[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedType, setSelectedType] = useState<ClauseType | 'all'>('all');
  const [showMandatoryOnly, setShowMandatoryOnly] = useState(false);

  useEffect(() => {
    loadClauses();
  }, [selectedType, showMandatoryOnly]);

  const loadClauses = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const filters = {
        page: 1,
        pageSize: 100,
        isActive: true,
        clauseType: selectedType !== 'all' ? selectedType : undefined,
        isMandatory: showMandatoryOnly ? true : undefined,
      };

      const response = await clauseService.getClauses(filters);
      setClauses(response.items);
    } catch (err) {
      setError('Erro ao carregar cláusulas. Tente novamente.');
      console.error('Erro ao carregar cláusulas:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const filteredClauses = clauses.filter((clause) => {
    const matchesSearch =
      clause.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      clause.description?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      false;
    return matchesSearch;
  });

  // Group clauses by type
  const groupedClauses = filteredClauses.reduce((acc, clause) => {
    if (!acc[clause.clauseType]) {
      acc[clause.clauseType] = [];
    }
    acc[clause.clauseType].push(clause);
    return acc;
  }, {} as Record<ClauseType, Clause[]>);

  const notifyClauseObjects = (ids: string[]) => {
    const selected = clauses.filter((c) => ids.includes(c.id));
    onUpdateClauses?.(selected);
  };

  const handleToggleClause = (clauseId: string) => {
    const isSelected = selectedClauseIds.includes(clauseId);
    const newIds = isSelected
      ? selectedClauseIds.filter((id) => id !== clauseId)
      : [...selectedClauseIds, clauseId];
    onUpdate(newIds);
    notifyClauseObjects(newIds);
  };

  const handleSelectAll = () => {
    const allClauseIds = filteredClauses.map((c) => c.id);
    onUpdate(allClauseIds);
    notifyClauseObjects(allClauseIds);
  };

  const handleDeselectAll = () => {
    onUpdate([]);
    onUpdateClauses?.([]);
  };

  const handleSelectMandatory = () => {
    const mandatoryIds = filteredClauses.filter((c) => c.isMandatory).map((c) => c.id);
    onUpdate(mandatoryIds);
    notifyClauseObjects(mandatoryIds);
  };

  return (
    <div className="space-y-6">
      {/* Description */}
      <div className="text-center mb-8">
        <FileText className="w-16 h-16 text-cyan-600 mx-auto mb-4" />
        <h3 className="text-lg font-medium text-gray-900 mb-2">
          Selecione as cláusulas do contrato
        </h3>
        <p className="text-sm text-gray-500">
          Escolha as cláusulas que farão parte do contrato. Cláusulas obrigatórias são
          marcadas com um badge.
        </p>
      </div>

      {/* Filters and Actions */}
      <div className="flex flex-col md:flex-row gap-4 mb-6">
        <div className="flex-1 relative">
          <Search className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
          <input
            type="text"
            placeholder="Buscar cláusulas..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
          />
        </div>
        <select
          value={selectedType}
          onChange={(e) => setSelectedType(e.target.value as ClauseType | 'all')}
          className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
        >
          <option value="all">Todos os Tipos</option>
          {Object.entries(CLAUSE_TYPE_CONFIG).map(([key, config]) => (
            <option key={key} value={key}>
              {config.label}
            </option>
          ))}
        </select>
        <label className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg cursor-pointer hover:bg-gray-50">
          <input
            type="checkbox"
            checked={showMandatoryOnly}
            onChange={(e) => setShowMandatoryOnly(e.target.checked)}
            className="w-4 h-4 text-cyan-600 focus:ring-2 focus:ring-cyan-600"
          />
          <span className="text-sm text-gray-700">Apenas Obrigatórias</span>
        </label>
      </div>

      {/* Bulk Actions */}
      <div className="flex items-center justify-between bg-gray-50 px-4 py-3 rounded-lg">
        <span className="text-sm text-gray-700">
          {selectedClauseIds.length} cláusula{selectedClauseIds.length !== 1 ? 's' : ''}{' '}
          selecionada{selectedClauseIds.length !== 1 ? 's' : ''}
        </span>
        <div className="flex gap-2">
          <Button variant="secondary" size="sm" onClick={handleSelectMandatory}>
            Selecionar Obrigatórias
          </Button>
          <Button variant="secondary" size="sm" onClick={handleSelectAll}>
            Selecionar Todas
          </Button>
          <Button variant="secondary" size="sm" onClick={handleDeselectAll}>
            Limpar Seleção
          </Button>
        </div>
      </div>

      {/* Loading State */}
      {isLoading && (
        <div className="text-center py-12">
          <Loader2 className="w-12 h-12 text-cyan-600 mx-auto mb-4 animate-spin" />
          <p className="text-gray-500">Carregando cláusulas...</p>
        </div>
      )}

      {/* Error State */}
      {!isLoading && error && (
        <div className="text-center py-12">
          <div className="text-red-600 mb-4">⚠️</div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">Erro ao carregar</h3>
          <p className="text-gray-500 mb-4">{error}</p>
          <Button variant="primary" onClick={loadClauses}>
            Tentar Novamente
          </Button>
        </div>
      )}

      {/* Clauses List (Grouped by Type) */}
      {!isLoading && !error && (
        <div className="space-y-6">
          {Object.entries(groupedClauses).map(([type, typeClauses]) => {
            const typeConfig = CLAUSE_TYPE_CONFIG[type as ClauseType];
            return (
              <div key={type}>
                <h4 className="text-md font-semibold text-gray-900 mb-3 flex items-center gap-2">
                  <Tag className="w-5 h-5 text-gray-500" />
                  {typeConfig.label} ({typeClauses.length})
                </h4>
                <div className="space-y-2">
                  {typeClauses.map((clause) => {
                    const isSelected = selectedClauseIds.includes(clause.id);
                    return (
                      <Card
                        key={clause.id}
                        className={`p-4 cursor-pointer border transition-all ${
                          isSelected
                            ? 'border-cyan-600 bg-cyan-50'
                            : 'border-gray-200 hover:border-cyan-400'
                        }`}
                        onClick={() => handleToggleClause(clause.id)}
                      >
                        <div className="flex items-start gap-3">
                          <div className="mt-0.5">
                            {isSelected ? (
                              <CheckSquare className="w-5 h-5 text-cyan-600" />
                            ) : (
                              <Square className="w-5 h-5 text-gray-400" />
                            )}
                          </div>
                          <div className="flex-1">
                            <div className="flex items-center gap-2 mb-1">
                              <h5 className="font-medium text-gray-900">{clause.name}</h5>
                              {clause.isMandatory && (
                                <span className="px-2 py-0.5 bg-red-100 text-red-700 text-xs rounded-full">
                                  Obrigatória
                                </span>
                              )}
                            </div>
                            {clause.description && (
                              <p className="text-sm text-gray-600 mb-2">{clause.description}</p>
                            )}
                            <div className="flex items-center gap-3 text-xs text-gray-500">
                              <span>Código: {clause.code}</span>
                              <span>Versão {clause.version}</span>
                              {clause.tags && clause.tags.length > 0 && (
                                <div className="flex gap-1">
                                  {clause.tags.slice(0, 2).map((tag, idx) => (
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
                          </div>
                        </div>
                      </Card>
                    );
                  })}
                </div>
              </div>
            );
          })}

          {/* Empty State */}
          {filteredClauses.length === 0 && (
            <div className="text-center py-12 border-2 border-dashed border-gray-300 rounded-lg">
              <FileText className="w-16 h-16 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                Nenhuma cláusula encontrada
              </h3>
              <p className="text-gray-500">
                {searchTerm
                  ? 'Tente ajustar sua busca ou filtros'
                  : 'Não há cláusulas disponíveis no momento'}
              </p>
            </div>
          )}
        </div>
      )}

      {/* Info Box */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <p className="text-sm text-blue-800">
          <strong>💡 Dica:</strong> Cláusulas obrigatórias devem sempre ser incluídas no
          contrato. Você pode personalizar o conteúdo das cláusulas no próximo passo.
        </p>
      </div>
    </div>
  );
};

export default Step3SelectClauses;
