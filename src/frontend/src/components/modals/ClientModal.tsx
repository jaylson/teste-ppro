import { useForm } from 'react-hook-form';
import { useEffect } from 'react';
import { X } from 'lucide-react';
import { Button } from '@/components/ui';

interface Client {
  id?: string;
  name: string;
  email: string;
  document: string;
  type: 'individual' | 'company';
  status: 'active' | 'suspended' | 'cancelled';
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
}

interface ClientModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (client: Client) => void;
  client?: Client | null;
}

export default function ClientModal({ isOpen, onClose, onSave, client }: ClientModalProps) {
  const { register, handleSubmit, formState: { errors }, watch, reset } = useForm<Client>({
    defaultValues: {
      name: '',
      email: '',
      document: '',
      type: 'company',
      status: 'active',
      phone: '',
      address: '',
      city: '',
      state: '',
      zipCode: '',
      country: 'Brasil',
    }
  });

  // Atualizar formulário quando client mudar ou modal abrir
  useEffect(() => {
    if (isOpen) {
      if (client) {
        reset(client);
      } else {
        reset({
          name: '',
          email: '',
          document: '',
          type: 'company',
          status: 'active',
          phone: '',
          address: '',
          city: '',
          state: '',
          zipCode: '',
          country: 'Brasil',
        });
      }
    }
  }, [isOpen, client, reset]);

  const clientType = watch('type');

  const onSubmit = (data: Client) => {
    onSave({ ...data, id: client?.id });
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl p-6 max-w-3xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold">
            {client ? 'Editar Cliente' : 'Novo Cliente'}
          </h2>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground">
            <X className="w-6 h-6" />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {/* Tipo de Cliente */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Tipo de Cliente *
            </label>
            <div className="flex gap-4">
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  value="company"
                  {...register('type')}
                  className="w-4 h-4"
                />
                <span>Empresa</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  value="individual"
                  {...register('type')}
                  className="w-4 h-4"
                />
                <span>Pessoa Física</span>
              </label>
            </div>
          </div>

          {/* Nome */}
          <div>
            <label className="block text-sm font-medium mb-2">
              {clientType === 'company' ? 'Razão Social' : 'Nome Completo'} *
            </label>
            <input
              type="text"
              {...register('name', { required: 'Nome é obrigatório' })}
              className="input w-full"
              placeholder={clientType === 'company' ? 'Ex: TechStartup Ltda' : 'Ex: João Silva'}
            />
            {errors.name && (
              <p className="text-red-500 text-sm mt-1">{errors.name.message}</p>
            )}
          </div>

          {/* Email e Documento */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-2">
                E-mail *
              </label>
              <input
                type="email"
                {...register('email', { 
                  required: 'E-mail é obrigatório',
                  pattern: {
                    value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                    message: 'E-mail inválido'
                  }
                })}
                className="input w-full"
                placeholder="email@exemplo.com"
              />
              {errors.email && (
                <p className="text-red-500 text-sm mt-1">{errors.email.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                {clientType === 'company' ? 'CNPJ' : 'CPF'} *
              </label>
              <input
                type="text"
                {...register('document', { required: 'Documento é obrigatório' })}
                className="input w-full"
                placeholder={clientType === 'company' ? '00.000.000/0000-00' : '000.000.000-00'}
              />
              {errors.document && (
                <p className="text-red-500 text-sm mt-1">{errors.document.message}</p>
              )}
            </div>
          </div>

          {/* Telefone */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Telefone
            </label>
            <input
              type="tel"
              {...register('phone')}
              className="input w-full"
              placeholder="+55 (00) 00000-0000"
            />
          </div>

          {/* Endereço */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Endereço
            </label>
            <input
              type="text"
              {...register('address')}
              className="input w-full"
              placeholder="Rua, Número, Complemento"
            />
          </div>

          {/* Cidade, Estado, CEP */}
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium mb-2">
                Cidade
              </label>
              <input
                type="text"
                {...register('city')}
                className="input w-full"
                placeholder="São Paulo"
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                Estado
              </label>
              <input
                type="text"
                {...register('state')}
                className="input w-full"
                placeholder="SP"
                maxLength={2}
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                CEP
              </label>
              <input
                type="text"
                {...register('zipCode')}
                className="input w-full"
                placeholder="00000-000"
              />
            </div>
          </div>

          {/* País */}
          <div>
            <label className="block text-sm font-medium mb-2">
              País
            </label>
            <input
              type="text"
              {...register('country')}
              className="input w-full"
              placeholder="Brasil"
            />
          </div>

          {/* Status */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Status *
            </label>
            <select {...register('status')} className="input w-full">
              <option value="active">Ativo</option>
              <option value="suspended">Suspenso</option>
              <option value="cancelled">Cancelado</option>
            </select>
          </div>

          {/* Ações */}
          <div className="flex gap-3 pt-4">
            <Button type="button" variant="outline" onClick={onClose} className="flex-1">
              Cancelar
            </Button>
            <Button type="submit" className="flex-1">
              {client ? 'Salvar Alterações' : 'Criar Cliente'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
