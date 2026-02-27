import { useState } from 'react';
import { X, TrendingUp } from 'lucide-react';
import type { GrantMilestone, RecordMilestoneProgressRequest } from '@/types';
import { ProgressDataSource } from '@/types';
import { useRecordMilestoneProgress } from '@/hooks';

interface MilestoneProgressTrackerProps {
  milestone: GrantMilestone;
  onClose: () => void;
  onSuccess?: () => void;
}

export function MilestoneProgressTracker({
  milestone,
  onClose,
  onSuccess,
}: MilestoneProgressTrackerProps) {
  const today = new Date().toISOString().substring(0, 10);
  const [form, setForm] = useState<RecordMilestoneProgressRequest>({
    recordedDate: today,
    recordedValue: milestone.currentValue ?? 0,
    notes: '',
    dataSource: ProgressDataSource.Manual,
  });

  const recordProgress = useRecordMilestoneProgress();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await recordProgress.mutateAsync({ id: milestone.id, data: form });
      onSuccess?.();
      onClose();
    } catch {}
  };

  const projectedProgress = milestone.targetValue > 0
    ? Math.min((form.recordedValue / milestone.targetValue) * 100, 100)
    : 0;

  return (
    <div className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
        {/* Header */}
        <div className="flex items-center justify-between p-5 border-b border-gray-100">
          <div>
            <h2 className="text-lg font-semibold text-gray-900">Registrar Progresso</h2>
            <p className="text-sm text-gray-500 mt-0.5 truncate max-w-xs">{milestone.name}</p>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg hover:bg-gray-100 text-gray-400"
          >
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-5 space-y-4">
          {/* Target info */}
          <div className="bg-indigo-50 rounded-lg p-3 text-sm text-indigo-700">
            Meta: <strong>{milestone.targetValue.toLocaleString('pt-BR')}</strong>
            {milestone.targetUnit ? ` ${milestone.targetUnit}` : ''}
            {' '}({milestone.targetOperator === 'GreaterThanOrEqual' ? '≥' :
              milestone.targetOperator === 'GreaterThan' ? '>' :
              milestone.targetOperator === 'LessThanOrEqual' ? '≤' :
              milestone.targetOperator === 'LessThan' ? '<' : '='})
          </div>

          {/* Recorded Value */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Valor atual
            </label>
            <input
              type="number"
              step="any"
              required
              value={form.recordedValue}
              onChange={(e) =>
                setForm((f) => ({ ...f, recordedValue: parseFloat(e.target.value) || 0 }))
              }
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
            />
            {/* Mini progress preview */}
            <div className="mt-2">
              <div className="flex justify-between text-xs text-gray-500 mb-1">
                <span>Progresso projetado</span>
                <span className="font-semibold text-indigo-600">{projectedProgress.toFixed(1)}%</span>
              </div>
              <div className="w-full bg-gray-100 rounded-full h-1.5">
                <div
                  className="h-1.5 rounded-full bg-indigo-500 transition-all"
                  style={{ width: `${projectedProgress}%` }}
                />
              </div>
            </div>
          </div>

          {/* Date */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Data de medição
            </label>
            <input
              type="date"
              required
              value={form.recordedDate}
              max={today}
              onChange={(e) => setForm((f) => ({ ...f, recordedDate: e.target.value }))}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
            />
          </div>

          {/* Data Source */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Fonte dos dados
            </label>
            <select
              value={form.dataSource}
              onChange={(e) =>
                setForm((f) => ({ ...f, dataSource: e.target.value as ProgressDataSource }))
              }
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
            >
              <option value={ProgressDataSource.Manual}>Manual</option>
              <option value={ProgressDataSource.Automatic}>Automático</option>
              <option value={ProgressDataSource.Audited}>Auditado</option>
            </select>
          </div>

          {/* Notes */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Observações
            </label>
            <textarea
              rows={2}
              value={form.notes}
              onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
              placeholder="Opcional..."
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm resize-none focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
            />
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-4 py-2 text-sm border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={recordProgress.isPending}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-2 text-sm bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-60"
            >
              <TrendingUp size={15} />
              {recordProgress.isPending ? 'Salvando...' : 'Salvar progresso'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
