import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, Send, CheckCircle, Lock } from 'lucide-react';
import { Button, Card, Spinner, Badge } from '@/components/ui';
import {
  useFinancialPeriods,
  useSubmitFinancialPeriod,
  useApproveFinancialPeriod,
  useLockFinancialPeriod,
  useUpsertRevenue,
  useUpsertCashBurn,
  useUpsertUnitEconomics,
  useUpsertProfitability,
} from '@/hooks';
import { useClientStore } from '@/stores/clientStore';
import {
  financialPeriodStatusLabels,
  financialPeriodStatusColors,
  monthNames,
} from '@/types';

function MetricInput({
  label,
  value,
  onChange,
  prefix = 'R$',
  disabled = false,
  type = 'number',
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
  prefix?: string;
  disabled?: boolean;
  type?: string;
}) {
  return (
    <div>
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      <div className="flex items-center border border-gray-200 rounded-lg overflow-hidden focus-within:ring-2 focus-within:ring-blue-500">
        {prefix && (
          <span className="px-2 py-2 text-xs text-gray-400 bg-gray-50 border-r border-gray-200">
            {prefix}
          </span>
        )}
        <input
          type={type}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          disabled={disabled}
          className="flex-1 px-3 py-2 text-sm text-gray-900 bg-transparent focus:outline-none disabled:cursor-not-allowed disabled:text-gray-400"
        />
      </div>
    </div>
  );
}

function toNum(v: string): number | undefined {
  const n = parseFloat(v);
  return isNaN(n) ? undefined : n;
}

function fromNum(v: number | null | undefined): string {
  return v != null ? String(v) : '';
}

export default function FinancialPeriodPage() {
  const { year, month } = useParams<{ year: string; month: string }>();
  const navigate = useNavigate();
  const { selectedCompanyId } = useClientStore();

  // Find by year+month from list (or use a dedicated endpoint if available)
  const filters = {
    companyId: selectedCompanyId || undefined,
    year: Number(year),
    pageSize: 12,
  };
  const { data: list } = useFinancialPeriods(filters);
  const period = list?.items.find(
    (p) => p.year === Number(year) && p.month === Number(month)
  );

  const submitPeriod = useSubmitFinancialPeriod();
  const approvePeriod = useApproveFinancialPeriod();
  const lockPeriod = useLockFinancialPeriod();
  const upsertRevenue = useUpsertRevenue();
  const upsertCashBurn = useUpsertCashBurn();
  const upsertUnitEconomics = useUpsertUnitEconomics();
  const upsertProfitability = useUpsertProfitability();

  const locked = period?.status === 'locked' || period?.status === 'approved';

  // Revenue state
  const [revenue, setRevenue] = useState({ grossRevenue: '', netRevenue: '', mrr: '' });
  // Cash burn state
  const [cashBurn, setCashBurn] = useState({ cashBalance: '', burnRate: '' });
  // Unit economics state
  const [unitEcon, setUnitEcon] = useState({
    customerCount: '', churnRate: '', cac: '', ltv: '', nps: '',
  });
  // Profitability state
  const [profit, setProfit] = useState({ ebitda: '', netIncome: '' });

  // Populate from existing metrics
  useEffect(() => {
    if (!period?.metrics) return;
    const m = period.metrics;
    setRevenue({
      grossRevenue: fromNum(m.grossRevenue),
      netRevenue: fromNum(m.netRevenue),
      mrr: fromNum(m.mrr),
    });
    setCashBurn({
      cashBalance: fromNum(m.cashBalance),
      burnRate: fromNum(m.burnRate),
    });
    setUnitEcon({
      customerCount: fromNum(m.customerCount),
      churnRate: fromNum(m.churnRate),
      cac: fromNum(m.cac),
      ltv: fromNum(m.ltv),
      nps: fromNum(m.nps),
    });
    setProfit({
      ebitda: fromNum(m.ebitda),
      netIncome: fromNum(m.netIncome),
    });
  }, [period?.id]);

  if (!period && !list) {
    return (
      <div className="flex justify-center py-20">
        <Spinner className="w-8 h-8" />
      </div>
    );
  }

  if (!period) {
    return (
      <div className="text-center py-20 text-gray-400">
        <p>Período {monthNames[Number(month) - 1]}/{year} não encontrado.</p>
        <Button variant="secondary" size="sm" className="mt-4" onClick={() => navigate('/financial')}>
          Voltar
        </Button>
      </div>
    );
  }

  const label = `${monthNames[period.month - 1]}/${period.year}`;

  return (
    <div className="max-w-3xl space-y-6 animate-fade-in">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/financial')}
          className="p-2 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors"
        >
          <ArrowLeft size={18} />
        </button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h1 className="text-xl font-bold text-gray-900">Período {label}</h1>
            <Badge className={financialPeriodStatusColors[period.status] ?? ''}>
              {financialPeriodStatusLabels[period.status] ?? period.status}
            </Badge>
          </div>
        </div>
        <div className="flex gap-2">
          {period.status === 'draft' && (
            <Button
              icon={<Send size={14} />}
              size="sm"
              onClick={() => submitPeriod.mutate(period.id)}
              loading={submitPeriod.isPending}
            >
              Submeter
            </Button>
          )}
          {period.status === 'submitted' && (
            <Button
              icon={<CheckCircle size={14} />}
              size="sm"
              variant="success"
              onClick={() => approvePeriod.mutate(period.id)}
              loading={approvePeriod.isPending}
            >
              Aprovar
            </Button>
          )}
          {period.status === 'approved' && (
            <Button
              icon={<Lock size={14} />}
              size="sm"
              variant="secondary"
              onClick={() => lockPeriod.mutate(period.id)}
              loading={lockPeriod.isPending}
            >
              Bloquear
            </Button>
          )}
        </div>
      </div>

      {/* Revenue */}
      <Card>
        <div className="p-5 space-y-4">
          <h2 className="text-sm font-semibold text-gray-800">Receita</h2>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <MetricInput
              label="Receita Bruta"
              value={revenue.grossRevenue}
              onChange={(v) => setRevenue((r) => ({ ...r, grossRevenue: v }))}
              disabled={locked}
            />
            <MetricInput
              label="Receita Líquida"
              value={revenue.netRevenue}
              onChange={(v) => setRevenue((r) => ({ ...r, netRevenue: v }))}
              disabled={locked}
            />
            <MetricInput
              label="MRR"
              value={revenue.mrr}
              onChange={(v) => setRevenue((r) => ({ ...r, mrr: v }))}
              disabled={locked}
            />
          </div>
          {!locked && (
            <div className="flex justify-end">
              <Button
                size="sm"
                variant="secondary"
                onClick={() =>
                  upsertRevenue.mutate({
                    id: period.id,
                    data: {
                      grossRevenue: toNum(revenue.grossRevenue),
                      netRevenue: toNum(revenue.netRevenue),
                      mrr: toNum(revenue.mrr),
                    },
                  })
                }
              >
                Salvar Receita
              </Button>
            </div>
          )}
        </div>
      </Card>

      {/* Cash & Burn */}
      <Card>
        <div className="p-5 space-y-4">
          <h2 className="text-sm font-semibold text-gray-800">Caixa & Burn</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <MetricInput
              label="Saldo de Caixa"
              value={cashBurn.cashBalance}
              onChange={(v) => setCashBurn((c) => ({ ...c, cashBalance: v }))}
              disabled={locked}
            />
            <MetricInput
              label="Burn Rate (mensal)"
              value={cashBurn.burnRate}
              onChange={(v) => setCashBurn((c) => ({ ...c, burnRate: v }))}
              disabled={locked}
            />
          </div>
          {period.metrics?.runwayMonths != null && (
            <p className="text-sm text-gray-500">
              Runway calculado:{' '}
              <span className="font-medium">{period.metrics.runwayMonths.toFixed(1)} meses</span>
            </p>
          )}
          {!locked && (
            <div className="flex justify-end">
              <Button
                size="sm"
                variant="secondary"
                onClick={() =>
                  upsertCashBurn.mutate({
                    id: period.id,
                    data: {
                      cashBalance: toNum(cashBurn.cashBalance),
                      burnRate: toNum(cashBurn.burnRate),
                    },
                  })
                }
              >
                Salvar Caixa/Burn
              </Button>
            </div>
          )}
        </div>
      </Card>

      {/* Unit Economics */}
      <Card>
        <div className="p-5 space-y-4">
          <h2 className="text-sm font-semibold text-gray-800">Unit Economics</h2>
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
            <MetricInput
              label="Clientes Ativos"
              value={unitEcon.customerCount}
              onChange={(v) => setUnitEcon((u) => ({ ...u, customerCount: v }))}
              prefix="#"
              disabled={locked}
            />
            <MetricInput
              label="Churn Rate (%)"
              value={unitEcon.churnRate}
              onChange={(v) => setUnitEcon((u) => ({ ...u, churnRate: v }))}
              prefix="%"
              disabled={locked}
            />
            <MetricInput
              label="CAC"
              value={unitEcon.cac}
              onChange={(v) => setUnitEcon((u) => ({ ...u, cac: v }))}
              disabled={locked}
            />
            <MetricInput
              label="LTV"
              value={unitEcon.ltv}
              onChange={(v) => setUnitEcon((u) => ({ ...u, ltv: v }))}
              disabled={locked}
            />
            <MetricInput
              label="NPS"
              value={unitEcon.nps}
              onChange={(v) => setUnitEcon((u) => ({ ...u, nps: v }))}
              prefix="pts"
              disabled={locked}
            />
          </div>
          {!locked && (
            <div className="flex justify-end">
              <Button
                size="sm"
                variant="secondary"
                onClick={() =>
                  upsertUnitEconomics.mutate({
                    id: period.id,
                    data: {
                      customerCount: toNum(unitEcon.customerCount) as number | undefined,
                      churnRate: toNum(unitEcon.churnRate),
                      cac: toNum(unitEcon.cac),
                      ltv: toNum(unitEcon.ltv),
                      nps: toNum(unitEcon.nps) as number | undefined,
                    },
                  })
                }
              >
                Salvar Unit Economics
              </Button>
            </div>
          )}
        </div>
      </Card>

      {/* Profitability */}
      <Card>
        <div className="p-5 space-y-4">
          <h2 className="text-sm font-semibold text-gray-800">Lucratividade</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <MetricInput
              label="EBITDA"
              value={profit.ebitda}
              onChange={(v) => setProfit((p) => ({ ...p, ebitda: v }))}
              disabled={locked}
            />
            <MetricInput
              label="Lucro Líquido"
              value={profit.netIncome}
              onChange={(v) => setProfit((p) => ({ ...p, netIncome: v }))}
              disabled={locked}
            />
          </div>
          {period.metrics?.ebitdaMargin != null && (
            <p className="text-sm text-gray-500">
              Margem EBITDA:{' '}
              <span className="font-medium">{period.metrics.ebitdaMargin.toFixed(1)}%</span>
            </p>
          )}
          {!locked && (
            <div className="flex justify-end">
              <Button
                size="sm"
                variant="secondary"
                onClick={() =>
                  upsertProfitability.mutate({
                    id: period.id,
                    data: {
                      ebitda: toNum(profit.ebitda),
                      netIncome: toNum(profit.netIncome),
                    },
                  })
                }
              >
                Salvar Lucratividade
              </Button>
            </div>
          )}
        </div>
      </Card>
    </div>
  );
}
