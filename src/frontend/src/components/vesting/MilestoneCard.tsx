import { useState } from 'react';
import {
  Target,
  CheckCircle2,
  XCircle,
  Clock,
  AlertCircle,
  TrendingUp,
  MoreVertical,
  Zap,
  Activity,
  type LucideIcon,
} from 'lucide-react';
import type { GrantMilestone } from '@/types';
import {
  MilestoneStatus,
  milestoneStatusLabels,
  milestoneCategoryLabels,
} from '@/types';

interface MilestoneCardProps {
  milestone: GrantMilestone;
  onRecordProgress?: (milestone: GrantMilestone) => void;
  onAchieve?: (milestone: GrantMilestone) => void;
  onVerify?: (id: string) => void;
  onFail?: (id: string) => void;
  onCancel?: (id: string) => void;
  onApplyAcceleration?: (milestone: GrantMilestone) => void;
  onDelete?: (id: string) => void;
  showActions?: boolean;
}

const statusConfig: Record<
  MilestoneStatus,
  { color: string; bg: string; icon: LucideIcon }
> = {
  [MilestoneStatus.Pending]: {
    color: 'text-gray-600',
    bg: 'bg-gray-100',
    icon: Clock,
  },
  [MilestoneStatus.InProgress]: {
    color: 'text-blue-700',
    bg: 'bg-blue-100',
    icon: Activity,
  },
  [MilestoneStatus.Achieved]: {
    color: 'text-green-700',
    bg: 'bg-green-100',
    icon: CheckCircle2,
  },
  [MilestoneStatus.Failed]: {
    color: 'text-red-700',
    bg: 'bg-red-100',
    icon: XCircle,
  },
  [MilestoneStatus.Cancelled]: {
    color: 'text-gray-500',
    bg: 'bg-gray-100',
    icon: AlertCircle,
  },
};

const progressBarColor: Record<MilestoneStatus, string> = {
  [MilestoneStatus.Pending]: 'bg-gray-400',
  [MilestoneStatus.InProgress]: 'bg-blue-500',
  [MilestoneStatus.Achieved]: 'bg-green-500',
  [MilestoneStatus.Failed]: 'bg-red-400',
  [MilestoneStatus.Cancelled]: 'bg-gray-300',
};

export function MilestoneCard({
  milestone,
  onRecordProgress,
  onAchieve,
  onVerify,
  onFail,
  onCancel,
  onApplyAcceleration,
  onDelete,
  showActions = true,
}: MilestoneCardProps) {
  const [menuOpen, setMenuOpen] = useState(false);
  const cfg = statusConfig[milestone.status];
  const StatusIcon = cfg.icon;

  const canRecord =
    milestone.status === MilestoneStatus.Pending ||
    milestone.status === MilestoneStatus.InProgress;
  const canAchieve = canRecord;
  const canVerify =
    milestone.status === MilestoneStatus.Achieved && !milestone.verifiedAt;
  const canAccelerate =
    milestone.status === MilestoneStatus.Achieved &&
    !!milestone.verifiedAt &&
    !milestone.accelerationApplied;

  return (
    <div className="bg-white rounded-xl border border-gray-200 p-4 hover:shadow-sm transition-shadow">
      {/* Header */}
      <div className="flex items-start justify-between gap-2 mb-3">
        <div className="flex items-center gap-2 min-w-0">
          <Target size={16} className="text-indigo-500 flex-shrink-0" />
          <div className="min-w-0">
            <p className="font-medium text-gray-900 truncate">{milestone.name}</p>
            <p className="text-xs text-gray-500">
              {milestoneCategoryLabels[milestone.category]}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2 flex-shrink-0">
          {milestone.accelerationApplied && (
            <span className="flex items-center gap-1 text-xs font-medium text-purple-700 bg-purple-100 px-2 py-0.5 rounded-full">
              <Zap size={10} />
              Acelerado
            </span>
          )}
          <span
            className={`flex items-center gap-1 text-xs font-medium px-2.5 py-1 rounded-full ${cfg.bg} ${cfg.color}`}
          >
            <StatusIcon size={12} />
            {milestoneStatusLabels[milestone.status]}
          </span>
          {showActions && (onRecordProgress || onAchieve || onVerify || onFail || onCancel || onDelete) && (
            <div className="relative">
              <button
                className="p-1 rounded hover:bg-gray-100 text-gray-400"
                onClick={() => setMenuOpen((v) => !v)}
              >
                <MoreVertical size={15} />
              </button>
              {menuOpen && (
                <div className="absolute right-0 top-7 bg-white border border-gray-200 rounded-lg shadow-lg z-20 min-w-[180px] py-1">
                  {canRecord && onRecordProgress && (
                    <button
                      className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50 text-gray-700"
                      onClick={() => { setMenuOpen(false); onRecordProgress(milestone); }}
                    >
                      Registrar progresso
                    </button>
                  )}
                  {canAchieve && onAchieve && (
                    <button
                      className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50 text-green-700"
                      onClick={() => { setMenuOpen(false); onAchieve(milestone); }}
                    >
                      Marcar como atingido
                    </button>
                  )}
                  {canVerify && onVerify && (
                    <button
                      className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50 text-blue-700"
                      onClick={() => { setMenuOpen(false); onVerify(milestone.id); }}
                    >
                      Verificar / Confirmar
                    </button>
                  )}
                  {canAccelerate && onApplyAcceleration && (
                    <button
                      className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50 text-purple-700"
                      onClick={() => { setMenuOpen(false); onApplyAcceleration(milestone); }}
                    >
                      Aplicar aceleração
                    </button>
                  )}
                  {milestone.status === MilestoneStatus.Pending && onFail && (
                    <button
                      className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50 text-red-600"
                      onClick={() => { setMenuOpen(false); onFail(milestone.id); }}
                    >
                      Marcar como falhou
                    </button>
                  )}
                  {canRecord && onCancel && (
                    <button
                      className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50 text-gray-500"
                      onClick={() => { setMenuOpen(false); onCancel(milestone.id); }}
                    >
                      Cancelar
                    </button>
                  )}
                  {onDelete && (
                    <>
                      <div className="border-t border-gray-100 my-1" />
                      <button
                        className="w-full text-left px-4 py-2 text-sm hover:bg-red-50 text-red-600"
                        onClick={() => { setMenuOpen(false); onDelete(milestone.id); }}
                      >
                        Remover
                      </button>
                    </>
                  )}
                </div>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Progress Bar */}
      <div className="mb-3">
        <div className="flex items-center justify-between mb-1">
          <span className="text-xs text-gray-500">Progresso</span>
          <span className="text-xs font-semibold text-gray-700">
            {milestone.progressPercentage.toFixed(0)}%
          </span>
        </div>
        <div className="w-full bg-gray-100 rounded-full h-2">
          <div
            className={`h-2 rounded-full transition-all ${progressBarColor[milestone.status]}`}
            style={{ width: `${Math.min(milestone.progressPercentage, 100)}%` }}
          />
        </div>
        {milestone.currentValue !== undefined && (
          <div className="flex items-center justify-between mt-1">
            <span className="text-xs text-gray-400">
              Atual: {milestone.currentValue.toLocaleString('pt-BR')}
              {milestone.targetUnit ? ` ${milestone.targetUnit}` : ''}
            </span>
            <span className="text-xs text-gray-400">
              Meta: {milestone.targetValue.toLocaleString('pt-BR')}
              {milestone.targetUnit ? ` ${milestone.targetUnit}` : ''}
            </span>
          </div>
        )}
      </div>

      {/* Footer: acceleration info */}
      <div className="flex items-center gap-3 text-xs text-gray-500">
        <div className="flex items-center gap-1">
          <TrendingUp size={12} />
          <span>
            Aceleração: <strong className="text-gray-700">{milestone.accelerationAmount}
            {milestone.accelerationType === 'Percentage' ? '%' :
             milestone.accelerationType === 'Months' ? ' meses' :
             ' ações'}
            </strong>
          </span>
        </div>
        {milestone.targetDate && (
          <div className="flex items-center gap-1 ml-auto">
            <Clock size={12} />
            <span>{new Date(milestone.targetDate).toLocaleDateString('pt-BR')}</span>
          </div>
        )}
      </div>
    </div>
  );
}
