import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { X } from 'lucide-react';
import { Button } from '@/components/ui';

interface Plan {
  id?: string;
  name: string;
  description: string;
  price: number;
  billingCycle: 'monthly' | 'yearly';
  features?: string[];
  maxCompanies: number;
  maxUsers: number;
  isActive: boolean;
}

interface PlanModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (plan: Plan) => void;
  plan?: Plan | null;
}

export default function PlanModal({ isOpen, onClose, onSave, plan }: PlanModalProps) {
  const { register, handleSubmit, formState: { errors }, reset } = useForm<Plan>({
    defaultValues: plan || {
      name: '',
      description: '',
      price: 0,
      billingCycle: 'monthly',
      features: [],
      maxCompanies: 1,
      maxUsers: 5,
      isActive: true,
    }
  });

  const [featureInput, setFeatureInput] = React.useState('');
  const [features, setFeatures] = React.useState<string[]>(plan?.features || []);

  // Resetar o formulário quando o plano ou isOpen mudar
  useEffect(() => {
    if (isOpen) {
      if (plan) {
        reset(plan);
        setFeatures(plan.features || []);
      } else {
        reset({
          name: '',
          description: '',
          price: 0,
          billingCycle: 'monthly',
          features: [],
          maxCompanies: 1,
          maxUsers: 5,
          isActive: true,
        });
        setFeatures([]);
      }
    }
  }, [isOpen, plan, reset]);

  const addFeature = () => {
    if (featureInput.trim()) {
      setFeatures([...features, featureInput.trim()]);
      setFeatureInput('');
    }
  };

  const removeFeature = (index: number) => {
    setFeatures(features.filter((_, i) => i !== index));
  };

  const onSubmit = (data: Plan) => {
    onSave({ ...data, features, id: plan?.id });
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl p-6 max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold">
            {plan ? 'Editar Plano' : 'Novo Plano'}
          </h2>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground">
            <X className="w-6 h-6" />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {/* Nome */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Nome do Plano *
            </label>
            <input
              type="text"
              {...register('name', { required: 'Nome é obrigatório' })}
              className="input w-full"
              placeholder="Ex: Starter, Professional, Enterprise"
            />
            {errors.name && (
              <p className="text-red-500 text-sm mt-1">{errors.name.message}</p>
            )}
          </div>

          {/* Descrição */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Descrição *
            </label>
            <textarea
              {...register('description', { required: 'Descrição é obrigatória' })}
              className="input w-full min-h-[80px]"
              placeholder="Descreva o plano..."
            />
            {errors.description && (
              <p className="text-red-500 text-sm mt-1">{errors.description.message}</p>
            )}
          </div>

          {/* Preço e Ciclo */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-2">
                Preço (R$) *
              </label>
              <input
                type="number"
                step="0.01"
                {...register('price', { 
                  required: 'Preço é obrigatório',
                  min: { value: 0, message: 'Preço deve ser positivo' }
                })}
                className="input w-full"
                placeholder="0.00"
              />
              {errors.price && (
                <p className="text-red-500 text-sm mt-1">{errors.price.message}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                Ciclo de Cobrança *
              </label>
              <select {...register('billingCycle')} className="input w-full">
                <option value="monthly">Mensal</option>
                <option value="yearly">Anual</option>
              </select>
            </div>
          </div>

          {/* Limites */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-2">
                Máx. Empresas *
              </label>
              <input
                type="number"
                {...register('maxCompanies', { 
                  required: 'Campo obrigatório',
                  min: { value: -1, message: 'Use -1 para ilimitado' }
                })}
                className="input w-full"
                placeholder="-1 para ilimitado"
              />
              <p className="text-xs text-muted-foreground mt-1">
                -1 = ilimitado
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                Máx. Usuários *
              </label>
              <input
                type="number"
                {...register('maxUsers', { 
                  required: 'Campo obrigatório',
                  min: { value: -1, message: 'Use -1 para ilimitado' }
                })}
                className="input w-full"
                placeholder="-1 para ilimitado"
              />
              <p className="text-xs text-muted-foreground mt-1">
                -1 = ilimitado
              </p>
            </div>
          </div>

          {/* Features */}
          <div>
            <label className="block text-sm font-medium mb-2">
              Recursos/Features
            </label>
            <div className="flex gap-2 mb-3">
              <input
                type="text"
                value={featureInput}
                onChange={(e) => setFeatureInput(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addFeature())}
                className="input flex-1"
                placeholder="Digite um recurso e pressione Enter"
              />
              <Button type="button" onClick={addFeature}>
                Adicionar
              </Button>
            </div>
            <div className="space-y-2">
              {features.map((feature, index) => (
                <div key={index} className="flex items-center justify-between p-2 bg-muted rounded-lg">
                  <span className="text-sm">{feature}</span>
                  <button
                    type="button"
                    onClick={() => removeFeature(index)}
                    className="text-red-500 hover:text-red-700"
                  >
                    <X className="w-4 h-4" />
                  </button>
                </div>
              ))}
            </div>
          </div>

          {/* Status */}
          <div className="flex items-center gap-3">
            <input
              type="checkbox"
              id="isActive"
              {...register('isActive')}
              className="w-4 h-4"
            />
            <label htmlFor="isActive" className="text-sm font-medium">
              Plano Ativo
            </label>
          </div>

          {/* Ações */}
          <div className="flex gap-3 pt-4">
            <Button type="button" variant="secondary" onClick={onClose} className="flex-1">
              Cancelar
            </Button>
            <Button type="submit" className="flex-1">
              {plan ? 'Salvar Alterações' : 'Criar Plano'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
