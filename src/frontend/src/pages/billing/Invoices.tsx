import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Download, Search, Eye, CheckCircle, X, FileText, RefreshCw } from 'lucide-react';
import { Button, AlertContainer, useAlerts, ConfirmDialog } from '@/components/ui';
import { invoicesApi, clientsApi, plansApi, type Invoice, type InvoiceFilters, type ClientListItem, type Plan } from '@/services/api';

export default function Invoices() {
  const { t } = useTranslation();
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
      const errorMsg = err.response?.data?.message || err.message || t('billing.loadInvoicesError');
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
      showSuccess(t('billing.invoiceMarkedAsPaid', { invoiceNumber: paymentModalInvoice.invoiceNumber }));
      setPaymentModalInvoice(null);
      setPaymentDate('');
      loadInvoices();
    } catch (err: any) {
      showError(err.response?.data?.message || t('billing.invoiceMarkAsPaidError'));
    }
  };

  const handleCancel = async (id: string) => {
    const invoice = invoices.find((inv: Invoice) => inv.id === id);
    
    setConfirmDialog({
      isOpen: true,
      title: t('billing.cancelInvoice'),
      message: t('billing.cancelInvoiceConfirm', { invoiceNumber: invoice?.invoiceNumber }),
      confirmText: t('billing.yesCancelInvoice'),
      confirmVariant: 'danger',
      onConfirm: async () => {
        setConfirmLoading(true);
        try {
          console.log('Cancelando fatura:', id);
          const result = await invoicesApi.cancel(id);
          console.log('Resultado do cancelamento:', result);
          showSuccess(t('billing.invoiceCancelledSuccess', { invoiceNumber: invoice?.invoiceNumber }));
          setConfirmDialog((prev) => ({ ...prev, isOpen: false }));
          loadInvoices();
        } catch (err: any) {
          console.error('Erro ao cancelar fatura:', err);
          console.error('Resposta do erro:', err.response);
          showError(err.response?.data?.message || t('billing.invoiceCancelError'));
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
      showSuccess(t('billing.pdfDownloadedSuccess', { invoiceNumber: invoice.invoiceNumber }));
    } catch (err: any) {
      showError(err.response?.data?.message || t('billing.pdfDownloadError'));
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
      await invoicesApi.generateMonthly(generateMonth, generateYear);
      const monthName = new Date(generateYear, generateMonth - 1).toLocaleString('pt-BR', { month: 'long' });
      showSuccess(t('billing.monthlyInvoicesGeneratedSuccess', { month: monthName, year: generateYear }));
      setGenerateModalOpen(false);
      loadInvoices();
    } catch (err: any) {
      showError(err.response?.data?.message || t('billing.monthlyInvoicesGenerateError'));
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
      showWarning(t('billing.noInvoicesSelected'));
      return;
    }

    setConfirmDialog({
      isOpen: true,
      title: t('billing.cancelInvoicesTitle'),
      message: t('billing.cancelInvoicesConfirm', { count }),
      confirmText: t('billing.yesCancelAll'),
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
            showSuccess(t('billing.batchCancelledSuccess', { count: success }));
          }
          if (failed > 0) {
            showError(t('billing.batchCancelFailed', { count: failed }));
          }

          setSelectedInvoices(new Set());
          setConfirmDialog((prev) => ({ ...prev, isOpen: false }));
          loadInvoices();
        } catch (err: any) {
          showError(t('billing.batchCancelError'));
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
      showWarning(t('billing.noInvoicesSelected'));
      return;
    }

    const selectedInvoiceObjects = invoices.filter((inv: Invoice) => selectedInvoices.has(inv.id));
    const nonPendingCount = selectedInvoiceObjects.filter((inv: Invoice) => inv.status.toLowerCase() !== 'pending').length;

    if (nonPendingCount > 0) {
      showWarning(t('billing.nonPendingInvoices', { count: nonPendingCount }));
    }

    const pendingInvoices = selectedInvoiceObjects.filter((inv: Invoice) => inv.status.toLowerCase() === 'pending');
    if (pendingInvoices.length === 0) {
      showWarning(t('billing.noPendingInvoicesSelected'));
      return;
    }

    setConfirmDialog({
      isOpen: true,
      title: t('billing.markAsPaidTitle'),
      message: t('billing.markAsPaidConfirm', { count: pendingInvoices.length }),
      confirmText: t('billing.yesMarkAsPaid'),
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
            showSuccess(t('billing.batchPaidSuccess', { count: success }));
          }
          if (failed > 0) {
            showError(t('billing.batchPayFailed', { count: failed }));
          }

          setSelectedInvoices(new Set());
          setConfirmDialog((prev) => ({ ...prev, isOpen: false }));
          loadInvoices();
        } catch (err: any) {
          showError(t('billing.batchPayError'));
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
      pending: { bg: 'bg-yellow-100', text: 'text-yellow-800', label: t('billing.statusPending') },
      paid: { bg: 'bg-green-100', text: 'text-green-800', label: t('billing.statusPaidFem') },
      overdue: { bg: 'bg-red-100', text: 'text-red-800', label: t('billing.statusOverdueLabel') },
      cancelled: { bg: 'bg-gray-100', text: 'text-gray-800', label: t('billing.statusCancelledFem') },
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
          <p className="page-subtitle">{t('billing.invoicesSubtitle')}</p>
        </div>
        <Button
          onClick={handleGenerateMonthly}
          disabled={generating}
          className="bg-blue-600 hover:bg-blue-700 text-white"
        >
          <RefreshCw className={`w-4 h-4 mr-2 ${generating ? 'animate-spin' : ''}`} />
          {generating ? t('billing.generating') : t('billing.generateMonthlyInvoices')}
        </Button>
      </div>

      {/* Navigation Tabs */}
      <div className="flex gap-2 mb-8 border-b border-border pb-0">
        <Link to="/billing" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          {t('billing.dashboard')}
        </Link>
        <Link to="/billing/plans" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          {t('billing.plans')}
        </Link>
        <Link to="/billing/clients" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          {t('billing.clientsSubscriptions')}
        </Link>
        <Link to="/billing/invoices" className="px-4 py-3 font-medium text-primary border-b-2 border-primary">
          {t('billing.invoices')}
        </Link>
      </div>

      {/* Filtros */}
      <div className="bg-white shadow rounded-lg p-6">
        <h2 className="text-lg font-semibold mb-4">{t('billing.filters')}</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('billing.clients')}
            </label>
            <select
              value={filters.clientId || ''}
              onChange={(e) => setFilters({ ...filters, clientId: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">{t('billing.allClients')}</option>
              {clients.map((client) => (
                <option key={client.id} value={client.id}>
                  {client.name}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('common.status')}
            </label>
            <select
              value={filters.status || ''}
              onChange={(e) => setFilters({ ...filters, status: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">{t('billing.allStatuses')}</option>
              <option value="pending">{t('billing.statusPending')}</option>
              <option value="paid">{t('billing.statusPaidFem')}</option>
              <option value="overdue">{t('billing.statusOverdueLabel')}</option>
              <option value="cancelled">{t('billing.statusCancelledFem')}</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('billing.startDate')}
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
              {t('billing.endDate')}
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
              {t('billing.plans')}
            </label>
            <select
              value={filters.planId || ''}
              onChange={(e) => setFilters({ ...filters, planId: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">{t('billing.allPlans')}</option>
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
            {t('billing.applyFilters')}
          </Button>
          <Button onClick={handleClearFilters} variant="secondary">
            {t('billing.clearFilters')}
          </Button>
        </div>
      </div>

      {/* Busca */}
      <div className="bg-white shadow rounded-lg p-4">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
          <input
            type="text"
            placeholder={t('billing.searchInvoicesPlaceholder')}
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
              {t('billing.invoicesSelected', { count: selectedInvoices.size })}
            </span>
            <button
              onClick={() => setSelectedInvoices(new Set())}
              className="text-sm text-blue-600 hover:text-blue-800 underline"
            >
              {t('billing.clearSelection')}
            </button>
          </div>
          <div className="flex gap-2">
            <Button
              onClick={handleBatchPay}
              disabled={batchProcessing}
              className="bg-green-600 hover:bg-green-700 text-white"
            >
              <CheckCircle className="w-4 h-4 mr-2" />
              {batchProcessing ? t('billing.processing') : t('billing.markAsPaidAction')}
            </Button>
            <Button
              onClick={handleBatchCancel}
              disabled={batchProcessing}
              className="bg-red-600 hover:bg-red-700 text-white"
            >
              <X className="w-4 h-4 mr-2" />
              {batchProcessing ? t('billing.processing') : t('billing.cancelSelected')}
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
          <p className="mt-4 text-gray-600">{t('billing.loadingInvoices')}</p>
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
                  {t('billing.invoiceNumber')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  {t('billing.clientColumn')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  {t('billing.plans')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  {t('billing.period')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  {t('billing.amount')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  {t('billing.dueDate')}
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  {t('common.status')}
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  {t('common.actions')}
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredInvoices.length === 0 ? (
                <tr>
                  <td colSpan={9} className="px-6 py-8 text-center text-gray-500">
                    <FileText className="w-12 h-12 mx-auto mb-2 text-gray-400" />
                    {t('billing.noInvoices')}
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
                  {t('billing.invoiceDetails')}
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
                    <label className="block text-sm font-medium text-gray-500">{t('billing.invoiceNumber')}</label>
                    <p className="mt-1 text-sm text-gray-900">{viewingInvoice.invoiceNumber}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500">{t('common.status')}</label>
                    <div className="mt-1">{getStatusBadge(viewingInvoice.status)}</div>
                  </div>
                </div>

                <div className="border-t pt-4">
                  <h3 className="font-semibold text-gray-900 mb-2">{t('billing.clientColumn')}</h3>
                  <div className="space-y-2">
                    <p className="text-sm"><span className="font-medium">{t('common.name')}:</span> {viewingInvoice.clientName}</p>
                    <p className="text-sm"><span className="font-medium">{t('common.email')}:</span> {viewingInvoice.clientEmail}</p>
                    <p className="text-sm"><span className="font-medium">{t('billing.document')}:</span> {viewingInvoice.clientDocument}</p>
                  </div>
                </div>

                <div className="border-t pt-4">
                  <h3 className="font-semibold text-gray-900 mb-2">{t('billing.subscriptions')}</h3>
                  <div className="space-y-2">
                    <p className="text-sm"><span className="font-medium">{t('billing.plans')}:</span> {viewingInvoice.planName}</p>
                    <p className="text-sm"><span className="font-medium">{t('billing.period')}:</span> {viewingInvoice.referenceMonth.toString().padStart(2, '0')}/{viewingInvoice.referenceYear}</p>
                  </div>
                </div>

                <div className="border-t pt-4">
                  <h3 className="font-semibold text-gray-900 mb-2">{t('billing.valuesAndDates')}</h3>
                  <div className="space-y-2">
                    <p className="text-sm"><span className="font-medium">{t('billing.amount')}:</span> {formatCurrency(viewingInvoice.amount)}</p>
                    <p className="text-sm"><span className="font-medium">{t('billing.issueDate')}:</span> {formatDate(viewingInvoice.issueDate)}</p>
                    <p className="text-sm"><span className="font-medium">{t('billing.dueDate')}:</span> {formatDate(viewingInvoice.dueDate)}</p>
                    {viewingInvoice.paymentDate && (
                      <p className="text-sm"><span className="font-medium">{t('billing.paymentDate')}:</span> {formatDate(viewingInvoice.paymentDate)}</p>
                    )}
                  </div>
                </div>

                {viewingInvoice.description && (
                  <div className="border-t pt-4">
                    <h3 className="font-semibold text-gray-900 mb-2">{t('common.description')}</h3>
                    <p className="text-sm text-gray-700">{viewingInvoice.description}</p>
                  </div>
                )}

                {viewingInvoice.notes && (
                  <div className="border-t pt-4">
                    <h3 className="font-semibold text-gray-900 mb-2">{t('common.notes')}</h3>
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
                  {t('billing.downloadPdf')}
                </Button>
                <Button
                  onClick={() => setViewingInvoice(null)}
                  variant="secondary"
                >
                  {t('common.close')}
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
              <h3 className="text-lg font-semibold text-gray-900">{t('billing.registerPayment')}</h3>
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
                  {t('billing.invoices')}
                </label>
                <p className="text-sm text-gray-900 font-semibold">{paymentModalInvoice.invoiceNumber}</p>
                <p className="text-sm text-gray-500">{paymentModalInvoice.clientName}</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  {t('billing.amount')}
                </label>
                <p className="text-lg font-bold text-green-600">{formatCurrency(paymentModalInvoice.amount)}</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  {t('billing.paymentDateRequired')}
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
                  {t('billing.paymentDateHelp')}
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
                {t('billing.confirmPayment')}
              </Button>
              <Button
                onClick={() => {
                  setPaymentModalInvoice(null);
                  setPaymentDate('');
                }}
                variant="secondary"
              >
                {t('common.cancel')}
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
                {t('billing.generateMonthlyTitle')}
              </h3>
              
              <p className="text-sm text-gray-600 mb-4">
                {t('billing.generateMonthlyDesc')}
              </p>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t('billing.month')} *
                  </label>
                  <select
                    value={generateMonth}
                    onChange={(e) => setGenerateMonth(Number(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    <option value={1}>{t('billing.january')}</option>
                    <option value={2}>{t('billing.february')}</option>
                    <option value={3}>{t('billing.march')}</option>
                    <option value={4}>{t('billing.april')}</option>
                    <option value={5}>{t('billing.may')}</option>
                    <option value={6}>{t('billing.june')}</option>
                    <option value={7}>{t('billing.july')}</option>
                    <option value={8}>{t('billing.august')}</option>
                    <option value={9}>{t('billing.september')}</option>
                    <option value={10}>{t('billing.october')}</option>
                    <option value={11}>{t('billing.november')}</option>
                    <option value={12}>{t('billing.december')}</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    {t('billing.year')} *
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
                    <strong>{t('billing.selectedPeriod')}</strong> {new Date(generateYear, generateMonth - 1).toLocaleString('pt-BR', { month: 'long' })} de {generateYear}
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
                {t('common.cancel')}
              </Button>
              <Button
                onClick={handleConfirmGenerate}
                disabled={confirmLoading}
                className="bg-blue-600 hover:bg-blue-700 text-white"
                loading={confirmLoading}
              >
                {t('billing.generateInvoices')}
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
