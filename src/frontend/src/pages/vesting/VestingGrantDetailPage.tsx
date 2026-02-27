import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  CheckCircle,
  Clock,
  TrendingUp,
  DollarSign,
  Play,
  XCircle,
  BadgeCheck,
  Target,
} from 'lucide-react';
import { Button, Card, Spinner } from '@/components/ui';
import {
  VestingProgressBar,
  VestingScheduleTimeline,
  MilestoneList,
  ExerciseSharesModal,
} from '@/components/vesting';
import GrantMilestonesPage from './GrantMilestonesPage';
import {
  useVestingGrant,
  useVestingProjection,
  useVestingTransactions,
  useApproveVestingGrant,
  useActivateVestingGrant,
  useCancelVestingGrant,
  useExerciseShares,
  useVestingMilestonesByPlan,
  useAchieveVestingMilestone,
  useFailVestingMilestone,
} from '@/hooks';
import {
  VestingGrantDetailStatus,
  vestingGrantStatusLabels,
  vestingTransactionTypeLabels,
  type ExerciseSharesRequest,
  type AchieveMilestoneRequest,
} from '@/types';

const statusColors: Record<VestingGrantDetailStatus, string> = {
  [VestingGrantDetailStatus.Pending]: 'bg-amber-100 text-amber-700',
  [VestingGrantDetailStatus.Approved]: 'bg-blue-100 text-blue-700',
  [VestingGrantDetailStatus.Active]: 'bg-green-100 text-green-700',
  [VestingGrantDetailStatus.Exercised]: 'bg-indigo-100 text-indigo-700',
  [VestingGrantDetailStatus.Expired]: 'bg-red-100 text-red-700',
  [VestingGrantDetailStatus.Cancelled]: 'bg-gray-100 text-gray-500',
};

export default function VestingGrantDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [exerciseModalOpen, setExerciseModalOpen] = useState(false);

  const { data: grant, isLoading, isError } = useVestingGrant(id ?? '');
  const { data: projection } = useVestingProjection(id ?? '');
  const { data: transactions } = useVestingTransactions(id ?? '');
  const { data: milestones } = useVestingMilestonesByPlan(grant?.vestingPlanId ?? '');

  const approveMutation = useApproveVestingGrant();
  const activateMutation = useActivateVestingGrant();
  const cancelMutation = useCancelVestingGrant();
  const exerciseMutation = useExerciseShares();
  const achieveMilestoneMutation = useAchieveVestingMilestone();
  const failMilestoneMutation = useFailVestingMilestone();

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <Spinner className="w-8 h-8 text-indigo-600" />
      </div>
    );
  }

  if (isError || !grant) {
    return (
      <div className="flex flex-col items-center justify-center py-20 gap-4">
        <p className="text-gray-500">Grant não encontrado</p>
        <Button variant="ghost" onClick={() => navigate(-1)}>
          <ArrowLeft size={16} className="mr-2" />
          Voltar
        </Button>
      </div>
    );
  }

  function handleExercise(data: ExerciseSharesRequest) {
    exerciseMutation.mutate(
      { id: grant!.id, data },
      { onSuccess: () => setExerciseModalOpen(false) }
    );
  }

  function handleAchieveMilestone(milestoneId: string) {
    const req: AchieveMilestoneRequest = {
      achievedDate: new Date().toISOString().slice(0, 10),
    };
    achieveMilestoneMutation.mutate({ id: milestoneId, data: req });
  }

  const canApprove = grant.status === VestingGrantDetailStatus.Pending;
  const canActivate = grant.status === VestingGrantDetailStatus.Approved;
  const canCancel =
    grant.status === VestingGrantDetailStatus.Pending ||
    grant.status === VestingGrantDetailStatus.Approved ||
    grant.status === VestingGrantDetailStatus.Active;
  const canExercise =
    grant.status === VestingGrantDetailStatus.Active && grant.availableToExercise > 0;

  return (
    <div className="space-y-6 animate-fade-in max-w-4xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <button
            className="p-2 rounded-lg hover:bg-gray-100 text-gray-500 transition-colors"
            onClick={() => navigate(-1)}
          >
            <ArrowLeft size={18} />
          </button>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-xl font-bold text-gray-900">{grant.shareholderName}</h1>
              <span
                className={`text-xs font-medium px-2.5 py-1 rounded-full ${statusColors[grant.status]}`}
              >
                {vestingGrantStatusLabels[grant.status]}
              </span>
            </div>
            <p className="text-sm text-gray-500 mt-0.5">{grant.vestingPlanName}</p>
          </div>
        </div>

        {/* Actions */}
        <div className="flex items-center gap-2">
          {canApprove && (
            <Button
              size="sm"
              variant="ghost"
              onClick={() => approveMutation.mutate(grant.id)}
              disabled={approveMutation.isPending}
            >
              <BadgeCheck size={15} className="mr-1.5" />
              Aprovar
            </Button>
          )}
          {canActivate && (
            <Button
              size="sm"
              onClick={() => activateMutation.mutate(grant.id)}
              disabled={activateMutation.isPending}
            >
              <Play size={15} className="mr-1.5" />
              Ativar
            </Button>
          )}
          {canExercise && (
            <Button size="sm" onClick={() => setExerciseModalOpen(true)}>
              <TrendingUp size={15} className="mr-1.5" />
              Exercer ações
            </Button>
          )}
          {canCancel && (
            <Button
              size="sm"
              variant="ghost"
              className="text-red-600 hover:bg-red-50"
              onClick={() => cancelMutation.mutate(grant.id)}
              disabled={cancelMutation.isPending}
            >
              <XCircle size={15} className="mr-1.5" />
              Cancelar
            </Button>
          )}
        </div>
      </div>

      {/* Summary cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <Card className="p-4">
          <p className="text-xs text-gray-500 mb-1">Total de ações</p>
          <p className="text-xl font-bold text-gray-900">{grant.totalShares.toLocaleString('pt-BR')}</p>
          <p className="text-xs text-gray-400 mt-0.5">
            {grant.equityPercentage.toFixed(4)}% equity
          </p>
        </Card>
        <Card className="p-4">
          <p className="text-xs text-gray-500 mb-1">Vestidas</p>
          <p className="text-xl font-bold text-indigo-600">
            {grant.vestedShares.toLocaleString('pt-BR')}
          </p>
          <p className="text-xs text-gray-400 mt-0.5">{grant.vestedPercentage.toFixed(1)}%</p>
        </Card>
        <Card className="p-4">
          <p className="text-xs text-gray-500 mb-1">Disponíveis</p>
          <p className="text-xl font-bold text-green-600">
            {grant.availableToExercise.toLocaleString('pt-BR')}
          </p>
          <p className="text-xs text-gray-400 mt-0.5">para exercício</p>
        </Card>
        <Card className="p-4">
          <p className="text-xs text-gray-500 mb-1">Exercidas</p>
          <p className="text-xl font-bold text-gray-700">
            {grant.exercisedShares.toLocaleString('pt-BR')}
          </p>
          <p className="text-xs text-gray-400 mt-0.5">
            {grant.sharePrice.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })} / ação
          </p>
        </Card>
      </div>

      {/* Progress */}
      <Card className="p-5">
        <h2 className="font-semibold text-gray-800 mb-4 flex items-center gap-2">
          <TrendingUp size={17} className="text-indigo-500" />
          Progresso de vesting
        </h2>
        <VestingProgressBar
          vestedPercentage={grant.vestedPercentage}
          exercisedShares={grant.exercisedShares}
          vestedShares={grant.vestedShares}
          totalShares={grant.totalShares}
          size="lg"
        />
        <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 mt-4 text-sm text-gray-600">
          <div>
            <span className="text-xs text-gray-400 block">Data do grant</span>
            {new Date(grant.grantDate).toLocaleDateString('pt-BR')}
          </div>
          <div>
            <span className="text-xs text-gray-400 block">Início do vesting</span>
            {new Date(grant.vestingStartDate).toLocaleDateString('pt-BR')}
          </div>
          {grant.cliffDate && (
            <div>
              <span className="text-xs text-gray-400 block">Data do cliff</span>
              {new Date(grant.cliffDate).toLocaleDateString('pt-BR')}
            </div>
          )}
          <div>
            <span className="text-xs text-gray-400 block">Fim do vesting</span>
            {new Date(grant.vestingEndDate).toLocaleDateString('pt-BR')}
          </div>
          {grant.approvedAt && (
            <div>
              <span className="text-xs text-gray-400 block">Aprovado em</span>
              {new Date(grant.approvedAt).toLocaleDateString('pt-BR')}
            </div>
          )}
        </div>
      </Card>

      {/* Timeline */}
      {projection && projection.points.length > 0 && (
        <Card className="p-5">
          <h2 className="font-semibold text-gray-800 mb-4 flex items-center gap-2">
            <Clock size={17} className="text-indigo-500" />
            Cronograma de vesting
          </h2>
          <VestingScheduleTimeline
            points={projection.points}
            cliffDate={grant.cliffDate}
            vestingStartDate={grant.vestingStartDate}
            vestingEndDate={grant.vestingEndDate}
          />
        </Card>
      )}

      {/* Milestones */}
      {milestones && milestones.length > 0 && (
        <Card className="p-5">
          <h2 className="font-semibold text-gray-800 mb-4 flex items-center gap-2">
            <CheckCircle size={17} className="text-indigo-500" />
            Metas de performance
          </h2>
          <MilestoneList
            milestones={milestones}
            onAchieve={handleAchieveMilestone}
            onFail={(milestoneId) => failMilestoneMutation.mutate(milestoneId)}
          />
        </Card>
      )}

      {/* Transactions */}
      {transactions && transactions.length > 0 && (
        <Card className="p-5">
          <h2 className="font-semibold text-gray-800 mb-4 flex items-center gap-2">
            <DollarSign size={17} className="text-indigo-500" />
            Histórico de exercícios
          </h2>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100">
                  <th className="text-left py-2 px-3 text-xs text-gray-500 font-medium">Data</th>
                  <th className="text-right py-2 px-3 text-xs text-gray-500 font-medium">Ações</th>
                  <th className="text-right py-2 px-3 text-xs text-gray-500 font-medium">Preço</th>
                  <th className="text-right py-2 px-3 text-xs text-gray-500 font-medium">Total</th>
                  <th className="text-right py-2 px-3 text-xs text-gray-500 font-medium">Ganho</th>
                  <th className="text-left py-2 px-3 text-xs text-gray-500 font-medium">Tipo</th>
                </tr>
              </thead>
              <tbody>
                {transactions.map((tx) => (
                  <tr key={tx.id} className="border-b border-gray-50 hover:bg-gray-50">
                    <td className="py-2.5 px-3 text-gray-600">
                      {new Date(tx.transactionDate).toLocaleDateString('pt-BR')}
                    </td>
                    <td className="py-2.5 px-3 text-right text-gray-900 font-medium">
                      {tx.sharesExercised.toLocaleString('pt-BR')}
                    </td>
                    <td className="py-2.5 px-3 text-right text-gray-600">
                      {tx.sharePriceAtExercise.toLocaleString('pt-BR', {
                        style: 'currency',
                        currency: 'BRL',
                      })}
                    </td>
                    <td className="py-2.5 px-3 text-right text-gray-900">
                      {tx.totalExerciseValue.toLocaleString('pt-BR', {
                        style: 'currency',
                        currency: 'BRL',
                      })}
                    </td>
                    <td
                      className={`py-2.5 px-3 text-right font-medium ${tx.gainAmount >= 0 ? 'text-green-600' : 'text-red-600'}`}
                    >
                      {tx.gainAmount.toLocaleString('pt-BR', {
                        style: 'currency',
                        currency: 'BRL',
                      })}
                    </td>
                    <td className="py-2.5 px-3 text-gray-500">
                      {vestingTransactionTypeLabels[tx.transactionType]}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      {/* Grant-level Performance Milestones */}
      <Card className="p-5">
        <h2 className="font-semibold text-gray-800 mb-4 flex items-center gap-2">
          <Target size={17} className="text-purple-500" />
          Milestones de Performance do Grant
        </h2>
        <GrantMilestonesPage grantId={grant.id} companyId={grant.companyId} />
      </Card>

      {/* Exercise Modal */}
      <ExerciseSharesModal
        grant={grant}
        open={exerciseModalOpen}
        onClose={() => setExerciseModalOpen(false)}
        onConfirm={handleExercise}
        isLoading={exerciseMutation.isPending}
      />
    </div>
  );
}
