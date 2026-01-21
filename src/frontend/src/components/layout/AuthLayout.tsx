import { Outlet, Navigate } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';
import { PieChart } from 'lucide-react';

export default function AuthLayout() {
  const { isAuthenticated } = useAuthStore();

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <div className="min-h-screen bg-primary flex">
      {/* Left Side - Branding */}
      <div className="hidden lg:flex lg:w-1/2 flex-col justify-between p-12">
        <div className="flex items-center gap-3">
          <div className="w-12 h-12 bg-white/10 rounded-xl flex items-center justify-center">
            <PieChart className="w-7 h-7 text-white" />
          </div>
          <div>
            <h1 className="font-bold text-2xl text-white">Partnership</h1>
            <p className="text-sm text-white/60">Manager</p>
          </div>
        </div>

        <div className="space-y-6">
          <h2 className="text-4xl font-bold text-white leading-tight">
            Gestão Societária
            <br />
            <span className="text-gradient">Simplificada</span>
          </h2>
          <p className="text-white/70 text-lg max-w-md">
            Cap Table, Vesting, Contratos e Valuation em uma única plataforma
            profissional e integrada.
          </p>

          {/* Features */}
          <div className="grid grid-cols-2 gap-4 pt-4">
            {[
              'Cap Table Visual',
              'Simulador de Rodadas',
              'Contratos Dinâmicos',
              'Portal do Investidor',
            ].map((feature) => (
              <div
                key={feature}
                className="flex items-center gap-2 text-white/80"
              >
                <div className="w-2 h-2 rounded-full bg-accent" />
                <span className="text-sm">{feature}</span>
              </div>
            ))}
          </div>
        </div>

        <p className="text-white/40 text-sm">
          © 2025 Partnership Manager. Todos os direitos reservados.
        </p>
      </div>

      {/* Right Side - Auth Form */}
      <div className="flex-1 flex items-center justify-center p-8 bg-background rounded-l-3xl">
        <div className="w-full max-w-md">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
