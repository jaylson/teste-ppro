import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus,
  TrendingUp,
  Search,
  ChevronLeft,
  ChevronRight,
  CheckCircle,
  Clock,
  XCircle,
  FileText,
} from 'lucide-react';
import { Button, Card, Spinner, StatCard, Badge, ConfirmDialog } from '@/components/ui';
import { useValuations, useDeleteValuation } from '@/hooks';
import { useClientStore } from '@/stores/clientStore';
import { useConfirm } from '@/components/ui';
import {
  valuationStatusLabels,
  valuationStatusColors,
  valuationEventTypeLabels,
  type Valuation,
  type ValuationFilters,
} from '@/types';
import { formatCurrency, formatDate } from '@/utils/format';

export default function ValuationsPage() {
  const navigate = useNavigate();
  const { selectedCompanyId } = useClientStore();
  const { confirm, confirmState, handleCancel } = useConfirm();

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const pageSize = 12;

  const filters: ValuationFilters = useMemo(
    () => ({
      companyId: selectedCompanyId || undefined,
      page,
      pageSize,
      status: statusFilter || undefined,
    }),
    [selectedCompanyId, page, statusFilter]
  );

  const { data, isLoading, refetch } = useValuations(filters);
  const deleteValuation = useDeleteValuation();

  const stats = useMemo(() => {
    const items = data?.items ?? [];
    return {
      total: data?.total ?? 0,
      approved: items.filter((v) => v.status === 'approved').length,
      pending: items.filter((v) => v.status === 'pending_approval').length,
      draft: items.filter((v) => v.status === 'draft').length,
    };
  }, [data]);

  async function handleDelete(v: Valuation) {
    const ok = await confirm({
      title: 'Excluir Valuation',
      message: `Deseja excluir o valuation "${v.eventType}" de ${formatDate(v.valuationDate)}?`,
      confirmText: 'Excluir',
      confirmVariant: 'danger',
    });
    if (ok) deleteValuation.mutate(v.id);
  }

  if (!selectedCompanyId) {
    return (
      <div className="flex items-center justify-center h-64 text-gray-400">
        <p>Selecione uma empresa para ver os valuations.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6 animate-fade-in">
      <ConfirmDialog {...confirmState} onCancel={handleCancel} />

      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Valuations</h1>
          <p className="text-sm text-gray-500 mt-1">
            Histórico de avaliações de valuation da empresa
          </p>
        </div>
        <Button icon={<Plus size={16} />} onClick={() => navigate('/valuations/new')}>
          Novo Valuation
        </Button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <StatCard icon={<TrendingUp />} iconColor="text-blue-600" value={stats.total} label="Total" />
        <StatCard icon={<CheckCircle />} iconColor="text-green-600" value={stats.approved} label="Aprovados" />
        <StatCard icon={<Clock />} iconColor="text-yellow-600" value={stats.pending} label="Aguardando" />
        <StatCard icon={<FileText />} iconColor="text-gray-500" value={stats.draft} label="Rascunhos" />
      </div>

      {/* Filters */}
      <Card>
        <div className="p-4 flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Buscar por tipo de evento..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1); }}
              className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <select
            value={statusFilter}
            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">Todos os status</option>
            <option value="draft">Rascunho</option>
            <option value="pending_approval">Aguardando Aprovação</option>
            <option value="approved">Aprovado</option>
            <option value="rejected">Rejeitado</option>
          </select>
          <button
            onClick={() => refetch()}
            className="p-2 text-gray-400 hover:text-gray-600 transition-colors"
            title="Atualizar"
          >
            <ChevronRight size={16} />
          </button>
        </div>
      </Card>

      {/* Table */}
      {isLoading ? (
        <div className="flex justify-center py-16">
          <Spinner className="w-8 h-8" />
        </div>
      ) : !data?.items.length ? (
        <div className="text-center py-16 text-gray-400">
          <TrendingUp size={40} className="mx-auto mb-3 opacity-30" />
          <p className="font-medium">Nenhum valuation encontrado</p>
          <p className="text-sm mt-1">Crie o primeiro valuation para esta empresa.</p>
        </div>
      ) : (
        <Card>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left border-b border-gray-100">
                  <th className="px-4 py-3 font-medium text-gray-500">Data</th>
                  <th className="px-4 py-3 font-medium text-gray-500">Evento</th>
                  <th className="px-4 py-3 font-medium text-gray-500">Valor</th>
                  <th className="px-4 py-3 font-medium text-gray-500">Preço/Ação</th>
                  <th className="px-4 py-3 font-medium text-gray-500">Metodologias</th>
                  <th className="px-4 py-3 font-medium text-gray-500">Status</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {data.items
                  .filter((v) =>
                    !search ||
                    valuationEventTypeLabels[v.eventType]?.toLowerCase().includes(search.toLowerCase()) ||
                    v.eventName?.toLowerCase().includes(search.toLowerCase())
                  )
                  .map((v) => (
                    <tr
                      key={v.id}
                      className="hover:bg-gray-50 cursor-pointer transition-colors"
                      onClick={() => navigate(`/valuations/${v.id}`)}
                    >
                      <td className="px-4 py-3 text-gray-700">{formatDate(v.valuationDate)}</td>
                      <td className="px-4 py-3">
                        <div className="font-medium text-gray-900">
                          {valuationEventTypeLabels[v.eventType] ?? v.eventType}
                        </div>
                        {v.eventName && (
                          <div className="text-xs text-gray-400">{v.eventName}</div>
                        )}
                      </td>
                      <td className="px-4 py-3 font-medium text-gray-900">
                        {v.valuationAmount ? formatCurrency(v.valuationAmount) : '—'}
                      </td>
                      <td className="px-4 py-3 text-gray-600">
                        {v.pricePerShare ? `R$ ${v.pricePerShare.toFixed(4)}` : '—'}
                      </td>
                      <td className="px-4 py-3 text-gray-600">{v.methods.length}</td>
                      <td className="px-4 py-3">
                        <Badge className={valuationStatusColors[v.status] ?? ''}>
                          {valuationStatusLabels[v.status] ?? v.status}
                        </Badge>
                      </td>
                      <td
                        className="px-4 py-3 text-right"
                        onClick={(e) => e.stopPropagation()}
                      >
                        {v.status === 'draft' && (
                          <button
                            onClick={() => handleDelete(v)}
                            className="p-1 text-gray-300 hover:text-red-500 transition-colors rounded"
                          >
                            <XCircle size={16} />
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {(data.totalPages ?? 1) > 1 && (
            <div className="flex items-center justify-between px-4 py-3 border-t border-gray-100">
              <p className="text-sm text-gray-500">
                {data.total} valuations · página {page} de {data.totalPages}
              </p>
              <div className="flex gap-1">
                <button
                  disabled={page <= 1}
                  onClick={() => setPage((p) => p - 1)}
                  className="p-1.5 rounded hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  <ChevronLeft size={16} />
                </button>
                <button
                  disabled={page >= (data.totalPages ?? 1)}
                  onClick={() => setPage((p) => p + 1)}
                  className="p-1.5 rounded hover:bg-gray-100 disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  <ChevronRight size={16} />
                </button>
              </div>
            </div>
          )}
        </Card>
      )}
    </div>
  );
}
