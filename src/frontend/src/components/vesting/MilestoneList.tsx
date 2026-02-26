import { Target, CheckCircle, XCircle, Clock } from 'lucide-react';
import type { VestingMilestone } from '@/types';
import { MilestoneStatus, milestoneStatusLabels, milestoneTypeLabels } from '@/types';

interface MilestoneListProps {
  milestones: VestingMilestone[];
  onAchieve?: (id: string) => void;
  onFail?: (id: string) => void;
  compact?: boolean;
}

const statusIcons: Record<MilestoneStatus, React.ReactNode> = {
  [MilestoneStatus.Pending]: <Clock size={16} className="text-amber-500" />,
  [MilestoneStatus.Achieved]: <CheckCircle size={16} className="text-green-500" />,
  [MilestoneStatus.Failed]: <XCircle size={16} className="text-red-500" />,
  [MilestoneStatus.Cancelled]: <XCircle size={16} className="text-gray-400" />,
};

const statusBg: Record<MilestoneStatus, string> = {
  [MilestoneStatus.Pending]: 'border-amber-200 bg-amber-50',
  [MilestoneStatus.Achieved]: 'border-green-200 bg-green-50',
  [MilestoneStatus.Failed]: 'border-red-200 bg-red-50',
  [MilestoneStatus.Cancelled]: 'border-gray-200 bg-gray-50',
};

export function MilestoneList({
  milestones,
  onAchieve,
  onFail,
  compact = false,
}: MilestoneListProps) {
  if (milestones.length === 0) {
    return (
      <div className="text-center py-8 text-gray-400 text-sm">
        <Target size={32} className="mx-auto mb-2 opacity-30" />
        <p>Nenhuma meta definida para este plano</p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {milestones.map((m) => (
        <div
          key={m.id}
          className={`rounded-xl border p-4 flex items-start gap-3 ${statusBg[m.status]}`}
        >
          <div className="mt-0.5 flex-shrink-0">{statusIcons[m.status]}</div>

          <div className="flex-1 min-w-0">
            <div className="flex items-center justify-between gap-2">
              <p className="font-medium text-gray-800 text-sm truncate">{m.name}</p>
              <span className="text-xs text-gray-500 flex-shrink-0">
                {milestoneStatusLabels[m.status]}
              </span>
            </div>

            {!compact && m.description && (
              <p className="text-xs text-gray-500 mt-0.5 line-clamp-2">{m.description}</p>
            )}

            <div className="flex flex-wrap gap-3 mt-2 text-xs text-gray-500">
              <span>{milestoneTypeLabels[m.milestoneType]}</span>
              {m.targetValue !== undefined && (
                <span>
                  Meta: {m.targetValue.toLocaleString('pt-BR')} {m.targetUnit}
                </span>
              )}
              {m.accelerationPercentage > 0 && (
                <span className="text-indigo-600">+{m.accelerationPercentage}% aceleração</span>
              )}
              {m.targetDate && (
                <span>
                  Prazo:{' '}
                  {new Date(m.targetDate).toLocaleDateString('pt-BR', {
                    month: 'short',
                    year: 'numeric',
                  })}
                </span>
              )}
            </div>
          </div>

          {/* Actions */}
          {m.status === MilestoneStatus.Pending && (onAchieve || onFail) && (
            <div className="flex flex-col gap-1.5 flex-shrink-0">
              {onAchieve && (
                <button
                  className="text-xs bg-green-600 text-white px-2.5 py-1 rounded-md hover:bg-green-700 transition-colors"
                  onClick={() => onAchieve(m.id)}
                >
                  Atingido
                </button>
              )}
              {onFail && (
                <button
                  className="text-xs bg-red-100 text-red-700 px-2.5 py-1 rounded-md hover:bg-red-200 transition-colors"
                  onClick={() => onFail(m.id)}
                >
                  Falhou
                </button>
              )}
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
