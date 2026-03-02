import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  ArrowLeft,
  CheckCircle,
  XCircle,
  Send,
  RotateCcw,
  Calculator,
  Star,
  Plus,
  ChevronUp,
} from 'lucide-react';
import { Button, Card, Spinner, Badge } from '@/components/ui';
import {
  useValuation,
  useSubmitValuation,
  useApproveValuation,
  useRejectValuation,
  useReturnValuationToDraft,
  useSelectValuationMethod,
  useAddValuationMethod,
  useCalculateMethod,
  useCustomFormulas,
} from '@/hooks';
import {
  valuationStatusLabels,
  valuationStatusColors,
  valuationMethodLabels,
  valuationEventTypeLabels,
  ValuationMethodType,
  type FormulaVariableDefinition,
} from '@/types';
import { formatCurrency, formatDate } from '@/utils/format';
import { useClientStore } from '@/stores/clientStore';

// ─── Inputs padrão por tipo de metodologia ───────────────────────────────────

const METHOD_INPUTS: Record<string, { key: string; label: string; type?: string }[]> = {
  [ValuationMethodType.ArrMultiple]: [
    { key: 'arr', label: 'ARR (R$)' },
    { key: 'multiple', label: 'Múltiplo' },
  ],
  [ValuationMethodType.MrrMultiple]: [
    { key: 'mrr', label: 'MRR (R$)' },
    { key: 'multiple', label: 'Múltiplo' },
  ],
  [ValuationMethodType.EbitdaMultiple]: [
    { key: 'ebitda', label: 'EBITDA (R$)' },
    { key: 'multiple', label: 'Múltiplo' },
  ],
  [ValuationMethodType.Dcf]: [
    { key: 'revenue', label: 'Receita Base (R$)' },
    { key: 'growth_rate', label: 'Taxa de Crescimento (%)' },
    { key: 'discount_rate', label: 'Taxa de Desconto (%)' },
    { key: 'years', label: 'Anos de Projeção' },
    { key: 'terminal_multiple', label: 'Múltiplo Terminal (opcional)' },
  ],
  [ValuationMethodType.Comparables]: [
    { key: 'avg_revenue', label: 'Receita Média dos Comparáveis (R$)' },
    { key: 'revenue_multiple', label: 'Múltiplo de Receita' },
  ],
  [ValuationMethodType.AssetBased]: [
    { key: 'total_assets', label: 'Total de Ativos (R$)' },
    { key: 'total_liabilities', label: 'Total de Passivos (R$)' },
  ],
  [ValuationMethodType.Berkus]: [
    { key: 'team', label: 'Equipe (R$)' },
    { key: 'idea', label: 'Ideia / Modelo de Negócio (R$)' },
    { key: 'prototype', label: 'Protótipo (R$)' },
    { key: 'strategic_relationships', label: 'Relacionamentos Estratégicos (R$)' },
    { key: 'production_rollout', label: 'Produção / Rollout (R$)' },
  ],
};

// ─── Painel de adição de metodologia ─────────────────────────────────────────

interface AddMethodPanelProps {
  valuationId: string;
  companyId: string;
  onClose: () => void;
}

function AddMethodPanel({ valuationId, companyId, onClose }: AddMethodPanelProps) {
  const [methodType, setMethodType] = useState<string>(ValuationMethodType.ArrMultiple);
  const [inputs, setInputs] = useState<Record<string, string>>({});
  const [selectedFormulaId, setSelectedFormulaId] = useState('');
  const [selectedVersionId, setSelectedVersionId] = useState('');
  const [dataSource, setDataSource] = useState('');
  const [notes, setNotes] = useState('');
  const [calcResult, setCalcResult] = useState<null | { value: number; breakdown: Record<string, unknown> }>(null);

  const addMethod = useAddValuationMethod();
  const calculateMethod = useCalculateMethod();
  const { data: formulasData } = useCustomFormulas({ companyId, isActive: true, pageSize: 100 });

  const formulas = formulasData?.items ?? [];
  const selectedFormula = formulas.find((f) => f.id === selectedFormulaId) ?? null;
  const isCustom = methodType === ValuationMethodType.Custom;

  // campos dinâmicos para fórmula customizada
  const customVars: FormulaVariableDefinition[] =
    isCustom && selectedFormula?.currentVersion
      ? selectedFormula.currentVersion.variables ?? []
      : [];

  useEffect(() => {
    setInputs({});
    setCalcResult(null);
    if (!isCustom) setSelectedFormulaId('');
  }, [methodType]);

  useEffect(() => {
    if (selectedFormulaId) {
      const f = formulas.find((f) => f.id === selectedFormulaId);
      setSelectedVersionId(f?.currentVersionId ?? '');
    }
  }, [selectedFormulaId]);

  function setInput(key: string, value: string) {
    setInputs((p) => ({ ...p, [key]: value }));
    setCalcResult(null);
  }

  function buildInputsRecord(): Record<string, number> {
    const rec: Record<string, number> = {};
    const fields = isCustom
      ? customVars.map((v) => v.name)
      : (METHOD_INPUTS[methodType] ?? []).map((f) => f.key);
    for (const key of fields) {
      const val = parseFloat(inputs[key] ?? '');
      if (!isNaN(val)) rec[key] = val;
    }
    return rec;
  }

  async function handleAddAndCalculate() {
    const numInputs = buildInputsRecord();
    // 1. Adicionar método
    addMethod.mutate(
      {
        valuationId,
        data: {
          methodType,
          dataSource: dataSource || undefined,
          notes: notes || undefined,
          formulaVersionId: isCustom && selectedVersionId ? selectedVersionId : undefined,
        },
      },
      {
        onSuccess: (method) => {
          // 2. Calcular se houver inputs
          if (Object.keys(numInputs).length > 0) {
            calculateMethod.mutate(
              {
                valuationId,
                methodId: method.id,
                data: {
                  methodType,
                  inputs: numInputs,
                  formulaVersionId:
                    isCustom && selectedVersionId ? selectedVersionId : undefined,
                },
              },
              {
                onSuccess: (res) => {
                  setCalcResult({ value: res.calculatedValue, breakdown: res.breakdown });
                },
              }
            );
          } else {
            onClose();
          }
        },
      }
    );
  }

  const standardFields = isCustom ? [] : (METHOD_INPUTS[methodType] ?? []);
  const isPending = addMethod.isPending || calculateMethod.isPending;

  return (
    <Card>
      <div className="p-5 space-y-4">
        <h3 className="text-sm font-semibold text-gray-900">Adicionar Metodologia</h3>

        {/* Tipo */}
        <div>
          <label className="block text-xs font-medium text-gray-600 mb-1">Tipo de Metodologia</label>
          <select
            value={methodType}
            onChange={(e) => setMethodType(e.target.value)}
            className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            {Object.entries(valuationMethodLabels).map(([k, v]) => (
              <option key={k} value={k}>{v}</option>
            ))}
          </select>
        </div>

        {/* Fórmula customizada */}
        {isCustom && (
          <div className="space-y-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Fórmula Customizada</label>
              <select
                value={selectedFormulaId}
                onChange={(e) => setSelectedFormulaId(e.target.value)}
                className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">— Selecione uma fórmula —</option>
                {formulas.map((f) => (
                  <option key={f.id} value={f.id}>
                    {f.name} {f.currentVersion ? `(v${f.currentVersion.versionNumber})` : ''}
                  </option>
                ))}
              </select>
              {formulas.length === 0 && (
                <p className="text-xs text-amber-600 mt-1">
                  Nenhuma fórmula customizada ativa. Crie uma em Configurações → Fórmulas.
                </p>
              )}
            </div>
            {selectedFormula?.currentVersion?.expression && (
              <div className="bg-gray-50 rounded-lg px-3 py-2">
                <p className="text-xs text-gray-500 font-mono">{selectedFormula.currentVersion.expression}</p>
              </div>
            )}
          </div>
        )}

        {/* Inputs padrão */}
        {!isCustom && standardFields.length > 0 && (
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-2">Parâmetros de Cálculo</label>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              {standardFields.map((f) => (
                <div key={f.key}>
                  <label className="block text-xs text-gray-500 mb-1">{f.label}</label>
                  <input
                    type="number"
                    step="any"
                    value={inputs[f.key] ?? ''}
                    onChange={(e) => setInput(f.key, e.target.value)}
                    className="w-full text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="0"
                  />
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Inputs dinâmicos de fórmula customizada */}
        {isCustom && customVars.length > 0 && (
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-2">Variáveis da Fórmula</label>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              {customVars
                .slice()
                .sort((a, b) => a.displayOrder - b.displayOrder)
                .map((v) => (
                  <div key={v.name}>
                    <label className="block text-xs text-gray-500 mb-1">
                      {v.label}
                      {v.unit ? ` (${v.unit})` : ''}
                      {v.isRequired && <span className="text-red-400 ml-0.5">*</span>}
                    </label>
                    <input
                      type="number"
                      step="any"
                      value={inputs[v.name] ?? ''}
                      onChange={(e) => setInput(v.name, e.target.value)}
                      placeholder={v.defaultValue !== undefined ? String(v.defaultValue) : '0'}
                      className="w-full text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                    {v.description && (
                      <p className="text-xs text-gray-400 mt-0.5">{v.description}</p>
                    )}
                  </div>
                ))}
            </div>
          </div>
        )}

        {/* Fonte de dados */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Fonte de Dados</label>
            <input
              type="text"
              value={dataSource}
              onChange={(e) => setDataSource(e.target.value)}
              placeholder="Ex: Relatório interno Q4 2025"
              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">Observações</label>
            <input
              type="text"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Comentário opcional"
              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        {/* Resultado do cálculo */}
        {calcResult && (
          <div className="bg-green-50 border border-green-200 rounded-lg px-4 py-3">
            <p className="text-xs font-medium text-green-700 mb-1">Resultado calculado</p>
            <p className="text-lg font-bold text-green-800">{formatCurrency(calcResult.value)}</p>
            {Object.keys(calcResult.breakdown).length > 0 && (
              <details className="mt-2">
                <summary className="text-xs text-green-600 cursor-pointer">Ver breakdown</summary>
                <pre className="text-xs mt-1 text-green-700 whitespace-pre-wrap">
                  {JSON.stringify(calcResult.breakdown, null, 2)}
                </pre>
              </details>
            )}
          </div>
        )}

        {/* Botões */}
        <div className="flex gap-2 justify-end">
          <Button variant="secondary" size="sm" onClick={onClose}>
            Cancelar
          </Button>
          <Button
            size="sm"
            icon={<Calculator size={14} />}
            loading={isPending}
            disabled={isCustom && !selectedFormulaId}
            onClick={handleAddAndCalculate}
          >
            {calcResult ? 'Fechar' : 'Adicionar e Calcular'}
          </Button>
          {calcResult && (
            <Button size="sm" onClick={onClose}>
              Concluir
            </Button>
          )}
        </div>
      </div>
    </Card>
  );
}

// ─── Painel de recálculo de método existente ──────────────────────────────────

interface RecalcPanelProps {
  valuationId: string;
  method: { id: string; methodType: string; inputs: string | null; formulaVersionId: string | null };
  onClose: () => void;
}

function RecalcPanel({ valuationId, method, onClose }: RecalcPanelProps) {
  const existingInputs: Record<string, number> = (() => {
    try { return method.inputs ? JSON.parse(method.inputs) : {}; } catch { return {}; }
  })();

  const [inputs, setInputs] = useState<Record<string, string>>(
    Object.fromEntries(Object.entries(existingInputs).map(([k, v]) => [k, String(v)]))
  );
  const [calcResult, setCalcResult] = useState<null | { value: number }>(null);
  const calculateMethod = useCalculateMethod();

  const isCustom = method.methodType === ValuationMethodType.Custom;
  const standardFields = isCustom ? [] : (METHOD_INPUTS[method.methodType] ?? []);

  const allKeys = isCustom
    ? Object.keys(existingInputs)
    : standardFields.map((f) => f.key);

  function setInput(key: string, value: string) {
    setInputs((p) => ({ ...p, [key]: value }));
  }

  function handleCalc() {
    const numInputs: Record<string, number> = {};
    for (const key of allKeys) {
      const val = parseFloat(inputs[key] ?? '');
      if (!isNaN(val)) numInputs[key] = val;
    }
    calculateMethod.mutate(
      {
        valuationId,
        methodId: method.id,
        data: {
          methodType: method.methodType,
          inputs: numInputs,
          formulaVersionId: method.formulaVersionId ?? undefined,
        },
      },
      { onSuccess: (res) => setCalcResult({ value: res.calculatedValue }) }
    );
  }

  return (
    <div className="mt-3 p-3 bg-gray-50 rounded-xl border border-gray-100 space-y-3">
      {allKeys.length > 0 ? (
        <div className="grid grid-cols-2 gap-2">
          {allKeys.map((key) => {
            const label = standardFields.find((f) => f.key === key)?.label ?? key;
            return (
              <div key={key}>
                <label className="block text-xs text-gray-500 mb-0.5">{label}</label>
                <input
                  type="number"
                  step="any"
                  value={inputs[key] ?? ''}
                  onChange={(e) => setInput(key, e.target.value)}
                  className="w-full text-xs border border-gray-200 rounded px-2 py-1 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
            );
          })}
        </div>
      ) : (
        <p className="text-xs text-gray-400">Sem parâmetros armazenados. Adicione inputs manualmente.</p>
      )}
      {calcResult && (
        <p className="text-sm font-semibold text-green-700">
          Resultado: {formatCurrency(calcResult.value)}
        </p>
      )}
      <div className="flex gap-2 justify-end">
        <button onClick={onClose} className="text-xs text-gray-400 hover:text-gray-600">Fechar</button>
        <Button size="sm" icon={<Calculator size={12} />} loading={calculateMethod.isPending} onClick={handleCalc}>
          Calcular
        </Button>
      </div>
    </div>
  );
}

export default function ValuationDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { selectedCompanyId } = useClientStore();
  const [rejectReason, setRejectReason] = useState('');
  const [showRejectForm, setShowRejectForm] = useState(false);
  const [showAddMethod, setShowAddMethod] = useState(false);
  const [recalcMethodId, setRecalcMethodId] = useState<string | null>(null);

  const { data: valuation, isLoading } = useValuation(id!);
  const submitValuation = useSubmitValuation();
  const approveValuation = useApproveValuation();
  const rejectValuation = useRejectValuation();
  const returnToDraft = useReturnValuationToDraft();
  const selectMethod = useSelectValuationMethod();

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <Spinner className="w-8 h-8" />
      </div>
    );
  }

  if (!valuation) {
    return (
      <div className="text-center py-20 text-gray-400">
        <p>Valuation não encontrado.</p>
      </div>
    );
  }

  const canSubmit = valuation.status === 'draft';
  const canApprove = valuation.status === 'pending_approval';
  const canReject = valuation.status === 'pending_approval';
  const canReturn = valuation.status === 'rejected';

  function handleReject() {
    if (!rejectReason.trim()) return;
    rejectValuation.mutate(
      { id: valuation!.id, data: { reason: rejectReason } },
      { onSuccess: () => setShowRejectForm(false) }
    );
  }

  return (
    <div className="space-y-6 animate-fade-in max-w-4xl">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/valuations')}
          className="p-2 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors"
        >
          <ArrowLeft size={18} />
        </button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h1 className="text-xl font-bold text-gray-900">
              {valuationEventTypeLabels[valuation.eventType] ?? valuation.eventType}
              {valuation.eventName && ` — ${valuation.eventName}`}
            </h1>
            <Badge className={valuationStatusColors[valuation.status] ?? ''}>
              {valuationStatusLabels[valuation.status] ?? valuation.status}
            </Badge>
          </div>
          <p className="text-sm text-gray-500 mt-0.5">{formatDate(valuation.valuationDate)}</p>
        </div>
        {/* Actions */}
        <div className="flex gap-2">
          {canSubmit && (
            <Button
              icon={<Send size={14} />}
              size="sm"
              onClick={() => submitValuation.mutate(valuation.id)}
              loading={submitValuation.isPending}
            >
              Submeter
            </Button>
          )}
          {canApprove && (
            <Button
              icon={<CheckCircle size={14} />}
              size="sm"
              variant="success"
              onClick={() => approveValuation.mutate(valuation.id)}
              loading={approveValuation.isPending}
            >
              Aprovar
            </Button>
          )}
          {canReject && (
            <Button
              icon={<XCircle size={14} />}
              size="sm"
              variant="danger"
              onClick={() => setShowRejectForm(true)}
            >
              Rejeitar
            </Button>
          )}
          {canReturn && (
            <Button
              icon={<RotateCcw size={14} />}
              size="sm"
              variant="secondary"
              onClick={() => returnToDraft.mutate(valuation.id)}
              loading={returnToDraft.isPending}
            >
              Retornar
            </Button>
          )}
        </div>
      </div>

      {/* Reject form */}
      {showRejectForm && (
        <Card>
          <div className="p-4 space-y-3">
            <p className="text-sm font-medium text-gray-700">Motivo da rejeição (obrigatório)</p>
            <textarea
              rows={3}
              value={rejectReason}
              onChange={(e) => setRejectReason(e.target.value)}
              className="w-full text-sm border border-gray-200 rounded-lg p-3 focus:outline-none focus:ring-2 focus:ring-red-400"
              placeholder="Descreva o motivo da rejeição..."
            />
            <div className="flex gap-2 justify-end">
              <Button variant="secondary" size="sm" onClick={() => setShowRejectForm(false)}>
                Cancelar
              </Button>
              <Button
                variant="danger"
                size="sm"
                disabled={!rejectReason.trim()}
                onClick={handleReject}
                loading={rejectValuation.isPending}
              >
                Confirmar Rejeição
              </Button>
            </div>
          </div>
        </Card>
      )}

      {/* Summary */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Card>
          <div className="p-4">
            <p className="text-xs text-gray-500 mb-1">Valor do Valuation</p>
            <p className="text-xl font-bold text-gray-900">
              {valuation.valuationAmount ? formatCurrency(valuation.valuationAmount) : '—'}
            </p>
          </div>
        </Card>
        <Card>
          <div className="p-4">
            <p className="text-xs text-gray-500 mb-1">Total de Ações</p>
            <p className="text-xl font-bold text-gray-900">
              {valuation.totalShares.toLocaleString('pt-BR')}
            </p>
          </div>
        </Card>
        <Card>
          <div className="p-4">
            <p className="text-xs text-gray-500 mb-1">Preço por Ação</p>
            <p className="text-xl font-bold text-gray-900">
              {valuation.pricePerShare ? `R$ ${valuation.pricePerShare.toFixed(4)}` : '—'}
            </p>
          </div>
        </Card>
        <Card>
          <div className="p-4">
            <p className="text-xs text-gray-500 mb-1">Metodologias</p>
            <p className="text-xl font-bold text-gray-900">{valuation.methods.length}</p>
          </div>
        </Card>
      </div>

      {/* Rejection reason */}
      {valuation.status === 'rejected' && valuation.rejectionReason && (
        <Card>
          <div className="p-4 bg-red-50 rounded-xl">
            <p className="text-sm font-medium text-red-700 mb-1">Motivo da Rejeição</p>
            <p className="text-sm text-red-600">{valuation.rejectionReason}</p>
          </div>
        </Card>
      )}

      {/* Methods */}
      <div>
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-base font-semibold text-gray-900">Metodologias de Cálculo</h2>
          {valuation.status === 'draft' && !showAddMethod && (
            <Button
              size="sm"
              icon={<Plus size={14} />}
              onClick={() => setShowAddMethod(true)}
            >
              Adicionar Metodologia
            </Button>
          )}
        </div>

        {/* Painel de adição */}
        {showAddMethod && valuation.status === 'draft' && (
          <div className="mb-4">
            <AddMethodPanel
              valuationId={valuation.id}
              companyId={selectedCompanyId ?? valuation.companyId}
              onClose={() => setShowAddMethod(false)}
            />
          </div>
        )}

        {valuation.methods.length === 0 && !showAddMethod ? (
          <Card>
            <div className="p-8 text-center text-gray-400">
              <Calculator size={32} className="mx-auto mb-2 opacity-30" />
              <p className="text-sm">Nenhuma metodologia adicionada.</p>
              {valuation.status === 'draft' && (
                <Button
                  size="sm"
                  icon={<Plus size={14} />}
                  className="mt-3"
                  onClick={() => setShowAddMethod(true)}
                >
                  Adicionar Metodologia
                </Button>
              )}
            </div>
          </Card>
        ) : (
          <div className="space-y-3">
            {valuation.methods.map((m) => (
              <Card key={m.id}>
                <div className="p-4">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      {m.isSelected && (
                        <Star size={16} className="text-yellow-500 fill-yellow-400 flex-shrink-0" />
                      )}
                      <div>
                        <p className="font-medium text-gray-900">
                          {valuationMethodLabels[m.methodType] ?? m.methodType}
                        </p>
                        {m.dataSource && (
                          <p className="text-xs text-gray-400">{m.dataSource}</p>
                        )}
                        {m.notes && (
                          <p className="text-xs text-gray-400 italic">{m.notes}</p>
                        )}
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <div className="text-right">
                        <p className="font-semibold text-gray-900">
                          {m.calculatedValue ? formatCurrency(m.calculatedValue) : '—'}
                        </p>
                        {m.isSelected && (
                          <p className="text-xs text-yellow-600 font-medium">Principal</p>
                        )}
                      </div>
                      {valuation.status === 'draft' && (
                        <div className="flex gap-1 ml-2">
                          {/* Calcular / Recalcular */}
                          <button
                            title={m.calculatedValue ? 'Recalcular' : 'Calcular'}
                            onClick={() =>
                              setRecalcMethodId((prev) => (prev === m.id ? null : m.id))
                            }
                            className="p-1.5 rounded-lg hover:bg-blue-50 text-blue-500 transition-colors"
                          >
                            {recalcMethodId === m.id ? (
                              <ChevronUp size={15} />
                            ) : (
                              <Calculator size={15} />
                            )}
                          </button>
                          {/* Selecionar como principal */}
                          {!m.isSelected && m.calculatedValue && (
                            <button
                              title="Usar como metodologia principal"
                              onClick={() =>
                                selectMethod.mutate({ valuationId: valuation.id, methodId: m.id })
                              }
                              disabled={selectMethod.isPending}
                              className="p-1.5 rounded-lg hover:bg-yellow-50 text-yellow-500 transition-colors"
                            >
                              <Star size={15} />
                            </button>
                          )}
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Painel inline de recálculo */}
                  {recalcMethodId === m.id && (
                    <RecalcPanel
                      valuationId={valuation.id}
                      method={m}
                      onClose={() => setRecalcMethodId(null)}
                    />
                  )}
                </div>
              </Card>
            ))}
          </div>
        )}
      </div>

      {/* Notes */}
      {valuation.notes && (
        <Card>
          <div className="p-4">
            <p className="text-sm font-medium text-gray-700 mb-1">Observações</p>
            <p className="text-sm text-gray-600">{valuation.notes}</p>
          </div>
        </Card>
      )}
    </div>
  );
}
