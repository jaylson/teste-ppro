import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Search, Eye, Edit, Ban, CheckCircle, Calendar, DollarSign } from 'lucide-react';
import { Button } from '@/components/ui';
import { ClientModal, SubscriptionModal } from '@/components/modals';
import { clientsApi, subscriptionsApi, plansApi, type Client as ApiClient, type ClientListItem, type SubscriptionListItem, type Plan } from '@/services/api';

interface Client {
  id: string;
  name: string;
  email: string;
  document: string;
  type: 'individual' | 'company';
  status: 'active' | 'suspended' | 'cancelled';
  createdAt: string;
}

interface Subscription {
  id: string;
  clientId: string;
  clientName: string;
  planId: string;
  planName: string;
  planPrice: number;
  status: 'active' | 'suspended' | 'cancelled' | 'pending';
  startDate: string;
  endDate?: string;
  billingCycle: 'monthly' | 'yearly';
  autoRenew: boolean;
  companiesCount: number;
  usersCount: number;
}

export default function ClientsSubscriptions() {
  const [activeTab, setActiveTab] = useState<'clients' | 'subscriptions'>('clients');
  const [searchTerm, setSearchTerm] = useState('');
  const [showClientModal, setShowClientModal] = useState(false);
  const [showSubscriptionModal, setShowSubscriptionModal] = useState(false);
  const [editingClient, setEditingClient] = useState<Client | null>(null);
  const [editingSubscription, setEditingSubscription] = useState<Subscription | null>(null);
  const [clients, setClients] = useState<Client[]>([]);
  const [subscriptions, setSubscriptions] = useState<Subscription[]>([]);
  const [plans, setPlans] = useState<Plan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [viewingClient, setViewingClient] = useState<Client | null>(null);
  const [viewingSubscription, setViewingSubscription] = useState<Subscription | null>(null);

  // Carregar clientes, assinaturas e planos da API
  useEffect(() => {
    loadClients();
    loadSubscriptions();
    loadPlans();
  }, []);

  const loadClients = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await clientsApi.getAll();
      setClients(data.map(item => ({
        id: item.id,
        name: item.name,
        email: item.email,
        document: item.document,
        type: item.type,
        status: item.status,
        createdAt: item.createdAt
      })));
    } catch (err: any) {
      console.error('Erro ao carregar clientes:', err);
      console.error('Detalhes do erro:', {
        message: err.message,
        response: err.response?.data,
        status: err.response?.status
      });
      setError(err.response?.data?.message || err.message || 'Erro ao carregar clientes');
      // Manter dados mockados em caso de erro para demonstração
      setClients([
    {
      id: '1',
      name: 'TechStartup Ltda',
      email: 'contato@techstartup.com',
      document: '12.345.678/0001-90',
      type: 'company',
      status: 'active',
      createdAt: '2025-01-15',
    },
    {
      id: '2',
      name: 'João Silva',
      email: 'joao@email.com',
      document: '123.456.789-00',
      type: 'individual',
      status: 'active',
      createdAt: '2025-01-10',
    },
    {
      id: '3',
      name: 'InnovaCorp S.A.',
      email: 'financeiro@innovacorp.com',
      document: '98.765.432/0001-10',
      type: 'company',
      status: 'suspended',
      createdAt: '2024-12-20',
    },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const loadSubscriptions = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await subscriptionsApi.getAll();
      setSubscriptions(data.map(item => ({
        id: item.id,
        clientId: item.clientId,
        clientName: item.clientName,
        planId: item.planId,
        planName: item.planName,
        planPrice: item.planPrice,
        status: item.status,
        startDate: item.startDate,
        endDate: item.endDate,
        billingCycle: item.billingCycle,
        autoRenew: item.autoRenew,
        companiesCount: item.companiesCount,
        usersCount: item.usersCount
      })));
    } catch (err: any) {
      console.error('Erro ao carregar assinaturas:', err);
      setError(err.response?.data?.message || err.message || 'Erro ao carregar assinaturas');
      // Manter dados mockados em caso de erro para demonstração
      setSubscriptions([
        {
          id: '1',
          clientId: '1',
          clientName: 'TechStartup Ltda',
          planId: '2',
          planName: 'Professional',
          planPrice: 299.00,
          status: 'active',
          startDate: '2025-01-15',
          billingCycle: 'monthly',
          autoRenew: true,
          companiesCount: 2,
          usersCount: 12,
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const loadPlans = async () => {
    try {
      const data = await plansApi.getActive();
      setPlans(data);
    } catch (err: any) {
      console.error('Erro ao carregar planos:', err);
      // Fallback para planos mockados
      setPlans([
        { id: '1', name: 'Starter', description: '', price: 99.00, billingCycle: 'monthly', maxCompanies: 1, maxUsers: 5, isActive: true },
        { id: '2', name: 'Professional', description: '', price: 299.00, billingCycle: 'monthly', maxCompanies: 3, maxUsers: 20, isActive: true },
        { id: '3', name: 'Enterprise', description: '', price: 999.00, billingCycle: 'monthly', maxCompanies: -1, maxUsers: -1, isActive: true },
      ]);
    }
  };

  const getStatusBadge = (status: string) => {
    const variants: Record<string, string> = {
      active: 'badge-success',
      suspended: 'badge-warning',
      cancelled: 'badge-error',
      pending: 'badge-info',
    };
    const labels: Record<string, string> = {
      active: 'Ativo',
      suspended: 'Suspenso',
      cancelled: 'Cancelado',
      pending: 'Pendente',
    };
    return <span className={`badge ${variants[status]}`}>{labels[status]}</span>;
  };

  const handleSaveClient = async (client: Client) => {
    try {
      if (client.id) {
        // Atualizar
        await clientsApi.update(client.id, client);
      } else {
        // Criar
        await clientsApi.create(client);
      }
      // Recarregar lista
      await loadClients();
    } catch (err: any) {
      console.error('Erro ao salvar cliente:', err);
      alert(err.response?.data?.message || 'Erro ao salvar cliente');
    }
  };

  const handleSaveSubscription = async (subscription: Subscription) => {
    try {
      console.log('Salvando assinatura:', subscription);
      if (subscription.id) {
        // Atualizar
        const updateData = {
          planId: subscription.planId,
          autoRenew: subscription.autoRenew,
          companiesCount: subscription.companiesCount,
          usersCount: subscription.usersCount,
          startDate: subscription.startDate,
          endDate: subscription.endDate || undefined
        };
        console.log('Dados de atualização:', updateData);
        await subscriptionsApi.update(subscription.id, updateData);
      } else {
        // Criar
        await subscriptionsApi.create({
          clientId: subscription.clientId,
          planId: subscription.planId,
          autoRenew: subscription.autoRenew
        });
      }
      // Recarregar lista
      await loadSubscriptions();
      setShowSubscriptionModal(false);
      setEditingSubscription(null);
      alert('Assinatura salva com sucesso!');
    } catch (err: any) {
      console.error('Erro ao salvar assinatura:', err);
      alert(err.response?.data?.message || 'Erro ao salvar assinatura');
    }
  };

  const handleEditClient = async (client: Client) => {
    try {
      // Carregar dados completos do cliente
      const fullClient = await clientsApi.getById(client.id!);
      setEditingClient(fullClient);
      setShowClientModal(true);
    } catch (err: any) {
      console.error('Erro ao carregar cliente:', err);
      alert('Erro ao carregar dados do cliente');
    }
  };

  const handleViewClient = async (client: Client) => {
    try {
      const fullClient = await clientsApi.getById(client.id!);
      setViewingClient(fullClient);
    } catch (err: any) {
      console.error('Erro ao carregar cliente:', err);
      alert('Erro ao carregar dados do cliente');
    }
  };

  const handleNewClient = () => {
    setEditingClient(null);
    setShowClientModal(true);
  };

  const handleEditSubscription = (subscription: Subscription) => {
    setEditingSubscription(subscription);
    setShowSubscriptionModal(true);
  };

  const handleViewSubscription = (subscription: Subscription) => {
    setViewingSubscription(subscription);
  };

  const handleSuspendSubscription = async (subscriptionId: string) => {
    if (!confirm('Deseja realmente suspender esta assinatura?')) return;
    
    try {
      await subscriptionsApi.suspend(subscriptionId);
      await loadSubscriptions();
      alert('Assinatura suspensa com sucesso!');
    } catch (err: any) {
      console.error('Erro ao suspender assinatura:', err);
      alert(err.response?.data?.message || 'Erro ao suspender assinatura');
    }
  };

  const handleActivateSubscription = async (subscriptionId: string) => {
    if (!confirm('Deseja realmente ativar esta assinatura?')) return;
    
    try {
      await subscriptionsApi.activate(subscriptionId);
      await loadSubscriptions();
      alert('Assinatura ativada com sucesso!');
    } catch (err: any) {
      console.error('Erro ao ativar assinatura:', err);
      alert(err.response?.data?.message || 'Erro ao ativar assinatura');
    }
  };

  const filteredClients = clients.filter(client =>
    client.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    client.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
    client.document.includes(searchTerm)
  );

  const filteredSubscriptions = subscriptions.filter(sub =>
    sub.clientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    sub.planName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="page-container">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="page-title">Billing</h1>
          <p className="page-subtitle">Gerencie clientes e suas assinaturas ativas</p>
        </div>
        <Button onClick={activeTab === 'clients' ? handleNewClient : () => setShowSubscriptionModal(true)}>
          <Plus className="w-4 h-4 mr-2" />
          {activeTab === 'clients' ? 'Novo Cliente' : 'Nova Assinatura'}
        </Button>
      </div>

      {/* Navigation Tabs */}
      <div className="flex gap-2 mb-6 border-b border-border pb-0">
        <Link to="/billing" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Dashboard
        </Link>
        <Link to="/billing/plans" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Planos
        </Link>
        <Link to="/billing/clients" className="px-4 py-3 font-medium text-primary border-b-2 border-primary">
          Clientes & Assinaturas
        </Link>
        <Link to="/billing/invoices" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Faturas
        </Link>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">Total de Clientes</div>
          <div className="text-3xl font-bold">{clients.length}</div>
        </div>
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">Assinaturas Ativas</div>
          <div className="text-3xl font-bold text-green-600">
            {subscriptions.filter(s => s.status === 'active').length}
          </div>
        </div>
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">Suspensas</div>
          <div className="text-3xl font-bold text-yellow-600">
            {subscriptions.filter(s => s.status === 'suspended').length}
          </div>
        </div>
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">MRR Total</div>
          <div className="text-3xl font-bold text-primary">
            R$ {subscriptions
              .filter(s => s.status === 'active')
              .reduce((acc, s) => acc + (s.billingCycle === 'monthly' ? s.planPrice : s.planPrice / 12), 0)
              .toFixed(2)}
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-4 mb-6 border-b border-border">
        <button
          className={`pb-3 px-4 font-medium transition-colors ${
            activeTab === 'clients'
              ? 'text-primary border-b-2 border-primary'
              : 'text-muted-foreground hover:text-foreground'
          }`}
          onClick={() => setActiveTab('clients')}
        >
          Clientes ({clients.length})
        </button>
        <button
          className={`pb-3 px-4 font-medium transition-colors ${
            activeTab === 'subscriptions'
              ? 'text-primary border-b-2 border-primary'
              : 'text-muted-foreground hover:text-foreground'
          }`}
          onClick={() => setActiveTab('subscriptions')}
        >
          Assinaturas ({subscriptions.length})
        </button>
      </div>

      {/* Search */}
      <div className="mb-6">
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded-lg mb-4 flex items-center justify-between">
            <div>
              <strong>Erro:</strong> {error}
              <div className="text-sm mt-1">Usando dados de demonstração. Verifique se o backend está rodando.</div>
            </div>
            <button onClick={() => setError(null)} className="text-red-600 hover:text-red-800">×</button>
          </div>
        )}
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-muted-foreground" />
          <input
            type="text"
            placeholder={activeTab === 'clients' ? 'Buscar por nome, email ou documento...' : 'Buscar por cliente ou plano...'}
            className="input pl-10 w-full"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </div>

      {/* Clients Table */}
      {activeTab === 'clients' && (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="table">
              <thead>
                <tr>
                  <th>Cliente</th>
                  <th>Tipo</th>
                  <th>Documento</th>
                  <th>Data Cadastro</th>
                  <th>Status</th>
                  <th className="text-right">Ações</th>
                </tr>
              </thead>
              <tbody>
                {filteredClients.map((client) => (
                  <tr key={client.id}>
                    <td>
                      <div>
                        <div className="font-medium">{client.name}</div>
                        <div className="text-sm text-muted-foreground">{client.email}</div>
                      </div>
                    </td>
                    <td>
                      <span className="badge badge-outline">
                        {client.type === 'company' ? 'Empresa' : 'Pessoa Física'}
                      </span>
                    </td>
                    <td className="font-mono text-sm">{client.document}</td>
                    <td>{new Date(client.createdAt).toLocaleDateString('pt-BR')}</td>
                    <td>{getStatusBadge(client.status)}</td>
                    <td className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button variant="ghost" size="sm" onClick={() => handleViewClient(client)} title="Visualizar">
                          <Eye className="w-4 h-4" />
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleEditClient(client)} title="Editar">
                          <Edit className="w-4 h-4" />
                        </Button>
                        <Button variant="ghost" size="sm">
                          <Ban className="w-4 h-4 text-red-500" />
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Subscriptions Table */}
      {activeTab === 'subscriptions' && (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="table">
              <thead>
                <tr>
                  <th>Cliente</th>
                  <th>Plano</th>
                  <th>Valor</th>
                  <th>Ciclo</th>
                  <th>Início</th>
                  <th>Uso</th>
                  <th>Status</th>
                  <th className="text-right">Ações</th>
                </tr>
              </thead>
              <tbody>
                {filteredSubscriptions.map((sub) => (
                  <tr key={sub.id}>
                    <td className="font-medium">{sub.clientName}</td>
                    <td>
                      <span className="badge badge-primary badge-outline">
                        {sub.planName}
                      </span>
                    </td>
                    <td className="font-semibold">
                      R$ {sub.planPrice.toFixed(2)}
                    </td>
                    <td>
                      <div className="flex items-center gap-1 text-sm text-muted-foreground">
                        <Calendar className="w-4 h-4" />
                        {sub.billingCycle === 'monthly' ? 'Mensal' : 'Anual'}
                      </div>
                    </td>
                    <td>{new Date(sub.startDate).toLocaleDateString('pt-BR')}</td>
                    <td>
                      <div className="text-sm">
                        <div>{sub.companiesCount} empresas</div>
                        <div className="text-muted-foreground">{sub.usersCount} usuários</div>
                      </div>
                    </td>
                    <td>{getStatusBadge(sub.status)}</td>
                    <td className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button variant="ghost" size="sm" onClick={() => handleViewSubscription(sub)}>
                          <Eye className="w-4 h-4" />
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleEditSubscription(sub)}>
                          <Edit className="w-4 h-4" />
                        </Button>
                        {sub.status === 'active' ? (
                          <Button 
                            variant="ghost" 
                            size="sm" 
                            onClick={() => handleSuspendSubscription(sub.id)}
                            title="Suspender assinatura"
                          >
                            <Ban className="w-4 h-4 text-yellow-500" />
                          </Button>
                        ) : (
                          <Button 
                            variant="ghost" 
                            size="sm" 
                            onClick={() => handleActivateSubscription(sub.id)}
                            title="Ativar assinatura"
                          >
                            <CheckCircle className="w-4 h-4 text-green-500" />
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
      
      {/* Modals */}
      <ClientModal
        isOpen={showClientModal}
        onClose={() => {
          setShowClientModal(false);
          setEditingClient(null);
        }}
        onSave={handleSaveClient}
        client={editingClient}
      />

      <SubscriptionModal
        isOpen={showSubscriptionModal}
        onClose={() => {
          setShowSubscriptionModal(false);
          setEditingSubscription(null);
        }}
        onSave={handleSaveSubscription}
        subscription={editingSubscription}
        clients={clients.map(c => ({ id: c.id, name: c.name }))}
        plans={plans}
      />

      {/* Modal de Visualização */}
      {viewingClient && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onClick={() => setViewingClient(null)}>
          <div className="bg-background rounded-lg shadow-xl max-w-2xl w-full m-4 max-h-[90vh] overflow-y-auto" onClick={(e) => e.stopPropagation()}>
            <div className="p-6 border-b border-border flex justify-between items-center">
              <h2 className="text-xl font-semibold">Detalhes do Cliente</h2>
              <button onClick={() => setViewingClient(null)} className="text-muted-foreground hover:text-foreground">
                ×
              </button>
            </div>
            <div className="p-6 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Nome</label>
                  <p className="text-base">{viewingClient.name}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Tipo</label>
                  <p className="text-base">{viewingClient.type === 'company' ? 'Empresa' : 'Pessoa Física'}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Email</label>
                  <p className="text-base">{viewingClient.email}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Documento</label>
                  <p className="text-base">{viewingClient.document}</p>
                </div>
                {viewingClient.phone && (
                  <div>
                    <label className="text-sm font-medium text-muted-foreground">Telefone</label>
                    <p className="text-base">{viewingClient.phone}</p>
                  </div>
                )}
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Status</label>
                  <p className="text-base">{getStatusBadge(viewingClient.status)}</p>
                </div>
                {viewingClient.address && (
                  <div className="col-span-2">
                    <label className="text-sm font-medium text-muted-foreground">Endereço</label>
                    <p className="text-base">{viewingClient.address}</p>
                  </div>
                )}
                {viewingClient.city && (
                  <div>
                    <label className="text-sm font-medium text-muted-foreground">Cidade</label>
                    <p className="text-base">{viewingClient.city} - {viewingClient.state}</p>
                  </div>
                )}
                {viewingClient.zipCode && (
                  <div>
                    <label className="text-sm font-medium text-muted-foreground">CEP</label>
                    <p className="text-base">{viewingClient.zipCode}</p>
                  </div>
                )}
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Data de Cadastro</label>
                  <p className="text-base">{new Date(viewingClient.createdAt!).toLocaleDateString('pt-BR')}</p>
                </div>
              </div>
            </div>
            <div className="p-6 border-t border-border flex justify-end gap-2">
              <Button variant="outline" onClick={() => setViewingClient(null)}>
                Fechar
              </Button>
              <Button onClick={() => {
                setEditingClient(viewingClient);
                setViewingClient(null);
                setShowClientModal(true);
              }}>
                <Edit className="w-4 h-4 mr-2" />
                Editar
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Modal de Visualização de Assinatura */}
      {viewingSubscription && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onClick={() => setViewingSubscription(null)}>
          <div className="bg-background rounded-lg shadow-xl max-w-2xl w-full m-4 max-h-[90vh] overflow-y-auto" onClick={(e) => e.stopPropagation()}>
            <div className="p-6 border-b border-border flex justify-between items-center">
              <h2 className="text-xl font-semibold">Detalhes da Assinatura</h2>
              <button onClick={() => setViewingSubscription(null)} className="text-muted-foreground hover:text-foreground">
                ×
              </button>
            </div>
            <div className="p-6 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Cliente</label>
                  <p className="text-base">{viewingSubscription.clientName}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Status</label>
                  <p className="text-base">{getStatusBadge(viewingSubscription.status)}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Plano</label>
                  <p className="text-base">{viewingSubscription.planName}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Valor</label>
                  <p className="text-base">R$ {viewingSubscription.planPrice.toFixed(2).replace('.', ',')}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Ciclo de Cobrança</label>
                  <p className="text-base">{viewingSubscription.billingCycle === 'monthly' ? 'Mensal' : 'Anual'}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Renovação Automática</label>
                  <p className="text-base">{viewingSubscription.autoRenew ? 'Sim' : 'Não'}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Data de Início</label>
                  <p className="text-base">{new Date(viewingSubscription.startDate).toLocaleDateString('pt-BR')}</p>
                </div>
                {viewingSubscription.endDate && (
                  <div>
                    <label className="text-sm font-medium text-muted-foreground">Data de Término</label>
                    <p className="text-base">{new Date(viewingSubscription.endDate).toLocaleDateString('pt-BR')}</p>
                  </div>
                )}
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Empresas</label>
                  <p className="text-base">{viewingSubscription.companiesCount} empresa(s)</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">Usuários</label>
                  <p className="text-base">{viewingSubscription.usersCount} usuário(s)</p>
                </div>
              </div>
            </div>
            <div className="p-6 border-t border-border flex justify-end gap-2">
              <Button variant="outline" onClick={() => setViewingSubscription(null)}>
                Fechar
              </Button>
              <Button onClick={() => {
                setEditingSubscription(viewingSubscription);
                setViewingSubscription(null);
                setShowSubscriptionModal(true);
              }}>
                <Edit className="w-4 h-4 mr-2" />
                Editar
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
