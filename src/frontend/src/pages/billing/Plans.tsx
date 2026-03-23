import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, Edit, Trash2, Check, X } from 'lucide-react';
import { Button, AlertContainer, useAlerts, ConfirmDialog } from '@/components/ui';
import { PlanModal } from '@/components/modals';
import { plansApi, type Plan } from '@/services/api';

export default function Plans() {
  const { t } = useTranslation();
  const [plans, setPlans] = useState<Plan[]>([]);
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
      const data = await plansApi.getAll();
      setPlans(data);
    } catch (err: any) {
      console.error('Erro ao carregar planos:', err);
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
    }
  };

  const handleEdit = (plan: Plan) => {
    setEditingPlan(plan);
    setShowModal(true);
  };

  const handleDelete = async (planId?: string) => {
    if (!planId) return;
    const plan = plans.find(p => p.id === planId);
    
    setConfirmDialog({
      isOpen: true,
      title: t('billing.deletePlan'),
      message: t('billing.deletePlanConfirm', { name: plan?.name }),
      confirmText: t('billing.yesDelete'),
      confirmVariant: 'danger',
      onConfirm: async () => {
        setConfirmLoading(true);
        try {
          await plansApi.delete(planId);
          await loadPlans();
          showSuccess(t('billing.planDeletedSuccess', { name: plan?.name }));
          setConfirmDialog((prev) => ({ ...prev, isOpen: false }));
        } catch (err: any) {
          console.error('Erro ao deletar plano:', err);
          showError(err.response?.data?.message || t('billing.planDeleteError'));
        } finally {
          setConfirmLoading(false);
        }
      },
    });
  };

  const handleToggleStatus = async (planId?: string) => {
    if (!planId) return;
    const plan = plans.find(p => p.id === planId);
    const newStatus = !plan?.isActive;
    
    setConfirmDialog({
      isOpen: true,
      title: newStatus ? t('billing.activatePlan') : t('billing.deactivatePlan'),
      message: newStatus
        ? t('billing.activatePlanConfirm', { name: plan?.name })
        : t('billing.deactivatePlanConfirm', { name: plan?.name }),
      confirmText: newStatus ? t('billing.yesActivate') : t('billing.yesDeactivate'),
      confirmVariant: 'primary',
      onConfirm: async () => {
        setConfirmLoading(true);
        try {
          await plansApi.toggleStatus(planId);
          await loadPlans();
          showSuccess(
            newStatus
              ? t('billing.planActivatedSuccess', { name: plan?.name })
              : t('billing.planDeactivatedSuccess', { name: plan?.name })
          );
          setConfirmDialog((prev) => ({ ...prev, isOpen: false }));
        } catch (err: any) {
          console.error('Erro ao alterar status do plano:', err);
          showError(err.response?.data?.message || t('billing.planStatusError'));
        } finally {
          setConfirmLoading(false);
        }
      },
    });
  };

  const handleSavePlan = async (plan: Plan) => {
    try {
      if (plan.id) {
        await plansApi.update(plan.id, plan);
        showSuccess(t('billing.planSavedSuccess', { name: plan.name }));
      } else {
        await plansApi.create(plan);
        showSuccess(t('billing.planCreatedSuccess', { name: plan.name }));
      }
      await loadPlans();
      setShowModal(false);
      setEditingPlan(null);
    } catch (err: any) {
      console.error('Erro ao salvar plano:', err);
      showError(err.response?.data?.message || t('billing.planSaveError'));
    }
  };

  return (
    <div className="page-container">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="page-title">Billing</h1>
          <p className="page-subtitle">{t('billing.managePlansSubtitlePage')}</p>
        </div>
        <Button onClick={() => setShowModal(true)}>
          <Plus className="w-4 h-4 mr-2" />
          {t('billing.newPlan')}
        </Button>
      </div>

      {/* Navigation Tabs */}
      <div className="flex gap-2 mb-8 border-b border-border pb-0">
        <Link to="/billing" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          {t('billing.dashboard')}
        </Link>
        <Link to="/billing/plans" className="px-4 py-3 font-medium text-primary border-b-2 border-primary">
          {t('billing.plans')}
        </Link>
        <Link to="/billing/clients" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          {t('billing.clientsSubscriptions')}
        </Link>
        <Link to="/billing/invoices" className="px-4 py-3 font-medium text-muted-foreground hover:text-foreground transition-colors">
          {t('billing.invoices')}
        </Link>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">{t('billing.totalPlans')}</div>
          <div className="text-3xl font-bold">{plans.length}</div>
        </div>
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">{t('billing.activePlans')}</div>
          <div className="text-3xl font-bold text-green-600">
            {plans.filter(p => p.isActive).length}
          </div>
        </div>
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">{t('billing.inactivePlans')}</div>
          <div className="text-3xl font-bold text-gray-400">
            {plans.filter(p => !p.isActive).length}
          </div>
        </div>
        <div className="card">
          <div className="text-sm text-muted-foreground mb-1">{t('billing.averagePrice')}</div>
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
                {plan.isActive ? t('common.active') : t('common.inactive')}
              </span>
            </div>

            <div className="mb-6">
              <div className="flex items-baseline gap-1">
                <span className="text-4xl font-bold">R$ {plan.price.toFixed(2)}</span>
                <span className="text-sm text-muted-foreground">
                  {plan.billingCycle === 'monthly' ? t('billing.perMonth') : t('billing.perYear')}
                </span>
              </div>
            </div>

            <div className="space-y-2 mb-6">
              <div className="text-sm font-semibold text-muted-foreground mb-2">{t('billing.features')}</div>
              {(plan.features ?? []).map((feature, index) => (
                <div key={index} className="flex items-center gap-2 text-sm">
                  <Check className="w-4 h-4 text-green-500" />
                  <span>{feature}</span>
                </div>
              ))}
            </div>

            <div className="pt-4 border-t border-border space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">{t('billing.maxCompanies')}:</span>
                <span className="font-medium">
                  {plan.maxCompanies === -1 ? t('billing.unlimited') : plan.maxCompanies}
                </span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">{t('billing.maxUsers')}:</span>
                <span className="font-medium">
                  {plan.maxUsers === -1 ? t('billing.unlimited') : plan.maxUsers}
                </span>
              </div>
            </div>

            <div className="flex gap-2 mt-6">
              <Button
                variant="secondary"
                size="sm"
                className="flex-1"
                onClick={() => handleEdit(plan)}
              >
                <Edit className="w-4 h-4 mr-1" />
                {t('common.edit')}
              </Button>
              <Button
                variant="secondary"
                size="sm"
                onClick={() => handleToggleStatus(plan.id)}
              >
                {plan.isActive ? <X className="w-4 h-4" /> : <Check className="w-4 h-4" />}
              </Button>
              <Button
                variant="secondary"
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
      <AlertContainer alerts={alerts} onRemove={removeAlert} />
      
      {/* Modal de Confirmação */}
      <ConfirmDialog
        isOpen={confirmDialog.isOpen}
        title={confirmDialog.title}
        message={confirmDialog.message}
        confirmText={confirmDialog.confirmText}
        confirmVariant={confirmDialog.confirmVariant}
        onConfirm={confirmDialog.onConfirm}
        onCancel={() => setConfirmDialog((prev) => ({ ...prev, isOpen: false }))}
        isLoading={confirmLoading}
      />
    </div>
  );
}
