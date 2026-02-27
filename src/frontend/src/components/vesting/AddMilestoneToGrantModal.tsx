import { useState } from 'react';
import { X, ChevronRight, CheckCircle2, AlertCircle, PlusCircle } from 'lucide-react';
import type { MilestoneTemplate, CreateGrantMilestoneRequest } from '@/types';
import {
  MilestoneCategory,
  MetricType,
  TargetOperator,
  MeasurementFrequency,
  VestingAccelerationType,
  milestoneCategoryLabels,
  metricTypeLabels,
  targetOperatorLabels,
  measurementFrequencyLabels,
  vestingAccelerationTypeLabels,
} from '@/types';
import { useMilestoneTemplatesByCompany, useCreateGrantMilestone } from '@/hooks';

interface AddMilestoneToGrantModalProps {
  grantId: string;
  companyId: string;
  onClose: () => void;
}

type Step = 'select' | 'configure';

const BLANK: Omit<CreateGrantMilestoneRequest, 'vestingGrantId'> = {
  name: '',
  description: '',
  category: MilestoneCategory.Financial,
  metricType: MetricType.Revenue,
  targetOperator: TargetOperator.GreaterThanOrEqual,
  targetValue: 0,
  targetUnit: '',
  measurementFrequency: MeasurementFrequency.OneTime,
  accelerationType: VestingAccelerationType.Percentage,
  accelerationAmount: 10,
  targetDate: '',
  notes: '',
};

export function AddMilestoneToGrantModal({
  grantId,
  companyId,
  onClose,
}: AddMilestoneToGrantModalProps) {
  const [step, setStep] = useState<Step>('select');
  const [selectedTemplate, setSelectedTemplate] = useState<MilestoneTemplate | null>(null);
  const [form, setForm] = useState(BLANK);

  const { data: templates, isLoading } = useMilestoneTemplatesByCompany(companyId, true);
  const createMutation = useCreateGrantMilestone(companyId);

  function applyTemplate(tpl: MilestoneTemplate) {
    setSelectedTemplate(tpl);
    setForm({
      milestoneTemplateId: tpl.id,
      name: tpl.name,
      description: tpl.description ?? '',
      category: tpl.category,
      metricType: tpl.metricType,
      targetOperator: tpl.targetOperator,
      targetValue: tpl.targetValue,
      targetUnit: tpl.targetUnit ?? '',
      measurementFrequency: tpl.measurementFrequency,
      accelerationType: tpl.accelerationType,
      accelerationAmount: tpl.accelerationAmount,
      maxAccelerationCap: tpl.maxAccelerationCap,
      targetDate: '',
      notes: '',
    });
    setStep('configure');
  }

  function startBlank() {
    setSelectedTemplate(null);
    setForm(BLANK);
    setStep('configure');
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const payload: CreateGrantMilestoneRequest = {
      vestingGrantId: grantId,
      ...form,
      targetDate: form.targetDate || undefined,
      notes: form.notes || undefined,
      description: form.description || undefined,
      targetUnit: form.targetUnit || undefined,
    };
    createMutation.mutate(payload, { onSuccess: onClose });
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} />
      <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-2xl max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-5 border-b border-gray-100">
          <div>
            <h2 className="text-lg font-semibold text-gray-900">Adicionar Milestone ao Grant</h2>
            <p className="text-xs text-gray-400 mt-0.5">
              {step === 'select'
                ? 'Selecione um template ou crie do zero'
                : selectedTemplate
                  ? `Template: ${selectedTemplate.name}`
                  : 'Milestone personalizado'}
            </p>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg hover:bg-gray-100 text-gray-400"
          >
            <X size={18} />
          </button>
        </div>

        {/* Body */}
        <div className="overflow-y-auto flex-1 p-5">
          {/* ── Step 1: Select template ── */}
          {step === 'select' && (
            <div className="space-y-3">
              {/* Blank option */}
              <button
                onClick={startBlank}
                className="w-full text-left flex items-center gap-3 px-4 py-3 border-2 border-dashed border-indigo-300 rounded-xl hover:border-indigo-500 hover:bg-indigo-50 transition-colors"
              >
                <PlusCircle size={20} className="text-indigo-500 shrink-0" />
                <div>
                  <p className="font-medium text-indigo-700 text-sm">Criar milestone do zero</p>
                  <p className="text-xs text-gray-400">Defina todos os critérios manualmente</p>
                </div>
                <ChevronRight size={16} className="ml-auto text-indigo-400" />
              </button>

              {/* Templates */}
              {isLoading && (
                <p className="text-sm text-gray-400 text-center py-4">Carregando templates...</p>
              )}

              {!isLoading && (!templates || templates.length === 0) && (
                <div className="flex flex-col items-center gap-2 py-8 text-gray-400">
                  <AlertCircle size={24} className="opacity-50" />
                  <p className="text-sm">Nenhum template ativo disponível para esta empresa.</p>
                  <p className="text-xs">
                    Crie templates em{' '}
                    <span className="font-medium text-indigo-600">
                      Administração → Templates de Milestone
                    </span>
                    .
                  </p>
                </div>
              )}

              {templates && templates.length > 0 && (
                <>
                  <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider mt-4 mb-2">
                    Templates disponíveis ({templates.length})
                  </p>
                  <div className="space-y-2">
                    {templates.map((tpl) => (
                      <button
                        key={tpl.id}
                        onClick={() => applyTemplate(tpl)}
                        className="w-full text-left flex items-start gap-3 px-4 py-3 border border-gray-200 rounded-xl hover:border-indigo-400 hover:bg-indigo-50/50 transition-colors"
                      >
                        <div className="flex-1 min-w-0">
                          <p className="font-medium text-gray-900 text-sm truncate">{tpl.name}</p>
                          {tpl.description && (
                            <p className="text-xs text-gray-400 mt-0.5 line-clamp-1">
                              {tpl.description}
                            </p>
                          )}
                          <div className="flex flex-wrap gap-2 mt-2">
                            <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
                              {milestoneCategoryLabels[tpl.category]}
                            </span>
                            <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
                              {metricTypeLabels[tpl.metricType]}
                            </span>
                            <span className="text-xs bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded-full font-medium">
                              {targetOperatorLabels[tpl.targetOperator]}{' '}
                              {tpl.targetValue.toLocaleString('pt-BR')}
                              {tpl.targetUnit ? ` ${tpl.targetUnit}` : ''}
                            </span>
                            <span className="text-xs bg-purple-100 text-purple-700 px-2 py-0.5 rounded-full font-medium">
                              +{tpl.accelerationAmount}
                              {tpl.accelerationType === VestingAccelerationType.Percentage
                                ? '%'
                                : tpl.accelerationType === VestingAccelerationType.Months
                                  ? ' meses'
                                  : ' ações'}
                            </span>
                          </div>
                        </div>
                        <ChevronRight size={16} className="text-gray-300 shrink-0 mt-1" />
                      </button>
                    ))}
                  </div>
                </>
              )}
            </div>
          )}

          {/* ── Step 2: Configure ── */}
          {step === 'configure' && (
            <form id="milestone-form" onSubmit={handleSubmit} className="space-y-4">
              {selectedTemplate && (
                <div className="flex items-center gap-2 text-xs text-indigo-700 bg-indigo-50 border border-indigo-200 rounded-lg px-3 py-2">
                  <CheckCircle2 size={14} />
                  <span>
                    Preenchido a partir do template <strong>{selectedTemplate.name}</strong>. Ajuste
                    os campos conforme necessário.
                  </span>
                </div>
              )}

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {/* Name */}
                <div className="sm:col-span-2">
                  <label className="block text-xs font-medium text-gray-700 mb-1">
                    Nome do Milestone <span className="text-red-500">*</span>
                  </label>
                  <input
                    required
                    type="text"
                    className="input w-full"
                    value={form.name}
                    onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                  />
                </div>

                {/* Description */}
                <div className="sm:col-span-2">
                  <label className="block text-xs font-medium text-gray-700 mb-1">Descrição</label>
                  <textarea
                    rows={2}
                    className="input w-full resize-none"
                    value={form.description}
                    onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                  />
                </div>

                {/* Category */}
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Categoria</label>
                  <select
                    className="input w-full"
                    value={form.category}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, category: e.target.value as MilestoneCategory }))
                    }
                  >
                    {Object.values(MilestoneCategory).map((c) => (
                      <option key={c} value={c}>
                        {milestoneCategoryLabels[c]}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Metric Type */}
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Métrica</label>
                  <select
                    className="input w-full"
                    value={form.metricType}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, metricType: e.target.value as MetricType }))
                    }
                  >
                    {Object.values(MetricType).map((m) => (
                      <option key={m} value={m}>
                        {metricTypeLabels[m]}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Target Operator + Value */}
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">
                    Operador de Meta <span className="text-red-500">*</span>
                  </label>
                  <select
                    className="input w-full"
                    value={form.targetOperator}
                    onChange={(e) =>
                      setForm((f) => ({
                        ...f,
                        targetOperator: e.target.value as TargetOperator,
                      }))
                    }
                  >
                    {Object.values(TargetOperator).map((op) => (
                      <option key={op} value={op}>
                        {targetOperatorLabels[op]}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">
                    Valor Alvo <span className="text-red-500">*</span>
                  </label>
                  <input
                    required
                    type="number"
                    min={0}
                    step="any"
                    className="input w-full"
                    value={form.targetValue}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, targetValue: parseFloat(e.target.value) || 0 }))
                    }
                  />
                </div>

                {/* Target Unit */}
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">
                    Unidade (ex: R$, %, usuários)
                  </label>
                  <input
                    type="text"
                    className="input w-full"
                    value={form.targetUnit}
                    onChange={(e) => setForm((f) => ({ ...f, targetUnit: e.target.value }))}
                  />
                </div>

                {/* Frequency */}
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">
                    Frequência de Medição
                  </label>
                  <select
                    className="input w-full"
                    value={form.measurementFrequency}
                    onChange={(e) =>
                      setForm((f) => ({
                        ...f,
                        measurementFrequency: e.target.value as MeasurementFrequency,
                      }))
                    }
                  >
                    {Object.values(MeasurementFrequency).map((mf) => (
                      <option key={mf} value={mf}>
                        {measurementFrequencyLabels[mf]}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Acceleration Type + Amount */}
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">
                    Tipo de Aceleração
                  </label>
                  <select
                    className="input w-full"
                    value={form.accelerationType}
                    onChange={(e) =>
                      setForm((f) => ({
                        ...f,
                        accelerationType: e.target.value as VestingAccelerationType,
                      }))
                    }
                  >
                    {Object.values(VestingAccelerationType).map((at) => (
                      <option key={at} value={at}>
                        {vestingAccelerationTypeLabels[at]}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">
                    Valor de Aceleração <span className="text-red-500">*</span>
                  </label>
                  <input
                    required
                    type="number"
                    min={0}
                    step="any"
                    className="input w-full"
                    value={form.accelerationAmount}
                    onChange={(e) =>
                      setForm((f) => ({
                        ...f,
                        accelerationAmount: parseFloat(e.target.value) || 0,
                      }))
                    }
                  />
                </div>

                {/* Target Date */}
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">
                    Data Limite (opcional)
                  </label>
                  <input
                    type="date"
                    className="input w-full"
                    value={form.targetDate}
                    onChange={(e) => setForm((f) => ({ ...f, targetDate: e.target.value }))}
                  />
                </div>

                {/* Notes */}
                <div className="sm:col-span-2">
                  <label className="block text-xs font-medium text-gray-700 mb-1">
                    Observações
                  </label>
                  <textarea
                    rows={2}
                    className="input w-full resize-none"
                    value={form.notes}
                    onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
                  />
                </div>
              </div>
            </form>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between gap-3 p-5 border-t border-gray-100">
          {step === 'configure' ? (
            <button
              type="button"
              onClick={() => setStep('select')}
              className="text-sm text-gray-500 hover:text-gray-700"
            >
              ← Voltar
            </button>
          ) : (
            <div />
          )}
          <div className="flex gap-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm text-gray-600 border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              Cancelar
            </button>
            {step === 'configure' && (
              <button
                form="milestone-form"
                type="submit"
                disabled={createMutation.isPending}
                className="px-5 py-2 text-sm font-medium bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50"
              >
                {createMutation.isPending ? 'Salvando...' : 'Adicionar Milestone'}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
