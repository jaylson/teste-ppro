import { useState, useMemo } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Eye, EyeOff, ShieldCheck, ArrowLeft } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { authService } from '@/services/authService';
import toast from 'react-hot-toast';

export default function ActivateAccount() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';

  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const schema = useMemo(
    () =>
      z
        .object({
          newPassword: z
            .string()
            .min(8, 'A senha deve ter no mínimo 8 caracteres')
            .max(128, 'A senha deve ter no máximo 128 caracteres')
            .regex(/[A-Z]/, 'Deve conter pelo menos uma letra maiúscula')
            .regex(/[a-z]/, 'Deve conter pelo menos uma letra minúscula')
            .regex(/[0-9]/, 'Deve conter pelo menos um número')
            .regex(/[^a-zA-Z0-9]/, 'Deve conter pelo menos um caractere especial'),
          confirmNewPassword: z.string().min(1, 'Confirmação de senha é obrigatória'),
        })
        .refine((data) => data.newPassword === data.confirmNewPassword, {
          path: ['confirmNewPassword'],
          message: 'As senhas não coincidem',
        }),
    []
  );

  type FormValues = z.infer<typeof schema>;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const onSubmit = async (data: FormValues) => {
    if (!token) {
      toast.error('Link inválido. Solicite ao administrador um novo convite.');
      return;
    }

    setLoading(true);
    try {
      await authService.activateAccount({
        token,
        newPassword: data.newPassword,
        confirmNewPassword: data.confirmNewPassword,
      });

      toast.success('Conta ativada com sucesso! Faça login para continuar.');
      navigate('/login', { replace: true });
    } catch (error: unknown) {
      const message =
        (error as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'O link de ativação é inválido ou já expirou. Solicite ao administrador um novo convite.';
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  // Token ausente na URL
  if (!token) {
    return (
      <div className="animate-fade-in text-center">
        <h2 className="text-2xl font-bold text-primary mb-3">Link inválido</h2>
        <p className="text-primary-500 mb-6">
          O link de ativação é inválido ou já expirou. Entre em contato com o administrador.
        </p>
        <Link
          to="/login"
          className="inline-flex items-center gap-2 text-sm text-accent hover:text-accent-700 font-medium"
        >
          <ArrowLeft className="w-4 h-4" />
          Voltar ao login
        </Link>
      </div>
    );
  }

  return (
    <div className="animate-fade-in">
      {/* Mobile Logo */}
      <div className="lg:hidden flex items-center justify-center gap-3 mb-8">
        <div className="w-12 h-12 bg-primary rounded-xl flex items-center justify-center">
          <span className="text-white font-bold text-xl">PM</span>
        </div>
        <div>
          <h1 className="font-bold text-xl text-primary">Partnership</h1>
          <p className="text-sm text-primary-500">Manager</p>
        </div>
      </div>

      {/* Header */}
      <div className="mb-8">
        <div className="flex items-center gap-3 mb-4">
          <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
            <ShieldCheck className="w-5 h-5 text-green-600" />
          </div>
        </div>
        <h2 className="text-2xl font-bold text-primary">Bem-vindo! Ative sua conta</h2>
        <p className="text-primary-500 mt-1">
          Defina uma senha segura para acessar o sistema.
        </p>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
        {/* New Password */}
        <div className="relative">
          <Input
            label="Nova senha"
            type={showPassword ? 'text' : 'password'}
            placeholder="••••••••"
            error={errors.newPassword?.message}
            {...register('newPassword')}
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="absolute right-3 top-9 text-primary-400 hover:text-primary-600"
          >
            {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
          </button>
        </div>

        {/* Confirm Password */}
        <div className="relative">
          <Input
            label="Confirmar senha"
            type={showConfirm ? 'text' : 'password'}
            placeholder="••••••••"
            error={errors.confirmNewPassword?.message}
            {...register('confirmNewPassword')}
          />
          <button
            type="button"
            onClick={() => setShowConfirm(!showConfirm)}
            className="absolute right-3 top-9 text-primary-400 hover:text-primary-600"
          >
            {showConfirm ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
          </button>
        </div>

        {/* Password hints */}
        <p className="text-xs text-primary-400">
          Mínimo 8 caracteres com maiúscula, minúscula, número e caractere especial.
        </p>

        <Button
          type="submit"
          className="w-full"
          loading={loading}
          icon={<ShieldCheck className="w-5 h-5" />}
        >
          Ativar minha conta
        </Button>
      </form>

      <div className="mt-6 text-center">
        <Link
          to="/login"
          className="inline-flex items-center gap-2 text-sm text-accent hover:text-accent-700 font-medium"
        >
          <ArrowLeft className="w-4 h-4" />
          Voltar ao login
        </Link>
      </div>
    </div>
  );
}
