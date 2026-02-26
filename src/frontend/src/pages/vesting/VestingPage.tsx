import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus,
  Target,
  Search,
  ChevronLeft,
  ChevronRight,
  RefreshCw,
  TrendingUp,
  Users,
  CheckCircle,
} from 'lucide-react';
import { Button, Card, Spinner, StatCard } from '@/components/ui';
import { VestingPlanCard } from '@/components/vesting';
import {
  useVestingPlans,
  useCreateVestingPlan,
  useUpdateVestingPlan,
  useActivateVestingPlan,
  useDeactivateVestingPlan,
  useArchiveVestingPlan,
  useDeleteVestingPlan,
} from '@/hooks';
import { useClientStore } from '@/stores/clientStore';
import { useConfirm } from '@/components/ui';
import {
  VestingPlanStatus,
  VestingType,
  type VestingPlan,
  type CreateVestingPlanRequest,
  type UpdateVestingPlanRequest,
  vestingTypeLabels,
} from '@/types';

// ─── Simple inline modal form ─────────────────────────────────────────────────

interface PlanFormData {
  name: string;
  description: string;
  vestingType: VestingType;
  cliffMonths: number;
  vestingMonths: number;
  totalEquityPercentage: number;
}

const defaultForm: PlanFormData = {
  name: '',
  description: '',
  vestingType: VestingType.TimeBasedLinear,
  cliffMonths: 12,
  vestingMonths: 48,
  totalEquityPercentage: 1,
};

interface PlanFormModalProps {
  open: boolean;
  companyId: string;
  editing?: VestingPlan;
  onClose: () => void;
  onSubmit: (data: CreateVestingPlanRequest | UpdateVestingPlanRequest) => void;
  isLoading: boolean;
}

function PlanFormModal({ open, companyId, editing, onClose, onSubmit, isLoading }: PlanFormModalProps) {
  const [form, setForm] = useState<PlanFormData>(
    editing
      ? {
          name: editing.name,
          description: editing.description ?? '',
          vestingType: editing.vestingType,
          cliffMonths: editing.cliffMonths,
          vestingMonths: editing.vestingMonths,
          totalEquityPercentage: editing.totalEquityPercentage,
        }
      : defaultForm
  );

  if (!open) return null;

  function handleChange<K extends keyof PlanFormData>(key: K, value: PlanFormData[K]) {
    setForm((prev) => ({ ...prev, [key]: value }));
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (editing) {
      onSubmit({
        name: form.name,
        description: form.description || undefined,
        cliffMonths: form.cliffMonths,
        vestingMonths: form.vestingMonths,
        totalEquityPercentage: form.totalEquityPercentage,
      } as UpdateVestingPlanRequest);
    } else {
      onSubmit({
        companyId,
        name: form.name,
        description: form.description || undefined,
        vestingType: form.vestingType,
        cliffMonths: form.cliffMonths,
        vestingMonths: form.vestingMonths,
        totalEquityPercentage: form.totalEquityPercentage,
      } as CreateVestingPlanRequest);
    }
  }

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="fixed inset-0 bg-black/40" onClick={onClose} />
      <div className="relative flex min-h-full items-center justify-center p-4">
        <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-lg">
          <div className="flex items-center justify-between p-6 border-b border-gray-100">
            <h2 className="text-lg font-semibold text-gray-900">
              {editing ? 'Editar Plano de Vesting' : 'Novo Plano de Vesting'}
            </h2>
            <button
              className="p-2 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors"
              onClick={onClose}
            >
              ×
            </button>
          </div>
          <form onSubmit={handleSubmit} className="p-6 space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Nome *</label>
              <input
                type="text"
                required
                maxLength={200}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                value={form.name}
                onChange={(e) => handleChange('name', e.target.value)}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Descrição</label>
              <textarea
                rows={2}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 resize-none"
                value={form.description}
                onChange={(e) => handleChange('description', e.target.value)}
              />
            </div>

            {!editing && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Tipo de Vesting *</label>
                <select
                  required
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  value={form.vestingType}
                  onChange={(e) => handleChange('vestingType', e.target.value as VestingType)}
                >
                  {Object.values(VestingType).map((v) => (
                    <option key={v} value={v}>
                      {vestingTypeLabels[v]}
                    </option>
                  ))}
                </select>
              </div>
            )}

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Cliff (meses)
                </label>
                <input
                  type="number"
                  min={0}
                  max={120}
                  required
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  value={form.cliffMonths}
                  onChange={(e) => handleChange('cliffMonths', parseInt(e.target.value) || 0)}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Duração (meses)
                </label>
                <input
                  type="number"
                  min={1}
                  max={240}
                  required
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  value={form.vestingMonths}
                  onChange={(e) => handleChange('vestingMonths', parseInt(e.target.value) || 48)}
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Total Equity (%) *
              </label>
              <input
                type="number"
                min={0.0001}
                max={100}
                step={0.0001}
                required
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                value={form.totalEquityPercentage}
                onChange={(e) =>
                  handleChange('totalEquityPercentage', parseFloat(e.target.value) || 1)
                }
              />
            </div>

            <div className="flex gap-3 pt-2">
              <button
                type="button"
                className="flex-1 px-4 py-2.5 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50"
                onClick={onClose}
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isLoading}
                className="flex-1 px-4 py-2.5 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 disabled:opacity-50 transition-colors"
              >
                {isLoading ? 'Salvando...' : editing ? 'Salvar alterações' : 'Criar plano'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function VestingPage() {
  const navigate = useNavigate();
  const { confirm } = useConfirm();
  const { selectedCompanyId } = useClientStore();

  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 12;

  const [modalOpen, setModalOpen] = useState(false);
  const [editingPlan, setEditingPlan] = useState<VestingPlan | undefined>();

  const filters = useMemo(
    () => ({
      companyId: selectedCompanyId ?? '',
      page,
      pageSize,
      search: search || undefined,
      status: statusFilter || undefined,
    }),
    [selectedCompanyId, page, pageSize, search, statusFilter]
  );

  const { data, isLoading, isError, refetch } = useVestingPlans(filters);
  const createMutation = useCreateVestingPlan();
  const updateMutation = useUpdateVestingPlan();
  const activateMutation = useActivateVestingPlan();
  const deactivateMutation = useDeactivateVestingPlan();
  const archiveMutation = useArchiveVestingPlan();
  const deleteMutation = useDeleteVestingPlan();

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending;

  const stats = useMemo(() => {
    const items = data?.items ?? [];
    return {
      total: data?.totalCount ?? 0,
      active: items.filter((p) => p.status === VestingPlanStatus.Active).length,
      draft: items.filter((p) => p.status === VestingPlanStatus.Draft).length,
      grants: items.reduce((acc, p) => acc + p.activeGrantsCount, 0),
    };
  }, [data]);

  function handleOpenCreate() {
    setEditingPlan(undefined);
    setModalOpen(true);
  }

  function handleEdit(plan: VestingPlan) {
    setEditingPlan(plan);
    setModalOpen(true);
  }

  async function handleSubmitForm(req: CreateVestingPlanRequest | UpdateVestingPlanRequest) {
    if (editingPlan) {
      await updateMutation.mutateAsync({ id: editingPlan.id, data: req as UpdateVestingPlanRequest });
    } else {
      await createMutation.mutateAsync(req as CreateVestingPlanRequest);
    }
    setModalOpen(false);
  }

  async function handleDelete(id: string) {
    const confirmed = await confirm({
      title: 'Excluir Plano',
      message: 'Tem certeza que deseja excluir este plano de vesting? Esta ação não pode ser desfeita.',
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      confirmVariant: 'danger',
    });
    if (confirmed) deleteMutation.mutate(id);
  }

  if (!selectedCompanyId) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="p-8 text-center">
          <Target className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Nenhuma empresa selecionada</h2>
          <p className="text-gray-500">Selecione uma empresa para visualizar os planos de vesting.</p>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Vesting & Metas</h1>
          <p className="page-subtitle">Gerencie planos de vesting e grants para colaboradores</p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="sm" onClick={() => refetch()}>
            <RefreshCw size={16} />
          </Button>
          <Button onClick={handleOpenCreate} className="flex items-center gap-2">
            <Plus size={16} />
            Novo plano
          </Button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="Total de planos" value={stats.total} icon={<Target size={20} className="text-indigo-500" />} />
        <StatCard label="Planos ativos" value={stats.active} icon={<CheckCircle size={20} className="text-green-500" />} />
        <StatCard label="Rascunhos" value={stats.draft} icon={<TrendingUp size={20} className="text-amber-500" />} />
        <StatCard label="Grants ativos" value={stats.grants} icon={<Users size={20} className="text-blue-500" />} />
      </div>

      {/* Filters */}
      <Card className="p-4">
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Buscar planos..."
              className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            />
          </div>
          <select
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            value={statusFilter}
            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
          >
            <option value="">Todos os status</option>
            <option value="Draft">Rascunho</option>
            <option value="Active">Ativo</option>
            <option value="Inactive">Inativo</option>
            <option value="Archived">Arquivado</option>
          </select>
          {(search || statusFilter) && (
            <button
              className="text-sm text-gray-500 hover:text-gray-700 px-3 py-2 border border-gray-200 rounded-lg"
              onClick={() => { setSearch(''); setStatusFilter(''); setPage(1); }}
            >
              Limpar
            </button>
          )}
        </div>
      </Card>

      {/* Content */}
      {isLoading ? (
        <div className="flex justify-center py-16">
          <Spinner className="w-8 h-8 text-indigo-600" />
        </div>
      ) : isError ? (
        <Card className="p-8 text-center">
          <p className="text-gray-500 mb-4">Erro ao carregar planos de vesting.</p>
          <Button onClick={() => refetch()}>Tentar novamente</Button>
        </Card>
      ) : !data?.items?.length ? (
        <Card className="p-12 text-center">
          <Target className="w-12 h-12 text-gray-300 mx-auto mb-3" />
          <h3 className="font-semibold text-gray-700 mb-1">Nenhum plano encontrado</h3>
          <p className="text-gray-400 text-sm mb-4">
            {search || statusFilter
              ? 'Tente ajustar os filtros'
              : 'Crie o primeiro plano de vesting para esta empresa'}
          </p>
          {!search && !statusFilter && (
            <Button onClick={handleOpenCreate} className="flex items-center gap-2 mx-auto">
              <Plus size={16} />
              Criar plano
            </Button>
          )}
        </Card>
      ) : (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {data.items.map((plan) => (
              <VestingPlanCard
                key={plan.id}
                plan={plan}
                onClick={(p) => navigate(`/vesting/${p.id}`)}
                onEdit={handleEdit}
                onActivate={(id) => activateMutation.mutate(id)}
                onDeactivate={(id) => deactivateMutation.mutate(id)}
                onArchive={(id) => archiveMutation.mutate(id)}
                onDelete={handleDelete}
              />
            ))}
          </div>

          {/* Pagination */}
          {(data.totalPages ?? 1) > 1 && (
            <div className="flex items-center justify-center gap-2">
              <button
                className="p-2 rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-40"
                disabled={page === 1}
                onClick={() => setPage((p) => p - 1)}
              >
                <ChevronLeft size={16} />
              </button>
              <span className="text-sm text-gray-600">
                Página {page} de {data.totalPages}
              </span>
              <button
                className="p-2 rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-40"
                disabled={page === data.totalPages}
                onClick={() => setPage((p) => p + 1)}
              >
                <ChevronRight size={16} />
              </button>
            </div>
          )}
        </>
      )}

      {/* Modal */}
      <PlanFormModal
        open={modalOpen}
        companyId={selectedCompanyId}
        editing={editingPlan}
        onClose={() => setModalOpen(false)}
        onSubmit={handleSubmitForm}
        isLoading={isMutating}
      />
    </div>
  );
}
