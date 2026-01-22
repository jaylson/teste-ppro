import { useForm } from 'react-hook-form';
import { X } from 'lucide-react';
import { Button } from '@/components/ui';
import { useState, useEffect } from 'react';

interface Invoice {
  id?: string;
  clientId: string;
  clientName: string;
  subscriptionId?: string;
  invoiceNumber: string;
  amount: number;
  issueDate: string;
  dueDate: string;
  status: 'paid' | 'pending' | 'overdue' | 'cancelled';
  description: string;
  notes?: string;
}

interface Client {
  id: string;
  name: string;
}

interface Subscription {
  id: string;
  clientId: string;
  planName: string;
  planPrice: number;
}

interface InvoiceModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (invoice: Invoice) => void;
  invoice?: Invoice | null;
  clients: Client[];
  subscriptions: Subscription[];
}

export default function InvoiceModal({ 
  isOpen, 
  onClose, 
  onSave, 
  invoice,
  clients,
  subscriptions 
}: InvoiceModalProps) {
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<Invoice>({
    defaultValues: invoice || {
      clientId: '',
      clientName: '',
      subscriptionId: '',
      invoiceNumber: `INV-${Date.now()}`,
      amount: 0,
      issueDate: new Date().toISOString().split('T')[0],
      dueDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
      status: 'pending',
      description: '',
      notes: '',
    }
  });

  const selectedClientId = watch('clientId');
  const selectedSubscriptionId = watch('subscriptionId');
  const [filteredSubscriptions, setFilteredSubscriptions] = useState<Subscription[]>([]);

  useEffect(() => {
    if (selectedClientId) {
      const client = clients.find(c => c.id === selectedClientId);
      if (client) {
        setValue('clientName', client.name);
      }
      
      const clientSubs = subscriptions.filter(s => s.clientId === selectedClientId);
      setFilteredSubscriptions(clientSubs);
    } else {
      setFilteredSubscriptions([]);
    }
  }, [selectedClientId, clients, subscriptions, setValue]);

  useEffect(() => {
    if (selectedSubscriptionId) {
      const subscription = subscriptions.find(s => s.id === selectedSubscriptionId);
      if (subscription) {
        setValue('amount', subscription.planPrice);
        setValue('description', `Assinatura ${subscription.planName} - ${new Date().toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' })}`);
      }
    }
  }, [selectedSubscriptionId, subscriptions, setValue]);

  const onSubmit = (data: Invoice) => {
    onSave({ ...data, id: invoice?.id });
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl p-6 max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold">
            {invoice ? 'Editar Fatura' : 'Nova Fatura'}
          </h2>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground">
            <X className="w-6 h-6" />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {/* Número da Fatura */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Número da Fatura *
            </label>
            <input
              type="text"
              {...register('invoiceNumber', { required: 'Número é obrigatório' })}
              className="input w-full"
              placeholder="INV-001"
            />
            {errors.invoiceNumber && (
              <p className="text-red-500 text-sm mt-1">{errors.invoiceNumber.message}</p>
            )}
          </div>

          {/* Cliente */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Cliente *
            </label>
            <select 
              {...register('clientId', { required: 'Cliente é obrigatório' })}
              className="input w-full"
              disabled={!!invoice}
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

          {/* Assinatura (opcional) */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Assinatura (opcional)
            </label>
            <select 
              {...register('subscriptionId')}
              className="input w-full"
              disabled={!selectedClientId}
            >
              <option value="">Sem assinatura vinculada</option>
              {filteredSubscriptions.map(sub => (
                <option key={sub.id} value={sub.id}>
                  {sub.planName} - R$ {sub.planPrice.toFixed(2)}
                </option>
              ))}
            </select>
            <p className="text-xs text-muted-foreground mt-1">
              {!selectedClientId ? 'Selecione um cliente primeiro' : 'Vincule a uma assinatura para preencher automaticamente'}
            </p>
          </div>

          {/* Valor */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Valor (R$) *
            </label>
            <input
              type="number"
              step="0.01"
              {...register('amount', { 
                required: 'Valor é obrigatório',
                min: { value: 0.01, message: 'Valor deve ser maior que zero' }
              })}
              className="input w-full"
              placeholder="0.00"
            />
            {errors.amount && (
              <p className="text-red-500 text-sm mt-1">{errors.amount.message}</p>
            )}
          </div>

          {/* Datas */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-2">
                Data de Emissão *
              </label>
              <input
                type="date"
                {...register('issueDate', { required: 'Data de emissão é obrigatória' })}
                className="input w-full"
              />
              {errors.issueDate && (
                <p className="text-red-500 text-sm mt-1">{errors.issueDate.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                Data de Vencimento *
              </label>
              <input
                type="date"
                {...register('dueDate', { required: 'Data de vencimento é obrigatória' })}
                className="input w-full"
              />
              {errors.dueDate && (
                <p className="text-red-500 text-sm mt-1">{errors.dueDate.message}</p>
              )}
            </div>
          </div>

          {/* Descrição */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Descrição *
            </label>
            <input
              type="text"
              {...register('description', { required: 'Descrição é obrigatória' })}
              className="input w-full"
              placeholder="Ex: Plano Professional - Janeiro/2025"
            />
            {errors.description && (
              <p className="text-red-500 text-sm mt-1">{errors.description.message}</p>
            )}
          </div>

          {/* Notas */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Observações
            </label>
            <textarea
              {...register('notes')}
              className="input w-full min-h-[80px]"
              placeholder="Observações adicionais..."
            />
          </div>

          {/* Status */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Status *
            </label>
            <select {...register('status')} className="input w-full">
              <option value="pending">Pendente</option>
              <option value="paid">Pago</option>
              <option value="overdue">Vencido</option>
              <option value="cancelled">Cancelado</option>
            </select>
          </div>

          {/* Ações */}
          <div className="flex gap-3 pt-4">
            <Button type="button" variant="outline" onClick={onClose} className="flex-1">
              Cancelar
            </Button>
            <Button type="submit" className="flex-1">
              {invoice ? 'Salvar Alterações' : 'Criar Fatura'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
