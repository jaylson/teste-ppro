import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  LineChart, Line, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import { TrendingUp, ArrowLeft, TrendingDown, Minus, DollarSign, BarChart2, CheckCircle, Clock } from 'lucide-react';
import { Card, Spinner } from '@/components/ui';
import { useValuations } from '@/hooks';
import { useClientStore } from '@/stores/clientStore';
import {
  valuationMethodLabels,
  valuationEventTypeLabels,
} from '@/types';
import { formatCurrency, formatDate } from '@/utils/format';

// ─── Chart colours ────────────────────────────────────────────────────────────

const CHART_COLORS = ['#3B82F6', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6', '#EC4899', '#06B6D4', '#84CC16'];

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
  color?: 'blue' | 'green' | 'purple' | 'orange';
  trend?: { value: number; label: string };
}) {
  const colors = {
    blue: 'bg-blue-50 text-blue-600',
    green: 'bg-green-50 text-green-600',
    purple: 'bg-purple-50 text-purple-600',
    orange: 'bg-orange-50 text-orange-600',
  };

  return (
    <Card className="p-5">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">{title}</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{value}</p>
          {subtitle && <p className="text-sm text-gray-400 mt-0.5">{subtitle}</p>}
          {trend && (
            <div className={`flex items-center gap-1 mt-2 text-xs font-medium ${trend.value > 0 ? 'text-green-600' : trend.value < 0 ? 'text-red-500' : 'text-gray-400'}`}>
              {trend.value > 0 ? <TrendingUp className="w-3.5 h-3.5" /> : trend.value < 0 ? <TrendingDown className="w-3.5 h-3.5" /> : <Minus className="w-3.5 h-3.5" />}
              {Math.abs(trend.value).toFixed(1)}% {trend.label}
            </div>
          )}
        </div>
        <div className={`w-10 h-10 flex items-center justify-center rounded-xl ${colors[color]}`}>
          {icon}
        </div>
      </div>
    </Card>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function ValuationDashboardPage() {
  const navigate = useNavigate();
  const { selectedCompanyId } = useClientStore();

  const { data, isLoading } = useValuations({
    companyId: selectedCompanyId || undefined,
    page: 1,
    pageSize: 100,
  });

  const valuations = useMemo(() => data?.items ?? [], [data]);

  // ─── KPI calculations ──────────────────────────────────────────────────────

  const kpis = useMemo(() => {
    const approved = valuations
      .filter((v) => v.status === 'approved' && v.valuationAmount != null)
      .sort((a, b) => new Date(b.valuationDate).getTime() - new Date(a.valuationDate).getTime());

    const latest = approved[0] ?? null;
    const previous = approved[1] ?? null;

    const variation =
      latest && previous && previous.valuationAmount
        ? ((latest.valuationAmount! - previous.valuationAmount) / previous.valuationAmount) * 100
        : null;

    return {
      latest,
      previous,
      variation,
      approved: valuations.filter((v) => v.status === 'approved').length,
      pending: valuations.filter((v) => v.status === 'pending_approval').length,
    };
  }, [valuations]);

  // ─── Chart data ────────────────────────────────────────────────────────────

  const historyData = useMemo(
    () =>
      valuations
        .filter((v) => v.status === 'approved' && v.valuationAmount != null)
        .map((v) => ({
          date: formatDate(v.valuationDate),
          valor: v.valuationAmount ?? 0,
          rawDate: new Date(v.valuationDate).getTime(),
        }))
        .sort((a, b) => a.rawDate - b.rawDate),
    [valuations]
  );

  const methodologyData = useMemo(() => {
    const latest = valuations
      .filter((v) => v.status === 'approved')
      .sort((a, b) => new Date(b.valuationDate).getTime() - new Date(a.valuationDate).getTime())[0];
    if (!latest?.methods) return [];
    return latest.methods
      .filter((m) => m.calculatedValue != null)
      .map((m) => ({
        name: valuationMethodLabels[m.methodType] ?? m.methodType,
        valor: m.calculatedValue ?? 0,
        principal: m.isSelected,
      }));
  }, [valuations]);

  const eventDistribution = useMemo(() => {
    const counts: Record<string, number> = {};
    valuations.forEach((v) => {
      const label = valuationEventTypeLabels[v.eventType] ?? v.eventType;
      counts[label] = (counts[label] ?? 0) + 1;
    });
    return Object.entries(counts).map(([name, value]) => ({ name, value }));
  }, [valuations]);

  const timelineData = useMemo(
    () =>
      valuations
        .filter((v) => v.status === 'approved' && v.valuationAmount != null)
        .sort((a, b) => new Date(a.valuationDate).getTime() - new Date(b.valuationDate).getTime())
        .slice(-8),
    [valuations]
  );

  if (!selectedCompanyId) {
    return (
      <div className="text-center py-20 text-gray-400">
        <p>Selecione uma empresa para ver o dashboard.</p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <Spinner className="w-8 h-8" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/valuations')}
          className="p-2 rounded-xl hover:bg-gray-100 text-gray-500"
        >
          <ArrowLeft className="w-5 h-5" />
        </button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard de Valuation</h1>
          <p className="text-sm text-gray-400 mt-0.5">Histórico e análise comparativa de valuations aprovados</p>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <KPICard
          title="Valuation Atual"
          value={kpis.latest ? formatCurrency(kpis.latest.valuationAmount!) : '—'}
          subtitle={kpis.latest ? `Aprovado em ${formatDate(kpis.latest.approvedAt ?? kpis.latest.valuationDate)}` : 'Nenhum aprovado'}
          icon={<TrendingUp className="w-5 h-5" />}
          color="blue"
          trend={kpis.variation != null ? { value: kpis.variation, label: 'vs anterior' } : undefined}
        />
        <KPICard
          title="Variação vs Anterior"
          value={kpis.variation != null ? `${kpis.variation >= 0 ? '+' : ''}${kpis.variation.toFixed(1)}%` : '—'}
          subtitle={kpis.previous ? `Anterior: ${formatCurrency(kpis.previous.valuationAmount!)}` : 'Sem comparativo'}
          icon={kpis.variation != null && kpis.variation >= 0 ? <TrendingUp className="w-5 h-5" /> : <TrendingDown className="w-5 h-5" />}
          color={kpis.variation != null && kpis.variation >= 0 ? 'green' : 'orange'}
        />
        <KPICard
          title="Price per Share"
          value={kpis.latest?.pricePerShare != null ? formatCurrency(kpis.latest.pricePerShare) : '—'}
          subtitle={kpis.latest ? `${new Intl.NumberFormat('pt-BR').format(kpis.latest.totalShares)} ações` : undefined}
          icon={<DollarSign className="w-5 h-5" />}
          color="purple"
        />
        <KPICard
          title="Valuations"
          value={`${kpis.approved}`}
          subtitle={`${kpis.pending} aguardando aprovação`}
          icon={<BarChart2 className="w-5 h-5" />}
          color="orange"
          trend={undefined}
        />
      </div>

      {/* Charts row 1 */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        {/* History */}
        <Card className="p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4 flex items-center gap-2">
            <CheckCircle className="w-4 h-4 text-blue-500" />
            Histórico de Valuations Aprovados
          </h3>
          {historyData.length < 2 ? (
            <p className="text-sm text-gray-400 text-center py-12">Dados insuficientes para o gráfico.</p>
          ) : (
            <ResponsiveContainer width="100%" height={220}>
              <LineChart data={historyData} margin={{ top: 5, right: 15, left: 0, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#F3F4F6" />
                <XAxis dataKey="date" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `R$${(v / 1e6).toFixed(1)}M`} />
                <Tooltip formatter={(v: number) => formatCurrency(v)} />
                <Line
                  type="monotone"
                  dataKey="valor"
                  stroke="#3B82F6"
                  strokeWidth={2}
                  dot={{ r: 4, fill: '#3B82F6' }}
                  activeDot={{ r: 6 }}
                />
              </LineChart>
            </ResponsiveContainer>
          )}
        </Card>

        {/* Methodology comparison */}
        <Card className="p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4 flex items-center gap-2">
            <BarChart2 className="w-4 h-4 text-green-500" />
            Comparativo de Metodologias (último valuation aprovado)
          </h3>
          {methodologyData.length === 0 ? (
            <p className="text-sm text-gray-400 text-center py-12">Sem metodologias calculadas.</p>
          ) : (
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={methodologyData} margin={{ top: 5, right: 15, left: 0, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#F3F4F6" />
                <XAxis dataKey="name" tick={{ fontSize: 10 }} />
                <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `R$${(v / 1e6).toFixed(0)}M`} />
                <Tooltip formatter={(v: number) => formatCurrency(v)} />
                <Bar dataKey="valor" radius={[4, 4, 0, 0]}>
                  {methodologyData.map((entry, index) => (
                    <Cell
                      key={`cell-${index}`}
                      fill={entry.principal ? '#10B981' : '#93C5FD'}
                    />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          )}
          {methodologyData.some((m) => m.principal) && (
            <p className="text-xs text-green-600 mt-2">🟢 Metodologia principal selecionada em verde</p>
          )}
        </Card>
      </div>

      {/* Charts row 2 */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        {/* Timeline */}
        <Card className="p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4 flex items-center gap-2">
            <Clock className="w-4 h-4 text-purple-500" />
            Timeline de Eventos
          </h3>
          {timelineData.length === 0 ? (
            <p className="text-sm text-gray-400 text-center py-12">Nenhum valuation aprovado.</p>
          ) : (
            <div className="relative pl-6">
              <div className="absolute left-2.5 top-0 bottom-0 w-0.5 bg-gray-200" />
              <div className="space-y-4">
                {timelineData.map((v, i) => (
                  <div key={v.id} className="relative">
                    <div
                      className={`absolute -left-4 w-3 h-3 rounded-full border-2 border-white ${i === timelineData.length - 1 ? 'bg-blue-500' : 'bg-gray-300'}`}
                    />
                    <div className="ml-2">
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-semibold text-gray-700">
                          {formatCurrency(v.valuationAmount!)}
                        </span>
                        <span className="text-xs px-1.5 py-0.5 bg-gray-100 text-gray-500 rounded">
                          {valuationEventTypeLabels[v.eventType] ?? v.eventType}
                        </span>
                      </div>
                      <p className="text-xs text-gray-400 mt-0.5">{formatDate(v.valuationDate)}</p>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </Card>

        {/* Distribution by event */}
        <Card className="p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4 flex items-center gap-2">
            <TrendingUp className="w-4 h-4 text-orange-500" />
            Distribuição por Tipo de Evento
          </h3>
          {eventDistribution.length === 0 ? (
            <p className="text-sm text-gray-400 text-center py-12">Nenhum dado disponível.</p>
          ) : (
            <ResponsiveContainer width="100%" height={220}>
              <PieChart>
                <Pie
                  data={eventDistribution}
                  cx="50%"
                  cy="50%"
                  outerRadius={80}
                  dataKey="value"
                  label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                  labelLine
                >
                  {eventDistribution.map((_, index) => (
                    <Cell key={`cell-${index}`} fill={CHART_COLORS[index % CHART_COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          )}
        </Card>
      </div>
    </div>
  );
}
