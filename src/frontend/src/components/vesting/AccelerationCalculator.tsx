import { X, Zap, Calendar, AlertTriangle, TrendingUp } from 'lucide-react';
import type { GrantMilestone } from '@/types';
import {
  useAccelerationPreview,
  useApplyAcceleration,
} from '@/hooks';

interface AccelerationCalculatorProps {
  milestone: GrantMilestone;
  onClose: () => void;
  onSuccess?: () => void;
}

export function AccelerationCalculator({
  milestone,
  onClose,
  onSuccess,
}: AccelerationCalculatorProps) {
  const { data: preview, isLoading, error } = useAccelerationPreview(milestone.id);
  const applyMutation = useApplyAcceleration();

  const handleApply = async () => {
    try {
      await applyMutation.mutateAsync(milestone.id);
      onSuccess?.();
      onClose();
    } catch {}
  };

  return (
    <div className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg">
        {/* Header */}
        <div className="flex items-center justify-between p-5 border-b border-gray-100">
          <div className="flex items-center gap-2">
            <Zap size={20} className="text-purple-600" />
            <div>
              <h2 className="text-lg font-semibold text-gray-900">Calculadora de Aceleração</h2>
              <p className="text-sm text-gray-500 mt-0.5 truncate max-w-sm">{milestone.name}</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg hover:bg-gray-100 text-gray-400"
          >
            <X size={18} />
          </button>
        </div>

        <div className="p-5">
          {isLoading && (
            <div className="flex items-center justify-center py-10 text-gray-400">
              Calculando impacto...
            </div>
          )}

          {error && (
            <div className="flex items-center gap-2 text-red-600 bg-red-50 rounded-lg p-3 text-sm">
              <AlertTriangle size={16} />
              Não foi possível calcular a aceleração. Verifique se o milestone está verificado.
            </div>
          )}

          {preview && (
            <>
              {/* Impact Cards */}
              <div className="grid grid-cols-2 gap-3 mb-5">
                <div className="bg-gray-50 rounded-xl p-4 text-center">
                  <p className="text-xs text-gray-500 mb-1">Data atual de término</p>
                  <p className="text-sm font-semibold text-gray-700">
                    {new Date(preview.currentEndDate).toLocaleDateString('pt-BR')}
                  </p>
                </div>
                <div className="bg-purple-50 rounded-xl p-4 text-center">
                  <p className="text-xs text-purple-600 mb-1">Nova data de término</p>
                  <p className="text-sm font-semibold text-purple-800">
                    {new Date(preview.projectedNewEndDate).toLocaleDateString('pt-BR')}
                  </p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3 mb-5">
                <div className="border border-indigo-200 rounded-xl p-4 text-center">
                  <div className="flex items-center justify-center gap-1 text-indigo-600 mb-1">
                    <Calendar size={14} />
                    <span className="text-xs">Meses adiantados</span>
                  </div>
                  <p className="text-2xl font-bold text-indigo-700">
                    {preview.monthsAccelerated.toFixed(1)}
                  </p>
                </div>
                <div className="border border-green-200 rounded-xl p-4 text-center">
                  <div className="flex items-center justify-center gap-1 text-green-600 mb-1">
                    <TrendingUp size={14} />
                    <span className="text-xs">Ações desbloqueadas</span>
                  </div>
                  <p className="text-2xl font-bold text-green-700">
                    {preview.sharesUnlocked.toLocaleString('pt-BR')}
                  </p>
                </div>
              </div>

              {/* Cap warning */}
              {preview.cappedByMaximum && (
                <div className="flex items-center gap-2 bg-amber-50 border border-amber-200 rounded-lg p-3 mb-4 text-sm text-amber-700">
                  <AlertTriangle size={16} className="flex-shrink-0" />
                  <span>
                    Aceleração limitada pelo teto máximo de {preview.effectiveCap}%.
                  </span>
                </div>
              )}

              {/* Summary */}
              <div className="bg-gray-50 rounded-lg p-3 text-sm text-gray-600 mb-5">
                Aceleração de <strong>{milestone.accelerationAmount}
                {milestone.accelerationType === 'Percentage' ? '%' :
                 milestone.accelerationType === 'Months' ? ' meses' :
                 ' ações'}</strong> aplicada ao grant — 
                teto efetivo: <strong>{preview.effectiveCap}%</strong>.
              </div>

              {/* Buttons */}
              <div className="flex gap-3">
                <button
                  type="button"
                  onClick={onClose}
                  className="flex-1 px-4 py-2 text-sm border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
                >
                  Cancelar
                </button>
                <button
                  type="button"
                  onClick={handleApply}
                  disabled={applyMutation.isPending}
                  className="flex-1 flex items-center justify-center gap-2 px-4 py-2 text-sm bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:opacity-60"
                >
                  <Zap size={15} />
                  {applyMutation.isPending ? 'Aplicando...' : 'Aplicar aceleração'}
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
