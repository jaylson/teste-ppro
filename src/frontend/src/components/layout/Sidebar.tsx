import { NavLink, useNavigate } from 'react-router-dom';
import {
  LayoutDashboard,
  PieChart,
  Users,
  CheckSquare,
  FileText,
  Target,
  TrendingUp,
  DollarSign,
  Briefcase,
  Settings,
  LogOut,
  Globe,
  CreditCard,
  Building2,
  List,
  Layers,
  LucideIcon,
} from 'lucide-react';
import { useAuthStore } from '@/stores/authStore';
import { cn } from '@/utils/cn';

interface NavigationItem {
  name: string;
  href: string;
  icon: LucideIcon;
  requiresRole?: string[];
}

const navigation: NavigationItem[] = [
  { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
  { name: 'Empresas', href: '/companies', icon: Building2 },
  { name: 'Sócios', href: '/shareholders', icon: Users },
  { name: 'Cap Table', href: '/cap-table', icon: PieChart },
  { name: 'Aprovações', href: '/approvals', icon: CheckSquare },
  { name: 'Contratos', href: '/contracts', icon: FileText },
  { name: 'Vesting — Planos', href: '/vesting', icon: Target },
  { name: 'Vesting — Grants', href: '/vesting/grants', icon: Users },
  { name: 'Meu Vesting', href: '/my-vesting', icon: TrendingUp },
  { name: 'Valuation', href: '/valuation', icon: TrendingUp },
  { name: 'Financeiro', href: '/financial', icon: DollarSign },
  { name: 'Portal Investidor', href: '/investor', icon: Briefcase },
];

const settingsNavigation: NavigationItem[] = [
  { name: 'Usuários', href: '/settings/users', icon: Users },
  { name: 'Billing', href: '/billing', icon: CreditCard },
  { name: 'Templates de Contratos', href: '/contracts/templates', icon: FileText, requiresRole: ['Admin', 'Legal'] },
  { name: 'Cláusulas', href: '/contracts/clauses', icon: List, requiresRole: ['Admin', 'Legal'] },
  { name: 'Templates de Milestone', href: '/vesting/milestone-templates', icon: Layers, requiresRole: ['Admin'] },
];

const languages = [
  { code: 'pt', name: 'Português', flag: '🇧🇷' },
  { code: 'en', name: 'English', flag: '🇺🇸' },
  { code: 'es', name: 'Español', flag: '🇪🇸' },
];

export default function Sidebar() {
  const navigate = useNavigate();
  const { user, logout } = useAuthStore();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <aside className="sidebar">
      {/* Logo */}
      <div className="sidebar-logo">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-white/10 rounded-xl flex items-center justify-center">
            <PieChart className="w-6 h-6 text-white" />
          </div>
          <div>
            <h1 className="font-bold text-lg text-white">Partnership</h1>
            <p className="text-xs text-white/60">Manager</p>
          </div>
        </div>
      </div>

      {/* Navigation */}
      <nav className="sidebar-nav">
        <ul className="space-y-1">
          {navigation
            .filter((item) => {
              if (!item.requiresRole) return true;
              return item.requiresRole.some((role) => user?.roles?.includes(role));
            })
            .map((item) => (
            <li key={item.name}>
              <NavLink
                to={item.href}
                className={({ isActive }) =>
                  cn('sidebar-item', isActive && 'sidebar-item-active')
                }
              >
                <item.icon className="w-5 h-5" />
                <span>{item.name}</span>
              </NavLink>
            </li>
          ))}
        </ul>

        {/* Settings Section */}
        <div className="mt-6 pt-6 border-t border-white/10">
          <div className="px-4 py-2 text-xs font-semibold text-white/60 uppercase tracking-wider">
            Administração
          </div>
          <ul className="space-y-1">
            {settingsNavigation
              .filter((item) => {
                // Show item if it doesn't require a role, or if user has the required role
                if (!item.requiresRole) return true;
                return item.requiresRole.some((role) => user?.roles?.includes(role));
              })
              .map((item) => (
                <li key={item.name}>
                  <NavLink
                    to={item.href}
                    className={({ isActive }) =>
                      cn('sidebar-item', isActive && 'sidebar-item-active')
                    }
                  >
                    <item.icon className="w-5 h-5" />
                    <span>{item.name}</span>
                  </NavLink>
                </li>
              ))}
          </ul>
        </div>
      </nav>

      {/* Footer */}
      <div className="sidebar-footer space-y-3">
        {/* Language Selector */}
        <div className="flex items-center gap-2 px-4 py-2 text-white/60">
          <Globe className="w-4 h-4" />
          <select
            className="bg-transparent text-sm cursor-pointer focus:outline-none"
            defaultValue={user?.language || 'pt'}
          >
            {languages.map((lang) => (
              <option key={lang.code} value={lang.code} className="text-primary">
                {lang.flag} {lang.name}
              </option>
            ))}
          </select>
        </div>

        {/* Actions */}
        <div className="flex gap-2">
          <button className="sidebar-item flex-1 justify-center">
            <Settings className="w-4 h-4" />
          </button>
          <button
            onClick={handleLogout}
            className="sidebar-item flex-1 justify-center hover:bg-error/20 hover:text-error-300"
          >
            <LogOut className="w-4 h-4" />
          </button>
        </div>
      </div>
    </aside>
  );
}
