import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  LineChart, Line, BarChart, Bar, AreaChart, Area, XAxis, YAxis,
  CartesianGrid, Tooltip, Legend, ResponsiveContainer, ReferenceLine,
} from 'recharts';
import {
  ArrowLeft, TrendingUp, TrendingDown, DollarSign, Flame, Users, BarChart2,
} from 'lucide-react';
import { Card, Spinner } from '@/components/ui';
import { useFinancialDashboard } from '@/hooks';
import { useClientStore } from '@/stores/clientStore';
import { monthNames } from '@/types';
import { formatCurrency } from '@/utils/format';

// ─── KPI Card ─────────────────────────────────────────────────────────────────

function KPICard({
  title,
  value,
  subtitle,
  icon,
  color = 'blue',
  trend,
}: {
  title: string;
  value: string;
  subtitle?: string;
  icon: React.ReactNode;
  color?: 'blue' | 'green' | 'red' | 'orange' | 'yellow';
  trend?: number | null;
}) {
  const bg = {
    blue: 'bg-blue-50 text-blue-600',
    green: 'bg-green-50 text-green-600',
    red: 'bg-red-50 text-red-600',
    orange: 'bg-orange-50 text-orange-600',
    yellow: 'bg-yellow-50 text-yellow-600',
  };

  return (
    <Card className="p-5">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">{title}</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{value}</p>
          {subtitle && <p className="text-sm text-gray-400 mt-0.5">{subtitle}</p>}
          {trend != null && (
            <div
              className={`flex items-center gap-1 mt-2 text-xs font-medium ${
                trend > 0 ? 'text-green-600' : trend < 0 ? 'text-red-500' : 'text-gray-400'
              }`}
            >
              {trend > 0 ? <TrendingUp className="w-3.5 h-3.5" /> : <TrendingDown className="w-3.5 h-3.5" />}
              {Math.abs(trend).toFixed(1)}% MoM
            </div>
          )}
        </div>
        <div className={`w-10 h-10 flex items-center justify-center rounded-xl ${bg[color]}`}>
          {icon}
        </div>
      </div>
    </Card>
  );
}

// ─── Runway Gauge ─────────────────────────────────────────────────────────────

function RunwayGauge({ months, status }: { months: number | null | undefined; status: string | null | undefined }) {
  const capped = Math.min(months ?? 0, 24);
  const pct = (capped / 24) * 100;

  const colorClass =
    status === 'green' ? 'bg-green-500'
    : status === 'yellow' ? 'bg-yellow-500'
    : 'bg-red-500';

  const label =
    status === 'green' ? 'Saudável (>12 meses)'
    : status === 'yellow' ? 'Atenção (6-12 meses)'
    : 'Crítico (<6 meses)';

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium text-gray-700">Runway atual</span>
        <span className="text-sm font-bold text-gray-900">{months != null ? `${months.toFixed(0)} meses` : '—'}</span>
      </div>
      <div className="h-3 bg-gray-100 rounded-full overflow-hidden">
        <div
          className={`h-full rounded-full transition-all duration-500 ${colorClass}`}
          style={{ width: `${pct}%` }}
        />
      </div>
      <p className={`text-xs font-medium ${status === 'green' ? 'text-green-600' : status === 'yellow' ? 'text-yellow-600' : 'text-red-600'}`}>
        {label}
      </p>
    </div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function FinancialDashboardPage() {
  const navigate = useNavigate();
  const { selectedCompanyId } = useClientStore();
  const [year, setYear] = useState(new Date().getFullYear());

  const { data: dashboard, isLoading } = useFinancialDashboard(
    selectedCompanyId || '',
    year,
  );

  // ─── Processed chart data ────────────────────────────────────────────────

  const chartData = useMemo(() => {
    const periods = dashboard?.periods ?? [];
    // Sort by month ascending
    const sorted = [...periods].sort((a, b) => a.month - b.month);
    return sorted.map((p) => ({
      month: monthNames[p.month - 1] ?? `M${p.month}`,
      mrr: p.metrics?.mrr ?? null,
      arr: p.metrics?.arr ?? null,
      burnRate: p.metrics?.burnRate ?? null,
      customerCount: p.metrics?.customerCount ?? null,
      cac: p.metrics?.cac ?? null,
      ltv: p.metrics?.ltv ?? null,
    }));
  }, [dashboard]);

  // ─── KPIs from latest approved period ───────────────────────────────────

  const kpis = useMemo(() => {
    const trend = dashboard?.trend ?? null;
    const periods = (dashboard?.periods ?? [])
      .filter((p) => p.status === 'approved' || p.status === 'locked')
      .sort((a, b) => b.month - a.month);
    const latest = periods[0]?.metrics ?? null;
    const previous = periods[1]?.metrics ?? null;

    const burnVariation =
      latest?.burnRate && previous?.burnRate
        ? ((latest.burnRate - previous.burnRate) / previous.burnRate) * 100
        : null;

    return {
      mrr: latest?.mrr ?? null,
      arr: latest?.arr ?? (latest?.mrr != null ? latest.mrr * 12 : null),
      burnRate: latest?.burnRate ?? null,
      burnVariation,
      runwayMonths: latest?.runwayMonths ?? trend?.runwayMonths ?? null,
      runwayStatus: latest?.runwayStatus ?? trend?.runwayStatus ?? null,
      mrrGrowthPercent: trend?.mrrGrowthPercent ?? null,
    };
  }, [dashboard]);

  if (!selectedCompanyId) {
    return (
      <div className="text-center py-20 text-gray-400">
        <p>Selecione uma empresa para ver o dashboard.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/financial')}
          className="p-2 rounded-xl hover:bg-gray-100 text-gray-500"
        >
          <ArrowLeft className="w-5 h-5" />
        </button>
        <div className="flex-1">
          <h1 className="text-2xl font-bold text-gray-900">Dashboard Financeiro</h1>
          <p className="text-sm text-gray-400 mt-0.5">Métricas e tendências ao longo do ano</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={() => setYear((y) => y - 1)}
            className="p-2 rounded-lg hover:bg-gray-100 text-gray-500"
          >‹</button>
          <span className="text-sm font-semibold text-gray-700 w-12 text-center">{year}</span>
          <button
            onClick={() => setYear((y) => y + 1)}
            className="p-2 rounded-lg hover:bg-gray-100 text-gray-500"
          >›</button>
        </div>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-20">
          <Spinner className="w-8 h-8" />
        </div>
      ) : (
        <>
          {/* KPI Cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
            <KPICard
              title="MRR Atual"
              value={kpis.mrr != null ? formatCurrency(kpis.mrr) : '—'}
              subtitle="Receita Recorrente Mensal"
              icon={<DollarSign className="w-5 h-5" />}
              color="blue"
              trend={kpis.mrrGrowthPercent}
            />
            <KPICard
              title="ARR Projetado"
              value={kpis.arr != null ? formatCurrency(kpis.arr) : '—'}
              subtitle="MRR × 12"
              icon={<TrendingUp className="w-5 h-5" />}
              color="green"
            />
            <KPICard
              title="Burn Rate"
              value={kpis.burnRate != null ? formatCurrency(kpis.burnRate) : '—'}
              subtitle="Gasto mensal líquido"
              icon={<Flame className="w-5 h-5" />}
              color={kpis.burnVariation != null && kpis.burnVariation > 10 ? 'red' : 'orange'}
              trend={kpis.burnVariation}
            />
            <Card className="p-5">
              <div className="flex items-start justify-between mb-3">
                <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">Runway</p>
                <div className="w-10 h-10 flex items-center justify-center rounded-xl bg-yellow-50 text-yellow-600">
                  <BarChart2 className="w-5 h-5" />
                </div>
              </div>
              <RunwayGauge months={kpis.runwayMonths} status={kpis.runwayStatus} />
            </Card>
          </div>

          {/* Charts row 1 */}
          <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
            {/* MRR / ARR */}
            <Card className="p-5">
              <h3 className="text-sm font-semibold text-gray-700 mb-4">MRR & ARR — {year}</h3>
              {chartData.length === 0 ? (
                <p className="text-sm text-gray-400 text-center py-12">Sem dados para o período.</p>
              ) : (
                <ResponsiveContainer width="100%" height={220}>
                  <LineChart data={chartData} margin={{ top: 5, right: 15, left: 0, bottom: 5 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#F3F4F6" />
                    <XAxis dataKey="month" tick={{ fontSize: 11 }} />
                    <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `R$${(v / 1e3).toFixed(0)}K`} />
                    <Tooltip formatter={(v: number) => formatCurrency(v)} />
                    <Legend />
                    <Line type="monotone" dataKey="mrr" name="MRR" stroke="#3B82F6" strokeWidth={2} dot={{ r: 3 }} connectNulls />
                    <Line type="monotone" dataKey="arr" name="ARR" stroke="#10B981" strokeWidth={2} dot={{ r: 3 }} connectNulls />
                  </LineChart>
                </ResponsiveContainer>
              )}
            </Card>

            {/* Burn Rate */}
            <Card className="p-5">
              <h3 className="text-sm font-semibold text-gray-700 mb-4">Burn Rate — {year}</h3>
              {chartData.filter((d) => d.burnRate != null).length === 0 ? (
                <p className="text-sm text-gray-400 text-center py-12">Sem dados para o período.</p>
              ) : (
                <ResponsiveContainer width="100%" height={220}>
                  <BarChart data={chartData} margin={{ top: 5, right: 15, left: 0, bottom: 5 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#F3F4F6" />
                    <XAxis dataKey="month" tick={{ fontSize: 11 }} />
                    <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `R$${(v / 1e3).toFixed(0)}K`} />
                    <Tooltip formatter={(v: number) => formatCurrency(v)} />
                    {kpis.burnRate != null && (
                      <ReferenceLine y={kpis.burnRate} stroke="#EF4444" strokeDasharray="4 2" label={{ value: 'Atual', position: 'right', fontSize: 10, fill: '#EF4444' }} />
                    )}
                    <Bar dataKey="burnRate" name="Burn Rate" fill="#F87171" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </Card>
          </div>

          {/* Charts row 2 */}
          <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
            {/* Customer Growth */}
            <Card className="p-5">
              <h3 className="text-sm font-semibold text-gray-700 mb-4 flex items-center gap-2">
                <Users className="w-4 h-4 text-purple-500" />
                Crescimento de Clientes — {year}
              </h3>
              {chartData.filter((d) => d.customerCount != null).length === 0 ? (
                <p className="text-sm text-gray-400 text-center py-12">Sem dados para o período.</p>
              ) : (
                <ResponsiveContainer width="100%" height={220}>
                  <AreaChart data={chartData} margin={{ top: 5, right: 15, left: 0, bottom: 5 }}>
                    <defs>
                      <linearGradient id="colorCustomers" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="#8B5CF6" stopOpacity={0.3} />
                        <stop offset="95%" stopColor="#8B5CF6" stopOpacity={0} />
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="#F3F4F6" />
                    <XAxis dataKey="month" tick={{ fontSize: 11 }} />
                    <YAxis tick={{ fontSize: 11 }} />
                    <Tooltip />
                    <Area
                      type="monotone"
                      dataKey="customerCount"
                      name="Clientes"
                      stroke="#8B5CF6"
                      strokeWidth={2}
                      fill="url(#colorCustomers)"
                      connectNulls
                    />
                  </AreaChart>
                </ResponsiveContainer>
              )}
            </Card>

            {/* CAC vs LTV */}
            <Card className="p-5">
              <h3 className="text-sm font-semibold text-gray-700 mb-4 flex items-center gap-2">
                <BarChart2 className="w-4 h-4 text-orange-500" />
                CAC vs LTV — {year}
              </h3>
              {chartData.filter((d) => d.cac != null || d.ltv != null).length === 0 ? (
                <p className="text-sm text-gray-400 text-center py-12">Sem dados para o período.</p>
              ) : (
                <ResponsiveContainer width="100%" height={220}>
                  <BarChart data={chartData} margin={{ top: 5, right: 15, left: 0, bottom: 5 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#F3F4F6" />
                    <XAxis dataKey="month" tick={{ fontSize: 11 }} />
                    <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `R$${(v / 1e3).toFixed(0)}K`} />
                    <Tooltip formatter={(v: number) => formatCurrency(v)} />
                    <Legend />
                    <Bar dataKey="cac" name="CAC" fill="#F59E0B" radius={[4, 4, 0, 0]} />
                    <Bar dataKey="ltv" name="LTV" fill="#10B981" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </Card>
          </div>
        </>
      )}
    </div>
  );
}
