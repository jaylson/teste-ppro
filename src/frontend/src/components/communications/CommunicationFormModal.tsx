import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X, Pin, Mail } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useCreateCommunication, useUpdateCommunication, usePublishCommunication } from '@/hooks/useCommunications';
import type { Communication, CreateCommunicationRequest } from '@/types/phase6';

const schema = z.object({
  title: z.string().min(1, 'Título é obrigatório').max(200),
  content: z.string().min(1, 'Conteúdo é obrigatório'),
  summary: z.string().optional(),
  commType: z.string().min(1, 'Tipo é obrigatório'),
  visibility: z.string().min(1, 'Visibilidade é obrigatória'),
  isPinned: z.boolean(),
  sendEmail: z.boolean(),
  expiresAt: z.string().optional(),
});

type FormData = z.infer<typeof schema>;

const COMM_TYPE_OPTIONS = [
  { value: 'announcement', label: 'Anúncio' },
  { value: 'update', label: 'Atualização' },
  { value: 'report', label: 'Relatório' },
  { value: 'alert', label: 'Alerta' },
  { value: 'invitation', label: 'Convite' },
];

const VISIBILITY_OPTIONS = [
  { value: 'all', label: 'Todos' },
  { value: 'investors', label: 'Investidores' },
  { value: 'founders', label: 'Fundadores' },
  { value: 'employees', label: 'Colaboradores' },
  { value: 'specific', label: 'Específico' },
];

interface CommunicationFormModalProps {
  communication?: Communication;
  onClose: () => void;
}

export default function CommunicationFormModal({ communication, onClose }: CommunicationFormModalProps) {
  const isEditing = !!communication;
  const create = useCreateCommunication();
  const update = useUpdateCommunication();
  const publish = usePublishCommunication();

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      title: communication?.title ?? '',
      content: communication?.content ?? '',
      summary: communication?.summary ?? '',
      commType: communication?.commType ?? 'announcement',
      visibility: communication?.visibility ?? 'all',
      isPinned: communication?.isPinned ?? false,
      sendEmail: communication?.sendEmail ?? false,
      expiresAt: communication?.expiresAt ? communication.expiresAt.split('T')[0] : '',
    },
  });

  const isPinned = watch('isPinned');
  const sendEmail = watch('sendEmail');

  async function handleSaveDraft(data: FormData) {
    const payload: CreateCommunicationRequest = {
      ...data,
      expiresAt: data.expiresAt || undefined,
    };
    if (isEditing) {
      await update.mutateAsync({ id: communication.id, data: payload });
    } else {
      await create.mutateAsync(payload);
    }
    onClose();
  }

  async function handlePublishNow(data: FormData) {
    const payload: CreateCommunicationRequest = {
      ...data,
      expiresAt: data.expiresAt || undefined,
    };
    let id = communication?.id;
    if (isEditing) {
      await update.mutateAsync({ id: communication!.id, data: payload });
      id = communication!.id;
    } else {
      const created = await create.mutateAsync(payload);
      id = created.id;
    }
    if (id) {
      await publish.mutateAsync(id);
    }
    onClose();
  }

  const isBusy = isSubmitting || create.isPending || update.isPending || publish.isPending;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-primary-100">
          <h2 className="text-lg font-semibold text-primary">
            {isEditing ? 'Editar Comunicação' : 'Nova Comunicação'}
          </h2>
          <button onClick={onClose} className="text-primary-400 hover:text-primary transition-colors">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Form */}
        <form className="px-6 py-5 space-y-4">
          <Input
            label="Título"
            error={errors.title?.message}
            {...register('title')}
            placeholder="Título da comunicação"
          />

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="input-label">Tipo</label>
              <select className="input" {...register('commType')}>
                {COMM_TYPE_OPTIONS.map((opt) => (
                  <option key={opt.value} value={opt.value}>{opt.label}</option>
                ))}
              </select>
              {errors.commType && <p className="input-error-message">{errors.commType.message}</p>}
            </div>
            <div>
              <label className="input-label">Visibilidade</label>
              <select className="input" {...register('visibility')}>
                {VISIBILITY_OPTIONS.map((opt) => (
                  <option key={opt.value} value={opt.value}>{opt.label}</option>
                ))}
              </select>
              {errors.visibility && <p className="input-error-message">{errors.visibility.message}</p>}
            </div>
          </div>

          <div>
            <label className="input-label">Resumo (opcional)</label>
            <input
              className="input"
              placeholder="Breve descrição da comunicação"
              {...register('summary')}
            />
          </div>

          <div>
            <label className="input-label">Conteúdo</label>
            <textarea
              className={`input min-h-[140px] resize-y ${errors.content ? 'input-error' : ''}`}
              placeholder="Escreva o conteúdo completo da comunicação..."
              {...register('content')}
            />
            {errors.content && <p className="input-error-message">{errors.content.message}</p>}
          </div>

          <div>
            <label className="input-label">Data de expiração (opcional)</label>
            <input type="date" className="input" {...register('expiresAt')} />
          </div>

          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={() => setValue('isPinned', !isPinned)}
              className={`flex items-center gap-2 px-3 py-2 rounded-lg border text-sm font-medium transition-colors ${
                isPinned
                  ? 'bg-amber-50 border-amber-300 text-amber-700'
                  : 'border-primary-200 text-primary-500 hover:bg-primary-50'
              }`}
            >
              <Pin className="w-4 h-4" />
              {isPinned ? 'Fixado' : 'Fixar comunicação'}
            </button>

            <button
              type="button"
              onClick={() => setValue('sendEmail', !sendEmail)}
              className={`flex items-center gap-2 px-3 py-2 rounded-lg border text-sm font-medium transition-colors ${
                sendEmail
                  ? 'bg-cyan-50 border-cyan-300 text-cyan-700'
                  : 'border-primary-200 text-primary-500 hover:bg-primary-50'
              }`}
            >
              <Mail className="w-4 h-4" />
              {sendEmail ? 'Envio por e-mail ativo' : 'Enviar por e-mail'}
            </button>
          </div>
          {sendEmail && (
            <p className="text-xs text-cyan-600 bg-cyan-50 border border-cyan-200 rounded-lg px-3 py-2">
              Ao publicar, um e-mail será enviado para todos os usuários que se
              enquadram na visibilidade selecionada, respeitando as preferências
              de notificação de cada usuário.
            </p>
          )}
        </form>

        {/* Footer */}
        <div className="flex justify-end gap-3 px-6 py-4 border-t border-primary-100">
          <Button variant="secondary" onClick={onClose} disabled={isBusy}>
            Cancelar
          </Button>
          <Button
            variant="secondary"
            onClick={handleSubmit(handleSaveDraft)}
            loading={isBusy}
          >
            Salvar rascunho
          </Button>
          <Button
            variant="primary"
            onClick={handleSubmit(handlePublishNow)}
            loading={isBusy}
          >
            Publicar agora
          </Button>
        </div>
      </div>
    </div>
  );
}
