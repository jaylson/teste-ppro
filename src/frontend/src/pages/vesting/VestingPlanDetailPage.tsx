import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Plus,
  Users,
  Calendar,
  TrendingUp,
  Clock,
  Target,
  CheckCircle,
  PlayCircle,
  Archive,
  PauseCircle,
  RefreshCw,
} from 'lucide-react';
import { Button, Card, Spinner } from '@/components/ui';
import { VestingProgressBar } from '@/components/vesting';
import CreateGrantModal from '@/components/vesting/CreateGrantModal';
import {
  useVestingPlan,
  useActivateVestingPlan,
  useDeactivateVestingPlan,
  useArchiveVestingPlan,
} from '@/hooks/useVestingPlans';
import { useVestingGrants, useCreateVestingGrant } from '@/hooks/useVestingGrants';
import {
  VestingPlanStatus,
  VestingGrantDetailStatus,
  vestingTypeLabels,
  vestingGrantStatusLabels,
  type CreateVestingGrantRequest,
} from '@/types';

const statusColors: Record<VestingPlanStatus, string> = {
  [VestingPlanStatus.Draft]: 'bg-amber-100 text-amber-700',
  [VestingPlanStatus.Active]: 'bg-green-100 text-green-700',
  [VestingPlanStatus.Inactive]: 'bg-gray-100 text-gray-600',
  [VestingPlanStatus.Archived]: 'bg-red-100 text-red-600',
};

const grantStatusColors: Record<VestingGrantDetailStatus, string> = {
  [VestingGrantDetailStatus.Pending]: 'bg-amber-100 text-amber-700',
  [VestingGrantDetailStatus.Approved]: 'bg-blue-100 text-blue-700',
  [VestingGrantDetailStatus.Active]: 'bg-green-100 text-green-700',
  [VestingGrantDetailStatus.Exercised]: 'bg-indigo-100 text-indigo-700',
  [VestingGrantDetailStatus.Expired]: 'bg-red-100 text-red-600',
  [VestingGrantDetailStatus.Cancelled]: 'bg-gray-100 text-gray-500',
};

export default function VestingPlanDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [grantModalOpen, setGrantModalOpen] = useState(false);

  const { data: plan, isLoading, isError, refetch } = useVestingPlan(id ?? '');
  const { data: grantsData, isLoading: loadingGrants } = useVestingGrants({
    vestingPlanId: id,
    pageSize: 50,
  });

  const activateMutation = useActivateVestingPlan();
  const deactivateMutation = useDeactivateVestingPlan();
  const archiveMutation = useArchiveVestingPlan();
  const createGrantMutation = useCreateVestingGrant();

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <Spinner className="w-8 h-8 text-indigo-600" />
      </div>
    );
  }

  if (isError || !plan) {
    return (
      <div className="flex flex-col items-center justify-center py-20 gap-4">
        <p className="text-gray-500">Plano não encontrado.</p>
        <Button variant="ghost" onClick={() => navigate('/vesting')}>
          <ArrowLeft size={16} className="mr-2" />
          Voltar
        </Button>
      </div>
    );
  }

  const grants = grantsData?.items ?? [];
  const totalShares = grants.reduce((a, g) => a + g.totalShares, 0);
  const totalVested = grants.reduce((a, g) => a + g.vestedShares, 0);
  const totalExercised = grants.reduce((a, g) => a + g.exercisedShares, 0);
  const activeGrants = grants.filter((g) => g.status === VestingGrantDetailStatus.Active).length;

  function handleCreateGrant(data: CreateVestingGrantRequest) {
    createGrantMutation.mutate(data, {
      onSuccess: () => setGrantModalOpen(false),
    });
  }

  return (
    <div className="space-y-6 animate-fade-in max-w-5xl mx-auto">
      {/* Cabeçalho */}
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-start gap-3">
          <button
            className="mt-1 p-2 rounded-lg hover:bg-gray-100 text-gray-500 transition-colors"
            onClick={() => navigate('/vesting')}
          >
            <ArrowLeft size={18} />
          </button>
          <div>
            <div className="flex items-center gap-2 flex-wrap">
              <h1 className="text-xl font-bold text-gray-900">{plan.name}</h1>
              <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${statusColors[plan.status]}`}>
                {plan.status}
              </span>
            </div>
            {plan.description && (
              <p className="text-sm text-gray-500 mt-1">{plan.description}</p>
            )}
          </div>
        </div>

        {/* Ações do plano */}
        <div className="flex items-center gap-2 shrink-0">
          <button
            className="p-2 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors"
            onClick={() => refetch()}
            title="Atualizar"
          >
            <RefreshCw size={16} />
          </button>
          {plan.status === VestingPlanStatus.Draft && (
            <Button
              size="sm"
              onClick={() => activateMutation.mutate(plan.id)}
              disabled={activateMutation.isPending}
            >
              <PlayCircle size={15} className="mr-1.5" />
              Ativar plano
            </Button>
          )}
          {plan.status === VestingPlanStatus.Active && (
            <Button
              size="sm"
              variant="ghost"
              onClick={() => deactivateMutation.mutate(plan.id)}
              disabled={deactivateMutation.isPending}
            >
              <PauseCircle size={15} className="mr-1.5" />
              Desativar
            </Button>
          )}
          {(plan.status === VestingPlanStatus.Active || plan.status === VestingPlanStatus.Inactive) && (
            <Button
              size="sm"
              variant="ghost"
              className="text-gray-500"
              onClick={() => archiveMutation.mutate(plan.id)}
              disabled={archiveMutation.isPending}
            >
              <Archive size={15} className="mr-1.5" />
              Arquivar
            </Button>
          )}
          {plan.status === VestingPlanStatus.Active && (
            <Button
              size="sm"
              onClick={() => setGrantModalOpen(true)}
              className="flex items-center gap-2"
            >
              <Plus size={15} />
              Novo grant
            </Button>
          )}
        </div>
      </div>

      {/* Cards de info do plano */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <Card className="p-4">
          <p className="text-xs text-gray-500 mb-1">Tipo</p>
          <p className="text-sm font-semibold text-gray-900">
            {vestingTypeLabels[plan.vestingType] ?? plan.vestingType}
          </p>
        </Card>
        <Card className="p-4">
          <div className="flex items-center gap-1.5 mb-1">
            <Clock size={13} className="text-gray-400" />
            <p className="text-xs text-gray-500">Cliff</p>
          </div>
          <p className="text-sm font-semibold text-gray-900">
            {plan.cliffMonths === 0 ? 'Sem cliff' : `${plan.cliffMonths} meses`}
          </p>
        </Card>
        <Card className="p-4">
          <div className="flex items-center gap-1.5 mb-1">
            <Calendar size={13} className="text-gray-400" />
            <p className="text-xs text-gray-500">Período de vesting</p>
          </div>
          <p className="text-sm font-semibold text-gray-900">{plan.vestingMonths} meses</p>
        </Card>
        <Card className="p-4">
          <div className="flex items-center gap-1.5 mb-1">
            <TrendingUp size={13} className="text-gray-400" />
            <p className="text-xs text-gray-500">Equity total</p>
          </div>
          <p className="text-sm font-semibold text-indigo-600">{plan.totalEquityPercentage}%</p>
        </Card>
      </div>

      {/* Resumo dos grants */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <Card className="p-4 text-center">
          <Users size={20} className="text-blue-500 mx-auto mb-1" />
          <p className="text-2xl font-bold text-gray-900">{grants.length}</p>
          <p className="text-xs text-gray-500 mt-0.5">Beneficiários</p>
        </Card>
        <Card className="p-4 text-center">
          <CheckCircle size={20} className="text-green-500 mx-auto mb-1" />
          <p className="text-2xl font-bold text-gray-900">{activeGrants}</p>
          <p className="text-xs text-gray-500 mt-0.5">Grants ativos</p>
        </Card>
        <Card className="p-4 text-center">
          <TrendingUp size={20} className="text-indigo-500 mx-auto mb-1" />
          <p className="text-2xl font-bold text-indigo-600">{totalVested.toLocaleString('pt-BR')}</p>
          <p className="text-xs text-gray-500 mt-0.5">Ações vestidas</p>
        </Card>
        <Card className="p-4 text-center">
          <Target size={20} className="text-amber-500 mx-auto mb-1" />
          <p className="text-2xl font-bold text-gray-900">{totalExercised.toLocaleString('pt-BR')}</p>
          <p className="text-xs text-gray-500 mt-0.5">Ações exercidas</p>
        </Card>
      </div>

      {/* Tabela de grants */}
      <Card>
        <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100">
          <h2 className="text-sm font-semibold text-gray-900">
            Beneficiários do plano
            {grants.length > 0 && (
              <span className="ml-2 text-xs font-normal bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
                {grants.length}
              </span>
            )}
          </h2>
          {plan.status === VestingPlanStatus.Active && (
            <Button size="sm" variant="ghost" onClick={() => setGrantModalOpen(true)}>
              <Plus size={14} className="mr-1" />
              Adicionar beneficiário
            </Button>
          )}
        </div>

        {loadingGrants ? (
          <div className="flex justify-center py-10">
            <Spinner className="w-6 h-6 text-indigo-600" />
          </div>
        ) : grants.length === 0 ? (
          <div className="py-12 text-center">
            <Users className="w-10 h-10 text-gray-300 mx-auto mb-3" />
            <p className="text-sm font-medium text-gray-500">Nenhum beneficiário ainda</p>
            {plan.status === VestingPlanStatus.Active ? (
              <p className="text-xs text-gray-400 mt-1 mb-4">
                Ative o plano e adicione o primeiro beneficiário
              </p>
            ) : (
              <p className="text-xs text-gray-400 mt-1 mb-4">
                Ative o plano para adicionar beneficiários
              </p>
            )}
            {plan.status === VestingPlanStatus.Active && (
              <Button size="sm" onClick={() => setGrantModalOpen(true)}>
                <Plus size={14} className="mr-1.5" />
                Adicionar beneficiário
              </Button>
            )}
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100 bg-gray-50/50">
                  <th className="text-left px-5 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide">
                    Beneficiário
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
                  <th className="px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide">
                    Progresso
                  </th>
                  <th className="text-left px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide">
                    Início
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {grants.map((grant) => (
                  <tr
                    key={grant.id}
                    className="hover:bg-gray-50/50 cursor-pointer transition-colors"
                    onClick={() => navigate(`/vesting/grant/${grant.id}`)}
                  >
                    <td className="px-5 py-3.5 font-medium text-gray-900">
                      {grant.shareholderName}
                    </td>
                    <td className="px-4 py-3.5">
                      <span
                        className={`text-xs font-medium px-2.5 py-1 rounded-full ${
                          grantStatusColors[grant.status]
                        }`}
                      >
                        {vestingGrantStatusLabels[grant.status] ?? grant.status}
                      </span>
                    </td>
                    <td className="px-4 py-3.5 text-right text-gray-700">
                      {grant.totalShares.toLocaleString('pt-BR')}
                    </td>
                    <td className="px-4 py-3.5 text-right text-indigo-600 font-medium">
                      {grant.vestedShares.toLocaleString('pt-BR')}
                    </td>
                    <td className="px-4 py-3.5 min-w-[140px]">
                      <VestingProgressBar vestedPercentage={grant.vestedPercentage} size="sm" />
                    </td>
                    <td className="px-4 py-3.5 text-gray-500 text-xs">
                      {new Date(grant.vestingStartDate).toLocaleDateString('pt-BR')}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            {/* Totais */}
            {totalShares > 0 && (
              <div className="flex items-center justify-end gap-6 px-5 py-3 border-t border-gray-100 bg-gray-50/50 text-xs text-gray-500">
                <span>
                  Total pool:{' '}
                  <strong className="text-gray-800">{totalShares.toLocaleString('pt-BR')} ações</strong>
                </span>
                <span>
                  Vestidas:{' '}
                  <strong className="text-indigo-600">{totalVested.toLocaleString('pt-BR')}</strong>
                </span>
                <span>
                  Exercidas:{' '}
                  <strong className="text-gray-800">{totalExercised.toLocaleString('pt-BR')}</strong>
                </span>
              </div>
            )}
          </div>
        )}
      </Card>

      {/* Modal criar grant */}
      <CreateGrantModal
        open={grantModalOpen}
        companyId={plan.companyId}
        preselectedPlanId={plan.id}
        onClose={() => setGrantModalOpen(false)}
        onSubmit={handleCreateGrant}
        isLoading={createGrantMutation.isPending}
      />
    </div>
  );
}
