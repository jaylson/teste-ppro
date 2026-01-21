import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Eye, EyeOff, LogIn } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { useAuthStore } from '@/stores/authStore';
import toast from 'react-hot-toast';

const loginSchema = z.object({
  email: z.string().email('Email inválido').min(1, 'Email é obrigatório'),
  password: z.string().min(1, 'Senha é obrigatória'),
});

type LoginForm = z.infer<typeof loginSchema>;

export default function Login() {
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginForm) => {
    setLoading(true);

    try {
      // Simular login (substituir por chamada real à API)
      await new Promise((resolve) => setTimeout(resolve, 1000));

      // Mock de resposta
      const mockUser = {
        id: 'f1e2d3c4-b5a6-7890-abcd-ef1234567890',
        email: data.email,
        name: 'Administrador',
        companyId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
        companyName: 'Demo Corp',
        roles: ['Admin'],
        language: 'pt',
      };

      setAuth(mockUser, 'mock-access-token', 'mock-refresh-token');
      toast.success('Login realizado com sucesso!');
      navigate('/dashboard');
    } catch (error) {
      toast.error('Credenciais inválidas');
    } finally {
      setLoading(false);
    }
  };

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

      {/* Form Header */}
      <div className="mb-8">
        <h2 className="text-2xl font-bold text-primary">Bem-vindo de volta</h2>
        <p className="text-primary-500 mt-1">
          Entre com suas credenciais para acessar o sistema
        </p>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
        <Input
          label="Email"
          type="email"
          placeholder="seu@email.com"
          error={errors.email?.message}
          {...register('email')}
        />

        <div className="relative">
          <Input
            label="Senha"
            type={showPassword ? 'text' : 'password'}
            placeholder="••••••••"
            error={errors.password?.message}
            {...register('password')}
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="absolute right-3 top-9 text-primary-400 hover:text-primary-600"
          >
            {showPassword ? (
              <EyeOff className="w-5 h-5" />
            ) : (
              <Eye className="w-5 h-5" />
            )}
          </button>
        </div>

        <div className="flex items-center justify-between">
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              className="w-4 h-4 rounded border-primary-300 text-accent focus:ring-accent"
            />
            <span className="text-sm text-primary-600">Lembrar de mim</span>
          </label>
          <a
            href="#"
            className="text-sm text-accent hover:text-accent-700 font-medium"
          >
            Esqueceu a senha?
          </a>
        </div>

        <Button
          type="submit"
          className="w-full"
          loading={loading}
          icon={<LogIn className="w-5 h-5" />}
        >
          Entrar
        </Button>
      </form>

      {/* Demo Info */}
      <div className="mt-8 p-4 bg-primary-50 rounded-lg">
        <p className="text-sm text-primary-600">
          <strong>Demo:</strong> Use qualquer email e senha para acessar.
        </p>
      </div>
    </div>
  );
}
