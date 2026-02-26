import { useState, useEffect } from 'react';
import { X } from 'lucide-react';
import { Spinner } from '@/components/ui';
import { useShareholders } from '@/hooks/useShareholders';
import { useVestingPlansByCompany } from '@/hooks/useVestingPlans';
import type { CreateVestingGrantRequest } from '@/types';
import { VestingPlanStatus } from '@/types';

interface CreateGrantModalProps {
  open: boolean;
  companyId: string;
  // Se informado, o plano vem pré-selecionado
  preselectedPlanId?: string;
  onClose: () => void;
  onSubmit: (data: CreateVestingGrantRequest) => void;
  isLoading: boolean;
}

interface GrantFormData {
  vestingPlanId: string;
  shareholderId: string;
  grantDate: string;
  totalShares: number;
  sharePrice: number;
  equityPercentage: number;
  vestingStartDate: string;
  notes: string;
}

const today = new Date().toISOString().slice(0, 10);

export default function CreateGrantModal({
  open,
  companyId,
  preselectedPlanId,
  onClose,
  onSubmit,
  isLoading,
}: CreateGrantModalProps) {
  const [form, setForm] = useState<GrantFormData>({
    vestingPlanId: preselectedPlanId ?? '',
    shareholderId: '',
    grantDate: today,
    totalShares: 1000,
    sharePrice: 1,
    equityPercentage: 1,
    vestingStartDate: today,
    notes: '',
  });

  // Reset form when modal opens or preselected plan changes
  useEffect(() => {
    if (open) {
      setForm((f) => ({
        ...f,
        vestingPlanId: preselectedPlanId ?? '',
        shareholderId: '',
        grantDate: today,
        vestingStartDate: today,
        totalShares: 1000,
        sharePrice: 1,
        equityPercentage: 1,
        notes: '',
      }));
    }
  }, [open, preselectedPlanId]);

  const { data: shareholdersData, isLoading: loadingShareholders } = useShareholders({
    companyId,
    pageSize: 100,
  });

  const { data: plans, isLoading: loadingPlans } = useVestingPlansByCompany(
    companyId,
    VestingPlanStatus.Active
  );

  if (!open) return null;

  function set<K extends keyof GrantFormData>(key: K, value: GrantFormData[K]) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    onSubmit({
      vestingPlanId: form.vestingPlanId,
      shareholderId: form.shareholderId,
      grantDate: form.grantDate,
      totalShares: form.totalShares,
      sharePrice: form.sharePrice,
      equityPercentage: form.equityPercentage,
      vestingStartDate: form.vestingStartDate,
      notes: form.notes || undefined,
    });
  }

  const shareholders = shareholdersData?.items ?? [];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4 animate-fade-in">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="text-base font-semibold text-gray-900">Novo Grant de Vesting</h2>
          <button
            type="button"
            className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors"
            onClick={onClose}
          >
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4 max-h-[80vh] overflow-y-auto">
          {/* Plano */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Plano de Vesting *
            </label>
            {preselectedPlanId ? (
              <div className="w-full border border-gray-200 bg-gray-50 rounded-lg px-3 py-2 text-sm text-gray-600">
                {loadingPlans
                  ? 'Carregando...'
                  : plans?.find((p) => p.id === preselectedPlanId)?.name ?? preselectedPlanId}
              </div>
            ) : (
              <select
                required
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                value={form.vestingPlanId}
                onChange={(e) => set('vestingPlanId', e.target.value)}
              >
                <option value="">Selecione um plano</option>
                {loadingPlans ? (
                  <option disabled>Carregando planos...</option>
                ) : (
                  plans?.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name}
                    </option>
                  ))
                )}
              </select>
            )}
          </div>

          {/* Sócio / Beneficiário */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Beneficiário (Sócio) *
            </label>
            <select
              required
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
              value={form.shareholderId}
              onChange={(e) => set('shareholderId', e.target.value)}
            >
              <option value="">Selecione um sócio</option>
              {loadingShareholders ? (
                <option disabled>Carregando sócios...</option>
              ) : (
                shareholders.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.name}
                  </option>
                ))
              )}
            </select>
          </div>

          {/* Datas */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Data do Grant *
              </label>
              <input
                type="date"
                required
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                value={form.grantDate}
                onChange={(e) => set('grantDate', e.target.value)}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Início do Vesting *
              </label>
              <input
                type="date"
                required
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                value={form.vestingStartDate}
                onChange={(e) => set('vestingStartDate', e.target.value)}
              />
            </div>
          </div>

          {/* Ações e preço */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Total de ações *
              </label>
              <input
                type="number"
                min={1}
                step={1}
                required
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                value={form.totalShares}
                onChange={(e) => set('totalShares', parseInt(e.target.value) || 1)}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Preço por ação (R$) *
              </label>
              <input
                type="number"
                min={0}
                step={0.0001}
                required
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                value={form.sharePrice}
                onChange={(e) => set('sharePrice', parseFloat(e.target.value) || 0)}
              />
            </div>
          </div>

          {/* % equity */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              % de Equity *
            </label>
            <input
              type="number"
              min={0.0001}
              max={100}
              step={0.0001}
              required
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
              value={form.equityPercentage}
              onChange={(e) => set('equityPercentage', parseFloat(e.target.value) || 0.0001)}
            />
          </div>

          {/* Observações */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Observações
            </label>
            <textarea
              rows={2}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 resize-none"
              placeholder="Observações opcionais..."
              value={form.notes}
              onChange={(e) => set('notes', e.target.value)}
            />
          </div>

          {/* Ações */}
          <div className="flex gap-3 pt-1">
            <button
              type="button"
              className="flex-1 px-4 py-2.5 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50"
              onClick={onClose}
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isLoading}
              className="flex-1 px-4 py-2.5 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 disabled:opacity-50 transition-colors flex items-center justify-center gap-2"
            >
              {isLoading ? (
                <>
                  <Spinner className="w-4 h-4" />
                  Criando...
                </>
              ) : (
                'Criar Grant'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
