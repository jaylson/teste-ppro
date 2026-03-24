import { useState, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Mail, ArrowLeft, CheckCircle } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button, Input } from '@/components/ui';
import { authService } from '@/services/authService';
import toast from 'react-hot-toast';

export default function ForgotPassword() {
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);
  const { t, i18n } = useTranslation();

  const schema = useMemo(
    () =>
      z.object({
        email: z
          .string()
          .min(1, t('auth.emailRequired', 'E-mail é obrigatório'))
          .email(t('auth.emailInvalid', 'E-mail inválido')),
      }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [i18n.language]
  );

  type FormValues = z.infer<typeof schema>;

  const {
    register,
    handleSubmit,
    getValues,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const onSubmit = async (data: FormValues) => {
    setLoading(true);
    try {
      await authService.forgotPassword({ email: data.email });
      setSent(true);
    } catch {
      // Mesmo em caso de erro de rede mostramos toast mas não revelamos se o e-mail existe
      toast.error(
        t('auth.forgotPasswordError', 'Não foi possível enviar o e-mail. Tente novamente.')
      );
    } finally {
      setLoading(false);
    }
  };

  if (sent) {
    return (
      <div className="animate-fade-in text-center">
        <div className="flex justify-center mb-6">
          <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center">
            <CheckCircle className="w-8 h-8 text-green-600" />
          </div>
        </div>
        <h2 className="text-2xl font-bold text-primary mb-3">
          {t('auth.forgotPasswordSentTitle', 'Verifique seu e-mail')}
        </h2>
        <p className="text-primary-500 mb-2">
          {t(
            'auth.forgotPasswordSentLine1',
            'Se o endereço'
          )}{' '}
          <span className="font-medium text-primary">{getValues('email')}</span>{' '}
          {t(
            'auth.forgotPasswordSentLine2',
            'estiver cadastrado, você receberá um link de recuperação em instantes.'
          )}
        </p>
        <p className="text-sm text-primary-400 mb-8">
          {t('auth.forgotPasswordSentHint', 'Verifique também a pasta de spam.')}
        </p>
        <Link
          to="/login"
          className="inline-flex items-center gap-2 text-sm text-accent hover:text-accent-700 font-medium"
        >
          <ArrowLeft className="w-4 h-4" />
          {t('auth.backToLogin', 'Voltar ao login')}
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
          <div className="w-10 h-10 bg-accent/10 rounded-lg flex items-center justify-center">
            <Mail className="w-5 h-5 text-accent" />
          </div>
        </div>
        <h2 className="text-2xl font-bold text-primary">
          {t('auth.forgotPasswordTitle', 'Recuperar senha')}
        </h2>
        <p className="text-primary-500 mt-1">
          {t(
            'auth.forgotPasswordDescription',
            'Digite seu e-mail e enviaremos um link para redefinir sua senha.'
          )}
        </p>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
        <Input
          label={t('auth.email', 'E-mail')}
          type="email"
          placeholder={t('auth.emailPlaceholder', 'seu@email.com')}
          error={errors.email?.message}
          {...register('email')}
        />

        <Button
          type="submit"
          className="w-full"
          loading={loading}
          icon={<Mail className="w-5 h-5" />}
        >
          {t('auth.forgotPasswordButton', 'Enviar link de recuperação')}
        </Button>
      </form>

      <div className="mt-6 text-center">
        <Link
          to="/login"
          className="inline-flex items-center gap-2 text-sm text-accent hover:text-accent-700 font-medium"
        >
          <ArrowLeft className="w-4 h-4" />
          {t('auth.backToLogin', 'Voltar ao login')}
        </Link>
      </div>
    </div>
  );
}
