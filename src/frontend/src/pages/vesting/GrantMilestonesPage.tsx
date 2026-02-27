import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { Target, Zap, Plus } from 'lucide-react';
import { Card } from '@/components/ui';
import {
  MilestoneCard,
  MilestoneProgressTracker,
  AccelerationCalculator,
  AddMilestoneToGrantModal,
} from '@/components/vesting';
import {
  useGrantMilestonesByGrant,
  useMilestoneProgressDashboard,
  useVerifyGrantMilestone,
  useFailGrantMilestone,
  useCancelGrantMilestone,
  useDeleteGrantMilestone,
} from '@/hooks';
import type { GrantMilestone, AchieveGrantMilestoneRequest } from '@/types';
import { useAchieveGrantMilestone } from '@/hooks';

interface GrantMilestonesPageProps {
  grantId?: string;
  companyId?: string;
}

export default function GrantMilestonesPage({
  grantId: propGrantId,
  companyId,
}: GrantMilestonesPageProps) {
  const { grantId: paramGrantId } = useParams<{ grantId: string }>();
  const grantId = propGrantId ?? paramGrantId ?? '';

  const [trackingMilestone, setTrackingMilestone] = useState<GrantMilestone | null>(null);
  const [acceleratingMilestone, setAcceleratingMilestone] = useState<GrantMilestone | null>(null);
  const [addModalOpen, setAddModalOpen] = useState(false);

  const { data: milestones, isLoading } = useGrantMilestonesByGrant(grantId);
  const { data: dashboard } = useMilestoneProgressDashboard(grantId);

  const achieveMutation = useAchieveGrantMilestone();
  const verifyMutation = useVerifyGrantMilestone();
  const failMutation = useFailGrantMilestone();
  const cancelMutation = useCancelGrantMilestone();
  const deleteMutation = useDeleteGrantMilestone();

  function handleAchieve(milestone: GrantMilestone) {
    const req: AchieveGrantMilestoneRequest = {
      achievedDate: new Date().toISOString().slice(0, 10),
    };
    achieveMutation.mutate({ id: milestone.id, data: req });
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12 text-gray-400 text-sm">
        Carregando milestones...
      </div>
    );
  }

  return (
    <div className="space-y-5">
      {/* Dashboard summary */}
      {dashboard && (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
          {[
            { label: 'Pendentes', value: dashboard.pendingCount, color: 'text-gray-600' },
            { label: 'Em andamento', value: dashboard.inProgressCount, color: 'text-blue-600' },
            { label: 'Atingidos', value: dashboard.achievedCount, color: 'text-green-600' },
            { label: 'Falharam', value: dashboard.failedCount, color: 'text-red-500' },
          ].map((stat) => (
            <Card key={stat.label} className="p-4 text-center">
              <p className={`text-2xl font-bold ${stat.color}`}>{stat.value}</p>
              <p className="text-xs text-gray-400 mt-0.5">{stat.label}</p>
            </Card>
          ))}
        </div>
      )}

      {/* Acceleration summary */}
      {dashboard && dashboard.pendingAcceleration > 0 && (
        <div className="flex items-center gap-3 bg-purple-50 border border-purple-200 rounded-xl p-4">
          <Zap size={18} className="text-purple-600 flex-shrink-0" />
          <div className="text-sm">
            <span className="font-semibold text-purple-800">Aceleração pendente</span>
            <span className="text-purple-600 ml-2">
              {dashboard.pendingAcceleration.toLocaleString('pt-BR')} ações aguardando aplicação
            </span>
          </div>
        </div>
      )}

      {/* Add button */}
      {companyId && (
        <div className="flex justify-end">
          <button
            onClick={() => setAddModalOpen(true)}
            className="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            <Plus size={14} />
            Adicionar Milestone
          </button>
        </div>
      )}

      {/* Milestones list */}
      {!milestones || milestones.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-gray-400">
          <Target size={32} className="mb-3 opacity-40" />
          <p className="text-sm">Nenhum milestone configurado.</p>
          {companyId && (
            <button
              onClick={() => setAddModalOpen(true)}
              className="mt-3 flex items-center gap-1.5 px-3 py-1.5 text-sm bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
            >
              <Plus size={14} />
              Adicionar primeiro milestone
            </button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          {milestones.map((m) => (
            <MilestoneCard
              key={m.id}
              milestone={m}
              onRecordProgress={(ms: GrantMilestone) => setTrackingMilestone(ms)}
              onAchieve={handleAchieve}
              onVerify={(id: string) => verifyMutation.mutate(id)}
              onFail={(id: string) => failMutation.mutate(id)}
              onCancel={(id: string) => cancelMutation.mutate(id)}
              onApplyAcceleration={(ms: GrantMilestone) => setAcceleratingMilestone(ms)}
              onDelete={(id: string) => {
                if (confirm('Remover milestone?')) deleteMutation.mutate(id);
              }}
            />
          ))}
        </div>
      )}

      {/* Modals */}
      {trackingMilestone && (
        <MilestoneProgressTracker
          milestone={trackingMilestone}
          onClose={() => setTrackingMilestone(null)}
        />
      )}
      {acceleratingMilestone && (
        <AccelerationCalculator
          milestone={acceleratingMilestone}
          onClose={() => setAcceleratingMilestone(null)}
        />
      )}

      {addModalOpen && companyId && (
        <AddMilestoneToGrantModal
          grantId={grantId}
          companyId={companyId}
          onClose={() => setAddModalOpen(false)}
        />
      )}
    </div>
  );
}
