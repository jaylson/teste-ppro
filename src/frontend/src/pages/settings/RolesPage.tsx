import { useState } from 'react';
import { Shield, Users, Search, Plus, X, Mail } from 'lucide-react';
import { Button, Card, Spinner } from '@/components/ui';
import { useUsers, useAddUserRole, useRemoveUserRole } from '@/hooks/useUsers';
import type { UserSummary } from '@/types';
import { cn } from '@/utils/cn';

const ALL_ROLES = [
  'SuperAdmin', 'Admin', 'Founder', 'BoardMember', 'Legal',
  'Finance', 'HR', 'Employee', 'Investor', 'Viewer',
] as const;

const ROLE_DESCRIPTIONS: Record<string, string> = {
  SuperAdmin: 'Acesso total ao sistema, incluindo todos os tenants e configurações globais.',
  Admin: 'Administrador da empresa — gerencia usuários, dados e configurações.',
  Founder: 'Fundador — visualiza e aprova operações societárias e de vesting.',
  BoardMember: 'Membro do Conselho — acesso às aprovações estratégicas e valuations.',
  Legal: 'Jurídico — gerenecia contratos, documentos e aprovações legais.',
  Finance: 'Financeiro — acesso a faturamento, invoices e dados financeiros.',
  HR: 'Recursos Humanos — gerencia usuários e vesting de colaboradores.',
  Employee: 'Colaborador — acesso limitado às suas próprias informações.',
  Investor: 'Investidor — acesso ao portal do investidor e relatórios.',
  Viewer: 'Visualizador — somente leitura em áreas autorizadas.',
};

const ROLE_COLORS: Record<string, string> = {
  SuperAdmin: 'bg-purple-100 text-purple-700',
  Admin: 'bg-purple-100 text-purple-700',
  Founder: 'bg-blue-100 text-blue-700',
  BoardMember: 'bg-indigo-100 text-indigo-700',
  Legal: 'bg-amber-100 text-amber-700',
  Finance: 'bg-green-100 text-green-700',
  HR: 'bg-pink-100 text-pink-700',
  Employee: 'bg-gray-100 text-gray-600',
  Investor: 'bg-teal-100 text-teal-700',
  Viewer: 'bg-gray-100 text-gray-500',
};

// ─── Manage Role Modal ────────────────────────────────────────────────────────

interface ManageRoleModalProps {
  user: UserSummary;
  onClose: () => void;
}

function ManageRoleModal({ user, onClose }: ManageRoleModalProps) {
  const addRole = useAddUserRole();
  const removeRole = useRemoveUserRole();
  const [selectedRole, setSelectedRole] = useState('');

  const isPending = addRole.isPending || removeRole.isPending;
  const availableRoles = ALL_ROLES.filter((r) => !user.roles.includes(r) && r !== 'SuperAdmin');

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
        <div className="flex items-center justify-between px-6 py-4 border-b border-primary-100">
          <div>
            <h3 className="font-semibold text-primary">Perfis de Acesso</h3>
            <p className="text-xs text-primary-500">{user.name} · {user.email}</p>
          </div>
          <button onClick={onClose} className="text-primary-400 hover:text-primary text-xl leading-none">&times;</button>
        </div>

        <div className="px-6 py-4 space-y-4">
          <div>
            <p className="text-sm font-medium text-primary mb-2">Perfis atuais</p>
            {user.roles.length === 0 ? (
              <p className="text-xs text-primary-400 italic">Nenhum perfil atribuído</p>
            ) : (
              <div className="space-y-2">
                {user.roles.map((role) => (
                  <div key={role} className="flex items-center justify-between gap-2 p-2 rounded-lg bg-primary-50">
                    <div className="flex items-center gap-2">
                      <span className={cn('px-2 py-0.5 rounded-full text-xs font-semibold', ROLE_COLORS[role] ?? 'bg-gray-100 text-gray-600')}>
                        {role}
                      </span>
                      <span className="text-xs text-primary-500 line-clamp-1">{ROLE_DESCRIPTIONS[role]}</span>
                    </div>
                    {role !== 'SuperAdmin' && (
                      <button
                        onClick={() => removeRole.mutate({ userId: user.id, role })}
                        disabled={isPending}
                        className="text-red-400 hover:text-red-600 p-1 shrink-0"
                        title="Remover perfil"
                      >
                        <X className="w-3.5 h-3.5" />
                      </button>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>

          {availableRoles.length > 0 && (
            <div>
              <p className="text-sm font-medium text-primary mb-2">Adicionar perfil</p>
              <div className="flex gap-2">
                <select
                  className="input flex-1 text-sm"
                  value={selectedRole}
                  onChange={(e) => setSelectedRole(e.target.value)}
                >
                  <option value="">Selecione um perfil...</option>
                  {availableRoles.map((r) => (
                    <option key={r} value={r}>{r} — {ROLE_DESCRIPTIONS[r]?.split('—')[0]?.trim()}</option>
                  ))}
                </select>
                <Button
                  size="sm"
                  onClick={() => {
                    if (!selectedRole) return;
                    addRole.mutate({ userId: user.id, data: { role: selectedRole as any } }, {
                      onSuccess: () => setSelectedRole(''),
                    });
                  }}
                  disabled={!selectedRole || isPending}
                  loading={addRole.isPending}
                >
                  Adicionar
                </Button>
              </div>
            </div>
          )}
        </div>

        <div className="flex justify-end px-6 py-4 border-t border-primary-100">
          <Button variant="secondary" onClick={onClose}>Fechar</Button>
        </div>
      </div>
    </div>
  );
}

// ─── Role Group Card ──────────────────────────────────────────────────────────

function RoleGroupCard({
  role,
  users,
  selected,
  onClick,
}: {
  role: string;
  users: UserSummary[];
  selected: boolean;
  onClick: () => void;
}) {
  return (
    <Card
      className={cn(
        'cursor-pointer transition-all border-2',
        selected ? 'border-accent shadow-md' : 'border-transparent hover:border-primary-200'
      )}
      onClick={onClick}
    >
      <div className="p-4">
        <div className="flex items-center gap-2 mb-2">
          <div className={cn('p-1.5 rounded-lg', ROLE_COLORS[role] ?? 'bg-gray-100')}>
            <Shield className="w-4 h-4" />
          </div>
          <span className="font-semibold text-primary text-sm">{role}</span>
          <span className="ml-auto text-lg font-bold text-accent">{users.length}</span>
        </div>
        <p className="text-xs text-primary-400 line-clamp-2">{ROLE_DESCRIPTIONS[role]}</p>
      </div>
    </Card>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function RolesPage() {
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [page, setPage] = useState(1);
  const [managingUser, setManagingUser] = useState<UserSummary | null>(null);

  const { data, isLoading } = useUsers({ page, pageSize: 50 });
  const allUsers: UserSummary[] = data?.items ?? [];

  const roleGroups: Record<string, UserSummary[]> = {};
  for (const role of ALL_ROLES) {
    roleGroups[role] = allUsers.filter((u) => u.roles.includes(role));
  }

  const displayedUsers = allUsers.filter((u) => {
    const matchSearch =
      !search ||
      u.name.toLowerCase().includes(search.toLowerCase()) ||
      u.email.toLowerCase().includes(search.toLowerCase());
    const matchRole = !roleFilter || u.roles.includes(roleFilter);
    return matchSearch && matchRole;
  });

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div>
        <h1 className="page-title">Perfis de Acesso</h1>
        <p className="page-subtitle">Gerencie os perfis e permissões de cada usuário no sistema</p>
      </div>

      {/* Role group summary */}
      <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-5 gap-3">
        {ALL_ROLES.filter((r) => r !== 'SuperAdmin').map((role) => (
          <RoleGroupCard
            key={role}
            role={role}
            users={roleGroups[role] ?? []}
            selected={roleFilter === role}
            onClick={() => setRoleFilter((r) => (r === role ? '' : role))}
          />
        ))}
      </div>

      {/* Search */}
      <Card>
        <div className="p-4 flex flex-wrap gap-3 items-center">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-primary-400" />
            <input
              className="input pl-9 text-sm"
              placeholder="Buscar usuário..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          {roleFilter && (
            <button
              onClick={() => setRoleFilter('')}
              className="flex items-center gap-1 px-3 py-2 rounded-lg bg-accent/10 text-accent text-sm font-medium"
            >
              <X className="w-3.5 h-3.5" />
              Filtro: {roleFilter}
            </button>
          )}
        </div>
      </Card>

      {/* Users table */}
      {isLoading ? (
        <div className="flex justify-center py-12"><Spinner /></div>
      ) : (
        <Card>
          {displayedUsers.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 text-primary-400 gap-2">
              <Users className="w-10 h-10 opacity-40" />
              <p className="text-sm font-medium">Nenhum usuário encontrado</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-primary-50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-primary-600 uppercase tracking-wide">Usuário</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-primary-600 uppercase tracking-wide">Perfis</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-primary-600 uppercase tracking-wide">Status</th>
                    <th className="px-4 py-3 text-right text-xs font-semibold text-primary-600 uppercase tracking-wide">Ações</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-primary-100">
                  {displayedUsers.map((user) => (
                    <tr key={user.id} className="hover:bg-primary-50 transition-colors">
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-3">
                          <div className="w-8 h-8 rounded-full bg-primary-100 flex items-center justify-center shrink-0">
                            <span className="text-sm font-semibold text-primary-700">
                              {user.name.charAt(0).toUpperCase()}
                            </span>
                          </div>
                          <div className="min-w-0">
                            <p className="font-medium text-primary text-sm truncate">{user.name}</p>
                            <p className="text-xs text-primary-500 flex items-center gap-1 truncate">
                              <Mail className="w-3 h-3 shrink-0" />
                              {user.email}
                            </p>
                          </div>
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex flex-wrap gap-1">
                          {user.roles.length === 0 ? (
                            <span className="text-xs text-primary-400 italic">Sem perfil</span>
                          ) : (
                            user.roles.map((role) => (
                              <span
                                key={role}
                                className={cn('px-2 py-0.5 rounded-full text-xs font-medium', ROLE_COLORS[role] ?? 'bg-gray-100 text-gray-600')}
                              >
                                {role}
                              </span>
                            ))
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <span className={cn(
                          'px-2 py-0.5 rounded-full text-xs font-medium',
                          user.status === 'Active' ? 'bg-green-100 text-green-700' :
                          user.status === 'Pending' ? 'bg-amber-100 text-amber-700' :
                          'bg-gray-100 text-gray-500'
                        )}>
                          {user.status === 'Active' ? 'Ativo' : user.status === 'Pending' ? 'Pendente' : 'Inativo'}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right">
                        <button
                          onClick={() => setManagingUser(user)}
                          className="flex items-center gap-1.5 ml-auto px-3 py-1.5 rounded-lg text-xs font-medium text-primary-600 hover:bg-primary-100 transition-colors"
                        >
                          <Shield className="w-3.5 h-3.5" />
                          Gerenciar
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {data && data.totalPages > 1 && (
            <div className="px-4 py-3 border-t border-primary-100 flex items-center justify-between">
              <p className="text-sm text-primary-500">{data.total} usuário{data.total !== 1 ? 's' : ''}</p>
              <div className="flex gap-2">
                <Button variant="secondary" size="sm" disabled={page === 1} onClick={() => setPage((p) => p - 1)}>Anterior</Button>
                <Button variant="secondary" size="sm" disabled={page === data.totalPages} onClick={() => setPage((p) => p + 1)}>Próximo</Button>
              </div>
            </div>
          )}
        </Card>
      )}

      {managingUser && (
        <ManageRoleModal user={managingUser} onClose={() => setManagingUser(null)} />
      )}
    </div>
  );
}
