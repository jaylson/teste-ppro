import { useState } from 'react';
import {
  UserCheck, Search, Plus, X, Shield, Mail, User,
} from 'lucide-react';
import { Button, Card, Badge, Spinner } from '@/components/ui';
import { useUsers, useAddUserRole, useRemoveUserRole } from '@/hooks/useUsers';
import type { UserSummary } from '@/types';
import { cn } from '@/utils/cn';

const APPROVER_ROLES = ['Admin', 'Founder', 'BoardMember', 'Legal', 'Finance'];

const ROLE_DESCRIPTIONS: Record<string, string> = {
  Admin: 'Administrador — pode aprovar qualquer tipo de fluxo',
  Founder: 'Fundador — aprovações societárias e de vesting',
  BoardMember: 'Membro do Conselho — aprovações estratégicas',
  Legal: 'Jurídico — aprovações de contratos e documentos',
  Finance: 'Financeiro — aprovações de faturamento e valuations',
};

const ROLE_COLORS: Record<string, string> = {
  Admin: 'bg-purple-100 text-purple-700',
  SuperAdmin: 'bg-purple-100 text-purple-700',
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
  const allRoles = ['Admin', 'Founder', 'BoardMember', 'Legal', 'Finance', 'HR', 'Employee', 'Investor', 'Viewer'];
  const [selectedRole, setSelectedRole] = useState('');

  const isPending = addRole.isPending || removeRole.isPending;

  function handleAdd() {
    if (!selectedRole) return;
    addRole.mutate({ userId: user.id, data: { role: selectedRole as any } }, {
      onSuccess: () => setSelectedRole(''),
    });
  }

  function handleRemove(role: string) {
    removeRole.mutate({ userId: user.id, role });
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
        <div className="flex items-center justify-between px-6 py-4 border-b border-primary-100">
          <h3 className="font-semibold text-primary">Gerenciar Papéis — {user.name}</h3>
          <button onClick={onClose} className="text-primary-400 hover:text-primary text-xl leading-none">&times;</button>
        </div>

        <div className="px-6 py-4 space-y-4">
          {/* Papéis atuais */}
          <div>
            <p className="text-sm font-medium text-primary mb-2">Papéis atuais</p>
            {user.roles.length === 0 ? (
              <p className="text-xs text-primary-400">Nenhum papel atribuído</p>
            ) : (
              <div className="flex flex-wrap gap-2">
                {user.roles.map((role) => (
                  <span
                    key={role}
                    className={cn('flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium', ROLE_COLORS[role] ?? 'bg-gray-100 text-gray-600')}
                  >
                    {role}
                    <button
                      onClick={() => handleRemove(role)}
                      disabled={isPending}
                      className="hover:opacity-60 transition-opacity"
                      title="Remover papel"
                    >
                      <X className="w-3 h-3" />
                    </button>
                  </span>
                ))}
              </div>
            )}
          </div>

          {/* Adicionar papel */}
          <div>
            <p className="text-sm font-medium text-primary mb-2">Adicionar papel</p>
            <div className="flex gap-2">
              <select
                className="input flex-1 text-sm"
                value={selectedRole}
                onChange={(e) => setSelectedRole(e.target.value)}
              >
                <option value="">Selecione um papel...</option>
                {allRoles
                  .filter((r) => !user.roles.includes(r))
                  .map((r) => (
                    <option key={r} value={r}>{r}</option>
                  ))}
              </select>
              <Button
                size="sm"
                onClick={handleAdd}
                disabled={!selectedRole || isPending}
                loading={addRole.isPending}
              >
                Adicionar
              </Button>
            </div>
          </div>
        </div>

        <div className="flex justify-end px-6 py-4 border-t border-primary-100">
          <Button variant="secondary" onClick={onClose}>Fechar</Button>
        </div>
      </div>
    </div>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function ApproversPage() {
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [page, setPage] = useState(1);
  const [managingUser, setManagingUser] = useState<UserSummary | null>(null);

  const { data, isLoading } = useUsers({ page, pageSize: 15 });

  const users: UserSummary[] = (data?.items ?? []).filter((u) => {
    const matchSearch =
      !search ||
      u.name.toLowerCase().includes(search.toLowerCase()) ||
      u.email.toLowerCase().includes(search.toLowerCase());
    const matchRole =
      !roleFilter || u.roles.includes(roleFilter);
    return matchSearch && matchRole;
  });

  // Usuários com papéis de aprovação
  const approvers = users.filter((u) =>
    u.roles.some((r) => APPROVER_ROLES.includes(r))
  );
  const nonApprovers = users.filter(
    (u) => !u.roles.some((r) => APPROVER_ROLES.includes(r))
  );

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div>
        <h1 className="page-title">Aprovadores</h1>
        <p className="page-subtitle">Gerencie os usuários com poderes de aprovação nos fluxos</p>
      </div>

      {/* Role summary cards */}
      <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
        {APPROVER_ROLES.map((role) => {
          const count = (data?.items ?? []).filter((u) => u.roles.includes(role)).length;
          return (
            <Card
              key={role}
              className={cn(
                'cursor-pointer transition-all border-2',
                roleFilter === role
                  ? 'border-accent shadow-md'
                  : 'border-transparent hover:border-primary-200'
              )}
              onClick={() => setRoleFilter((r) => (r === role ? '' : role))}
            >
              <div className="p-3 text-center">
                <div className={cn('inline-flex items-center justify-center w-8 h-8 rounded-full mb-1', ROLE_COLORS[role] ?? 'bg-gray-100 text-gray-600')}>
                  <Shield className="w-4 h-4" />
                </div>
                <p className="text-xs font-semibold text-primary truncate">{role}</p>
                <p className="text-lg font-bold text-accent">{count}</p>
              </div>
            </Card>
          );
        })}
      </div>

      {/* Search & filter */}
      <Card>
        <div className="p-4 flex flex-wrap gap-3 items-center">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-primary-400" />
            <input
              className="input pl-9 text-sm"
              placeholder="Buscar por nome ou email..."
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
              Limpar filtro: {roleFilter}
            </button>
          )}
        </div>
      </Card>

      {isLoading ? (
        <div className="flex justify-center py-12"><Spinner /></div>
      ) : (
        <>
          {/* Approvers section */}
          {approvers.length > 0 && (
            <div>
              <h2 className="text-sm font-semibold text-primary-600 mb-3 flex items-center gap-2">
                <UserCheck className="w-4 h-4 text-green-500" />
                Aprovadores ativos ({approvers.length})
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-3">
                {approvers.map((user) => (
                  <Card key={user.id} className="hover:shadow-sm transition-shadow">
                    <div className="p-4 flex items-start gap-3">
                      <div className="w-9 h-9 rounded-full bg-primary-100 flex items-center justify-center shrink-0">
                        <span className="text-sm font-semibold text-primary-700">
                          {user.name.charAt(0).toUpperCase()}
                        </span>
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="font-semibold text-primary text-sm truncate">{user.name}</p>
                        <p className="text-xs text-primary-500 flex items-center gap-1 truncate">
                          <Mail className="w-3 h-3 shrink-0" />
                          {user.email}
                        </p>
                        <div className="flex flex-wrap gap-1 mt-2">
                          {user.roles.map((role) => (
                            <span key={role} className={cn('px-2 py-0.5 rounded-full text-xs font-medium', ROLE_COLORS[role] ?? 'bg-gray-100 text-gray-600')}>
                              {role}
                            </span>
                          ))}
                        </div>
                        {user.roles.some((r) => APPROVER_ROLES.includes(r)) && (
                          <p className="text-xs text-primary-400 mt-1.5 line-clamp-1">
                            {ROLE_DESCRIPTIONS[user.roles.find((r) => APPROVER_ROLES.includes(r))!] ?? ''}
                          </p>
                        )}
                      </div>
                      <button
                        onClick={() => setManagingUser(user)}
                        className="p-1.5 rounded-lg hover:bg-primary-100 transition-colors shrink-0"
                        title="Gerenciar papéis"
                      >
                        <Shield className="w-4 h-4 text-primary-400" />
                      </button>
                    </div>
                  </Card>
                ))}
              </div>
            </div>
          )}

          {/* Non-approvers */}
          {nonApprovers.length > 0 && !roleFilter && (
            <div>
              <h2 className="text-sm font-semibold text-primary-500 mb-3 flex items-center gap-2">
                <User className="w-4 h-4" />
                Outros usuários ({nonApprovers.length})
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-3">
                {nonApprovers.map((user) => (
                  <Card key={user.id} className="opacity-75 hover:opacity-100 hover:shadow-sm transition-all">
                    <div className="p-4 flex items-center gap-3">
                      <div className="w-9 h-9 rounded-full bg-primary-100 flex items-center justify-center shrink-0">
                        <span className="text-sm font-semibold text-primary-700">
                          {user.name.charAt(0).toUpperCase()}
                        </span>
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="font-semibold text-primary text-sm truncate">{user.name}</p>
                        <p className="text-xs text-primary-500 truncate">{user.email}</p>
                        <div className="flex flex-wrap gap-1 mt-1.5">
                          {user.roles.length === 0 ? (
                            <span className="text-xs text-primary-400 italic">Sem papéis</span>
                          ) : (
                            user.roles.map((role) => (
                              <span key={role} className={cn('px-2 py-0.5 rounded-full text-xs font-medium', ROLE_COLORS[role] ?? 'bg-gray-100 text-gray-600')}>
                                {role}
                              </span>
                            ))
                          )}
                        </div>
                      </div>
                      <button
                        onClick={() => setManagingUser(user)}
                        className="p-1.5 rounded-lg hover:bg-primary-100 transition-colors shrink-0"
                        title="Gerenciar papéis"
                      >
                        <Plus className="w-4 h-4 text-primary-400" />
                      </button>
                    </div>
                  </Card>
                ))}
              </div>
            </div>
          )}

          {users.length === 0 && (
            <Card>
              <div className="flex flex-col items-center justify-center py-16 text-primary-400 gap-2">
                <UserCheck className="w-10 h-10 opacity-40" />
                <p className="text-sm font-medium">Nenhum usuário encontrado</p>
              </div>
            </Card>
          )}

          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-primary-500">{data.total} usuário{data.total !== 1 ? 's' : ''}</p>
              <div className="flex gap-2">
                <Button variant="secondary" size="sm" disabled={page === 1} onClick={() => setPage((p) => p - 1)}>Anterior</Button>
                <Button variant="secondary" size="sm" disabled={page === data.totalPages} onClick={() => setPage((p) => p + 1)}>Próximo</Button>
              </div>
            </div>
          )}
        </>
      )}

      {managingUser && (
        <ManageRoleModal user={managingUser} onClose={() => setManagingUser(null)} />
      )}
    </div>
  );
}
