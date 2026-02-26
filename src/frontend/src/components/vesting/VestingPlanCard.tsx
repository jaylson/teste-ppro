import { Clock, Users, CheckCircle, Archive, MoreVertical } from 'lucide-react';
import { useState } from 'react';
import type { VestingPlan } from '@/types';
import {
  VestingPlanStatus,
  vestingPlanStatusLabels,
  vestingTypeLabels,
} from '@/types';

interface VestingPlanCardProps {
  plan: VestingPlan;
  onActivate?: (id: string) => void;
  onDeactivate?: (id: string) => void;
  onArchive?: (id: string) => void;
  onEdit?: (plan: VestingPlan) => void;
  onDelete?: (id: string) => void;
  onClick?: (plan: VestingPlan) => void;
}

const statusColors: Record<VestingPlanStatus, string> = {
  [VestingPlanStatus.Draft]: 'bg-gray-100 text-gray-700',
  [VestingPlanStatus.Active]: 'bg-green-100 text-green-700',
  [VestingPlanStatus.Inactive]: 'bg-yellow-100 text-yellow-700',
  [VestingPlanStatus.Archived]: 'bg-red-100 text-red-700',
};

export function VestingPlanCard({
  plan,
  onActivate,
  onDeactivate,
  onArchive,
  onEdit,
  onDelete,
  onClick,
}: VestingPlanCardProps) {
  const [menuOpen, setMenuOpen] = useState(false);

  return (
    <div
      className="bg-white rounded-xl border border-gray-200 p-5 hover:shadow-md transition-shadow cursor-pointer relative"
      onClick={() => onClick?.(plan)}
    >
      {/* Header */}
      <div className="flex items-start justify-between gap-2 mb-3">
        <div className="flex-1 min-w-0">
          <h3 className="font-semibold text-gray-900 truncate">{plan.name}</h3>
          {plan.description && (
            <p className="text-sm text-gray-500 mt-0.5 line-clamp-2">{plan.description}</p>
          )}
        </div>
        <div className="flex items-center gap-2 flex-shrink-0">
          <span
            className={`text-xs font-medium px-2.5 py-1 rounded-full ${statusColors[plan.status]}`}
          >
            {vestingPlanStatusLabels[plan.status]}
          </span>
          {(onEdit || onDelete || onActivate || onDeactivate || onArchive) && (
            <div className="relative">
              <button
                className="p-1 rounded hover:bg-gray-100 text-gray-400"
                onClick={(e) => {
                  e.stopPropagation();
                  setMenuOpen((v) => !v);
                }}
              >
                <MoreVertical size={16} />
              </button>
              {menuOpen && (
                <div
                  className="absolute right-0 top-7 bg-white border border-gray-200 rounded-lg shadow-lg z-20 min-w-[160px] py-1"
                  onClick={(e) => e.stopPropagation()}
                >
                  {onEdit && (
                    <button
                      className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50 text-gray-700"
                      onClick={() => { setMenuOpen(false); onEdit(plan); }}
                    >
                      Editar
                    </button>
                  )}
                  {onActivate && plan.status === VestingPlanStatus.Draft && (
                    <button
                      className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50 text-green-700"
                      onClick={() => { setMenuOpen(false); onActivate(plan.id); }}
                    >
                      Ativar
                    </button>
                  )}
                  {onDeactivate && plan.status === VestingPlanStatus.Active && (
                    <button
                      className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50 text-yellow-700"
                      onClick={() => { setMenuOpen(false); onDeactivate(plan.id); }}
                    >
                      Desativar
                    </button>
                  )}
                  {onArchive && plan.status !== VestingPlanStatus.Archived && (
                    <button
                      className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50 text-gray-500"
                      onClick={() => { setMenuOpen(false); onArchive(plan.id); }}
                    >
                      Arquivar
                    </button>
                  )}
                  {onDelete && (
                    <button
                      className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50 text-red-600"
                      onClick={() => { setMenuOpen(false); onDelete(plan.id); }}
                    >
                      Excluir
                    </button>
                  )}
                </div>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Info grid */}
      <div className="grid grid-cols-2 gap-3 mt-4 text-sm">
        <div className="flex items-center gap-2 text-gray-600">
          <Clock size={14} className="text-indigo-400 flex-shrink-0" />
          <span>{plan.vestingMonths} meses</span>
        </div>
        <div className="flex items-center gap-2 text-gray-600">
          <Archive size={14} className="text-indigo-400 flex-shrink-0" />
          <span>Cliff: {plan.cliffMonths}m</span>
        </div>
        <div className="flex items-center gap-2 text-gray-600">
          <Users size={14} className="text-indigo-400 flex-shrink-0" />
          <span>{plan.activeGrantsCount} grants</span>
        </div>
        <div className="flex items-center gap-2 text-gray-600">
          <CheckCircle size={14} className="text-indigo-400 flex-shrink-0" />
          <span>{plan.totalEquityPercentage.toFixed(2)}% equity</span>
        </div>
      </div>

      {/* Type badge */}
      <div className="mt-3 pt-3 border-t border-gray-100">
        <span className="text-xs text-indigo-600 bg-indigo-50 px-2 py-1 rounded-md font-medium">
          {vestingTypeLabels[plan.vestingType]}
        </span>
      </div>
    </div>
  );
}
