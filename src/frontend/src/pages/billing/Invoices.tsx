import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Download, Search, Eye, CheckCircle, X, FileText, RefreshCw } from 'lucide-react';
import { Button, AlertContainer, useAlerts, ConfirmDialog } from '@/components/ui';
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
  const [paymentModalInvoice, setPaymentModalInvoice] = useState<Invoice | null>(null);
  const [paymentDate, setPaymentDate] = useState<string>('');
  
  // Seleção em lote
  const [selectedInvoices, setSelectedInvoices] = useState<Set<string>>(new Set());
  const [batchProcessing, setBatchProcessing] = useState(false);
  
  // Sistema de alertas
  const { alerts, showSuccess, showError, showWarning, removeAlert } = useAlerts();
  
  // Modais de confirmação
  const [confirmDialog, setConfirmDialog] = useState<{
    isOpen: boolean;
    title: string;
    message: string;
    onConfirm: () => void;
    confirmText?: string;
    confirmVariant?: 'primary' | 'danger';
  }>({
    isOpen: false,
    title: '',
    message: '',
    onConfirm: () => {},
  });
  const [confirmLoading, setConfirmLoading] = useState(false);
  
  // Modal de geração de faturas
  const [generateModalOpen, setGenerateModalOpen] = useState(false);
  const [generateMonth, setGenerateMonth] = useState<number>(new Date().getMonth() + 1);
  const [generateYear, setGenerateYear] = useState<number>(new Date().getFullYear());

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
      // Limpar seleções ao recarregar
      setSelectedInvoices(new Set());
    } catch (err: any) {
      console.error('Erro ao carregar faturas:', err);
      const errorMsg = err.response?.data?.message || err.message || 'Erro ao carregar faturas';
      setError(errorMsg);
      showError(errorMsg);
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

  const handleMarkAsPaid = (invoice: Invoice) => {
    setPaymentModalInvoice(invoice);
    // Definir data atual como padrão
    const today = new Date().toISOString().split('T')[0];
    setPaymentDate(today);
  };
  
  const handleConfirmPayment = async () => {
    if (!paymentModalInvoice) return;
    
    try {
      await invoicesApi.markAsPaid(paymentModalInvoice.id, paymentDate ? new Date(paymentDate) : undefined);
      showSuccess(`Fatura ${paymentModalInvoice.invoiceNumber} marcada como paga com sucesso!`);
      setPaymentModalInvoice(null);
      setPaymentDate('');
      loadInvoices();
    } catch (err: any) {
      showError(err.response?.data?.message || 'Erro ao marcar fatura como paga');
    }
  };

  const handleCancel = async (id: string) => {
    const invoice = invoices.find((inv: Invoice) => inv.id === id);
    
    setConfirmDialog({
      isOpen: true,
      title: 'Cancelar Fatura',
      message: `Tem certeza que deseja cancelar a fatura ${invoice?.invoiceNumber}?`,
      confirmText: 'Sim, cancelar',
      confirmVariant: 'danger',
      onConfirm: async () => {
        setConfirmLoading(true);
        try {
          console.log('Cancelando fatura:', id);
          const result = await invoicesApi.cancel(id);
          console.log('Resultado do cancelamento:', result);
          showSuccess(`Fatura ${invoice?.invoiceNumber} cancelada com sucesso!`);
          setConfirmDialog((prev) => ({ ...prev, isOpen: false }));
          loadInvoices();
        } catch (err: any) {
          console.error('Erro ao cancelar fatura:', err);
          console.error('Resposta do erro:', err.response);
          showError(err.response?.data?.message || 'Erro ao cancelar fatura');
        } finally {
          setConfirmLoading(false);
        }
      },
    });
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
      showSuccess(`PDF da fatura ${invoice.invoiceNumber} baixado com sucesso!`);
    } catch (err: any) {
      showError(err.response?.data?.message || 'Erro ao baixar PDF');
    }
  };

  const handleGenerateMonthly = async () => {
    // Abrir modal para selecionar período
    setGenerateModalOpen(true);
  };

  const handleConfirmGenerate = async () => {
    setConfirmLoading(true);
    setGenerating(true);
    try {
      const result = await invoicesApi.generateMonthly(generateMonth, generateYear);
      const monthName = new Date(generateYear, generateMonth - 1).toLocaleString('pt-BR', { month: 'long' });
      showSuccess(`Faturas de ${monthName}/${generateYear} geradas com sucesso!`);
      setGenerateModalOpen(false);
      loadInvoices();
    } catch (err: any) {
      showError(err.response?.data?.message || 'Erro ao gerar faturas mensais');
    } finally {
      setConfirmLoading(false);
      setGenerating(false);
    }
  };

  // Funções de seleção em lote
  const toggleSelectInvoice = (id: string) => {
    setSelectedInvoices((prev: Set<string>) => {
      const newSet = new Set(prev);
      if (newSet.has(id)) {
        newSet.delete(id);
      } else {
        newSet.add(id);
      }
      return newSet;
    });
  };

  const toggleSelectAll = () => {
    if (selectedInvoices.size === filteredInvoices.length) {
      setSelectedInvoices(new Set());
    } else {
      setSelectedInvoices(new Set(filteredInvoices.map((inv: Invoice) => inv.id)));
    }
  };

  const handleBatchCancel = async () => {
    const count = selectedInvoices.size;
    if (count === 0) {
      showWarning('Nenhuma fatura selecionada');
      return;
    }

    setConfirmDialog({
      isOpen: true,
      title: 'Cancelar Faturas',
      message: `Tem certeza que deseja cancelar ${count} fatura(s) selecionada(s)?`,
      confirmText: 'Sim, cancelar todas',
      confirmVariant: 'danger',
      onConfirm: async () => {
        setConfirmLoading(true);
        setBatchProcessing(true);
        try {
          let success = 0;
          let failed = 0;

          for (const id of Array.from(selectedInvoices)) {
            try {
              console.log('Cancelando fatura em lote:', id);
              await invoicesApi.cancel(id as string);
              success++;
            } catch (err) {
              console.error(`Erro ao cancelar fatura ${id}:`, err);
              failed++;
            }
          }

          if (success > 0) {
            showSuccess(`${success} fatura(s) cancelada(s) com sucesso!`);
          }
          if (failed > 0) {
            showError(`Falha ao cancelar ${failed} fatura(s)`);
          }

          setSelectedInvoices(new Set());
          setConfirmDialog((prev) => ({ ...prev, isOpen: false }));
          loadInvoices();
        } catch (err: any) {
          showError('Erro ao processar cancelamento em lote');
        } finally {
          setConfirmLoading(false);
          setBatchProcessing(false);
        }
      },
    });
  };

  const handleBatchPay = async () => {
    const count = selectedInvoices.size;
    if (count === 0) {
      showWarning('Nenhuma fatura selecionada');
      return;
    }

    // Verificar se todas as faturas selecionadas são pendentes
    const selectedInvoiceObjects = invoices.filter((inv: Invoice) => selectedInvoices.has(inv.id));
    const nonPendingCount = selectedInvoiceObjects.filter((inv: Invoice) => inv.status.toLowerCase() !== 'pending').length;

    if (nonPendingCount > 0) {
      showWarning(`${nonPendingCount} fatura(s) selecionada(s) não estão pendentes e serão ignoradas`);
    }

    const pendingInvoices = selectedInvoiceObjects.filter((inv: Invoice) => inv.status.toLowerCase() === 'pending');
    if (pendingInvoices.length === 0) {
      showWarning('Nenhuma fatura pendente selecionada');
      return;
    }

    setConfirmDialog({
      isOpen: true,
      title: 'Marcar como Pagas',
      message: `Tem certeza que deseja marcar ${pendingInvoices.length} fatura(s) como paga(s)?`,
      confirmText: 'Sim, marcar como pagas',
      confirmVariant: 'primary',
      onConfirm: async () => {
        setConfirmLoading(true);
        setBatchProcessing(true);
        try {
          let success = 0;
          let failed = 0;
          const today = new Date();

          for (const invoice of pendingInvoices) {
            try {
              await invoicesApi.markAsPaid(invoice.id, today);
              success++;
            } catch (err) {
              console.error(`Erro ao marcar fatura ${invoice.id} como paga:`, err);
              failed++;
            }
          }

          if (success > 0) {
            showSuccess(`${success} fatura(s) marcada(s) como paga(s) com sucesso!`);
          }
          if (failed > 0) {
            showError(`Falha ao processar ${failed} fatura(s)`);
          }

          setSelectedInvoices(new Set());
          setConfirmDialog((prev) => ({ ...prev, isOpen: false }));
          loadInvoices();
        } catch (err: any) {
          showError('Erro ao processar pagamento em lote');
        } finally {
          setConfirmLoading(false);
          setBatchProcessing(false);
        }
      },
    });
  };

  const filteredInvoices = invoices.filter((invoice: Invoice) => {
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

    const config = statusConfig[status.toLowerCase()] || statusConfig.pending;
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
    <div className="page-container">
      {/* Container de Alertas */}
      <AlertContainer alerts={alerts} onRemove={removeAlert} />
      
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="page-title">Billing</h1>
          <p className="page-subtitle">Gerencie todas as faturas dos seus clientes</p>
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

      {/* Navigation Tabs */}
      <div className="flex gap-2 mb-8 border-b border-border pb-0">
        <Link to="/billing" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Dashboard
        </Link>
        <Link to="/billing/plans" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Planos
        </Link>
        <Link to="/billing/clients" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Clientes & Assinaturas
        </Link>
        <Link to="/billing/invoices" className="px-4 py-3 font-medium text-primary border-b-2 border-primary">
          Faturas
        </Link>
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

      {/* Ações em Lote */}
      {selectedInvoices.size > 0 && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 flex items-center justify-between">
          <div className="flex items-center gap-4">
            <span className="text-sm font-medium text-blue-900">
              {selectedInvoices.size} fatura(s) selecionada(s)
            </span>
            <button
              onClick={() => setSelectedInvoices(new Set())}
              className="text-sm text-blue-600 hover:text-blue-800 underline"
            >
              Limpar seleção
            </button>
          </div>
          <div className="flex gap-2">
            <Button
              onClick={handleBatchPay}
              disabled={batchProcessing}
              className="bg-green-600 hover:bg-green-700 text-white"
            >
              <CheckCircle className="w-4 h-4 mr-2" />
              {batchProcessing ? 'Processando...' : 'Marcar como Pagas'}
            </Button>
            <Button
              onClick={handleBatchCancel}
              disabled={batchProcessing}
              className="bg-red-600 hover:bg-red-700 text-white"
            >
              <X className="w-4 h-4 mr-2" />
              {batchProcessing ? 'Processando...' : 'Cancelar Selecionadas'}
            </Button>
          </div>
        </div>
      )}

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
                <th className="px-6 py-3 text-left">
                  <input
                    type="checkbox"
                    checked={filteredInvoices.length > 0 && selectedInvoices.size === filteredInvoices.length}
                    onChange={toggleSelectAll}
                    className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  />
                </th>
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
                  <td colSpan={9} className="px-6 py-8 text-center text-gray-500">
                    <FileText className="w-12 h-12 mx-auto mb-2 text-gray-400" />
                    Nenhuma fatura encontrada
                  </td>
                </tr>
              ) : (
                filteredInvoices.map((invoice) => (
                  <tr key={invoice.id} className={`hover:bg-gray-50 ${selectedInvoices.has(invoice.id) ? 'bg-blue-50' : ''}`}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <input
                        type="checkbox"
                        checked={selectedInvoices.has(invoice.id)}
                        onChange={() => toggleSelectInvoice(invoice.id)}
                        className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                      />
                    </td>
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
                        {invoice.status.toLowerCase() === 'pending' && (
                          <button
                            onClick={() => handleMarkAsPaid(invoice)}
                            className="text-green-600 hover:text-green-900"
                            title="Marcar como Paga"
                          >
                            <CheckCircle className="w-4 h-4" />
                          </button>
                        )}
                        {invoice.status.toLowerCase() !== 'cancelled' && invoice.status.toLowerCase() !== 'paid' && (
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

      {/* Modal de Registro de Pagamento */}
      {paymentModalInvoice && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900">Registrar Pagamento</h3>
              <button
                onClick={() => {
                  setPaymentModalInvoice(null);
                  setPaymentDate('');
                }}
                className="text-gray-400 hover:text-gray-600"
              >
                <X className="w-6 h-6" />
              </button>
            </div>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Fatura
                </label>
                <p className="text-sm text-gray-900 font-semibold">{paymentModalInvoice.invoiceNumber}</p>
                <p className="text-sm text-gray-500">{paymentModalInvoice.clientName}</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Valor
                </label>
                <p className="text-lg font-bold text-green-600">{formatCurrency(paymentModalInvoice.amount)}</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Data de Pagamento *
                </label>
                <input
                  type="date"
                  value={paymentDate}
                  onChange={(e) => setPaymentDate(e.target.value)}
                  max={new Date().toISOString().split('T')[0]}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  required
                />
                <p className="mt-1 text-xs text-gray-500">
                  Data em que o pagamento foi recebido
                </p>
              </div>
            </div>

            <div className="mt-6 flex gap-2">
              <Button
                onClick={handleConfirmPayment}
                disabled={!paymentDate}
                className="flex-1 bg-green-600 hover:bg-green-700 text-white disabled:bg-gray-400"
              >
                <CheckCircle className="w-4 h-4 mr-2" />
                Confirmar Pagamento
              </Button>
              <Button
                onClick={() => {
                  setPaymentModalInvoice(null);
                  setPaymentDate('');
                }}
                variant="outline"
              >
                Cancelar
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Modal de Geração de Faturas */}
      {generateModalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-md w-full shadow-xl animate-slide-in-top">
            <div className="p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">
                Gerar Faturas Mensais
              </h3>
              
              <p className="text-sm text-gray-600 mb-4">
                Selecione o período de referência para gerar as faturas de todas as assinaturas ativas.
              </p>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Mês *
                  </label>
                  <select
                    value={generateMonth}
                    onChange={(e) => setGenerateMonth(Number(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    <option value={1}>Janeiro</option>
                    <option value={2}>Fevereiro</option>
                    <option value={3}>Março</option>
                    <option value={4}>Abril</option>
                    <option value={5}>Maio</option>
                    <option value={6}>Junho</option>
                    <option value={7}>Julho</option>
                    <option value={8}>Agosto</option>
                    <option value={9}>Setembro</option>
                    <option value={10}>Outubro</option>
                    <option value={11}>Novembro</option>
                    <option value={12}>Dezembro</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Ano *
                  </label>
                  <input
                    type="number"
                    value={generateYear}
                    onChange={(e) => setGenerateYear(Number(e.target.value))}
                    min={2000}
                    max={2100}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div className="bg-blue-50 border border-blue-200 rounded-md p-3">
                  <p className="text-xs text-blue-800">
                    <strong>Período selecionado:</strong> {new Date(generateYear, generateMonth - 1).toLocaleString('pt-BR', { month: 'long' })} de {generateYear}
                  </p>
                </div>
              </div>
            </div>

            <div className="bg-gray-50 px-6 py-4 flex gap-3 justify-end rounded-b-lg">
              <Button
                onClick={() => setGenerateModalOpen(false)}
                disabled={confirmLoading}
                variant="secondary"
              >
                Cancelar
              </Button>
              <Button
                onClick={handleConfirmGenerate}
                disabled={confirmLoading}
                className="bg-blue-600 hover:bg-blue-700 text-white"
                loading={confirmLoading}
              >
                Gerar Faturas
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Modal de Confirmação */}
      <ConfirmDialog
        isOpen={confirmDialog.isOpen}
        title={confirmDialog.title}
        message={confirmDialog.message}
        confirmText={confirmDialog.confirmText}
        confirmVariant={confirmDialog.confirmVariant}
        onConfirm={confirmDialog.onConfirm}
        onCancel={() => setConfirmDialog((prev: any) => ({ ...prev, isOpen: false }))}
        isLoading={confirmLoading}
      />
    </div>
  );
}
