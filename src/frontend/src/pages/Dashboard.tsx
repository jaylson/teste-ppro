import { useQuery } from '@tanstack/react-query';
import {
  TrendingUp,
  Users,
  AlertCircle,
  FileText,
  ArrowUpRight,
  Plus,
  Download,
} from 'lucide-react';
import { Button, Card, StatCard, Badge, Avatar } from '@/components/ui';
import { useAuthStore } from '@/stores/authStore';
import { useClientStore } from '@/stores/clientStore';
import { useValuations } from '@/hooks/useValuations';
import { useShareholders } from '@/hooks/useShareholders';
import { usePendingWorkflows, useWorkflows } from '@/hooks/useWorkflows';
import { useCapTableSummaryByType } from '@/hooks/useCapTable';
import { contractService } from '@/services/contractService';

// ─── Helpers ─────────────────────────────────────────────────────────────────

const CIRCUMFERENCE = 2 * Math.PI * 40; // ≈ 251.3

const CHART_COLORS = [
  '#2563EB', // azul  — Founders
  '#7C3AED', // roxo  — Investors
  '#059669', // verde — ESOP
  '#EA580C', // laranja — Employees
  '#CA8A04', // amarelo — Advisors
  '#EC4899', // rosa
  '#6B7280', // cinza  — Other
];

function formatCurrency(amount: number | null | undefined): string {
  if (amount == null) return '—';
  if (amount >= 1_000_000_000) return `R$ ${(amount / 1_000_000_000).toFixed(1)}B`;
  if (amount >= 1_000_000) return `R$ ${(amount / 1_000_000).toFixed(1)}M`;
  if (amount >= 1_000) return `R$ ${(amount / 1_000).toFixed(0)}K`;
  return `R$ ${amount.toFixed(0)}`;
}

function formatRelativeTime(dateStr: string | null | undefined): string {
  if (!dateStr) return '—';
  const diffMs = Date.now() - new Date(dateStr).getTime();
  const mins = Math.floor(diffMs / 60_000);
  if (mins < 60) return `${mins} min atrás`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours} hora${hours > 1 ? 's' : ''} atrás`;
  const days = Math.floor(hours / 24);
  if (days < 30) return `${days} dia${days > 1 ? 's' : ''} atrás`;
  const months = Math.floor(days / 30);
  return `${months} mês${months > 1 ? 'es' : ''} atrás`;
}

// ─── Component ───────────────────────────────────────────────────────────────

export default function Dashboard() {
  const { user } = useAuthStore();
  const { selectedCompanyId } = useClientStore();
  const companyId = user?.companyId || selectedCompanyId || undefined;

  // ─── Data Fetching ──────────────────────────────────────────────────────────

  const { data: valuationsData } = useValuations({ companyId, pageSize: 20 });
  const { data: shareholdersData } = useShareholders({ companyId, pageSize: 1 });
  const { data: pendingWorkflows } = usePendingWorkflows();
  const { data: recentWorkflows } = useWorkflows({ pageSize: 4 });
  const { data: capTableByType } = useCapTableSummaryByType(companyId);
  const { data: contractsData } = useQuery({
    queryKey: ['contracts', 'dashboard', companyId],
    queryFn: () => contractService.getContracts({ companyId: companyId!, pageSize: 1 }),
    enabled: !!companyId,
    staleTime: 60_000,
  });

  // ─── Derived Values ──────────────────────────────────────────────────────────

  // Valuations sorted chronologically (oldest → newest)
  const sortedValuations = [...(valuationsData?.items ?? [])].sort(
    (a, b) => new Date(a.valuationDate).getTime() - new Date(b.valuationDate).getTime()
  );
  const approvedValuations = sortedValuations.filter((v) => v.status === 'approved');
  const latestValuation = approvedValuations[approvedValuations.length - 1] ?? null;
  const firstValuation = approvedValuations[0] ?? null;

  const valuationGrowthPct =
    approvedValuations.length >= 2 &&
    latestValuation?.valuationAmount &&
    firstValuation?.valuationAmount
      ? Math.round(
          ((latestValuation.valuationAmount - firstValuation.valuationAmount) /
            firstValuation.valuationAmount) *
            100
        )
      : null;

  // Stats cards
  const stats = [
    {
      icon: <TrendingUp className="w-6 h-6" />,
      iconColor: 'bg-success',
      value: latestValuation?.valuationAmount != null
        ? formatCurrency(latestValuation.valuationAmount)
        : '—',
      label: 'Valuation Atual',
      badge: valuationGrowthPct != null
        ? { value: `${valuationGrowthPct >= 0 ? '+' : ''}${valuationGrowthPct}%`, variant: 'success' as const }
        : undefined,
    },
    {
      icon: <Users className="w-6 h-6" />,
      iconColor: 'bg-info',
      value: shareholdersData != null ? String(shareholdersData.totalCount) : '—',
      label: 'Total de Sócios',
      badge: undefined,
    },
    {
      icon: <AlertCircle className="w-6 h-6" />,
      iconColor: 'bg-warning',
      value: pendingWorkflows != null ? String(pendingWorkflows.length) : '—',
      label: 'Aprovações Pendentes',
      badge: pendingWorkflows?.length
        ? { value: 'Urgente', variant: 'warning' as const }
        : undefined,
    },
    {
      icon: <FileText className="w-6 h-6" />,
      iconColor: 'bg-purple',
      value: contractsData != null ? String(contractsData.totalCount) : '—',
      label: 'Contratos',
      badge: undefined,
    },
  ];

  // Donut chart segments calculated from Cap Table summary
  const donutSegments = (() => {
    if (!capTableByType?.length) return [];
    let offsetAccum = 0;
    return capTableByType.map((entry, i) => {
      const arcLength = (entry.ownershipPercentage / 100) * CIRCUMFERENCE;
      const dashArray = `${arcLength.toFixed(2)} ${CIRCUMFERENCE.toFixed(2)}`;
      const dashOffset = -offsetAccum;
      offsetAccum += arcLength;
      return {
        ...entry,
        dashArray,
        dashOffset: dashOffset.toFixed(2),
        color: CHART_COLORS[i % CHART_COLORS.length],
      };
    });
  })();

  // Bar chart — all valuations by date
  const maxValuation = Math.max(...sortedValuations.map((v) => v.valuationAmount ?? 0), 1);
  const valuationBars = sortedValuations
    .filter((v) => v.valuationAmount != null)
    .map((v) => ({
      label: new Date(v.valuationDate).toLocaleDateString('pt-BR', {
        year: '2-digit',
        month: 'short',
      }),
      height: Math.max(5, ((v.valuationAmount ?? 0) / maxValuation) * 100),
      amount: v.valuationAmount,
    }));

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Dashboard</h1>
          <p className="page-subtitle">
            Visão geral de {user?.companyName || 'sua empresa'}
          </p>
        </div>
        <div className="flex gap-3">
          <Button variant="secondary" icon={<Download className="w-4 h-4" />}>
            Exportar
          </Button>
          <Button icon={<Plus className="w-4 h-4" />}>Novo Evento</Button>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((stat, index) => (
          <StatCard key={index} {...stat} />
        ))}
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Distribuição Societária */}
        <Card>
          <h3 className="font-semibold text-primary mb-4">Distribuição Societária</h3>
          {donutSegments.length > 0 ? (
            <>
              <div className="relative w-48 h-48 mx-auto">
                <svg viewBox="0 0 100 100" className="w-full h-full -rotate-90">
                  {donutSegments.map((seg, i) => (
                    <circle
                      key={i}
                      cx="50"
                      cy="50"
                      r="40"
                      fill="none"
                      stroke={seg.color}
                      strokeWidth="20"
                      strokeDasharray={seg.dashArray}
                      strokeDashoffset={seg.dashOffset}
                      className="opacity-90"
                    />
                  ))}
                </svg>
                <div className="absolute inset-0 flex items-center justify-center">
                  <div className="text-center">
                    <span className="text-2xl font-bold text-primary">100%</span>
                    <p className="text-xs text-primary-500">Cap Table</p>
                  </div>
                </div>
              </div>
              <div className="flex flex-wrap justify-center gap-x-4 gap-y-2 mt-4">
                {donutSegments.map((seg, i) => (
                  <div key={i} className="flex items-center gap-2">
                    <div
                      className="w-3 h-3 rounded-full flex-shrink-0"
                      style={{ backgroundColor: seg.color }}
                    />
                    <span className="text-sm text-primary-600">
                      {seg.typeDescription} {seg.ownershipPercentage.toFixed(1)}%
                    </span>
                  </div>
                ))}
              </div>
            </>
          ) : (
            <div className="flex items-center justify-center h-48 text-primary-400 text-sm">
              Sem dados de Cap Table
            </div>
          )}
        </Card>

        {/* Evolução do Valuation */}
        <Card>
          <div className="flex items-center justify-between mb-4">
            <h3 className="font-semibold text-primary">Evolução do Valuation</h3>
            {valuationGrowthPct != null && (
              <div className="flex items-center gap-1 text-success text-sm font-medium">
                <ArrowUpRight className="w-4 h-4" />
                {valuationGrowthPct >= 0 ? '+' : ''}{valuationGrowthPct}%
              </div>
            )}
          </div>
          {valuationBars.length > 0 ? (
            <>
              <div className="flex items-end justify-between h-40 gap-2">
                {valuationBars.map((item, i) => (
                  <div key={i} className="flex-1 flex flex-col items-center gap-2">
                    <div
                      className="w-full bg-gradient-to-t from-info to-purple rounded-t"
                      style={{ height: `${item.height}%` }}
                    />
                    <span className="text-2xs text-primary-500 text-center">{item.label}</span>
                  </div>
                ))}
              </div>
              <div className="flex justify-between mt-4 pt-4 border-t border-primary-100">
                <div>
                  <p className="text-xs text-primary-500">Primeiro</p>
                  <p className="font-semibold text-primary">
                    {formatCurrency(sortedValuations[0]?.valuationAmount)}
                  </p>
                </div>
                <div className="text-right">
                  <p className="text-xs text-primary-500">Atual</p>
                  <p className="font-semibold text-success">
                    {formatCurrency(latestValuation?.valuationAmount)}
                  </p>
                </div>
              </div>
            </>
          ) : (
            <div className="flex items-center justify-center h-40 text-primary-400 text-sm">
              Sem dados de valuation
            </div>
          )}
        </Card>

        {/* Atividade Recente */}
        <Card>
          <h3 className="font-semibold text-primary mb-4">Atividade Recente</h3>
          {recentWorkflows?.items?.length ? (
            <div className="space-y-4">
              {recentWorkflows.items.map((workflow) => (
                <div key={workflow.id} className="flex items-start gap-3">
                  <div className="w-2 h-2 mt-2 rounded-full bg-accent flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-sm text-primary">{workflow.title}</p>
                    <p className="text-xs text-primary-500">
                      {workflow.requestedBy} • {formatRelativeTime(workflow.requestedAt)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="flex items-center justify-center h-40 text-primary-400 text-sm">
              Sem atividades recentes
            </div>
          )}
        </Card>
      </div>

      {/* Pending Approvals Table */}
      <Card>
        <div className="flex items-center justify-between mb-4">
          <h3 className="font-semibold text-primary">Aprovações Pendentes</h3>
          <Button variant="ghost" size="sm">
            Ver todas
          </Button>
        </div>
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Tipo</th>
                <th>Solicitante</th>
                <th>Status</th>
                <th>Prazo</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {pendingWorkflows?.length ? (
                pendingWorkflows.map((workflow) => (
                  <tr key={workflow.id}>
                    <td>
                      <span className="font-medium">{workflow.workflowType}</span>
                    </td>
                    <td>
                      <div className="flex items-center gap-3">
                        <Avatar name={workflow.requestedBy} size="sm" />
                        {workflow.requestedBy}
                      </div>
                    </td>
                    <td>
                      <Badge variant="pending">{workflow.status}</Badge>
                    </td>
                    <td className="text-primary-500">
                      {formatRelativeTime(workflow.requestedAt)}
                    </td>
                    <td>
                      <Button variant="secondary" size="sm">
                        Revisar
                      </Button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5} className="text-center text-primary-400 py-8">
                    Nenhuma aprovação pendente
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}
