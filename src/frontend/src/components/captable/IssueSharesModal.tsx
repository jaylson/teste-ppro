import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X, Plus, DollarSign, FileText } from 'lucide-react';
import { Button, Input, Card } from '@/components/ui';
import { useIssueShares } from '@/hooks/useCapTable';
import { useShareholders } from '@/hooks/useShareholders';
import { useShareClassesByCompany } from '@/hooks/useShareClasses';
import type { IssueSharesRequest } from '@/types';

const issueSharesSchema = z.object({
  shareholderId: z.string().min(1, 'Selecione um sócio'),
  shareClassId: z.string().min(1, 'Selecione uma classe de ações'),
  quantity: z.coerce.number().min(1, 'Quantidade deve ser maior que 0'),
  pricePerShare: z.coerce.number().min(0, 'Preço deve ser maior ou igual a 0'),
  referenceDate: z.string().min(1, 'Data de referência é obrigatória'),
  certificateNumber: z.string().optional(),
  transactionNumber: z.string().optional(),
  reason: z.string().optional(),
  documentReference: z.string().optional(),
  notes: z.string().optional(),
});

type IssueSharesFormData = z.infer<typeof issueSharesSchema>;

interface IssueSharesModalProps {
  isOpen: boolean;
  onClose: () => void;
  companyId: string;
  onSuccess?: () => void;
}

export default function IssueSharesModal({
  isOpen,
  onClose,
  companyId,
  onSuccess,
}: IssueSharesModalProps) {
  const [showAdvanced, setShowAdvanced] = useState(false);

  const { data: shareholdersData, isLoading: loadingShareholders } = useShareholders({ 
    companyId, 
    pageSize: 100 
  });
  const { data: shareClassesData, isLoading: loadingClasses } = useShareClassesByCompany(companyId);

  const issueShares = useIssueShares();

  const {
    register,
    handleSubmit,
    watch,
    reset,
    formState: { errors },
  } = useForm<IssueSharesFormData>({
    resolver: zodResolver(issueSharesSchema),
    defaultValues: {
      shareholderId: '',
      shareClassId: '',
      quantity: 0,
      pricePerShare: 0,
      referenceDate: new Date().toISOString().split('T')[0],
      certificateNumber: '',
      transactionNumber: '',
      reason: '',
      documentReference: '',
      notes: '',
    },
  });

  const watchQuantity = watch('quantity');
  const watchPrice = watch('pricePerShare');
  const totalValue = (watchQuantity || 0) * (watchPrice || 0);

  const onSubmit = async (data: IssueSharesFormData) => {
    const request: IssueSharesRequest = {
      companyId,
      shareholderId: data.shareholderId,
      shareClassId: data.shareClassId,
      quantity: data.quantity,
      pricePerShare: data.pricePerShare,
      referenceDate: data.referenceDate,
      certificateNumber: data.certificateNumber || undefined,
      transactionNumber: data.transactionNumber || undefined,
      reason: data.reason || undefined,
      documentReference: data.documentReference || undefined,
      notes: data.notes || undefined,
    };

    issueShares.mutate(request, {
      onSuccess: () => {
        reset();
        onClose();
        onSuccess?.();
      },
    });
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center p-4">
        {/* Backdrop */}
        <div className="fixed inset-0 bg-black/50 transition-opacity" onClick={handleClose} />

        {/* Modal */}
        <div className="relative w-full max-w-lg bg-white rounded-xl shadow-xl">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                <Plus className="w-5 h-5 text-green-600" />
              </div>
              <div>
                <h2 className="text-lg font-semibold text-gray-900">Emitir Ações</h2>
                <p className="text-sm text-gray-500">Registrar nova emissão de ações</p>
              </div>
            </div>
            <button onClick={handleClose} className="text-gray-400 hover:text-gray-600">
              <X className="w-5 h-5" />
            </button>
          </div>

          {/* Body */}
          <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
            {/* Sócio */}
            <div>
              <label className="input-label">Sócio *</label>
              <select
                {...register('shareholderId')}
                className="input"
                disabled={loadingShareholders}
              >
                <option value="">Selecione um sócio</option>
                {shareholdersData?.items?.map((shareholder) => (
                  <option key={shareholder.id} value={shareholder.id}>
                    {shareholder.name}
                  </option>
                ))}
              </select>
              {errors.shareholderId && (
                <p className="input-error-message">{errors.shareholderId.message}</p>
              )}
            </div>

            {/* Classe de Ações */}
            <div>
              <label className="input-label">Classe de Ações *</label>
              <select
                {...register('shareClassId')}
                className="input"
                disabled={loadingClasses}
              >
                <option value="">Selecione uma classe</option>
                {shareClassesData?.map((shareClass) => (
                  <option key={shareClass.id} value={shareClass.id}>
                    {shareClass.code} - {shareClass.name}
                  </option>
                ))}
              </select>
              {errors.shareClassId && (
                <p className="input-error-message">{errors.shareClassId.message}</p>
              )}
            </div>

            {/* Quantidade e Preço */}
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Quantidade *"
                type="number"
                step="1"
                min="1"
                {...register('quantity')}
                error={errors.quantity?.message}
              />
              <Input
                label="Preço por Ação (R$) *"
                type="number"
                step="0.01"
                min="0"
                {...register('pricePerShare')}
                error={errors.pricePerShare?.message}
              />
            </div>

            {/* Valor Total (calculado) */}
            <Card className="p-4 bg-gray-50">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2 text-gray-600">
                  <DollarSign className="w-4 h-4" />
                  <span className="text-sm">Valor Total</span>
                </div>
                <span className="text-lg font-semibold text-gray-900">
                  {totalValue.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                </span>
              </div>
            </Card>

            {/* Data de Referência */}
            <Input
              label="Data de Referência *"
              type="date"
              {...register('referenceDate')}
              error={errors.referenceDate?.message}
            />

            {/* Toggle Campos Avançados */}
            <button
              type="button"
              onClick={() => setShowAdvanced(!showAdvanced)}
              className="flex items-center gap-2 text-sm text-primary hover:text-primary/80"
            >
              <FileText className="w-4 h-4" />
              {showAdvanced ? 'Ocultar campos adicionais' : 'Mostrar campos adicionais'}
            </button>

            {/* Campos Avançados */}
            {showAdvanced && (
              <div className="space-y-4 pt-2 border-t border-gray-200">
                <div className="grid grid-cols-2 gap-4">
                  <Input
                    label="Nº do Certificado"
                    {...register('certificateNumber')}
                    error={errors.certificateNumber?.message}
                  />
                  <Input
                    label="Nº da Transação"
                    {...register('transactionNumber')}
                    error={errors.transactionNumber?.message}
                  />
                </div>
                <Input
                  label="Motivo"
                  {...register('reason')}
                  error={errors.reason?.message}
                />
                <Input
                  label="Referência de Documento"
                  {...register('documentReference')}
                  error={errors.documentReference?.message}
                />
                <div>
                  <label className="input-label">Observações</label>
                  <textarea
                    {...register('notes')}
                    className="input min-h-[80px]"
                    placeholder="Observações adicionais..."
                  />
                </div>
              </div>
            )}

            {/* Actions */}
            <div className="flex justify-end gap-3 pt-4 border-t">
              <Button type="button" variant="secondary" onClick={handleClose}>
                Cancelar
              </Button>
              <Button
                type="submit"
                variant="success"
                loading={issueShares.isPending}
                icon={<Plus className="w-4 h-4" />}
              >
                Emitir Ações
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
