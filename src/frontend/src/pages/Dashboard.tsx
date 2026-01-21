import {
  TrendingUp,
  Users,
  AlertCircle,
  FileText,
  ArrowUpRight,
  ArrowDownRight,
  Plus,
  Download,
} from 'lucide-react';
import { Button, Card, StatCard, Badge, Avatar, ProgressBar } from '@/components/ui';
import { useAuthStore } from '@/stores/authStore';

// Mock data
const stats = [
  {
    icon: <TrendingUp className="w-6 h-6" />,
    iconColor: 'bg-success',
    value: 'R$ 15M',
    label: 'Valuation Atual',
    badge: { value: '+25%', variant: 'success' as const },
  },
  {
    icon: <Users className="w-6 h-6" />,
    iconColor: 'bg-info',
    value: '12',
    label: 'Total de Sócios',
    badge: { value: '+2', variant: 'info' as const },
  },
  {
    icon: <AlertCircle className="w-6 h-6" />,
    iconColor: 'bg-warning',
    value: '3',
    label: 'Aprovações Pendentes',
    badge: { value: 'Urgente', variant: 'warning' as const },
  },
  {
    icon: <FileText className="w-6 h-6" />,
    iconColor: 'bg-purple',
    value: '28',
    label: 'Contratos Ativos',
    badge: { value: '+5', variant: 'success' as const },
  },
];

const pendingApprovals = [
  {
    id: 1,
    type: 'Entrada de Sócio',
    name: 'Maria Silva',
    status: 'Aguardando Conselho',
    date: '2 dias',
  },
  {
    id: 2,
    type: 'Vesting Acelerado',
    name: 'João Santos',
    status: 'Revisão Jurídica',
    date: '3 dias',
  },
  {
    id: 3,
    type: 'Transferência',
    name: 'Pedro Costa',
    status: 'Aguardando Aprovação',
    date: '5 dias',
  },
];

const recentActivities = [
  { id: 1, action: 'Novo sócio aprovado', user: 'Carlos Lima', time: '2 horas atrás' },
  { id: 2, action: 'Contrato assinado', user: 'Ana Souza', time: '5 horas atrás' },
  { id: 3, action: 'Valuation atualizado', user: 'Sistema', time: '1 dia atrás' },
  { id: 4, action: 'Vesting liberado', user: 'Maria Silva', time: '2 dias atrás' },
];

export default function Dashboard() {
  const { user } = useAuthStore();

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
          <h3 className="font-semibold text-primary mb-4">
            Distribuição Societária
          </h3>
          <div className="relative w-48 h-48 mx-auto">
            {/* Donut Chart SVG */}
            <svg viewBox="0 0 100 100" className="w-full h-full -rotate-90">
              {/* Fundadores - 60% */}
              <circle
                cx="50"
                cy="50"
                r="40"
                fill="none"
                stroke="#2563EB"
                strokeWidth="20"
                strokeDasharray="150.8 251.3"
                className="opacity-90"
              />
              {/* Investidores - 25% */}
              <circle
                cx="50"
                cy="50"
                r="40"
                fill="none"
                stroke="#7C3AED"
                strokeWidth="20"
                strokeDasharray="62.8 251.3"
                strokeDashoffset="-150.8"
                className="opacity-90"
              />
              {/* Pool ESOP - 15% */}
              <circle
                cx="50"
                cy="50"
                r="40"
                fill="none"
                stroke="#059669"
                strokeWidth="20"
                strokeDasharray="37.7 251.3"
                strokeDashoffset="-213.6"
                className="opacity-90"
              />
            </svg>
            <div className="absolute inset-0 flex items-center justify-center">
              <div className="text-center">
                <span className="text-2xl font-bold text-primary">100%</span>
                <p className="text-xs text-primary-500">Cap Table</p>
              </div>
            </div>
          </div>
          {/* Legend */}
          <div className="flex justify-center gap-4 mt-4">
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-info" />
              <span className="text-sm text-primary-600">Fundadores 60%</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-purple" />
              <span className="text-sm text-primary-600">Investidores 25%</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-success" />
              <span className="text-sm text-primary-600">Pool 15%</span>
            </div>
          </div>
        </Card>

        {/* Evolução do Valuation */}
        <Card>
          <div className="flex items-center justify-between mb-4">
            <h3 className="font-semibold text-primary">Evolução do Valuation</h3>
            <div className="flex items-center gap-1 text-success text-sm font-medium">
              <ArrowUpRight className="w-4 h-4" />
              +650%
            </div>
          </div>
          {/* Simple Bar Chart */}
          <div className="flex items-end justify-between h-40 gap-2">
            {[
              { year: '2023', value: 13 },
              { year: '2024 Q1', value: 27 },
              { year: '2024 Q2', value: 40 },
              { year: '2024 Q3', value: 60 },
              { year: '2024 Q4', value: 80 },
              { year: '2025', value: 100 },
            ].map((item, i) => (
              <div key={i} className="flex-1 flex flex-col items-center gap-2">
                <div
                  className="w-full bg-gradient-to-t from-info to-purple rounded-t"
                  style={{ height: `${item.value}%` }}
                />
                <span className="text-2xs text-primary-500">{item.year}</span>
              </div>
            ))}
          </div>
          <div className="flex justify-between mt-4 pt-4 border-t border-primary-100">
            <div>
              <p className="text-xs text-primary-500">Fundação</p>
              <p className="font-semibold text-primary">R$ 2M</p>
            </div>
            <div className="text-right">
              <p className="text-xs text-primary-500">Atual</p>
              <p className="font-semibold text-success">R$ 15M</p>
            </div>
          </div>
        </Card>

        {/* Atividade Recente */}
        <Card>
          <h3 className="font-semibold text-primary mb-4">Atividade Recente</h3>
          <div className="space-y-4">
            {recentActivities.map((activity) => (
              <div key={activity.id} className="flex items-start gap-3">
                <div className="w-2 h-2 mt-2 rounded-full bg-accent" />
                <div className="flex-1">
                  <p className="text-sm text-primary">{activity.action}</p>
                  <p className="text-xs text-primary-500">
                    {activity.user} • {activity.time}
                  </p>
                </div>
              </div>
            ))}
          </div>
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
                <th>Nome</th>
                <th>Status</th>
                <th>Prazo</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {pendingApprovals.map((item) => (
                <tr key={item.id}>
                  <td>
                    <span className="font-medium">{item.type}</span>
                  </td>
                  <td>
                    <div className="flex items-center gap-3">
                      <Avatar name={item.name} size="sm" />
                      {item.name}
                    </div>
                  </td>
                  <td>
                    <Badge variant="pending">{item.status}</Badge>
                  </td>
                  <td className="text-primary-500">{item.date}</td>
                  <td>
                    <Button variant="secondary" size="sm">
                      Revisar
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}
