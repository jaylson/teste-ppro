import { useState, useEffect, useMemo } from 'react';
import { useForm, useFieldArray, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  X,
  Plus,
  Trash2,
  Calculator,
  TrendingDown,
  DollarSign,
  HelpCircle,
  ChevronRight,
  Users,
  PieChart,
  ArrowLeft,
  Info,
  CheckCircle2,
  AlertCircle,
} from 'lucide-react';
import { Button, Card } from '@/components/ui';
import { useSimulateRound, useDilutionCalculation } from '@/hooks/useSimulator';
import { useClientStore } from '@/stores/clientStore';
import {
  RoundType,
  getRoundTypeLabel,
  roundNameSuggestions,
  type RoundSimulationRequest,
  type RoundSimulationResponse,
} from '@/types';
import { formatCurrency, formatPercentage } from '@/utils/format';

// Validation schema
const newInvestorSchema = z.object({
  name: z.string().min(2, 'Nome obrigatório'),
  investmentAmount: z.coerce.number().min(1, 'Valor deve ser maior que 0'),
  email: z.string().email('E-mail inválido').optional().or(z.literal('')),
  document: z.string().optional(),
});

const simulationSchema = z.object({
  roundName: z.string().min(1, 'Nome da rodada obrigatório'),
  roundType: z.nativeEnum(RoundType),
  preMoneyValuation: z.coerce.number().min(1, 'Valuation pre-money obrigatório'),
  newInvestors: z.array(newInvestorSchema).min(1, 'Adicione pelo menos 1 investidor'),
  includeOptionPool: z.boolean(),
  optionPoolPercentage: z.coerce.number().min(0).max(50).optional(),
  optionPoolPreMoney: z.boolean().optional(),
});

type SimulationFormData = z.infer<typeof simulationSchema>;

interface RoundSimulatorModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSimulationComplete?: (result: RoundSimulationResponse) => void;
  companyId?: string;
}

export default function RoundSimulatorModal({
  isOpen,
  onClose,
  onSimulationComplete,
  companyId: propCompanyId,
}: RoundSimulatorModalProps) {
  const { selectedCompanyId } = useClientStore();
  const companyId = propCompanyId || selectedCompanyId;

  const [result, setResult] = useState<RoundSimulationResponse | null>(null);
  const [showResults, setShowResults] = useState(false);
  const [currentStep, setCurrentStep] = useState(1);

  const simulateRound = useSimulateRound();

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
    reset,
    trigger,
  } = useForm<SimulationFormData>({
    resolver: zodResolver(simulationSchema),
    mode: 'onChange',
    defaultValues: {
      roundName: '',
      roundType: RoundType.Equity,
      preMoneyValuation: 0,
      newInvestors: [{ name: '', investmentAmount: 0, email: '', document: '' }],
      includeOptionPool: false,
      optionPoolPercentage: 10,
      optionPoolPreMoney: true,
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'newInvestors',
  });

  // Usar useWatch para observar valores de forma reativa
  const watchPreMoney = useWatch({ control, name: 'preMoneyValuation' });
  const watchIncludePool = useWatch({ control, name: 'includeOptionPool' });
  const watchNewInvestors = useWatch({ control, name: 'newInvestors' });
  const watchRoundName = useWatch({ control, name: 'roundName' });

  // Calcular valor total do investimento de forma reativa
  const totalInvestment = useMemo(() => {
    if (!watchNewInvestors || watchNewInvestors.length === 0) return 0;
    return watchNewInvestors.reduce((sum, inv) => sum + (Number(inv?.investmentAmount) || 0), 0);
  }, [watchNewInvestors]);

  // Quick dilution calculation
  const { data: quickDilution } = useDilutionCalculation(
    companyId || undefined,
    totalInvestment,
    Number(watchPreMoney) || 0,
    !!companyId && totalInvestment > 0 && Number(watchPreMoney) > 0
  );

  // Reset form when modal opens/closes
  useEffect(() => {
    if (isOpen) {
      setResult(null);
      setShowResults(false);
      setCurrentStep(1);
      reset();
    }
  }, [isOpen, reset]);

  const onSubmit = async (data: SimulationFormData) => {
    if (!companyId) {
      return;
    }

    const request: RoundSimulationRequest = {
      companyId,
      preMoneyValuation: data.preMoneyValuation,
      investmentAmount: totalInvestment,
      roundName: data.roundName,
      roundType: data.roundType,
      newInvestors: data.newInvestors,
      includeOptionPool: data.includeOptionPool,
      optionPoolPercentage: data.optionPoolPercentage || 0,
      optionPoolPreMoney: data.optionPoolPreMoney ?? true,
    };

    try {
      const simulationResult = await simulateRound.mutateAsync(request);
      setResult(simulationResult);
      setShowResults(true);
      onSimulationComplete?.(simulationResult);
    } catch {
      // Error handled by mutation
    }
  };

  const handleBack = () => {
    setShowResults(false);
  };

  const handleNextStep = async () => {
    const fieldsToValidate =
      currentStep === 1
        ? (['roundName', 'roundType', 'preMoneyValuation'] as const)
        : (['newInvestors'] as const);

    const isStepValid = await trigger(fieldsToValidate);
    if (isStepValid) {
      setCurrentStep(2);
    }
  };

  const handlePrevStep = () => {
    setCurrentStep(1);
  };

  if (!isOpen) return null;

  const postMoneyValuation = (Number(watchPreMoney) || 0) + totalInvestment;
  const estimatedDilution = postMoneyValuation > 0 ? (totalInvestment / postMoneyValuation) * 100 : 0;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-2xl max-w-4xl w-full max-h-[90vh] overflow-hidden flex flex-col shadow-2xl">
        {/* Header */}
        <div className="bg-gradient-to-r from-primary-600 to-primary-700 p-6 text-white">
          <div className="flex justify-between items-start">
            <div>
              <h2 className="text-2xl font-bold">
                {showResults ? '📊 Resultado da Simulação' : '🚀 Simular Rodada de Investimento'}
              </h2>
              <p className="text-primary-100 mt-1">
                {showResults
                  ? 'Analise o impacto da rodada no Cap Table da sua empresa'
                  : 'Simule cenários de captação e veja o impacto na estrutura societária'}
              </p>
            </div>
            <button
              onClick={onClose}
              className="text-white/80 hover:text-white hover:bg-white/10 rounded-lg p-2 transition-colors"
            >
              <X className="w-6 h-6" />
            </button>
          </div>

          {/* Steps Indicator (only on form) */}
          {!showResults && (
            <div className="flex items-center gap-4 mt-6">
              <div className={`flex items-center gap-2 ${currentStep >= 1 ? 'text-white' : 'text-primary-200'}`}>
                <div
                  className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold ${
                    currentStep >= 1 ? 'bg-white text-primary-600' : 'bg-primary-500 text-white'
                  }`}
                >
                  {currentStep > 1 ? <CheckCircle2 className="w-5 h-5" /> : '1'}
                </div>
                <span className="font-medium">Dados da Rodada</span>
              </div>
              <ChevronRight className="w-5 h-5 text-primary-300" />
              <div className={`flex items-center gap-2 ${currentStep >= 2 ? 'text-white' : 'text-primary-200'}`}>
                <div
                  className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold ${
                    currentStep >= 2 ? 'bg-white text-primary-600' : 'bg-primary-500 text-white'
                  }`}
                >
                  2
                </div>
                <span className="font-medium">Investidores</span>
              </div>
            </div>
          )}
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {showResults && result ? (
            <SimulationResultsView result={result} onBack={handleBack} />
          ) : (
            <form id="simulation-form" onSubmit={handleSubmit(onSubmit)} className="space-y-6">
              {/* Step 1: Round Info */}
              {currentStep === 1 && (
                <div className="space-y-6 animate-fade-in">
                  {/* Info Banner */}
                  <div className="bg-blue-50 border border-blue-200 rounded-xl p-4 flex gap-3">
                    <Info className="w-5 h-5 text-blue-500 flex-shrink-0 mt-0.5" />
                    <div className="text-sm text-blue-800">
                      <p className="font-semibold mb-1">Como funciona a simulação?</p>
                      <p>
                        Configure os parâmetros da rodada de investimento para visualizar como ficará
                        a distribuição de participações após a captação. A simulação não altera dados reais.
                      </p>
                    </div>
                  </div>

                  {/* Round Name & Type */}
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-2">
                        Nome da Rodada *
                      </label>
                      <div className="relative">
                        <input
                          {...register('roundName')}
                          list="round-suggestions"
                          placeholder="Ex: Series A, Seed, Pre-Seed..."
                          className="input w-full text-lg"
                        />
                        <datalist id="round-suggestions">
                          {roundNameSuggestions.map((name) => (
                            <option key={name} value={name} />
                          ))}
                        </datalist>
                      </div>
                      {errors.roundName && (
                        <p className="text-sm text-red-500 mt-1 flex items-center gap-1">
                          <AlertCircle className="w-3 h-3" />
                          {errors.roundName.message}
                        </p>
                      )}
                      <p className="text-xs text-gray-500 mt-1">
                        Dica: Use nomes padrão como "Seed" ou "Series A" para facilitar a identificação
                      </p>
                    </div>

                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-2">
                        Tipo da Rodada *
                      </label>
                      <select
                        {...register('roundType', { valueAsNumber: true })}
                        className="input w-full text-lg"
                      >
                        {Object.entries(RoundType)
                          .filter(([key]) => isNaN(Number(key)))
                          .map(([key, value]) => (
                            <option key={key} value={value}>
                              {getRoundTypeLabel(value as RoundType)}
                            </option>
                          ))}
                      </select>
                      <p className="text-xs text-gray-500 mt-1">
                        Equity: emissão de ações | SAFE/Nota: conversíveis em ações
                      </p>
                    </div>
                  </div>

                  {/* Valuation */}
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2 flex items-center gap-2">
                      Valuation Pre-Money *
                      <button
                        type="button"
                        className="text-gray-400 hover:text-gray-600"
                        title="Valor da empresa antes do investimento"
                      >
                        <HelpCircle className="w-4 h-4" />
                      </button>
                    </label>
                    <div className="relative">
                      <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-medium">
                        R$
                      </span>
                      <input
                        {...register('preMoneyValuation')}
                        type="number"
                        min="0"
                        step="100000"
                        placeholder="10.000.000"
                        className="input w-full pl-12 text-lg font-semibold"
                      />
                    </div>
                    {errors.preMoneyValuation && (
                      <p className="text-sm text-red-500 mt-1 flex items-center gap-1">
                        <AlertCircle className="w-3 h-3" />
                        {errors.preMoneyValuation.message}
                      </p>
                    )}
                    <p className="text-xs text-gray-500 mt-1">
                      Valor acordado da empresa <strong>antes</strong> de receber o investimento
                    </p>
                  </div>

                  {/* Option Pool */}
                  <Card className="p-5 border-2 border-dashed border-gray-200 hover:border-gray-300 transition-colors">
                    <div className="flex items-start gap-4">
                      <input
                        {...register('includeOptionPool')}
                        type="checkbox"
                        id="includeOptionPool"
                        className="w-5 h-5 rounded border-gray-300 text-primary-600 focus:ring-primary-500 mt-0.5"
                      />
                      <div className="flex-1">
                        <label
                          htmlFor="includeOptionPool"
                          className="block text-sm font-semibold text-gray-900 cursor-pointer"
                        >
                          Incluir Pool de Opções (ESOP)
                        </label>
                        <p className="text-sm text-gray-500 mt-1">
                          Reserve uma porcentagem de ações para futuros colaboradores. Isso é comum em rodadas
                          de investimento e pode impactar a diluição dos sócios atuais.
                        </p>

                        {watchIncludePool && (
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4 pt-4 border-t border-gray-200">
                            <div>
                              <label className="block text-sm font-medium text-gray-700 mb-1">
                                Tamanho do Pool (%)
                              </label>
                              <input
                                {...register('optionPoolPercentage')}
                                type="number"
                                min="0"
                                max="50"
                                step="1"
                                className="input w-full"
                              />
                            </div>
                            <div>
                              <label className="block text-sm font-medium text-gray-700 mb-1">
                                Momento da Diluição
                              </label>
                              <select {...register('optionPoolPreMoney')} className="input w-full">
                                <option value="true">Pre-Money (dilui fundadores)</option>
                                <option value="">Post-Money (dilui todos)</option>
                              </select>
                            </div>
                          </div>
                        )}
                      </div>
                    </div>
                  </Card>
                </div>
              )}

              {/* Step 2: Investors */}
              {currentStep === 2 && (
                <div className="space-y-6 animate-fade-in">
                  {/* Summary Preview */}
                  <div className="grid grid-cols-3 gap-4">
                    <Card className="p-4 bg-gray-50">
                      <p className="text-xs text-gray-500 uppercase font-medium">Rodada</p>
                      <p className="text-lg font-bold text-gray-900 truncate">
                        {watchRoundName || '-'}
                      </p>
                    </Card>
                    <Card className="p-4 bg-gray-50">
                      <p className="text-xs text-gray-500 uppercase font-medium">Pre-Money</p>
                      <p className="text-lg font-bold text-gray-900">
                        {formatCurrency(Number(watchPreMoney) || 0)}
                      </p>
                    </Card>
                    <Card className="p-4 bg-gray-50">
                      <p className="text-xs text-gray-500 uppercase font-medium">Post-Money</p>
                      <p className="text-lg font-bold text-primary-600">{formatCurrency(postMoneyValuation)}</p>
                    </Card>
                  </div>

                  {/* Investors Header */}
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                        <Users className="w-5 h-5 text-primary-600" />
                        Investidores da Rodada
                      </h3>
                      <p className="text-sm text-gray-500 mt-0.5">
                        Adicione os investidores e seus respectivos aportes
                      </p>
                    </div>
                    <Button
                      type="button"
                      variant="secondary"
                      size="sm"
                      onClick={() => append({ name: '', investmentAmount: 0, email: '', document: '' })}
                      icon={<Plus className="w-4 h-4" />}
                    >
                      Adicionar Investidor
                    </Button>
                  </div>

                  {/* Investors List */}
                  <div className="space-y-4">
                    {fields.map((field, index) => (
                      <Card key={field.id} className="p-4 hover:shadow-md transition-shadow">
                        <div className="flex items-start gap-4">
                          <div className="w-10 h-10 rounded-full bg-primary-100 flex items-center justify-center flex-shrink-0">
                            <span className="text-primary-600 font-bold">{index + 1}</span>
                          </div>

                          <div className="flex-1 grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                              <label className="block text-xs font-medium text-gray-500 mb-1">
                                Nome do Investidor *
                              </label>
                              <input
                                {...register(`newInvestors.${index}.name`)}
                                placeholder="Nome completo ou razão social"
                                className="input w-full"
                              />
                              {errors.newInvestors?.[index]?.name && (
                                <p className="text-xs text-red-500 mt-1">
                                  {errors.newInvestors[index]?.name?.message}
                                </p>
                              )}
                            </div>
                            <div>
                              <label className="block text-xs font-medium text-gray-500 mb-1">
                                Valor do Aporte (R$) *
                              </label>
                              <input
                                {...register(`newInvestors.${index}.investmentAmount`)}
                                type="number"
                                min="0"
                                step="1000"
                                placeholder="500.000"
                                className="input w-full"
                              />
                              {errors.newInvestors?.[index]?.investmentAmount && (
                                <p className="text-xs text-red-500 mt-1">
                                  {errors.newInvestors[index]?.investmentAmount?.message}
                                </p>
                              )}
                            </div>
                            <div className="md:col-span-2">
                              <label className="block text-xs font-medium text-gray-500 mb-1">
                                E-mail (opcional)
                              </label>
                              <input
                                {...register(`newInvestors.${index}.email`)}
                                type="email"
                                placeholder="investidor@email.com"
                                className="input w-full"
                              />
                            </div>
                          </div>

                          {fields.length > 1 && (
                            <button
                              type="button"
                              onClick={() => remove(index)}
                              className="p-2 text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                              title="Remover investidor"
                            >
                              <Trash2 className="w-5 h-5" />
                            </button>
                          )}
                        </div>
                      </Card>
                    ))}
                  </div>

                  {errors.newInvestors?.message && (
                    <p className="text-sm text-red-500 flex items-center gap-1">
                      <AlertCircle className="w-4 h-4" />
                      {errors.newInvestors.message}
                    </p>
                  )}

                  {/* Investment Summary */}
                  <Card className="p-5 bg-gradient-to-r from-green-50 to-emerald-50 border-green-200">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <div className="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center">
                          <DollarSign className="w-6 h-6 text-green-600" />
                        </div>
                        <div>
                          <p className="text-sm font-medium text-green-800">Investimento Total da Rodada</p>
                          <p className="text-3xl font-bold text-green-700">{formatCurrency(totalInvestment)}</p>
                        </div>
                      </div>

                      {totalInvestment > 0 && (
                        <div className="text-right">
                          <div className="flex items-center gap-2 text-orange-600">
                            <TrendingDown className="w-5 h-5" />
                            <span className="text-sm font-medium">Diluição estimada</span>
                          </div>
                          <p className="text-2xl font-bold text-orange-600">
                            {quickDilution
                              ? formatPercentage(quickDilution.dilutionPercentage)
                              : formatPercentage(estimatedDilution)}
                          </p>
                        </div>
                      )}
                    </div>
                  </Card>
                </div>
              )}
            </form>
          )}
        </div>

        {/* Footer */}
        <div className="border-t border-gray-200 p-6 bg-gray-50">
          {showResults ? (
            <div className="flex justify-between">
              <Button variant="secondary" onClick={handleBack} icon={<ArrowLeft className="w-4 h-4" />}>
                Nova Simulação
              </Button>
              <Button variant="primary" onClick={onClose}>
                Fechar
              </Button>
            </div>
          ) : (
            <div className="flex justify-between items-center">
              <div>
                {currentStep === 2 && (
                  <Button variant="ghost" onClick={handlePrevStep} icon={<ArrowLeft className="w-4 h-4" />}>
                    Voltar
                  </Button>
                )}
              </div>
              <div className="flex gap-3">
                <Button variant="secondary" onClick={onClose}>
                  Cancelar
                </Button>
                {currentStep === 1 ? (
                  <Button
                    type="button"
                    onClick={handleNextStep}
                    icon={<ChevronRight className="w-4 h-4" />}
                  >
                    Próximo: Investidores
                  </Button>
                ) : (
                  <Button
                    type="submit"
                    form="simulation-form"
                    loading={simulateRound.isPending}
                    disabled={totalInvestment === 0}
                    icon={<Calculator className="w-4 h-4" />}
                  >
                    Simular Rodada
                  </Button>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

// ============================================================================
// SIMULATION RESULTS VIEW (Internal Component)
// ============================================================================

interface SimulationResultsViewProps {
  result: RoundSimulationResponse;
  onBack: () => void;
}

function SimulationResultsView({ result }: SimulationResultsViewProps) {
  return (
    <div className="space-y-6 animate-fade-in">
      {/* Success Banner */}
      <div className="bg-green-50 border border-green-200 rounded-xl p-4 flex gap-3">
        <CheckCircle2 className="w-5 h-5 text-green-500 flex-shrink-0 mt-0.5" />
        <div className="text-sm text-green-800">
          <p className="font-semibold">Simulação concluída com sucesso!</p>
          <p>
            Abaixo você pode ver como ficará o Cap Table após a rodada "{result.roundName}".
            Lembre-se: esta é apenas uma simulação e não afeta seus dados reais.
          </p>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Card className="p-4 text-center bg-gradient-to-br from-blue-50 to-indigo-50 border-blue-200">
          <p className="text-xs text-blue-600 uppercase font-medium">Post-Money</p>
          <p className="text-xl font-bold text-blue-900 mt-1">
            {formatCurrency(result.postMoneyValuation)}
          </p>
        </Card>
        <Card className="p-4 text-center bg-gradient-to-br from-purple-50 to-fuchsia-50 border-purple-200">
          <p className="text-xs text-purple-600 uppercase font-medium">Preço/Ação</p>
          <p className="text-xl font-bold text-purple-900 mt-1">
            {formatCurrency(result.pricePerShare)}
          </p>
        </Card>
        <Card className="p-4 text-center bg-gradient-to-br from-green-50 to-emerald-50 border-green-200">
          <p className="text-xs text-green-600 uppercase font-medium">Novas Ações</p>
          <p className="text-xl font-bold text-green-700 mt-1">
            +{result.newSharesIssued.toLocaleString('pt-BR')}
          </p>
        </Card>
        <Card className="p-4 text-center bg-gradient-to-br from-orange-50 to-amber-50 border-orange-200">
          <p className="text-xs text-orange-600 uppercase font-medium">Diluição Total</p>
          <p className="text-xl font-bold text-orange-600 mt-1">
            {formatPercentage(result.totalDilution)}
          </p>
        </Card>
      </div>

      {/* New Investors */}
      <Card className="overflow-hidden">
        <div className="px-4 py-3 bg-gray-50 border-b border-gray-200">
          <h3 className="text-sm font-semibold text-gray-900 flex items-center gap-2">
            <Users className="w-4 h-4 text-primary-600" />
            Novos Investidores
          </h3>
        </div>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                  Investidor
                </th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">
                  Investimento
                </th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">
                  Ações
                </th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">
                  Participação
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {result.newInvestors.map((investor, idx) => (
                <tr key={idx} className="bg-white hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <div className="w-8 h-8 rounded-full bg-green-100 flex items-center justify-center">
                        <DollarSign className="w-4 h-4 text-green-600" />
                      </div>
                      <span className="font-medium text-gray-900">{investor.name}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-right text-gray-900 font-medium">
                    {formatCurrency(investor.investmentAmount)}
                  </td>
                  <td className="px-4 py-3 text-right text-gray-600">
                    {investor.sharesReceived.toLocaleString('pt-BR')}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold bg-green-100 text-green-700">
                      {formatPercentage(investor.ownershipPercentage)}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Option Pool (if included) */}
      {result.optionPool && (
        <Card className="p-4 bg-purple-50 border-purple-200">
          <h3 className="text-sm font-semibold text-purple-900 mb-3 flex items-center gap-2">
            <PieChart className="w-4 h-4" />
            Pool de Opções (ESOP)
          </h3>
          <div className="grid grid-cols-3 gap-4 text-sm">
            <div>
              <p className="text-purple-600">Percentual</p>
              <p className="text-lg font-bold text-purple-900">
                {formatPercentage(result.optionPool.percentage)}
              </p>
            </div>
            <div>
              <p className="text-purple-600">Ações Reservadas</p>
              <p className="text-lg font-bold text-purple-900">
                {result.optionPool.shares.toLocaleString('pt-BR')}
              </p>
            </div>
            <div>
              <p className="text-purple-600">Momento</p>
              <p className="text-lg font-bold text-purple-900">
                {result.optionPool.isPreMoney ? 'Pre-Money' : 'Post-Money'}
              </p>
            </div>
          </div>
        </Card>
      )}

      {/* Cap Table Comparison */}
      <div>
        <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <PieChart className="w-5 h-5 text-primary-600" />
          Comparativo do Cap Table
        </h3>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {/* Before */}
          <Card className="p-4 border-2 border-gray-200">
            <h4 className="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-2">
              <span className="w-3 h-3 rounded-full bg-gray-400"></span>
              Antes da Rodada
            </h4>
            <div className="space-y-2 max-h-48 overflow-y-auto">
              {result.capTableBefore.map((entry, idx) => (
                <div key={idx} className="flex justify-between items-center text-sm py-1">
                  <span className="text-gray-700">{entry.shareholderName}</span>
                  <span className="font-semibold text-gray-900">{formatPercentage(entry.ownership)}</span>
                </div>
              ))}
            </div>
            <div className="mt-3 pt-3 border-t border-gray-200 flex justify-between text-sm font-semibold">
              <span className="text-gray-600">Total de Ações</span>
              <span className="text-gray-900">{result.sharesBefore.toLocaleString('pt-BR')}</span>
            </div>
          </Card>

          {/* After */}
          <Card className="p-4 border-2 border-green-300 bg-green-50/50">
            <h4 className="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-2">
              <span className="w-3 h-3 rounded-full bg-green-500"></span>
              Depois da Rodada
            </h4>
            <div className="space-y-2 max-h-48 overflow-y-auto">
              {result.capTableAfter.map((entry, idx) => (
                <div key={idx} className="flex justify-between items-center text-sm py-1">
                  <div className="flex items-center gap-2">
                    <span className="text-gray-700">{entry.shareholderName}</span>
                    {entry.isNewInvestor && (
                      <span className="text-xs px-1.5 py-0.5 bg-green-100 text-green-700 rounded-full font-medium">
                        Novo
                      </span>
                    )}
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="font-semibold text-gray-900">{formatPercentage(entry.ownership)}</span>
                    {entry.dilutionPercentage > 0 && (
                      <span className="text-xs text-orange-600 font-medium">
                        (-{formatPercentage(entry.dilutionPercentage, 1)})
                      </span>
                    )}
                  </div>
                </div>
              ))}
            </div>
            <div className="mt-3 pt-3 border-t border-green-200 flex justify-between text-sm font-semibold">
              <span className="text-gray-600">Total de Ações</span>
              <span className="text-green-700">{result.sharesAfter.toLocaleString('pt-BR')}</span>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
