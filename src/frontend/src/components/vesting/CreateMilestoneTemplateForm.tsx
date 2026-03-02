import { useState } from 'react';
import { X, Save } from 'lucide-react';
import type { MilestoneTemplate, CreateMilestoneTemplateRequest } from '@/types';
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
import { useCreateMilestoneTemplate, useUpdateMilestoneTemplate } from '@/hooks';

interface CreateMilestoneTemplateFormProps {
  companyId: string;
  template?: MilestoneTemplate;
  onClose: () => void;
  onSuccess?: () => void;
}

const DEFAULT_FORM: CreateMilestoneTemplateRequest = {
  name: '',
  description: '',
  category: MilestoneCategory.Financial,
  metricType: MetricType.Revenue,
  targetOperator: TargetOperator.GreaterThanOrEqual,
  targetValue: 0,
  targetUnit: '',
  measurementFrequency: MeasurementFrequency.Quarterly,
  accelerationType: VestingAccelerationType.Percentage,
  accelerationAmount: 10,
  maxAccelerationCap: undefined,
};

export function CreateMilestoneTemplateForm({
  companyId,
  template,
  onClose,
  onSuccess,
}: CreateMilestoneTemplateFormProps) {
  const isEdit = !!template;
  const [form, setForm] = useState<CreateMilestoneTemplateRequest>(
    template
      ? {
          name: template.name,
          description: template.description,
          category: template.category,
          metricType: template.metricType,
          targetOperator: template.targetOperator,
          targetValue: template.targetValue ?? 0,
          targetUnit: template.targetUnit ?? '',
          measurementFrequency: template.measurementFrequency,
          accelerationType: template.accelerationType,
          accelerationAmount: template.accelerationAmount,
          maxAccelerationCap: template.maxAccelerationCap,
        }
      : DEFAULT_FORM
  );

  const createMutation = useCreateMilestoneTemplate(companyId);
  const updateMutation = useUpdateMilestoneTemplate(template?.id ?? '');

  const isPending = createMutation.isPending || updateMutation.isPending;

  const set = <K extends keyof CreateMilestoneTemplateRequest>(
    key: K,
    value: CreateMilestoneTemplateRequest[K]
  ) => setForm((f) => ({ ...f, [key]: value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (isEdit) {
        await updateMutation.mutateAsync(form);
      } else {
        await createMutation.mutateAsync(form);
      }
      onSuccess?.();
      onClose();
    } catch {}
  };

  const accelerationUnit =
    form.accelerationType === VestingAccelerationType.Percentage
      ? '%'
      : form.accelerationType === VestingAccelerationType.Months
      ? 'meses'
      : 'ações';

  return (
    <div className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4 overflow-y-auto">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-xl my-4">
        {/* Header */}
        <div className="flex items-center justify-between p-5 border-b border-gray-100">
          <h2 className="text-lg font-semibold text-gray-900">
            {isEdit ? 'Editar' : 'Novo'} Template de Milestone
          </h2>
          <button onClick={onClose} className="p-2 rounded-lg hover:bg-gray-100 text-gray-400">
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-5 space-y-4">
          {/* Name */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Nome *</label>
            <input
              required
              value={form.name}
              onChange={(e) => set('name', e.target.value)}
              placeholder="Ex: Meta de Receita Trimestral"
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
            />
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Descrição</label>
            <textarea
              rows={2}
              value={form.description ?? ''}
              onChange={(e) => set('description', e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm resize-none focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
            />
          </div>

          {/* Category + Metric */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Categoria *</label>
              <select
                value={form.category}
                onChange={(e) => set('category', e.target.value as MilestoneCategory)}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
              >
                {Object.values(MilestoneCategory).map((v) => (
                  <option key={v} value={v}>{milestoneCategoryLabels[v]}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Métrica *</label>
              <select
                value={form.metricType}
                onChange={(e) => set('metricType', e.target.value as MetricType)}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
              >
                {Object.values(MetricType).map((v) => (
                  <option key={v} value={v}>{metricTypeLabels[v]}</option>
                ))}
              </select>
            </div>
          </div>

          {/* Target Value + Operator + Unit */}
          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Operador</label>
              <select
                value={form.targetOperator}
                onChange={(e) => set('targetOperator', e.target.value as TargetOperator)}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
              >
                {Object.values(TargetOperator).map((v) => (
                  <option key={v} value={v}>{targetOperatorLabels[v]}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Valor meta *</label>
              <input
                type="number"
                step="any"
                required
                value={form.targetValue}
                onChange={(e) => set('targetValue', parseFloat(e.target.value) || 0)}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Unidade</label>
              <input
                value={form.targetUnit ?? ''}
                onChange={(e) => set('targetUnit', e.target.value)}
                placeholder="Ex: R$, usuários"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
              />
            </div>
          </div>

          {/* Frequency */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Frequência de medição</label>
            <select
              value={form.measurementFrequency}
              onChange={(e) => set('measurementFrequency', e.target.value as MeasurementFrequency)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
            >
              {Object.values(MeasurementFrequency).map((v) => (
                <option key={v} value={v}>{measurementFrequencyLabels[v]}</option>
              ))}
            </select>
          </div>

          {/* Acceleration */}
          <div className="border border-indigo-100 rounded-xl p-4 bg-indigo-50 space-y-3">
            <p className="text-sm font-semibold text-indigo-800">Aceleração de Vesting</p>
            <div className="grid grid-cols-3 gap-3">
              <div className="col-span-2">
                <label className="block text-xs font-medium text-gray-700 mb-1">Tipo de aceleração</label>
                <select
                  value={form.accelerationType}
                  onChange={(e) => set('accelerationType', e.target.value as VestingAccelerationType)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-white focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
                >
                  {Object.values(VestingAccelerationType).map((v) => (
                    <option key={v} value={v}>{vestingAccelerationTypeLabels[v]}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Quantidade ({accelerationUnit})
                </label>
                <input
                  type="number"
                  step="any"
                  min={0}
                  required
                  value={form.accelerationAmount}
                  onChange={(e) => set('accelerationAmount', parseFloat(e.target.value) || 0)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-white focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
                />
              </div>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Teto máximo de aceleração (%) — deixe em branco para usar padrão de 75%
              </label>
              <input
                type="number"
                step="1"
                min={1}
                max={100}
                value={form.maxAccelerationCap ?? ''}
                onChange={(e) =>
                  set('maxAccelerationCap', e.target.value ? parseFloat(e.target.value) : undefined)
                }
                placeholder="75"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-white focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
              />
            </div>
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
              disabled={isPending}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-2 text-sm bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-60"
            >
              <Save size={15} />
              {isPending ? 'Salvando...' : isEdit ? 'Salvar alterações' : 'Criar template'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
