import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X, HelpCircle } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import {
  ShareholderType,
  ShareholderStatus,
  Gender,
  MaritalStatus,
  DocumentType,
  shareholderTypeLabels,
  shareholderStatusLabels,
  genderLabels,
  maritalStatusLabels,
  documentTypeLabels,
  type Shareholder,
  type CreateShareholderRequest,
  type UpdateShareholderRequest,
} from '@/types';
import { useCreateShareholder, useUpdateShareholder } from '@/hooks/useShareholders';
import { useClientCompanies } from '@/hooks/useClient';
import { useAuthStore } from '@/stores/authStore';
import { useClientStore } from '@/stores/clientStore';
import { isValidCnpj, isValidCpf, normalizeDocument } from '@/utils/documentValidators';

const brazilStates = [
  'AC',
  'AL',
  'AP',
  'AM',
  'BA',
  'CE',
  'DF',
  'ES',
  'GO',
  'MA',
  'MT',
  'MS',
  'MG',
  'PA',
  'PB',
  'PR',
  'PE',
  'PI',
  'RJ',
  'RN',
  'RS',
  'RO',
  'RR',
  'SC',
  'SP',
  'SE',
  'TO',
];

// Validation schema
const shareholderSchema = z
  .object({
    companyId: z.string().min(1, 'Selecione uma empresa'),
    name: z.string().min(2, 'Nome deve ter pelo menos 2 caracteres'),
    document: z
      .string()
      .min(1, 'Documento é obrigatório')
      .transform((value) => normalizeDocument(value)),
    documentType: z.nativeEnum(DocumentType),
    type: z.nativeEnum(ShareholderType),
    status: z.nativeEnum(ShareholderStatus).optional(),
    email: z.string().email('E-mail inválido').optional().or(z.literal('')),
    phone: z.string().optional(),
    notes: z.string().optional(),
    addressStreet: z.string().optional(),
    addressNumber: z.string().optional(),
    addressComplement: z.string().optional(),
    addressZipCode: z.string().optional().or(z.literal('')),
    addressCity: z.string().optional(),
    addressState: z.string().optional(),
    maritalStatus: z.nativeEnum(MaritalStatus).optional(),
    gender: z.nativeEnum(Gender).optional(),
    birthDate: z.string().optional().or(z.literal('')),
    earnOut: z.boolean().default(false),
    tagAlong: z.boolean().default(false),
    dragAlong: z.boolean().default(false),
    shareholdersAgreement: z.boolean().default(false),
    rightOfFirstRefusal: z.boolean().default(false),
    liquidationPreferenceRight: z.boolean().default(false),
    antiDilution: z.boolean().default(false),
  })
  .superRefine((data, ctx) => {
    const documentLength = data.document?.length ?? 0;

    if (data.documentType === DocumentType.Cpf) {
      if (documentLength !== 11 || !isValidCpf(data.document)) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Documento inválido',
          path: ['document'],
        });
      }
    }

    if (data.documentType === DocumentType.Cnpj) {
      if (documentLength !== 14 || !isValidCnpj(data.document)) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Documento inválido',
          path: ['document'],
        });
      }
    }

    const cep = normalizeDocument(data.addressZipCode);
    if (cep && cep.length !== 8) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'CEP inválido',
        path: ['addressZipCode'],
      });
    }

    if (data.addressState && data.addressState.trim().length !== 2) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'UF inválida',
        path: ['addressState'],
      });
    }

    if (data.birthDate) {
      const birth = new Date(data.birthDate);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      if (Number.isNaN(birth.getTime()) || birth > today) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Data de nascimento inválida',
          path: ['birthDate'],
        });
      }
    }
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
    setValue,
    setError,
    clearErrors,
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
      addressStreet: '',
      addressNumber: '',
      addressComplement: '',
      addressZipCode: '',
      addressCity: '',
      addressState: '',
      maritalStatus: undefined,
      gender: undefined,
      birthDate: '',
      earnOut: false,
      tagAlong: false,
      dragAlong: false,
      shareholdersAgreement: false,
      rightOfFirstRefusal: false,
      liquidationPreferenceRight: false,
      antiDilution: false,
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
        addressStreet: shareholder.addressStreet || '',
        addressNumber: shareholder.addressNumber || '',
        addressComplement: shareholder.addressComplement || '',
        addressZipCode: shareholder.addressZipCode || '',
        addressCity: shareholder.addressCity || '',
        addressState: shareholder.addressState || '',
        maritalStatus: shareholder.maritalStatus,
        gender: shareholder.gender,
        birthDate: shareholder.birthDate ? shareholder.birthDate.split('T')[0] : '',
        earnOut: shareholder.earnOut ?? false,
        tagAlong: shareholder.tagAlong ?? false,
        dragAlong: shareholder.dragAlong ?? false,
        shareholdersAgreement: shareholder.shareholdersAgreement ?? false,
        rightOfFirstRefusal: shareholder.rightOfFirstRefusal ?? false,
        liquidationPreferenceRight: shareholder.liquidationPreferenceRight ?? false,
        antiDilution: shareholder.antiDilution ?? false,
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
        addressStreet: '',
        addressNumber: '',
        addressComplement: '',
        addressZipCode: '',
        addressCity: '',
        addressState: '',
        maritalStatus: undefined,
        gender: undefined,
        birthDate: '',
        earnOut: false,
        tagAlong: false,
        dragAlong: false,
        shareholdersAgreement: false,
        rightOfFirstRefusal: false,
        liquidationPreferenceRight: false,
        antiDilution: false,
      });
    }
  }, [shareholder, reset, selectedCompanyId, user?.companyId]);

  const [isCepLoading, setIsCepLoading] = useState(false);
  const [lastFetchedCep, setLastFetchedCep] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'governance' | 'qualification'>('governance');

  const onSubmit = async (data: ShareholderFormData) => {
    try {
      const normalizedZipCode = normalizeDocument(data.addressZipCode);
      const normalizedState = data.addressState?.trim().toUpperCase();
      const normalizedDocument = normalizeDocument(data.document);

      if (isEditing && shareholder) {
        const effectiveCompanyId = data.companyId || shareholder.companyId;
        const updateData: UpdateShareholderRequest = {
          companyId: effectiveCompanyId,
          name: data.name,
          document: normalizedDocument,
          documentType: data.documentType,
          type: data.type,
          status: data.status || ShareholderStatus.Active,
          email: data.email || undefined,
          phone: data.phone || undefined,
          notes: data.notes || undefined,
          addressStreet: data.addressStreet || undefined,
          addressNumber: data.addressNumber || undefined,
          addressComplement: data.addressComplement || undefined,
          addressZipCode: normalizedZipCode || undefined,
          addressCity: data.addressCity || undefined,
          addressState: normalizedState || undefined,
          maritalStatus: data.maritalStatus,
          gender: data.gender,
          birthDate: data.birthDate || undefined,
          earnOut: data.earnOut,
          tagAlong: data.tagAlong,
          dragAlong: data.dragAlong,
          shareholdersAgreement: data.shareholdersAgreement,
          rightOfFirstRefusal: data.rightOfFirstRefusal,
          liquidationPreferenceRight: data.liquidationPreferenceRight,
          antiDilution: data.antiDilution,
        };
        await updateMutation.mutateAsync({ id: shareholder.id, data: updateData });
      } else {
        const effectiveCompanyId = data.companyId || selectedCompanyId || user?.companyId || '';
        const createData: CreateShareholderRequest = {
          companyId: effectiveCompanyId,
          name: data.name,
          document: normalizedDocument,
          documentType: data.documentType,
          type: data.type,
          email: data.email || undefined,
          phone: data.phone || undefined,
          notes: data.notes || undefined,
          addressStreet: data.addressStreet || undefined,
          addressNumber: data.addressNumber || undefined,
          addressComplement: data.addressComplement || undefined,
          addressZipCode: normalizedZipCode || undefined,
          addressCity: data.addressCity || undefined,
          addressState: normalizedState || undefined,
          maritalStatus: data.maritalStatus,
          gender: data.gender,
          birthDate: data.birthDate || undefined,
          earnOut: data.earnOut,
          tagAlong: data.tagAlong,
          dragAlong: data.dragAlong,
          shareholdersAgreement: data.shareholdersAgreement,
          rightOfFirstRefusal: data.rightOfFirstRefusal,
          liquidationPreferenceRight: data.liquidationPreferenceRight,
          antiDilution: data.antiDilution,
        };
        await createMutation.mutateAsync(createData);
      }
      onClose();
    } catch {
      // Error handled by mutation
    }
  };

  const documentType = watch('documentType');
  const zipCode = watch('addressZipCode');

  const fetchAddressByCep = async (rawCep?: string) => {
    const cep = normalizeDocument(rawCep ?? zipCode);
    if (!cep || cep.length !== 8) {
      setError('addressZipCode', { type: 'manual', message: 'CEP inválido' });
      return;
    }

    if (cep === lastFetchedCep) return;

    setIsCepLoading(true);
    clearErrors('addressZipCode');

    try {
      const response = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
      const data = await response.json();

      if (!response.ok || data?.erro) {
        setError('addressZipCode', { type: 'manual', message: 'CEP não encontrado' });
        return;
      }

      setValue('addressStreet', data.logradouro || '');
      setValue('addressCity', data.localidade || '');
      setValue('addressState', data.uf || '');
      setLastFetchedCep(cep);
    } catch {
      setError('addressZipCode', { type: 'manual', message: 'Erro ao buscar CEP' });
    } finally {
      setIsCepLoading(false);
    }
  };

  useEffect(() => {
    const cep = normalizeDocument(zipCode);
    if (cep.length === 8) {
      fetchAddressByCep(cep);
    }
  }, [zipCode]);

  if (!isOpen) return null;

  // Helper for governance toggle rows
  const governanceFields: { field: keyof ShareholderFormData; label: string; help: string }[] = [
    { field: 'earnOut', label: 'Earn-out', help: 'Parte do pagamento vinculada a resultados futuros da empresa' },
    { field: 'tagAlong', label: 'Tag Along', help: 'Direito de vender junto se o controlador vender' },
    { field: 'dragAlong', label: 'Drag Along', help: 'Obrigação de vender junto (proteção do majoritário)' },
    { field: 'shareholdersAgreement', label: 'Shareholders Agreement', help: 'Acordo de sócios que define regras de governança' },
    { field: 'rightOfFirstRefusal', label: 'Direitos de preferência (ROFR)', help: 'Prioridade de compra antes de terceiros' },
    { field: 'liquidationPreferenceRight', label: 'Liquidation Preference', help: 'Prioridade de recebimento em caso de venda/liquidação' },
    { field: 'antiDilution', label: 'Anti-diluição', help: 'Proteção contra diluição em rodadas futuras a preço menor' },
  ];

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/50 transition-opacity"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="flex min-h-full items-center justify-center p-4">
        <div className="relative bg-white rounded-xl shadow-xl w-full max-w-4xl transform transition-all">
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

          {/* Tabs */}
          <div className="border-b border-gray-200 px-6">
            <nav className="-mb-px flex gap-1">
              {([
                { id: 'qualification', label: 'Qualificação' },
                { id: 'governance', label: 'Configuração Societária' },
              ] as const).map(tab => (
                <button
                  key={tab.id}
                  type="button"
                  onClick={() => setActiveTab(tab.id)}
                  className={`px-5 py-3 text-sm font-medium border-b-2 transition-colors ${
                    activeTab === tab.id
                      ? 'border-primary-600 text-primary-700'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  {tab.label}
                </button>
              ))}
            </nav>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit(onSubmit)}>
            {/* Tab: Qualificação */}
            {activeTab === 'qualification' && (
              <div className="p-6 space-y-4">
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                  {/* Left */}
                  <div className="space-y-4">
                    <Input
                      label="Nome *"
                      {...register('name')}
                      error={errors.name?.message}
                      placeholder="Nome completo do sócio"
                    />

                    <div className="grid grid-cols-3 gap-4">
                      <div>
                        <label className="input-label">Tipo doc. *</label>
                        <select {...register('documentType', { valueAsNumber: true })} className="input">
                          {Object.entries(documentTypeLabels).map(([value, label]) => (
                            <option key={value} value={value}>{label}</option>
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

                    <Input
                      label="E-mail"
                      type="email"
                      {...register('email')}
                      error={errors.email?.message}
                      placeholder="email@exemplo.com"
                    />

                    <Input
                      label="Telefone"
                      {...register('phone')}
                      placeholder="+55 11 99999-0000"
                    />

                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="input-label">Sexo</label>
                        <select
                          {...register('gender', { setValueAs: (v) => (v === '' ? undefined : Number(v)) })}
                          className="input"
                        >
                          <option value="">Selecione</option>
                          {Object.entries(genderLabels).map(([value, label]) => (
                            <option key={value} value={value}>{label}</option>
                          ))}
                        </select>
                      </div>
                      <div>
                        <label className="input-label">Estado civil</label>
                        <select
                          {...register('maritalStatus', { setValueAs: (v) => (v === '' ? undefined : Number(v)) })}
                          className="input"
                        >
                          <option value="">Selecione</option>
                          {Object.entries(maritalStatusLabels).map(([value, label]) => (
                            <option key={value} value={value}>{label}</option>
                          ))}
                        </select>
                      </div>
                    </div>

                    <Input
                      label="Data de nascimento"
                      type="date"
                      {...register('birthDate')}
                      error={errors.birthDate?.message}
                    />
                  </div>

                  {/* Right — Endereço */}
                  <div className="space-y-4">
                    <div className="grid grid-cols-3 gap-4">
                      <div className="col-span-2">
                        <Input
                          label="CEP"
                          {...register('addressZipCode')}
                          error={errors.addressZipCode?.message}
                          placeholder="00000-000"
                        />
                      </div>
                      <div className="flex items-end">
                        <Button
                          type="button"
                          variant="secondary"
                          size="sm"
                          loading={isCepLoading}
                          onClick={() => fetchAddressByCep()}
                          className="w-full"
                        >
                          Buscar CEP
                        </Button>
                      </div>
                    </div>

                    <Input label="Rua" {...register('addressStreet')} placeholder="Rua / Avenida" />

                    <div className="grid grid-cols-3 gap-4">
                      <Input label="Número" {...register('addressNumber')} placeholder="123" />
                      <div className="col-span-2">
                        <Input label="Complemento" {...register('addressComplement')} placeholder="Apto, bloco, etc." />
                      </div>
                    </div>

                    <div className="grid grid-cols-3 gap-4">
                      <div className="col-span-2">
                        <Input label="Cidade" {...register('addressCity')} placeholder="Cidade" />
                      </div>
                      <div>
                        <label className="input-label">Estado</label>
                        <select {...register('addressState')} className="input">
                          <option value="">UF</option>
                          {brazilStates.map((state) => (
                            <option key={state} value={state}>{state}</option>
                          ))}
                        </select>
                        {errors.addressState && (
                          <p className="input-error-message">{errors.addressState.message}</p>
                        )}
                      </div>
                    </div>
                  </div>
                </div>

                {/* Observações */}
                <div>
                  <label className="input-label">Observações</label>
                  <textarea
                    {...register('notes')}
                    className="input min-h-[80px] resize-y"
                    placeholder="Observações adicionais..."
                  />
                </div>
              </div>
            )}

            {/* Tab: Configuração Societária */}
            {activeTab === 'governance' && (
              <div className="p-6 space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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

                  {/* Tipo de Sócio */}
                  <div>
                    <label className="input-label">Tipo de Sócio *</label>
                    <select {...register('type', { valueAsNumber: true })} className="input">
                      {Object.entries(shareholderTypeLabels).map(([value, label]) => (
                        <option key={value} value={value}>{label}</option>
                      ))}
                    </select>
                  </div>

                  {/* Status (only when editing) */}
                  {isEditing && (
                    <div>
                      <label className="input-label">Status</label>
                      <select {...register('status', { valueAsNumber: true })} className="input">
                        {Object.entries(shareholderStatusLabels).map(([value, label]) => (
                          <option key={value} value={value}>{label}</option>
                        ))}
                      </select>
                    </div>
                  )}
                </div>

                {/* Governance Toggles */}
                <div>
                  <h3 className="text-sm font-semibold text-gray-700 mb-3 uppercase tracking-wide">
                    Cláusulas e Direitos Societários
                  </h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                    {governanceFields.map(({ field, label, help }) => (
                      <label
                        key={field}
                        className="flex items-start gap-3 p-3 rounded-lg border border-transparent hover:bg-gray-50 hover:border-gray-200 cursor-pointer group transition-colors"
                      >
                        {/* Toggle switch */}
                        <div className="relative flex items-center mt-0.5 flex-shrink-0">
                          <input
                            type="checkbox"
                            {...register(field)}
                            className="sr-only peer"
                          />
                          <div className="w-10 h-6 bg-gray-200 rounded-full peer peer-focus:ring-2 peer-focus:ring-primary-400 peer-checked:bg-primary-600 transition-colors after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:after:translate-x-4 after:shadow-sm" />
                        </div>
                        <div className="flex-1 min-w-0">
                          <span className="text-sm font-medium text-gray-900 block">{label}</span>
                          <span className="text-xs text-gray-500 flex items-center gap-1 mt-0.5">
                            <HelpCircle className="w-3 h-3 flex-shrink-0" />
                            {help}
                          </span>
                        </div>
                      </label>
                    ))}
                  </div>
                </div>
              </div>
            )}



            {/* Actions — always visible */}
            <div className="flex justify-end gap-3 p-6 pt-4 border-t">
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
