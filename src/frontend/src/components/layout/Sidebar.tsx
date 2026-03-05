import { useState, useEffect } from 'react';
import { NavLink, useNavigate, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  PieChart,
  Users,
  UserCheck,
  CheckSquare,
  FileText,
  Target,
  TrendingUp,
  DollarSign,
  Briefcase,
  Settings,
  LogOut,
  Globe,
  Building2,
  List,
  Layers,
  FlaskConical,
  ChevronDown,
  LucideIcon,
} from 'lucide-react';
import { useAuthStore } from '@/stores/authStore';
import { cn } from '@/utils/cn';

// ─── Types ────────────────────────────────────────────────────────────────────

interface NavItem {
  name: string;
  href: string;
  icon: LucideIcon;
  requiresRole?: string[];
}

interface NavGroup {
  key: string;
  label: string;
  icon: LucideIcon;
  items: NavItem[];
}

// ─── Navigation structure ─────────────────────────────────────────────────────

const navGroups: NavGroup[] = [
  {
    key: 'dashboards',
    label: 'Dashboards',
    icon: LayoutDashboard,
    items: [
      { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
      { name: 'Valuation Dashboard', href: '/valuations/dashboard', icon: TrendingUp },
      { name: 'Financeiro Dashboard', href: '/financial/dashboard', icon: DollarSign },
      { name: 'Meu Vesting', href: '/my-vesting', icon: Target },
      { name: 'Portal do Investidor', href: '/investor', icon: Briefcase },
    ],
  },
  {
    key: 'captable',
    label: 'Cap Table',
    icon: PieChart,
    items: [
      { name: 'Empresas', href: '/companies', icon: Building2 },
      { name: 'Sócios', href: '/shareholders', icon: Users },
      { name: 'Cap Table', href: '/cap-table', icon: PieChart },
      { name: 'Vesting — Planos', href: '/vesting', icon: Target },
      { name: 'Vesting — Grants', href: '/vesting/grants', icon: UserCheck },
    ],
  },
  {
    key: 'valuation',
    label: 'Valuation',
    icon: TrendingUp,
    items: [
      { name: 'Valuation', href: '/valuations', icon: TrendingUp },
      { name: 'Financeiro', href: '/financial', icon: DollarSign },
      { name: 'Documentos', href: '/documents', icon: FileText },
    ],
  },
  {
    key: 'contratos',
    label: 'Contratos',
    icon: FileText,
    items: [
      { name: 'Contratos', href: '/contracts', icon: FileText },
      { name: 'Templates de Contratos', href: '/contracts/templates', icon: FileText, requiresRole: ['Admin', 'Legal'] },
    ],
  },
  {
    key: 'aprovacoes',
    label: 'Aprovações',
    icon: CheckSquare,
    items: [
      { name: 'Fluxos', href: '/approvals/flows', icon: List },
      { name: 'Aprovadores', href: '/approvals/approvers', icon: UserCheck },
      { name: 'Aprovações', href: '/approvals', icon: CheckSquare },
    ],
  },
  {
    key: 'admin',
    label: 'Administração',
    icon: Settings,
    items: [
      { name: 'Usuários', href: '/settings/users', icon: Users },
      { name: 'Perfis de Acesso', href: '/settings/roles', icon: UserCheck },
    ],
  },
  {
    key: 'acessorios',
    label: 'Acessórios',
    icon: Layers,
    items: [
      { name: 'Fórmulas Customizadas', href: '/valuations/custom-formulas', icon: FlaskConical },
      { name: 'Cláusulas', href: '/contracts/clauses', icon: List, requiresRole: ['Admin', 'Legal'] },
      { name: 'Templates de Milestone', href: '/vesting/milestone-templates', icon: Layers, requiresRole: ['Admin'] },
    ],
  },
];

// ─── Helpers ──────────────────────────────────────────────────────────────────

const STORAGE_KEY = 'sidebar_open_groups';

/** Returns the group key whose item best (most specifically) matches the given path. */
function getGroupForPath(pathname: string): string | null {
  let bestKey: string | null = null;
  let bestLen = -1;

  for (const group of navGroups) {
    for (const item of group.items) {
      if (pathname === item.href || pathname.startsWith(item.href + '/')) {
        if (item.href.length > bestLen) {
          bestLen = item.href.length;
          bestKey = group.key;
        }
      }
    }
  }

  return bestKey;
}

function loadSavedGroups(): string[] | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) return JSON.parse(raw) as string[];
  } catch {
    // ignore parse errors
  }
  return null;
}

// ─── Languages ────────────────────────────────────────────────────────────────

const languages = [
  { code: 'pt', name: 'Português', flag: '🇧🇷' },
  { code: 'en', name: 'English', flag: '🇺🇸' },
  { code: 'es', name: 'Español', flag: '🇪🇸' },
];

// ─── Component ────────────────────────────────────────────────────────────────

export default function Sidebar() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuthStore();

  const [openGroups, setOpenGroups] = useState<string[]>(() => {
    const saved = loadSavedGroups();
    if (saved !== null) return saved;
    const active = getGroupForPath(location.pathname);
    return active ? [active] : [];
  });

  // When navigating to a new route, ensure that group is expanded
  useEffect(() => {
    const active = getGroupForPath(location.pathname);
    if (active && !openGroups.includes(active)) {
      setOpenGroups((prev) => [...prev, active]);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.pathname]);

  // Persist open groups to localStorage whenever they change
  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(openGroups));
  }, [openGroups]);

  const toggleGroup = (key: string) => {
    setOpenGroups((prev) =>
      prev.includes(key) ? prev.filter((k) => k !== key) : [...prev, key]
    );
  };

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
        <ul className="space-y-0.5">
          {navGroups.map((group) => {
            const isOpen = openGroups.includes(group.key);
            return (
              <li key={group.key}>
                {/* Group header (collapsible trigger) */}
                <button
                  onClick={() => toggleGroup(group.key)}
                  className={cn(
                    'w-full flex items-center gap-3 px-4 py-2 rounded-lg',
                    'text-white/50 hover:bg-white/10 hover:text-white/80',
                    'transition-all duration-200'
                  )}
                >
                  <group.icon className="w-3.5 h-3.5 shrink-0" />
                  <span className="flex-1 text-left text-[10px] font-bold uppercase tracking-widest">
                    {group.label}
                  </span>
                  <ChevronDown
                    className={cn(
                      'w-3 h-3 shrink-0 transition-transform duration-300',
                      isOpen ? 'rotate-0' : '-rotate-90'
                    )}
                  />
                </button>

                {/* Animated items container — CSS grid row trick */}
                <div
                  className={cn(
                    'grid transition-all duration-300 ease-in-out',
                    isOpen ? 'grid-rows-[1fr]' : 'grid-rows-[0fr]'
                  )}
                >
                  <div className="overflow-hidden">
                    <ul className="pt-0.5 pb-2 space-y-0.5">
                      {group.items
                        .filter((item) => {
                          if (!item.requiresRole) return true;
                          return item.requiresRole.some((role) =>
                            user?.roles?.includes(role)
                          );
                        })
                        .map((item) => (
                          <li key={item.href}>
                            <NavLink
                              to={item.href}
                              end
                              className={({ isActive }) =>
                                cn(
                                  'flex items-center gap-3 pl-9 pr-4 py-2 rounded-lg text-sm',
                                  'text-white/60 hover:bg-white/10 hover:text-white transition-all duration-200',
                                  isActive && 'bg-white/10 text-white font-medium'
                                )
                              }
                            >
                              <item.icon className="w-4 h-4 shrink-0" />
                              <span>{item.name}</span>
                            </NavLink>
                          </li>
                        ))}
                    </ul>
                  </div>
                </div>
              </li>
            );
          })}
        </ul>
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
