import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, Search, FlaskConical, Power, PowerOff, Trash2 } from 'lucide-react';
import { Button, Card, Spinner, Badge, StatCard } from '@/components/ui';
import {
  useCustomFormulas,
  useActivateFormula,
  useDeactivateFormula,
  useDeleteCustomFormula,
} from '@/hooks';
import { useClientStore } from '@/stores/clientStore';
import {
  formulaValidationStatusLabels,
  formulaValidationStatusColors,
  type CustomFormulaFilters,
} from '@/types';
import { formatDate } from '@/utils/format';

export default function CustomFormulasPage() {
  const navigate = useNavigate();
  const { selectedCompanyId } = useClientStore();
  const [search, setSearch] = useState('');
  const [activeFilter, setActiveFilter] = useState<'' | 'true' | 'false'>('');
  const [sectorFilter, setSectorFilter] = useState('');
  const [page, setPage] = useState(1);
  const [deleting, setDeleting] = useState<string | null>(null);

  const filters: CustomFormulaFilters = {
    companyId: selectedCompanyId || undefined,
    search: search || undefined,
    isActive: activeFilter === '' ? undefined : activeFilter === 'true',
    sectorTag: sectorFilter || undefined,
    page,
    pageSize: 15,
  };

  const { data, isLoading } = useCustomFormulas(filters);
  const activate = useActivateFormula();
  const deactivate = useDeactivateFormula();
  const deleteFormula = useDeleteCustomFormula();

  const totalCount = data?.total ?? 0;
  const activeCount = data?.items.filter((f) => f.isActive).length ?? 0;
  const totalPages = data ? Math.ceil(data.total / 15) : 1;

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Fórmulas Customizadas</h1>
          <p className="text-sm text-gray-500 mt-1">Motor de cálculo com variáveis configuráveis por setor</p>
        </div>
        <Button icon={<Plus size={16} />} onClick={() => navigate('/valuations/custom-formulas/new')}>
          Nova Fórmula
        </Button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <StatCard icon={<FlaskConical size={20} />} iconColor="bg-purple-100 text-purple-600" label="Total" value={String(totalCount)} />
        <StatCard icon={<Power size={16} />} iconColor="bg-green-100 text-green-600" label="Ativas" value={String(activeCount)} />
        <StatCard icon={<PowerOff size={16} />} iconColor="bg-gray-100 text-gray-500" label="Inativas" value={String(totalCount - activeCount)} />
        <StatCard icon={<FlaskConical size={16} />} iconColor="bg-blue-100 text-blue-600" label="Validadas" value={String(data?.items.filter((f) => f.currentVersion?.validationStatus === 'validated').length ?? 0)} />
      </div>

      {/* Filters */}
      <Card>
        <div className="p-4 flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              className="input-base w-full pl-8"
              placeholder="Buscar por nome..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            />
          </div>
          <input
            className="input-base sm:w-44"
            placeholder="Setor (tag)..."
            value={sectorFilter}
            onChange={(e) => { setSectorFilter(e.target.value); setPage(1); }}
          />
          <select
            className="input-base sm:w-36"
            value={activeFilter}
            onChange={(e) => { setActiveFilter(e.target.value as '' | 'true' | 'false'); setPage(1); }}
          >
            <option value="">Todas</option>
            <option value="true">Ativas</option>
            <option value="false">Inativas</option>
          </select>
        </div>
      </Card>

      {/* Table */}
      <Card>
        {isLoading ? (
          <div className="flex justify-center py-12">
            <Spinner />
          </div>
        ) : !data?.items.length ? (
          <div className="text-center py-12 text-gray-400">
            <FlaskConical size={40} className="mx-auto mb-3 opacity-30" />
            <p className="text-sm">Nenhuma fórmula encontrada.</p>
            <Button
              variant="secondary"
              size="sm"
              className="mt-4"
              onClick={() => navigate('/valuations/custom-formulas/new')}
            >
              Criar primeira fórmula
            </Button>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100 text-xs text-gray-400 uppercase tracking-wide">
                  <th className="px-4 py-3 text-left">Nome</th>
                  <th className="px-4 py-3 text-left">Setor</th>
                  <th className="px-4 py-3 text-left">Validação</th>
                  <th className="px-4 py-3 text-left">Versão Atual</th>
                  <th className="px-4 py-3 text-left">Status</th>
                  <th className="px-4 py-3 text-left">Atualizado</th>
                  <th className="px-4 py-3 text-right">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {data.items.map((formula) => (
                  <tr key={formula.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3">
                      <span className="font-medium text-gray-900 hover:text-blue-600 cursor-pointer">
                        {formula.name}
                      </span>
                      {formula.description && (
                        <p className="text-xs text-gray-400 mt-0.5 truncate max-w-xs">{formula.description}</p>
                      )}
                    </td>
                    <td className="px-4 py-3 text-gray-500">{formula.sectorTag || '—'}</td>
                    <td className="px-4 py-3">
                      <Badge className={formulaValidationStatusColors[formula.currentVersion?.validationStatus ?? ''] ?? ''}>
                        {formulaValidationStatusLabels[formula.currentVersion?.validationStatus ?? ''] ?? (formula.currentVersion?.validationStatus ?? '—')}
                      </Badge>
                    </td>
                    <td className="px-4 py-3 text-gray-500">
                      {formula.currentVersionId ? `v${formula.currentVersionId.slice(-4)}` : '—'}
                    </td>
                    <td className="px-4 py-3">
                      {formula.isActive ? (
                        <Badge className="bg-green-100 text-green-700">Ativa</Badge>
                      ) : (
                        <Badge className="bg-gray-100 text-gray-500">Inativa</Badge>
                      )}
                    </td>
                    <td className="px-4 py-3 text-gray-500">{formatDate(formula.updatedAt)}</td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-2">
                        {formula.isActive ? (
                          <button
                            onClick={() => deactivate.mutate(formula.id)}
                            className="p-1.5 rounded hover:bg-yellow-50 text-yellow-600 transition-colors"
                            title="Desativar"
                          >
                            <PowerOff size={14} />
                          </button>
                        ) : (
                          <button
                            onClick={() => activate.mutate(formula.id)}
                            className="p-1.5 rounded hover:bg-green-50 text-green-600 transition-colors"
                            title="Ativar"
                          >
                            <Power size={14} />
                          </button>
                        )}
                        <button
                          onClick={() => {
                            if (deleting === formula.id) {
                              deleteFormula.mutate(formula.id);
                              setDeleting(null);
                            } else {
                              setDeleting(formula.id);
                            }
                          }}
                          className={`p-1.5 rounded transition-colors ${
                            deleting === formula.id
                              ? 'bg-red-100 text-red-600'
                              : 'hover:bg-red-50 text-gray-400'
                          }`}
                          title={deleting === formula.id ? 'Confirmar exclusão' : 'Excluir'}
                        >
                          <Trash2 size={14} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="px-4 py-3 border-t border-gray-100 flex items-center justify-between">
            <span className="text-xs text-gray-400">{totalCount} fórmulas</span>
            <div className="flex gap-1">
              <button
                disabled={page === 1}
                onClick={() => setPage((p) => p - 1)}
                className="px-3 py-1.5 text-xs rounded border disabled:opacity-40 hover:bg-gray-50"
              >
                Anterior
              </button>
              <span className="px-3 py-1.5 text-xs text-gray-500">{page}/{totalPages}</span>
              <button
                disabled={page === totalPages}
                onClick={() => setPage((p) => p + 1)}
                className="px-3 py-1.5 text-xs rounded border disabled:opacity-40 hover:bg-gray-50"
              >
                Próxima
              </button>
            </div>
          </div>
        )}
      </Card>
    </div>
  );
}
