interface VestingProgressBarProps {
  vestedPercentage: number;
  exercisedShares?: number;
  vestedShares?: number;
  totalShares?: number;
  showLabels?: boolean;
  size?: 'sm' | 'md' | 'lg';
}

const sizeMap = {
  sm: 'h-1.5',
  md: 'h-2.5',
  lg: 'h-4',
};

export function VestingProgressBar({
  vestedPercentage,
  exercisedShares = 0,
  vestedShares = 0,
  totalShares = 0,
  showLabels = true,
  size = 'md',
}: VestingProgressBarProps) {
  const exercisedPct = totalShares > 0 ? (exercisedShares / totalShares) * 100 : 0;
  const availablePct = Math.max(0, vestedPercentage - exercisedPct);

  const barH = sizeMap[size];

  return (
    <div className="w-full">
      {showLabels && (
        <div className="flex justify-between text-xs text-gray-500 mb-1.5">
          <span>
            {vestedShares.toLocaleString('pt-BR')} /{' '}
            {totalShares.toLocaleString('pt-BR')} ações vestidas
          </span>
          <span className="font-medium text-indigo-600">
            {vestedPercentage.toFixed(1)}%
          </span>
        </div>
      )}

      {/* Track */}
      <div className={`relative w-full ${barH} bg-gray-100 rounded-full overflow-hidden`}>
        {/* Exercised */}
        <div
          className="absolute top-0 left-0 h-full bg-indigo-600 rounded-full transition-all duration-500"
          style={{ width: `${Math.min(exercisedPct, 100)}%` }}
        />
        {/* Available to exercise */}
        <div
          className="absolute top-0 h-full bg-indigo-200 rounded-full transition-all duration-500"
          style={{
            left: `${Math.min(exercisedPct, 100)}%`,
            width: `${Math.min(availablePct, 100 - exercisedPct)}%`,
          }}
        />
      </div>

      {showLabels && (exercisedShares > 0 || vestedShares > 0) && (
        <div className="flex gap-4 mt-2 text-xs">
          {exercisedShares > 0 && (
            <span className="flex items-center gap-1 text-gray-500">
              <span className="inline-block w-2 h-2 rounded-full bg-indigo-600" />
              Exercidas: {exercisedShares.toLocaleString('pt-BR')}
            </span>
          )}
          {vestedShares - exercisedShares > 0 && (
            <span className="flex items-center gap-1 text-gray-500">
              <span className="inline-block w-2 h-2 rounded-full bg-indigo-200" />
              Disponíveis: {(vestedShares - exercisedShares).toLocaleString('pt-BR')}
            </span>
          )}
        </div>
      )}
    </div>
  );
}
