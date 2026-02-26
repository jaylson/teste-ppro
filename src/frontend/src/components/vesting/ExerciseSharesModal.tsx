import { useState } from 'react';
import { X, AlertCircle } from 'lucide-react';
import type { VestingGrant, ExerciseSharesRequest } from '@/types';
import { VestingTransactionType } from '@/types';

interface ExerciseSharesModalProps {
  grant: VestingGrant;
  open: boolean;
  onClose: () => void;
  onConfirm: (data: ExerciseSharesRequest) => void;
  isLoading?: boolean;
}

export function ExerciseSharesModal({
  grant,
  open,
  onClose,
  onConfirm,
  isLoading = false,
}: ExerciseSharesModalProps) {
  const [shares, setShares] = useState<string>('');
  const [price, setPrice] = useState<string>(grant.sharePrice.toString());
  const [date, setDate] = useState<string>(new Date().toISOString().slice(0, 10));
  const [notes, setNotes] = useState<string>('');

  if (!open) return null;

  const sharesToExercise = parseFloat(shares) || 0;
  const priceAtExercise = parseFloat(price) || 0;
  const totalValue = sharesToExercise * priceAtExercise;
  const gain = totalValue - sharesToExercise * grant.sharePrice;
  const isValid =
    sharesToExercise > 0 &&
    sharesToExercise <= grant.availableToExercise &&
    priceAtExercise > 0 &&
    !!date;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!isValid) return;
    onConfirm({
      sharesToExercise,
      sharePriceAtExercise: priceAtExercise,
      exerciseDate: date,
      transactionType: VestingTransactionType.Exercise,
      notes: notes || undefined,
    });
  }

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      {/* Backdrop */}
      <div className="fixed inset-0 bg-black/40" onClick={onClose} />

      <div className="relative flex min-h-full items-center justify-center p-4">
        <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-md">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-100">
            <div>
              <h2 className="text-lg font-semibold text-gray-900">Exercer Ações</h2>
              <p className="text-sm text-gray-500 mt-0.5">{grant.vestingPlanName}</p>
            </div>
            <button
              className="p-2 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors"
              onClick={onClose}
            >
              <X size={18} />
            </button>
          </div>

          {/* Info strip */}
          <div className="bg-indigo-50 px-6 py-3 grid grid-cols-3 gap-2 text-center text-sm">
            <div>
              <p className="text-xs text-gray-500">Vestidas</p>
              <p className="font-semibold text-indigo-700">
                {grant.vestedShares.toLocaleString('pt-BR')}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Disponíveis</p>
              <p className="font-semibold text-green-700">
                {grant.availableToExercise.toLocaleString('pt-BR')}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Exercidas</p>
              <p className="font-semibold text-gray-700">
                {grant.exercisedShares.toLocaleString('pt-BR')}
              </p>
            </div>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="p-6 space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Quantidade de ações a exercer
              </label>
              <input
                type="number"
                min="1"
                max={grant.availableToExercise}
                step="1"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                placeholder={`Máx: ${grant.availableToExercise}`}
                value={shares}
                onChange={(e) => setShares(e.target.value)}
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Preço por ação no exercício (R$)
              </label>
              <input
                type="number"
                min="0.0001"
                step="0.0001"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                value={price}
                onChange={(e) => setPrice(e.target.value)}
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Data do exercício
              </label>
              <input
                type="date"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                value={date}
                onChange={(e) => setDate(e.target.value)}
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Observações (opcional)
              </label>
              <textarea
                rows={2}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 resize-none"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
              />
            </div>

            {/* Summary */}
            {sharesToExercise > 0 && priceAtExercise > 0 && (
              <div className="bg-gray-50 rounded-lg p-3 text-sm space-y-1.5">
                <div className="flex justify-between text-gray-600">
                  <span>Valor total do exercício</span>
                  <span className="font-medium">
                    {totalValue.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Ganho estimado</span>
                  <span className={`font-semibold ${gain >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                    {gain.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                  </span>
                </div>
              </div>
            )}

            {sharesToExercise > grant.availableToExercise && (
              <div className="flex items-center gap-2 text-red-600 text-sm bg-red-50 rounded-lg px-3 py-2">
                <AlertCircle size={15} />
                <span>Quantidade excede o disponível para exercício</span>
              </div>
            )}

            <div className="flex gap-3 pt-2">
              <button
                type="button"
                className="flex-1 px-4 py-2.5 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
                onClick={onClose}
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={!isValid || isLoading}
                className="flex-1 px-4 py-2.5 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {isLoading ? 'Processando...' : 'Confirmar Exercício'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
