import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { X } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useCreateUser, useUpdateUser } from '@/hooks/useUsers';
import type { UserSummary, Role } from '@/types';

const RoleLabels: Record<Role, string> = {
  SuperAdmin: 'Super Admin',
  Admin: 'Administrador',
  Founder: 'Fundador',
  BoardMember: 'Membro do Conselho',
  Legal: 'Legal',
  Finance: 'Finanças',
  HR: 'Recursos Humanos',
  Employee: 'Funcionário',
  Investor: 'Investidor',
  Viewer: 'Visualizador',
};

const roleValues = ['SuperAdmin', 'Admin', 'Founder', 'BoardMember', 'Legal', 'Finance', 'HR', 'Employee', 'Investor', 'Viewer'] as const;

const createUserSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(200),
  email: z
    .string()
    .email('Email inválido')
    .min(1, 'Email é obrigatório'),
  password: z
    .string()
    .min(8, 'Mínimo 8 caracteres')
    .regex(/[A-Z]/, 'Deve conter letra maiúscula')
    .regex(/[a-z]/, 'Deve conter letra minúscula')
    .regex(/[0-9]/, 'Deve conter número')
    .regex(/[^a-zA-Z0-9]/, 'Deve conter caractere especial'),
  phone: z.string().optional(),
  initialRole: z.enum(roleValues),
});

const updateUserSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(200),
  phone: z.string().optional(),
  avatarUrl: z
    .string()
    .url('URL inválida')
    .optional()
    .or(z.literal('')),
});

type CreateUserForm = z.infer<typeof createUserSchema>;
type UpdateUserForm = z.infer<typeof updateUserSchema>;

interface UserFormProps {
  user?: UserSummary | null;
  onClose: () => void;
}

export function UserForm({ user, onClose }: UserFormProps) {
  const isEditing = !!user;
  const createUser = useCreateUser();
  const updateUser = useUpdateUser();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateUserForm | UpdateUserForm>({
    resolver: zodResolver(isEditing ? updateUserSchema : createUserSchema),
    defaultValues: isEditing ? { name: user.name } : { initialRole: 'Viewer' },
  });

  const onSubmit = async (data: CreateUserForm | UpdateUserForm) => {
    try {
      if (isEditing) {
        await updateUser.mutateAsync({
          id: user.id,
          data: data as UpdateUserForm,
        });
      } else {
        await createUser.mutateAsync(data as CreateUserForm);
      }
      onClose();
    } catch (error) {
      // Error handled by mutation
    }
  };

  const roleOptions = Object.entries(RoleLabels).map(([value, label]) => ({
    value,
    label,
  }));

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-md mx-4">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b">
          <h2 className="text-lg font-semibold text-primary">
            {isEditing ? 'Editar Usuário' : 'Novo Usuário'}
          </h2>
          <button
            onClick={onClose}
            className="p-1 hover:bg-primary-100 rounded"
          >
            <X className="w-5 h-5 text-primary-500" />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="p-4 space-y-4">
          <Input
            label="Nome"
            placeholder="Nome completo"
            error={errors.name?.message}
            {...register('name')}
          />

          {!isEditing && (
            <>
              <Input
                label="Email"
                type="email"
                placeholder="email@exemplo.com"
                error={(errors as any).email?.message}
                {...register('email')}
              />

              <Input
                label="Senha"
                type="password"
                placeholder="••••••••"
                error={(errors as any).password?.message}
                {...register('password')}
              />

              <div>
                <label className="input-label">Papel Inicial</label>
                <select
                  className="input"
                  {...register('initialRole')}
                  defaultValue={roleOptions[0]?.value}
                >
                  {roleOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
                {(errors as any).initialRole?.message && (
                  <p className="input-error-message">{(errors as any).initialRole.message}</p>
                )}
              </div>
            </>
          )}

          <Input
            label="Telefone"
            placeholder="(00) 00000-0000"
            error={errors.phone?.message}
            {...register('phone')}
          />

          {isEditing && (
            <Input
              label="URL do Avatar"
              placeholder="https://..."
              error={(errors as any).avatarUrl?.message}
              {...register('avatarUrl')}
            />
          )}

          {/* Actions */}
          <div className="flex gap-3 pt-4">
            <Button
              type="button"
              variant="secondary"
              className="flex-1"
              onClick={onClose}
            >
              Cancelar
            </Button>
            <Button
              type="submit"
              className="flex-1"
              loading={isSubmitting}
            >
              {isEditing ? 'Salvar' : 'Criar'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
