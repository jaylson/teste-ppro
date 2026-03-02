import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, TrendingUp } from 'lucide-react';
import { Button, Card } from '@/components/ui';
import { useCreateValuation } from '@/hooks';
import { useClientStore } from '@/stores/clientStore';
import { ValuationEventType, valuationEventTypeLabels } from '@/types';

interface FormData {
  valuationDate: string;
  eventType: string;
  eventName: string;
  totalShares: string;
  notes: string;
}

const defaultForm: FormData = {
  valuationDate: new Date().toISOString().split('T')[0],
  eventType: ValuationEventType.Seed,
  eventName: '',
  totalShares: '',
  notes: '',
};

export default function ValuationNewPage() {
  const navigate = useNavigate();
  const { selectedCompanyId } = useClientStore();
  const createValuation = useCreateValuation();
  const [form, setForm] = useState<FormData>(defaultForm);
  const [errors, setErrors] = useState<Partial<Record<keyof FormData, string>>>({});

  function set<K extends keyof FormData>(key: K, value: string) {
    setForm((prev) => ({ ...prev, [key]: value }));
    setErrors((prev) => ({ ...prev, [key]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof FormData, string>> = {};
    if (!form.valuationDate) e.valuationDate = 'Data obrigatória.';
    if (!form.eventType) e.eventType = 'Tipo de evento obrigatório.';
    const shares = parseFloat(form.totalShares);
    if (!form.totalShares || isNaN(shares) || shares <= 0)
      e.totalShares = 'Informe o total de ações (positivo).';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate() || !selectedCompanyId) return;
    createValuation.mutate(
      {
        companyId: selectedCompanyId,
        valuationDate: form.valuationDate,
        eventType: form.eventType,
        eventName: form.eventName || undefined,
        totalShares: parseFloat(form.totalShares),
        notes: form.notes || undefined,
      },
      {
        onSuccess: (v) => navigate(`/valuations/${v.id}`),
      }
    );
  }

  return (
    <div className="max-w-2xl space-y-6 animate-fade-in">
      {/* Header */}
      <div className="flex items-center gap-3">
        <button
          onClick={() => navigate('/valuations')}
          className="p-2 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors"
        >
          <ArrowLeft size={18} />
        </button>
        <div>
          <h1 className="text-xl font-bold text-gray-900">Novo Valuation</h1>
          <p className="text-sm text-gray-500">Registre um novo evento de avaliação</p>
        </div>
      </div>

      <Card>
        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          {/* Evento */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Tipo de Evento <span className="text-red-500">*</span>
              </label>
              <select
                value={form.eventType}
                onChange={(e) => set('eventType', e.target.value)}
                className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {Object.entries(valuationEventTypeLabels).map(([k, v]) => (
                  <option key={k} value={k}>{v}</option>
                ))}
              </select>
              {errors.eventType && <p className="text-xs text-red-500 mt-1">{errors.eventType}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Nome do Evento
              </label>
              <input
                type="text"
                value={form.eventName}
                onChange={(e) => set('eventName', e.target.value)}
                placeholder="Ex: Rodada Seed 2026"
                className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          {/* Data + Ações */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Data do Valuation <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                value={form.valuationDate}
                onChange={(e) => set('valuationDate', e.target.value)}
                className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              {errors.valuationDate && (
                <p className="text-xs text-red-500 mt-1">{errors.valuationDate}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Total de Ações <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                value={form.totalShares}
                onChange={(e) => set('totalShares', e.target.value)}
                placeholder="Ex: 10000000"
                min="1"
                className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              {errors.totalShares && (
                <p className="text-xs text-red-500 mt-1">{errors.totalShares}</p>
              )}
            </div>
          </div>

          {/* Observações */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Observações</label>
            <textarea
              rows={3}
              value={form.notes}
              onChange={(e) => set('notes', e.target.value)}
              placeholder="Contexto adicional sobre este evento de valuation..."
              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="flex gap-3 justify-end pt-2">
            <Button
              type="button"
              variant="secondary"
              onClick={() => navigate('/valuations')}
            >
              Cancelar
            </Button>
            <Button
              type="submit"
              icon={<TrendingUp size={14} />}
              loading={createValuation.isPending}
            >
              Criar Valuation
            </Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
