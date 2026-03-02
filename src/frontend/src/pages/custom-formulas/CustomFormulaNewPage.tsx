import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useClientStore } from '@/stores/clientStore';
import { ArrowLeft, Plus, Trash2, FlaskConical, Play } from 'lucide-react';
import { Button, Card, Badge } from '@/components/ui';
import {
  useCreateCustomFormula,
  useTestFormula,
} from '@/hooks';
import {
  FormulaVariableType,
  type FormulaVariableDefinition,
  type CreateCustomFormulaRequest,
  type TestFormulaRequest,
} from '@/types';
import toast from 'react-hot-toast';

const VAR_TYPE_OPTIONS: { value: FormulaVariableType; label: string }[] = [
  { value: FormulaVariableType.Currency, label: 'Moeda' },
  { value: FormulaVariableType.Number, label: 'Número' },
  { value: FormulaVariableType.Percentage, label: 'Percentual' },
  { value: FormulaVariableType.Integer, label: 'Inteiro' },
  { value: FormulaVariableType.Boolean, label: 'Booleano' },
];

function emptyVar(): FormulaVariableDefinition {
  return {
    name: '', label: '', type: FormulaVariableType.Number,
    unit: '', defaultValue: undefined, isRequired: true, displayOrder: 0,
  };
}

export default function CustomFormulaNewPage() {
  const navigate = useNavigate();
  const { selectedCompanyId } = useClientStore();
  const createFormula = useCreateCustomFormula();
  const testFormulaMutation = useTestFormula();

  // Step state
  const [step, setStep] = useState<1 | 2>(1);

  // Step 1 — metadata
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [sectorTag, setSectorTag] = useState('');

  // Step 2 — formula
  const [expression, setExpression] = useState('');
  const [variables, setVariables] = useState<FormulaVariableDefinition[]>([emptyVar()]);
  const [testValues, setTestValues] = useState<Record<string, number>>({});
  const [testResult, setTestResult] = useState<{ result?: number; error?: string } | null>(null);

  function addVariable() {
    setVariables((vs) => [...vs, { ...emptyVar(), displayOrder: vs.length }]);
  }

  function removeVariable(i: number) {
    setVariables((vs) => vs.filter((_, idx) => idx !== i));
  }

  function updateVar(i: number, field: keyof FormulaVariableDefinition, value: unknown) {
    setVariables((vs) => vs.map((v, idx) => idx === i ? { ...v, [field]: value } : v));
  }

  async function handleTest() {
    if (!expression) { toast.error('Insira a expressão antes de testar.'); return; }
    const req: TestFormulaRequest = {
      expression,
      inputs: testValues,
    };
    const res = await testFormulaMutation.mutateAsync(req);
    setTestResult({ result: res.result ?? undefined });
  }

  async function handleSubmit() {
    if (!name) { toast.error('Nome é obrigatório.'); return; }
    if (!expression) { toast.error('Expressão é obrigatória.'); return; }
    const payload: CreateCustomFormulaRequest = {
      companyId: selectedCompanyId!,
      name,
      description: description || undefined,
      sectorTag: sectorTag || undefined,
      expression,
      variables,
      resultUnit: 'BRL',
    };
    await createFormula.mutateAsync(payload);
    navigate('/valuations/custom-formulas');
  }

  return (
    <div className="max-w-3xl space-y-6 animate-fade-in">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/valuations/custom-formulas')}
          className="p-2 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors"
        >
          <ArrowLeft size={18} />
        </button>
        <div>
          <h1 className="text-xl font-bold text-gray-900">Nova Fórmula Customizada</h1>
          <p className="text-sm text-gray-400 mt-0.5">Etapa {step} de 2</p>
        </div>
      </div>

      {/* Step indicator */}
      <div className="flex gap-2 text-sm">
        <div className={`px-3 py-1.5 rounded-full font-medium transition-colors ${step === 1 ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-500'}`}>
          1 · Metadados
        </div>
        <div className={`px-3 py-1.5 rounded-full font-medium transition-colors ${step === 2 ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-500'}`}>
          2 · Fórmula & Variáveis
        </div>
      </div>

      {/* Step 1 */}
      {step === 1 && (
        <Card>
          <div className="p-6 space-y-4">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Nome *</label>
              <input
                className="input-base w-full"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Ex.: DCF Simplificado"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Descrição</label>
              <textarea
                className="input-base w-full h-20 resize-none"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Descreva o objetivo desta fórmula..."
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">Setor/Tag</label>
              <input
                className="input-base w-full"
                value={sectorTag}
                onChange={(e) => setSectorTag(e.target.value)}
                placeholder="Ex.: saas, fintech, marketplace"
              />
            </div>
            <div className="flex justify-end pt-2">
              <Button onClick={() => { if (!name) { toast.error('Nome é obrigatório.'); return; } setStep(2); }}>
                Próximo →
              </Button>
            </div>
          </div>
        </Card>
      )}

      {/* Step 2 */}
      {step === 2 && (
        <>
          {/* Expression */}
          <Card>
            <div className="p-5 space-y-3">
              <div className="flex items-center gap-2">
                <FlaskConical size={16} className="text-blue-500" />
                <h2 className="text-sm font-semibold text-gray-800">Expressão da Fórmula</h2>
              </div>
              <p className="text-xs text-gray-400">
                Use os nomes das variáveis definidas abaixo. Ex.:{' '}
                <code className="bg-gray-100 px-1 rounded">revenue * multiple / (1 + discount_rate)</code>
              </p>
              <textarea
                className="input-base w-full h-24 font-mono text-sm resize-none"
                value={expression}
                onChange={(e) => setExpression(e.target.value)}
                placeholder="Escreva a expressão matemática..."
              />
            </div>
          </Card>

          {/* Variables */}
          <Card>
            <div className="p-5 space-y-4">
              <div className="flex items-center justify-between">
                <h2 className="text-sm font-semibold text-gray-800">Variáveis</h2>
                <Button variant="secondary" size="sm" icon={<Plus size={14} />} onClick={addVariable}>
                  Adicionar
                </Button>
              </div>
              {variables.map((v, i) => (
                <div key={i} className="border border-gray-100 rounded-lg p-4 space-y-3">
                  <div className="flex items-center gap-2">
                    <Badge className="bg-gray-100 text-gray-500">#{i + 1}</Badge>
                    <span className="text-xs font-mono text-blue-700">{v.name || 'nova_variavel'}</span>
                    <button
                      onClick={() => removeVariable(i)}
                      className="ml-auto text-gray-300 hover:text-red-500 transition-colors"
                    >
                      <Trash2 size={14} />
                    </button>
                  </div>
                  <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                    <div>
                      <label className="block text-xs text-gray-500 mb-1">Nome (código) *</label>
                      <input
                        className="input-base w-full font-mono text-sm"
                        value={v.name}
                        onChange={(e) => updateVar(i, 'name', e.target.value.replace(/\s/g, '_').toLowerCase())}
                        placeholder="ex: revenue"
                      />
                    </div>
                    <div>
                      <label className="block text-xs text-gray-500 mb-1">Rótulo</label>
                      <input
                        className="input-base w-full"
                        value={v.label}
                        onChange={(e) => updateVar(i, 'label', e.target.value)}
                        placeholder="ex: Receita"
                      />
                    </div>
                    <div>
                      <label className="block text-xs text-gray-500 mb-1">Tipo</label>
                      <select
                        className="input-base w-full"
                        value={v.type}
                        onChange={(e) => updateVar(i, 'type', e.target.value)}
                      >
                        {VAR_TYPE_OPTIONS.map((o) => (
                          <option key={o.value} value={o.value}>{o.label}</option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label className="block text-xs text-gray-500 mb-1">Unidade</label>
                      <input
                        className="input-base w-full"
                        value={v.unit ?? ''}
                        onChange={(e) => updateVar(i, 'unit', e.target.value)}
                        placeholder="R$, %, x"
                      />
                    </div>
                    <div>
                      <label className="block text-xs text-gray-500 mb-1">Valor padrão</label>
                      <input
                        type="number"
                        className="input-base w-full"
                        value={v.defaultValue ?? ''}
                        onChange={(e) => updateVar(i, 'defaultValue', e.target.value ? parseFloat(e.target.value) : undefined)}
                      />
                    </div>
                    <div className="flex items-end pb-0.5">
                      <label className="flex items-center gap-2 cursor-pointer">
                        <input
                          type="checkbox"
                          checked={v.isRequired}
                          onChange={(e) => updateVar(i, 'isRequired', e.target.checked)}
                          className="w-4 h-4 rounded"
                        />
                        <span className="text-xs text-gray-500">Obrigatório</span>
                      </label>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </Card>

          {/* Inline Test */}
          <Card>
            <div className="p-5 space-y-4">
              <h2 className="text-sm font-semibold text-gray-800">Testar Fórmula</h2>
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                {variables.filter((v) => v.name).map((v) => (
                  <div key={v.name}>
                    <label className="block text-xs text-gray-500 mb-1">
                      {v.label || v.name} {v.unit ? `(${v.unit})` : ''}
                    </label>
                    <input
                      type="number"
                      className="input-base w-full"
                      value={testValues[v.name] ?? v.defaultValue ?? ''}
                      onChange={(e) =>
                        setTestValues((tv) => ({
                          ...tv,
                          [v.name]: parseFloat(e.target.value),
                        }))
                      }
                    />
                  </div>
                ))}
              </div>
              <div className="flex items-center gap-3">
                <Button
                  variant="secondary"
                  size="sm"
                  icon={<Play size={14} />}
                  onClick={handleTest}
                  loading={testFormulaMutation.isPending}
                >
                  Executar Teste
                </Button>
                {testResult && (
                  <div className="text-sm">
                    {testResult.error ? (
                      <span className="text-red-500">Erro: {testResult.error}</span>
                    ) : (
                      <span className="text-green-600 font-semibold">
                        Resultado: {testResult.result?.toLocaleString('pt-BR')}
                      </span>
                    )}
                  </div>
                )}
              </div>
            </div>
          </Card>

          {/* Actions */}
          <div className="flex justify-between">
            <Button variant="secondary" onClick={() => setStep(1)}>
              ← Voltar
            </Button>
            <Button onClick={handleSubmit} loading={createFormula.isPending}>
              Criar Fórmula
            </Button>
          </div>
        </>
      )}
    </div>
  );
}
