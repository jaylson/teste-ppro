import { useState, useEffect } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X, Plus, Trash2, Calculator, TrendingDown, DollarSign } from 'lucide-react';
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
  name: z.string().min(2, 'Nome do investidor obrigatório'),
  investmentAmount: z.coerce.number().min(1, 'Valor deve ser maior que 0'),
  email: z.string().email('E-mail inválido').optional().or(z.literal('')),
  document: z.string().optional(),
});

const simulationSchema = z.object({
  roundName: z.string().min(1, 'Nome da rodada obrigatório'),
  roundType: z.nativeEnum(RoundType),
  preMoneyValuation: z.coerce.number().min(1, 'Valuation pre-money obrigatório'),
  investmentAmount: z.coerce.number().min(1, 'Valor do investimento obrigatório'),
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

  const simulateRound = useSimulateRound();

  const {
    register,
    handleSubmit,
    control,
    watch,
    formState: { errors },
    reset,
    setValue,
  } = useForm<SimulationFormData>({
    resolver: zodResolver(simulationSchema),
    defaultValues: {
      roundName: '',
      roundType: RoundType.Equity,
      preMoneyValuation: 0,
      investmentAmount: 0,
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

  const watchPreMoney = watch('preMoneyValuation');
  const watchInvestment = watch('investmentAmount');
  const watchIncludePool = watch('includeOptionPool');
  const watchNewInvestors = watch('newInvestors');

  // Quick dilution calculation
  const { data: quickDilution } = useDilutionCalculation(
    companyId || undefined,
    watchInvestment,
    watchPreMoney,
    !!companyId && watchInvestment > 0 && watchPreMoney > 0
  );

  // Auto-calculate total investment from investors
  useEffect(() => {
    if (watchNewInvestors.length > 0) {
      const total = watchNewInvestors.reduce((sum, inv) => sum + (inv.investmentAmount || 0), 0);
      if (total > 0 && total !== watchInvestment) {
        setValue('investmentAmount', total);
      }
    }
  }, [watchNewInvestors, watchInvestment, setValue]);

  // Reset form when modal opens/closes
  useEffect(() => {
    if (isOpen) {
      setResult(null);
      setShowResults(false);
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
      investmentAmount: data.investmentAmount,
      roundName: data.roundName,
      roundType: data.roundType,
      newInvestors: data.newInvestors,
      includeOptionPool: data.includeOptionPool,
      optionPoolPercentage: data.optionPoolPercentage || 0,
      optionPoolPreMoney: data.optionPoolPreMoney || true,
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

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl max-w-4xl w-full mx-4 max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex justify-between items-center p-6 border-b border-gray-200">
          <div>
            <h2 className="text-xl font-bold text-gray-900">
              {showResults ? 'Resultado da Simulação' : 'Simular Rodada de Investimento'}
            </h2>
            <p className="text-sm text-gray-500 mt-1">
              {showResults
                ? 'Analise o impacto da rodada no Cap Table'
                : 'Configure os parâmetros da rodada para simular'}
            </p>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {showResults && result ? (
            <SimulationResultsView result={result} onBack={handleBack} />
          ) : (
            <form id="simulation-form" onSubmit={handleSubmit(onSubmit)} className="space-y-6">
              {/* Round Info */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Nome da Rodada *
                  </label>
                  <div className="relative">
                    <input
                      {...register('roundName')}
                      list="round-suggestions"
                      placeholder="Ex: Series A"
                      className="input w-full"
                    />
                    <datalist id="round-suggestions">
                      {roundNameSuggestions.map((name) => (
                        <option key={name} value={name} />
                      ))}
                    </datalist>
                  </div>
                  {errors.roundName && (
                    <p className="text-sm text-red-500 mt-1">{errors.roundName.message}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Tipo da Rodada *
                  </label>
                  <select {...register('roundType', { valueAsNumber: true })} className="input w-full">
                    {Object.entries(RoundType)
                      .filter(([key]) => isNaN(Number(key)))
                      .map(([key, value]) => (
                        <option key={key} value={value}>
                          {getRoundTypeLabel(value as RoundType)}
                        </option>
                      ))}
                  </select>
                </div>
              </div>

              {/* Valuation */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Valuation Pre-Money (R$) *
                  </label>
                  <input
                    {...register('preMoneyValuation')}
                    type="number"
                    min="0"
                    step="1000"
                    placeholder="10.000.000"
                    className="input w-full"
                  />
                  {errors.preMoneyValuation && (
                    <p className="text-sm text-red-500 mt-1">{errors.preMoneyValuation.message}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Valor Total do Investimento (R$)
                  </label>
                  <input
                    {...register('investmentAmount')}
                    type="number"
                    min="0"
                    step="1000"
                    placeholder="2.000.000"
                    className="input w-full bg-gray-50"
                    readOnly
                  />
                  <p className="text-xs text-gray-500 mt-1">Calculado pela soma dos investidores</p>
                </div>
              </div>

              {/* Quick Dilution Preview */}
              {quickDilution && (
                <Card className="p-4 bg-gray-50 border-gray-200">
                  <div className="flex items-center gap-3">
                    <div className="p-2 bg-orange-100 rounded-lg">
                      <TrendingDown className="w-5 h-5 text-orange-600" />
                    </div>
                    <div>
                      <p className="text-sm text-gray-600">Diluição estimada</p>
                      <p className="text-lg font-semibold text-orange-600">
                        {formatPercentage(quickDilution.dilutionPercentage)}
                      </p>
                    </div>
                    <div className="ml-auto text-right">
                      <p className="text-sm text-gray-600">Post-money</p>
                      <p className="text-lg font-semibold text-gray-900">
                        {formatCurrency(quickDilution.postMoneyValuation)}
                      </p>
                    </div>
                  </div>
                </Card>
              )}

              {/* Investors */}
              <div>
                <div className="flex items-center justify-between mb-3">
                  <label className="block text-sm font-medium text-gray-700">
                    Novos Investidores *
                  </label>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => append({ name: '', investmentAmount: 0, email: '', document: '' })}
                    icon={<Plus className="w-4 h-4" />}
                  >
                    Adicionar
                  </Button>
                </div>

                <div className="space-y-3">
                  {fields.map((field, index) => (
                    <div key={field.id} className="flex gap-3 items-start p-3 bg-gray-50 rounded-lg">
                      <div className="flex-1 grid grid-cols-1 md:grid-cols-3 gap-3">
                        <div>
                          <input
                            {...register(`newInvestors.${index}.name`)}
                            placeholder="Nome do investidor"
                            className="input w-full"
                          />
                          {errors.newInvestors?.[index]?.name && (
                            <p className="text-xs text-red-500 mt-1">
                              {errors.newInvestors[index]?.name?.message}
                            </p>
                          )}
                        </div>
                        <div>
                          <input
                            {...register(`newInvestors.${index}.investmentAmount`)}
                            type="number"
                            min="0"
                            step="1000"
                            placeholder="Valor (R$)"
                            className="input w-full"
                          />
                        </div>
                        <div>
                          <input
                            {...register(`newInvestors.${index}.email`)}
                            type="email"
                            placeholder="E-mail (opcional)"
                            className="input w-full"
                          />
                        </div>
                      </div>
                      {fields.length > 1 && (
                        <button
                          type="button"
                          onClick={() => remove(index)}
                          className="p-2 text-red-500 hover:bg-red-50 rounded-lg"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      )}
                    </div>
                  ))}
                </div>
                {errors.newInvestors?.message && (
                  <p className="text-sm text-red-500 mt-2">{errors.newInvestors.message}</p>
                )}
              </div>

              {/* Option Pool */}
              <div className="border-t border-gray-200 pt-4">
                <div className="flex items-center gap-3 mb-4">
                  <input
                    {...register('includeOptionPool')}
                    type="checkbox"
                    id="includeOptionPool"
                    className="w-4 h-4 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                  />
                  <label htmlFor="includeOptionPool" className="text-sm font-medium text-gray-700">
                    Incluir Pool de Opções
                  </label>
                </div>

                {watchIncludePool && (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pl-7">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Percentual do Pool (%)
                      </label>
                      <input
                        {...register('optionPoolPercentage')}
                        type="number"
                        min="0"
                        max="50"
                        step="0.5"
                        className="input w-full"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Momento da Diluição
                      </label>
                      <select {...register('optionPoolPreMoney')} className="input w-full">
                        <option value="true">Pre-Money (diluição antes)</option>
                        <option value="">Post-Money (diluição depois)</option>
                      </select>
                    </div>
                  </div>
                )}
              </div>
            </form>
          )}
        </div>

        {/* Footer */}
        <div className="flex justify-end gap-3 p-6 border-t border-gray-200">
          {showResults ? (
            <>
              <Button variant="secondary" onClick={handleBack}>
                Nova Simulação
              </Button>
              <Button variant="primary" onClick={onClose}>
                Fechar
              </Button>
            </>
          ) : (
            <>
              <Button variant="secondary" onClick={onClose}>
                Cancelar
              </Button>
              <Button
                type="submit"
                form="simulation-form"
                loading={simulateRound.isPending}
                icon={<Calculator className="w-4 h-4" />}
              >
                Simular Rodada
              </Button>
            </>
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
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Card className="p-4 text-center">
          <p className="text-sm text-gray-500">Post-Money</p>
          <p className="text-xl font-bold text-gray-900">
            {formatCurrency(result.postMoneyValuation)}
          </p>
        </Card>
        <Card className="p-4 text-center">
          <p className="text-sm text-gray-500">Preço/Ação</p>
          <p className="text-xl font-bold text-gray-900">
            {formatCurrency(result.pricePerShare)}
          </p>
        </Card>
        <Card className="p-4 text-center">
          <p className="text-sm text-gray-500">Novas Ações</p>
          <p className="text-xl font-bold text-green-600">
            +{result.newSharesIssued.toLocaleString('pt-BR')}
          </p>
        </Card>
        <Card className="p-4 text-center">
          <p className="text-sm text-gray-500">Diluição Total</p>
          <p className="text-xl font-bold text-orange-600">
            {formatPercentage(result.totalDilution)}
          </p>
        </Card>
      </div>

      {/* New Investors */}
      <div>
        <h3 className="text-lg font-semibold text-gray-900 mb-3">Novos Investidores</h3>
        <div className="bg-gray-50 rounded-lg overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-100">
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
                <tr key={idx} className="bg-white">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <DollarSign className="w-4 h-4 text-green-500" />
                      <span className="font-medium text-gray-900">{investor.name}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-right text-gray-900">
                    {formatCurrency(investor.investmentAmount)}
                  </td>
                  <td className="px-4 py-3 text-right text-gray-600">
                    {investor.sharesReceived.toLocaleString('pt-BR')}
                  </td>
                  <td className="px-4 py-3 text-right font-medium text-green-600">
                    {formatPercentage(investor.ownershipPercentage)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Option Pool (if included) */}
      {result.optionPool && (
        <Card className="p-4 bg-purple-50 border-purple-200">
          <h3 className="text-sm font-semibold text-purple-900 mb-2">Pool de Opções</h3>
          <div className="grid grid-cols-3 gap-4 text-sm">
            <div>
              <p className="text-purple-600">Percentual</p>
              <p className="font-semibold text-purple-900">
                {formatPercentage(result.optionPool.percentage)}
              </p>
            </div>
            <div>
              <p className="text-purple-600">Ações Reservadas</p>
              <p className="font-semibold text-purple-900">
                {result.optionPool.shares.toLocaleString('pt-BR')}
              </p>
            </div>
            <div>
              <p className="text-purple-600">Momento</p>
              <p className="font-semibold text-purple-900">
                {result.optionPool.isPreMoney ? 'Pre-Money' : 'Post-Money'}
              </p>
            </div>
          </div>
        </Card>
      )}

      {/* Cap Table Comparison */}
      <div>
        <h3 className="text-lg font-semibold text-gray-900 mb-3">Impacto no Cap Table</h3>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {/* Before */}
          <Card className="p-4">
            <h4 className="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-2">
              <span className="w-3 h-3 rounded-full bg-gray-400"></span>
              Antes da Rodada
            </h4>
            <div className="space-y-2 max-h-48 overflow-y-auto">
              {result.capTableBefore.map((entry, idx) => (
                <div key={idx} className="flex justify-between items-center text-sm">
                  <span className="text-gray-700">{entry.shareholderName}</span>
                  <span className="font-medium text-gray-900">
                    {formatPercentage(entry.ownership)}
                  </span>
                </div>
              ))}
            </div>
            <div className="mt-3 pt-3 border-t border-gray-200 flex justify-between text-sm font-semibold">
              <span className="text-gray-600">Total</span>
              <span className="text-gray-900">{result.sharesBefore.toLocaleString('pt-BR')} ações</span>
            </div>
          </Card>

          {/* After */}
          <Card className="p-4 border-green-200 bg-green-50">
            <h4 className="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-2">
              <span className="w-3 h-3 rounded-full bg-green-500"></span>
              Depois da Rodada
            </h4>
            <div className="space-y-2 max-h-48 overflow-y-auto">
              {result.capTableAfter.map((entry, idx) => (
                <div key={idx} className="flex justify-between items-center text-sm">
                  <div className="flex items-center gap-2">
                    <span className="text-gray-700">{entry.shareholderName}</span>
                    {entry.isNewInvestor && (
                      <span className="text-xs px-1.5 py-0.5 bg-green-100 text-green-700 rounded">
                        Novo
                      </span>
                    )}
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="font-medium text-gray-900">
                      {formatPercentage(entry.ownership)}
                    </span>
                    {entry.dilutionPercentage > 0 && (
                      <span className="text-xs text-orange-600">
                        (-{formatPercentage(entry.dilutionPercentage, 1)})
                      </span>
                    )}
                  </div>
                </div>
              ))}
            </div>
            <div className="mt-3 pt-3 border-t border-green-200 flex justify-between text-sm font-semibold">
              <span className="text-gray-600">Total</span>
              <span className="text-green-700">{result.sharesAfter.toLocaleString('pt-BR')} ações</span>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
