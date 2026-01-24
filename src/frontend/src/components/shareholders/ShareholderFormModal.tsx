import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import {
  ShareholderType,
  ShareholderStatus,
  DocumentType,
  shareholderTypeLabels,
  shareholderStatusLabels,
  documentTypeLabels,
  type Shareholder,
  type CreateShareholderRequest,
  type UpdateShareholderRequest,
} from '@/types';
import { useCreateShareholder, useUpdateShareholder } from '@/hooks/useShareholders';
import { useClientCompanies } from '@/hooks/useClient';
import { useAuthStore } from '@/stores/authStore';
import { useClientStore } from '@/stores/clientStore';

// Validation schema
const shareholderSchema = z.object({
  companyId: z.string().min(1, 'Selecione uma empresa'),
  name: z.string().min(2, 'Nome deve ter pelo menos 2 caracteres'),
  document: z
    .string()
    .min(11, 'Documento inválido')
    .max(14, 'Documento inválido')
    .regex(/^\d+$/, 'Documento deve conter apenas números'),
  documentType: z.nativeEnum(DocumentType),
  type: z.nativeEnum(ShareholderType),
  status: z.nativeEnum(ShareholderStatus).optional(),
  email: z.string().email('E-mail inválido').optional().or(z.literal('')),
  phone: z.string().optional(),
  notes: z.string().optional(),
});

type ShareholderFormData = z.infer<typeof shareholderSchema>;

interface ShareholderFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  shareholder?: Shareholder;
}

export function ShareholderFormModal({
  isOpen,
  onClose,
  shareholder,
}: ShareholderFormModalProps) {
  const isEditing = !!shareholder;
  const { user } = useAuthStore();
  const { selectedCompanyId } = useClientStore();
  const { data: companiesData } = useClientCompanies(user?.clientId || '');

  const createMutation = useCreateShareholder();
  const updateMutation = useUpdateShareholder();

  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<ShareholderFormData>({
    resolver: zodResolver(shareholderSchema),
    defaultValues: {
      companyId: selectedCompanyId || user?.companyId || '',
      name: '',
      document: '',
      documentType: DocumentType.Cpf,
      type: ShareholderType.Founder,
      status: ShareholderStatus.Active,
      email: '',
      phone: '',
      notes: '',
    },
  });

  // Populate form when editing
  useEffect(() => {
    if (shareholder) {
      reset({
        companyId: shareholder.companyId,
        name: shareholder.name,
        document: shareholder.document,
        documentType: shareholder.documentType,
        type: shareholder.type,
        status: shareholder.status,
        email: shareholder.email || '',
        phone: shareholder.phone || '',
        notes: shareholder.notes || '',
      });
    } else {
      reset({
        companyId: '',
        name: '',
        document: '',
        documentType: DocumentType.Cpf,
        type: ShareholderType.Founder,
        status: ShareholderStatus.Active,
        email: '',
        phone: '',
        notes: '',
      });
    }
  }, [shareholder, reset, selectedCompanyId, user?.companyId]);

  const onSubmit = async (data: ShareholderFormData) => {
    try {
      if (isEditing && shareholder) {
        const effectiveCompanyId = data.companyId || shareholder.companyId;
        const updateData: UpdateShareholderRequest = {
          companyId: effectiveCompanyId,
          name: data.name,
          document: data.document,
          documentType: data.documentType,
          type: data.type,
          status: data.status || ShareholderStatus.Active,
          email: data.email || undefined,
          phone: data.phone || undefined,
          notes: data.notes || undefined,
        };
        await updateMutation.mutateAsync({ id: shareholder.id, data: updateData });
      } else {
        const effectiveCompanyId = data.companyId || selectedCompanyId || user?.companyId || '';
        const createData: CreateShareholderRequest = {
          companyId: effectiveCompanyId,
          name: data.name,
          document: data.document,
          documentType: data.documentType,
          type: data.type,
          email: data.email || undefined,
          phone: data.phone || undefined,
          notes: data.notes || undefined,
        };
        await createMutation.mutateAsync(createData);
      }
      onClose();
    } catch {
      // Error handled by mutation
    }
  };

  const documentType = watch('documentType');

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/50 transition-opacity"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="flex min-h-full items-center justify-center p-4">
        <div className="relative bg-white rounded-xl shadow-xl w-full max-w-lg transform transition-all">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b">
            <h2 className="text-lg font-semibold text-primary">
              {isEditing ? 'Editar Sócio' : 'Novo Sócio'}
            </h2>
            <button
              onClick={onClose}
              className="p-2 hover:bg-primary-100 rounded-lg transition-colors"
            >
              <X className="w-5 h-5 text-primary-500" />
            </button>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
            {/* Company */}
            <div>
              <label className="input-label">Empresa *</label>
              <select
                {...register('companyId')}
                className="input"
                disabled={isEditing}
              >
                <option value="">Selecione uma empresa</option>
                {companiesData?.map((company) => (
                  <option key={company.id} value={company.id}>
                    {company.name}
                  </option>
                ))}
              </select>
              {errors.companyId && (
                <p className="input-error-message">{errors.companyId.message}</p>
              )}
            </div>

            {/* Name */}
            <Input
              label="Nome *"
              {...register('name')}
              error={errors.name?.message}
              placeholder="Nome completo do sócio"
            />

            {/* Document Type + Document */}
            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="input-label">Tipo *</label>
                <select {...register('documentType')} className="input">
                  {Object.entries(documentTypeLabels).map(([value, label]) => (
                    <option key={value} value={value}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="col-span-2">
                <Input
                  label="Documento *"
                  {...register('document')}
                  error={errors.document?.message}
                  placeholder={documentType === DocumentType.Cpf ? '00000000000' : '00000000000000'}
                  maxLength={documentType === DocumentType.Cpf ? 11 : 14}
                />
              </div>
            </div>

            {/* Type */}
            <div>
              <label className="input-label">Tipo de Sócio *</label>
              <select {...register('type')} className="input">
                {Object.entries(shareholderTypeLabels).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
                  </option>
                ))}
              </select>
            </div>

            {/* Status (only when editing) */}
            {isEditing && (
              <div>
                <label className="input-label">Status</label>
                <select {...register('status')} className="input">
                  {Object.entries(shareholderStatusLabels).map(([value, label]) => (
                    <option key={value} value={value}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
            )}

            {/* Email */}
            <Input
              label="E-mail"
              type="email"
              {...register('email')}
              error={errors.email?.message}
              placeholder="email@exemplo.com"
            />

            {/* Phone */}
            <Input
              label="Telefone"
              {...register('phone')}
              placeholder="+55 11 99999-0000"
            />

            {/* Notes */}
            <div>
              <label className="input-label">Observações</label>
              <textarea
                {...register('notes')}
                className="input min-h-[80px] resize-y"
                placeholder="Observações adicionais..."
              />
            </div>

            {/* Actions */}
            <div className="flex justify-end gap-3 pt-4 border-t">
              <Button type="button" variant="secondary" onClick={onClose}>
                Cancelar
              </Button>
              <Button
                type="submit"
                loading={isSubmitting || createMutation.isPending || updateMutation.isPending}
              >
                {isEditing ? 'Salvar' : 'Criar Sócio'}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
