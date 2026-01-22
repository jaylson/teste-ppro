import { useState, useEffect } from 'react';
import { Download, Search, Eye, CheckCircle, X, Calendar, DollarSign, FileText, RefreshCw } from 'lucide-react';
import { Button } from '@/components/ui';
import { invoicesApi, clientsApi, plansApi, type Invoice, type InvoiceFilters, type ClientListItem, type Plan } from '@/services/api';

export default function Invoices() {
  const [searchTerm, setSearchTerm] = useState('');
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [clients, setClients] = useState<ClientListItem[]>([]);
  const [plans, setPlans] = useState<Plan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [viewingInvoice, setViewingInvoice] = useState<Invoice | null>(null);
  const [generating, setGenerating] = useState(false);

  // Filtros
  const [filters, setFilters] = useState<InvoiceFilters>({
    clientId: undefined,
    status: undefined,
    startDate: undefined,
    endDate: undefined,
    planId: undefined,
  });

  useEffect(() => {
    loadInvoices();
    loadClients();
    loadPlans();
  }, []);

  const loadInvoices = async (appliedFilters?: InvoiceFilters) => {
    try {
      setLoading(true);
      setError(null);
      const data = await invoicesApi.getAll(appliedFilters || filters);
      setInvoices(data);
    } catch (err: any) {
      console.error('Erro ao carregar faturas:', err);
      setError(err.response?.data?.message || err.message || 'Erro ao carregar faturas');
    } finally {
      setLoading(false);
    }
  };

  const loadClients = async () => {
    try {
      const data = await clientsApi.getAll();
      setClients(data);
    } catch (err) {
      console.error('Erro ao carregar clientes:', err);
    }
  };

  const loadPlans = async () => {
    try {
      const data = await plansApi.getAll();
      setPlans(data);
    } catch (err) {
      console.error('Erro ao carregar planos:', err);
    }
  };

  const handleApplyFilters = () => {
    loadInvoices(filters);
  };

  const handleClearFilters = () => {
    const clearedFilters: InvoiceFilters = {
      clientId: undefined,
      status: undefined,
      startDate: undefined,
      endDate: undefined,
      planId: undefined,
    };
    setFilters(clearedFilters);
    loadInvoices(clearedFilters);
  };

  const handleMarkAsPaid = async (id: string) => {
    try {
      await invoicesApi.markAsPaid(id);
      loadInvoices();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Erro ao marcar fatura como paga');
    }
  };

  const handleCancel = async (id: string) => {
    if (!confirm('Tem certeza que deseja cancelar esta fatura?')) return;
    
    try {
      await invoicesApi.cancel(id);
      loadInvoices();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Erro ao cancelar fatura');
    }
  };

  const handleDownloadPdf = async (invoice: Invoice) => {
    try {
      const blob = await invoicesApi.downloadPdf(invoice.id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `fatura-${invoice.invoiceNumber}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err: any) {
      alert(err.response?.data?.message || 'Erro ao baixar PDF');
    }
  };

  const handleGenerateMonthly = async () => {
    if (!confirm('Tem certeza que deseja gerar as faturas do mês para todas as assinaturas ativas?')) return;
    
    try {
      setGenerating(true);
      const result = await invoicesApi.generateMonthly();
      alert(`${result.invoicesGenerated} faturas geradas com sucesso!`);
      loadInvoices();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Erro ao gerar faturas mensais');
    } finally {
      setGenerating(false);
    }
  };

  const filteredInvoices = invoices.filter((invoice) => {
    const matchesSearch = 
      invoice.invoiceNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      invoice.clientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      invoice.planName.toLowerCase().includes(searchTerm.toLowerCase());
    
    return matchesSearch;
  });

  const getStatusBadge = (status: string) => {
    const statusConfig: Record<string, { bg: string; text: string; label: string }> = {
      pending: { bg: 'bg-yellow-100', text: 'text-yellow-800', label: 'Pendente' },
      paid: { bg: 'bg-green-100', text: 'text-green-800', label: 'Paga' },
      overdue: { bg: 'bg-red-100', text: 'text-red-800', label: 'Atrasada' },
      cancelled: { bg: 'bg-gray-100', text: 'text-gray-800', label: 'Cancelada' },
    };

    const config = statusConfig[status] || statusConfig.pending;
    return (
      <span className={`px-2 py-1 rounded-full text-xs font-semibold ${config.bg} ${config.text}`}>
        {config.label}
      </span>
    );
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString('pt-BR');
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Faturas</h1>
          <p className="mt-2 text-sm text-gray-600">
            Gerencie todas as faturas dos seus clientes
          </p>
        </div>
        <Button
          onClick={handleGenerateMonthly}
          disabled={generating}
          className="bg-blue-600 hover:bg-blue-700 text-white"
        >
          <RefreshCw className={`w-4 h-4 mr-2 ${generating ? 'animate-spin' : ''}`} />
          {generating ? 'Gerando...' : 'Gerar Faturas do Mês'}
        </Button>
      </div>

      {/* Filtros */}
      <div className="bg-white shadow rounded-lg p-6">
        <h2 className="text-lg font-semibold mb-4">Filtros</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Cliente
            </label>
            <select
              value={filters.clientId || ''}
              onChange={(e) => setFilters({ ...filters, clientId: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos os clientes</option>
              {clients.map((client) => (
                <option key={client.id} value={client.id}>
                  {client.name}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Status
            </label>
            <select
              value={filters.status || ''}
              onChange={(e) => setFilters({ ...filters, status: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos os status</option>
              <option value="pending">Pendente</option>
              <option value="paid">Paga</option>
              <option value="overdue">Atrasada</option>
              <option value="cancelled">Cancelada</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Data Inicial
            </label>
            <input
              type="date"
              value={filters.startDate || ''}
              onChange={(e) => setFilters({ ...filters, startDate: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Data Final
            </label>
            <input
              type="date"
              value={filters.endDate || ''}
              onChange={(e) => setFilters({ ...filters, endDate: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Plano
            </label>
            <select
              value={filters.planId || ''}
              onChange={(e) => setFilters({ ...filters, planId: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos os planos</option>
              {plans.map((plan) => (
                <option key={plan.id} value={plan.id}>
                  {plan.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="flex gap-2 mt-4">
          <Button onClick={handleApplyFilters} className="bg-blue-600 hover:bg-blue-700 text-white">
            Aplicar Filtros
          </Button>
          <Button onClick={handleClearFilters} variant="outline">
            Limpar Filtros
          </Button>
        </div>
      </div>

      {/* Busca */}
      <div className="bg-white shadow rounded-lg p-4">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
          <input
            type="text"
            placeholder="Buscar por número da fatura, cliente ou plano..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      {/* Lista de Faturas */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md">
          {error}
        </div>
      )}

      {loading ? (
        <div className="bg-white shadow rounded-lg p-8 text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Carregando faturas...</p>
        </div>
      ) : (
        <div className="bg-white shadow rounded-lg overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Número
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Cliente
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Plano
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Período
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Valor
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Vencimento
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Ações
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredInvoices.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-6 py-8 text-center text-gray-500">
                    <FileText className="w-12 h-12 mx-auto mb-2 text-gray-400" />
                    Nenhuma fatura encontrada
                  </td>
                </tr>
              ) : (
                filteredInvoices.map((invoice) => (
                  <tr key={invoice.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {invoice.invoiceNumber}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">{invoice.clientName}</div>
                      <div className="text-sm text-gray-500">{invoice.clientEmail}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {invoice.planName}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {invoice.referenceMonth.toString().padStart(2, '0')}/{invoice.referenceYear}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {formatCurrency(invoice.amount)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {formatDate(invoice.dueDate)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {getStatusBadge(invoice.status)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => setViewingInvoice(invoice)}
                          className="text-blue-600 hover:text-blue-900"
                          title="Visualizar"
                        >
                          <Eye className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => handleDownloadPdf(invoice)}
                          className="text-green-600 hover:text-green-900"
                          title="Download PDF"
                        >
                          <Download className="w-4 h-4" />
                        </button>
                        {invoice.status === 'pending' && (
                          <button
                            onClick={() => handleMarkAsPaid(invoice.id)}
                            className="text-green-600 hover:text-green-900"
                            title="Marcar como Paga"
                          >
                            <CheckCircle className="w-4 h-4" />
                          </button>
                        )}
                        {invoice.status !== 'cancelled' && invoice.status !== 'paid' && (
                          <button
                            onClick={() => handleCancel(invoice.id)}
                            className="text-red-600 hover:text-red-900"
                            title="Cancelar"
                          >
                            <X className="w-4 h-4" />
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* Modal de Visualização */}
      {viewingInvoice && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <div className="flex justify-between items-start mb-4">
                <h2 className="text-2xl font-bold text-gray-900">
                  Detalhes da Fatura
                </h2>
                <button
                  onClick={() => setViewingInvoice(null)}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <X className="w-6 h-6" />
                </button>
              </div>

              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-500">Número</label>
                    <p className="mt-1 text-sm text-gray-900">{viewingInvoice.invoiceNumber}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">Status</label>
                    <div className="mt-1">{getStatusBadge(viewingInvoice.status)}</div>
                  </div>
                </div>

                <div className="border-t pt-4">
                  <h3 className="font-semibold text-gray-900 mb-2">Cliente</h3>
                  <div className="space-y-2">
                    <p className="text-sm"><span className="font-medium">Nome:</span> {viewingInvoice.clientName}</p>
                    <p className="text-sm"><span className="font-medium">Email:</span> {viewingInvoice.clientEmail}</p>
                    <p className="text-sm"><span className="font-medium">Documento:</span> {viewingInvoice.clientDocument}</p>
                  </div>
                </div>

                <div className="border-t pt-4">
                  <h3 className="font-semibold text-gray-900 mb-2">Assinatura</h3>
                  <div className="space-y-2">
                    <p className="text-sm"><span className="font-medium">Plano:</span> {viewingInvoice.planName}</p>
                    <p className="text-sm"><span className="font-medium">Período:</span> {viewingInvoice.referenceMonth.toString().padStart(2, '0')}/{viewingInvoice.referenceYear}</p>
                  </div>
                </div>

                <div className="border-t pt-4">
                  <h3 className="font-semibold text-gray-900 mb-2">Valores e Datas</h3>
                  <div className="space-y-2">
                    <p className="text-sm"><span className="font-medium">Valor:</span> {formatCurrency(viewingInvoice.amount)}</p>
                    <p className="text-sm"><span className="font-medium">Data de Emissão:</span> {formatDate(viewingInvoice.issueDate)}</p>
                    <p className="text-sm"><span className="font-medium">Data de Vencimento:</span> {formatDate(viewingInvoice.dueDate)}</p>
                    {viewingInvoice.paymentDate && (
                      <p className="text-sm"><span className="font-medium">Data de Pagamento:</span> {formatDate(viewingInvoice.paymentDate)}</p>
                    )}
                  </div>
                </div>

                {viewingInvoice.description && (
                  <div className="border-t pt-4">
                    <h3 className="font-semibold text-gray-900 mb-2">Descrição</h3>
                    <p className="text-sm text-gray-700">{viewingInvoice.description}</p>
                  </div>
                )}

                {viewingInvoice.notes && (
                  <div className="border-t pt-4">
                    <h3 className="font-semibold text-gray-900 mb-2">Observações</h3>
                    <p className="text-sm text-gray-700">{viewingInvoice.notes}</p>
                  </div>
                )}
              </div>

              <div className="mt-6 flex gap-2">
                <Button
                  onClick={() => handleDownloadPdf(viewingInvoice)}
                  className="flex-1 bg-blue-600 hover:bg-blue-700 text-white"
                >
                  <Download className="w-4 h-4 mr-2" />
                  Download PDF
                </Button>
                <Button
                  onClick={() => setViewingInvoice(null)}
                  variant="outline"
                >
                  Fechar
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
