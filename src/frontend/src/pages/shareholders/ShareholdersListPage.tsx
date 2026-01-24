import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, Users, Search, ChevronLeft, ChevronRight } from 'lucide-react';
import { Button, Card, Spinner, StatCard } from '@/components/ui';
import { ShareholderCard, ShareholderFilters, ShareholderFormModal } from '@/components/shareholders';
import { useShareholders, useDeleteShareholder } from '@/hooks/useShareholders';
import { useConfirm } from '@/components/ui';
import {
  ShareholderType,
  ShareholderStatus,
  type Shareholder,
} from '@/types';

export default function ShareholdersListPage() {
  const navigate = useNavigate();
  const { confirm } = useConfirm();

  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingShareholder, setEditingShareholder] = useState<Shareholder | undefined>(undefined);

  // Filter state
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState<ShareholderType | null>(null);
  const [statusFilter, setStatusFilter] = useState<ShareholderStatus | null>(null);
  const [page, setPage] = useState(1);
  const pageSize = 12;

  // Build filters object
  const filters = useMemo(
    () => ({
      page,
      pageSize,
      search: search || undefined,
      type: typeFilter ?? undefined,
      status: statusFilter ?? undefined,
    }),
    [page, search, typeFilter, statusFilter]
  );

  // Query
  const { data, isLoading, isError, refetch } = useShareholders(filters);
  const deleteShareholderMutation = useDeleteShareholder();

  // Stats calculation
  const stats = useMemo(() => {
    if (!data?.items) return { total: 0, active: 0, pending: 0 };
    return {
      total: data.totalCount,
      active: data.items.filter((s) => s.status === ShareholderStatus.Active).length,
      pending: data.items.filter((s) => s.status === ShareholderStatus.Pending).length,
    };
  }, [data]);

  // Handlers
  const handleClearFilters = () => {
    setSearch('');
    setTypeFilter(null);
    setStatusFilter(null);
    setPage(1);
  };

  const handleView = (shareholder: Shareholder) => {
    navigate(`/shareholders/${shareholder.id}`);
  };

  const handleEdit = (shareholder: Shareholder) => {
    setEditingShareholder(shareholder);
    setIsModalOpen(true);
  };

  const handleDelete = async (shareholder: Shareholder) => {
    const confirmed = await confirm({
      title: 'Excluir Sócio',
      message: `Tem certeza que deseja excluir "${shareholder.name}"? Esta ação não pode ser desfeita.`,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      confirmVariant: 'danger',
    });

    if (confirmed) {
      deleteShareholderMutation.mutate(shareholder.id);
    }
  };

  const handleAddNew = () => {
    setEditingShareholder(undefined);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingShareholder(undefined);
  };

  // Pagination
  const totalPages = data?.totalPages || 1;
  const hasPreviousPage = page > 1;
  const hasNextPage = page < totalPages;

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Sócios</h1>
          <p className="page-subtitle">
            Gerencie os sócios e acionistas das empresas
          </p>
        </div>
        <Button icon={<Plus className="w-4 h-4" />} onClick={handleAddNew}>
          Adicionar Sócio
        </Button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <StatCard
          icon={<Users className="w-5 h-5 text-white" />}
          iconColor="bg-primary"
          value={stats.total}
          label="Total de Sócios"
        />
        <StatCard
          icon={<Users className="w-5 h-5 text-white" />}
          iconColor="bg-success"
          value={stats.active}
          label="Sócios Ativos"
        />
        <StatCard
          icon={<Users className="w-5 h-5 text-white" />}
          iconColor="bg-warning"
          value={stats.pending}
          label="Pendentes"
        />
      </div>

      {/* Filters */}
      <ShareholderFilters
        search={search}
        onSearchChange={(val) => {
          setSearch(val);
          setPage(1);
        }}
        typeFilter={typeFilter}
        onTypeFilterChange={(val) => {
          setTypeFilter(val);
          setPage(1);
        }}
        statusFilter={statusFilter}
        onStatusFilterChange={(val) => {
          setStatusFilter(val);
          setPage(1);
        }}
        onClearFilters={handleClearFilters}
      />

      {/* Loading State */}
      {isLoading && (
        <div className="flex justify-center items-center py-12">
          <Spinner className="w-8 h-8" />
        </div>
      )}

      {/* Error State */}
      {isError && (
        <Card className="text-center py-12">
          <div className="w-16 h-16 bg-error-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <Users className="w-8 h-8 text-error-500" />
          </div>
          <h3 className="font-semibold text-primary mb-2">
            Erro ao carregar sócios
          </h3>
          <p className="text-primary-500 mb-4">
            Ocorreu um erro ao carregar a lista de sócios.
          </p>
          <Button variant="secondary" onClick={() => refetch()}>
            Tentar novamente
          </Button>
        </Card>
      )}

      {/* Cards Grid */}
      {!isLoading && !isError && data?.items && data.items.length > 0 && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {data.items.map((shareholder) => (
              <ShareholderCard
                key={shareholder.id}
                shareholder={shareholder}
                onView={handleView}
                onEdit={handleEdit}
                onDelete={handleDelete}
              />
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between pt-4">
              <p className="text-sm text-primary-500">
                Mostrando {(page - 1) * pageSize + 1} a{' '}
                {Math.min(page * pageSize, data.totalCount)} de {data.totalCount}{' '}
                sócios
              </p>
              <div className="flex items-center gap-2">
                <Button
                  variant="secondary"
                  size="sm"
                  disabled={!hasPreviousPage}
                  onClick={() => setPage((p) => p - 1)}
                >
                  <ChevronLeft className="w-4 h-4" />
                </Button>
                <span className="text-sm text-primary-600 px-2">
                  Página {page} de {totalPages}
                </span>
                <Button
                  variant="secondary"
                  size="sm"
                  disabled={!hasNextPage}
                  onClick={() => setPage((p) => p + 1)}
                >
                  <ChevronRight className="w-4 h-4" />
                </Button>
              </div>
            </div>
          )}
        </>
      )}

      {/* Empty State */}
      {!isLoading && !isError && data?.items && data.items.length === 0 && (
        <Card className="text-center py-12">
          <div className="w-16 h-16 bg-primary-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <Search className="w-8 h-8 text-primary-400" />
          </div>
          <h3 className="font-semibold text-primary mb-2">
            Nenhum sócio encontrado
          </h3>
          <p className="text-primary-500 mb-4">
            {search || typeFilter !== null || statusFilter !== null
              ? 'Tente ajustar os filtros ou realizar uma nova busca.'
              : 'Comece adicionando o primeiro sócio.'}
          </p>
          {search || typeFilter !== null || statusFilter !== null ? (
            <Button variant="secondary" onClick={handleClearFilters}>
              Limpar filtros
            </Button>
          ) : (
            <Button onClick={handleAddNew}>
              <Plus className="w-4 h-4 mr-2" />
              Adicionar Sócio
            </Button>
          )}
        </Card>
      )}

      {/* Modal de Criação/Edição */}
      <ShareholderFormModal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        shareholder={editingShareholder}
      />
    </div>
  );
}
