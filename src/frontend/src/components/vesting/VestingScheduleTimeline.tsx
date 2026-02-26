import { CheckCircle, Circle, Clock } from 'lucide-react';
import type { VestingProjectionPoint } from '@/types';

interface VestingScheduleTimelineProps {
  points: VestingProjectionPoint[];
  cliffDate?: string;
  vestingStartDate: string;
  vestingEndDate: string;
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('pt-BR', { month: 'short', year: 'numeric' });
}

export function VestingScheduleTimeline({
  points,
  cliffDate,
  vestingStartDate,
  vestingEndDate,
}: VestingScheduleTimelineProps) {
  // Sample at most 12 evenly-spaced points for display
  const step = Math.max(1, Math.floor(points.length / 12));
  const displayed = points.filter((_, i) => i % step === 0 || i === points.length - 1);
  const today = new Date();

  return (
    <div className="space-y-1">
      {/* Header info */}
      <div className="flex justify-between text-xs text-gray-500 mb-3">
        <span>Início: {formatDate(vestingStartDate)}</span>
        {cliffDate && <span className="text-amber-600 font-medium">Cliff: {formatDate(cliffDate)}</span>}
        <span>Fim: {formatDate(vestingEndDate)}</span>
      </div>

      {/* Timeline scroll */}
      <div className="overflow-x-auto pb-2">
        <div className="flex items-end gap-1 min-w-max">
          {displayed.map((pt, idx) => {
            const ptDate = new Date(pt.date);
            const isPast = ptDate <= today;
            const isCliff = cliffDate && pt.date.startsWith(cliffDate.slice(0, 7));

            return (
              <div key={idx} className="flex flex-col items-center gap-1" style={{ width: 48 }}>
                {/* Bar */}
                <div className="w-full flex flex-col items-center justify-end" style={{ height: 80 }}>
                  <div
                    className={`w-6 rounded-t transition-all ${isPast ? 'bg-indigo-500' : 'bg-gray-200'} ${isCliff ? 'ring-2 ring-amber-400' : ''}`}
                    style={{ height: `${Math.max(6, pt.vestedPercentage * 0.8)}%`, minHeight: 6 }}
                  />
                </div>
                {/* Icon */}
                <div className="text-gray-400">
                  {isPast ? (
                    <CheckCircle size={12} className="text-indigo-500" />
                  ) : idx === 0 ? (
                    <Clock size={12} className="text-gray-300" />
                  ) : (
                    <Circle size={12} className="text-gray-200" />
                  )}
                </div>
                {/* Label */}
                <span className="text-[10px] text-gray-400 text-center leading-tight whitespace-nowrap">
                  {formatDate(pt.date)}
                </span>
                <span className="text-[10px] font-medium text-indigo-600">
                  {pt.vestedPercentage.toFixed(0)}%
                </span>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
