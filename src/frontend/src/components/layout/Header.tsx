import { UserRound } from 'lucide-react';
import { useAuthStore } from '@/stores/authStore';
import { CompanySwitcher } from './CompanySwitcher';

export default function Header() {
  const { user } = useAuthStore();

  return (
    <header className="flex flex-wrap items-center justify-between gap-4 pb-4 border-b border-primary-100">
      <div className="flex items-center gap-3 text-primary">
        <div className="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center">
          <UserRound className="w-5 h-5" />
        </div>
        <div>
          <p className="text-xs text-primary-600">Bem-vindo(a)</p>
          <h2 className="text-lg font-semibold leading-none">
            {user?.name || 'Usuário'}
          </h2>
          <p className="text-xs text-primary-600 truncate">
            {user?.email || 'email@dominio.com'}
          </p>
        </div>
      </div>

      <CompanySwitcher />
    </header>
  );
}
