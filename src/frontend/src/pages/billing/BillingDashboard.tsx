import { useState } from 'react';
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
  Send,
  Eye,
  Calendar,
  CreditCard,
  Users,
  Package,
} from 'lucide-react';
import { Button } from '@/components/ui';
import { InvoiceModal } from '@/components/modals';

interface Invoice {
  id: string;
  clientName: string;
  amount: number;
  dueDate: string;
  issueDate: string;
  status: 'paid' | 'pending' | 'overdue' | 'cancelled';
  paymentDate?: string;
  description: string;
}

interface Payment {
  id: string;
  invoiceId: string;
  clientName: string;
  amount: number;
  paymentDate: string;
  paymentMethod: 'bank_transfer' | 'credit_card' | 'pix' | 'boleto';
  reference: string;
}

export default function BillingDashboard() {
  const [showInvoiceModal, setShowInvoiceModal] = useState(false);
  const [editingInvoice, setEditingInvoice] = useState<Invoice | null>(null);
  
  const [invoices, setInvoices] = useState<Invoice[]>([
    {
      id: 'INV-001',
      clientName: 'TechStartup Ltda',
      amount: 299.00,
      dueDate: '2025-02-15',
      issueDate: '2025-01-15',
      status: 'pending',
      description: 'Plano Professional - Janeiro/2025',
    },
    {
      id: 'INV-002',
      clientName: 'João Silva',
      amount: 99.00,
      dueDate: '2025-02-10',
      issueDate: '2025-01-10',
      status: 'paid',
      paymentDate: '2025-01-12',
      description: 'Plano Starter - Janeiro/2025',
    },
    {
      id: 'INV-003',
      clientName: 'InnovaCorp S.A.',
      amount: 999.00,
      dueDate: '2025-01-20',
      issueDate: '2024-12-20',
      status: 'overdue',
      description: 'Plano Enterprise - Janeiro/2025',
    },
    {
      id: 'INV-004',
      clientName: 'Startup XYZ',
      amount: 299.00,
      dueDate: '2025-02-01',
      issueDate: '2025-01-01',
      status: 'paid',
      paymentDate: '2025-01-03',
      description: 'Plano Professional - Janeiro/2025',
    },
  ]);

  const [payments] = useState<Payment[]>([
    {
      id: 'PAY-001',
      invoiceId: 'INV-002',
      clientName: 'João Silva',
      amount: 99.00,
      paymentDate: '2025-01-12',
      paymentMethod: 'pix',
      reference: 'PIX-ABC123',
    },
    {
      id: 'PAY-002',
      invoiceId: 'INV-004',
      clientName: 'Startup XYZ',
      amount: 299.00,
      paymentDate: '2025-01-03',
      paymentMethod: 'bank_transfer',
      reference: 'TRF-XYZ789',
    },
  ]);

  const totalRevenue = invoices
    .filter(inv => inv.status === 'paid')
    .reduce((acc, inv) => acc + inv.amount, 0);

  const pendingRevenue = invoices
    .filter(inv => inv.status === 'pending')
    .reduce((acc, inv) => acc + inv.amount, 0);

  const overdueRevenue = invoices
    .filter(inv => inv.status === 'overdue')
    .reduce((acc, inv) => acc + inv.amount, 0);

  const getStatusBadge = (status: string) => {
    const variants: Record<string, string> = {
      paid: 'badge-success',
      pending: 'badge-warning',
      overdue: 'badge-error',
      cancelled: 'badge-muted',
    };
    const labels: Record<string, string> = {
      paid: 'Pago',
      pending: 'Pendente',
      overdue: 'Vencido',
      cancelled: 'Cancelado',
    };
    return <span className={`badge ${variants[status]}`}>{labels[status]}</span>;
  };

  const getPaymentMethodLabel = (method: string) => {
    const labels: Record<string, string> = {
      bank_transfer: 'Transferência',
      credit_card: 'Cartão',
      pix: 'PIX',
      boleto: 'Boleto',
    };
    return labels[method] || method;
  };

  const clients = [
    { id: '1', name: 'TechStartup Ltda' },
    { id: '2', name: 'João Silva' },
    { id: '3', name: 'InnovaCorp S.A.' },
    { id: '4', name: 'Startup XYZ' },
  ];

  const subscriptions = [
    { id: '1', clientId: '1', planName: 'Professional', planPrice: 299.00 },
    { id: '2', clientId: '2', planName: 'Starter', planPrice: 99.00 },
    { id: '3', clientId: '3', planName: 'Enterprise', planPrice: 999.00 },
    { id: '4', clientId: '4', planName: 'Professional', planPrice: 299.00 },
  ];

  const handleSaveInvoice = (invoice: Invoice) => {
    if (invoice.id) {
      setInvoices(invoices.map(inv => inv.id === invoice.id ? invoice : inv));
    } else {
      setInvoices([...invoices, { ...invoice, id: `INV-${invoices.length + 1}` }]);
    }
  };

  const handleEditInvoice = (invoice: Invoice) => {
    setEditingInvoice(invoice);
    setShowInvoiceModal(true);
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
          <Button>
            <FileText className="w-4 h-4 mr-2" />
            Nova Fatura
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
          <div className="text-3xl font-bold">R$ {totalRevenue.toFixed(2)}</div>
          <div className="text-xs text-muted-foreground mt-1">
            {invoices.filter(i => i.status === 'paid').length} faturas pagas
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
            R$ {pendingRevenue.toFixed(2)}
          </div>
          <div className="text-xs text-muted-foreground mt-1">
            {invoices.filter(i => i.status === 'pending').length} faturas pendentes
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
            R$ {overdueRevenue.toFixed(2)}
          </div>
          <div className="text-xs text-muted-foreground mt-1">
            {invoices.filter(i => i.status === 'overdue').length} faturas vencidas
          </div>
        </div>

        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <div className="p-3 bg-blue-100 rounded-xl">
              <FileText className="w-6 h-6 text-blue-600" />
            </div>
          </div>
          <div className="text-sm text-muted-foreground mb-1">Total de Faturas</div>
          <div className="text-3xl font-bold">{invoices.length}</div>
          <div className="text-xs text-muted-foreground mt-1">
            Este mês
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Invoices */}
        <div className="card">
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-lg font-semibold">Faturas Recentes</h2>
            <Button variant="ghost" size="sm">
              Ver todas
            </Button>
          </div>
          <div className="space-y-4">
            {invoices.slice(0, 5).map((invoice) => (
              <div key={invoice.id} className="flex items-center justify-between p-4 border border-border rounded-lg hover:bg-muted/50 transition-colors">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-1">
                    <span className="font-medium">{invoice.id}</span>
                    {getStatusBadge(invoice.status)}
                  </div>
                  <div className="text-sm text-muted-foreground mb-1">
                    {invoice.clientName}
                  </div>
                  <div className="text-xs text-muted-foreground">
                    {invoice.description}
                  </div>
                </div>
                <div className="text-right ml-4">
                  <div className="font-bold text-lg mb-1">
                    R$ {invoice.amount.toFixed(2)}
                  </div>
                  <div className="text-xs text-muted-foreground flex items-center gap-1">
                    <Calendar className="w-3 h-3" />
                    Venc: {new Date(invoice.dueDate).toLocaleDateString('pt-BR')}
                  </div> onClick={() => handleEditInvoice(invoice)}
                </div>
                <div className="flex gap-2 ml-4">
                  <Button variant="ghost" size="sm">
                    <Eye className="w-4 h-4" />
                  </Button>
                  {invoice.status === 'pending' && (
                    <Button variant="ghost" size="sm">
                      <Send className="w-4 h-4" />
                    </Button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Recent Payments */}
        <div className="card">
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-lg font-semibold">Pagamentos Recentes</h2>
            <Button variant="outline" size="sm">
              <CheckCircle className="w-4 h-4 mr-2" />
              Registrar Pagamento
            </Button>
          </div>
          <div className="space-y-4">
            {payments.map((payment) => (
              <div key={payment.id} className="flex items-center justify-between p-4 border border-border rounded-lg hover:bg-muted/50 transition-colors">
                <div className="flex items-center gap-4">
                  <div className="p-3 bg-green-100 rounded-lg">
                    <CheckCircle className="w-5 h-5 text-green-600" />
                  </div>
                  <div>
                    <div className="font-medium mb-1">{payment.clientName}</div>
                    <div className="text-sm text-muted-foreground mb-1">
                      Fatura: {payment.invoiceId}
                    </div>
                    <div className="flex items-center gap-2 text-xs text-muted-foreground">
                      <span className="badge badge-sm badge-outline">
                        {getPaymentMethodLabel(payment.paymentMethod)}
                      </span>
                      <span>Ref: {payment.reference}</span>
                    </div>
                  </div>
                </div>
                <div className="text-right">
                  <div className="font-bold text-lg text-green-600 mb-1">
                    R$ {payment.amount.toFixed(2)}
                  </div>
                  <div className="text-xs text-muted-foreground">
                    {new Date(payment.paymentDate).toLocaleDateString('pt-BR')}
                  </div>
                </div>
              </div>
            ))}
            
            {payments.length === 0 && (
              <div className="text-center py-12 text-muted-foreground">
                Nenhum pagamento registrado ainda
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="mt-6 grid grid-cols-1 md:grid-cols-3 gap-4">
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

        <button className="card hover:shadow-lg transition-shadow cursor-pointer text-left">
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
        </button>
      </div>

      {/* Modal */}
      <InvoiceModal
        isOpen={showInvoiceModal}
        onClose={() => {
          setShowInvoiceModal(false);
          setEditingInvoice(null);
        }}
        onSave={handleSaveInvoice}
        invoice={editingInvoice}
        clients={clients}
        subscriptions={subscriptions}
      />
    </div>
  );
}
