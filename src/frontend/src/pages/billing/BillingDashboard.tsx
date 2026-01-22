import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  DollarSign,
  TrendingUp,
  TrendingDown,
  FileText,
  CheckCircle,
  Clock,
  AlertCircle,
  Download,
  Eye,
  Calendar,
  CreditCard,
  Users,
  Package,
} from 'lucide-react';
import { LineChart, Line, AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { Button } from '@/components/ui';
import { invoicesApi } from '@/services/api';

interface InvoiceStatistics {
  totalRevenue: number;
  pendingRevenue: number;
  overdueRevenue: number;
  totalInvoices: number;
  paidInvoices: number;
  pendingInvoices: number;
  overdueInvoices: number;
  cancelledInvoices: number;
}

interface MrrData {
  monthlyData: Array<{
    year: number;
    month: number;
    monthName: string;
    revenue: number;
    invoiceCount: number;
  }>;
  currentMrr: number;
  averageMrr: number;
  growthRate: number;
}

export default function BillingDashboard() {
  const [statistics, setStatistics] = useState<InvoiceStatistics>({
    totalRevenue: 0,
    pendingRevenue: 0,
    overdueRevenue: 0,
    totalInvoices: 0,
    paidInvoices: 0,
    pendingInvoices: 0,
    overdueInvoices: 0,
    cancelledInvoices: 0,
  });
  const [mrrData, setMrrData] = useState<MrrData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadStatistics();
    loadMrrData();
  }, []);

  const loadStatistics = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await fetch('/api/invoices/statistics');
      const data = await response.json();
      setStatistics(data);
    } catch (err: any) {
      console.error('Erro ao carregar estatísticas:', err);
      setError(err.message || 'Erro ao carregar estatísticas');
    } finally {
      setLoading(false);
    }
  };

  const loadMrrData = async () => {
    try {
      console.log('[BillingDashboard] Carregando dados de MRR...');
      const data = await invoicesApi.getMrrData(12);
      console.log('[BillingDashboard] Dados de MRR carregados:', data);
      setMrrData(data);
    } catch (err: any) {
      console.error('Erro ao carregar dados de MRR:', err);
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  };



  return (
    <div className="page-container">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="page-title">Billing</h1>
          <p className="page-subtitle">Gestão de assinaturas, planos e faturamento</p>
        </div>
        <div className="flex gap-3">
          <Button variant="outline">
            <Download className="w-4 h-4 mr-2" />
            Exportar
          </Button>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div className="flex gap-2 mb-8 border-b border-border pb-0">
        <Link to="/billing" className="px-4 py-3 font-medium text-primary border-b-2 border-primary">
          Dashboard
        </Link>
        <Link to="/billing/plans" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Planos
        </Link>
        <Link to="/billing/clients" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Clientes & Assinaturas
        </Link>
        <Link to="/billing/invoices" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Faturas
        </Link>
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-6">
          {error}
        </div>
      )}

      {/* Loading State */}
      {loading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Carregando estatísticas...</p>
        </div>
      ) : (
        <>
          {/* Stats Cards */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            <div className="card">
              <div className="flex items-center justify-between mb-4">
                <div className="p-3 bg-green-100 rounded-xl">
                  <DollarSign className="w-6 h-6 text-green-600" />
                </div>
                <div className="flex items-center gap-1 text-sm text-green-600">
                  <TrendingUp className="w-4 h-4" />
                  <span>+12%</span>
                </div>
              </div>
              <div className="text-sm text-muted-foreground mb-1">Receita Recebida</div>
              <div className="text-3xl font-bold">R$ {statistics.totalRevenue.toFixed(2)}</div>
              <div className="text-xs text-muted-foreground mt-1">
                {statistics.paidInvoices} faturas pagas
              </div>
            </div>

            <div className="card">
              <div className="flex items-center justify-between mb-4">
                <div className="p-3 bg-yellow-100 rounded-xl">
                  <Clock className="w-6 h-6 text-yellow-600" />
                </div>
              </div>
              <div className="text-sm text-muted-foreground mb-1">A Receber</div>
              <div className="text-3xl font-bold text-yellow-600">
                R$ {statistics.pendingRevenue.toFixed(2)}
              </div>
              <div className="text-xs text-muted-foreground mt-1">
                {statistics.pendingInvoices} faturas pendentes
              </div>
            </div>

            <div className="card">
              <div className="flex items-center justify-between mb-4">
                <div className="p-3 bg-red-100 rounded-xl">
                  <AlertCircle className="w-6 h-6 text-red-600" />
                </div>
                <div className="flex items-center gap-1 text-sm text-red-600">
                  <TrendingDown className="w-4 h-4" />
                  <span>Atenção</span>
                </div>
              </div>
              <div className="text-sm text-muted-foreground mb-1">Em Atraso</div>
              <div className="text-3xl font-bold text-red-600">
                R$ {statistics.overdueRevenue.toFixed(2)}
              </div>
              <div className="text-xs text-muted-foreground mt-1">
                {statistics.overdueInvoices} faturas vencidas
              </div>
            </div>

            <div className="card">
              <div className="flex items-center justify-between mb-4">
                <div className="p-3 bg-blue-100 rounded-xl">
                  <FileText className="w-6 h-6 text-blue-600" />
                </div>
              </div>
              <div className="text-sm text-muted-foreground mb-1">Total de Faturas</div>
              <div className="text-3xl font-bold">{statistics.totalInvoices}</div>
              <div className="text-xs text-muted-foreground mt-1">
                Este mês
              </div>
            </div>
          </div>

          {/* Gráfico de MRR */}
          {mrrData && mrrData.monthlyData && mrrData.monthlyData.length > 0 ? (
            <div className="card mb-8">
              <div className="flex items-center justify-between mb-6">
                <div>
                  <h2 className="text-xl font-bold text-gray-900">
                    Faturamento Mensal (MRR)
                  </h2>
                  <p className="text-sm text-gray-600 mt-1">
                    Receita recorrente dos últimos 12 meses
                  </p>
                </div>
                <div className="flex items-center gap-4">
                  <div className="text-right">
                    <p className="text-xs text-gray-500">MRR Atual</p>
                    <p className="text-lg font-bold text-green-600">
                      {formatCurrency(mrrData.currentMrr)}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-xs text-gray-500">Crescimento</p>
                    <p className={`text-lg font-bold flex items-center gap-1 ${
                      mrrData.growthRate >= 0 ? 'text-green-600' : 'text-red-600'
                    }`}>
                      {mrrData.growthRate >= 0 ? (
                        <TrendingUp className="w-4 h-4" />
                      ) : (
                        <TrendingDown className="w-4 h-4" />
                      )}
                      {Math.abs(mrrData.growthRate).toFixed(1)}%
                    </p>
                  </div>
                </div>
              </div>

              <ResponsiveContainer width="100%" height={350}>
                <AreaChart
                  data={mrrData.monthlyData}
                  margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
                >
                  <defs>
                    <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#10b981" stopOpacity={0.8}/>
                      <stop offset="95%" stopColor="#10b981" stopOpacity={0}/>
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis 
                    dataKey="monthName" 
                    stroke="#6b7280"
                    style={{ fontSize: '12px' }}
                  />
                  <YAxis 
                    stroke="#6b7280"
                    style={{ fontSize: '12px' }}
                    tickFormatter={(value) => `R$ ${(value / 1000).toFixed(0)}k`}
                  />
                  <Tooltip 
                    contentStyle={{
                      backgroundColor: 'rgba(255, 255, 255, 0.95)',
                      border: '1px solid #e5e7eb',
                      borderRadius: '8px',
                      boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)',
                    }}
                    formatter={(value: number) => [formatCurrency(value), 'Receita']}
                    labelFormatter={(label) => `Período: ${label}`}
                  />
                  <Legend 
                    wrapperStyle={{ paddingTop: '20px' }}
                    formatter={() => 'Receita Mensal'}
                  />
                  <Area 
                    type="monotone" 
                    dataKey="revenue" 
                    stroke="#10b981" 
                    strokeWidth={2}
                    fillOpacity={1} 
                    fill="url(#colorRevenue)" 
                  />
                </AreaChart>
              </ResponsiveContainer>

              <div className="mt-6 pt-6 border-t border-gray-200">
                <div className="grid grid-cols-3 gap-4 text-center">
                  <div>
                    <p className="text-xs text-gray-500 mb-1">MRR Médio</p>
                    <p className="text-lg font-semibold text-gray-900">
                      {formatCurrency(mrrData.averageMrr)}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-500 mb-1">Total de Faturas</p>
                    <p className="text-lg font-semibold text-gray-900">
                      {mrrData.monthlyData.reduce((acc, m) => acc + m.invoiceCount, 0)}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-500 mb-1">Período</p>
                    <p className="text-lg font-semibold text-gray-900">
                      12 meses
                    </p>
                  </div>
                </div>
              </div>
            </div>
          ) : (
            <div className="card mb-8 text-center py-12">
              <p className="text-gray-500">
                {mrrData === null ? 'Carregando dados de MRR...' : 'Nenhum dado de faturamento disponível'}
              </p>
            </div>
          )}

          {/* Quick Actions */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Link to="/billing/plans">
          <div className="card hover:shadow-lg transition-shadow cursor-pointer text-left">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-blue-100 rounded-xl">
                <Package className="w-6 h-6 text-blue-600" />
              </div>
              <div>
                <div className="font-semibold mb-1">Gerenciar Planos</div>
                <div className="text-sm text-muted-foreground">
                  Ver e editar planos de assinatura
                </div>
              </div>
            </div>
          </div>
        </Link>

        <Link to="/billing/clients">
          <div className="card hover:shadow-lg transition-shadow cursor-pointer text-left">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-green-100 rounded-xl">
                <Users className="w-6 h-6 text-green-600" />
              </div>
              <div>
                <div className="font-semibold mb-1">Clientes & Assinaturas</div>
                <div className="text-sm text-muted-foreground">
                  Gerenciar clientes e suas assinaturas
                </div>
              </div>
            </div>
          </div>
        </Link>

        <Link to="/billing/invoices">
          <div className="card hover:shadow-lg transition-shadow cursor-pointer text-left">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-purple-100 rounded-xl">
                <CreditCard className="w-6 h-6 text-purple-600" />
              </div>
              <div>
                <div className="font-semibold mb-1">Gerar Faturas do Mês</div>
                <div className="text-sm text-muted-foreground">
                  Criar faturas para todas as assinaturas ativas
                </div>
              </div>
            </div>
          </div>
        </Link>
          </div>
        </>
      )}
    </div>
  );
}
