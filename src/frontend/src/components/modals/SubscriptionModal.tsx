import { useForm } from 'react-hook-form';
import { X } from 'lucide-react';
import { Button } from '@/components/ui';
import { useState, useEffect } from 'react';

interface Subscription {
  id?: string;
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

interface Client {
  id: string;
  name: string;
}

interface Plan {
  id: string;
  name: string;
  price: number;
  billingCycle: 'monthly' | 'yearly';
}

interface SubscriptionModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (subscription: Subscription) => void;
  subscription?: Subscription | null;
  clients: Client[];
  plans: Plan[];
}

export default function SubscriptionModal({ 
  isOpen, 
  onClose, 
  onSave, 
  subscription,
  clients,
  plans 
}: SubscriptionModalProps) {
  const { register, handleSubmit, formState: { errors }, watch, setValue, reset } = useForm<Subscription>({
    defaultValues: subscription || {
      clientId: '',
      clientName: '',
      planId: '',
      planName: '',
      planPrice: 0,
      status: 'pending',
      startDate: new Date().toISOString().split('T')[0],
      billingCycle: 'monthly',
      autoRenew: true,
      companiesCount: 0,
      usersCount: 0,
    }
  });

  const selectedPlanId = watch('planId');
  const selectedClientId = watch('clientId');

  // Resetar o formulário quando a subscription ou isOpen mudar
  useEffect(() => {
    if (isOpen) {
      if (subscription) {
        // Formatar datas para o formato aceito pelo input type="date" (YYYY-MM-DD)
        const formattedSubscription = {
          ...subscription,
          startDate: subscription.startDate ? subscription.startDate.split('T')[0] : new Date().toISOString().split('T')[0],
          endDate: subscription.endDate ? subscription.endDate.split('T')[0] : ''
        };
        reset(formattedSubscription);
      } else {
        reset({
          clientId: '',
          clientName: '',
          planId: '',
          planName: '',
          planPrice: 0,
          status: 'pending',
          startDate: new Date().toISOString().split('T')[0],
          billingCycle: 'monthly',
          autoRenew: true,
          companiesCount: 0,
          usersCount: 0,
        });
      }
    }
  }, [isOpen, subscription, reset]);

  useEffect(() => {
    if (selectedPlanId) {
      const plan = plans.find(p => p.id === selectedPlanId);
      if (plan) {
        setValue('planName', plan.name);
        setValue('planPrice', plan.price);
        setValue('billingCycle', plan.billingCycle);
      }
    }
  }, [selectedPlanId, plans, setValue]);

  useEffect(() => {
    if (selectedClientId) {
      const client = clients.find(c => c.id === selectedClientId);
      if (client) {
        setValue('clientName', client.name);
      }
    }
  }, [selectedClientId, clients, setValue]);

  const onSubmit = (data: Subscription) => {
    onSave({ ...data, id: subscription?.id });
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl p-6 max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold">
            {subscription ? 'Editar Assinatura' : 'Nova Assinatura'}
          </h2>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground">
            <X className="w-6 h-6" />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {/* Cliente */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Cliente *
            </label>
            <select 
              {...register('clientId', { required: 'Cliente é obrigatório' })}
              className="input w-full"
              disabled={!!subscription}
            >
              <option value="">Selecione um cliente</option>
              {clients.map(client => (
                <option key={client.id} value={client.id}>
                  {client.name}
                </option>
              ))}
            </select>
            {errors.clientId && (
              <p className="text-red-500 text-sm mt-1">{errors.clientId.message}</p>
            )}
          </div>

          {/* Plano */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Plano *
            </label>
            <select 
              {...register('planId', { required: 'Plano é obrigatório' })}
              className="input w-full"
            >
              <option value="">Selecione um plano</option>
              {plans.map(plan => (
                <option key={plan.id} value={plan.id}>
                  {plan.name} - R$ {plan.price.toFixed(2)} ({plan.billingCycle === 'monthly' ? 'Mensal' : 'Anual'})
                </option>
              ))}
            </select>
            {errors.planId && (
              <p className="text-red-500 text-sm mt-1">{errors.planId.message}</p>
            )}
          </div>

          {/* Datas */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-2">
                Data de Início *
              </label>
              <input
                type="date"
                {...register('startDate', { required: 'Data de início é obrigatória' })}
                className="input w-full"
              />
              {errors.startDate && (
                <p className="text-red-500 text-sm mt-1">{errors.startDate.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                Data de Término
              </label>
              <input
                type="date"
                {...register('endDate')}
                className="input w-full"
              />
              <p className="text-xs text-muted-foreground mt-1">
                Deixe em branco para assinatura contínua
              </p>
            </div>
          </div>

          {/* Uso Atual */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-2">
                Empresas Ativas
              </label>
              <input
                type="number"
                {...register('companiesCount', { 
                  valueAsNumber: true,
                  min: { value: 0, message: 'Valor mínimo é 0' }
                })}
                className="input w-full"
                placeholder="0"
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                Usuários Ativos
              </label>
              <input
                type="number"
                {...register('usersCount', { 
                  valueAsNumber: true,
                  min: { value: 0, message: 'Valor mínimo é 0' }
                })}
                className="input w-full"
                placeholder="0"
              />
            </div>
          </div>

          {/* Status */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Status *
            </label>
            <select {...register('status')} className="input w-full">
              <option value="pending">Pendente</option>
              <option value="active">Ativo</option>
              <option value="suspended">Suspenso</option>
              <option value="cancelled">Cancelado</option>
            </select>
          </div>

          {/* Renovação Automática */}
          <div className="flex items-center gap-3">
            <input
              type="checkbox"
              id="autoRenew"
              {...register('autoRenew')}
              className="w-4 h-4"
            />
            <label htmlFor="autoRenew" className="text-sm font-medium">
              Renovação Automática
            </label>
          </div>

          {/* Ações */}
          <div className="flex gap-3 pt-4">
            <Button type="button" variant="outline" onClick={onClose} className="flex-1">
              Cancelar
            </Button>
            <Button type="submit" className="flex-1">
              {subscription ? 'Salvar Alterações' : 'Criar Assinatura'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
