import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, Search, Pin, Eye, Edit2, Trash2, Send } from 'lucide-react';
import { Button, Card, Spinner } from '@/components/ui';
import { useCommunications, useDeleteCommunication, usePublishCommunication } from '@/hooks/useCommunications';
import CommunicationFormModal from '@/components/communications/CommunicationFormModal';
import type { Communication } from '@/types/phase6';
import { formatDate } from '@/utils/format';
import { useConfirm } from '@/components/ui';
import { cn } from '@/utils/cn';

const TYPE_LABELS: Record<string, string> = {
  announcement: 'Anúncio',
  update: 'Atualização',
  report: 'Relatório',
  alert: 'Alerta',
  invitation: 'Convite',
};

const TYPE_COLORS: Record<string, string> = {
  announcement: 'bg-blue-100 text-blue-700',
  update: 'bg-green-100 text-green-700',
  report: 'bg-purple-100 text-purple-700',
  alert: 'bg-red-100 text-red-700',
  invitation: 'bg-amber-100 text-amber-700',
};

const VISIBILITY_LABELS: Record<string, string> = {
  all: 'Todos',
  investors: 'Investidores',
  founders: 'Fundadores',
  employees: 'Colaboradores',
  specific: 'Específico',
};

export default function CommunicationsPage() {
  const navigate = useNavigate();
  const { confirm } = useConfirm();

  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingComm, setEditingComm] = useState<Communication | undefined>();

  const { data, isLoading } = useCommunications({
    search: search || undefined,
    commType: typeFilter || undefined,
    status: (statusFilter as 'draft' | 'published') || undefined,
    pageSize: 20,
  });

  const deleteMutation = useDeleteCommunication();
  const publishMutation = usePublishCommunication();

  const items = data?.items ?? [];

  async function handleDelete(comm: Communication) {
    const ok = await confirm({
      title: 'Excluir comunicação',
      message: `Tem certeza que deseja excluir "${comm.title}"?`,
      confirmText: 'Excluir',
      confirmVariant: 'danger',
    });
    if (ok) deleteMutation.mutate(comm.id);
  }

  function handleEdit(comm: Communication) {
    setEditingComm(comm);
    setShowModal(true);
  }

  function handleCloseModal() {
    setShowModal(false);
    setEditingComm(undefined);
  }

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="page-title">Comunicações</h1>
          <p className="page-subtitle">Gerencie as comunicações com investidores e equipe</p>
        </div>
        <Button
          icon={<Plus className="w-4 h-4" />}
          onClick={() => setShowModal(true)}
        >
          Nova Comunicação
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <div className="p-4 flex flex-wrap gap-3">
          <div className="relative flex-1 min-w-48">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-primary-400" />
            <input
              className="input pl-9"
              placeholder="Buscar comunicações..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <select
            className="input w-auto"
            value={typeFilter}
            onChange={(e) => setTypeFilter(e.target.value)}
          >
            <option value="">Todos os tipos</option>
            {Object.entries(TYPE_LABELS).map(([value, label]) => (
              <option key={value} value={value}>{label}</option>
            ))}
          </select>
          <select
            className="input w-auto"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
          >
            <option value="">Todos os status</option>
            <option value="draft">Rascunho</option>
            <option value="published">Publicado</option>
          </select>
        </div>
      </Card>

      {/* List */}
      {isLoading ? (
        <div className="flex justify-center py-12"><Spinner /></div>
      ) : items.length === 0 ? (
        <Card>
          <div className="flex flex-col items-center justify-center py-12 text-primary-400 gap-2">
            <Send className="w-10 h-10 opacity-40" />
            <p className="text-sm font-medium">Nenhuma comunicação encontrada</p>
          </div>
        </Card>
      ) : (
        <div className="space-y-3">
          {items.map((comm) => (
            <Card key={comm.id} className="hover:shadow-md transition-shadow">
              <div className="p-4 flex items-center gap-4">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-1">
                    {comm.isPinned && <Pin className="w-3.5 h-3.5 text-amber-500 shrink-0" />}
                    <h3
                      className="font-semibold text-primary truncate cursor-pointer hover:text-accent transition-colors"
                      onClick={() => navigate(`/communications/${comm.id}`)}
                    >
                      {comm.title}
                    </h3>
                  </div>
                  {comm.summary && (
                    <p className="text-sm text-primary-500 line-clamp-1 mb-2">{comm.summary}</p>
                  )}
                  <div className="flex items-center gap-3 flex-wrap">
                    <span className={cn('px-2 py-0.5 rounded-full text-xs font-medium', TYPE_COLORS[comm.commType] || 'bg-gray-100 text-gray-600')}>
                      {TYPE_LABELS[comm.commType] || comm.commType}
                    </span>
                    <span className="text-xs text-primary-500">
                      {VISIBILITY_LABELS[comm.visibility] || comm.visibility}
                    </span>
                    {comm.publishedAt ? (
                      <span className="text-xs text-green-600">
                        Publicado em {formatDate(comm.publishedAt)}
                      </span>
                    ) : (
                      <span className="text-xs text-amber-600">Rascunho</span>
                    )}
                    <span className="flex items-center gap-1 text-xs text-primary-400">
                      <Eye className="w-3 h-3" />
                      {comm.viewsCount}
                    </span>
                  </div>
                </div>
                <div className="flex items-center gap-2 shrink-0">
                  {!comm.publishedAt && (
                    <button
                      onClick={() => publishMutation.mutate(comm.id)}
                      className="p-1.5 rounded-lg text-green-600 hover:bg-green-50 transition-colors"
                      title="Publicar"
                    >
                      <Send className="w-4 h-4" />
                    </button>
                  )}
                  <button
                    onClick={() => handleEdit(comm)}
                    className="p-1.5 rounded-lg text-primary-500 hover:bg-primary-50 transition-colors"
                    title="Editar"
                  >
                    <Edit2 className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => handleDelete(comm)}
                    className="p-1.5 rounded-lg text-red-500 hover:bg-red-50 transition-colors"
                    title="Excluir"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      {showModal && (
        <CommunicationFormModal
          communication={editingComm}
          onClose={handleCloseModal}
        />
      )}
    </div>
  );
}
