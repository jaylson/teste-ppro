import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Edit, Trash2, Check, X } from 'lucide-react';
import { Button, AlertContainer, useAlerts, ConfirmDialog } from '@/components/ui';
import { PlanModal } from '@/components/modals';
import { plansApi, type Plan } from '@/services/api';

export default function Plans() {
  const [plans, setPlans] = useState<Plan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [editingPlan, setEditingPlan] = useState<Plan | null>(null);
  
  // Sistema de alertas
  const { alerts, showSuccess, showError, removeAlert } = useAlerts();
  
  // Modal de confirmação
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

  // Carregar planos da API
  useEffect(() => {
    loadPlans();
  }, []);

  const loadPlans = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await plansApi.getAll();
      setPlans(data);
    } catch (err: any) {
      console.error('Erro ao carregar planos:', err);
      setError(err.response?.data?.message || err.message || 'Erro ao carregar planos');
      // Dados mockados em caso de erro
      setPlans([
        {
          id: '1',
          name: 'Starter',
          description: 'Ideal para startups em estágio inicial',
          price: 99.00,
          billingCycle: 'monthly',
          features: ['1 empresa', 'Até 5 usuários', 'Cap Table básico', 'Suporte por email'],
          maxCompanies: 1,
          maxUsers: 5,
          isActive: true,
          createdAt: '2025-01-01',
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (plan: Plan) => {
    setEditingPlan(plan);
    setShowModal(true);
  };

  const handleDelete = async (planId: string) => {
    const plan = plans.find(p => p.id === planId);
    
    setConfirmDialog({
      isOpen: true,
      title: 'Excluir Plano',
      message: `Tem certeza que deseja excluir o plano "${plan?.name}"?`,
      confirmText: 'Sim, excluir',
      confirmVariant: 'danger',
      onConfirm: async () => {
        setConfirmLoading(true);
        try {
          await plansApi.delete(planId);
          await loadPlans();
          showSuccess(`Plano "${plan?.name}" excluído com sucesso!`);
          setConfirmDialog((prev) => ({ ...prev, isOpen: false }));
        } catch (err: any) {
          console.error('Erro ao deletar plano:', err);
          showError(err.response?.data?.message || 'Erro ao deletar plano');
        } finally {
          setConfirmLoading(false);
        }
      },
    });
  };

  const handleToggleStatus = async (planId: string) => {
    const plan = plans.find(p => p.id === planId);
    const newStatus = !plan?.isActive;
    
    setConfirmDialog({
      isOpen: true,
      title: newStatus ? 'Ativar Plano' : 'Desativar Plano',
      message: `Tem certeza que deseja ${newStatus ? 'ativar' : 'desativar'} o plano "${plan?.name}"?`,
      confirmText: newStatus ? 'Sim, ativar' : 'Sim, desativar',
      confirmVariant: 'primary',
      onConfirm: async () => {
        setConfirmLoading(true);
        try {
          await plansApi.toggleStatus(planId);
          await loadPlans();
          showSuccess(`Plano "${plan?.name}" ${newStatus ? 'ativado' : 'desativado'} com sucesso!`);
          setConfirmDialog((prev) => ({ ...prev, isOpen: false }));
        } catch (err: any) {
          console.error('Erro ao alterar status do plano:', err);
          showError(err.response?.data?.message || 'Erro ao alterar status do plano');
        } finally {
          setConfirmLoading(false);
        }
      },
    });
  };

  const handleSavePlan = async (plan: Plan) => {
    try {
      if (plan.id) {
        // Editar
        await plansApi.update(plan.id, plan);
        showSuccess(`Plano "${plan.name}" atualizado com sucesso!`);
      } else {
        // Criar novo
        await plansApi.create(plan);
        showSuccess(`Plano "${plan.name}" criado com sucesso!`);
      }
      await loadPlans();
      setShowModal(false);
      setEditingPlan(null);
    } catch (err: any) {
      console.error('Erro ao salvar plano:', err);
      showError(err.response?.data?.message || 'Erro ao salvar plano');
    }
  };

  return (
    <div className="page-container">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="page-title">Billing</h1>
          <p className="page-subtitle">Gerencie os planos disponíveis para seus clientes</p>
        </div>
        <Button onClick={() => setShowModal(true)}>
          <Plus className="w-4 h-4 mr-2" />
          Novo Plano
        </Button>
      </div>

      {/* Navigation Tabs */}
      <div className="flex gap-2 mb-8 border-b border-border pb-0">
        <Link to="/billing" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Dashboard
        </Link>
        <Link to="/billing/plans" className="px-4 py-3 font-medium text-primary border-b-2 border-primary">
          Planos
        </Link>
        <Link to="/billing/clients" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Clientes & Assinaturas
        </Link>
        <Link to="/billing/invoices" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          Faturas
        </Link>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">Total de Planos</div>
          <div className="text-3xl font-bold">{plans.length}</div>
        </div>
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">Planos Ativos</div>
          <div className="text-3xl font-bold text-green-600">
            {plans.filter(p => p.isActive).length}
          </div>
        </div>
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">Planos Inativos</div>
          <div className="text-3xl font-bold text-gray-400">
            {plans.filter(p => !p.isActive).length}
          </div>
        </div>
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">Preço Médio</div>
          <div className="text-3xl font-bold text-primary">
            R$ {(plans.reduce((acc, p) => acc + p.price, 0) / plans.length).toFixed(2)}
          </div>
        </div>
      </div>

      {/* Plans Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {plans.map((plan) => (
          <div key={plan.id} className={`card ${!plan.isActive ? 'opacity-60' : ''}`}>
            <div className="flex justify-between items-start mb-4">
              <div>
                <h3 className="text-xl font-bold mb-1">{plan.name}</h3>
                <p className="text-sm text-muted-foreground">{plan.description}</p>
              </div>
              <span className={`badge ${plan.isActive ? 'badge-success' : 'badge-muted'}`}>
                {plan.isActive ? 'Ativo' : 'Inativo'}
              </span>
            </div>

            <div className="mb-6">
              <div className="flex items-baseline gap-1">
                <span className="text-4xl font-bold">R$ {plan.price.toFixed(2)}</span>
                <span className="text-sm text-muted-foreground">
                  /{plan.billingCycle === 'monthly' ? 'mês' : 'ano'}
                </span>
              </div>
            </div>

            <div className="space-y-2 mb-6">
              <div className="text-sm font-semibold text-muted-foreground mb-2">Recursos:</div>
              {plan.features.map((feature, index) => (
                <div key={index} className="flex items-center gap-2 text-sm">
                  <Check className="w-4 h-4 text-green-500" />
                  <span>{feature}</span>
                </div>
              ))}
            </div>

            <div className="pt-4 border-t border-border space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Max. Empresas:</span>
                <span className="font-medium">
                  {plan.maxCompanies === -1 ? 'Ilimitado' : plan.maxCompanies}
                </span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Max. Usuários:</span>
                <span className="font-medium">
                  {plan.maxUsers === -1 ? 'Ilimitado' : plan.maxUsers}
                </span>
              </div>
            </div>

            <div className="flex gap-2 mt-6">
              <Button
                variant="outline"
                size="sm"
                className="flex-1"
                onClick={() => handleEdit(plan)}
              >
                <Edit className="w-4 h-4 mr-1" />
                Editar
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => handleToggleStatus(plan.id)}
              >
                {plan.isActive ? <X className="w-4 h-4" /> : <Check className="w-4 h-4" />}
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => handleDelete(plan.id)}
              >
                <Trash2 className="w-4 h-4 text-red-500" />
              </Button>
            </div>
          </div>
        ))}
      </div>

      {/* Modal */}
      <PlanModal
        isOpen={showModal}
        onClose={() => {
          setShowModal(false);
          setEditingPlan(null);
        }}
        onSave={handleSavePlan}
        plan={editingPlan}
      />
      
      {/* Sistema de Alertas */}
      <AlertContainer alerts={alerts} onClose={removeAlert} />
      
      {/* Modal de Confirmação */}
      <ConfirmDialog
        isOpen={confirmDialog.isOpen}
        title={confirmDialog.title}
        message={confirmDialog.message}
        confirmText={confirmDialog.confirmText}
        confirmVariant={confirmDialog.confirmVariant}
        onConfirm={confirmDialog.onConfirm}
        onCancel={() => setConfirmDialog((prev) => ({ ...prev, isOpen: false }))}
        loading={confirmLoading}
      />
    </div>
  );
}
