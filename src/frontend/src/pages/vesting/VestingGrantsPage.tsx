import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus,
  Search,
  Users,
  TrendingUp,
  CheckCircle,
  Clock,
  RefreshCw,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react';
import { Button, Card, Spinner, StatCard } from '@/components/ui';
import { VestingProgressBar } from '@/components/vesting';
import CreateGrantModal from '@/components/vesting/CreateGrantModal';
import { useVestingGrants, useCreateVestingGrant } from '@/hooks/useVestingGrants';
import { useClientStore } from '@/stores/clientStore';
import {
  VestingGrantDetailStatus,
  vestingGrantStatusLabels,
  type CreateVestingGrantRequest,
} from '@/types';

const statusColors: Record<VestingGrantDetailStatus, string> = {
  [VestingGrantDetailStatus.Pending]: 'bg-amber-100 text-amber-700',
  [VestingGrantDetailStatus.Approved]: 'bg-blue-100 text-blue-700',
  [VestingGrantDetailStatus.Active]: 'bg-green-100 text-green-700',
  [VestingGrantDetailStatus.Exercised]: 'bg-indigo-100 text-indigo-700',
  [VestingGrantDetailStatus.Expired]: 'bg-red-100 text-red-600',
  [VestingGrantDetailStatus.Cancelled]: 'bg-gray-100 text-gray-500',
};

const PAGE_SIZE = 15;

export default function VestingGrantsPage() {
  const navigate = useNavigate();
  const { selectedCompanyId } = useClientStore();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const [grantModalOpen, setGrantModalOpen] = useState(false);

  const filters = useMemo(
    () => ({
      companyId: selectedCompanyId ?? undefined,
      status: statusFilter || undefined,
      page,
      pageSize: PAGE_SIZE,
    }),
    [selectedCompanyId, statusFilter, page]
  );

  const { data, isLoading, isError, refetch } = useVestingGrants(filters);
  const createGrantMutation = useCreateVestingGrant();

  const grants = data?.items ?? [];

  // Filtro de busca local por nome do sócio / plano
  const filtered = useMemo(() => {
    if (!search.trim()) return grants;
    const q = search.toLowerCase();
    return grants.filter(
      (g) =>
        g.shareholderName.toLowerCase().includes(q) ||
        g.vestingPlanName.toLowerCase().includes(q)
    );
  }, [grants, search]);

  const stats = useMemo(() => {
    const all = grants;
    return {
      total: data?.totalCount ?? 0,
      active: all.filter((g) => g.status === VestingGrantDetailStatus.Active).length,
      pending: all.filter(
        (g) =>
          g.status === VestingGrantDetailStatus.Pending ||
          g.status === VestingGrantDetailStatus.Approved
      ).length,
      totalVested: all.reduce((a, g) => a + g.vestedShares, 0),
    };
  }, [grants, data]);

  function handleCreateGrant(req: CreateVestingGrantRequest) {
    createGrantMutation.mutate(req, {
      onSuccess: () => setGrantModalOpen(false),
    });
  }

  if (!selectedCompanyId) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="p-8 text-center">
          <Users className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Nenhuma empresa selecionada</h2>
          <p className="text-gray-500">Selecione uma empresa para ver os grants de vesting.</p>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Cabeçalho */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Grants de Vesting</h1>
          <p className="page-subtitle">Gerencie as concessões de equity para beneficiários</p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="sm" onClick={() => refetch()}>
            <RefreshCw size={16} />
          </Button>
          <Button onClick={() => setGrantModalOpen(true)} className="flex items-center gap-2">
            <Plus size={16} />
            Novo Grant
          </Button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Total de grants"
          value={stats.total}
          icon={<Users size={20} className="text-blue-500" />}
        />
        <StatCard
          label="Grants ativos"
          value={stats.active}
          icon={<CheckCircle size={20} className="text-green-500" />}
        />
        <StatCard
          label="Aguardando aprovação"
          value={stats.pending}
          icon={<Clock size={20} className="text-amber-500" />}
        />
        <StatCard
          label="Ações vestidas"
          value={stats.totalVested.toLocaleString('pt-BR')}
          icon={<TrendingUp size={20} className="text-indigo-500" />}
        />
      </div>

      {/* Filtros */}
      <Card className="p-4">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Buscar por sócio ou plano..."
              className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPage(1);
              }}
            />
          </div>
          <select
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value);
              setPage(1);
            }}
          >
            <option value="">Todos os status</option>
            <option value="Pending">Pendente</option>
            <option value="Approved">Aprovado</option>
            <option value="Active">Ativo</option>
            <option value="Exercised">Exercido</option>
            <option value="Expired">Expirado</option>
            <option value="Cancelled">Cancelado</option>
          </select>
          {(search || statusFilter) && (
            <button
              className="text-sm text-gray-500 hover:text-gray-700 px-3 py-2 border border-gray-200 rounded-lg"
              onClick={() => {
                setSearch('');
                setStatusFilter('');
                setPage(1);
              }}
            >
              Limpar
            </button>
          )}
        </div>
      </Card>

      {/* Tabela */}
      <Card>
        {isLoading ? (
          <div className="flex justify-center py-16">
            <Spinner className="w-7 h-7 text-indigo-600" />
          </div>
        ) : isError ? (
          <div className="p-8 text-center">
            <p className="text-gray-500 mb-4">Erro ao carregar grants.</p>
            <Button onClick={() => refetch()}>Tentar novamente</Button>
          </div>
        ) : filtered.length === 0 ? (
          <div className="py-14 text-center">
            <Users className="w-10 h-10 text-gray-300 mx-auto mb-3" />
            <h3 className="font-semibold text-gray-700 mb-1">Nenhum grant encontrado</h3>
            <p className="text-gray-400 text-sm mb-4">
              {search || statusFilter
                ? 'Tente ajustar os filtros'
                : 'Crie o primeiro grant de vesting para esta empresa'}
            </p>
            {!search && !statusFilter && (
              <Button onClick={() => setGrantModalOpen(true)} className="mx-auto flex gap-2">
                <Plus size={15} />
                Criar grant
              </Button>
            )}
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100 bg-gray-50/50">
                    <th className="text-left px-5 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide">
                      Beneficiário
                    </th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide">
                      Plano
                    </th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide">
                      Status
                    </th>
                    <th className="text-right px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide">
                      Total
                    </th>
                    <th className="text-right px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide">
                      Vestidas
                    </th>
                    <th className="text-right px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide hidden lg:table-cell">
                      Exercidas
                    </th>
                    <th className="px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide hidden md:table-cell">
                      Progresso
                    </th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide hidden xl:table-cell">
                      Início
                    </th>
                    <th className="text-left px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide hidden xl:table-cell">
                      Fim
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {filtered.map((grant) => (
                    <tr
                      key={grant.id}
                      className="hover:bg-gray-50/60 cursor-pointer transition-colors"
                      onClick={() => navigate(`/vesting/grant/${grant.id}`)}
                    >
                      <td className="px-5 py-3.5 font-medium text-gray-900">
                        {grant.shareholderName}
                      </td>
                      <td className="px-4 py-3.5 text-gray-600 max-w-[180px] truncate">
                        {grant.vestingPlanName}
                      </td>
                      <td className="px-4 py-3.5">
                        <span
                          className={`text-xs font-medium px-2.5 py-1 rounded-full ${
                            statusColors[grant.status]
                          }`}
                        >
                          {vestingGrantStatusLabels[grant.status] ?? grant.status}
                        </span>
                      </td>
                      <td className="px-4 py-3.5 text-right text-gray-700 tabular-nums">
                        {grant.totalShares.toLocaleString('pt-BR')}
                      </td>
                      <td className="px-4 py-3.5 text-right text-indigo-600 font-medium tabular-nums">
                        {grant.vestedShares.toLocaleString('pt-BR')}
                      </td>
                      <td className="px-4 py-3.5 text-right text-gray-600 tabular-nums hidden lg:table-cell">
                        {grant.exercisedShares.toLocaleString('pt-BR')}
                      </td>
                      <td className="px-4 py-3.5 min-w-[130px] hidden md:table-cell">
                        <VestingProgressBar vestedPercentage={grant.vestedPercentage} size="sm" />
                      </td>
                      <td className="px-4 py-3.5 text-gray-500 text-xs hidden xl:table-cell">
                        {new Date(grant.vestingStartDate).toLocaleDateString('pt-BR')}
                      </td>
                      <td className="px-4 py-3.5 text-gray-500 text-xs hidden xl:table-cell">
                        {new Date(grant.vestingEndDate).toLocaleDateString('pt-BR')}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Paginação */}
            {(data?.totalPages ?? 1) > 1 && (
              <div className="flex items-center justify-between px-5 py-3 border-t border-gray-100">
                <p className="text-xs text-gray-500">
                  Mostrando {(page - 1) * PAGE_SIZE + 1}–
                  {Math.min(page * PAGE_SIZE, data?.totalCount ?? 0)} de{' '}
                  {data?.totalCount ?? 0} grants
                </p>
                <div className="flex items-center gap-2">
                  <button
                    className="p-1.5 rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-40 transition-colors"
                    disabled={page === 1}
                    onClick={() => setPage((p) => p - 1)}
                  >
                    <ChevronLeft size={15} />
                  </button>
                  <span className="text-sm text-gray-600 px-2">
                    {page} / {data?.totalPages}
                  </span>
                  <button
                    className="p-1.5 rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-40 transition-colors"
                    disabled={page === data?.totalPages}
                    onClick={() => setPage((p) => p + 1)}
                  >
                    <ChevronRight size={15} />
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </Card>

      {/* Modal criar grant */}
      <CreateGrantModal
        open={grantModalOpen}
        companyId={selectedCompanyId}
        onClose={() => setGrantModalOpen(false)}
        onSubmit={handleCreateGrant}
        isLoading={createGrantMutation.isPending}
      />
    </div>
  );
}
