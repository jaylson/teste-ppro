import React, { useState, useEffect } from 'react';
import {
  Search,
  Plus,
  Edit2,
  Trash2,
  Filter,
  CheckCircle,
  XCircle,
  Eye,
  AlertCircle,
} from 'lucide-react';
import { clauseService } from '@/services/contractService';
import {
  Clause,
  ClauseType,
  CreateClauseRequest,
  UpdateClauseRequest,
} from '@/types/contract.types';
import { CLAUSE_TYPE_CONFIG } from '@/constants/contractConstants';
import VariablesList from '@/components/contracts/VariablesList';

const ClausesPage: React.FC = () => {
  // Estado
  const [clauses, setClauses] = useState<Clause[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedType, setSelectedType] = useState<ClauseType | 'all'>('all');
  const [showMandatory, setShowMandatory] = useState<boolean | 'all'>('all');
  const [showActive, setShowActive] = useState<boolean | 'all'>('all');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isPreviewOpen, setIsPreviewOpen] = useState(false);
  const [editingClause, setEditingClause] = useState<Clause | null>(null);
  const [previewClause, setPreviewClause] = useState<Clause | null>(null);
  const [formData, setFormData] = useState<CreateClauseRequest>({
    name: '',
    code: '',
    content: '',
    clauseType: ClauseType.General,
    isMandatory: false,
    description: '',
    tags: [],
    displayOrder: 0,
  });

  // Carregar cláusulas
  const loadClauses = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await clauseService.getClauses({
        page: 1,
        pageSize: 100,
      });
      setClauses(response.items);
    } catch (err: any) {
      setError(err.message || 'Erro ao carregar cláusulas');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadClauses();
  }, []);

  // Filtrar cláusulas
  const filteredClauses = clauses.filter((clause) => {
    const matchesSearch =
      searchTerm === '' ||
      clause.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      clause.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
      clause.description?.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesType = selectedType === 'all' || clause.clauseType === selectedType;

    const matchesMandatory =
      showMandatory === 'all' || clause.isMandatory === showMandatory;

    const matchesActive = showActive === 'all' || clause.isActive === showActive;

    return matchesSearch && matchesType && matchesMandatory && matchesActive;
  });

  // Abrir modal de criação
  const openCreateModal = () => {
    setEditingClause(null);
    setFormData({
      name: '',
      code: '',
      content: '',
      clauseType: ClauseType.General,
      isMandatory: false,
      description: '',
      tags: [],
      displayOrder: 0,
    });
    setIsModalOpen(true);
  };

  // Abrir modal de edição
  const openEditModal = (clause: Clause) => {
    setEditingClause(clause);
    setFormData({
      name: clause.name,
      code: clause.code,
      content: clause.content,
      clauseType: clause.clauseType,
      isMandatory: clause.isMandatory,
      description: clause.description || '',
      tags: clause.tags,
      displayOrder: clause.displayOrder,
    });
    setIsModalOpen(true);
  };

  // Abrir preview
  const openPreview = (clause: Clause) => {
    setPreviewClause(clause);
    setIsPreviewOpen(true);
  };

  // Salvar cláusula
  const handleSave = async () => {
    try {
      setError(null);
      if (editingClause) {
        const updateData: UpdateClauseRequest = {
          name: formData.name,
          description: formData.description,
          content: formData.content,
          tags: formData.tags,
          isMandatory: formData.isMandatory,
        };
        await clauseService.updateClause(editingClause.id, updateData);
      } else {
        await clauseService.createClause(formData);
      }
      setIsModalOpen(false);
      loadClauses();
    } catch (err: any) {
      setError(err.message || 'Erro ao salvar cláusula');
    }
  };

  // Deletar cláusula
  const handleDelete = async (id: string) => {
    if (!window.confirm('Tem certeza que deseja deletar esta cláusula?')) {
      return;
    }
    try {
      setError(null);
      await clauseService.deleteClause(id);
      loadClauses();
    } catch (err: any) {
      setError(err.message || 'Erro ao deletar cláusula');
    }
  };

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          Biblioteca de Cláusulas
        </h1>
        <p className="text-gray-600">
          Gerencie as cláusulas padrão para seus contratos
        </p>
      </div>

      {/* Mensagem de erro */}
      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-2">
          <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
          <p className="text-red-800">{error}</p>
        </div>
      )}

      {/* Filtros e busca */}
      <div className="mb-6 bg-white p-4 rounded-lg shadow space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* Busca */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Buscar cláusulas..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          {/* Tipo */}
          <select
            value={selectedType}
            onChange={(e) => setSelectedType(e.target.value as ClauseType | 'all')}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="all">Todos os tipos</option>
            {Object.entries(CLAUSE_TYPE_CONFIG).map(([key, config]) => (
              <option key={key} value={key}>
                {config.label}
              </option>
            ))}
          </select>

          {/* Obrigatória */}
          <select
            value={String(showMandatory)}
            onChange={(e) =>
              setShowMandatory(
                e.target.value === 'all' ? 'all' : e.target.value === 'true'
              )
            }
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="all">Todas (obrigatórias/opcionais)</option>
            <option value="true">Apenas obrigatórias</option>
            <option value="false">Apenas opcionais</option>
          </select>

          {/* Ativa */}
          <select
            value={String(showActive)}
            onChange={(e) =>
              setShowActive(e.target.value === 'all' ? 'all' : e.target.value === 'true')
            }
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="all">Todas (ativas/inativas)</option>
            <option value="true">Apenas ativas</option>
            <option value="false">Apenas inativas</option>
          </select>
        </div>

        {/* Botão de criar */}
        <div className="flex justify-end">
          <button
            onClick={openCreateModal}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
          >
            <Plus className="w-5 h-5" />
            Nova Cláusula
          </button>
        </div>
      </div>

      {/* Lista de cláusulas */}
      {loading ? (
        <div className="text-center py-12">
          <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          <p className="mt-4 text-gray-600">Carregando cláusulas...</p>
        </div>
      ) : filteredClauses.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-lg shadow">
          <Filter className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-600 mb-2">Nenhuma cláusula encontrada</p>
          <p className="text-sm text-gray-500">
            Tente ajustar os filtros ou criar uma nova cláusula
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4">
          {filteredClauses.map((clause) => (
            <div
              key={clause.id}
              className="bg-white p-6 rounded-lg shadow hover:shadow-md transition"
            >
              <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <h3 className="text-lg font-semibold text-gray-900">
                      {clause.name}
                    </h3>
                    <span
                      className={`px-2 py-1 text-xs font-medium rounded-full bg-${
                        CLAUSE_TYPE_CONFIG[clause.clauseType].color
                      } bg-opacity-10 text-${CLAUSE_TYPE_CONFIG[clause.clauseType].color}`}
                    >
                      {CLAUSE_TYPE_CONFIG[clause.clauseType].label}
                    </span>
                    {clause.isMandatory && (
                      <span className="px-2 py-1 text-xs font-medium rounded-full bg-red-100 text-red-800">
                        Obrigatória
                      </span>
                    )}
                    {clause.isActive ? (
                      <CheckCircle className="w-5 h-5 text-green-600" />
                    ) : (
                      <XCircle className="w-5 h-5 text-gray-400" />
                    )}
                  </div>
                  <p className="text-sm text-gray-600 mb-1">
                    <strong>Código:</strong> {clause.code}
                  </p>
                  {clause.description && (
                    <p className="text-sm text-gray-600 mb-3">{clause.description}</p>
                  )}
                  <div className="text-sm text-gray-700 mb-3 p-3 bg-gray-50 rounded border border-gray-200 max-h-32 overflow-y-auto">
                    {clause.content.substring(0, 300)}
                    {clause.content.length > 300 && '...'}
                  </div>
                  <VariablesList content={clause.content} className="mt-3" />
                </div>
                <div className="flex gap-2 ml-4">
                  <button
                    onClick={() => openPreview(clause)}
                    className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition"
                    title="Visualizar"
                  >
                    <Eye className="w-5 h-5" />
                  </button>
                  <button
                    onClick={() => openEditModal(clause)}
                    className="p-2 text-gray-600 hover:bg-gray-100 rounded-lg transition"
                    title="Editar"
                  >
                    <Edit2 className="w-5 h-5" />
                  </button>
                  <button
                    onClick={() => handleDelete(clause.id)}
                    className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition"
                    title="Deletar"
                  >
                    <Trash2 className="w-5 h-5" />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal de criação/edição */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-3xl w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <h2 className="text-2xl font-bold text-gray-900 mb-6">
                {editingClause ? 'Editar Cláusula' : 'Nova Cláusula'}
              </h2>

              <div className="space-y-4">
                {/* Nome */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Nome *
                  </label>
                  <input
                    type="text"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    required
                  />
                </div>

                {/* Código */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Código *
                  </label>
                  <input
                    type="text"
                    value={formData.code}
                    onChange={(e) => setFormData({ ...formData, code: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    required
                    disabled={!!editingClause}
                  />
                </div>

                {/* Tipo */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Tipo *
                  </label>
                  <select
                    value={formData.clauseType}
                    onChange={(e) =>
                      setFormData({ ...formData, clauseType: e.target.value as ClauseType })
                    }
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    disabled={!!editingClause}
                  >
                    {Object.entries(CLAUSE_TYPE_CONFIG).map(([key, config]) => (
                      <option key={key} value={key}>
                        {config.label}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Descrição */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Descrição
                  </label>
                  <textarea
                    value={formData.description}
                    onChange={(e) =>
                      setFormData({ ...formData, description: e.target.value })
                    }
                    rows={2}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>

                {/* Conteúdo */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Conteúdo * (use {`{{variavel}}`} para inserir variáveis)
                  </label>
                  <textarea
                    value={formData.content}
                    onChange={(e) =>
                      setFormData({ ...formData, content: e.target.value })
                    }
                    rows={10}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 font-mono text-sm"
                    required
                  />
                  {formData.content && (
                    <div className="mt-2">
                      <VariablesList content={formData.content} title="Variáveis nesta cláusula" />
                    </div>
                  )}
                </div>

                {/* Obrigatória */}
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="isMandatory"
                    checked={formData.isMandatory}
                    onChange={(e) =>
                      setFormData({ ...formData, isMandatory: e.target.checked })
                    }
                    className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  />
                  <label htmlFor="isMandatory" className="ml-2 text-sm text-gray-700">
                    Cláusula obrigatória (será incluída automaticamente em todos os
                    contratos)
                  </label>
                </div>
              </div>

              {/* Ações */}
              <div className="flex justify-end gap-3 mt-6 pt-6 border-t">
                <button
                  onClick={() => setIsModalOpen(false)}
                  className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition"
                >
                  Cancelar
                </button>
                <button
                  onClick={handleSave}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
                >
                  {editingClause ? 'Salvar Alterações' : 'Criar Cláusula'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Modal de preview */}
      {isPreviewOpen && previewClause && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-3xl w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <div className="flex items-start justify-between mb-6">
                <div>
                  <h2 className="text-2xl font-bold text-gray-900 mb-2">
                    {previewClause.name}
                  </h2>
                  <div className="flex items-center gap-2">
                    <span
                      className={`px-2 py-1 text-xs font-medium rounded-full bg-${
                        CLAUSE_TYPE_CONFIG[previewClause.clauseType].color
                      } bg-opacity-10 text-${
                        CLAUSE_TYPE_CONFIG[previewClause.clauseType].color
                      }`}
                    >
                      {CLAUSE_TYPE_CONFIG[previewClause.clauseType].label}
                    </span>
                    {previewClause.isMandatory && (
                      <span className="px-2 py-1 text-xs font-medium rounded-full bg-red-100 text-red-800">
                        Obrigatória
                      </span>
                    )}
                  </div>
                </div>
              </div>

              {previewClause.description && (
                <p className="text-gray-600 mb-4">{previewClause.description}</p>
              )}

              <div className="bg-gray-50 p-4 rounded-lg border border-gray-200 mb-4">
                <p className="text-sm text-gray-700 whitespace-pre-wrap">
                  {previewClause.content}
                </p>
              </div>

              <VariablesList content={previewClause.content} className="mb-6" />

              <div className="flex justify-end gap-3 pt-6 border-t">
                <button
                  onClick={() => setIsPreviewOpen(false)}
                  className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition"
                >
                  Fechar
                </button>
                <button
                  onClick={() => {
                    setIsPreviewOpen(false);
                    openEditModal(previewClause);
                  }}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
                >
                  Editar
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ClausesPage;
